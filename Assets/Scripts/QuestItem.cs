using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * Creator: Nate Smith
 * Creation Date: 2/28/2021
 * Description: Holdable Quest item script.
 * 
 * Acts as a HoldableItem, but you can only drop it if within a certain specified area, and doing so will trigger a script.
 */
public class QuestItem : HoldableItem
{
    public enum QuestItemEnum { Seed, Soil, Rain };

    public QuestItemEnum itemEnum;

    // Need to implement non-droppable code.
}
