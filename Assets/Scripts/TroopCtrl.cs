using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TroopCtrl : MonoBehaviour {
    public delegate void DelegateAttackTroop(TroopCtrl other);
    public event DelegateAttackTroop EventAttackTroop;

    new SphereCollider collider = null;

    public static int globalTroopIndex = 0;
    public int troopIdx = -1;

    Material troopMateral;
    public Material TroopMateral
    {
        get { return troopMateral; }
    }

    Unit nearUnit = null;
    List<Unit> detectionUnitList = null;

    Unit generalUnit;
    List<Unit> soldierUnitList = null;

    int soldierCnt;
    public int SoliderCnt
    {
        get { return soldierCnt; }
    }

    int teamMorale;
    public int TeamMorale
    {
        get { return teamMorale; }
        set { teamMorale = value; }
    }

    public Unit NearUnit
    {
        get { return nearUnit; }
    }

    public float delayOfAILoop = 0.1f;

    [SerializeField]
    float detectRangeDistence = 5f;
    public float DetectRangeDistance
    {
        get { return detectRangeDistence; }
        set
        {
            detectRangeDistence = value;
            //collider.radius = detectRangeDistence;
            transform.localScale = Vector3.one * detectRangeDistence;
        }
    }
    //public float startDetectRangeDistance = 5f;
    float addDetectRange = 0f;
    Vector3 detectRangeScale;
    float startDetectRangeDistance = 0f;


    bool isEnemyInRange = false;
    public bool IsEnemyInRange
    {
        get { return isEnemyInRange; }
    }

    private void OnEnable()
    {
        StartCoroutine(CheckDetection());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private void Awake()
    {
        Manager manager = Manager.Instance;

        //
        troopIdx = globalTroopIndex++;
        //

        detectionUnitList = new List<Unit>();
        soldierUnitList = new List<Unit>();
        collider = GetComponent<SphereCollider>();
        //collider.radius = detectRangeDistence;
        ///
        transform.localScale = detectRangeScale = Vector3.one * DetectRangeDistance;
        startDetectRangeDistance = DetectRangeDistance;
        ///
        //troopColor = new Color(Random.Range(0.1f, 0.9f), Random.Range(0.1f, 0.9f), Random.Range(0.1f, 0.9f));
        troopMateral = new Material(manager.soldierMaterial);
        troopMateral.color = manager.troopColorList[manager.troopColorIdx++];
        if (manager.troopColorIdx == manager.troopColorList.Count)
            manager.troopColorIdx = 0;

        //
        manager.arrTroop[troopIdx] = this;
        Color color = troopMateral.color;
        color.a = 1f;
        manager.arrTextTroop[troopIdx].color = color;
        //
    }

    IEnumerator CheckDetection()
    {
        yield return new WaitForSeconds(delayOfAILoop);
        soldierCnt = soldierUnitList.Count;
        CheckIsInRange();
        CheckNearUnit();
        RecruitSoldier();
        StartCoroutine(CheckDetection());
    }

    void CheckIsInRange()
    {
        isEnemyInRange = nearUnit != null ? true : false;
    }

    void CheckNearUnit()
    {
        nearUnit = null;
        if (detectionUnitList.Count <= 0)
            return;

        float nearDistance = 1000f;
        for (int i = 0; i < detectionUnitList.Count; i++)
        {
            float dist = Vector3.Distance(transform.position, detectionUnitList[i].transform.position);
            if (dist < nearDistance)
            {
                if (detectionUnitList[i].Type != eUnitType.Vegabond)
                    continue;
                nearDistance = dist;
                nearUnit = detectionUnitList[i];
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Vegabond"))
        {
            detectionUnitList.Add(other.GetComponent<Unit>());
        }

        if(other.CompareTag("Troop"))
        {

        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Vegabond"))
        {
            detectionUnitList.Remove(other.GetComponent<Unit>());
        }
    }

    public static TroopCtrl CreateNewTrop(Unit general)
    {
        if (general.Type != eUnitType.General)
        {
            Debug.Log("장군만이 새 부대를 만들 수 있습니다.");
            return null;
        }

        Debug.Log(Manager.Instance.prefabTroop);
        GameObject troopObject = Instantiate(Manager.Instance.prefabTroop, general.transform.position, general.transform.rotation);
        troopObject.name = "Troop";
        TroopCtrl troop = troopObject.GetComponent<TroopCtrl>();
        troop.transform.parent = general.transform;
        general.Troop = general.MoveCtrl.Troop = troop;

        troop.generalUnit = general;

        troop.teamMorale = general.Morale;

        general.HeadRenderer.material = troop.TroopMateral;

        return troop;
    }

    //부대관리
    public void BreakupTroop()
    {
        for (int i = 0; i < soldierUnitList.Count;)
        {
            soldierUnitList[i].Init(eUnitType.Vegabond);
            RemoveSoldier(soldierUnitList[i]);
            i = 0;
        }
    }

    void RecruitSoldier()
    {
        if (nearUnit == null)
            return;
        if (nearUnit.Type == eUnitType.Soldier)
        {
            nearUnit = null;
            return;
        }
        nearUnit.JoinTroopMind += (int)(teamMorale * 0.05f);
        if(nearUnit.JoinTroopMind > nearUnit.Morale)
        {
            nearUnit.BodyRenderer.material = TroopMateral;
            AddSoldier(nearUnit);
            detectionUnitList.Remove(nearUnit);
        }
    }

    public void AddSoldier(Unit unit)
    {
        Debug.Log("유닛 추가됨");
        unit.Init(eUnitType.Soldier);
        soldierUnitList.Add(unit);
        addDetectRange = soldierUnitList.Count * 0.1f;
        DetectRangeDistance = startDetectRangeDistance + addDetectRange;
    }

    public void RemoveSoldier(Unit unit)
    {
        Debug.Log("유닛 삭제됨");
        soldierUnitList.Remove(unit);
        addDetectRange = soldierUnitList.Count * 0.05f;
        DetectRangeDistance -= startDetectRangeDistance + addDetectRange;

    }


}
