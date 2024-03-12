using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.AddressableAssets;

/// <summary>
/// 地上敵
/// </summary>
public class GroundEnemy : MonoBehaviour
{
    [SerializeField] ColliderEventHandler[] colliders = default;
    private bool[] hits;
    public bool GetHit(int index) => hits[index];
    private Collider hitCollider = null;
    public Collider HitCollider => hitCollider;

    //ビヘイビアデザイナーの条件判定用変数
    bool isChase = false;
    public bool IsChase
    {
        get { return isChase; }
        set { isChase = value; }
    }

    [SerializeField] float chaseTime = 10.0f;
    public float ChaseTime => chaseTime;

    float chaseCountTime;
    public float ChaseCountTime
    {
        get { return chaseCountTime; }
        set { chaseCountTime = value; }
    }

    [SerializeField] GameObject alert;
    public GameObject Alert => alert;
    //ビヘイビアデザイナーの条件判定用変数

    //ビヘイビアデザイナーのアクション用変数
    [UnityEngine.Tooltip("現在のパトロールポイントのナンバー")]
    int patrolPointNumber = 0;
    public int PatrolPointNumber
    {
        get { return patrolPointNumber; }
        set { patrolPointNumber = value; }
    }

    [SerializeField] List<GameObject> patrolPoints = new List<GameObject>();
    public List<GameObject> PatrolPoints => patrolPoints;
    //ビヘイビアデザイナーのアクション用変数

#if UNITY_EDITOR
    [SerializeField] TextMeshProUGUI debugText0;
    [SerializeField] TextMeshProUGUI debugText1;
    [SerializeField] TextMeshProUGUI debugText2;
    [SerializeField] TextMeshProUGUI debugText3;
    [SerializeField] TextMeshProUGUI debugText4;
    [SerializeField] TextMeshProUGUI debugText5;
#endif

    public GameObject centerPos;
    public GameObject CenterPos => centerPos;

    GameObject target;
    public GameObject Target => target;

    [SerializeField] Rigidbody rb;
    public Rigidbody Rigidbody => rb;

    void Start()
    {
        hits = new bool[colliders.Length];

        foreach (var i in colliders)
        {
            i.OnTriggerEnterEvent.AddListener(OnTriggerEnterHit);
            i.OnTriggerExitEvent.AddListener(OnTriggerExitHit);
        }

        Initialize();
    }

    /// <summary>
    /// リスポーンした際の初期化処理
    /// </summary>
    void Initialize()
    {
        if (target == null)
        {
            target = Player.SingletonInstance.gameObject;
        }

        if (rb == null)
        {
            rb = this.GetComponent<Rigidbody>();
        }

        //Debug.Log("<color=green>Enemy.Initialize2</color>");

        if (hits != null)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                hits[i] = false;
            }
        }
        hitCollider = null;

        isChase = false;
        alert.gameObject.SetActive(false);
    }

    void Update()
    {

#if UNITY_EDITOR
        DebugText();
#endif
    }

    void DebugText()
    {
        debugText5.text = "hits[1] : " + hits[1].ToString();
        debugText4.text = "hits[0] : " + hits[0].ToString();
        if (hitCollider != null)
        {
            debugText3.text = "hitCollider : " + hitCollider.ToString();
        }
        else
        {
            debugText3.text = "hitCollider : " + "null";
        }
        debugText2.text = "chaseCountTime : " + chaseCountTime.ToString();
        debugText1.text = "isChase : " + isChase.ToString();
        debugText0.text = "patrolPointNumber : " + patrolPointNumber.ToString();
    }

    void FixedUpdate()
    {

    }

    /// <summary>
    /// センサーコライダーの当たり判定(触れた時)
    /// </summary>
    void OnTriggerEnterHit(ColliderEventHandler self, Collider collider)
    {
        if (collider.tag == "Player" || collider.tag == "Object")
        {
            if (collider != null)
            {
                //Debug.Log("<color=blue>プレイヤーを発見!2</color>");
                hitCollider = collider;
            }

            int index = 0;
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] == self)
                {
                    index = i;
                }
            }

            hits[index] = true;
        }
    }

    /// <summary>
    /// センサーコライダーの当たり判定(離れた時)
    /// </summary>
    void OnTriggerExitHit(ColliderEventHandler self, Collider collider)
    {
        if (collider.tag == "Player" || collider.tag == "Object")
        {
            if (collider != null)
            {
                hitCollider = collider;
            }

            int index = 0;
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] == self)
                {
                    index = i;
                }
            }

            hits[index] = false;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("Hit");
        if (collision.collider.tag == "Player")
        {

        }
    }
}
