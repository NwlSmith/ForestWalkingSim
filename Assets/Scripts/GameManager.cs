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
        _fsm.TransitionTo<StartPlay>();
    }

    void Update()
    {
        _fsm.Update();
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

    #endregion 

    #region States

    private abstract class GameState : FiniteStateMachine<GameManager>.State { }

    private class StartPlay : GameState
    {
        public override void OnEnter()
        {
            Debug.Log("Entered StartPlay");
        }

        public override void Update()
        {
            base.Update();
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
            Debug.Log("Entered PlayState");
        }

        public override void Update()
        {
            base.Update();


            Services.InputManager.ProcessPlayInput();

            // This will be moved to InputManager.
            if (Input.GetKeyDown(KeyCode.P))
            {
                // Pause
                TransitionTo<PauseState>();
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                // Pause
                TransitionTo<InDialogueState>();
            }
        }

        public override void OnExit()
        {

        }
    }

    private class PauseState : GameState
    {
        public override void OnEnter()
        {
            Debug.Log("Entered Pause");
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
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
        }
    }

    private class InDialogueState : GameState
    {
        public override void OnEnter()
        {
            Debug.Log("Entered InDialogueState");
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Services.PlayerMovement.ForceIdle();
            Services.CameraManager.EnterDialogue();
            Services.UIManager.EnterDialogue();
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

            Services.PlayerMovement.EnterPlay(); // FIGURE THESE OUT
            Services.CameraManager.EnterPlay();
            Services.UIManager.EnterPlay();
        }
    }
    #endregion
}
