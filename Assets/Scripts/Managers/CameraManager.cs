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
    [SerializeField] private Transform targetVector;
    [SerializeField] private CinemachineVirtualCamera startCamera;
    [SerializeField] private CinemachineVirtualCamera mainFollowCamera;
    [SerializeField] private CinemachineVirtualCamera playerCameraView;
    [SerializeField] private CinemachineVirtualCamera npcCameraView;
    [SerializeField] private CinemachineVirtualCamera cutsceneCamera;

    // Ingame camera movement.
    private float _curVertRot = 0f;
    private float _curHorRot = 0f;
    private float _mouseY = 0f;
    private float _mouseX = 0f;
    private float _recenterTimeReset = 0f;
    private readonly float _minVert = -30f;
    private readonly float _maxVert = 30f;
    private NPC targetNPC = null;

    public Camera MainCamera { get; private set; }

    private void Awake()
    {
        mainFollowCamera.Follow = targetVector;

        _fsm = new FiniteStateMachine<CameraManager>(this);

        MainCamera = Camera.main;
    }

    void Start() => _fsm.TransitionTo<PlayState>();

    private void Update()
    {
        _fsm.Update();
        _taskManager.Update();
    }

    private void LateUpdate()
    {
        CameraState curGS = ((CameraState)_fsm.CurrentState);
        if (curGS != null)
            curGS.LateUpdate();
    }

    public void EnterStartMenu() => _fsm.TransitionTo<StartMenuState>();

    public void EnterPlay()
    {
        _fsm.TransitionTo<PlayState>();
        SetTargetNPC(null);
    }

    public void EnterPause() => _fsm.TransitionTo<PausedState>();

    public void EnterDialogue()
    {
        _fsm.TransitionTo<InDialogueState>();
        SetTargetNPC(Services.NPCInteractionManager.closestNPC);
    }

    public void EnterMidCutscene() => _fsm.TransitionTo<MidCutsceneState>();

    // Updates the camera movement inputs. Called in InputManager.
    public void InputUpdate(float mouseX, float mouseY)
    {
        if (_mouseX != mouseX || _mouseY != mouseY || Services.PlayerMovement.moving)
            _recenterTimeReset = Time.time;
        _mouseX = mouseX;
        _mouseY = mouseY;
    }

    public float CameraYAngle => targetVector.eulerAngles.y;

    public Transform MainFollowCameraPos => mainFollowCamera.transform;

    // Must be called upon initiating dialogue with an NPC.
    public void SetTargetNPC(NPC newTarget) => targetNPC = newTarget;

    // The point of view of the player looking at the NPC.
    public void PlayerCameraView()
    {
        mainFollowCamera.Priority = 10;
        playerCameraView.Priority = 30;
        npcCameraView.Priority = 20;

        // look at npc in question
        playerCameraView.LookAt = targetNPC.GetPlayerCameraLookAtPosition();
    }

    // The point of view of the NPC looking at the player.
    public void NPCCameraView()
    {
        mainFollowCamera.Priority = 10;
        playerCameraView.Priority = 20;
        npcCameraView.Priority = 30;

        // set to proper position
        npcCameraView.ForceCameraPosition(targetNPC.npcCameraViewPosition.position, targetNPC.transform.rotation);
        // look at player
        npcCameraView.LookAt = npcCameraTarget;
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
            Context.startCamera.Priority = 40;
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
        private readonly float _recenterCameraTime = 5f;
        private readonly float _recenterCameraSpeed = .5f;
        private readonly float _orbitCameraTime = 15f;
        private readonly float _orbitCameraSpeed = 20f;
        private readonly Vector3 _recenterTarget = new Vector3(5f, 0f, 0f);

        private float turningSmoothVel;

        public override void OnEnter()
        {
            Context.mainFollowCamera.Priority = 30;
            Context.playerCameraView.Priority = 20;
            Context.npcCameraView.Priority    = 10;
        }

        public override void LateUpdate()
        {
            if (Time.time - Context._recenterTimeReset < _recenterCameraTime)
            {
                // Calculate new vertical rotation.
                Context._curVertRot -= Context._mouseY * Context.mouseSensitivity * Time.deltaTime;
                Context._curVertRot = Mathf.Clamp(Context._curVertRot, Context._minVert, Context._maxVert);

                // Calculate new horizontal rotation.
                Context._curHorRot += Context._mouseX * Context.mouseSensitivity * Time.deltaTime;

                // Calculate new rotate targetVector.
                Context.targetVector.eulerAngles = new Vector3(Context._curVertRot, Context._curHorRot, 0);
            }
            else if (Time.time - Context._recenterTimeReset < _orbitCameraTime)
            {
                Quaternion rot = Quaternion.Slerp(Context.targetVector.localRotation, Quaternion.Euler(_recenterTarget), _recenterCameraSpeed * Time.deltaTime);
                Context.targetVector.localRotation = rot;
                Context._curVertRot = rot.eulerAngles.x;
                Context._curHorRot = rot.eulerAngles.y;
            }
            else
            {
                Vector3 rot = Context.targetVector.localEulerAngles;
                rot.y += _orbitCameraSpeed * Time.deltaTime;
                Context.targetVector.localEulerAngles = rot;
                Context._curVertRot = rot.x;
                Context._curHorRot = rot.y;
            }
        }
    }

    // Paused camera state.
    private class PausedState : CameraState
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
        public override void OnEnter() => Context.PlayerCameraView();
    }

    // Placeholder for cutscene state.
    private class MidCutsceneState : CameraState
    {

        private float elapsedTime = 0f;
        private float introDuration = 2f;

        public override void OnEnter() => Context._taskManager.Do(DefineSequence());

        private Task DefineSequence()
        {
            Context.cutsceneCamera.LookAt = Services.QuestItemRepository.currentQuestItem.transform;

            Context.cutsceneCamera.Priority = 50;
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
                    Context.cutsceneCamera.Priority = 50;
                }
                );

            WaitTask waitFor6Seconds = new WaitTask(5.5f);

            DelegateTask moveCameraBehindPlayer2 = new DelegateTask(
                () => {
                    elapsedTime = 0f;
                    Context.cutsceneCamera.Priority = 10;
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

            moveCameraBehindPlayer1.Then(waitFor6Seconds).Then(moveCameraBehindPlayer2);

            return moveCameraBehindPlayer1;
        }

        public override void OnExit()
        {
            Context._curVertRot = 0; // Causes issues.
            Context._curHorRot = Context.targetVector.eulerAngles.y;
            Context.targetVector.localEulerAngles = Vector3.zero;
        }
    }
    
    #endregion
}
