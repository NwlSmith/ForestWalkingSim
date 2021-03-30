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
    #region Const Strings.
    private readonly int _moving = Animator.StringToHash("Moving");
    private const string child1String = "$found_warbler_child_1";
    private const string child2String = "$found_warbler_child_2";
    private const string child3String = "$found_warbler_child_3";
    #endregion

    [System.Serializable]
    private class WarblerRoute
    {
        [SerializeField] public Transform[] route;
        public int Length => route.Length;
    }

    [SerializeField] private NPC[] _warblerNPC;

    [SerializeField] private WarblerRoute[] _warblerRoutes = new WarblerRoute[3];
    private int _curWarblerNum;

    private readonly float _warblerSpeed = 4f;
    private readonly float _distFromTarget = .35f;

    private bool[] warblerFound = { false, false, false };

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
    }

    private int GetTriggeredWarbler()
    {
        Yarn.Unity.InMemoryVariableStorage questMemory = Services.DialogueController.InMemoryVariableStorage;
        bool foundChild1 = questMemory.GetValue(child1String).AsBool;
        bool foundChild2 = questMemory.GetValue(child2String).AsBool;
        bool foundChild3 = questMemory.GetValue(child3String).AsBool;

        if (foundChild1 && !warblerFound[0])
        {
            warblerFound[0] = true;
            return 0;
        }
        else if (foundChild2 && !warblerFound[1])
        {
            warblerFound[1] = true;
            return 1;
        }
        else if (foundChild3 && !warblerFound[2])
        {
            warblerFound[2] = true;
            return 2;
        }

        Logger.Warning("No warblers have been found, yet GetTriggeredWarbler was called");
        return 0;
    }


    // Defines tasks for turtle movement.
    private void SendTriggeredBirdHome()
    {
        _curWarblerNum = GetTriggeredWarbler();
        NPC childNPC = _warblerNPC[_curWarblerNum];
        childNPC.GetComponentInChildren<NPCCollider>().transform.localScale = Vector3.zero;
        Task start = new ActionTask(() =>
        {
            childNPC.GetComponentInChildren<Animator>().SetBool(_moving, true);
            // sound?
        });
        Task prev = start;

        for (int i = 0; i < _warblerRoutes[_curWarblerNum].Length; i++)
        {
            Task next = WarblerMove(childNPC.transform, _warblerRoutes[_curWarblerNum].route[i]);
            prev = prev.Then(next);
        }

        Task finish = new ActionTask
            (
                () => {
                    childNPC.gameObject.SetActive(false);
                }
            );

        prev.Then(finish);

        _taskManager.Do(start);
    }

    private float turningSmoothVel;

    private DelegateTask WarblerMove(Transform npc, Transform target)
    {
        return new DelegateTask(
            () =>
            {
                Debug.Log("Going for target " + target.name);
            },
            () =>
            {
                float targetAngle = Quaternion.LookRotation((target.position - npc.position).normalized, Vector3.up).eulerAngles.y;
                float angle = Mathf.SmoothDampAngle(npc.eulerAngles.y, targetAngle, ref turningSmoothVel, .1f);
                npc.rotation = Quaternion.Euler(0f, angle, 0f);
                
                npc.position = npc.position + npc.forward * _warblerSpeed * Time.deltaTime;
                if (target.position.y > npc.position.y + .25f)
                {
                    npc.position = npc.position + npc.up * _warblerSpeed * Time.deltaTime;
                } else if (target.position.y < npc.position.y - .25f)
                {
                    npc.position = npc.position - npc.up * _warblerSpeed * Time.deltaTime;
                }

                return Vector3.Distance(npc.position, target.position) < _distFromTarget;
            }
        );
    }

    protected override void Update() => _taskManager.Update();


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

            ((WarblerQuest)Context).SendTriggeredBirdHome();
        }
    }

    // Stage 3: Advance to stage 4 by finding the third child.
    private class Stage3State : QuestState
    {
        public override void OnEnter()
        {
            _stageNum = 3;
            base.OnEnter();

            ((WarblerQuest)Context).SendTriggeredBirdHome();
        }
    }

    // Stage 4: Spawn all children near mother. Advance to stage 5 by talking to them.
    private class Stage4State : QuestState
    {
        public override void OnEnter()
        {
            _stageNum = 4;
            base.OnEnter();

            ((WarblerQuest)Context).SendTriggeredBirdHome();
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
