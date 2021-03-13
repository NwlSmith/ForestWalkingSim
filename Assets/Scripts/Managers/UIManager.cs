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

    private void Update() => _fsm.Update();
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
        {
            Animator anim = graphic.GetComponent<Animator>();
            if (anim != null)
                anim.SetTrigger("FadeIn");
            else
                graphic.gameObject.SetActive(true);
        }
    }
    private void HideUI(List<RectTransform> UI)
    {
        foreach (RectTransform graphic in UI)
        {
            Animator anim = graphic.GetComponent<Animator>();
            if (anim != null)
                anim.SetTrigger("FadeOut");
            else
                graphic.gameObject.SetActive(false);
        }
    }

    private void DisplayUI(RectTransform UI)
    {
        Animator anim = UI.GetComponent<Animator>();
        if (anim != null)
            anim.SetTrigger("FadeIn");
        else
            UI.gameObject.SetActive(true);
    }

    private void HideUI(RectTransform UI)
    {
        Animator anim = UI.GetComponent<Animator>();
        if (anim != null)
            anim.SetTrigger("FadeOut");
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
            Debug.Log("StartMenuState OnEnter !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            IntroTasks();
        }

        public override void Update()
        {
            base.Update();

            Context._taskManager.Update();
        }

        public override void OnExit()
        {
            Context.HideUI(Context._startMenu);
            Context.HideContinue();
        }
        
        private void IntroTasks()
        {
            Debug.Log("IntroTasks !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            Task Start = new ActionTask(FadeOverlay());
            Task Wait2Secs = new WaitTask(2);
            Task FadeInStart = new FadeInStart(Context, Context._startMenu);
            Task StartMusic = new ActionTask(() => { /* maybe start music here? */ });

            Start.Then(Wait2Secs).Then(FadeInStart).Then(StartMusic);

            Context._taskManager.Do(Start);
        }

        private System.Action FadeOverlay()
        {
            return () => 
            {
                Debug.Log("FadeOverlay task !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                Context.HideUI(Context._startOverlay);
            };
        }

        private class FadeInStart : Task
        {
            private float gapBetweenFadeIns = 1f;
            private float elapsedTime = 0f;
            private enum StageToFadeInAtEndEnum { Title, NewGame, Continue, Quit };
            private StageToFadeInAtEndEnum curStage = StageToFadeInAtEndEnum.Title;
            private UIManager uim;
            private List<RectTransform> startMenuItems;

            public FadeInStart(UIManager uim, List<RectTransform> startMenuItems)
            {
                this.uim = uim;
                this.startMenuItems = startMenuItems;
            }

            internal override void Update()
            {
                elapsedTime += Time.deltaTime;

                if (elapsedTime >= gapBetweenFadeIns)
                {
                    if (curStage == StageToFadeInAtEndEnum.Continue && !Services.SaveManager.SaveExists()) curStage = StageToFadeInAtEndEnum.Quit; // Skip continue button.
                    if (curStage == StageToFadeInAtEndEnum.Quit) SetStatus(TaskStatus.Success);

                    CompleteIntermediateStage(startMenuItems[(int)curStage]);


                }
            }

            private void CompleteIntermediateStage(RectTransform rectTransform)
            {
                uim.DisplayUI(rectTransform);
                curStage++;
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

        public override void OnExit() => Context.HideUI(Context._pauseUI);
    }
    #endregion

}
