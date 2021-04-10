using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

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
 * - In URPExampleAssets > Settings > UniversalRenderPipeline Shadow Max distance was initially 50
 * - The camera is a bit frustrating to work with, I found myself constantly adjusting it after moving a few steps.
 *      If it is possible, I think you should lock the camera behind the fox and then see if you can have another key that enables you to adjust the camera if you so desire. - Bad idea
 * - I also found myself phasing through a lot of the landscapes
 * - I hope the jump does not stop the momentum from sprinting. 
 * - I can no longer control the fox or press any button after I planted the seed. The game is still running, but it does not reacting to any key(include the esc) I pressed.
 * - highlight start button, OR continue button for controllers
 * - _recenterCameraTime = -1 for controllers?
 * - sensitivity too low
 * - tiring to use joystick button for sprinting
 * 
 * Issues:
 * - People want more variety in systems, like collecting rewards from NPCs/other kinds of interaction with the game environment.
 * - People weren't that happy about NPC POV camera
 * - make sure player looks at center of characters? maybe make player turn to face each character
 * - "When I pause and click “main menu”, it doesn’t do anything." ??? Fix mac save issue.
 * 
 */

public class GameManager : MonoBehaviour
{

    #region Variables

    // The finite state machine of the current gamestate.
    private FiniteStateMachine<GameManager> _fsm;

    private TaskManager _taskManager = new TaskManager();

    private bool _gameStarted = false;
    private bool _endingGame = false;

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
    public void ReturnToPlay()
    {
        if (_endingGame)
            EndGame();
        else
            _fsm.TransitionTo<PlayState>();
    }

    // Called on Quest item trigger.
    public void MidrollCutscene() => _fsm.TransitionTo<MidCutsceneState>();

    public void EndCutscene() => _fsm.TransitionTo<EndCutsceneState>();

    public void EndGame() => _fsm.TransitionTo<EndGameState>();

    // Called when the player goes to main menu.
    public void MainMenu()
    {
        Services.SaveManager.SaveData();

        Task save = new ActionTask(() => { Services.SaveManager.SaveData(); });
        Task wait = new WaitTask(.5f);
        Task finish = new ActionTask(() => { SceneManager.LoadScene(1); });
        save.Then(wait).Then(finish);
        _taskManager.Do(save);
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
            InputManager.ProcessPlayInput();
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
            if (Context._endingGame)
            {
                TransitionTo<EndGameState>();
            }
            Logger.Debug("GameManager: Entered PlayState");
            Services.PlayerMovement.EnterPlay();
            Services.CameraManager.EnterPlay();
            Services.UIManager.EnterPlay();
        }

        public override void Update()
        {
            base.Update();

            InputManager.ProcessPlayInput();
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

            InputManager.ProcessPauseMenuInput();
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

        private readonly Task _enterDialogue;

        public InDialogueState() =>
            // Pre-define task.
            _enterDialogue = DefineSequence();

        private Task DefineSequence()
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
            return enterDialogue;
        }

        public override void OnEnter() => Context._taskManager.Do(_enterDialogue);

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
         * 7. 1 sec later have player get up and return to normal controls. 1s // turns around!
         */

        private readonly Task _enterSequence;

        public MidCutsceneState()
        {
            _enterSequence = DefineSequence();
        }

