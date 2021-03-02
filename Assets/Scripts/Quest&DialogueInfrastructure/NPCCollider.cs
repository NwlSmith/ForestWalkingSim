using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * Creator: Nate Smith
 * Creation Date: 2/15/2021
 * Description: Collider for entering dialogue with an NPC.
 * 
 * Can be placed away from the NPC.
 */
public class NPCCollider : MonoBehaviour
{
    private NPC parentNPC;

    private void Awake()
    {
        parentNPC = GetComponentInParent<NPC>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Services.NPCInteractionManager.PlayerEncounteredNPC(parentNPC);
        Services.UIManager.DisplayDialogueEnterPrompt();
    }

    private void OnTriggerExit(Collider other)
    {
        Services.NPCInteractionManager.PlayerLeftNPC();
        Services.UIManager.HideDialogueEnterPrompt();
    }
}
