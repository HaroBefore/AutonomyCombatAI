using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eUnitType
{
    None,
    Vegabond,
    Soldier,
    General
}

[RequireComponent(typeof(PhysicsMoveCtrl), typeof(StateMachine), typeof(DetectorCtrl))]
public class Unit : MonoBehaviour
{
    [SerializeField]
    eUnitType type;
    public eUnitType Type
    {
        get { return type; }
    }

    [SerializeField]
    int attackDamage;       //공격력
    public int AttackDamage
    {
        get { return attackDamage; }
        set { attackDamage = value; }
    }

    [SerializeField]
    int defensivePower;     //방어력
    public int DefensivePower
    {
        get { return defensivePower; }
        set { defensivePower = value; }
    }

    [SerializeField]
    int healthPoint;        //체력
    public int HealthPoint
    {
        get { return healthPoint; }
        set { healthPoint = value; }
    }
    [SerializeField]
    int maxHealthPoint;
    public int MaxHealthPoint
    {
        get { return maxHealthPoint; }
        set { maxHealthPoint = value; }
    }

    [SerializeField]
    int morale;             //사기
    public int Morale
    {
        get { return morale; }
        set { morale = value; }
    }
    [SerializeField]
    int maxMorale;
    public int MaxMorale
    {
        get { return maxMorale; }
        set { maxMorale = value; }
    }
    int joinTroopMind;
    public int JoinTroopMind
    {
        get { return joinTroopMind; }
        set { joinTroopMind = value; }
    }

    [SerializeField]
    MeshRenderer bodyRenderer = null;
    public MeshRenderer BodyRenderer
    {
        get { return bodyRenderer; }
    }

    [SerializeField]
    MeshRenderer headRenderer = null;
    public MeshRenderer HeadRenderer
    {
        get { return headRenderer; }
    }

    PhysicsMoveCtrl moveCtrl = null;
    public PhysicsMoveCtrl MoveCtrl
    {
        get { return moveCtrl; }
    }

    StateMachine stateMachine = null;
    public StateMachine StateMachine
    {
        get { return stateMachine; }
    }
    TroopCtrl troopCtrl = null;
    public TroopCtrl Troop
    {
        get { return troopCtrl; }
        set { troopCtrl = value; }
    }

    public void Init(eUnitType type)
    {
        transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        joinTroopMind = 0;
        switch (type)
        {
            case eUnitType.None:
                {
                    type = eUnitType.None;
                    name = tag = "Unit";
                    attackDamage = 1;
                    defensivePower = 1;
                    maxHealthPoint = healthPoint = 1;
                    maxMorale = morale = 1;

                    StateMachine.ChangeState(StateWander.Instance);
                }
                break;
            case eUnitType.Vegabond:
                {
                    type = eUnitType.Vegabond;
                    name = tag = "Vegabond";
                    attackDamage = Random.Range(1, 3);
                    defensivePower = Random.Range(1, 2);
                    maxHealthPoint = healthPoint = Random.Range(10, 15);
                    maxMorale = morale = Random.Range(20, 100);

                    StateMachine.ChangeState(StateWander.Instance);
                }
                break;
            case eUnitType.Soldier:
                {
                    type = eUnitType.Soldier;
                    name = tag = "Soldier";
                    attackDamage = Random.Range(2, 3);
                    defensivePower = Random.Range(1, 3);
                    maxHealthPoint = healthPoint = Random.Range(20, 30);
                    maxMorale = morale = Random.Range(50, 150);

                    StateMachine.ChangeState(StateWander.Instance);
                }
                break;
            case eUnitType.General:
                {
                    type = eUnitType.General;
                    name = tag = "General";
                    attackDamage = Random.Range(3, 5);
                    defensivePower = Random.Range(2, 4);
                    maxHealthPoint = healthPoint = Random.Range(30, 50);
                    maxMorale = morale = Random.Range(100, 200);

                    StateMachine.ChangeState(StateGeneralWander.Instance);
                    GeneralMakeNewTroop();
                }
                break;
            default:
                break;
        }
    }



    // Use this for initialization
    void Start()
    {
        moveCtrl = GetComponent<PhysicsMoveCtrl>();
        stateMachine = GetComponent<StateMachine>();
        bodyRenderer = transform.FindChild("Model").FindChild("Body").GetComponent<MeshRenderer>();
        headRenderer = transform.FindChild("Model").FindChild("Head").GetComponent<MeshRenderer>();
        //detectorCtrl = GetComponentInChildren<DetectorCtrl>();

        Manager.Instance.EventStartSimulation += OnStartSimulation;
        Manager.Instance.EventEndSimulation += OnEndSimulation;

        ////debug
        //StartCoroutine(CoInit());
    }

    public void OnStartSimulation()
    {
        StartCoroutine(CoInit());
    }

    public void OnEndSimulation()
    {

    }

    IEnumerator CoInit()
    {
        yield return new WaitForSeconds(0.5f);
        Init(type);
    }

    private void Update()
    {

    }

    public void SetAllFlagDeactive()
    {
        for (int i = 0; i < (int)eSteeringBehaviour.EndOfBehaviour; i++)
        {
            moveCtrl.Steering.SetFlagBehaviour((eSteeringBehaviour)i, false);
        }
    }

    public void SetFlagBehaviour(eSteeringBehaviour behaviour, bool isFlagOn)
    {
        moveCtrl.Steering.SetFlagBehaviour(behaviour, isFlagOn);
    }

    void GeneralMakeNewTroop()
    {
        if (type != eUnitType.General)
        {
            Debug.Log("장군만이 새 부대를 만들 수 있습니다.");
            return;
        }

        //이미 부대가 있다면
        if(troopCtrl != null)
        {
            troopCtrl.BreakupTroop();
        }

        troopCtrl = TroopCtrl.CreateNewTrop(this);
    }

}