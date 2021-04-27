using System.Collections.Generic;
using UnityEngine;

/*
 * Creator: Nate Smith
 * Date Created: 2/11/2021
 * Description: UI Manager class
 * 
 * Controls UI state, whether it is enabled or disabled.
 * 
 */

public class UIManager : MonoBehaviour
{


    // The finite state machine of the current UIState.
    private FiniteStateMachine<UIManager> _fsm;
    private TaskManager _taskManager = new TaskManager();

    [SerializeField] private RectTransform          _pickupItemUI;
    [SerializeField] private RectTransform          _dialogueEnterPromptUI;

    [SerializeField] private List<RectTransform>    _dialogueUI;
    [SerializeField] private UnityEngine.UI.Button  _dialogueContinueButton;
    [SerializeField] private UnityEngine.UI.Button  _dialogueSkipButton;
    [SerializeField] private List<RectTransform>    _pauseUI;
    [SerializeField] private UnityEngine.UI.Button  _unpauseButton;

    [SerializeField] private RectTransform          _cutsceneFadeOverlay;
    [SerializeField] private List<RectTransform>    _loadingOverlay;
    [SerializeField] private List<RectTransform>    _startOverlay;
    [SerializeField] private List<RectTransform>    _startMenu;
    [SerializeField] private List<RectTransform>    _continueButton;

    [SerializeField] private RectTransform          _questlogHolder;
    [SerializeField] private TMPro.TextMeshProUGUI  _mainQuestLog;
    [SerializeField] private TMPro.TextMeshProUGUI  _warblerQuestLog;
    [SerializeField] private TMPro.TextMeshProUGUI  _frogQuestLog;
    [SerializeField] private TMPro.TextMeshProUGUI  _turtleQuestLog;

    private readonly List<RectTransform> _allUI = new List<RectTransform>();
    private readonly Dictionary<int, Animator> _RTToAnim = new Dictionary<int, Animator>();
    private readonly Dictionary<string, TMPro.TextMeshProUGUI> _questTagToLog = new Dictionary<string, TMPro.TextMeshProUGUI>();

    private bool _updatingQuestLog = false;

    #region Lifecycle Management
    private void Awake()
    {
        _fsm = new FiniteStateMachine<UIManager>(this);


        CollectAllUI();
        CreateRectTransformToAnimatorDictionary();
        CreateTagToLogDictionary();

        HideAllUI();

        RegisterEvents();
    }

    private void CollectAllUI()
    {
        _allUI.Add(_pickupItemUI);
        _allUI.Add(_dialogueEnterPromptUI);

        foreach (RectTransform rt in _dialogueUI) _allUI.Add(rt);
        _allUI.Add((RectTransform)_dialogueContinueButton.transform);
        _allUI.Add((RectTransform)_dialogueSkipButton.transform);
        foreach (RectTransform rt in _pauseUI) _allUI.Add(rt);

        _allUI.Add(_cutsceneFadeOverlay);
        foreach (RectTransform rt in _loadingOverlay) _allUI.Add(rt);
        foreach (RectTransform rt in _startOverlay) _allUI.Add(rt);
        foreach (RectTransform rt in _startMenu) _allUI.Add(rt);
        foreach (RectTransform rt in _continueButton) _allUI.Add(rt);

        _allUI.Add(_questlogHolder);
        _allUI.Add(_mainQuestLog.rectTransform);
        _allUI.Add(_warblerQuestLog.rectTransform);
        _allUI.Add(_frogQuestLog.rectTransform);
        _allUI.Add(_turtleQuestLog.rectTransform);
    }

    private void CreateRectTransformToAnimatorDictionary()
    {
        foreach (RectTransform rt in _allUI)
        {
            if (rt.TryGetComponent(typeof(Animator), out Component c))
            {
                int id = rt.GetInstanceID();
                if (!_RTToAnim.ContainsKey(id) && !_RTToAnim.ContainsValue((Animator)c))
                    _RTToAnim.Add(id, (Animator)c);
            }
            else
            {
                _RTToAnim.Add(rt.GetInstanceID(), null);
            }
        }
    }

