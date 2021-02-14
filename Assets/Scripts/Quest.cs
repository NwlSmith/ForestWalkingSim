using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * Creator: Nate Smith
 * Creation Date: 2/13/2021
 * Description: Base class for quests.
 * 
 * Each quest will extend from this quest and implement more specific behavior.
 * 
 * Should it be a FSM? lol this will be difficult
 * 
 * Okay so a quest consists of goals.
 * 
 * goals will be either "grab this item", "bring it here", or "trigger this dialogue"
 * 
 * Quests will be hardcoded, unfortunately, but honestly that's probably fine.
 * 
 * 
 */
public abstract class Quest : MonoBehaviour
{
    [SerializeField] private string questName = "";
}
