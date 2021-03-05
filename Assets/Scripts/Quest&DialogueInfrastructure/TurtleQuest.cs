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
 * Stage 1: Turtle says she wants you to race. Advance to stage 2 by talking leaving the area.
 * 
 * Stage 2: Despawn original Turtle. Go to start line (out of sight from turtle's house) and see turtle there. Advance to stage 3 by talking to Turtle.
 * 
 * Stage 3: Talk to turtle at start line. She says she wants to race you. Count down then make turtle move forward super slow. Advance to stage 4 by either turning corner or turtle crosses finish line.
 *  - When the turtle despawns from turning the corner, have cartoon zoom sound and puff of dust
 * 
 * Stage 4: Spawn in Rain. Talk to turtle at finish line. Advance to stage 5 by placing Rain in the heart.
 * 
 * Stage 5: Finish the quest, despawn everything.
 */
public class TurtleQuest : FSMQuest
{
    protected override void Awake()
    {
        base.Awake();

        startNextStages = new StartNextStage[]
        {
            _fsm.TransitionTo<Stage1State>,
            _fsm.TransitionTo<Stage2State>,
            _fsm.TransitionTo<Stage3State>,
            _fsm.TransitionTo<Stage4State>,
            _fsm.TransitionTo<Stage5State>,
            _fsm.TransitionTo<Stage0State>
        };

        startNextStage = _fsm.TransitionTo<Stage0State>;
    }

    // Stage 0: Quest is spawned. Advance to stage 1 by talking to turtle.
    private class Stage0State : QuestState
    {
        public override void OnEnter()
        {
            _stageNum = 0;
            base.OnEnter();
        }
    }

    // Stage 1: Turtle says she wants you to race. Advance to stage 2 by talking leaving the area.
    private class Stage1State : QuestState
    {
        public override void OnEnter()
        {
            _stageNum = 1;
            base.OnEnter();
        }
    }

    // Stage 2: Despawn original Turtle. Go to start line (out of sight from turtle's house) and see turtle there. Advance to stage 3 by talking to Turtle.
    private class Stage2State : QuestState
    {
        public override void OnEnter()
        {
            _stageNum = 2;
            base.OnEnter();
        }
    }

    // Stage 3: Talk to turtle at start line. She says she wants to race you. Count down then make turtle move forward super slow. Advance to stage 4 by either turning corner or turtle crosses finish line.
    private class Stage3State : QuestState
    {
        public override void OnEnter()
        {
            _stageNum = 3;
            base.OnEnter();
        }

        public override void Update()
        {
            base.Update();
            // MAKE THE TURTLE MOVE
        }
    }

    // Stage 4: Spawn in Rain. Talk to turtle at finish line. Finish the quest by placing Rain in the heart. Despawn everything
    private class Stage4State : QuestState
    {
        public override void OnEnter()
        {
            _stageNum = 4;
            base.OnEnter();
        }
    }

    // Stage 5: Finish the quest, despawn everything.
    private class Stage5State : QuestState
    {
        public override void OnEnter()
        {
            _stageNum = 5;
            base.OnEnter();
            // End.
        }
    }
}
