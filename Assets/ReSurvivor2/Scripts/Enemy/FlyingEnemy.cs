using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 空中敵
/// </summary>
public class FlyingEnemy : MonoBehaviour
{
    GameObject target;
    public GameObject Target
    {
        get { return target; }
        private set { target = value; }
    }

    [SerializeField] Rigidbody rb;
    public Rigidbody Rigidbody
    {
        get { return rb; }
        private set { rb = value; }
    }

    //センサーコライダー用の変数
    [SerializeField] ColliderEventHandler[] colliders = default;
    private bool[] hits;
    public bool GetHit(int index) => hits[index];
    private Collider hitCollider = null;
    public Collider HitCollider => hitCollider;

    //ビヘイビアデザイナー用変数
    bool isRotateToDirectionPlayer = false;
    public bool IsRotateToDirectionPlayer
    {
        get { return isRotateToDirectionPlayer; }
        set { isRotateToDirectionPlayer = value; }
    }

    bool isMoveForward = false;
    public bool IsMoveForward
    {
        get { return isMoveForward; }
        set { isMoveForward = value; }
    }

    bool isMoveBack = false;
    public bool IsMoveBack
    {
        get { return isMoveBack; }
        set { isMoveBack = value; }
    }

    [SerializeField] GameObject alert;
    public GameObject Alert => alert;
    //ビヘイビアデザイナー用変数

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

        alert.gameObject.SetActive(false);
    }

    void Update()
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
}
