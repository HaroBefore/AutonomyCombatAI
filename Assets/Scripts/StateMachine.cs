using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour {
    Unit unit;
    public Unit UnitInstance
    {
        get { return unit; }
    }

    [SerializeField]
    IUnitState currentState = null;
    public IUnitState CurrentState
    {
        get { return currentState; }
        set { currentState = value; }
    }
    [SerializeField]
    IUnitState previousState = null;
    public IUnitState PreviousState
    {
        get { return previousState; }
        set { previousState = value; }
    }
    [SerializeField]
    IUnitState globalState = null;
    public IUnitState GlobalState
    {
        get { return globalState; }
        set { globalState = value; }
    }

	// Use this for initialization
	void Start () {
        unit = GetComponent<Unit>();
	}

    private void FixedUpdate()
    {
        if (globalState != null)
            globalState.Execuete(unit);
        if (currentState != null)
            currentState.Execuete(unit);
    }

    public void ChangeState(IUnitState newState)
    {
        previousState = currentState;
        if(currentState != null)
        {
            currentState.Exit(unit);
        }
        currentState = newState;
        currentState.Enter(unit);
    }

    public void RevertToPreviousState()
    {
        ChangeState(previousState);
    }
}
