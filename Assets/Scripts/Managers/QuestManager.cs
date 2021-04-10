using System.Collections.Generic;
using UnityEngine;
/*
 * Creator: Nate Smith
 * Creation Date: 2/26/2021
 * Description: Manager class that controls logic across quests.
 */
public static class QuestManager
{
    [SerializeField] private static Dictionary<string, FSMQuest> _questDictionary = new Dictionary<string, FSMQuest>();
    [SerializeField] private static Dictionary<string, string> _stringToYarnVarDictionary = new Dictionary<string, string>();

    static QuestManager()
    {
        AddQuestsToDictionary();
    }

    private static void AddQuestsToDictionary()
    {
        FSMQuest main = Object.FindObjectOfType<MainQuest>();
        _questDictionary.Add(Str.Main, main);
        _stringToYarnVarDictionary.Add(Str.Main, "$quest_main_stage");

        FSMQuest warbler = Object.FindObjectOfType<WarblerQuest>();
        _questDictionary.Add(Str.Warbler, warbler);
        _stringToYarnVarDictionary.Add(Str.Warbler, "$quest_warbler_stage");

        FSMQuest frog = Object.FindObjectOfType<FrogQuest>();
        _questDictionary.Add(Str.Frog, frog);
        _stringToYarnVarDictionary.Add(Str.Frog, "$quest_frog_stage");

        FSMQuest turtle = Object.FindObjectOfType<TurtleQuest>();
        _questDictionary.Add(Str.Turtle, turtle);
        _stringToYarnVarDictionary.Add(Str.Turtle, "$quest_turtle_stage");

        _stringToYarnVarDictionary.Add(Str.Seed, Str.SeedString);
        _stringToYarnVarDictionary.Add(Str.Soil, Str.SoilString);
        _stringToYarnVarDictionary.Add(Str.Rain, Str.RainString);
    }

    public static void AdvanceQuest(string[] parameters)
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

    public static void AdvanceQuest(string questName)
    {
        string[] param = new string[1];
        param[0] = questName;
        AdvanceQuest(param);
    }

    /*
     * Keys are Str.Main, Str.Warbler, Str.Frog, Str.Turtle, not "$quest_main_stage"
     */
    public static void AdvanceQuestMemoryVar(string key) => Services.DialogueController.InMemoryVariableStorage.SetValue(_stringToYarnVarDictionary[key], _questDictionary[key].QuestStage);

    public static void FoundItemQuestMemoryVar(string key) => Services.DialogueController.InMemoryVariableStorage.SetValue(_stringToYarnVarDictionary[key], true);
}
