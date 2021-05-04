using UnityEngine;
/*
 * Creator: Nate Smith
 * Creation Date: 2/19/2021
 * Description: Manager that interfaces between the player and NPCs for dialogue.
 */
public static class NPCInteractionManager
{
    private const float _closestDistForNPCEncounter = 50f;
    private static NPC closestNPC = null;
    private static readonly Transform _transform;
    private static readonly NPC[] _npcs;
    private static float _timeLeftNPC = 0;
    private static float _npcCooldown = 1f;
    private static bool enteringDialogueFailsafe = false;

    static NPCInteractionManager()
    {
        _transform = Object.FindObjectOfType<PlayerMovement>().transform;
        _npcs = Object.FindObjectsOfType<NPC>();
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
        if (enteringDialogueFailsafe || closestNPC == null || Time.time < _timeLeftNPC + _npcCooldown)
            return;
        else if (Vector3.Distance(_transform.position, closestNPC.transform.position) > _closestDistForNPCEncounter)
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

        foreach (NPC npc in _npcs)
        {
            float curDist = Vector3.Distance(_transform.position, npc.transform.position);
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

    public static void EnterDialogue()
    {
        enteringDialogueFailsafe = true;
        if (closestNPC == null)
            FindClosestNPC();
        closestNPC.EnterDialogue(Services.PlayerMovement.transform);
    }

    public static void ExitDialogue()
    {
        enteringDialogueFailsafe = false;
        _timeLeftNPC = Time.time;
        closestNPC?.ExitDialogue();
    }

    public static NPC ClosestNPC()
    {
        if (closestNPC == null)
            FindClosestNPC();
        return closestNPC;
    }

    public static bool CanEnterConversation => closestNPC != null;
}