    private void CreateTagToLogDictionary()
    {
        _questTagToLog.Add(Str.Main, _mainQuestLog);
        _questTagToLog.Add(Str.Warbler, _warblerQuestLog);
        _questTagToLog.Add(Str.Frog, _frogQuestLog);
        _questTagToLog.Add(Str.Turtle, _turtleQuestLog);
    }


    private void Start() => _fsm.TransitionTo<PlayState>();

    private void Update()
    {
        _fsm.Update();
        _taskManager.Update();
    }

    private void OnDestroy()
    {
        UnregisterEvents();
    }

    private void RegisterEvents()
    {
        Services.EventManager.Register<OnStartMenu>(_fsm.TransitionTo<StartMenuState>);
        Services.EventManager.Register<OnEnterPlay>(_fsm.TransitionTo<PlayState>);
        Services.EventManager.Register<OnPause>(_fsm.TransitionTo<PauseState>);
        Services.EventManager.Register<OnEnterDialogue>(HideDialogueEnterPrompt);
        Services.EventManager.Register<OnEnterEndGame>(CutsceneFadeIn);
    }

    private void UnregisterEvents()
    {
        Services.EventManager.Unregister<OnStartMenu>(_fsm.TransitionTo<StartMenuState>);
        Services.EventManager.Unregister<OnEnterPlay>(_fsm.TransitionTo<PlayState>);
        Services.EventManager.Unregister<OnPause>(_fsm.TransitionTo<PauseState>);
        Services.EventManager.Unregister<OnEnterDialogue>(HideDialogueEnterPrompt);
        Services.EventManager.Unregister<OnEnterEndGame>(CutsceneFadeIn);
    }

    #endregion

    #region Triggers

    public void DisplayItemPickupPrompt() => DisplayUI(_pickupItemUI);

    public void PositionItemPrompt(Vector3 position) => _pickupItemUI.transform.position = Services.CameraManager.MainCamera.WorldToScreenPoint(position) + Vector3.up;

    public void HideItemPickupPrompt() => HideUI(_pickupItemUI);

    public void DisplayDialogueEnterPrompt()
    {
        if (_fsm.CurrentState.GetType() != typeof(InDialogueState))
        {
            DisplayUI(_dialogueEnterPromptUI);
        }
    }

    public void PositionDialogueEntryPrompt(Vector3 position) => _dialogueEnterPromptUI.transform.position = Services.CameraManager.MainCamera.WorldToScreenPoint(position);

    public void HideDialogueEnterPrompt(AGPEvent e = null) => HideUI(_dialogueEnterPromptUI);

    public void EnterDialogue() => _fsm.TransitionTo<InDialogueState>();

    public void ShowContinueDialogue()
    {
        DisplayUI(_dialogueContinueButton);
        _dialogueContinueButton.Select();
    }

    public void HideContinueDialogue() => HideUI(_dialogueContinueButton);

    public void ShowSkipDialogue()
    {
        DisplayUI(_dialogueSkipButton);
        _dialogueSkipButton.Select();
    }

    public void HideSkipDialogue() => HideUI(_dialogueSkipButton);

    public void EnterLoadSave() => _fsm.TransitionTo<LoadSaveState>();

    public void ShowContinueGame() => DisplayUI(_continueButton);

    public void HideContinueGame() => HideUI(_continueButton);

    public void CutsceneFadeIn(AGPEvent e = null) => DisplayUI(_cutsceneFadeOverlay);

    public void CutsceneFadeOut() => HideUI(_cutsceneFadeOverlay);

    #endregion

    #region Utilities

    private void DisplayUI(List<RectTransform> UI)
    {
        foreach (RectTransform graphic in UI)
            DisplayUI(graphic);
    }

    private void HideUI(List<RectTransform> UI)
    {
        foreach (RectTransform graphic in UI)
            HideUI(graphic);
    }

    private void DisplayUI(UnityEngine.UI.Button UI) => DisplayUI((RectTransform)UI.transform);

    private void HideUI(UnityEngine.UI.Button UI) => HideUI((RectTransform)UI.transform);

