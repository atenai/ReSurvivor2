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
    public GameObject Alert => alert;

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

    [NonSerialized] public float jumpWaitCount = 0.0f;
    //ビヘイビアデザイナー用変数

#if UNITY_EDITOR
    [SerializeField] TextMeshProUGUI debugText0;
    [SerializeField] TextMeshProUGUI debugText1;
    [SerializeField] TextMeshProUGUI debugText2;
    [SerializeField] TextMeshProUGUI debugText3;
    [SerializeField] TextMeshProUGUI debugText4;
    [SerializeField] TextMeshProUGUI debugText5;
    [SerializeField] TextMeshProUGUI debugText6;
    [SerializeField] TextMeshProUGUI debugText7;
#endif

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

        //コライダーが何も当たっていない状態にする
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

        GroundCheck();
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

#if UNITY_EDITOR
        DebugText();
#endif
    }

    void DebugText()
    {
        debugText7.text = "jumpWaitCount : " + jumpWaitCount.ToString();
        debugText6.text = "isGrounded : " + isGrounded.ToString();
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
