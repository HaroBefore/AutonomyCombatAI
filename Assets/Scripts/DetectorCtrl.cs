using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathologicalGames;

public class DetectorCtrl : MonoBehaviour {

    List<Unit> unitList = null;
    Unit nearUnit = null;
    new SphereCollider collider = null;

    public Unit NearUnit
    {
        get { return nearUnit; }
    }

    public float delayOfAILoop = 0.1f;

    public float detectRangeDistence = 5f;
    bool isInRange = false;
    public bool IsInRange
    {
        get { return isInRange; }
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
        unitList = new List<Unit>();
        collider = GetComponent<SphereCollider>();
        collider.radius = detectRangeDistence;
    }

    IEnumerator CheckDetection()
    {
        yield return new WaitForSeconds(delayOfAILoop);
        CheckIsInRange();
        CheckNearUnit();
        StartCoroutine(CheckDetection());
    }

    void CheckIsInRange()
    {
        isInRange = nearUnit != null ? true : false;
    }

    void CheckNearUnit()
    {
        nearUnit = null;
        if (unitList.Count <= 0)
            return;

        float nearDistance = 1000f;
        for (int i = 0; i < unitList.Count; i++)
        {
            float dist = Vector3.Distance(transform.position, unitList[i].transform.position);
            if(dist < nearDistance)
            {
                nearDistance = dist;
                nearUnit = unitList[i];
            }
        }
        Debug.Log("NearUnit : " + nearUnit);
        Debug.Log(nearUnit + " / " + isInRange + " / " + nearUnit.name);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Vegabond"))
        {
            unitList.Add(other.GetComponent<Unit>());
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Vegabond"))
        {
            unitList.Remove(other.GetComponent<Unit>());
        }
    }

}
