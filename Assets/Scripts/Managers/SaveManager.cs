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
    private const string questStageMain = "main";
    private const string questStageWarbler = "warbler";
    private const string questStageFrog = "frog";
    private const string questStageTurtle = "turtle";
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

    #region Functions.
    public void SaveData()
    {

        QuestStageData[] questStagesArray =
        {
            new QuestStageData{quest = questStageMain, stage = 0},
            new QuestStageData{quest = questStageWarbler, stage = 0},
            new QuestStageData{quest = questStageFrog, stage = 0},
            new QuestStageData{quest = questStageTurtle, stage = 0}
        };

        PlayerData playerData = new PlayerData {
            position = Services.PlayerMovement.transform.position,
            rotation = Services.PlayerMovement.transform.rotation };

        PlayerHolding playerHolding = new PlayerHolding { itemHolding = ItemEnum.Rain };

        Data saveData = new Data {
            questStageData = questStagesArray,
            playerData = playerData,
            playerHolding = playerHolding };

        string saveDataJson = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(Application.dataPath + "/save.json", saveDataJson);
    }

    public void LoadData()
    {
        string saveString = File.ReadAllText(Application.dataPath + "/save.json");
        Data data = JsonUtility.FromJson<Data>(saveString);

        // Set data from here
    }
    #endregion
}
