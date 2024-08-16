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
    GameObject target;
    public GameObject Target => target;

    [SerializeField] Rigidbody rb;
    public Rigidbody Rigidbody => rb;

    //センサーコライダー用の変数
    [SerializeField] ColliderEventHandler[] colliders = default;
    private bool[] hits;
    public bool GetHit(int index) => hits[index];
    private Collider hitCollider = null;
    public Collider HitCollider => hitCollider;

    //ビヘイビアデザイナー用変数
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

    float rayDistance = 3.5f;

    [UnityEngine.Tooltip("現在のパトロールポイントのナンバー")]
    int patrolPointNumber = 0;
    public int PatrolPointNumber
    {
        get { return patrolPointNumber; }
        set { patrolPointNumber = value; }
    }

    [SerializeField] List<GameObject> patrolPoints = new List<GameObject>();
    public List<GameObject> PatrolPoints => patrolPoints;

    [SerializeField] GameObject centerPos;
    public GameObject CenterPos => centerPos;

    [SerializeField] Transform groundCheckCenter;
    [SerializeField] LayerMask groundLayers;
    [SerializeField] float groundedRadius = 0.2f;
    bool isGrounded = false;
    public bool IsGrounded => isGrounded;

    [UnityEngine.Tooltip("射撃距離")]
    [SerializeField] float shootingDistance = 4.0f;
    public float ShootingDistance => shootingDistance;

    [Header("デバッグ")]
    [Tooltip("デバッグモード")]
    [SerializeField] bool isDebugMode = true;
    [Tooltip("デバッグテキスト")]
    [SerializeField] List<TextMeshProUGUI> debugText = new List<TextMeshProUGUI>();


    void Start()
    {
        InitSensorCollider();

        Initialize();
    }

    /// <summary>
    /// センサーコライダーの初期化処理
    /// </summary> 
    void InitSensorCollider()
    {
        hits = new bool[colliders.Length];

        foreach (var i in colliders)
        {
            i.OnTriggerEnterEvent.AddListener(OnTriggerEnterHit);
            i.OnTriggerExitEvent.AddListener(OnTriggerExitHit);
        }
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

        ResetSensorCollider();

        //各種パラメーターを初期化
        isChase = false;
        patrolPointNumber = 0;
        isGrounded = false;

        GroundCheck();
    }

    /// <summary>
    /// センサーコライダーの状態をリセットする
    /// </summary>
    void ResetSensorCollider()
    {
        //コライダーが何も当たっていない状態にする
        if (hits != null)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                hits[i] = false;
            }
        }
        hitCollider = null;
    }

    /// <summary>
    /// 地面に接触しているか？をチェックする関数
    /// </summary>
    void GroundCheck()
    {
        Vector3 spherePosition = groundCheckCenter.transform.position;
        bool centerChecker = Physics.CheckSphere(spherePosition, groundedRadius, groundLayers, QueryTriggerInteraction.Ignore);
        isGrounded = centerChecker;
    }

    /// <summary>
    /// 地面に接触しているか？を可視化する関数
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (isGrounded == true)
        {
            //地面についている
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Gizmos.color = transparentGreen;
        }
        else
        {
            //地面についていない
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);
            Gizmos.color = transparentRed;
        }

        Gizmos.DrawSphere(new Vector3(groundCheckCenter.transform.position.x, groundCheckCenter.transform.position.y, groundCheckCenter.transform.position.z), groundedRadius);
    }

    void Update()
    {
        GroundCheck();

        Eyesight();
        alert.gameObject.SetActive(isChase);
        ChasePlayer();

        DebugText();
    }

    /// <summary>
    /// 視界
    /// </summary>
    void Eyesight()
    {
        Vector3 eyePos = new Vector3(this.transform.position.x, this.transform.position.y + 1.7f, this.transform.position.z);

        Ray ray = new Ray(eyePos, this.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, rayDistance))
        {
            if (hit.collider.tag == "Player")
            {
                //Debug.Log("<color=red>プレイヤーを発見!</color>");
                isChase = true;
                chaseCountTime = chaseTime;
            }
        }
        Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.red, 1);
    }

    /// <summary>
    /// プレイヤーを追跡中の処理
    /// </summary>
    void ChasePlayer()
    {
        if (isChase == true)
        {
            chaseCountTime -= Time.deltaTime;
            const float minTimer = 0.0f;
            if (chaseCountTime <= minTimer)
            {
                isChase = false;
            }
        }
    }

    /// <summary>
    /// デバッグテキスト
    /// </summary> 
    void DebugText()
    {
        if (isDebugMode == false)
        {
            return;
        }

        debugText[5].text = "chaseCountTime : " + chaseCountTime.ToString();
        debugText[4].text = "isChase : " + isChase.ToString();

        debugText[3].text = "patrolPointNumber : " + patrolPointNumber.ToString();
        debugText[2].text = "isGrounded : " + isGrounded.ToString();


        if (hitCollider != null)
        {
            debugText[1].text = "hitCollider : " + hitCollider.ToString();
        }
        else
        {
            debugText[1].text = "hitCollider : " + "null";
        }
        debugText[0].text = "hits[0] : " + hits[0].ToString();
    }

    void FixedUpdate()
    {

    }

    /// <summary>
    /// センサーコライダーの当たり判定(触れた時)
    /// </summary>
    void OnTriggerEnterHit(ColliderEventHandler self, Collider collider)
    {
        if (collider.tag == "Player")
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
        if (collider.tag == "Player")
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
    }

    /// <summary>
    /// 地面に接触しているか？を判別する当たり判定(触れた時)
    /// </summary>
    public void OnTriggerEnterHitCliff(Collider collider)
    {
        //Debug.Log("<color=red>OnTriggerEnterHitGround</color>");
    }

    /// <summary>
    /// 地面に接触しているか？を判別する当たり判定(離れた時)
    /// </summary>
    public void OnTriggerExitHitCliff(Collider collider)
    {
        //Debug.Log("<color=green>OnTriggerExitHitGround</color>");
    }
}
