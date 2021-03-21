/*
 * Creator: Nate Smith
 * Creation Date: 2/26/2021
 * Description: FSMQuest for the Frog&Toad quest.
 * 
 * Stage 0: Quest is spawned. Advance to stage 1 by talking to F&T.
 * 
 * Stage 1: Spawn in Toad at "Carp" location. Advance to stage 2 by talking to Toad at new location.
 * 
 * Stage 2: Spawn in flower and Soil. Advance to stage 3 by picking up the Soil and placing Soil in the heart.
 * 
 * Stage 3: Finish the quest, despawn everything.
 */
public class FrogQuest : FSMQuest
{
    protected override void Awake()
    {
        base.Awake();

        startNextStages = new StartNextStage[]
        {
            _fsm.TransitionTo<Stage1State>,
            _fsm.TransitionTo<Stage2State>,
            _fsm.TransitionTo<Stage3State>,
            _fsm.TransitionTo<Stage0State>
        };

        startNextStage = _fsm.TransitionTo<Stage0State>;
    }

    // Stage 0: Quest is spawned. Advance to stage 1 by talking to F&T.
    private class Stage0State : QuestState
    {
        public override void OnEnter()
        {
            _stageNum = 0;
            base.OnEnter();
        }
    }

    // Stage 1: Spawn in Toad at "Carp" location. Advance to stage 2 by talking to Toad at new location.
    private class Stage1State : QuestState
    {
        public override void OnEnter()
        {
            _stageNum = 1;
            base.OnEnter();
        }
    }

    // Stage 2: Spawn in flower and Soil. Advance to stage 3 by picking up the Soil and placing Soil in the heart.
    private class Stage2State : QuestState
    {
        public override void OnEnter()
        {
            _stageNum = 2;
            base.OnEnter();
        }
    }

    // Stage 3: Advance to stage 4 by placing Soil in the heart.
    private class Stage3State : QuestState
    {
        public override void OnEnter()
        {
            _stageNum = 3;
            base.OnEnter();
        }
    }
    
}
