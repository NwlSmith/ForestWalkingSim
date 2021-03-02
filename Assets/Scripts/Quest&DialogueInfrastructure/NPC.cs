using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * Creator: Nate Smith
 * Creation Date: 2/15/2021
 * Description: NPC class script.
 * 
 * Holds variables and functions for an instance of an NPC.
 * 
 * Maybe make NPC face toward player on dialogue entry?
 */
public class NPC : MonoBehaviour
{


    public string YarnStartNode;
    public YarnProgram YarnDialogue;
    public NPCSpeakerData NPCSpeakerData;

    public Transform npcCameraViewPosition;// { get; private set; }
    [SerializeField] protected Transform playerCameraLookAtPosition;// { get; private set; }

    private Quaternion _initRot;

    private void Awake()
    {
        _initRot = transform.rotation;

    }

    private void Start()
    {
        if (GetComponent<MultiNPC>() || !GetComponentInParent<MultiNPC>())
            Services.DialogueController.AddYarnDialogue(YarnDialogue);
    }

    public virtual void EnterDialogue(Transform playerPos)
    {
        transform.LookAt(playerPos, Vector3.up); // Change to a lerp
    }

    public virtual void ExitDialogue()
    {
        transform.rotation = _initRot;
    }

    public virtual NPCSpeakerData GetNPCSpeakerData(int npcNum = 0)
    {
        return NPCSpeakerData;
    }

    public virtual Transform GetPlayerCameraLookAtPosition(int num = 0)
    {
        return playerCameraLookAtPosition;
    }
}
