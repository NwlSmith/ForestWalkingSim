using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
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
    #endregion

    #region Quest GameObjects.
    private MainQuest mainQuest;
    private WarblerQuest warblerQuest;
    private FrogQuest frogQuest;
    private TurtleQuest turtleQuest;
    #endregion

    #region Serialized Quest Data.
    [System.Serializable]
    public class Data
    {
        public QuestStageData[] questStageData;
        public PlayerData playerData;
        public PlayerHolding playerHolding;
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

    public enum ItemEnum { Seed, Soil, Rain, None };
    [System.Serializable]
    public class PlayerHolding
    {
        public ItemEnum itemHolding;
    }
    #endregion


    #region Lifecycle Management.
    public SaveManager()
    {
        mainQuest = Object.FindObjectOfType<MainQuest>();
        warblerQuest = Object.FindObjectOfType<WarblerQuest>();
        frogQuest = Object.FindObjectOfType<FrogQuest>();
        turtleQuest = Object.FindObjectOfType<TurtleQuest>();
}
    #endregion

    #region Functions.
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

        PlayerHolding playerHolding = new PlayerHolding { itemHolding = ItemEnum.None };

        Data saveData = new Data {
            questStageData = questStagesArray,
            playerData = playerData,
            playerHolding = playerHolding };

        string saveDataJson = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(Application.dataPath + "/save.json", saveDataJson);
    }

    public void LoadData()
    {
        //StartCoroutine(LoadDataCO());
    }

    public IEnumerator LoadDataCO()
    {
        string saveString = File.ReadAllText(Application.dataPath + "/save.json");
        Data data = JsonUtility.FromJson<Data>(saveString);
        
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
