using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
/*
 * Creator: Nate Smith
 * Creation Date: 3/4/2021
 * Description: Manages saving and loading data on game start and finish.
 * 
 * Needed data:
 * - Quest stages
 * - Player position
 * - Player holding something?
 * - Other holdable object positions
 */
public class SaveManager
{
    #region Const Quest Strings.
    private const string questStageMain = "Main";
    private const string questStageWarbler = "Warbler";
    private const string questStageFrog = "Frog";
    private const string questStageTurtle = "Turtle";
    private const string child1String = "$found_warbler_child_1";
    private const string child2String = "$found_warbler_child_2";
    private const string child3String = "$found_warbler_child_3";
    #endregion

    #region Quest and Item GameObjects.
    private MainQuest mainQuest;
    private WarblerQuest warblerQuest;
    private FrogQuest frogQuest;
    private TurtleQuest turtleQuest;
    private QuestItem seedItem;
    private QuestItem soilItem;
    private QuestItem rainItem;
    #endregion

    #region Serialized Quest Data.
    [System.Serializable]
    public class Data
    {
        public QuestStageData[] questStageData;
        public PlayerData playerData;
        public PlayerHolding playerHolding;
        public WarblerChildrenStatus warblerChildrenStatus;
    }

    [System.Serializable]
    public class QuestStageData
    {
        public string quest;
        public int stage;
    }

    [System.Serializable]
    public class PlayerData
    {
        public Vector3 position;
        public Quaternion rotation;
    }

    [System.Serializable]
    public class PlayerHolding
    {
        public QuestItem.QuestItemEnum itemHolding;
    }

    [System.Serializable]
    public class WarblerChildrenStatus
    {
        public bool foundChild1;
        public bool foundChild2;
        public bool foundChild3;
    }
    #endregion


    #region Lifecycle Management.
    public SaveManager()
    {
        mainQuest = Object.FindObjectOfType<MainQuest>();
        warblerQuest = Object.FindObjectOfType<WarblerQuest>();
        frogQuest = Object.FindObjectOfType<FrogQuest>();
        turtleQuest = Object.FindObjectOfType<TurtleQuest>();

        QuestItem[] questItems = Object.FindObjectsOfType<QuestItem>();

        if (questItems.Length != 3)
        {
            Debug.LogWarning($"Error retrieving questItems. {questItems.Length} were found.");
        }
        else
        {
            foreach (QuestItem questItem in questItems)
            {
                switch (questItem.itemEnum)
                {
                    case QuestItem.QuestItemEnum.Seed:
                        seedItem = questItem;
                        break;
                    case QuestItem.QuestItemEnum.Soil:
                        soilItem = questItem;
                        break;
                    case QuestItem.QuestItemEnum.Rain:
                        rainItem = questItem;
                        break;
                    case QuestItem.QuestItemEnum.None:
                        break;
                }
            }
        }

        if (!File.Exists(Application.dataPath + "/save_default.json"))
            CreateDefaultSave();
    }
    #endregion

    #region Functions.

    public void CreateDefaultSave()
    {
        QuestStageData[] questStagesArray =
        {
            new QuestStageData{quest = questStageMain, stage = 0},
            new QuestStageData{quest = questStageWarbler, stage = 0},
            new QuestStageData{quest = questStageFrog, stage = 0},
            new QuestStageData{quest = questStageTurtle, stage = 0}
        };

        PlayerData playerData = new PlayerData
        {
            position = new Vector3(37.5f, 0.8299999237060547f, 0f),
            rotation = new Quaternion(0f, 0f, 0f, 1f)
        };

        PlayerHolding playerHolding = new PlayerHolding { itemHolding = QuestItem.QuestItemEnum.None };

        WarblerChildrenStatus warblerChildrenStatus = new WarblerChildrenStatus { foundChild1 = false, foundChild2 = false, foundChild3 = false };

        Data saveData = new Data
        {
            questStageData = questStagesArray,
            playerData = playerData,
            playerHolding = playerHolding,
            warblerChildrenStatus = warblerChildrenStatus
        };

        string saveDataJson = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(Application.dataPath + "/save_default.json", saveDataJson);
    }

