using System.Collections;
using UnityEngine;
using Cinemachine;
/*
 * Creator: Nate Smith
 * Creation Date: 2/13/2021
 * Description: Camera management script.
 * 
 * Handles the state and behavior of cameras.
 * 
 * Defaults to play-mode camera, but can be transitioned to other camera state as well.
 * 
 * Issues:
 * - Camera can get buggy when holding a holdableObject
 * - Must be able to accommodate having conversations with multiple people!!!!!
 */
public class CameraManager : MonoBehaviour
{

    // The finite state machine of the current CameraState.
    private FiniteStateMachine<CameraManager> _fsm;
    private readonly TaskManager _taskManager = new TaskManager();

    public float mouseSensitivity = 200f;
    
    [SerializeField] private Transform npcCameraTarget;
    [SerializeField] private Transform playerConvoCameraPos;
    [SerializeField] private Transform targetVector;
    [SerializeField] private Rigidbody targetVectorRB; // TEST
    [SerializeField] private CinemachineVirtualCamera startCamera;
    [SerializeField] private CinemachineVirtualCamera mainFollowCamera;
    [SerializeField] private CinemachineVirtualCamera playerCameraView;
    [SerializeField] private CinemachineVirtualCamera npcCameraView;
    [SerializeField] private CinemachineVirtualCamera cutsceneCamera;
    [SerializeField] private CinemachineVirtualCamera cutsceneDollyCamera1;

    // Ingame camera movement.
    private float _curVertRot = 0f;
    private float _curHorRot = 0f;
    private float _mouseY = 0f;
    private float _mouseX = 0f;
    private float _recenterTimeReset = 0f;
    private readonly float _minVert = -30f;
    private readonly float _maxVert = 30f;
    private NPC targetNPC = null;

    private int cutsceneNum = 0;

    private Animator anim = null;

    public Camera MainCamera { get; private set; }

    private void Awake()
    {
        mainFollowCamera.Follow = targetVector;

        _fsm = new FiniteStateMachine<CameraManager>(this);

        MainCamera = Camera.main;

        anim = FindObjectOfType<CinemachineStateDrivenCamera>().GetComponent<Animator>();
        targetVectorRB = targetVector.GetComponent<Rigidbody>(); // TEST

        RegisterEvents();
    }

    void Start() => _fsm.TransitionTo<PlayState>();

    private void Update()
    {
        _fsm.Update();
        _taskManager.Update();
    }

    //private void LateUpdate()
    private void FixedUpdate()
    {
        if (_fsm.PendingState != null) return;
        CameraState curGS = ((CameraState)_fsm.CurrentState);
        if (curGS != null)
            curGS.LateUpdate();
    }

    private void OnDestroy()
    {
        UnregisterEvents();
    }

    private void RegisterEvents()
    {
        Services.EventManager.Register<OnStartMenu>(_fsm.TransitionTo<StartMenuState>);
        Services.EventManager.Register<OnEnterPlay>(_fsm.TransitionTo<PlayState>);
        Services.EventManager.Register<OnEnterPlay>(EnterPlay);
        Services.EventManager.Register<OnPause>(_fsm.TransitionTo<PauseState>);
        Services.EventManager.Register<OnEnterMidCutscene>(_fsm.TransitionTo<MidCutsceneState>);
        Services.EventManager.Register<OnEnterEndCutscene>(_fsm.TransitionTo<EndCutsceneState>);
    }

    private void UnregisterEvents()
    {
        Services.EventManager.Unregister<OnStartMenu>(_fsm.TransitionTo<StartMenuState>);
        Services.EventManager.Unregister<OnEnterPlay>(_fsm.TransitionTo<PlayState>);
        Services.EventManager.Unregister<OnPause>(_fsm.TransitionTo<PauseState>);
        Services.EventManager.Unregister<OnEnterMidCutscene>(_fsm.TransitionTo<MidCutsceneState>);
        Services.EventManager.Unregister<OnEnterEndCutscene>(_fsm.TransitionTo<EndCutsceneState>);
    }
    
    private void EnterPlay(AGPEvent e)
    {
        _fsm.TransitionTo<PlayState>();
        SetTargetNPC(null);
    }
    
    public void EnterDialogue()
    {
        _fsm.TransitionTo<InDialogueState>();
        SetTargetNPC(NPCInteractionManager.ClosestNPC());
    }

    // Updates the camera movement inputs. Called in InputManager.
    public void InputUpdate(float mouseX, float mouseY)
    {
        if (_mouseX != mouseX || _mouseY != mouseY || Services.PlayerMovement.moving)
            _recenterTimeReset = Time.time;
        _mouseX = mouseX;
        _mouseY = mouseY;
    }

    public float CameraYAngle => targetVector.eulerAngles.y;

    private void ResetInputs()
    {
        _mouseX = 0;
        _mouseY = 0;
    }

    public Transform MainFollowCameraPos => mainFollowCamera.transform;

    // Must be called upon initiating dialogue with an NPC.
    public void SetTargetNPC(NPC newTarget) => targetNPC = newTarget;

