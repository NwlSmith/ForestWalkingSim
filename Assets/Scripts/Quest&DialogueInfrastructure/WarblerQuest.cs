using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * Creator: Nate Smith
 * Creation Date: 2/26/2021
 * Description: FSMQuest for the warbler quest.
 * 
 * Stage 0: Quest is spawned. Advance to stage 1 by talking to the mamma warbler.
 * 
 * Stage 1: Each child is spawned. Advance to stage 2 by finding one of the children.
 * 
 * Stage 2: Advance to stage 3 by finding the second child.
 * 
 * Stage 3: Advance to stage 4 by finding the third child.
 * 
 * Stage 4: Spawn all children near mother. Advance to stage 5 by talking to them.
 * 
 * Stage 5: Spawn in The Seed. Advance to stage 6 by placing Seed in the heart.
 * 
 * Stage 6: Finish the quest, despawn everything.
 */
public class WarblerQuest : FSMQuest
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
            _fsm.TransitionTo<Stage6State>,
            _fsm.TransitionTo<Stage0State>
        };

        startNextStage = _fsm.TransitionTo<Stage0State>;
    }

    // Stage 0: Quest is spawned. Advance to stage 1 by talking to the mamma warbler.
    private class Stage0State : QuestState
    {
        public override void OnEnter()
        {
            _stageNum = 0;
            base.OnEnter();
        }
    }

    // Stage 1: Each child is spawned. Advance to stage 2 by finding one of the children.
    private class Stage1State : QuestState
    {
        public override void OnEnter()
        {
            _stageNum = 1;
            base.OnEnter();
        }
    }

    // Stage 2: Advance to stage 3 by finding the second child.
    private class Stage2State : QuestState
    {
        public override void OnEnter()
        {
            _stageNum = 2;
            base.OnEnter();
        }
    }

    // Stage 3: Advance to stage 4 by finding the third child.
    private class Stage3State : QuestState
    {
        public override void OnEnter()
        {
            _stageNum = 3;
            base.OnEnter();
        }
    }

    // Stage 4: Spawn all children near mother. Advance to stage 5 by talking to them.
    private class Stage4State : QuestState
    {
        public override void OnEnter()
        {
            _stageNum = 4;
            base.OnEnter();
        }
    }

    // Stage 5: Spawn in The Seed. Advance to stage 6 by placing Seed in the heart.
    private class Stage5State : QuestState
    {
        public override void OnEnter()
        {
            _stageNum = 5;
            base.OnEnter();
        }
    }

    // Stage 6: Finish the quest, despawn everything.
    private class Stage6State : QuestState
    {
        public override void OnEnter()
        {
            _stageNum = 6;
            base.OnEnter();
        }
    }
}
