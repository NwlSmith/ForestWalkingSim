using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * Creator: Nate Smith
 * Creation Date: 2/26/2021
 * Description: FSMQuest for the turtle quest.
 * 
 * Stage 0: Quest is spawned. Advance to stage 1 by talking to Turtle.
 * 
 * Stage 1: This is kinda a WIP. I'm not super into this quest.
 */
public class TurtleQuest : FSMQuest
{
    protected override void Awake()
    {
        base.Awake();

        startNextStages[0] = _fsm.TransitionTo<Stage1State>;
        startNextStages[1] = _fsm.TransitionTo<Stage0State>;

        startNextStage = startNextStages[0];
    }

    // Stage 0: Quest is spawned. Advance to stage 1 by talking to turtle.
    private class Stage0State : QuestState
    {
        protected new readonly int _stageNum = 0;
    }

    private class Stage1State : QuestState
    {
        protected new readonly int _stageNum = 1;
        public override void OnEnter()
        {
            base.OnEnter();
            //((TurtleQuest)Context).startNextStage = TransitionTo<Stage2State>;
        }
    }
}
