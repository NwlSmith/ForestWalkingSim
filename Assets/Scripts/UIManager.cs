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

    [SerializeField] private List<RectTransform> _pickupItemUI;
    [SerializeField] private List<RectTransform> _dialogueEnterPromptUI;

    [SerializeField] private List<RectTransform> _dialogueUI;
    [SerializeField] private List<RectTransform> _pauseUI;


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

    #endregion

    #region Utilities
    private void DisplayUI(List<RectTransform> UI)
    {
        foreach (RectTransform graphic in UI)
        {
            graphic.gameObject.SetActive(true);
        }
    }
    private void HideUI(List<RectTransform> UI)
    {
        foreach (RectTransform graphic in UI)
        {
            graphic.gameObject.SetActive(false);
        }
    }

    private void HideAllUI()
    {
        HideUI(_pickupItemUI);
        HideUI(_dialogueEnterPromptUI);
        HideUI(_dialogueUI);
        HideUI(_pauseUI);
    }
    #endregion

    #region States

    private abstract class UIState : FiniteStateMachine<UIManager>.State
    {
        
    }

    // Normal play-time UI. Usually nothing, except possibly map, compass, and prompts to enter conversation and interact with objects.
    private class PlayState : UIState
    {
        public override void OnEnter() { }

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
