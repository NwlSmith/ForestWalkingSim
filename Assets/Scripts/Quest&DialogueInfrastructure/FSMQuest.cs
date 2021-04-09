using System;
using UnityEngine;
/*
 * Creator: Nate Smith
 * Creation Date: 2/26/2021
 * Description: Finite Stage Machine-based quest class.
 * 
 * Each stage of the quest is a state in a finite state machine.
 * Entering/exiting a stage will enable and disable some objects, and set dialogue triggers to progress dialogue as defined in the QuestStageData struct.
 */
public abstract class FSMQuest : MonoBehaviour
{
    [SerializeField] public string QuestTag;

    [Serializable]
    public struct QuestStageData
    {
        [Header("Called on Stage Enter")]
        // These objects will be activated at the beginning of this quest stage.
        [SerializeField] private GameObject[] _gameObjectsEnableOnStageEnter;
        // These objects will be deactivated at the beginning of this quest stage.
        [SerializeField] private GameObject[] _gameObjectsDisableOnStageEnter;
        // This text will be added to the conversation log at the beginning of this quest stage.
        [Header("Added to text log.")]
        [TextArea(10,100)]
        public string _textAddedToLog;

        public void OnStageEnter()
        {
            foreach (GameObject go in _gameObjectsEnableOnStageEnter)
            {
                if (go != null)
                    go.SetActive(true);
            }
            foreach (GameObject go in _gameObjectsDisableOnStageEnter)
            {
                if (go != null)
                    go.SetActive(false);
            }
        }
    }
    [SerializeField] protected QuestStageData[] _questStates;

    [SerializeField] protected int _questStage = 0;

    protected delegate void StartNextStage();
    protected StartNextStage startNextStage;

    protected StartNextStage[] startNextStages;

    // The finite state machine of the current gamestate.
    protected FiniteStateMachine<FSMQuest> _fsm;

    #region Lifecycle Management

    protected virtual void Awake() => _fsm = new FiniteStateMachine<FSMQuest>(this);

    protected virtual void Update()
    {
        if (_fsm.CurrentState != null)
            _fsm.Update();
    }

    #endregion

    public int QuestStage => _questStage;

    public void AdvanceQuestStage()
    {
        //Logger.Debug($"Attempting to advance to quest stage {_questStage + 1} of {name}.");
        if (_questStage + 1 < _questStates.Length)
        {
            Logger.Debug($"Advancing quest stage.");
            startNextStage();
            _fsm.Update();
        }
        else
        {
            Logger.Warning($"Attempting to advance quest stage of {name} past the number of quest stages, ({_questStates.Length}).");
        }
    }

    public void MoveBackQuestStage()
    {
        if (_questStage - 1 >= 0)
        {
            startNextStage();
        }
        else
        {
            Logger.Warning($"Attempting to move quest stage of {name} back before 0");
        }
    }

    protected void SetCurrentQuestLog(string newString) => _questStates[_questStage]._textAddedToLog = newString;

    #region States

    protected abstract class QuestState : FiniteStateMachine<FSMQuest>.State
    {
        protected readonly int _stageNum = -1;

        public QuestState(int num) => _stageNum = num;

        public override void OnEnter()
        {
            if (_stageNum < 0) return;
            Logger.Debug($"Quest stage {_stageNum} of {Context.name} started");
            Context._questStage = _stageNum;
            Context._questStates[_stageNum].OnStageEnter();

            Services.UIManager.SetQuestlogText(Context.QuestTag, Context._questStates[_stageNum]._textAddedToLog);

            Context.startNextStage = Context.startNextStages[_stageNum];
        }
    }

    #endregion
}
