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

    private readonly TaskManager _taskManager = new TaskManager();

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

        foreach (WarblerRoute route in _warblerRoutes)
        {
            foreach (Transform t in route.route)
            {
                t.GetComponent<MeshRenderer>().enabled = false;
            }
        }
    }

    private int GetTriggeredWarbler()
    {
        Yarn.Unity.InMemoryVariableStorage questMemory = Services.DialogueController.InMemoryVariableStorage;
        bool foundChild1 = questMemory.GetValue(Str.Child1String).AsBool;
        bool foundChild2 = questMemory.GetValue(Str.Child2String).AsBool;
        bool foundChild3 = questMemory.GetValue(Str.Child3String).AsBool;

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
    private void SendTriggeredBirdHome(int curWarblerNum = -1, bool disableAtEnd = false)
    {
        if (curWarblerNum == -1)
            curWarblerNum = GetTriggeredWarbler();
        
        NPC childNPC = _warblerNPC[curWarblerNum];
        childNPC.GetComponentInChildren<NPCCollider>().transform.localScale = Vector3.zero;
        Task start = new ActionTask(() =>
        {
            childNPC.GetComponentInChildren<Animator>().SetBool(Str.Moving, true);
            childNPC.enabled = false;
            // sound?
        });
        Task prev = start;

        for (int i = 0; i < _warblerRoutes[curWarblerNum].Length; i++)
        {
            Task next = WarblerMove(curWarblerNum, childNPC.transform, _warblerRoutes[curWarblerNum].route[i]);
            prev = prev.Then(next);
        }

        Task finish = new ActionTask(() =>
        {
            if (disableAtEnd)
                childNPC.gameObject.SetActive(false);
            childNPC.GetComponentInChildren<Animator>().SetBool(Str.Moving, false);
            childNPC.transform.rotation = _warblerRoutes[curWarblerNum].route[_warblerRoutes[curWarblerNum].route.Length - 1].rotation;
        });

        prev.Then(finish);

        _taskManager.Do(start);
    }

    private float[] turningSmoothVel = new float[3];

    private DelegateTask WarblerMove(int curWarblerNum, Transform npc, Transform target)
    {
        float _elapsedTime = 0f;
        return new DelegateTask(
            () =>
            {
                Debug.Log("Going for target " + target.name);
            },
            () =>
            {
                _elapsedTime += Time.deltaTime;
                float targetAngle = Quaternion.LookRotation((target.position - npc.position).normalized, Vector3.up).eulerAngles.y;
                float angle = Mathf.SmoothDampAngle(npc.eulerAngles.y, targetAngle, ref turningSmoothVel[curWarblerNum], .2f);
                npc.rotation = Quaternion.Euler(0f, angle, 0f);
                
                npc.position = npc.position + npc.forward * _warblerSpeed * Time.deltaTime;
                if (target.position.y > npc.position.y + .25f)
                {
                    npc.position = npc.position + npc.up * _warblerSpeed * Time.deltaTime;
                } else if (target.position.y < npc.position.y - .25f)
                {
                    npc.position = npc.position - npc.up * _warblerSpeed * Time.deltaTime;
                }

                return Vector3.Distance(npc.position, target.position) < _distFromTarget || _elapsedTime > 3f;
            },
            () =>
            {
                npc.position = target.position;
            }
        );
    }

    protected override void Update() => _taskManager.Update();


    // Stage 0: Quest is spawned. Advance to stage 1 by talking to the mamma warbler.
    private class Stage0State : QuestState
    {
        public Stage0State() : base(0) { }
    }

    // Stage 1: Each child is spawned. Advance to stage 2 by finding one of the children.
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

    // Stage 2: Advance to stage 3 by finding the second child.
    private class Stage2State : QuestState
    {
        public Stage2State() : base(2) { }

        public override void OnEnter()
        {
            base.OnEnter();
            ((WarblerQuest)Context).SendTriggeredBirdHome();
            if (!Services.SaveManager.loadingSave)
            {
                Services.SaveManager.SaveData();
            }
        }
    }

    // Stage 3: Advance to stage 4 by finding the third child.
    private class Stage3State : QuestState
    {

        public Stage3State() : base(3) { }

        public override void OnEnter()
        {
            base.OnEnter();
            ((WarblerQuest)Context).SendTriggeredBirdHome();
            if (!Services.SaveManager.loadingSave)
            {
                Services.SaveManager.SaveData();
            }
        }
    }

    // Stage 4: Spawn all children near mother. Advance to stage 5 by talking to them.
    private class Stage4State : QuestState
    {
        public Stage4State() : base(4) { }

        public override void OnEnter()
        {
            base.OnEnter();

            int lastWarbler = ((WarblerQuest)Context).GetTriggeredWarbler();

            for (int i = 0; i < 3; i++)
            {
                if (i != lastWarbler)
                {
                    ((WarblerQuest)Context)._warblerNPC[i].gameObject.SetActive(false);
                }
            }

            ((WarblerQuest)Context).SendTriggeredBirdHome(lastWarbler, true);
            if (!Services.SaveManager.loadingSave)
            {
                Services.SaveManager.SaveData();
            }
        }
    }

    // Stage 5: Spawn in The Seed. Advance to stage 6 by placing Seed in the heart.
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
