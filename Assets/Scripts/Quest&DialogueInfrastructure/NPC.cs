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
    private Animator _anim;

    private void Awake()
    {
        _initRot = transform.rotation;
        _anim = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        if (GetComponent<MultiNPC>() || !GetComponentInParent<MultiNPC>())
            Services.DialogueController.AddYarnDialogue(YarnDialogue);
    }

    public virtual void EnterDialogue(Transform playerPos)
    {
        Vector3 lookPos = playerPos.position - transform.position;
        lookPos.y = 0;
        transform.rotation = Quaternion.LookRotation(lookPos);
        _anim.SetBool("InConversation", true);
    }

    public virtual void ExitDialogue()
    {
        transform.rotation = _initRot;
        _anim.SetBool("InConversation", false);
    }

    public virtual void Speak(int npcNum = 0)
    {
        _anim.SetTrigger("Talk");
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
