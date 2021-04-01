using System.Collections;
using UnityEngine;

/*
 * Creator: Nate Smith
 * Date Created: 2/11/2021
 * Description: Game Manager class
 * 
 * Controls Game State and holds directory of other services.
 * 
 * Allows different game states to have different Update functions.
 * 
 * To do:
 * - Make pause call a pause animation function in PlayerAnimation.
 * - Maybe change quest items so they're not affected by physics? - let player drop items
 * - Make walking animation line up with music maybe?
 * - In URPExampleAssets > Settings > UniversalRenderPipeline Shadow Max distance was initially 50
 * - glitch with turtle quest
 * - Make turtle go to quest area.
 * - Feedback for main quest stuff
 *  - Have scripted thing where player stops moving, camera stops moving, camera looks at item, item floats into tree, then up, emit particles and play sound.
 * - Remove quest debug stuff in InputManager!
 * - Make tail more expressive!
 * 
 * Issues:
 * - Immediately thinks I want to talk to spirit - whenever you would hit e up until you talked to the frogs, it would pull up the dialogue for the mama bird regardless of where you were standing
 * - People want more variety in systems, like collecting rewards from NPCs/other kinds of interaction with the game environment.
 * - People weren't that happy about NPC POV camera
 * - make sure player looks at center of characters? maybe make player turn to face each character
 * - "When I pause and click �main menu�, it doesn�t do anything." ??? Fix mac save issue.
 * 
 */

public class GameManager : MonoBehaviour
{

    #region Variables

    // The finite state machine of the current gamestate.
    private FiniteStateMachine<GameManager> _fsm;

    private TaskManager _taskManager = new TaskManager();

    private bool _gameStarted = false;

    #endregion 

    #region Lifecycle Management

    private void Awake()
    {
        Services.InitializeServices(this);

        _fsm = new FiniteStateMachine<GameManager>(this);
    }

    void Start()
    {
        _fsm.TransitionTo<StartMenu>();
    }

    void Update()
    {
        _fsm.Update();
        _taskManager.Update();
    }

    #endregion

    #region Triggers.

    public void NewGame()
    {
        if (!_gameStarted)
        {
            _gameStarted = true;
            _fsm.TransitionTo<StartPlay>();
        }
        else
        {
            Services.SaveManager.NewGameSave();
            LoadSave();
        }
    }

    public void LoadSave()
    {
        if (!_gameStarted)
        {
            if (Services.SaveManager.SaveExists())
                StartCoroutine(LoadSaveCO());
            else
                Logger.Debug("Trying to load a save that does not exist");
        }
        else
        {
            _fsm.TransitionTo<StartPlay>();
        }
    }

    private IEnumerator LoadSaveCO()
    {
        Services.UIManager.EnterLoadSave();
        Coroutine loadCO = StartCoroutine(Services.SaveManager.LoadDataCO());
        yield return loadCO;
        _fsm.TransitionTo<StartPlay>();
    }

    // Called on Pause.
    public void Pause() => _fsm.TransitionTo<PauseState>();

    // Called on Unpause.
    public void Unpause() => _fsm.TransitionTo<PlayState>();

    // Called on Enter dialogue.
    public void EnterDialogue() => _fsm.TransitionTo<InDialogueState>();

    // Called on Exit dialogue.
    public void ReturnToPlay() => _fsm.TransitionTo<PlayState>();

    // Called on Quest item trigger.
    public void MidrollCutscene() => _fsm.TransitionTo<MidCutsceneState>();

    // Called when the player goes to main menu.
    public void MainMenu()
    {
        Services.SaveManager.SaveData();
        _fsm.TransitionTo<StartMenu>();
    }

    // Called on Quit.
    public void Quit() => Application.Quit();

    #endregion

    #region States

    private abstract class GameState : FiniteStateMachine<GameManager>.State { }

    private class StartMenu : GameState
    {
        public override void OnEnter()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Logger.Debug("GameManager: Entered StartMenu");
            Services.PlayerMovement.ForceIdle();
            Services.CameraManager.EnterStartMenu();
            Services.UIManager.EnterStartMenu();
        }

        public override void OnExit()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // Probably not needed?
    private class StartPlay : GameState
    {
        public override void OnEnter()
        {
            Logger.Debug("GameManager: Entered StartPlay");
        }

        public override void Update()
        {
            base.Update();
            // DELETE

            TransitionTo<PlayState>();
            Services.InputManager.ProcessPlayInput();
        }

        public override void OnExit()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private class PlayState : GameState
    {
        public override void OnEnter()
        {
            Logger.Debug("GameManager: Entered PlayState");
            Services.PlayerMovement.EnterPlay();
            Services.CameraManager.EnterPlay();
            Services.UIManager.EnterPlay();
        }

        public override void Update()
        {
            base.Update();

            Services.InputManager.ProcessPlayInput();
        }

        public override void OnExit() { }
    }

