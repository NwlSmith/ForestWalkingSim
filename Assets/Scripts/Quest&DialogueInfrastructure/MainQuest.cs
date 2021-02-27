using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * Creator: Nate Smith
 * Creation Date: 2/26/2021
 * Description: FSMQuest for the main quest.
 * 
 * Stage 0: Start of game. Advance to stage 1 by talking to the forest spirit.
 * 
 * Stage 1: Each quest is spawned. Advance to stage 2 by finding 1 of the items and bringing it to the Heart.
 * 
 * Stage 2: Each quest is spawned. Advance to stage 3 by finding 2 of the items and bringing them to the Heart.
 * 
 * Stage 3: Each quest is spawned. Advance to stage 4 by finding all of the items and bringing them to the Heart.
 * 
 * Stage 4: Finish the game, trigger cutscene.
 */
public class MainQuest : FSMQuest
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

        //startNextStage = _fsm.TransitionTo<Stage0State>;
        //moveBackStage = _fsm.TransitionTo<Stage0State>;
    }

    // Stage 0: Start of game. Advance to stage 1 by talking to the forest spirit.
    protected class Stage0State : QuestState
    {
        protected new readonly int _stageNum = 0;
    }

    // Stage 1: Each quest is spawned. Advance to stage 2 by finding 1 of the items and bringing it to the Heart.
    private class Stage1State : QuestState
    {
        protected new readonly int _stageNum = 1;
    }

    // Stage 2: Each quest is spawned. Advance to stage 3 by finding 2 of the items and bringing them to the Heart.
    private class Stage2State : QuestState
    {
        protected new readonly int _stageNum = 2;
    }

    // Stage 3: Each quest is spawned. Advance to stage 4 by finding all of the items and bringing them to the Heart.
    private class Stage3State : QuestState
    {
        protected new readonly int _stageNum = 3;
    }

    // Stage 4: Finish the game, trigger cutscene.
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
