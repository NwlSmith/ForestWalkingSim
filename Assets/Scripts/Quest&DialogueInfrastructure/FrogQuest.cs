using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * Creator: Nate Smith
 * Creation Date: 2/26/2021
 * Description: FSMQuest for the Frog&Toad quest.
 * 
 * Stage 0: Quest is spawned. Advance to stage 1 by talking to F&T.
 * 
 * Stage 1: Spawn in Toad at "Carp" location. Advance to stage 2 by talking to Toad at new location.
 * 
 * Stage 2: Spawn in flower and Soil. Advance to stage 3 by picking up the Soil.
 * 
 * Stage 3: Advance to stage 4 by placing Soil in the heart.
 * 
 * Stage 4: Finish the quest, despawn everything.
 */
public class FrogQuest : FSMQuest
{
    protected override void Awake()
    {
        base.Awake();

        startNextStages[0] = _fsm.TransitionTo<Stage1State>;
        startNextStages[1] = _fsm.TransitionTo<Stage2State>;
        startNextStages[2] = _fsm.TransitionTo<Stage3State>;
        startNextStages[3] = _fsm.TransitionTo<Stage4State>;
        startNextStages[4] = _fsm.TransitionTo<Stage0State>;

        startNextStage = startNextStages[0];
    }

    // Stage 0: Quest is spawned. Advance to stage 1 by talking to F&T.
    private class Stage0State : QuestState
    {
        protected new readonly int _stageNum = 0;
    }

    // Stage 1: Spawn in Toad at "Carp" location. Advance to stage 2 by talking to Toad at new location.
    private class Stage1State : QuestState
    {
        protected new readonly int _stageNum = 1;
    }

    // Stage 2: Spawn in flower and Soil. Advance to stage 3 by picking up the Soil.
    private class Stage2State : QuestState
    {
        protected new readonly int _stageNum = 2;
    }

    // Stage 3: Advance to stage 4 by placing Soil in the heart.
    private class Stage3State : QuestState
    {
        protected new readonly int _stageNum = 3;
    }

    // Stage 4: Finish the quest, despawn everything.
    private class Stage4State : QuestState
    {
        protected new readonly int _stageNum = 4;

        public override void OnEnter()
        {
            base.OnEnter();
            // TRIGGER END CUTSCENE.
        }
    }
}
