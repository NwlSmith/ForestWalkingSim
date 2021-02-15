using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        Services.NPCInteractionManager.closestNPC = null;
        Services.UIManager.HideDialogueEnterPrompt();
    }
}
