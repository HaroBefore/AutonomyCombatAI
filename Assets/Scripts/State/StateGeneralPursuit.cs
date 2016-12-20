using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateGeneralPursuit : IUnitState
{
    static StateGeneralPursuit instance = null;
    public static StateGeneralPursuit Instance
    {
        get
        {
            if (instance == null)
                instance = new StateGeneralPursuit();
            return instance;
        }
    }

    public void Enter(Unit unit)
    {
        unit.SetFlagBehaviour(eSteeringBehaviour.pursuit, true);
        unit.MoveCtrl.pursuitTarget = unit.Troop.NearUnit.MoveCtrl;
        unit.SetFlagBehaviour(eSteeringBehaviour.obstacleAvoidance, true);
        unit.SetFlagBehaviour(eSteeringBehaviour.wallAvoidance, true);
    }

    public void Execuete(Unit unit)
    {
        if (!unit.Troop.IsEnemyInRange)
        {
            unit.StateMachine.ChangeState(StateGeneralWander.Instance);
        }
    }

    public void Exit(Unit unit)
    {
        unit.SetAllFlagDeactive();
    }
}