    // The point of view of the player looking at the NPC.
    public void PlayerCameraView()
    {
        anim.Play(Str.ConvoPlayerPerspective);

        playerCameraView.ForceCameraPosition(playerConvoCameraPos.position, Quaternion.identity);
        // look at npc in question
        playerCameraView.LookAt = targetNPC.GetPlayerCameraLookAtPosition();
    }

    // The point of view of the NPC looking at the player.
    public void NPCCameraView()
    {
        anim.Play(Str.ConvoNPCPerspective);

        // set to proper position
        npcCameraView.ForceCameraPosition(targetNPC.npcCameraViewPosition.position, targetNPC.transform.rotation);
        // look at player
        npcCameraView.LookAt = npcCameraTarget;
    }

    public void EnterMidCutsceneFailsafe()
    {
        _fsm.TransitionTo<MidCutsceneState>();
    }

    public void EnterEndCutsceneFailsafe()
    {
        _fsm.TransitionTo<EndCutsceneState>();
    }


    #region States

    private abstract class CameraState : FiniteStateMachine<CameraManager>.State
    {
        public virtual void LateUpdate() { }
    }

    // Start menu state.
    private class StartMenuState : CameraState
    {
        public override void OnEnter()
        {
            Context.anim.Play(Str.Start);
        }

        public override void OnExit()
        {
            Context._curVertRot = Context.targetVector.eulerAngles.x - 360f; // Causes issues.
            Context._curHorRot = Context.targetVector.eulerAngles.y;
            Context.startCamera.Priority = 0;
        }
    }

    // Normal camera follow state.
    private class PlayState : CameraState
    {
        private const float _recenterCameraTime = 5f; // Set to -1 for controllers?
        private float _curRecenterCameraTime;
        private const float _recenterCameraSpeed = .5f;
        private const float _orbitCameraTime = 15f;
        private const float _orbitCameraSpeed = 10f;
        private readonly Vector3 _recenterTarget = new Vector3(5f, 0f, 0f);

        private float turningSmoothVel;

        public override void OnEnter()
        {
            Context.anim.Play(Str.PlayerFollow);
            Context._recenterTimeReset = Time.time;
        }

        public override void LateUpdate()
        {
            //if (InputManager.ControllerConnected)
            if (Time.time - Context._recenterTimeReset < _recenterCameraTime)
            {
                if (InputManager.UsingController())
                    ControllerControl();
                else
                    MouseControl();
            }
            else if (Time.time - Context._recenterTimeReset < _orbitCameraTime)
            {
                CenterBehindPlayer();
            }
            else
            {
                OrbitPlayer();
            }
        }

        private void MouseControl()
        {
            // Calculate new vertical rotation.
            Context._curVertRot -= Context._mouseY * Context.mouseSensitivity * Time.deltaTime;
            Context._curVertRot = Mathf.Clamp(Context._curVertRot, Context._minVert, Context._maxVert);

            // Calculate new horizontal rotation.
            Context._curHorRot += Context._mouseX * Context.mouseSensitivity * Time.deltaTime;

            // Calculate new rotate targetVector.
            Context.targetVector.eulerAngles = new Vector3(Context._curVertRot, Context._curHorRot, 0);
            //Context.targetVectorRB.MoveRotation(Quaternion.Euler(Context._curVertRot, Context._curHorRot, 0));
        }

        private void ControllerControl()
        {
            // Calculate new vertical rotation.
            Context._curVertRot -= Context._mouseY * Context.mouseSensitivity * 2 * Time.deltaTime;
            Context._curVertRot = Mathf.Clamp(Context._curVertRot, Context._minVert, Context._maxVert);

            // Calculate new horizontal rotation.
            Context._curHorRot += Context._mouseX * Context.mouseSensitivity * 2 * Time.deltaTime;

            // Calculate new rotate targetVector.
            Context.targetVector.eulerAngles = new Vector3(Context._curVertRot, Context._curHorRot, 0);
            //Context.targetVectorRB.MoveRotation(Quaternion.Euler(Context._curVertRot, Context._curHorRot, 0));
        }

        private void CenterBehindPlayer()
        {
            Quaternion rot = Quaternion.Slerp(Context.targetVector.localRotation, Quaternion.Euler(_recenterTarget), _recenterCameraSpeed * Time.deltaTime);
            Context.targetVector.localRotation = rot;
            Context._curVertRot = rot.eulerAngles.x;
            Context._curHorRot = rot.eulerAngles.y;
        }

        private void OrbitPlayer()
        {
            Vector3 rot = Context.targetVector.localEulerAngles;
            rot.y += _orbitCameraSpeed * Time.deltaTime;
            Context.targetVector.localEulerAngles = rot;
            Context._curVertRot = rot.x;
            Context._curHorRot = rot.y;
        }

        public override void OnExit()
        {
            base.OnExit();
            Context.ResetInputs();
        }

    }

    // Paused camera state.
    private class PauseState : CameraState
    {
        public override void OnEnter()
        {
            Context._mouseY = 0f;
            Context._mouseX = 0f;
        }
    }

