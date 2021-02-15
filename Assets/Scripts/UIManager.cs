using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    [SerializeField] private List<MaskableGraphic> DialogueUI;
    [SerializeField] private List<MaskableGraphic> PauseUI;

    #region Lifecycle Management
    private void Awake()
    {
        _fsm = new FiniteStateMachine<UIManager>(this);
    }

    void Start()
    {
        _fsm.TransitionTo<PlayState>();
    }

    private void Update()
    {
        _fsm.Update();
    }

    #endregion

    #region Triggers

    public void DisplayItemPickupPrompt()
    {

    }

    public void HideItemPickupPrompt()
    {

    }

    public void DisplayDialogueEnterPrompt()
    {

    }

    public void HideDialogueEnterPrompt()
    {

    }

    public void EnterDialogue()
    {
        _fsm.TransitionTo<InDialogueState>();
    }

    public void EnterPlay()
    {
        _fsm.TransitionTo<PlayState>();
    }

    #endregion

    #region States

    private abstract class UIState : FiniteStateMachine<UIManager>.State
    {
        public virtual void DisplayUI(List<MaskableGraphic> UI)
        {
            foreach (MaskableGraphic graphic in UI)
            {
                graphic.gameObject.SetActive(true);
            }
        }
        public virtual void HideUI(List<MaskableGraphic> UI)
        {
            foreach (MaskableGraphic graphic in UI)
            {
                graphic.gameObject.SetActive(false);
            }
        }
    }

    // Normal play-time UI. Usually nothing, except possibly map, compass, and prompts to enter conversation and interact with objects.
    private class PlayState : UIState
    {
        public override void OnEnter() { }

        public override void Update() { base.Update(); }

        public override void OnExit() { }
    }

    // Player is in dialogue.
    private class InDialogueState : UIState
    {
        public override void OnEnter() { DisplayUI(Context.DialogueUI); }

        public override void Update() { base.Update(); }

        public override void OnExit() { HideUI(Context.DialogueUI); }
    }

    // Pause UI.
    private class PauseState : UIState
    {
        public override void OnEnter() { DisplayUI(Context.PauseUI); }

        public override void Update() { base.Update(); }

        public override void OnExit() { HideUI(Context.PauseUI); }
    }
    #endregion

}
