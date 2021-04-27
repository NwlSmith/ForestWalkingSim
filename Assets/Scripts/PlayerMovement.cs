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
    public bool moving = false;
    public bool movingOnGround = false;


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

    // The finite state machine of the current gamestate.
    private FiniteStateMachine<PlayerMovement> _fsm;

    private TaskManager _taskManager = new TaskManager();

    #endregion 

    #region Lifecycle Management

    private void Awake()
    {
        _fsm = new FiniteStateMachine<PlayerMovement>(this);
        _charController = GetComponent<CharacterController>();

        _jumpSpeed = Mathf.Sqrt(_desiredJumpHeight * -2f * _gravity) * Time.fixedDeltaTime;

        raycastOriginOffset = _charController.center - new Vector3(0, _charController.height/2.3f, 0f);

        RegisterEvents();
    }

    private void Start() => _fsm.TransitionTo<LocomotionState>();

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

    public void ResetInputs()
    {
        _horizontalInput = 0f;
        _verticalInput = 0f;
        _jumpInput = false;
        _sprintInput = false;
    }

    private void OnDestroy() => UnregisterEvents();

    private void RegisterEvents()
    {
        Services.EventManager.Register<OnStartMenu>(_fsm.TransitionTo<ForcedIdleState>);
        Services.EventManager.Register<OnEnterPlay>(_fsm.TransitionTo<LocomotionState>);
        Services.EventManager.Register<OnPause>(_fsm.TransitionTo<PauseState>);
        Services.EventManager.Register<OnEnterDialogue>(_fsm.TransitionTo<InDialogueState>);
        Services.EventManager.Register<OnEnterMidCutscene>(_fsm.TransitionTo<MidCutsceneState>);
        Services.EventManager.Register<OnEnterEndCutscene>(_fsm.TransitionTo<EndCutsceneState>);
        Services.EventManager.Register<OnEnterEndGame>(_fsm.TransitionTo<ForcedIdleState>);
    }

    private void UnregisterEvents()
    {
        Services.EventManager.Unregister<OnStartMenu>(_fsm.TransitionTo<ForcedIdleState>);
        Services.EventManager.Unregister<OnEnterPlay>(_fsm.TransitionTo<LocomotionState>);
        Services.EventManager.Unregister<OnPause>(_fsm.TransitionTo<PauseState>);
        Services.EventManager.Unregister<OnEnterDialogue>(_fsm.TransitionTo<InDialogueState>);
        Services.EventManager.Unregister<OnEnterMidCutscene>(_fsm.TransitionTo<MidCutsceneState>);
        Services.EventManager.Unregister<OnEnterEndCutscene>(_fsm.TransitionTo<EndCutsceneState>);
        Services.EventManager.Unregister<OnEnterEndGame>(_fsm.TransitionTo<ForcedIdleState>);
    }

    #endregion

    #region Utilities

    // Returns if the player is currently on the ground.
    // Used two different checks (one faster, one raycast) to ensure correctness.
    private bool OnGround()
    {
        if (_charController.isGrounded) return true;
        if (Physics.Raycast(transform.position + raycastOriginOffset, Vector3.down, out RaycastHit hit, .4f, groundLayers, QueryTriggerInteraction.Ignore))
            return Vector3.Angle(hit.normal, Vector3.up) <= _charController.slopeLimit;
        return false;
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
        Task falling = new DelegateTask(() => { PlayerAnimation.Falling(true); }, ContinueDownwardAirMovement);
        Task rotateToPos = new DelegateTask(
            () => {
                elapsedTime = 0f;
                PlayerAnimation.Moving(true);
                PlayerAnimation.Falling(false);
                targetRot = Quaternion.LookRotation((targetPositionTrans.position - transform.position).normalized, Vector3.up).eulerAngles.y;
            },
            RotateToCorrectPos
        );

        Task moveToPos = new DelegateTask(
            () => {
                elapsedTime = 0f;

                PlayerAnimation.Falling(false);
                PlayerAnimation.Sprinting(false);
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

        PlayerAnimation.Moving(false);
        PlayerAnimation.Falling(false);

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

    // Player is forced to remain idle until told otherwise.
    private class ForcedIdleState : GameState
    {
        public override void OnEnter()
        {
            Context._currentMovementVector = Vector3.zero;
            PlayerAnimation.Moving(false);
            PlayerAnimation.Sitting(true);
            Context.ResetInputs();
        }

        public override void OnExit() => PlayerAnimation.Sitting(false); // Maybe change to sit???
    }
    
    // Player is forced to pause movement.
    private class PauseState : GameState
    {

        private delegate void FixedUpdateFunc();
        private FixedUpdateFunc fixedUpdateFunc;

        public override void OnEnter()
        {
            Context.ResetInputs();
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
            else if (Context._currentMovementVector.y < 0f && PlayerAnimation.IsFalling)
            {
                PlayerAnimation.Falling(true);
            }
        }

        private void OnLand()
        {
            PlayerAnimation.Falling(false);
            PlayerAnimation.Moving(false);
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
                NPCInteractionManager.FindClosestNPC();
            }
            
            Context._taskManager.Do(DefineSequence());
        }

        private Task DefineSequence()
        {
            return Context.PlayerMoveToTransform(
                NPCInteractionManager.DialogueTrans,
                NPCInteractionManager.closestNPC.transform,
                NPCInteractionManager.closestNPC.GetPlayerCameraLookAtPosition());
        }

        public override void OnExit() => Context.inPlaceForSequence = false;
    }

    // Finite State Machine base for all player-controlled locomotion.
    private class LocomotionState : GameState
    {

        // The finite state machine of the current gamestate.
        private readonly FiniteStateMachine<LocomotionState> _fsm;

        public LocomotionState() => _fsm = new FiniteStateMachine<LocomotionState>(this);

        #region Lifecycle Management.
        public override void OnEnter()
        {

            PlayerAnimation.Moving(false);
            Context._targetMovementVector = Vector3.zero;

            // Process inputs
            if (Context.GroundMovementInputsEntered)
                _fsm.TransitionTo<MovingOnGroundState>();
            else if (Context._jumpInput && Context._curJumpCooldown <= 0)
                _fsm.TransitionTo<JumpingState>();
            else if (!Context.OnGround())
                _fsm.TransitionTo<FallingState>();
            else
                _fsm.TransitionTo<IdleState>();
        }

        public override void Update() => _fsm.Update();

        public override void FixedUpdate()
        {
            MovementState curGS = ((MovementState)_fsm.CurrentState);
            if (curGS != null)
                curGS.FixedUpdate();
        }

        #endregion

        #region States.

        private abstract class MovementState : FiniteStateMachine<LocomotionState>.State
        {
            public virtual void FixedUpdate() { }

            protected PlayerMovement Cont => Context.Context;
        }

        // Player not inputting any inputs, but they are allowed to move if they want to.
        private class IdleState : MovementState
        {
            public override void OnEnter()
            {
                Cont.moving = false;
                PlayerAnimation.Moving(false);
                Cont._targetMovementVector = Vector3.zero;
            }

            public override void Update()
            {
                // Process inputs
                if (Cont.GroundMovementInputsEntered)
                    TransitionTo<MovingOnGroundState>();
                if (Cont._jumpInput && Cont._curJumpCooldown <= 0)
                    TransitionTo<JumpingState>();
            }

            public override void FixedUpdate()
            {
                if (!Cont.OnGround())
                    TransitionTo<FallingState>();

                Cont._currentMovementVector = Vector3.Lerp(Cont._currentMovementVector, Cont._targetMovementVector, Cont._movementChangeSpeed * Time.fixedDeltaTime);
                Cont._charController.Move(Cont._currentMovementVector);
            }
        }

        // Player is currently moving on the ground.
        private class MovingOnGroundState : MovementState
        {

            public override void OnEnter()
            {
                Cont.moving = true;
                Cont.movingOnGround = true;
                PlayerAnimation.Moving(true);
            }

            public override void Update()
            {
                // Process inputs.
                if (!Cont.GroundMovementInputsEntered && Cont.OnGround()) // Move to FixedUpdate
                    TransitionTo<IdleState>();
                else if (Cont._jumpInput && Cont._curJumpCooldown <= 0)
                    TransitionTo<JumpingState>();
            }

            // Physics calculations.
            public override void FixedUpdate()
            {
                if (!Cont.OnGround())
                    TransitionTo<FallingState>();


                Vector3 direction = new Vector3(Cont._horizontalInput, 0f, Cont._verticalInput).normalized;

                // If the player has entered input, calculate the forward angle and move the target movement.
                if (direction.sqrMagnitude >= .1f)
                {
                    float targetAngle = Mathf.Atan2(Cont._horizontalInput, Cont._verticalInput) * Mathf.Rad2Deg + Services.CameraManager.CameraYAngle;
                    float angle = Mathf.SmoothDampAngle(Cont.transform.eulerAngles.y, targetAngle, ref Cont.turningSmoothVel, .2f);
                    Cont.transform.rotation = Quaternion.Euler(0f, angle, 0f);

                    Cont._targetMovementVector = (Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward).normalized * Cont._movementSpeed * Time.fixedDeltaTime * (Cont._sprintInput ? Cont._shiftMultiplier : 1f);
                    PlayerAnimation.Sprinting(Cont._sprintInput);
                }
                else
                {
                    Cont._targetMovementVector = Vector3.zero;
                }

                // Lerp the current movement toward the targetmovement
                Cont._currentMovementVector = Vector3.Lerp(Cont._currentMovementVector, Cont._targetMovementVector, Cont._movementChangeSpeed * Time.fixedDeltaTime);

                // Fall downwards
                if (Cont._currentMovementVector.y > 0)
                    Cont._currentMovementVector.y -= Cont._gravity * Time.fixedDeltaTime;
                else
                    Cont._currentMovementVector.y = 4 * Cont._gravity * Time.fixedDeltaTime;


                Cont._charController.Move(Cont._currentMovementVector);
            }

            public override void OnExit() => Cont.movingOnGround = false;
        }

        // Player is currently jumping. Possibly change into 2 states - a jump charging state and a released jump state.
        private class JumpingState : MovementState
        {

            private const float _jumpingMovementMultiplier = .4f;

            public override void OnEnter()
            {
                Cont._curJumpCooldown = Cont._jumpCooldown;
                Cont.moving = true;

                Cont._charController.Move(Cont._currentMovementVector);
                PlayerAnimation.Jump();
                JumpTasks();
            }

            private void JumpTasks()
            {
                float elapsedTime = 0f;
                float duration = 0.33f;
                DelegateTask moveAndWait = new DelegateTask(() => { }, () =>
                {
                    elapsedTime += Time.fixedDeltaTime;
                    Cont._charController.Move(Cont._currentMovementVector * _jumpingMovementMultiplier);
                    return elapsedTime > duration;
                });

                DelegateTask jump = new DelegateTask(
                    () =>
                    {
                        Vector3 jumpVector = Cont.transform.forward * Cont._jumpForwardDistance;// + Vector3.up * Cont._upwardJumpSpeed;
                        Cont._currentMovementVector += jumpVector * Time.deltaTime;
                        Cont._currentMovementVector.y = Cont._jumpSpeed;
                    },
                    () =>
                    {
                        // Fall downwards
                        Cont._currentMovementVector.y += Cont._gravity * Time.deltaTime;
                        Cont._charController.Move(Cont._currentMovementVector);
                        if (Cont.OnGround() || Cont._currentMovementVector.y <= 0f)
                        {
                            return true;
                        }
                        return false;
                    },
                    () =>
                    {
                        System.Type type = Cont._fsm.CurrentState.GetType();
                        if (type == typeof(InDialogueState) || type == typeof(MidCutsceneState) || type == typeof(ForcedIdleState))
                        {
                            Logger.Warning($"Jump task finished outside of NPC area. Currently in {type.FullName}.");
                        }
                        else if (Cont._currentMovementVector.y < 0f)
                        {
                            TransitionTo<FallingState>();
                        }
                        else if (Cont.GroundMovementInputsEntered)
                        {
                            TransitionTo<MovingOnGroundState>();
                        }
                        else
                        {
                            TransitionTo<IdleState>();
                        }
                    });

                moveAndWait.Then(jump);

                Cont._taskManager.Do(moveAndWait);
            }
        }

        // Player is currently falling.
        private class FallingState : MovementState
        {
            public override void OnEnter()
            {
                PlayerAnimation.Falling(true);
                Cont.moving = true;
            }

            public override void Update()
            {
                // Detect if on the ground. If moving, transition to the moving state.
                if (Cont.OnGround())
                {
                    if (Cont.GroundMovementInputsEntered)
                        TransitionTo<MovingOnGroundState>();
                    else
                        TransitionTo<IdleState>();
                }
            }

            // Physics calculations.
            public override void FixedUpdate()
            {
                // Fall downwards
                Cont._currentMovementVector.y += Cont._gravity * Time.fixedDeltaTime;
                Cont._charController.Move(Cont._currentMovementVector);
            }

            public override void OnExit() => PlayerAnimation.Falling(false);

        }

        #endregion
    }

    // Player moves through sequence and faces South.
    private class MidCutsceneState : GameState
    {

        public override void OnEnter() => Context._taskManager.Do(DefineSequence());

        private Task DefineSequence()
        {
            Task start = new ActionTask(() =>
            {
                Context.ResetInputs();
                Context.inPlaceForSequence = false;
            });

            Task moveToInitPos = Context.PlayerMoveToTransform(
                Services.QuestItemRepository.TargetStep1PlayerPosition, // Position target for player
                Services.QuestItemRepository.TargetItemPosition, // direction target for player
                Services.QuestItemRepository.TargetItemPosition); // Not totally sure.

            start.Then(moveToInitPos);

            Task phase2Start = Context.LastTask(moveToInitPos);

            Task reset1 = new ActionTask(() => { Context.inPlaceForSequence = false; });

            Task moveToMidPos = Context.PlayerMoveToTransform(
                Services.QuestItemRepository.TargetStep2PlayerPosition,
                Services.QuestItemRepository.TargetItemPosition,
                Services.QuestItemRepository.TargetItemPosition);

            phase2Start.Then(reset1).Then(moveToMidPos);

            Task phase3Start = Context.LastTask(moveToMidPos);
            Task reset2 = new ActionTask(() => { Context.inPlaceForSequence = false; });
            Task wait4Secs = new WaitTask(4f);
            Task forceFinalTransform = new ActionTask(() => { Context.ForceTransform(Services.QuestItemRepository.TargetStep4PlayerPosition.position, Services.QuestItemRepository.TargetStep4PlayerPosition.rotation); });

            phase3Start.Then(reset2).Then(wait4Secs).Then(forceFinalTransform);

            return start;
        }

        public override void OnExit() => Context.inPlaceForSequence = false;
    }

    // Player moves through sequence and faces NPC.
    private class EndCutsceneState : GameState
    {
        public override void OnEnter() => Context._taskManager.Do(DefineSequence());

        private Task DefineSequence()
        {
            Task start = new ActionTask(() =>
            {
                Context.ResetInputs();
                Context.inPlaceForSequence = false;
            });

            Task moveToInitPos = Context.PlayerMoveToTransform(
                Services.QuestItemRepository.TargetStep1PlayerPosition, // Position target for player
                Services.QuestItemRepository.TargetItemPosition, // direction target for player
                Services.QuestItemRepository.TargetItemPosition); // Not totally sure.

            start.Then(moveToInitPos);

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
            Task forceFinalTransform = new ActionTask(() => { Context.ForceTransform(Services.QuestItemRepository.TargetStep4PlayerPosition.position, Services.QuestItemRepository.TargetStep4PlayerPosition.rotation); });
            //Task wait5Secs = new WaitTask(5f);
            //Task exitSequence = new ActionTask(() => { Context.EnterPlay(); Debug.Log("exitSequence!!!!!!!!!!!!!!!!!!!"); });

            phase3Start.Then(reset2).Then(wait4Secs).Then(forceFinalTransform);//.Then(wait5Secs).Then(exitSequence);

            return start;
        }

        public override void OnExit() => Context.inPlaceForSequence = false;
    }

    #endregion

}
