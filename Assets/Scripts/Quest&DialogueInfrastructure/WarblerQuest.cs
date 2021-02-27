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
 * Stage 5: Spawn in The Seed. Advance to stage 6 by picking it up.
 * 
 * Stage 6: Spawn in the Seed receptacle area. Advance to stage 7 by placing Seed in the heart.
 * 
 * Stage 7: Finish the quest, despawn everything.
 */
public class WarblerQuest : FSMQuest
{

    protected override void Awake()
    {
        base.Awake();

        startNextStages[0] = _fsm.TransitionTo<Stage1State>;
        startNextStages[1] = _fsm.TransitionTo<Stage2State>;
        startNextStages[2] = _fsm.TransitionTo<Stage3State>;
        startNextStages[3] = _fsm.TransitionTo<Stage4State>;
        startNextStages[4] = _fsm.TransitionTo<Stage5State>;
        startNextStages[5] = _fsm.TransitionTo<Stage6State>;
        startNextStages[6] = _fsm.TransitionTo<Stage7State>;
        startNextStages[7] = _fsm.TransitionTo<Stage0State>;

        startNextStage = startNextStages[0];
    }

    // Stage 0: Quest is spawned. Advance to stage 1 by talking to the mamma warbler.
    private class Stage0State : QuestState
    {
        protected new readonly int _stageNum = 0;
    }

    // Stage 1: Each child is spawned. Advance to stage 2 by finding one of the children.
    private class Stage1State : QuestState
    {
        protected new readonly int _stageNum = 1;
    }

    // Stage 2: Advance to stage 3 by finding the second child.
    private class Stage2State : QuestState
    {
        protected new readonly int _stageNum = 2;
    }

    // Stage 3: Advance to stage 4 by finding the third child.
    private class Stage3State : QuestState
    {
        protected new readonly int _stageNum = 3;
    }

    // Stage 4: Spawn all children near mother. Advance to stage 5 by talking to them.
    private class Stage4State : QuestState
    {
        protected new readonly int _stageNum = 4;
    }

    // Stage 5: Spawn in The Seed. Advance to stage 6 by picking it up.
    private class Stage5State : QuestState
    {
        protected new readonly int _stageNum = 5;
    }

    // Stage 6: Spawn in the Seed receptacle area. Advance to stage 7 by placing Seed in the heart.
    private class Stage6State : QuestState
    {
        protected new readonly int _stageNum = 6;
    }

    // Stage 7: Finish the quest, despawn everything.
    private class Stage7State : QuestState
    {
        protected new readonly int _stageNum = 7;

        public override void OnEnter()
        {
            base.OnEnter();
            // TRIGGER END CUTSCENE.
        }
    }
}
