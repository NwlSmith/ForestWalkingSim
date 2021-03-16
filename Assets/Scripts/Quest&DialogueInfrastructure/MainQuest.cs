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
 * Stage 2: Advance to stage 3 by finding 2 of the items and bringing them to the Heart.
 * 
 * Stage 3: Advance to stage 4 by finding all of the items and bringing them to the Heart.
 * 
 * Stage 4: Finish the game, trigger cutscene.
 */
public class MainQuest : FSMQuest
{

    [SerializeField] private GameObject[] barrierFoliage;

    protected override void Awake()
    {
        base.Awake();

        startNextStages = new StartNextStage[]
        {
            _fsm.TransitionTo<Stage1State>,
            _fsm.TransitionTo<Stage2State>,
            _fsm.TransitionTo<Stage3State>,
            _fsm.TransitionTo<Stage4State>,
            _fsm.TransitionTo<Stage0State>
        };

        startNextStage = _fsm.TransitionTo<Stage0State>;
    }

    private void Start()
    {
        startNextStage();
        _fsm.Update();
    }

    // Stage 0: Start of game. Advance to stage 1 by talking to the forest spirit.
    protected class Stage0State : QuestState
    {
        public override void OnEnter()
        {
            _stageNum = 0;
            base.OnEnter();
        }
    }

    // Stage 1: Each quest is spawned. Advance to stage 2 by finding 1 of the items and bringing it to the Heart.
    private class Stage1State : QuestState
    {
        public override void OnEnter()
        {
            _stageNum = 1;
            base.OnEnter();
            Services.QuestManager.AdvanceQuest("Warbler");
            Services.QuestManager.AdvanceQuest("Frog");
            Services.QuestManager.AdvanceQuest("Turtle");

            if (((MainQuest)Context).barrierFoliage.Length > 0)
                Context.StartCoroutine(LowerBarriers());
        }

        private IEnumerator LowerBarriers()
        {
            float elapsedTime = 0f;
            float duration = 5f;
            // play sound?

            float initY = ((MainQuest)Context).barrierFoliage[0].transform.position.y;
            float targetY = 4f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float frac = elapsedTime / duration;
                foreach (GameObject gameObject in ((MainQuest)Context).barrierFoliage)
                {
                    gameObject.transform.position = new Vector3(gameObject.transform.position.x, Mathf.Lerp(initY, targetY, frac), gameObject.transform.position.z);
                }
                yield return null;
            }
        }
    }

    // Stage 2: Each quest is spawned. Advance to stage 3 by finding 2 of the items and bringing them to the Heart.
    private class Stage2State : QuestState
    {
        public override void OnEnter()
        {
            _stageNum = 2;
            base.OnEnter();
        }
    }

    // Stage 3: Each quest is spawned. Advance to stage 4 by finding all of the items and bringing them to the Heart.
    private class Stage3State : QuestState
    {
        public override void OnEnter()
        {
            _stageNum = 3;
            base.OnEnter();
        }
    }

    // Stage 4: Finish the game, trigger cutscene.
    private class Stage4State : QuestState
    {
        public override void OnEnter()
        {
            _stageNum = 4;
            base.OnEnter();
            // TRIGGER END CUTSCENE.
        }
    }
}
