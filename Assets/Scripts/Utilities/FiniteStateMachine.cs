using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/*
 * Creator: Jack Schlesinger
 * Date Created: 2/9/2021
 * Description: Finite State Machine base class
 * 
 * Useful for programming different behaviors based on the state of the object.
 * 
 * An example in Unity is the Animation system. One animation plays, the player triggers something, and that makes the animation transition to something else.
 * 
 * More complicated than EventManager to explain how to make it work.
 * 
 * Finite State Machines need to be declared in each class that you want to use one in. The Player will have one SFM, AI another, GameManager another.
 * 
 * Jack explaining this: https://www.youtube.com/watch?v=53pWHP9WR-o&list=PLE3rHjk-SWif4JAzb4FB48FPJIr3xXZUi&index=2 at time = about 1:54:00
 * Github: https://github.com/jackschlesinger/AGP_2021/blob/master/Projects/AGP_SoccerExample/Assets/
 * 
 * Advantages:
 * Super modular, easy to read, easy to conceptualize, EASIER to debug
 * Warnings:
 * You need to define every state, and every transition into and out of every state.
 * Cannot be in multiple states at once
 * Not a good solution for modern AI
 * 
 * Needs to be created and an initial state transitioned to in Start()
 * 
 * for example:
 * _fsm = new FiniteStateMachine<AIPlayer>(this);
 * _fsm.TransitionTo<Offense>();
 * 
 * Needs to be Updated in Update. - IF YOU WANT IT TO. If you only want things to update when an event happens, you can call Update during that event.
 * _fsm.Update();
 * 
 * You need to define the states in the code, their OnEnter, Update, and OnExit functions.
 * 
 * 
 */

public class FiniteStateMachine<TContext>
{
    private readonly TContext _context;
    private readonly Dictionary<System.Type, State> _stateCache = new Dictionary<System.Type, State>();
    public State CurrentState { get; private set; }
    public State PendingState { get; private set; }

    public FiniteStateMachine(TContext context)
    {
        _context = context;
    }

    public void Update()
    {
        PerformPendingTransition();

        Debug.Assert(CurrentState != null,
            "Updating FiniteStateMachine with null current state. Did you forget to transition to a starting state?");

        CurrentState.Update();

        PerformPendingTransition();
    }

    public void TransitionTo<TState>() where TState : State
    {
        PendingState = GetOrCreateState<TState>();
    }

    private void PerformPendingTransition()
    {
        if (PendingState == null) return;

        CurrentState?.OnExit();

        CurrentState = PendingState;

        CurrentState.OnEnter();

        PendingState = null;
    }

    public void ResetCurrentState()
    {
        CurrentState.OnEnter();
    }

    private TState GetOrCreateState<TState>() where TState : State
    {
        if (_stateCache.TryGetValue(typeof(TState), out var state))
        {
            return (TState)state;
        }

        var newState = System.Activator.CreateInstance<TState>();

        newState.Parent = this;

        newState.Initialize();

        _stateCache[typeof(TState)] = newState;

        return newState;
    }

    public void Destroy()
    {
        var states = _stateCache.Values;

        foreach (var state in states)
        {
            state.CleanUp();
            _stateCache.Remove(state.GetType());
        }
    }

    public void EndState<TState>()
    {
        if (!_stateCache.TryGetValue(typeof(TState), out var state)) return;

        state.CleanUp();
        _stateCache.Remove(typeof(TState));
    }

    public void EndAllButCurrentState()
    {
        var states = _stateCache.Values;

        foreach (var state in states.Where(state => state != CurrentState))
        {
            state.CleanUp();
            _stateCache.Remove(state.GetType());
        }
    }

    public abstract class State
    {
        internal FiniteStateMachine<TContext> Parent { get; set; }

        protected TContext Context => Parent._context;

        protected void TransitionTo<TState>() where TState : State
        {
            Parent.TransitionTo<TState>();
        }

        public virtual void Initialize() { }

        public virtual void OnEnter() { }

        public virtual void OnExit() { }

        public virtual void Update() { }

        public virtual void CleanUp() { }


    }



}