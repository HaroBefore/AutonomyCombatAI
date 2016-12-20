using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateSoldierWander : IUnitState
{
    static StateSoldierWander instance = null;
    public static StateSoldierWander Instance
    {
        get
        {
            if (instance == null)
                instance = new StateSoldierWander();
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
        Debug.Log("General 배회중");
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
        unit.SetAllFlagDeactive();
    }
}
