using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * Creator: Nate Smith
 * Creation Date: 3/1/2021
 * Description: Multi-NPC class script.
 * 
 * Holds variables and functions for a conversation between multiple NPCs.
 * 
 * Acts as 1 NPC, but really it acts as multiple.
 */
public class MultiNPC : NPC
{
    public NPC[] npcs;

    public override void EnterDialogue(Transform playerPos)
    {
        foreach (NPC npc in npcs)
            npc.EnterDialogue(playerPos); // Maybe don't need to rotate the characters toward player?
    }

    public override void ExitDialogue()
    {
        foreach (NPC npc in npcs)
            npc.ExitDialogue();
    }

    public override NPCSpeakerData GetNPCSpeakerData(int npcNum = 0)
    {
        return npcs[npcNum].GetNPCSpeakerData();
    }

    public override Transform GetPlayerCameraLookAtPosition(int num = 0)
    {
        return npcs[num].GetPlayerCameraLookAtPosition();
    }
}
