using System.Collections.Generic;
using UnityEngine;
/*
 * Creator: Nate Smith
 * Creation Date: 2/26/2021
 * Description: Manager class that controls logic across quests.
 */
public class QuestManager
{
    [SerializeField] private Dictionary<string, FSMQuest> _questDictionary = new Dictionary<string, FSMQuest>();
    [SerializeField] private Dictionary<string, string> _stringToStageDictionary = new Dictionary<string, string>();

    public QuestManager()
    {
        AddQuestsToDictionary();
        Services.DialogueController.DialogueRunner.AddCommandHandler("AdvanceQuest", AdvanceQuest);
    }

    private void AddQuestsToDictionary()
    {
        FSMQuest main = Object.FindObjectOfType<MainQuest>();
        _questDictionary.Add("Main", main);
        _stringToStageDictionary.Add("Main", "$quest_main_stage");

        FSMQuest warbler = Object.FindObjectOfType<WarblerQuest>();
        _questDictionary.Add("Warbler", warbler);
        _stringToStageDictionary.Add("Warbler", "$quest_warbler_stage");

        FSMQuest frog = Object.FindObjectOfType<FrogQuest>();
        _questDictionary.Add("Frog", frog);
        _stringToStageDictionary.Add("Frog", "$quest_frog_stage");

        FSMQuest turtle = Object.FindObjectOfType<TurtleQuest>();
        _questDictionary.Add("Turtle", turtle);
        _stringToStageDictionary.Add("Turtle", "$quest_turtle_stage");
    }

    public void AdvanceQuest(string[] parameters)
    {
        // Quest name is parameter 0
        if (parameters.Length <= 0)
        {
            Logger.Warning("Cannot advance quest because the Yarn Command contained no quest parameter string");
            return;
        }
        string key = parameters[0];
        if (!_questDictionary.ContainsKey(parameters[0]))
        {
            Logger.Warning($"Quest dictionary does not contain quest {key}");
            return;
        }
        _questDictionary[key].AdvanceQuestStage();
        AdvanceQuestMemoryVar(key);
    }

    public void AdvanceQuest(string questName)
    {
        string[] param = new string[1];
        param[0] = questName;
        AdvanceQuest(param);
    }

    /*
     * Keys are "Main", "Warbler", "Frog", "Turtle", not "$quest_main_stage"
     */
    public void AdvanceQuestMemoryVar(string key) => Services.DialogueController.InMemoryVariableStorage.SetValue(_stringToStageDictionary[key], _questDictionary[key].QuestStage);
}
