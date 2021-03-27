using UnityEngine;
/*
 * Creator: Nate Smith
 * Creation Date: 3/2/2021
 * Description: Area quest trigger.
 * 
 * Triggers the next stage of a quest when the player enters/leaves an area, then marks itself as triggered.
 */
public class QuestAreaPlayerTrigger : MonoBehaviour
{

    [SerializeField] private bool _enter = false;
    [SerializeField] private FSMQuest _questToTrigger;
    private bool _triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (_enter && !_triggered && other.CompareTag(Services.PlayerTag))
        {
            Services.QuestManager.AdvanceQuest(_questToTrigger.QuestTag);
            _triggered = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!_enter && !_triggered && other.CompareTag(Services.PlayerTag))
        {
            Services.QuestManager.AdvanceQuest(_questToTrigger.QuestTag);
            _triggered = true;
        }
    }
}
