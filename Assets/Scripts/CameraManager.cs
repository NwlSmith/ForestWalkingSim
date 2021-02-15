using System.Collections;
using System.Collections.Generic;
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
 * 
 */
public class CameraManager : MonoBehaviour
{

    // The finite state machine of the current CameraState.
    private FiniteStateMachine<CameraManager> _fsm;

    public float mouseSensitivity = 200f;
    
    [SerializeField] private Transform targetVector;
    [SerializeField] private CinemachineVirtualCamera mainFollowCamera;
    [SerializeField] private CinemachineVirtualCamera playerCameraView;
    [SerializeField] private CinemachineVirtualCamera npcCameraView;

    // Ingame camera movement.
    private float _curVertRot = 0f;
    private float _curHorRot = 0f;
    private float _mouseY = 0f;
    private float _mouseX = 0f;
    private readonly float _minVert = -30f;
    private readonly float _maxVert = 30f;

    private NPC targetNPC = null;

    private void Awake()
    {
        mainFollowCamera.Follow = targetVector;

        _fsm = new FiniteStateMachine<CameraManager>(this);
    }

    void Start()
    {
        _fsm.TransitionTo<PlayState>();
    }

    private void Update()
    {
        _fsm.Update();
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
        SetTargetNPC(Services.NPCInteractionManager.closestNPC);
    }

    public void EnterPlay()
    {
        _fsm.TransitionTo<PlayState>();
        SetTargetNPC(null);
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

    // Must be called upon initiating dialogue with an NPC.
    public void SetTargetNPC(NPC newTarget)
    {
        targetNPC = newTarget;
    }


    #region States

    private abstract class CameraState : FiniteStateMachine<CameraManager>.State
    {
        public virtual void LateUpdate() { }
    }

    // Normal camera follow state.
    private class PlayState : CameraState
    {
        public override void OnEnter()
        {
            Context.mainFollowCamera.Priority = 30;
            Context.playerCameraView.Priority = 20;
            Context.npcCameraView.Priority = 10;
        }

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

    // Player is in dialogue. Transition to player dialogue camera 1 (looking from player to NPC)
    private class InDialogueState : CameraState
    {
        public override void OnEnter() { PlayerCameraView(); }

        public override void Update() { base.Update();
            if (Input.GetKeyDown(KeyCode.F)) // DEBUG, REMOVE THIS!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            {
                if (Context.playerCameraView.Priority == 30)
                    NPCCameraView();
                else
                    PlayerCameraView();
            }
        }

        public override void OnExit() { }

        // The point of view of the player looking at the NPC.
        public void PlayerCameraView()
        {
            Context.mainFollowCamera.Priority = 10;
            Context.playerCameraView.Priority = 30;
            Context.npcCameraView.Priority = 20;

            // look at npc in question

            Context.playerCameraView.LookAt = Context.targetNPC.transform;
        }

        // The point of view of the NPC looking at the player.
        public void NPCCameraView()
        {
            Context.mainFollowCamera.Priority = 10;
            Context.playerCameraView.Priority = 20;
            Context.npcCameraView.Priority = 30;

            // set to proper position
            Context.npcCameraView.ForceCameraPosition(Context.targetNPC.transform.position, Context.targetNPC.transform.rotation);
            // look at player
            Context.playerCameraView.LookAt = Context.transform;
        }
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
