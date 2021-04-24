using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurtleFinishLine : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Str.PlayerTag))
        {
            QuestManager.SetBoolMemoryVar("PlayerBeatTurtle");
            QuestManager.AdvanceQuest(Str.Turtle);
        }
    }
}
