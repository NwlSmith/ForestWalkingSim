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
        Logger.Debug("Displaying entry prompt from NPCInteractionManager");
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

    public void FindClosestNPC()
    {
        Logger.Warning("Failsafe: Someone called FindClosestNPC() on NPCInteractionManager");
        NPC[] npcs = FindObjectsOfType<NPC>();
        float closestDist = 10000f;
        NPC closeNPC = null;

        foreach (NPC npc in npcs)
        {
            float curDist = Vector3.Distance(transform.position, npc.transform.position);
            if (curDist < closestDist)
            {
                closestDist = curDist;
                MultiNPC multi = npc.GetComponentInParent<MultiNPC>();
                if (multi != null)
                {
                    closeNPC = multi;
                }
                else
                {
                    closeNPC = npc;
                }
            }
        }

        PlayerEncounteredNPC(closeNPC);
        Logger.Warning($"ClosestNPC = {closeNPC.name}");
    }

    public Transform DialogueTrans => closestNPC.dialoguePos;

    public void EnterDialogue() => closestNPC.EnterDialogue(Services.PlayerMovement.transform);

    public void ExitDialogue() => closestNPC?.ExitDialogue();
}
