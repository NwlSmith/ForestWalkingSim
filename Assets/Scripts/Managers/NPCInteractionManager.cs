using UnityEngine;
/*
 * Creator: Nate Smith
 * Creation Date: 2/19/2021
 * Description: Manager that interfaces between the player and NPCs for dialogue.
 */
public static class NPCInteractionManager
{
    private const float closestDistForNPCEncounter = 50f;
    public static NPC closestNPC = null;
    private static readonly Transform transform;
    private static readonly NPC[] npcs;

    static NPCInteractionManager()
    {
        transform = Object.FindObjectOfType<PlayerMovement>().transform;
        npcs = Object.FindObjectsOfType<NPC>();
    }

    public static void PlayerEncounteredNPC(NPC npc)
    {
        closestNPC = npc;
        Services.UIManager.DisplayDialogueEnterPrompt();
    }

    public static void PlayerLeftNPC()
    {
        closestNPC = null;
        Services.UIManager.HideDialogueEnterPrompt();
    }

    public static void InputPressed()
    {
        if (closestNPC == null)
            return;
        else if (Vector3.Distance(transform.position, closestNPC.transform.position) > closestDistForNPCEncounter)
        {
            PlayerLeftNPC();
            return;
        }
        // Enter Dialogue
        Services.GameManager.EnterDialogue();
        
    }

    public static void FindClosestNPC()
    {
        Logger.Warning("Failsafe: Someone called FindClosestNPC() on NPCInteractionManager");
        float closestDist = 75f;
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
        if (closeNPC == null)
            return;
        PlayerEncounteredNPC(closeNPC);
        Logger.Warning($"ClosestNPC = {closeNPC.name}");
    }

    public static Transform DialogueTrans => closestNPC.dialoguePos;

    public static void EnterDialogue() => closestNPC.EnterDialogue(Services.PlayerMovement.transform);

    public static void ExitDialogue() => closestNPC?.ExitDialogue();
}
