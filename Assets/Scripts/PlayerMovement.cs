using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Creator: Nate Smith
 * Creation Date: 2/11/2021
 * Description: Processes the player's movement input and moves the player.
 * 
 * Input is processed differently based on what state the player is in.'
 * 
 * NOTE: ADD STATE TO FORCE PLAYER TO WALK TO A PRE-DEFINED POSITION WHEN TALKING TO NPCS.
 */

public class PlayerMovement : MonoBehaviour
{

    #region Variables

    // The finite state machine of the current gamestate.
    private FiniteStateMachine<PlayerMovement> _fsm;

    // Stored inputs
    private float _horizontalInput = 0f;
    private float _verticalInput = 0f;
    private bool _jumpInput = false;
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

    // Jumping.
    // The downward push of gravity that is added to currentMovement.
    private readonly float _gravity = -.981f;

    private readonly float _desiredJumpHeight = 100f;
    private readonly float _jumpForwardDistance = 1f;

    // The calculated jump speed (so we don't calculate square root every update).
    private float _jumpSpeed = 0f;

    private readonly float _jumpCooldown = .5f;
    private float _curJumpCooldown = 0f;

    // Components

    private CharacterController _charController;

    private PlayerAnimation _playerAnimation;

    #endregion 

    #region Lifecycle Management

    private void Awake()
    {
        _fsm = new FiniteStateMachine<PlayerMovement>(this);
        _charController = GetComponent<CharacterController>();
        _playerAnimation = GetComponent<PlayerAnimation>();
        if (_playerAnimation == null)
            Debug.LogWarning("Failed to retrieve _playerAnimation");

        _jumpSpeed = Mathf.Sqrt(_desiredJumpHeight * -2f * _gravity) * Time.fixedDeltaTime;
    }

    // Start is called before the first frame update
    void Start() => _fsm.TransitionTo<IdleState>();

    // Update is called once per frame
    void Update()
    {
        if (_curJumpCooldown >= 0f)
        {
            _curJumpCooldown -= Time.deltaTime;
        }

        _fsm.Update();
    }

    void FixedUpdate()
    {
        GameState curGS = ((GameState)_fsm.CurrentState);
        if (curGS != null)
            curGS.FixedUpdate();
    }

    // Updates the player movement inputs. Called in InputManager.
    public void InputUpdate(float hor, float vert, bool jump, bool sprint)
    {
        _horizontalInput = hor;
        _verticalInput = vert;
        _jumpInput = jump;
        _sprintInput = sprint;
    }

    #endregion

    #region Triggers

    // Returns player to play mode.
    public void EnterPlay() => _fsm.TransitionTo<IdleState>();

    // Forces the player to be idle.
    public void ForceIdle() => _fsm.TransitionTo<ForcedIdleState>();

    // Forces the player to stop moving.
    public void EnterPause()
    {
        _fsm.TransitionTo<PauseState>();
    }

    // Forces the player to be idle and faces toward NPC.
    public void EnterDialogue() => _fsm.TransitionTo<InDialogueState>();

    public void ForceTransform(Vector3 pos, Quaternion rot)
    {
        _charController.enabled = false;
        transform.position = pos;
        transform.rotation = rot;
        _charController.enabled = true;
    }

    #endregion

    #region Utilities

    // Returns if the player is currently on the ground.
    private bool OnGround => _charController.isGrounded;

    // Returns whether or not the player entered any ground movement inputs W, A, S, D, NOT space.
    private bool GroundMovementInputsEntered => _horizontalInput != 0f || _verticalInput != 0f;

    #endregion

    #region States

    private abstract class GameState : FiniteStateMachine<PlayerMovement>.State {
        public virtual void FixedUpdate() { }
    }

    // Player not inputting any inputs, but they are allowed to move if they want to.
    private class IdleState : GameState
    {
        public override void OnEnter()
        {
            Debug.Log("IdleState enter");
            //Context._currentMovementVector.y = 0f;
            
            Context._playerAnimation.Moving(false); // Maybe change to sit???
        }

        public override void Update()
        {
            base.Update();

            // Process inputs
            if (Context.GroundMovementInputsEntered || Context._currentMovementVector != Vector3.zero)
                TransitionTo<MovingOnGroundState>();
            else if (Context._jumpInput && Context._curJumpCooldown <= 0)
                TransitionTo<JumpingState>();
            else if (!Context.OnGround)
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
            Context._currentMovementVector = Vector3.zero;
            Context._playerAnimation.Moving(false); // Maybe change to sit???
        }

        public override void Update() => base.Update();

        // Physics calculations.
        public override void FixedUpdate() => base.FixedUpdate();

        public override void OnExit() { }
    }
    
    // Player is forced to pause movement.
    private class PauseState : GameState
    {
        public override void OnEnter()
        {
            Context._playerAnimation.Moving(false); // Maybe change to sit???
        }

        public override void OnExit() { }
    }

