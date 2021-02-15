using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCInteractionManager : MonoBehaviour
{

    public NPC closestNPC = null;

    public void PlayerEncounteredNPC(NPC npc)
    {
        closestNPC = npc;
        Services.UIManager.DisplayDialogueEnterPrompt();
    }

    public void PlayerLeftNPC()
    {
        closestNPC = null;
        Services.UIManager.HideDialogueEnterPrompt();
    }

    public void InputPressed()
    {
        if (closestNPC != null)
        {
            // Enter Dialogue
            Services.GameManager.EnterDialogue();
        }
    }
}
