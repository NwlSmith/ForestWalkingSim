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
    private TaskManager _taskManager = new TaskManager();


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
        
        foreach (Transform route in _turtleRoute)
        {
            route.GetComponent<MeshRenderer>().enabled = false;
        }
    }

    // Stage 0: Quest is spawned. Advance to stage 1 by talking to turtle.
    private class Stage0State : QuestState
    {
        public Stage0State() : base(0) { }
    }

    // Stage 1: Turtle says she wants you to race. Advance to stage 2 by talking leaving the area.
    private class Stage1State : QuestState
    {
        public Stage1State() : base(1) { }

        public override void OnEnter()
        {
            base.OnEnter();
            if (!Services.SaveManager.loadingSave)
            {
                Services.SaveManager.SaveData();
            }
        }
    }

    // Stage 2: Despawn original Turtle. Go to start line (out of sight from turtle's house) and see turtle there. Advance to stage 3 by talking to Turtle.
    private class Stage2State : QuestState
    {
        public Stage2State() : base(2) { }

        public override void OnEnter()
        {
            base.OnEnter();
            if (!Services.SaveManager.loadingSave)
            {
                Services.SaveManager.SaveData();
            }
        }
    }

    // Stage 3: Talk to turtle at start line. She says she wants to race you. Count down then make turtle move forward super slow. Advance to stage 4 by either turning corner or turtle crosses finish line.
    // Need to: Move toward 1 transform, then the next, then the next, until it reaches the end, then advance quest.
    // If player gets ahead, need to speed ahead?
    private class Stage3State : QuestState
    {

        private const float _turtleSpeed = 5f;
        private Transform _turtleTrans;
        private Animator _turtleAnim;
        private const float _distFromTarget = 2f;


        public Stage3State() : base(3) { }

        public override void OnEnter()
        {
            base.OnEnter();
            if (ReferenceEquals(_turtleTrans, null)) _turtleTrans = ((TurtleQuest)Context)._turtleNPC.transform;
            if (ReferenceEquals(_turtleAnim, null)) _turtleAnim = ((TurtleQuest)Context)._turtleNPC.GetComponentInChildren<Animator>();
            ((TurtleQuest)Context)._taskManager.Do(DefineTasks());
        }

        // Defines tasks for turtle movement.
        private Task DefineTasks()
        {
            NPCCollider npcCollider = _turtleTrans.GetComponentInChildren<NPCCollider>();
            Vector3 initScale = npcCollider.transform.localScale;
            npcCollider.transform.localScale = Vector3.zero;
            Task wait = new WaitTask(1f);

            Task start = new ActionTask( () =>
            {
                _turtleAnim.SetBool(Str.Running, true);
                // sound?
            });

            Task prev = start;

            for (int i = 0; i < ((TurtleQuest)Context)._turtleRoute.Length; i++)
            {
                Task next = TurtleMove(((TurtleQuest)Context)._turtleRoute[i]);
                prev = prev.Then(next);
            }

            Task finish = new ActionTask
                (
                    () => {
                        _turtleAnim.SetBool(Str.Running, false);
                        npcCollider.transform.localScale = initScale; // causes problems
                        npcCollider.Appear(); // causes problems
                        if (Context.QuestStage < 4)
                            QuestManager.AdvanceQuest(Context.QuestTag);
                    }
                );


            wait.Then(start);

            prev.Then(finish);

            return wait;
        }

        private DelegateTask TurtleMove(Transform target)
        {
            return new DelegateTask(
                () => 
                {
                    _turtleTrans.LookAt(target);
                },
                () =>
                {
                    _turtleTrans.LookAt(target);
                    _turtleTrans.position = (_turtleTrans.position + _turtleTrans.forward * _turtleSpeed * Time.deltaTime);
                    //_turtleRB.MovePosition(_turtleRB.position + _turtleRB.transform.forward * _turtleSpeed * Time.deltaTime);
                    //_turtleRB.transform.position = _turtleRB.position + _turtleRB.transform.forward * _turtleSpeed * Time.deltaTime;
                    return Vector3.Distance(_turtleTrans.position, target.position) < _distFromTarget;
                }
            );
        }

        public override void Update() => ((TurtleQuest)Context)._taskManager.Update();
    }

    // Stage 4: Talk to turtle at finish line. Advance to stage 5 by talking to turtle.
    private class Stage4State : QuestState
    {
        public Stage4State() : base(4) { }

        public override void OnEnter()
        {
            base.OnEnter();
            if (!Services.SaveManager.loadingSave)
            {
                Services.SaveManager.SaveData();
            }
        }

        public override void Update() => ((TurtleQuest)Context)._taskManager.Update();
    }

    // Stage 5: Spawn in Rain. Advance to stage 6 by placing Rain in the heart.
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

    // Stage 6: Finish the quest, despawn everything.
    private class Stage6State : QuestState
    {
        public Stage6State() : base(6) { }
    }
}
