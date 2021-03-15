using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * Creator: Nate Smith
 * Creation Date: 2/19/2021
 * Description: Manager that interfaces between the player and NPCs for dialogue.
 */
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

    public Transform DialogueTrans()
    {
        return closestNPC.dialoguePos;
    }

    public void EnterDialogue()
    {
        closestNPC.EnterDialogue(Services.PlayerMovement.transform);
    }

    public void ExitDialogue()
    {
        closestNPC?.ExitDialogue();
    }
}
