using System.Collections;
using System.Collections.Generic;
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
 * - Implement Ground Fitter. - My own version. FImpossible studios thing is impossible to work with.
 * - Maybe change quest items so they're not affected by physics? - let player drop items
 * - Place barriers blocking off areas from Heart. Make stage 1(?) lower them.
 * - Make walking animation line up with music maybe?
 * - Maybe make NPCCollider increase size when enter conversations, and decrease after?
 * - Short delay between press continue and next dialogue.
 * 
 * Issues:
 * - Immediately thinks I want to talk to spirit
 * - Have pickup and talk prompts easier to see, maybe more central? Maybe following characters?
 * - "After I talk to the Frog and Toad and complete the Warbler quest, if I talk to Frog and Toad again I get hard-locked into dialogue."
 * - People want more variety in systems, like collecting rewards from NPCs/other kinds of interaction with the game environment.
 * - People weren't that happy about NPC POV camera
 * - whenever you would hit e up until you talked to the frogs, it would pull up the dialogue for the mama bird regardless of where you were standing
 * - Player can jump out of NPCCollider before dialogue, locking their game
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
                Debug.Log("Trying to load a save that does not exist");
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
    public void Pause()
    {
        _fsm.TransitionTo<PauseState>();
    }

    // Called on Unpause.
    public void Unpause()
    {
        _fsm.TransitionTo<PlayState>();
    }

    // Called on Enter dialogue.
    public void EnterDialogue()
    {
        _fsm.TransitionTo<InDialogueState>();
    }

    // Called on Exit dialogue.
    public void ExitDialogue()
    {
        _fsm.TransitionTo<PlayState>();
    }

    // Called when the player goes to main menu.
    public void MainMenu()
    {
        Services.SaveManager.SaveData();
        _fsm.TransitionTo<StartMenu>();
    }

    // Called on Quit.
    public void Quit()
    {
        Application.Quit();
    }

    #endregion

    #region States

    private abstract class GameState : FiniteStateMachine<GameManager>.State { }

    private class StartMenu : GameState
    {
        public override void OnEnter()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Debug.Log("GameManager: Entered StartMenu");
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
            Debug.Log("GameManager: Entered StartPlay");
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
            Debug.Log("GameManager: Entered PlayState");
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
            Debug.Log("GameManager: Entered Pause");
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

            // This will be moved to InputManager.
            if (Input.GetKeyDown(KeyCode.P))
            {
                // Pause
                TransitionTo<PlayState>();
            }
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

            float elapsedTime = 0f;
            // Need to 
            Task enterDialogue = new DelegateTask(() =>
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                Services.PlayerMovement.EnterDialogue();
                Services.UIManager.HideDialogueEnterPrompt();
            }, () =>
            {
                return Services.PlayerMovement.inPlaceForDialogue;
            });

            Task fadeIn = new DelegateTask(() =>
            {
                Services.CameraManager.EnterDialogue();
                Services.UIManager.EnterDialogue();
                elapsedTime = 0f;
            }, () =>
            {
                elapsedTime += Time.deltaTime;
                return elapsedTime > delay;
            });

            Task startConvo = new ActionTask(() =>
            {
                Services.DialogueController.EnterDialogue();
            });

            enterDialogue.Then(fadeIn).Then(startConvo);

            Context._taskManager.Do(enterDialogue);
        }

        public override void Update()
        {
            base.Update();

            Services.InputManager.ProcessDialogueInput();

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
    #endregion
}