    // Player is in dialogue. Transition to player dialogue camera 1 (looking from player to NPC)
    private class InDialogueState : CameraState
    {
        public override void OnEnter()
        {
            Context.PlayerCameraView();
            Logger.Warning("CameraManager entering dialogue");
        }
    }

    // Midroll cutscene state.
    private class MidCutsceneState : CameraState
    {

        private float elapsedTime = 0f;
        private float introDuration = 2f;

        public override void OnEnter() => DefineSequence();

        private void DefineSequence()
        {
            Context.cutsceneCamera.LookAt = Services.QuestItemRepository.CurrentQuestItem().transform;

            // will be independent.
            DelegateTask moveCameraBehindPlayer1 = new DelegateTask(
                () => {
                    elapsedTime = 0f;
                },
                () => {
                    elapsedTime += Time.deltaTime;
                    Context.targetVector.localRotation = Quaternion.Slerp(Context.targetVector.localRotation, Quaternion.Euler(Vector3.zero), elapsedTime / introDuration);
                    return elapsedTime > introDuration;
                },
                () => {
                    Context.targetVector.localEulerAngles = Vector3.zero;
                    //Context.anim.Play(Str.Cutscene);
                }
                );

            DelegateTask waitForPlayerInPlace = new DelegateTask(() => { },
                () => {
                    return Services.PlayerMovement.inPlaceForSequence;
                });

            DelegateTask cameraDolly1 = new DelegateTask(
                () =>
                {
                    Context.anim.Play(Str.CutsceneDolly1);
                    Context.cutsceneDollyCamera1.LookAt = Services.QuestItemRepository.CurrentQuestItem().transform;
                    elapsedTime = 0f;
                },
                () =>
                {
                    elapsedTime += Time.deltaTime;
                    return elapsedTime > 5.5f;
                }, () =>
                {
                }
                );

            WaitTask wait4secs = new WaitTask(1f);

            DelegateTask cameraDolly2 = new DelegateTask(
                () =>
                {
                    if (Context.cutsceneNum == 0)
                        Context.anim.Play(Str.CutsceneDolly3);
                    else if (Context.cutsceneNum == 1)
                        Context.anim.Play(Str.CutsceneDolly4); // MAKE DOLLY FOR SECOND CUTSCENE
                    else if (Context.cutsceneNum == 2)
                        Context.anim.Play(Str.CutsceneDolly2);
                    elapsedTime = 0f;
                },
                () =>
                {
                    elapsedTime += Time.deltaTime;
                    return elapsedTime > 11.5f;
                }, () =>
                {
                    Context.cutsceneNum++;
                }
                );

            //WaitTask waitFor6Seconds = new WaitTask(5.5f);

            DelegateTask moveCameraBehindPlayer2 = new DelegateTask(
                () => {
                    elapsedTime = 0f;
                    //Context.anim.Play(Str.PlayerFollow);
                    Context.targetVector.localEulerAngles = Vector3.zero;
                },
                () => {
                    elapsedTime += Time.deltaTime;
                    Context.targetVector.localRotation = Quaternion.Slerp(Context.targetVector.localRotation, Quaternion.Euler(Vector3.zero), elapsedTime / introDuration);
                    return elapsedTime > introDuration;
                },
                () => {
                    Context.targetVector.localEulerAngles = Vector3.zero;
                }
                );

            waitForPlayerInPlace.Then(cameraDolly1).Then(wait4secs).Then(cameraDolly2).Then(moveCameraBehindPlayer2);
            //moveCameraBehindPlayer1.Then(waitFor6Seconds).Then(moveCameraBehindPlayer2);

            Context._taskManager.Do(moveCameraBehindPlayer1);
            Context._taskManager.Do(waitForPlayerInPlace);
        }

        public override void OnExit()
        {
            Context._curVertRot = 0; // Causes issues.
            Context._curHorRot = Context.targetVector.eulerAngles.y;
            Context.targetVector.localEulerAngles = Vector3.zero;
            Context.ResetInputs();
        }
    }

    // End cutscene state.
    private class EndCutsceneState : CameraState
    {

        private float elapsedTime = 0f;

        public override void OnEnter() => DefineSequence();

        private void DefineSequence()
        {
            //Context.cutsceneCamera.LookAt = Services.QuestItemRepository.CurrentQuestItem().transform; // figure out what it's looking at

            Task wait2Secs = new WaitTask(2f);
            DelegateTask cameraDolly5 = new DelegateTask(
                () => {
                    Context.anim.Play(Str.CutsceneDolly5); // Play end cutscene animation
                    elapsedTime = 0f;
                },
                () =>
                {
                    elapsedTime += Time.deltaTime;
                    return elapsedTime > 15f;
                });

            wait2Secs.Then(cameraDolly5);
            Context._taskManager.Do(wait2Secs);
        }

        public override void OnExit()
        {
            Context._curVertRot = 0; // Causes issues.
            Context._curHorRot = Context.targetVector.eulerAngles.y;
            Context.targetVector.localEulerAngles = Vector3.zero;
            Context.ResetInputs();
        }
    }

    #endregion
}
