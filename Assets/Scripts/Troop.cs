using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Troop : MonoBehaviour {
    public delegate void DelegateAttackTroop(Troop other);
    public event DelegateAttackTroop EventAttackTroop;

    Unit generalUnit;
    List<Unit> unitList;
    

    int teamMorale;
    public int TeamMorale
    {
        get { return teamMorale; }
        set { teamMorale = value; }
    }

	void Start () {
        unitList = new List<Unit>();
	}

    void AddSoldier(Unit unit)
    {

    }

    void RemoveSoldier(Unit unit)
    {

    }



	
}