    // Player is forced to remain idle, turns to look at NPC.
    private class InDialogueState : GameState
    {
        public override void OnEnter()
        {
            Context._playerAnimation.Moving(false); // Maybe change to sit???

            Context._currentMovementVector = Vector3.zero;
            Vector3 lookPos = Services.NPCInteractionManager.closestNPC.GetPlayerCameraLookAtPosition().position - Context.transform.position;
            lookPos.y = 0;
            Context.transform.rotation = Quaternion.LookRotation(lookPos);
            // SET LOCATION? LERP TO LOCATION?
        }
        

        public override void OnExit() { }
    }

    // Player is currently moving on the ground.
    private class MovingOnGroundState : GameState
    {
        private float turningSmoothVel;

        public override void OnEnter()
        {
            Debug.Log("MovingOnGround enter");
            //Context._currentMovementVector.y = Context._gravity * Time.fixedDeltaTime;
            Context._playerAnimation.Moving(true);
        }

        public override void Update()
        {
            base.Update();

            // Process inputs.
            if (Context._currentMovementVector == Vector3.zero) // !Context.GroundMovementInputsEntered()
                TransitionTo<IdleState>();
            else if (Context._jumpInput && Context._curJumpCooldown <= 0)
                TransitionTo<JumpingState>();
            else if (!Context.OnGround)
                TransitionTo<FallingState>();
        }

        // Physics calculations.
        public override void FixedUpdate()
        {
            base.FixedUpdate();

            Vector3 direction = new Vector3(Context._horizontalInput, 0f, Context._verticalInput).normalized;

            // If the player has entered input, calculate the forward angle and move the target movement.
            if (direction.sqrMagnitude >= .1f)
            {
                float targetAngle = Mathf.Atan2(Context._horizontalInput, Context._verticalInput) * Mathf.Rad2Deg + Services.CameraManager.CameraYAngle;
                float angle = Mathf.SmoothDampAngle(Context.transform.eulerAngles.y, targetAngle, ref turningSmoothVel, .2f);
                Context.transform.rotation = Quaternion.Euler(0f, angle, 0f);

                Context._targetMovementVector = (Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward).normalized * Context._movementSpeed * Time.fixedDeltaTime * (Context._sprintInput ? Context._shiftMultiplier : 1f);
                Context._playerAnimation.Sprinting(Context._sprintInput);
            }
            else
            {
                Context._targetMovementVector = Vector3.zero;
            }

            // Lerp the current movement toward the targetmovement
            Context._currentMovementVector = Vector3.Lerp(Context._currentMovementVector, Context._targetMovementVector, Context._movementChangeSpeed * Time.fixedDeltaTime);

            // Fall downwards
            Context._currentMovementVector.y = 4 * Context._gravity * Time.fixedDeltaTime;

            // Finally, move the character to that vector.
            Context._charController.Move(Context._currentMovementVector);
        }

        public override void OnExit() { }
    }

    // Player is currently jumping. Possibly change into 2 states - a jump charging state and a released jump state.
    private class JumpingState : GameState
    {
        public override void OnEnter()
        {
            //Debug.Log("JumpingState enter");
            Vector3 jumpVector = Context.transform.forward * Context._jumpForwardDistance;// + Vector3.up * Context._upwardJumpSpeed;
            Context._currentMovementVector += jumpVector * Time.fixedDeltaTime;
            Context._currentMovementVector.y = Context._jumpSpeed;

            Context._curJumpCooldown = Context._jumpCooldown;
            
            Context._charController.Move(Context._currentMovementVector);
        }

        public override void Update()
        {
            base.Update();

            // Process inputs in air?

            // Detect if on the ground or moving down.
            //Debug.Log("Current movement vector " + Context._currentMovementVector.y);

            if (Context.OnGround)
            {
                TransitionTo<IdleState>();
            }
            else if (Context._currentMovementVector.y < 0f)
            {
                TransitionTo<FallingState>();
            }
        }

        // Physics calculations.
        public override void FixedUpdate()
        {
            base.FixedUpdate();

            // Fall downwards
            Context._currentMovementVector.y += Context._gravity * Time.fixedDeltaTime;

            Context._charController.Move(Context._currentMovementVector);
        }

        public override void OnExit() { }
    }

    // Player is currently falling.
    private class FallingState : GameState
    {
        public override void OnEnter()
        {
            //Debug.Log("FallingState enter");

        }

        public override void Update()
        {
            base.Update();

            // Process inputs in air?

            // Detect if on the ground. If moving, transition to the moving state.
            if (Context.OnGround)
            {
                if (Context.GroundMovementInputsEntered)
                    TransitionTo<MovingOnGroundState>();
                else
                    TransitionTo<IdleState>();
            }
        }

        // Physics calculations.
        public override void FixedUpdate()
        {
            base.FixedUpdate();

            // Fall downwards
            Context._currentMovementVector.y += Context._gravity * Time.fixedDeltaTime;

            Context._charController.Move(Context._currentMovementVector);
        }

        public override void OnExit() { }
    }
    #endregion

}
