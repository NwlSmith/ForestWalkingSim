using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * Creator: Nate Smith
 * Creation Date: 3/2/2021
 * Description: Area entry quest trigger.
 * 
 * Triggers the next stage of a quest when the player enters an area, then marks itself as triggered.
 */
public class QuestAreaPlayerEnterTrigger : MonoBehaviour
{

    [SerializeField] private FSMQuest _questToTrigger;
    private bool _triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!_triggered && other.CompareTag("Player"))
        {
            _questToTrigger.AdvanceQuestStage();
            _triggered = true;
        }
    }
}
