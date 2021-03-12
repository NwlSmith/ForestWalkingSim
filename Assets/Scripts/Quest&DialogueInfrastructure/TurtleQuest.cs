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
 * Stage 2: Despawn original Turtle. Go to start line (out of sight from turtle's house) and see turtle there. Advance to stage 3 by talking to Turtle. She says she wants to race you. Count down then make turtle move forward super slow.
 * 
 * Stage 3: Talked to turtle at start line. Advance to stage 4 by either turning corner or turtle crosses finish line.
 *  - When the turtle despawns from turning the corner, have cartoon zoom sound and puff of dust
 * 
 * Stage 4: Talk to turtle at finish line. Advance to stage 5 by talking to turtle.
 * 
 * Stage 5: Spawn in Rain. Advance to stage 6 by placing Rain in the heart.
 * 
 * Stage 6: Finish the quest, despawn everything.
 */
public class TurtleQuest : FSMQuest
{

    [SerializeField] private NPC _turtleNPC;

    [SerializeField] private Transform[] _turtleRoute;


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
    // Need to: Move toward 1 transform, then the next, then the next, until it reaches the end, then advance quest.
    // If player gets ahead, need to speed ahead?
    // A task manager would work here... No need for update loop...
    private class Stage3State : QuestState
    {
        private float _turtleSpeed = 5f;
        private int _curTrans = 0;
        private Rigidbody _turtleRB;
        private Animator _turtleAnim;

        private TaskManager _taskManager = new TaskManager();

        public override void OnEnter()
        {
            _stageNum = 3;
            base.OnEnter();


            _turtleRB = ((TurtleQuest)Context)._turtleNPC.GetComponent<Rigidbody>();
            _turtleAnim = ((TurtleQuest)Context)._turtleNPC.GetComponentInChildren<Animator>();

            _taskManager.Do(DefineTasks());
        }

        private Task DefineTasks()
        {
            Task start = new DelegateTask(
                () =>
                {
                    _turtleAnim.SetBool("Running", true);
                },
                () =>
                {
                    _turtleRB.MovePosition(_turtleRB.transform.TransformDirection(_turtleRB.transform.forward));
                    return true;
                } // can add onsuccess handler
            );
            Task mid = new DelegateTask( // probably not efficient
                () =>
                {
                    //_turtleRB.transform.forward -
                },
                () =>
                {
                    _turtleRB.MovePosition(_turtleRB.transform.TransformDirection(_turtleRB.transform.forward));
                    return true;
                }
            );


            return start;
        }

        public override void Update()
        {
            base.Update();
            // MAKE THE TURTLE MOVE
        }
    }

    // Stage 4: Talk to turtle at finish line. Advance to stage 5 by talking to turtle.
    private class Stage4State : QuestState
    {
        public override void OnEnter()
        {
            _stageNum = 4;
            base.OnEnter();
        }
    }

    // Stage 5: Spawn in Rain. Advance to stage 6 by placing Rain in the heart.
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
