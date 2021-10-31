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

    public bool loadingSave = false;

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
        public float[] seedPos;
        public float[] soilPos;
        public float[] rainPos;
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
        foreach (QuestItem questItem in questItems)
        {
            if (!questItem.name.Contains("(1)"))
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
        

        if (!File.Exists(Application.dataPath + Str.SaveDefaultName))
            CreateDefaultSave();
    }
    #endregion

    #region Functions.

    public void CreateDefaultSave()
    {
        QuestStageData[] questStagesArray =
        {
            new QuestStageData{quest = Str.Main, stage = 0},
            new QuestStageData{quest = Str.Warbler, stage = 0},
            new QuestStageData{quest = Str.Frog, stage = 0},
            new QuestStageData{quest = Str.Turtle, stage = 0}
        };

        PlayerData playerData = new PlayerData
        {
            position = ToArray(new Vector3(369.7f, 32.45f, 411.16f)),
            rotation = ToArray(new Quaternion(0f, 0f, 0f, 1f))
        };

        PlayerHolding playerHolding = new PlayerHolding { itemHolding = QuestItem.QuestItemEnum.None };

        WarblerChildrenStatus warblerChildrenStatus = new WarblerChildrenStatus { foundChild1 = false, foundChild2 = false, foundChild3 = false };

        ItemStatus itemStatus = new ItemStatus {
            foundSeed = false,
            foundSoil = false,
            foundRain = false,
            seedPos = ToArray(seedItem.transform.position),
            soilPos = ToArray(soilItem.transform.position),
            rainPos = ToArray(rainItem.transform.position)};

        Data saveData = new Data
        {
            questStageData = questStagesArray,
            playerData = playerData,
            playerHolding = playerHolding,
            warblerChildrenStatus = warblerChildrenStatus,
            itemStatus = itemStatus
        };

        SerializeData(Str.SaveDefaultName, saveData);
        //SerializeJson(saveDefaultName, saveData);
    }

    public bool SaveExists()
    {
        if (!File.Exists(Application.dataPath + Str.SaveName)) return false;
        string defaultSaveString = File.ReadAllText(Application.dataPath + Str.SaveDefaultName);
        string saveString = File.ReadAllText(Application.dataPath + Str.SaveName);
        return !defaultSaveString.Equals(saveString); // If they are equal, then there is no distinct save file.
    }

    public void NewGameSave() => SerializeData(Str.SaveName, DeserializeData(Str.SaveDefaultName));

    public void SaveData()
    {
        Logger.Warning("Saving data...");
        QuestStageData[] questStagesArray =
        {
            new QuestStageData{quest = Str.Main, stage = mainQuest.QuestStage},
            new QuestStageData{quest = Str.Warbler, stage = warblerQuest.QuestStage},
            new QuestStageData{quest = Str.Frog, stage = frogQuest.QuestStage},
            new QuestStageData{quest = Str.Turtle, stage = turtleQuest.QuestStage}
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
            foundChild1 = questMemory.GetValue(Str.Child1String).AsBool,
            foundChild2 = questMemory.GetValue(Str.Child2String).AsBool,
            foundChild3 = questMemory.GetValue(Str.Child3String).AsBool
        };

        ItemStatus itemStatus = new ItemStatus {
            foundSeed = questMemory.GetValue(Str.SeedString).AsBool,
            foundSoil = questMemory.GetValue(Str.SoilString).AsBool,
            foundRain = questMemory.GetValue(Str.RainString).AsBool,
            seedPos = ToArray(seedItem.transform.position),
            soilPos = ToArray(soilItem.transform.position),
            rainPos = ToArray(rainItem.transform.position)
        };

        Data saveData = new Data {
            questStageData = questStagesArray,
            playerData = playerData,
            playerHolding = playerHolding,
            warblerChildrenStatus = warblerChildrenStatus,
            itemStatus = itemStatus
        };

        SerializeData(Str.SaveName, saveData);

        //SerializeJson(saveName, saveData);
        Logger.Warning("Data saved successfully.");
    }

    public IEnumerator LoadDataCO()
    {
        loadingSave = true;
        //DeserializeJson(saveName);
        Data data = DeserializeData(Str.SaveName);

        Yarn.Unity.InMemoryVariableStorage questMemory = Services.DialogueController.InMemoryVariableStorage;
        questMemory.SetValue(Str.Child1String, data.warblerChildrenStatus.foundChild1);
        questMemory.SetValue(Str.Child2String, data.warblerChildrenStatus.foundChild2);
        questMemory.SetValue(Str.Child3String, data.warblerChildrenStatus.foundChild3);

        questMemory.SetValue(Str.SeedString, data.itemStatus.foundSeed);
        questMemory.SetValue(Str.SoilString, data.itemStatus.foundSoil);
        questMemory.SetValue(Str.RainString, data.itemStatus.foundRain);

        seedItem.transform.position = ToVec3(data.itemStatus.seedPos);
        soilItem.transform.position = ToVec3(data.itemStatus.soilPos);
        rainItem.transform.position = ToVec3(data.itemStatus.rainPos);

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
                QuestManager.AdvanceQuest(questStageData.quest);
                yield return null;
            }
        }
        yield return null;
        loadingSave = false;
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
