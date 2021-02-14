using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New QuestStageData", menuName = "QuestStage")]
public class QuestStageData : ScriptableObject
{
    public enum QuestStageType { Get_Item, Bring_Item_To, Trigger_This_Dialogue };

    [Header("Information")]
    // The main info of the card, ie, what the scenario is.
    [TextArea(10, 100)]
    public string infoText;
    // Which alien is this? either Scientist, Undercover, Assimilation, Corporate, Clown, Vacation, Artist, Lonely
    //public string model = "Placeholder";
    public QuestStageType stageType;

    [Header("Image")]
    // Image that will be sent to the TV on a certain choice
    public Sprite image;
    // If true, pressing button 1 will send this image to the TV
    public bool imageAssociatedWithChoice1 = true;

    [Header("Decision 1")]
    // Text on the first decision's button
    [TextArea(10, 100)]
    public string decision1Text;
    // Modifiers added or subtracted from various stats
    public int d1BiodiversityModifier = 0;
    public int d1AtmosphereTempModifier = 0;
    public int d1DomSubModifier = 0;

    // The response of your roommate
    [TextArea(10, 100)]
    public string d1RoommateResponse;

    public bool d1RoommateResponseSpoken = false;

    // Cards to be shuffled into deck for choosing d1
    public QuestStageData nextStage;
}
