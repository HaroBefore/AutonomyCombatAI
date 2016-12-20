using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Manager : MonoBehaviour {

    public delegate void DelegateStartSimulation();
    public event DelegateStartSimulation EventStartSimulation;
    public delegate void DelegateEndSimulation();
    public event DelegateEndSimulation EventEndSimulation;

    static Manager instance = null;
    public static Manager Instance
    {
        get
        {
            return instance;
        }
    }

    public GameObject prefabTroop = null;

    public Material unitMaterial = null;
    public Material vegabondMaterial = null;
    public Material soldierMaterial = null;
    public Material generalMaterial = null;

    public List<Color> troopColorList = null;
    public int troopColorIdx = 0;

    public GameObject prefabGeneral = null;
    [Range(1, 10)]
    public int generalCnt = 4;

    [SerializeField]
    int timer = 120;
    public int TImer
    {
        get { return timer; }
        set
        {
            timer = value;
            textTimer.text = timer.ToString();
        }
    }


    public TroopCtrl[] arrTroop;
    //UI]
    public Text textTimer;
    public Text textLastWinner;
    public GameObject btnStartSimulationObject;
    public GameObject[] arrTextTroopObject;
    public Text[] arrTextTroop;


    private void Awake()
    {
        instance = this;
        arrTroop = new TroopCtrl[10];
        arrTextTroopObject = new GameObject[10];
        arrTextTroop = new Text[10];
    }

    private void Start()
    {
        textLastWinner = GameObject.Find("TextLastWinner").GetComponent<Text>();
        textTimer = GameObject.Find("TextTimer").GetComponent<Text>();
        TImer = TImer;
        for (int i = 0; i < arrTextTroop.Length; i++)
        {
            arrTextTroopObject[i] = GameObject.Find(string.Format("TextTroop{0}", i));
            arrTextTroop[i] = arrTextTroopObject[i].GetComponent<Text>();
            arrTextTroopObject[i].SetActive(false);
        }

        generalCnt = Random.Range(3, 6);
        GameObject generals = GameObject.Find("Generals");
        //x -40 ~ 40 / z -30 ~ 30
        for (int i = 0; i < generalCnt; i++)
        {
            GameObject general = Instantiate(prefabGeneral, new Vector3(Random.Range(-40f, 40f), 0f, Random.Range(-30f, 30f)), Quaternion.Euler(0f, Random.Range(0f, 360f), 0f)) as GameObject;
            general.transform.parent = general.transform;
        }

        EventStartSimulation += OnStartSimulation;
        EventEndSimulation += OnEndSimulation;
    }

    private void Update()
    {
        for (int i = 0; i < arrTroop.Length; i++)
        {
            if (arrTroop[i] == null)
                break;
            arrTextTroop[i].text = string.Format("Troop {0} : {1}", arrTroop[i].troopIdx, arrTroop[i].SoliderCnt);
        }
    }

    public void OnButtonDownStartSimulation()
    {
        EventStartSimulation();
    }

    void OnStartSimulation()
    {
        StartCoroutine(CoTimer());
        btnStartSimulationObject.SetActive(false);
        for (int i = 0; i < generalCnt; i++)
        {
            arrTextTroopObject[i].SetActive(true);
        }
    }

    public void OnButtonDownReset()
    {
        TroopCtrl.globalTroopIndex = 0;
        SceneManager.LoadScene(0);
    }

    IEnumerator CoTimer()
    {
        yield return new WaitForSeconds(1f);
        TImer--;
        if (timer <= 0)
            EventEndSimulation();
        else
            StartCoroutine(CoTimer());
    }

    void OnEndSimulation()
    {
        TroopCtrl winner = null;
        int maxSoldier = 0;
        for (int i = 0; i < arrTroop.Length; i++)
        {
            if (arrTroop[i] == null)
                break;
            if(maxSoldier < arrTroop[i].SoliderCnt)
            {
                maxSoldier = arrTroop[i].SoliderCnt;
                winner = arrTroop[i];
            }
        }

        textLastWinner.text = string.Format("승자 : {0} / 병사수 : {1}", winner.troopIdx, winner.SoliderCnt);
        Time.timeScale = 0f;
    }
}
