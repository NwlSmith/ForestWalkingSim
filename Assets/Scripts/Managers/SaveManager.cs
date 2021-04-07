using System.Collections;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
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
    #region Const Strings.
    private const string saveName = "/cur.save";
    private const string saveDefaultName = "/default.save";
    private const string questStageMain = "Main";
    private const string questStageWarbler = "Warbler";
    private const string questStageFrog = "Frog";
    private const string questStageTurtle = "Turtle";
    private const string child1String = "$found_warbler_child_1";
    private const string child2String = "$found_warbler_child_2";
    private const string child3String = "$found_warbler_child_3";
    private const string seedString = "$found_seed";
    private const string soilString = "$found_soil";
    private const string rainString = "$found_rain";
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

    #region Serialized Game State Data.
    [System.Serializable]
    public class Data
    {
        public QuestStageData[] questStageData;
        public PlayerData playerData;
        public PlayerHolding playerHolding;
        public WarblerChildrenStatus warblerChildrenStatus;
        public ItemStatus itemStatus;
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
        public float[] position;
        public float[] rotation;
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

    [System.Serializable]
    public class ItemStatus
    {
        public bool foundSeed;
        public bool foundSoil;
        public bool foundRain;
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
            Logger.Warning($"Error retrieving questItems. {questItems.Length} were found.");
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

        if (!File.Exists(Application.dataPath + saveDefaultName))
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
            position = ToArray(new Vector3(369.7f, 32.45f, 411.16f)),
            rotation = ToArray(new Quaternion(0f, 0f, 0f, 1f))
        };

        PlayerHolding playerHolding = new PlayerHolding { itemHolding = QuestItem.QuestItemEnum.None };

        WarblerChildrenStatus warblerChildrenStatus = new WarblerChildrenStatus { foundChild1 = false, foundChild2 = false, foundChild3 = false };

        ItemStatus itemStatus = new ItemStatus { foundSeed = false, foundSoil = false, foundRain = false };

        Data saveData = new Data
        {
            questStageData = questStagesArray,
            playerData = playerData,
            playerHolding = playerHolding,
            warblerChildrenStatus = warblerChildrenStatus,
            itemStatus = itemStatus
        };

        SerializeData(saveDefaultName, saveData);
        //SerializeJson(saveDefaultName, saveData);
    }

    public bool SaveExists()
    {
        if (!File.Exists(Application.dataPath + saveName)) return false;
        string defaultSaveString = File.ReadAllText(Application.dataPath + saveDefaultName);
        string saveString = File.ReadAllText(Application.dataPath + saveName);
        return !defaultSaveString.Equals(saveString); // If they are equal, then there is no distinct save file.
    }

    public void NewGameSave() => SerializeData(saveName, DeserializeData(saveDefaultName));

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
            position = ToArray(Services.PlayerMovement.transform.position),
            rotation = ToArray(Services.PlayerMovement.transform.rotation)
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

        ItemStatus itemStatus = new ItemStatus {
            foundSeed = questMemory.GetValue(seedString).AsBool,
            foundSoil = questMemory.GetValue(soilString).AsBool,
            foundRain = questMemory.GetValue(rainString).AsBool
        };

        Data saveData = new Data {
            questStageData = questStagesArray,
            playerData = playerData,
            playerHolding = playerHolding,
            warblerChildrenStatus = warblerChildrenStatus,
            itemStatus = itemStatus
        };

        SerializeData(saveName, saveData);

        //SerializeJson(saveName, saveData);
    }

    public IEnumerator LoadDataCO()
    {
        //DeserializeJson(saveName);
        Data data = DeserializeData(saveName);

        Yarn.Unity.InMemoryVariableStorage questMemory = Services.DialogueController.InMemoryVariableStorage;
        questMemory.SetValue(child1String, data.warblerChildrenStatus.foundChild1);
        questMemory.SetValue(child2String, data.warblerChildrenStatus.foundChild2);
        questMemory.SetValue(child3String, data.warblerChildrenStatus.foundChild3);

        questMemory.SetValue(seedString, data.itemStatus.foundSeed);
        questMemory.SetValue(soilString, data.itemStatus.foundSoil);
        questMemory.SetValue(rainString, data.itemStatus.foundRain);

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
                    Logger.Warning("Thought to be holding quest item, holding None quest item");
                    break;
            }
            Services.PlayerItemHolder.AttachToTransform(holding);
        }
        
        Services.PlayerMovement.ForceTransform(ToVec3(data.playerData.position), ToQuat(data.playerData.rotation));

        foreach (QuestStageData questStageData in data.questStageData)
        {
            for (int i = 0; i < questStageData.stage; i++)
            {
                Services.QuestManager.AdvanceQuest(questStageData.quest);
                yield return null;
            }
        }
        
    }

    #endregion

    #region Utilities.

    private void SerializeJson(string fileName, Data data) => File.WriteAllText(Application.dataPath + fileName, JsonUtility.ToJson(data, false));

    private Data DeserializeJson(string fileName) => JsonUtility.FromJson<Data>(File.ReadAllText(Application.dataPath + fileName));

    private void SerializeData(string fileName, Data data)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream fs = new FileStream(Application.dataPath + fileName, FileMode.Create);
        bf.Serialize(fs, data);
        fs.Close();
    }

    private Data DeserializeData(string fileName)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream fs = new FileStream(Application.dataPath + fileName, FileMode.Open);
        Data newData = (Data)bf.Deserialize(fs);
        fs.Close();
        return newData;
    }

    private Vector3 ToVec3(float[] floats) => new Vector3(floats[0], floats[1], floats[2]);

    private Quaternion ToQuat(float[] floats) => new Quaternion(floats[0], floats[1], floats[2], floats[3]);

    private float[] ToArray(Vector3 vec) => new float[3] { vec.x, vec.y, vec.z };

    private float[] ToArray(Quaternion quat) => new float[4] { quat.x, quat.y, quat.z, quat.w };

    #endregion
}
