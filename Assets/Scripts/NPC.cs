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

    public Transform npcCameraViewPosition;// { get; private set; }

    private Quaternion _initRot;

    private void Awake()
    {
        _initRot = transform.rotation;
    }

    public void EnterDialogue(Transform playerPos)
    {
        transform.LookAt(playerPos, Vector3.up);
    }

    public void ExitDialogue()
    {
        transform.rotation = _initRot;
    }
}