        private Task DefineSequence()
        {
            // 1. Move player to position. Move camera behind player. ~2s
            Task enterSequence = new DelegateTask(() =>
            {
                Services.PlayerMovement.EnterMidCutscene();
                Services.CameraManager.EnterMidCutscene();
            }, () =>
            {
                Services.UIManager.HideItemPickupPrompt();
                return Services.PlayerMovement.inPlaceForSequence;
            });

            Task waitForTime1 = new WaitTask(1f);

            // 2. Move Camera to cutsceneCamera, have camera slowly focus on item. Make player walk to tree. ~2s
            Task secondSequence = new DelegateTask(() =>
            {
                // Trigger particles?
                // Trigger music?
            }, () =>
            {
                Services.UIManager.HideItemPickupPrompt();
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

            Task waitForTime3 = new WaitTask(1.5f);

            // 4. Fade to white? black? 2s
            ActionTask fourthSequence = new ActionTask(() =>
            {
                // Fade out?
                Services.UIManager.CutsceneFadeIn();
            });

            // 5. stay there for a sec as music fades. Place player into new position. 3s
            Task waitForTime4 = new WaitTask(4.5f);

            // 6. Fade back in and have player turned around as environment changes are triggered. 2s
            ActionTask fifthSequence = new ActionTask(() =>
            {
                PlayerAnimation.Sitting(true);
                // Fade in?
                Services.UIManager.CutsceneFadeOut();
            });

            Task waitForTime5 = new WaitTask(3f);
            // 7. 1 sec later have player get up and return to normal controls. 1s
            ActionTask sixthSequence = new ActionTask(() =>
            {
                PlayerAnimation.Sitting(false);
                TransitionTo<PlayState>();
            });

            enterSequence.Then(waitForTime1).Then(secondSequence).Then(waitForTime2).Then(thirdSequence).Then(waitForTime3).Then(fourthSequence).Then(waitForTime4).Then(fifthSequence).Then(waitForTime5).Then(sixthSequence);
            return enterSequence;
        }

        public override void OnEnter()
        {
            Context._taskManager.Do(_enterSequence);
        }

        public override void OnExit()
        {
            Services.PlayerMovement.EnterPlay(); // These aren't needed
            Services.CameraManager.EnterPlay();
            Services.UIManager.EnterPlay();
        }
    }

    // Use delegates to control player movement, show item taken away, and coordinate UI and sound.
    // FIX COMMENTS!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    private class EndCutsceneState : GameState
    {
        /*
         * 1. Move player to position. Move camera behind player. ~2s
         * 2. Move Camera to cutsceneCamera, have camera slowly focus on item. Make player walk to tree. ~2s
         * 3. Trigger Quest item repository to take item, trigger sounds and particle effects, smoothly animate it into position and have large particle effect. 2s
         * 4. Fade to white? black? 2s
         * 5. stay there for a sec as music fades. Place player into new position. 3s
         * 6. Fade back in and have player turned around as environment changes are triggered. 2s
         * 7. 1 sec later have player get up and return to normal controls. 1s // turns around!
         */

        private readonly Task _enterSequence;

        public EndCutsceneState() => _enterSequence = DefineSequence();

        private Task DefineSequence()
        {
            // 1. Move player to position. Move camera behind player. ~2s
            Task enterSequence = new DelegateTask(() =>
            {
                Services.PlayerMovement.EnterEndCutscene();
                Services.CameraManager.EnterMidCutscene();
            }, () =>
            {
                return Services.PlayerMovement.inPlaceForSequence;
            });

            Task waitForTime1 = new WaitTask(1f);

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

            Task waitForTime3 = new WaitTask(1.5f);

            // 4. Fade to white? black? 2s
            ActionTask fourthSequence = new ActionTask(() =>
            {
                // Fade out?
                Services.UIManager.CutsceneFadeIn();
            });

            // 5. stay there for a sec as music fades. Place player into new position. 3s
            Task waitForTime4 = new WaitTask(4.5f);

            // 6. Fade back in and have player turned around as environment changes are triggered. 2s
            ActionTask fifthSequence = new ActionTask(() =>
            {
                PlayerAnimation.Sitting(true);
                // Fade in?
                Services.UIManager.CutsceneFadeOut();
            });

            Task waitForTime5 = new WaitTask(3f);
            // 7. 1 sec later have player get up and return to normal controls. 1s
            ActionTask sixthSequence = new ActionTask(() =>
            {
                Context._endingGame = true;
                PlayerAnimation.Sitting(false);
                Services.NPCInteractionManager.FindClosestNPC();
                Services.GameManager.EnterDialogue();
            });

            enterSequence.Then(waitForTime1).Then(secondSequence).Then(waitForTime2).Then(thirdSequence).Then(waitForTime3).Then(fourthSequence).Then(waitForTime4).Then(fifthSequence).Then(waitForTime5).Then(sixthSequence);
            return enterSequence;
        }

        public override void OnEnter() => Context._taskManager.Do(_enterSequence);

    }

    // Use delegates to control player movement, Fade out screen, load main menu.
    private class EndGameState : GameState
    {
        private readonly Task _endSequence;

        public EndGameState() => _endSequence = DefineSequence();

        private Task DefineSequence()
        {
            Task enterSequence = new ActionTask(() =>
            {
                Logger.Debug("Entering EndGameState.");
                Services.PlayerMovement.ForceIdle();
                PlayerAnimation.Sitting(true);
                Services.CameraManager.EnterPause();
                Services.UIManager.CutsceneFadeIn();
            });
            Task waitForTime = new WaitTask(4.5f);

            // 1. Move player to position. Move camera behind player. ~2s
            Task endGame = new ActionTask(() =>
            {
                SceneManager.LoadScene(2);
            });

            enterSequence.Then(waitForTime).Then(endGame);
            return enterSequence;
        }

        public override void OnEnter() => Context._taskManager.Do(_endSequence);

        public override void OnExit()
        {
            Logger.Debug("LEAVING EndGameState");
        }
    }

    #endregion

}
