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
 * Issues:
 * - Need to fix player position during dialogue.
 * - Need to allow for multiple-NPC conversations. - Use SetTargetNPC for camera, Not sure what to do for dialogueController
 * - Need to make mouse looking faster
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

    // Called on Quit.
    public void Quit()
    {
        Application.Quit();
    }

    #endregion 

    #region States

    private abstract class GameState : FiniteStateMachine<GameManager>.State { }

    private class StartPlay : GameState
    {
        public override void OnEnter()
        {
            Debug.Log("GameManager: Entered StartPlay");
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
            Services.PlayerMovement.EnterDialogue();
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

            Services.PlayerMovement.EnterPlay();
            Services.CameraManager.EnterPlay();
            Services.UIManager.EnterPlay();
            Services.NPCInteractionManager.ExitDialogue();
        }
    }
    #endregion
}
