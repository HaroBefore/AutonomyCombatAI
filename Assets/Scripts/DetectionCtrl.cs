using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathologicalGames;

using Haro;

public class DetectionCtrl : MonoBehaviour {

    AreaTargetTracker traker;
    Unit nearUnit = null;
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

    public float detectSightAngle = 90f;
    bool isInSight = false;
    public bool IsInSight
    {
        get { return isInSight; }
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
        traker = GetComponent<AreaTargetTracker>();
    }

    IEnumerator CheckDetection()
    {
        yield return new WaitForSeconds(delayOfAILoop);
        CheckRange();
        CheckSight();
        CheckNearUnit();
        StartCoroutine(CheckDetection());
    }

    void CheckRange()
    {
        isInRange = false;
        if (traker.targets.Count <= 0)
            return;
        isInRange = true;
    }

    void CheckSight()
    {
        foreach (var item in traker.targets)
        {
            if (item == null)
                break;


        }
    }

    void CheckNearUnit()
    {

    }


    
}
