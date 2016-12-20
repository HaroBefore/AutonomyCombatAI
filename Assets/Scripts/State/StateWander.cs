using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateWander : IUnitState
{
    static StateWander instance = null;
    public static StateWander Instance
    {
        get
        {
            if (instance == null)
                instance = new StateWander();
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

    }

    public void Exit(Unit unit)
    {
        unit.SetAllFlagDeactive();
    }
}
