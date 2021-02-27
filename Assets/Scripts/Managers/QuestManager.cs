using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * Creator: Nate Smith
 * Creation Date: 2/26/2021
 * Description: Manager class that controls logic across quests.
 */
public class QuestManager : MonoBehaviour
{
    [SerializeField] private Dictionary<string, FSMQuest> _questDictionary = new Dictionary<string, FSMQuest>();

    private void Start()
    {
        AddQuestsToDictionary();
        Services.DialogueController.DialogueRunner.AddCommandHandler("AdvanceQuest", AdvanceQuest);
    }

    private void AddQuestsToDictionary()
    {
        FSMQuest main = FindObjectOfType<MainQuest>();
        _questDictionary.Add("Main", main);

        FSMQuest warbler = FindObjectOfType<WarblerQuest>();
        _questDictionary.Add("Warbler", warbler);

        FSMQuest frog = FindObjectOfType<FrogQuest>();
        _questDictionary.Add("Frog", frog);

        FSMQuest turtle = FindObjectOfType<TurtleQuest>();
        _questDictionary.Add("Turtle", turtle);
    }

    public void AdvanceQuest(string[] parameters)
    {
        // Quest name is parameter 0
        if (parameters.Length <= 0)
        {
            Debug.LogWarning("Cannot advance quest because the Yarn Command contained no quest parameter string");
            return;
        }
        string key = parameters[0];
        if (!_questDictionary.ContainsKey(parameters[0]))
        {
            Debug.LogWarning($"Quest dictionary does not contain quest {key}");
            return;
        }
        _questDictionary[key].AdvanceQuestStage();
    }
}