    private void DisplayUI(TMPro.TextMeshProUGUI UI)
    {
        DisplayUI(UI.rectTransform);
    }

    private void HideUI(TMPro.TextMeshProUGUI UI) => HideUI(UI.rectTransform);

    private void DisplayUI(RectTransform UI)
    {
        Animator anim = _RTToAnim[UI.GetInstanceID()];
        if (anim != null)
            anim.SetBool(Str.Visible, true);
        else
            UI.gameObject.SetActive(true);
    }

    private void HideUI(RectTransform UI)
    {
        if (UI == null)
            return;
        Animator anim = _RTToAnim[UI.GetInstanceID()];
        if (anim != null)
            anim.SetBool(Str.Visible, false);
        else
            UI.gameObject.SetActive(false);
    }

    private void HideAllUI()
    {
        foreach (RectTransform rt in _allUI)
            HideUI(rt);
    }

    public void SetQuestlogText(string questTag, string newText)
    {
        Debug.LogWarning($"tag = {questTag} text = {newText}");
        if (_questTagToLog.ContainsKey(questTag) && _questTagToLog[questTag] == null)
            return;

        if (newText.Equals("") || newText.Equals("Find out what happened to the forest"))
        {
            _questTagToLog[questTag].text = newText;
            return;
        }

        Task displayHolder = new ActionTask(() =>
        {
            DisplayQuestLogUI();
        });

        Task wait1 = new WaitTask(.5f);

        Task triggerTextAnim = new ActionTask(() =>
        {
            HideUI(_questTagToLog[questTag]);
        });

        Task wait2 = new WaitTask(.5f);

        Task setText = new ActionTask(() =>
        {
            _questTagToLog[questTag].text = newText;
            DisplayUI(_questTagToLog[questTag]);
        });

        Task wait3 = new WaitTask(5.5f);

        Task hideHolder = new ActionTask(() =>
        {
            HideQuestLogUI();
            _updatingQuestLog = false;
        });

        if (_updatingQuestLog)
        {
            wait1.Then(triggerTextAnim).Then(wait2).Then(setText);
            _taskManager.Do(wait1);
        }
        else
        {
            _updatingQuestLog = true;
            displayHolder.Then(wait1).Then(triggerTextAnim).Then(wait2).Then(setText).Then(wait3).Then(hideHolder);
            _taskManager.Do(displayHolder);
        }
    }

    private void DisplayQuestLogUI()
    {
        DisplayUI(_questlogHolder);
        DisplayUI(_mainQuestLog);
        DisplayUI(_warblerQuestLog);
        DisplayUI(_frogQuestLog);
        DisplayUI(_turtleQuestLog);
    }

    private void HideQuestLogUI()
    {
        HideUI(_questlogHolder);
        HideUI(_mainQuestLog);
        HideUI(_warblerQuestLog);
        HideUI(_frogQuestLog);
        HideUI(_turtleQuestLog);
    }

    #endregion

    #region States

    private abstract class UIState : FiniteStateMachine<UIManager>.State
    {

    }

    // Start Menu state.
    private class StartMenuState : UIState
    {
        private bool introInProgress = false;
        private FadeInStartMenu fadeInStartTask;
        public override void OnEnter() => IntroTasks();

        public override void OnExit() => OutroTasks();

        private void IntroTasks()
        {
            Task Start = new ActionTask(() => { Context.HideUI(Context._startOverlay); });
            fadeInStartTask = new FadeInStartMenu(this);
            Task StartMusic = new ActionTask(() =>
            {
                /* maybe start music here? */
                // Highlight start button
            });

            Start.Then(fadeInStartTask).Then(StartMusic);

            Context._taskManager.Do(Start);
        }

        private void OutroTasks()
        {
            if (introInProgress) fadeInStartTask.Abort();
            else Context._taskManager.Do(new FadeOutStartMenu(this));
        }

        private abstract class FadeStartMenu : Task
        {
            protected readonly float gapBetweenFades;
            protected float elapsedTime = 0f;
            protected enum StageToFadeInAtEndEnum { Title, Subtitle, NewGame, Continue, Quit };
            protected StageToFadeInAtEndEnum curStage;
            protected readonly UIManager uim;
            protected readonly List<RectTransform> startMenuItems;
            protected readonly StartMenuState stateContext;

