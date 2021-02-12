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

    private float _horizontalInput = 0f;
    private float _verticalInput = 0f;
    private float _jumpInput = 0f;
    private bool _sprintInput = false;

    private readonly float _movementSpeed = 5f;
    private readonly float _shiftMultiplier = 1.5f;

    // Movement due only to physics. Only affects jumping.
    private Vector3 _physicsMovement = Vector3.zero;
    // The target on-ground velocity due only to input.
    private Vector3 _targetMovementVector = Vector3.zero;
    // The resulting current movement vector.
    private Vector3 _currentMovementVector = Vector3.zero;

    // The rate at which _currentMovementVector is lerped to _targetMovementVector.
    private readonly float _movementChangeSpeed = 3f;
    // The downward push of gravity that is added to currentMovement.
    private readonly Vector3 _downwardGravityPush = new Vector3(0f, -5f, 0f);
    // The vector used to move the player when jumping.
    private readonly Vector3 _jumpVector = new Vector3(0f, 30f, 1f);

    private readonly float _jumpCooldown = .5f;
    private float _curJumpCooldown = 0f;
    /*
     * We will have movement target which is directly from input and current movement.
     * lerp between target and current movement.
     * 
     * ALSO
     * Rotate input vector by camera pos (simplified lol) and set forward to that.
     * 
     * Gravity?
     * gravity will be calculated separately, only when jumping - NO, need to calculate slopes too... hmmmm
     * No, make the current movement y axis -2
     * 
     * When jumping, take current velocity, add in a jump speed, set physicsvector to that
     * in jump, check for ground, subtract gravity from physicsvector.
     */

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
        if (_curJumpCooldown >= 0f)
        {
            _curJumpCooldown -= Time.deltaTime;
        }

        _fsm.Update();
    }

    #endregion 

    // Updates the player movement inputs. Called in InputManager.
    public void InputUpdate(float hor, float vert, float jump, bool sprint)
    {
        _horizontalInput = hor;
        _verticalInput = vert;
        _jumpInput = jump;
        _sprintInput = sprint;
    }

    // Returns if the player is currently on the ground.
    private bool OnGround()
    {
        return true;
    }

    // Returns whether or not the player entered any ground movement inputs W, A, S, D, NOT space.
    private bool GroundMovementInputsEntered()
    {
        return _horizontalInput != 0f || _verticalInput != 0f;
    }

    #region States

    private abstract class GameState : FiniteStateMachine<PlayerMovement>.State { }

    // Player not inputting any inputs, but they are allowed to move if they want to.
    private class IdleState : GameState
    {
        public override void OnEnter()
        {
            Debug.Log("IdleState enter");
            Context._currentMovementVector.y = 0f;
        }

        public override void Update()
        {
            base.Update();

            // Process inputs
            if (Context.GroundMovementInputsEntered() || Context._currentMovementVector != Vector3.zero)
                TransitionTo<MovingOnGroundState>();
            else if (Context._jumpInput != 0 && Context._curJumpCooldown <= 0)
                TransitionTo<JumpingState>();
            else if (!Context.OnGround())
                TransitionTo<FallingState>();
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

            // Process inputs.
            if (Context._currentMovementVector == Vector3.zero) // !Context.GroundMovementInputsEntered()
                TransitionTo<IdleState>();
            else if (Context._jumpInput != 0 && Context._curJumpCooldown <= 0)
                TransitionTo<JumpingState>();
            else if (!Context.OnGround())
                TransitionTo<FallingState>();


            // Calculate the target movement.
            Context._targetMovementVector = new Vector3(Context._horizontalInput, 0, Context._verticalInput).normalized * Context._movementSpeed * Time.deltaTime;

            // Lerp the current movement toward the targetmovement
            Context._currentMovementVector = Vector3.Lerp(Context._currentMovementVector, Context._targetMovementVector, Context._movementChangeSpeed * Time.deltaTime); // Maybe change to Slerp?

            // Fall downwards
            Context._currentMovementVector.y = Context._downwardGravityPush.y * Time.deltaTime;

            Context._charController.Move(Context._currentMovementVector);

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
            Debug.Log("JumpingState enter");
            Context._currentMovementVector += Context._jumpVector * Time.deltaTime;
            Context._curJumpCooldown = Context._jumpCooldown;
        }

        public override void Update()
        {
            base.Update();

            // Process inputs
            // Move in air?

            // Detect if moving up or down?

            if (false)//Context.OnGround())
            {
                TransitionTo<IdleState>();
            }
            else if (Context._currentMovementVector.y < 0f)
            {
                TransitionTo<FallingState>();
            }

            // Fall downwards
            Context._currentMovementVector += Context._downwardGravityPush * Time.deltaTime;

            Context._charController.Move(Context._currentMovementVector);
            // Detect if hit the ground?
            // transfer to idle if not moving, moving if moving
        }

        public override void OnExit()
        {

        }
    }

    // Player is currently falling.
    private class FallingState : GameState
    {
        public override void OnEnter()
        {
            Debug.Log("FallingState enter");

        }

        public override void Update()
        {
            base.Update();

            // Process inputs
            // Move in air?

            // Detect if moving up or down?
            if (Context.OnGround())
            {
                TransitionTo<IdleState>();
            }

            // Detect if hit the ground?
            // transfer to idle if not moving, moving if moving
        }

        public override void OnExit()
        {

        }
    }
    #endregion

}
