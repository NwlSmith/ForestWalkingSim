using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

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

    [SerializeField] private List<RectTransform> _pickupItemUI;
    [SerializeField] private List<RectTransform> _dialogueEnterPromptUI;

    [SerializeField] private List<RectTransform> _dialogueUI;
    [SerializeField] private List<RectTransform> _pauseUI;

    [SerializeField] private List<RectTransform> _loadingOverlay;
    [SerializeField] private List<RectTransform> _startOverlay;
    [SerializeField] private List<RectTransform> _startMenu;
    [SerializeField] private List<RectTransform> _continueButton;


    #region Lifecycle Management
    private void Awake()
    {
        _fsm = new FiniteStateMachine<UIManager>(this);

        HideAllUI();
    }

    void Start() => _fsm.TransitionTo<PlayState>();

    private void Update()
    {
        _fsm.Update();
        _taskManager.Update();
    }
    #endregion

    #region Triggers
    public void DisplayItemPickupPrompt() => DisplayUI(_pickupItemUI);

    public void HideItemPickupPrompt() => HideUI(_pickupItemUI);

    public void DisplayDialogueEnterPrompt()
    {
        if (_fsm.CurrentState.GetType() != typeof(InDialogueState))
            DisplayUI(_dialogueEnterPromptUI);
    }

    public void HideDialogueEnterPrompt() => HideUI(_dialogueEnterPromptUI);

    public void EnterPlay() => _fsm.TransitionTo<PlayState>();

    public void EnterDialogue() => _fsm.TransitionTo<InDialogueState>();

    public void EnterPause() => _fsm.TransitionTo<PauseState>();

    public void EnterLoadSave() => _fsm.TransitionTo<LoadSaveState>();

    public void EnterStartMenu() => _fsm.TransitionTo<StartMenuState>();

    public void ShowContinue() => DisplayUI(_continueButton);

    public void HideContinue() => HideUI(_continueButton);

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

    private void DisplayUI(RectTransform UI)
    {
        Animator anim = UI.GetComponent<Animator>();
        if (anim != null)
            anim.SetBool("Visible", true);
        else
            UI.gameObject.SetActive(true);
    }

    private void HideUI(RectTransform UI)
    {
        Animator anim = UI.GetComponent<Animator>();
        if (anim != null)
            anim.SetBool("Visible", false);
        else
            UI.gameObject.SetActive(false);
    }

    private void HideAllUI()
    {
        HideUI(_pickupItemUI);
        HideUI(_dialogueEnterPromptUI);
        HideUI(_dialogueUI);
        HideUI(_pauseUI);
        HideUI(_loadingOverlay);
    }
    #endregion

    #region States

    private abstract class UIState : FiniteStateMachine<UIManager>.State
    {
        
    }

    // Start Menu state.
    private class StartMenuState : UIState
    {

        public override void OnEnter()
        {
            IntroTasks();
        }

        public override void Update()
        {
            base.Update();
        }

        public override void OnExit()
        {
            Task start = new FadeStartMenu(Context, Context._startMenu, false);
            Context._taskManager.Do(start);
        }
        
        private void IntroTasks()
        {
            Debug.Log("IntroTasks !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            Task Start = new ActionTask(() => { Context.HideUI(Context._startOverlay); } );
            Task FadeInStart = new FadeStartMenu(Context, Context._startMenu, true);
            Task StartMusic = new ActionTask(() => { /* maybe start music here? */ });

            Start.Then(FadeInStart).Then(StartMusic);

            Context._taskManager.Do(Start);
        }

        private class FadeStartMenu : Task
        {
            private readonly float gapBetweenFadeIns = .75f;
            private readonly float gapBetweenFadeOuts = .25f;
            private float gapBetweenFade;
            private float elapsedTime = 0f;
            private enum StageToFadeInAtEndEnum { Title, NewGame, Continue, Quit };
            private StageToFadeInAtEndEnum curStage;
            private UIManager uim;
            private List<RectTransform> startMenuItems;
            private delegate void UpdateFunc();
            private UpdateFunc updateFunc;

            public FadeStartMenu(UIManager uim, List<RectTransform> startMenuItems, bool fadeIn)
            {
                this.uim = uim;
                this.startMenuItems = startMenuItems;

                if (fadeIn)
                {
                    curStage = StageToFadeInAtEndEnum.Title;
                    updateFunc = FadeIn;
                    gapBetweenFade = gapBetweenFadeIns;
                }
                else
                {
                    Debug.Log("Fade out start");
                    curStage = StageToFadeInAtEndEnum.Quit;
                    updateFunc = FadeOut;
                    gapBetweenFade = gapBetweenFadeOuts;
                }
            }

            internal override void Update()
            {
                elapsedTime += Time.deltaTime;

                if (elapsedTime >= gapBetweenFade) updateFunc();
            }

            private void FadeIn()
            {

                if (curStage == StageToFadeInAtEndEnum.Continue && !Services.SaveManager.SaveExists()) curStage = StageToFadeInAtEndEnum.Quit; // Skip continue button.
                if (curStage == StageToFadeInAtEndEnum.Quit) SetStatus(TaskStatus.Success);

                uim.DisplayUI(startMenuItems[(int)curStage]);
                curStage++;
                elapsedTime = 0f;
            }

            private void FadeOut()
            {
                Debug.Log("Fade out update");
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
        public override void OnEnter() {
            Context.DisplayUI(Context._loadingOverlay);
        }

        public override void Update() => base.Update();

        public override void OnExit()
        {
            Context.HideUI(Context._loadingOverlay);
        }
    }

    // Normal play-time UI. Usually nothing, except possibly map, compass, and prompts to enter conversation and interact with objects.
    private class PlayState : UIState
    {
        public override void OnEnter()
        {
            Services.UISound.PlayUISound(0);
        }

        public override void Update() => base.Update();

        public override void OnExit()
        {
            Context.HideItemPickupPrompt();
            Context.HideDialogueEnterPrompt();
        }
    }

    // Player is in dialogue.
    private class InDialogueState : UIState
    {
        private readonly float _maxTimeElapsedBeforeUI = 1f;

        public override void OnEnter() => Context.StartCoroutine(DelayDisplayUI());


        private IEnumerator DelayDisplayUI()
        {
            yield return new WaitForSeconds(_maxTimeElapsedBeforeUI);
            Context.DisplayUI(Context._dialogueUI);
            Services.DialogueController.EnterDialogue();
        }

        public override void OnExit() => Context.HideUI(Context._dialogueUI);
    }

    // Pause UI.
    private class PauseState : UIState
    {
        public override void OnEnter() => Context.DisplayUI(Context._pauseUI);

        public override void Update() => base.Update();

        public override void OnExit()
        {
            Debug.Log("Exiting pause in UIManager");
            Context.HideUI(Context._pauseUI);
        }
    }
    #endregion

}