    private class PauseState : GameState
    {
        public override void OnEnter()
        {
            Logger.Debug("GameManager: Entered Pause");
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            Services.PlayerMovement.EnterPause();
            Services.CameraManager.EnterPause();
            Services.UIManager.EnterPause();
        }

        public override void Update()
        {
            base.Update();

            Services.InputManager.ProcessPauseMenuInput();
        }

        public override void OnExit()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Figure out where you are going. If you are going to play, lock mouse, enter play.
            Services.UIManager.EnterPlay();
        }
    }

    private class InDialogueState : GameState
    {
        private float delay = .5f;
        public override void OnEnter()
        {
            
            Task enterDialogue = new DelegateTask(() =>
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                Services.PlayerMovement.EnterDialogue();
                Services.UIManager.HideDialogueEnterPrompt();
            }, () =>
            {
                return Services.PlayerMovement.inPlaceForSequence;
            }, () =>
            {
                Services.CameraManager.EnterDialogue();
                Services.UIManager.EnterDialogue();
            });

            Task fadeIn = new WaitTask(delay);

            Task startConvo = new ActionTask(() =>
            {
                Services.DialogueController.EnterDialogue();
            });

            enterDialogue.Then(fadeIn).Then(startConvo);

            Context._taskManager.Do(enterDialogue);
        }

        public override void OnExit()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            Services.PlayerMovement.EnterPlay(); // These aren't needed
            Services.CameraManager.EnterPlay();
            Services.UIManager.EnterPlay();
            Services.NPCInteractionManager.ExitDialogue();
        }
    }

    // Use delegates to control player movement, show item taken away, and coordinate UI and sound.
    private class MidCutsceneState : GameState
    {
        /*
         * 1. Move player to position. Move camera behind player. ~2s
         * 2. Move Camera to cutsceneCamera, have camera slowly focus on item. Make player walk to tree. ~2s
         * 3. Trigger Quest item repository to take item, trigger sounds and particle effects, smoothly animate it into position and have large particle effect. 2s
         * 4. Fade to white? black? 2s
         * 5. stay there for a sec as music fades. Place player into new position. 3s
         * 6. Fade back in and have player turned around as environment changes are triggered. 2s
         * 7. 1 sec later have player get up and return to normal controls. 1s
         */

        public override void OnEnter()
        {

            // 1. Move player to position. Move camera behind player. ~2s
            Task enterSequence = new DelegateTask(() =>
            {
                Services.PlayerMovement.EnterMidCutscene();
                Services.CameraManager.EnterMidCutscene();
            }, () =>
            {
                return Services.PlayerMovement.inPlaceForSequence;
            });

            Task waitForTime1 = new WaitTask(.01f);

            // 2. Move Camera to cutsceneCamera, have camera slowly focus on item. Make player walk to tree. ~2s
            Task secondSequence = new DelegateTask(() =>
            {
                // Trigger particles?
                // Trigger music?
            }, () =>
            {
                return Services.PlayerMovement.inPlaceForSequence;
            }, () =>
            {
                Services.PlayerItemHolder.DropItem();
            });

            Task waitForTime2 = new WaitTask(.66f);

            // 3.Trigger Quest item repository to take item, trigger sounds and particle effects, smoothly animate it into position and have large particle effect. 2s
            ActionTask thirdSequence = new ActionTask(() =>
            {
                Services.QuestItemRepository.StartSequence();
                // Quest item Repository takes Item.
                // trigger other stuff.
            });

            Task waitForTime3 = new WaitTask(2f);

            // 4. Fade to white? black? 2s
            ActionTask fourthSequence = new ActionTask(() =>
            {
                // Fade out?
                Services.UIManager.CutsceneFadeIn();
            });

            // 5. stay there for a sec as music fades. Place player into new position. 3s
            Task waitForTime4 = new WaitTask(4f);

            // 6. Fade back in and have player turned around as environment changes are triggered. 2s
            ActionTask fifthSequence = new ActionTask(() =>
            {
                Services.PlayerAnimation.Sitting(true);
                // Fade in?
                Services.UIManager.CutsceneFadeOut();
            });

            Task waitForTime5 = new WaitTask(3f);
            // 7. 1 sec later have player get up and return to normal controls. 1s
            ActionTask sixthSequence = new ActionTask(() =>
            {
                Services.PlayerAnimation.Sitting(false);
                TransitionTo<PlayState>();
            });

            enterSequence.Then(waitForTime1).Then(secondSequence).Then(waitForTime2).Then(thirdSequence).Then(waitForTime3).Then(fourthSequence).Then(waitForTime4).Then(fifthSequence).Then(waitForTime5).Then(sixthSequence);

            Context._taskManager.Do(enterSequence);
        }

        public override void OnExit()
        {
            Services.PlayerMovement.EnterPlay(); // These aren't needed
            Services.CameraManager.EnterPlay();
            Services.UIManager.EnterPlay();
        }
    }

    #endregion

}