            public FadeStartMenu(StartMenuState state, float gapTime = .5f)
            {
                stateContext = state;
                this.uim = state.Context;
                this.startMenuItems = uim._startMenu;
                gapBetweenFades = gapTime;
            }

            internal override void Update()
            {
                elapsedTime += Time.deltaTime;

                if (elapsedTime >= gapBetweenFades) Fade();
            }

            protected override void OnAbort() => FadeAllOut();

            protected abstract void Fade();

            private void FadeAllOut()
            {
                foreach (RectTransform trans in startMenuItems)
                {
                    uim.HideUI(trans);
                }
            }
        }

        private class FadeInStartMenu : FadeStartMenu
        {

            public FadeInStartMenu(StartMenuState state) : base(state, .5f)
            {
                curStage = StageToFadeInAtEndEnum.Title;
                stateContext.introInProgress = true;
            }

            protected override void Initialize() => stateContext.introInProgress = true;

            protected override void OnSuccess()
            {
                int toSelect;
                if (Services.SaveManager.SaveExists())
                    toSelect = (int)StageToFadeInAtEndEnum.Continue;
                else
                    toSelect = (int)StageToFadeInAtEndEnum.NewGame;
                startMenuItems[toSelect].GetComponent<UnityEngine.UI.Button>().Select();
                stateContext.introInProgress = false;
            }

            protected override void Fade()
            {
                if (curStage == StageToFadeInAtEndEnum.Continue && !Services.SaveManager.SaveExists()) curStage = StageToFadeInAtEndEnum.Quit; // Skip continue button.
                if (curStage == StageToFadeInAtEndEnum.Quit) SetStatus(TaskStatus.Success);

                uim.DisplayUI(startMenuItems[(int)curStage]);
                curStage++;
                elapsedTime = 0f;
            }
        }

        private class FadeOutStartMenu : FadeStartMenu
        {

            public FadeOutStartMenu(StartMenuState state) : base(state, .25f)
            {
                Logger.Debug("Fade out start");
                curStage = StageToFadeInAtEndEnum.Quit;
            }

            protected override void Fade()
            {
                if (curStage == StageToFadeInAtEndEnum.Continue && !Services.SaveManager.SaveExists()) curStage = StageToFadeInAtEndEnum.NewGame; // Skip continue button.
                if (curStage == StageToFadeInAtEndEnum.Title) SetStatus(TaskStatus.Success);

                uim.HideUI(startMenuItems[(int)curStage]);
                curStage--;
                elapsedTime = 0f;
            }
        }

    }

    // UI overlay shown while game is loading.
    private class LoadSaveState : UIState
    {
        public override void OnEnter() => Context.DisplayUI(Context._loadingOverlay);

        public override void OnExit() => Context.HideUI(Context._loadingOverlay);
    }

    // Normal play-time UI. Usually nothing, except possibly map, compass, and prompts to enter conversation and interact with objects.
    private class PlayState : UIState
    {
        public override void OnEnter() => Services.UISound.PlayUISound(0);

        public override void OnExit()
        {
            Context.HideItemPickupPrompt();
            Context.HideDialogueEnterPrompt();
        }
    }

    // Player is in dialogue.
    private class InDialogueState : UIState
    {

        public override void OnEnter() => Context.DisplayUI(Context._dialogueUI);

        public override void OnExit()
        {
            Context.HideUI(Context._dialogueUI);
            Context.HideUI(Context._dialogueContinueButton);
            Context.HideUI(Context._dialogueSkipButton);
        }
    }

    // Pause UI.
    private class PauseState : UIState
    {
        public override void OnEnter()
        {
            Context.DisplayUI(Context._pauseUI);
            Context.DisplayQuestLogUI();
            Context._unpauseButton.Select();
        }

        public override void OnExit()
        {
            Context.HideUI(Context._pauseUI);
            Context.HideQuestLogUI();
        }
    }
    #endregion

}
