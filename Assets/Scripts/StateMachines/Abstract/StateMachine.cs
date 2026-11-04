using UnityEngine;
using System;
using System.Collections.Generic;

public abstract class StateMachine<EState> : MonoBehaviour where EState : Enum
{

    protected Dictionary<EState, BaseState<EState>> States = new Dictionary<EState, BaseState<EState>>();

    protected BaseState<EState> currentState;

    private bool isTransitioningState = false;

    void Start()
    {
        currentState.EnterState();
    }

    void Update()
    {
        if (isTransitioningState) return;

        EState nextStateKey = currentState.GetNextState();

        if (nextStateKey.Equals(currentState.StateKey))
        {
            currentState.UpdateState();
        }
        else
        {
            TransitionToState(nextStateKey);
        }
        
    }

    void TransitionToState(EState targetState)
    {
        isTransitioningState = true;
        currentState.ExitState();
        currentState = States[targetState];
        currentState.EnterState();
        isTransitioningState = false;
    }

    void OnTriggerEnter(Collider other)
    {
        currentState.OnTriggerEnter(other);
    }

    void OnTriggerStay(Collider other)
    {
        currentState.OnTriggerStay(other);
    }

    void OnTriggerExit(Collider other)
    {
        currentState.OnTriggerExit(other);
    }
}
