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

    public bool inPlaceForSequence = false;


    // Stored inputs
    private float _horizontalInput = 0f;
    private float _verticalInput = 0f;
    private bool _jumpInput = false;
    private bool _sprintInput = false;

    private readonly float _movementSpeed = 3f;
    private readonly float _shiftMultiplier = 2.5f;

    // Movement due only to physics. Only affects jumping.
    private Vector3 _physicsMovement = Vector3.zero;
    // The target on-ground velocity due only to input.
    private Vector3 _targetMovementVector = Vector3.zero;
    // The resulting current movement vector.
    private Vector3 _currentMovementVector = Vector3.zero;

    // The rate at which _currentMovementVector is lerped to _targetMovementVector.
    private readonly float _movementChangeSpeed = 5f;
    private float turningSmoothVel;

    // Jumping.
    // The downward push of gravity that is added to currentMovement.
    private readonly float _gravity = -.981f;

    private readonly float _desiredJumpHeight = 100f;
    private readonly float _jumpForwardDistance = 1f;

    // The calculated jump speed (so we don't calculate square root every update).
    private float _jumpSpeed = 0f;

    private readonly float _jumpCooldown = .5f;
    private float _curJumpCooldown = 0f;


    private Vector3 raycastOriginOffset;
    [SerializeField] private LayerMask groundLayers;

    // Components

    private CharacterController _charController;

    private PlayerAnimation _playerAnimation;

    // The finite state machine of the current gamestate.
    private FiniteStateMachine<PlayerMovement> _fsm;

    private TaskManager _taskManager = new TaskManager();

    #endregion 

    #region Lifecycle Management

    private void Awake()
    {
        _fsm = new FiniteStateMachine<PlayerMovement>(this);
        _charController = GetComponent<CharacterController>();
        _playerAnimation = Services.PlayerAnimation;
        if (_playerAnimation == null)
            Logger.Warning("Failed to retrieve _playerAnimation");

        _jumpSpeed = Mathf.Sqrt(_desiredJumpHeight * -2f * _gravity) * Time.fixedDeltaTime;

        raycastOriginOffset = _charController.center - new Vector3(0, _charController.height/2.3f, 0f);
    }

    private void Start() => _fsm.TransitionTo<IdleState>();

    private void Update()
    {
        elapsedTime += Time.deltaTime;
        if (_curJumpCooldown >= 0f) _curJumpCooldown -= Time.deltaTime;
        _fsm.Update();
    }

    private void FixedUpdate()
    {
        GameState curGS = ((GameState)_fsm.CurrentState);
        if (curGS != null)
            curGS.FixedUpdate();
        _taskManager.Update();
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
    public void EnterPlay()
    {
        Debug.Log("ENTERING PLAY TRIGGERED!!!!!!!!!!!!!!!!!!!!!!!!!!!");
        _fsm.TransitionTo<MovingOnGroundState>();
    }

    // Forces the player to be idle.
    public void ForceIdle() => _fsm.TransitionTo<ForcedIdleState>();

    // Forces the player to stop moving.
    public void EnterPause()
    {
        _fsm.TransitionTo<PauseState>();
    }

    // Forces the player to be idle and faces toward NPC.
    public void EnterDialogue() => _fsm.TransitionTo<InDialogueState>();

    // Forces the player to walk up to the designated cutscene point and wait there.
    public void EnterMidCutscene() => _fsm.TransitionTo<MidCutsceneState>();

    #endregion

    #region Utilities

    // Returns if the player is currently on the ground.
    // Used two different checks (one faster, one raycast) to ensure correctness.
    private bool OnGround()
    {
        if (_charController.isGrounded) return true;
        return Physics.Raycast(transform.position + raycastOriginOffset, Vector3.down, out RaycastHit hit, .3f, groundLayers, QueryTriggerInteraction.Ignore);
    }

    // Returns whether or not the player entered any ground movement inputs W, A, S, D, NOT space.
    private bool GroundMovementInputsEntered => _horizontalInput != 0f || _verticalInput != 0f;

    public void ForceTransform(Vector3 pos, Quaternion rot)
    {
        _charController.enabled = false;
        transform.position = pos;
        transform.rotation = rot;
        _charController.enabled = true;
    }

    #endregion

    #region Automated Movement Delegates.

    private float elapsedTime = 0f;
    private readonly float maxTimeOnOneStep = 3f;
    private Vector3 initPos;
    private float targetRot;
    private Vector3 targetPos;

    private Task PlayerMoveToTransform(Transform targetPositionTrans, Transform targetDirectionTrans, Transform finalTarget)
    {
        // Define dialogue entry tasks.
        Task jumping = new DelegateTask(() => {
            _currentMovementVector.x = 0;
            _currentMovementVector.z = 0;
        }, ContinueUpwardAirMovement);
        Task falling = new DelegateTask(() => { _playerAnimation.Falling(true); }, ContinueDownwardAirMovement);
        Task rotateToPos = new DelegateTask(
            () => {
                elapsedTime = 0f;
                _playerAnimation.Moving(true);
                _playerAnimation.Falling(false);
                targetRot = Quaternion.LookRotation((targetPositionTrans.position - transform.position).normalized, Vector3.up).eulerAngles.y;
            },
            RotateToCorrectPos
        );

        Task moveToPos = new DelegateTask(
            () => {
                elapsedTime = 0f;

                _playerAnimation.Falling(false);
                _playerAnimation.Sprinting(false);
                _currentMovementVector = Vector3.zero;
                initPos = transform.position;
                targetPos = targetPositionTrans.position;
            },
            MoveToCorrectPos
        );
        Task rotateToTarget = new DelegateTask(
            () => {
                elapsedTime = 0f;
                targetRot = Quaternion.LookRotation(targetDirectionTrans.position - transform.position, Vector3.up).eulerAngles.y;
            },
            RotateToCorrectPos,
            () => { OnCorrectPlace(finalTarget); }
        );


        jumping.Then(falling).Then(rotateToPos).Then(moveToPos).Then(rotateToTarget);
        if (!OnGround())
            return jumping;
        else
            return rotateToPos;
    }

    private bool ContinueUpwardAirMovement()
    {
        AirMovement();
        return OnGround() || _currentMovementVector.y < 0f;
    }

    private bool ContinueDownwardAirMovement()
    {
        AirMovement();
        return OnGround();
    }

    private void AirMovement()
    {
        // Fall downwards
        _currentMovementVector.y += _gravity * Time.fixedDeltaTime;
        _charController.Move(_currentMovementVector);
    }

    private bool RotateToCorrectPos()
    {
        if (elapsedTime > maxTimeOnOneStep)
        {
            Logger.Warning("RotateToCorrectPos failsafe triggered");
            ForceTransform(transform.position, Quaternion.Euler(0, targetRot, 0)); // failsafe
        }
        transform.rotation = Quaternion.Euler(0f, Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRot, ref turningSmoothVel, .1f), 0f);
        _charController.Move(transform.forward * _movementSpeed * Time.fixedDeltaTime * .1f); // move forward slightly to make the spineAnimator not be so horrible.
        return Quaternion.Angle(transform.rotation, Quaternion.Euler(0, targetRot, 0)) < 5f;
    }

    private bool MoveToCorrectPos()
    {
        if (elapsedTime > maxTimeOnOneStep)
        {
            Logger.Warning("MoveToCorrectPos failsafe triggered");
            ForceTransform(targetPos, transform.rotation);
        }
        targetRot = Quaternion.LookRotation((targetPos - transform.position).normalized, Vector3.up).eulerAngles.y;
        transform.rotation = Quaternion.Euler(0f, Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRot, ref turningSmoothVel, .1f), 0f);

        _currentMovementVector = transform.forward * _movementSpeed * Time.fixedDeltaTime;
        _currentMovementVector.y = _gravity;
        _charController.Move(_currentMovementVector);
        return Vector3.Distance(transform.position, targetPos) < .5f;
    }

    private void OnCorrectPlace(Transform target)
    {
        inPlaceForSequence = true;

        _playerAnimation.Moving(false);
        _playerAnimation.Falling(false);

        _currentMovementVector = Vector3.zero;
        Vector3 lookPos = target.position - transform.position; // WRONG
        lookPos.y = 0;
        transform.rotation = Quaternion.LookRotation(lookPos);
    }

    private Task LastTask(Task curTask)
    {
        if (curTask.NextTask == null)
            return curTask;
        else
            return LastTask(curTask.NextTask);
    }

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
            Context._playerAnimation.Moving(false);
            Context._targetMovementVector = Vector3.zero;
        }

        public override void Update()
        {
            // Process inputs
            if (Context.GroundMovementInputsEntered)
                TransitionTo<MovingOnGroundState>();
            if (Context._jumpInput && Context._curJumpCooldown <= 0)
                TransitionTo<JumpingState>();
        }

        public override void FixedUpdate()
        {
            if (!Context.OnGround())
                TransitionTo<FallingState>();

            Context._currentMovementVector = Vector3.Lerp(Context._currentMovementVector, Context._targetMovementVector, Context._movementChangeSpeed * Time.fixedDeltaTime);
            Context._charController.Move(Context._currentMovementVector);
        }
    }

    // Player is forced to remain idle until told otherwise.
    private class ForcedIdleState : GameState
    {
        public override void OnEnter()
        {
            Context._currentMovementVector = Vector3.zero;
            Context._playerAnimation.Moving(false);
            Context._playerAnimation.Sitting(true);
        }

        public override void OnExit() => Context._playerAnimation.Sitting(false); // Maybe change to sit???
    }
    
    // Player is forced to pause movement.
    private class PauseState : GameState
    {

        private delegate void FixedUpdateFunc();
        private FixedUpdateFunc fixedUpdateFunc;

        public override void OnEnter()
        {
            if (!Context.OnGround())
                fixedUpdateFunc = ContinueInAirMovement;
            else
                OnLand();
        }

        public override void FixedUpdate() => fixedUpdateFunc();

        private void ContinueInAirMovement()
        {
            // Fall downwards
            Context._currentMovementVector.y += Context._gravity * Time.fixedDeltaTime;

            Context._charController.Move(Context._currentMovementVector);

            if (Context.OnGround())
            {
                OnLand();
            }
            else if (Context._currentMovementVector.y < 0f && Context._playerAnimation.IsFalling)
            {
                Context._playerAnimation.Falling(true);
            }
        }

        private void OnLand()
        {
            Context._playerAnimation.Falling(false);
            Context._playerAnimation.Moving(false);
            Context._currentMovementVector = Vector3.zero;
            fixedUpdateFunc = () => { };
        }
    }

    // Player is forced to remain idle, turns to look at NPC.
    private class InDialogueState : GameState
    {

        public override void OnEnter()
        {
            Context.inPlaceForSequence = false;

            if (Context._taskManager.HasTasks())
            {
                Services.NPCInteractionManager.FindClosestNPC();
            }
            
            Context._taskManager.Do(Context.PlayerMoveToTransform(
                Services.NPCInteractionManager.DialogueTrans,
                Services.NPCInteractionManager.closestNPC.transform,
                Services.NPCInteractionManager.closestNPC.GetPlayerCameraLookAtPosition()));
        }

        public override void OnExit() => Context.inPlaceForSequence = false;
    }

    // Player is currently moving on the ground.
    private class MovingOnGroundState : GameState
    {

        public override void OnEnter() => Context._playerAnimation.Moving(true);

        public override void Update()
        {
            // Process inputs.
            if (!Context.GroundMovementInputsEntered && Context.OnGround()) // Move to FixedUpdate
                TransitionTo<IdleState>();
            else if (Context._jumpInput && Context._curJumpCooldown <= 0)
                TransitionTo<JumpingState>();
        }

        // Physics calculations.
        public override void FixedUpdate()
        {
            if (!Context.OnGround())
                TransitionTo<FallingState>();
            

            Vector3 direction = new Vector3(Context._horizontalInput, 0f, Context._verticalInput).normalized;

            // If the player has entered input, calculate the forward angle and move the target movement.
            if (direction.sqrMagnitude >= .1f)
            {
                float targetAngle = Mathf.Atan2(Context._horizontalInput, Context._verticalInput) * Mathf.Rad2Deg + Services.CameraManager.CameraYAngle;
                float angle = Mathf.SmoothDampAngle(Context.transform.eulerAngles.y, targetAngle, ref Context.turningSmoothVel, .2f);
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
    }

    // Player is currently jumping. Possibly change into 2 states - a jump charging state and a released jump state.
    private class JumpingState : GameState
    {
        public override void OnEnter()
        {
            Context._curJumpCooldown = Context._jumpCooldown;
            
            Context._charController.Move(Context._currentMovementVector);
            Context._playerAnimation.Jump();
            JumpTasks();
        }

        private void JumpTasks()
        {
            float elapsedTime = 0f;
            float duration = 0.33f;
            DelegateTask moveAndWait = new DelegateTask(() => { }, () => 
            {
                elapsedTime += Time.fixedDeltaTime;
                Context._charController.Move(Context._currentMovementVector * .3f);
                return elapsedTime > duration;
            });

            DelegateTask jump = new DelegateTask(
                () =>
                {
                    Vector3 jumpVector = Context.transform.forward * Context._jumpForwardDistance;// + Vector3.up * Context._upwardJumpSpeed;
                    Context._currentMovementVector += jumpVector * Time.deltaTime;
                    Context._currentMovementVector.y = Context._jumpSpeed;
                },
                () =>
                {
                    // Fall downwards
                    Context._currentMovementVector.y += Context._gravity * Time.deltaTime;

                    Context._charController.Move(Context._currentMovementVector);

                    if (Context.OnGround() || Context._currentMovementVector.y < 0f)
                    {
                        return true;
                    }
                    return false;
                },
                () =>
                {
                    if (Context._fsm.CurrentState.GetType() == typeof(InDialogueState))
                    {
                        Logger.Warning("Jump task finished outside of NPC area. Transitioning to InDialogueState.");
                        TransitionTo<InDialogueState>();
                    }
                    else if (Context._fsm.CurrentState.GetType() == typeof(MidCutsceneState))
                    {
                        Logger.Warning("Jump task finished outside of NPC area. Transitioning to MidCutsceneState.");
                        TransitionTo<MidCutsceneState>();
                    }
                    else if (Context._currentMovementVector.y < 0f)
                    {
                        TransitionTo<FallingState>();
                    }
                    else
                    {
                        TransitionTo<IdleState>();
                    }
                });

            moveAndWait.Then(jump);

            Context._taskManager.Do(moveAndWait);
        }
    }

    // Player is currently falling.
    private class FallingState : GameState
    {
        public override void OnEnter() => Context._playerAnimation.Falling(true);

        public override void Update()
        {
            // Detect if on the ground. If moving, transition to the moving state.
            if (Context.OnGround())
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
            // Fall downwards
            Context._currentMovementVector.y += Context._gravity * Time.fixedDeltaTime;

            Context._charController.Move(Context._currentMovementVector);
        }

        public override void OnExit() => Context._playerAnimation.Falling(false);

    }


    // Player is forced to remain idle, turns to look at NPC.
    private class MidCutsceneState : GameState
    {

        public override void OnEnter()
        {
            Context.inPlaceForSequence = false;

            Task moveToInitPos = Context.PlayerMoveToTransform(
                Services.QuestItemRepository.TargetStep1PlayerPosition, // Position target for player
                Services.QuestItemRepository.TargetItemPosition, // direction target for player
                Services.QuestItemRepository.TargetItemPosition); // Not totally sure.

            Task phase2Start = Context.LastTask(moveToInitPos);
            //phase2Start

            Task reset1 = new ActionTask(() => { Context.inPlaceForSequence = false; });

            Task moveToMidPos = Context.PlayerMoveToTransform(
                Services.QuestItemRepository.TargetStep2PlayerPosition,
                Services.QuestItemRepository.TargetItemPosition,
                Services.QuestItemRepository.TargetItemPosition);

            phase2Start.Then(reset1).Then(moveToMidPos);

            Task phase3Start = Context.LastTask(moveToMidPos);
            Task reset2 = new ActionTask(() => { Context.inPlaceForSequence = false; });
            Task wait4Secs = new WaitTask(4f);
            Task forceFinalTransform = new ActionTask(() => { Context.ForceTransform(Services.QuestItemRepository.TargetStep3PlayerPosition.position, Services.QuestItemRepository.TargetStep3PlayerPosition.rotation); });
            //Task wait5Secs = new WaitTask(5f);
            //Task exitSequence = new ActionTask(() => { Context.EnterPlay(); Debug.Log("exitSequence!!!!!!!!!!!!!!!!!!!"); });

            phase3Start.Then(reset2).Then(wait4Secs).Then(forceFinalTransform);//.Then(wait5Secs).Then(exitSequence);

            Context._taskManager.Do(moveToInitPos);
        }
        

        public override void OnExit() => Context.inPlaceForSequence = false;
    }

    #endregion

}
