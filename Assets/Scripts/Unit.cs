using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{

    int attackDamage;
    public int AttackDamage
    {
        get { return attackDamage; }
        set { attackDamage = value; }
    }
    int defensivePower;
    public int DefensivePower
    {
        get { return defensivePower; }
        set { defensivePower = value; }
    }
    int healthPoint;
    public int HealthPoint
    {
        get { return healthPoint; }
        set { healthPoint = value; }
    }
    int morale;
    public int Morale
    {
        get { return morale; }
        set { morale = value; }
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}