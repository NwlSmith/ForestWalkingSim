using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Creator: Nate Smith
 * Creation Date: 2/11/2021
 * Description: Processes the player's movement input and moves the player.
 * 
 * Input is processed differently based on what state the player is in.
 */

public class PlayerMovement : MonoBehaviour
{

    #region Variables

    // The finite state machine of the current gamestate.
    private FiniteStateMachine<PlayerMovement> _fsm;

    private float _horizontal = 0f;
    private float _vertical = 0f;
    private float _jump = 0f;
    private bool _sprint = false;

    private readonly float _movementSpeed = 5f;
    private readonly float _shiftMultiplier = 1.5f;

    private Rigidbody _rb;
    private CharacterController _charController;

    #endregion 

    #region Lifecycle Management

    private void Awake()
    {
        _fsm = new FiniteStateMachine<PlayerMovement>(this);
        _rb = GetComponent<Rigidbody>();
        _charController = GetComponent<CharacterController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        _fsm.TransitionTo<IdleState>();
    }

    // Update is called once per frame
    void Update()
    {
        _fsm.Update();
    }

    #endregion 

    // Updates the player movement inputs
    public void InputUpdate(float hor, float vert, float jump, bool sprint)
    {
        _horizontal = hor;
        _vertical = vert;
        _jump = jump;
        _sprint = sprint;
    }

    // Returns if the player is currently on the ground.
    private bool OnGround()
    {
        return true;
    }

    // Returns whether or not the player entered any ground movement inputs W, A, S, D, NOT space.
    private bool GroundMovementInputsEntered()
    {
        return _horizontal != 0f || _vertical != 0f;
    }

    #region States

    private abstract class GameState : FiniteStateMachine<PlayerMovement>.State { }

    // Player not inputting any inputs, but they are allowed to move if they want to.
    private class IdleState : GameState
    {
        public override void OnEnter()
        {
            Debug.Log("IdleState enter");
        }

        public override void Update()
        {
            base.Update();

            // Process inputs
            if (Context.GroundMovementInputsEntered())
                TransitionTo<MovingOnGroundState>();
            else if (Context._jump != 0)
                TransitionTo<JumpingState>();
        }

        public override void OnExit()
        {

        }
    }

    // Player is forced to remain idle until told otherwise.
    private class ForcedIdleState : GameState
    {
        public override void OnEnter()
        {

        }

        public override void Update()
        {
            base.Update();
            
        }

        public override void OnExit()
        {

        }
    }

    // Player is currently moving on the ground.
    private class MovingOnGroundState : GameState
    {
        public override void OnEnter()
        {
            Debug.Log("MovingOnGround enter");

        }

        public override void Update()
        {
            base.Update();

            // Process inputs
            if (!Context.GroundMovementInputsEntered())
                TransitionTo<IdleState>();
            else if (Context._jump != 0)
                TransitionTo<JumpingState>();

            // Movement pseudocode
            // Turn to face the horizontal vector of the camera.
            // Move in relation to that forward vector according to the movement keys
            Debug.Log("MovingOnGroundUpdate");
            Vector3 inputMovementVector = new Vector3(Context._horizontal, 0, Context._vertical) * Context._movementSpeed * Time.deltaTime;

            

            Context._charController.Move(inputMovementVector * Time.deltaTime);
        }

        public override void OnExit()
        {

        }
    }

    // Player is currently jumping.
    private class JumpingState : GameState
    {
        public override void OnEnter()
        {

        }

        public override void Update()
        {
            base.Update();

            // Process inputs
            // Move in air?

            // Detect if moving up or down?


            // Detect if hit the ground?
            // transfer to idle if not moving, moving if moving
        }

        public override void OnExit()
        {

        }
    }
    #endregion

}
