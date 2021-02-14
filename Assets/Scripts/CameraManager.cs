using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{

    // The finite state machine of the current gamestate.
    private FiniteStateMachine<CameraManager> _fsm;

    public float mouseSensitivity = 100f;
    
    [SerializeField] private Transform targetVector;
    private Cinemachine.CinemachineVirtualCamera mainFollowCamera;
    private float _curVertRot = 0f;
    private float _curHorRot = 0f;

    private float _mouseY = 0f;
    private float _mouseX = 0f;

    private readonly float _minVert = -30f;
    private readonly float _maxVert = 30f;

    private void Awake()
    {
        Cinemachine.CinemachineVirtualCamera cam = GetComponentInChildren<Cinemachine.CinemachineVirtualCamera>();
        cam.Follow = targetVector;

        _fsm = new FiniteStateMachine<CameraManager>(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        _fsm.TransitionTo<PlayState>();
    }

    private void LateUpdate()
    {
        CameraState curGS = ((CameraState)_fsm.CurrentState);
        if (curGS != null)
            curGS.LateUpdate();
    }

    public void EnterDialogue()
    {
        _fsm.TransitionTo<InDialogueState>();
    }

    // Updates the camera movement inputs. Called in InputManager.
    public void InputUpdate(float mouseX, float mouseY)
    {
        _mouseX = mouseX;
        _mouseY = mouseY;
    }

    public float CameraYAngle()
    {
        return targetVector.eulerAngles.y;
    }


    #region States

    private abstract class CameraState : FiniteStateMachine<CameraManager>.State
    {
        public virtual void LateUpdate() { }
    }

    // Normal camera follow state.
    private class PlayState : CameraState
    {
        public override void OnEnter() { }

        public override void Update() { base.Update(); }

        public override void LateUpdate()
        {
            base.LateUpdate();

            // Calculate new vertical rotation.
            Context._curVertRot -= Context._mouseY * Context.mouseSensitivity * Time.deltaTime;
            Context._curVertRot = Mathf.Clamp(Context._curVertRot, Context._minVert, Context._maxVert);

            // Calculate new horizontal rotation.
            Context._curHorRot += Context._mouseX * Context.mouseSensitivity * Time.deltaTime;

            // Calculate new rotate targetVector.
            Context.targetVector.eulerAngles = new Vector3(Context._curVertRot, Context._curHorRot, 0);

        }

        public override void OnExit() { }
    }

    // Player is in dialogue. Transition to player dialogue camera 1 (looking from player to NPC
    private class InDialogueState : CameraState
    {
        public override void OnEnter() { PlayerCameraView(); }

        public override void Update() { base.Update(); }

        public override void OnExit() { }

        // The point of view of the player looking at the NPC.
        public void PlayerCameraView() { }

        // The point of view of the NPC looking at the player.
        public void NPCCameraView() { }
    }

    // Placeholder for cutscene state.
    private class CutsceneState : CameraState
    {
        public override void OnEnter() { }

        public override void Update() { base.Update(); }

        public override void OnExit() { }
    }
    
    #endregion
}
