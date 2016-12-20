using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateGeneralWander : IUnitState
{
    static StateGeneralWander instance = null;
    public static StateGeneralWander Instance
    {
        get
        {
            if (instance == null)
                instance = new StateGeneralWander();
            return instance;
        }
    }

    public void Enter(Unit unit)
    {
        unit.SetFlagBehaviour(eSteeringBehaviour.wander, true);
        unit.SetFlagBehaviour(eSteeringBehaviour.obstacleAvoidance, true);
        unit.SetFlagBehaviour(eSteeringBehaviour.wallAvoidance, true);
    }

    public void Execuete(Unit unit)
    {
        if(unit.Troop == null)
        {
            return;
        }
        if(unit.Troop.IsEnemyInRange)
        {
            unit.StateMachine.ChangeState(StateGeneralPursuit.Instance);
        }
    }

    public void Exit(Unit unit)
    {

    }
}
