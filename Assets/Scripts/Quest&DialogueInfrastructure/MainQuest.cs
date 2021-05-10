using System.Collections;
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
 * Stage 2: Talk to Spirit.
 * 
 * Stage 3: Advance to stage 4 by finding 2 of the items and bringing them to the Heart.
 * 
 * Stage 4: Talk to Spirit.
 * 
 * Stage 5: Advance to stage 6 by finding all of the items and bringing them to the Heart.
 * 
 * Stage 6: Trigger cutscene. Put player in front of Spirit. Advance to stage 7 by talking to spirit.
 * 
 * Stage 7: Finish the game.
 */
public class MainQuest : FSMQuest
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
            _fsm.TransitionTo<Stage7State>,
            _fsm.TransitionTo<Stage0State>
        };

        startNextStage = _fsm.TransitionTo<Stage0State>;
    }

    public void LateUpdateLog()
    {
        Services.UIManager.SetQuestlogText(QuestTag, ConstructLogString());
    }

    private string ConstructLogString()
    {
        string toReturn = "Find ";
        if (!Services.QuestItemRepository._collectedSeed)
        {
            toReturn += " the Seed";
            if (!Services.QuestItemRepository._collectedSoil)
            {
                toReturn += ", and the Soil";
            }
            if (!Services.QuestItemRepository._collectedRain)
            {
                toReturn += ", and the Rain";
            }
        } else if (!Services.QuestItemRepository._collectedSoil)
        {
            toReturn += " the Soil";
            if (!Services.QuestItemRepository._collectedRain)
            {
                toReturn += ", and the Rain";
            }
        } else if (!Services.QuestItemRepository._collectedRain)
        {
            toReturn += " the Rain";
        }
        toReturn += ".";

        return toReturn;
    }

    private void Start()
    {
        startNextStage();
        _fsm.Update();
    }

    // Stage 0: Start of game. Advance to stage 1 by talking to the forest spirit.
    protected class Stage0State : QuestState
    {
        public Stage0State() : base(0) { }
    }

    // Stage 1: Each quest is spawned. Advance to stage 2 by finding 1 of the items and bringing it to the Heart.
    private class Stage1State : QuestState
    {
        public Stage1State() : base(1) { }

        public override void OnEnter()
        {
            base.OnEnter();
            QuestManager.AdvanceQuest(Str.Warbler);
            QuestManager.AdvanceQuest(Str.Frog);
            QuestManager.AdvanceQuest(Str.Turtle);

            FModMusicManager.PlayTrack("Layer 1");
            if (!Services.SaveManager.loadingSave)
            {
                Services.SaveManager.SaveData();
            }
        }
    }

    // Stage 2: Talk to Spirit.
    private class Stage2State : QuestState
    {

        public Stage2State() : base(2) { }

        public override void OnEnter()
        {
            ((MainQuest)Context).SetCurrentQuestLog(((MainQuest)Context).ConstructLogString());
            base.OnEnter();
            if (!Services.SaveManager.loadingSave)
            {
                Services.GameManager.MidrollCutscene();
                FModMusicManager.PlayTrack("Layer 2");
            }
        }
    }

    // Stage 3: Advance to stage 4 by finding 2 of the items and bringing them to the Heart.
    private class Stage3State : QuestState
    {
        public Stage3State() : base(3) { }

        public override void OnEnter()
        {
            base.OnEnter();
            if (!Services.SaveManager.loadingSave)
            {
                Services.SaveManager.SaveData();
            }
        }
    }

    // Stage 4: Talk to Spirit.
    private class Stage4State : QuestState
    {
        public Stage4State() : base(4) { }

        public override void OnEnter()
        {
            ((MainQuest)Context).SetCurrentQuestLog(((MainQuest)Context).ConstructLogString());
            base.OnEnter();
            if (!Services.SaveManager.loadingSave)
            {
                Services.GameManager.MidrollCutscene();
                FModMusicManager.PlayTrack("Layer 3");
            }
        }
    }

    // Stage 5: Advance to stage 6 by finding all of the items and bringing them to the Heart.
    private class Stage5State : QuestState
    {
        public Stage5State() : base(5) { }

        public override void OnEnter()
        {
            base.OnEnter();
            if (!Services.SaveManager.loadingSave)
            {
                Services.SaveManager.SaveData();
            }
        }
    }

    // Stage 6: Trigger cutscene. Put player in front of Spirit. Advance to stage 7 by talking to spirit.
    private class Stage6State : QuestState
    {

        public Stage6State() : base(6) { }

        public override void OnEnter()
        {
            base.OnEnter();
            if (!Services.SaveManager.loadingSave)
            {
                Services.GameManager.MidrollCutscene();
            }
        }
    }

    // Stage 7: Talk to Spirit. Finish the game.
    private class Stage7State : QuestState
    {

        public Stage7State() : base(7) { }

        public override void OnEnter()
        {
            base.OnEnter();
            Services.GameManager.EndCutscene();
            FModMusicManager.PlayTrack("End");
            // TRIGGER END CUTSCENE.
        }
    }
}
