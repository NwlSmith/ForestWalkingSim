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
 * - Implement item holding in saving and loading
 * - Implement turtle quest
 * - Implement other animations
 * - Make pause call a pause animation function in PlayerAnimation.
 * - Pull camera back from player.
 * - Implement Ground Fitter.
 * 
 * Issues:
 * - Need to fix player position during dialogue. - FSM within InDialogueState? Maybe transitions in, then goes to regular behavior?
 * - Immediately thinks I want to talk to spirit
 * - In group dialogue, only plays first NPC's sounds
 * - Maybe make MovingOnGroundState a composite FSM with walking and sprinting.
 * - Fix weird rotation from jumping.
 * - Player doesn't ever enter IdleState, Y vel is always -.1. This is messing up animations.
 * - figure out a good way to transition from paused -> main menu -> continue game without recreating stuff.
 * 
 */

public class GameManager : MonoBehaviour
{

    #region Variables

    // This instance of the Game Manager.
    public static GameManager instance = null;

    // The finite state machine of the current gamestate.
    private FiniteStateMachine<GameManager> _fsm;


    #endregion 

    #region Lifecycle Management

    private void Awake()
    {
        // Ensure that there is only one instance of the GameManager.ger
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

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
    }

    #endregion

    #region Triggers.

    public void NewGame()
    {
        Services.SaveManager.NewGameSave();
        _fsm.TransitionTo<StartPlay>();
    }

    public void LoadSave()
    {
        if (Services.SaveManager.SaveExists())
            StartCoroutine(LoadSaveCO());
        else
            Debug.Log("Trying to load a save that does not exist");
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
            if (!Services.SaveManager.SaveExists())
            {
                Debug.Log("Save does not exist");
                Services.UIManager.HideContinue();
            }
            
        }

        public override void OnExit()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // Possibly not needed?
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
        public override void OnEnter()
        {
            Debug.Log("GameManager: Entered InDialogueState");
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Services.PlayerMovement.EnterDialogue(); // Change each of these - make them only entering into dialogue, then in Update, have a function to actually commence dialogue.
            Services.CameraManager.EnterDialogue();
            Services.UIManager.EnterDialogue();
            Services.NPCInteractionManager.EnterDialogue();
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