    public bool SaveExists()
    {
        if (!File.Exists(Application.dataPath + "/save.json")) return false;
        string defaultSaveString = File.ReadAllText(Application.dataPath + "/save_default.json");
        string saveString = File.ReadAllText(Application.dataPath + "/save.json");
        return !defaultSaveString.Equals(saveString); // If they are equal, then there is no distinct save file.
    }

    public void NewGameSave()
    {
        string saveString = File.ReadAllText(Application.dataPath + "/save_default.json");
        File.WriteAllText(Application.dataPath + "/save.json", saveString);
    }

    public void SaveData()
    {

        QuestStageData[] questStagesArray =
        {
            new QuestStageData{quest = questStageMain, stage = mainQuest.QuestStage},
            new QuestStageData{quest = questStageWarbler, stage = warblerQuest.QuestStage},
            new QuestStageData{quest = questStageFrog, stage = frogQuest.QuestStage},
            new QuestStageData{quest = questStageTurtle, stage = turtleQuest.QuestStage}
        };

        PlayerData playerData = new PlayerData
        {
            position = Services.PlayerMovement.transform.position,
            rotation = Services.PlayerMovement.transform.rotation
        };
        
        PlayerHolding playerHolding = new PlayerHolding {
            itemHolding = Services.PlayerItemHolder._holdingItem ?
            Services.PlayerItemHolder._currentlyHeldItem.GetComponent<QuestItem>().itemEnum :
            QuestItem.QuestItemEnum.None
        };


        Yarn.Unity.InMemoryVariableStorage questMemory = Services.DialogueController.InMemoryVariableStorage;

        WarblerChildrenStatus warblerChildrenStatus = new WarblerChildrenStatus {
            foundChild1 = questMemory.GetValue(child1String).AsBool,
            foundChild2 = questMemory.GetValue(child2String).AsBool,
            foundChild3 = questMemory.GetValue(child3String).AsBool
        };

        Data saveData = new Data {
            questStageData = questStagesArray,
            playerData = playerData,
            playerHolding = playerHolding,
            warblerChildrenStatus = warblerChildrenStatus
        };

        string saveDataJson = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(Application.dataPath + "/save.json", saveDataJson);
    }

    public IEnumerator LoadDataCO()
    {
        string saveString = File.ReadAllText(Application.dataPath + "/save.json");
        Data data = JsonUtility.FromJson<Data>(saveString);

        Yarn.Unity.InMemoryVariableStorage questMemory = Services.DialogueController.InMemoryVariableStorage;
        questMemory.SetValue(child1String, data.warblerChildrenStatus.foundChild1);
        questMemory.SetValue(child2String, data.warblerChildrenStatus.foundChild2);
        questMemory.SetValue(child3String, data.warblerChildrenStatus.foundChild3);

        if (data.playerHolding.itemHolding != QuestItem.QuestItemEnum.None)
        {
            QuestItem holding = null;
            switch (data.playerHolding.itemHolding)
            {
                case QuestItem.QuestItemEnum.Seed:
                    holding = seedItem;
                    break;
                case QuestItem.QuestItemEnum.Soil:
                    holding = soilItem;
                    break;
                case QuestItem.QuestItemEnum.Rain:
                    holding = rainItem;
                    break;
                case QuestItem.QuestItemEnum.None:
                    Debug.LogWarning("Thought to be holding quest item, holding None quest item");
                    break;
            }
            Services.PlayerItemHolder.AttachToTransform(holding);
        }
        
        Services.PlayerMovement.ForceTransform(data.playerData.position, data.playerData.rotation);

        foreach (QuestStageData questStageData in data.questStageData)
        {
            for (int i = 0; i < questStageData.stage; i++)
            {
                Services.QuestManager.AdvanceQuest(questStageData.quest);
                yield return null;
            }
        }

        // Move item to player holder.
    }
    #endregion
}
