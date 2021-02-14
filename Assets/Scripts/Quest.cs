using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * Creator: Nate Smith
 * Creation Date: 2/14/2021
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
    [SerializeField] public string questName = "";

    [SerializeField] public enum QuestProgress { NOT_AVAILABLE, AVAILABLE, COMPLETED };

    [SerializeField] public QuestProgress progress = QuestProgress.NOT_AVAILABLE;


    // The finite state machine of the current QuestState.
    private FiniteStateMachine<Quest> _fsm;

    private void Awake()
    {
        _fsm = new FiniteStateMachine<Quest>(this);
    }

    void Start()
    {
        _fsm.TransitionTo<StartQuest>();
    }

    private void Update()
    {
        _fsm.Update();
    }

    [System.Serializable]
    private abstract class QuestState : FiniteStateMachine<Quest>.State
    {
        public string DefaultQuestStateString;
    }

    // Normal camera follow state.
    private class StartQuest : QuestState
    {
        public override void OnEnter() { }

        public override void Update() { base.Update(); }

        

        public override void OnExit() { }
    }
}
