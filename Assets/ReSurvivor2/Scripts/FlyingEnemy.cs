using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    //センサーコライダー用の変数
    [SerializeField] ColliderEventHandler[] colliders = default;
    private bool[] hits;
    public bool GetHit(int index) => hits[index];

    void Start()
    {
        hits = new bool[colliders.Length];

        foreach (var i in colliders)
        {
            i.OnTriggerEnterEvent.AddListener(OnTriggerEnterHit);
            i.OnTriggerExitEvent.AddListener(OnTriggerExitHit);
        }

        target = Player.SingletonInstance.gameObject;
    }

    void Update()
    {

    }

    /// <summary>
    /// センサーコライダーの当たり判定(触れた時)
    /// </summary>
    void OnTriggerEnterHit(ColliderEventHandler self, Collider collider)
    {
        if (collider.tag == "Player")
        {
            int index = 0;
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] == self)
                {
                    index = i;
                }
            }

            // 当たったのはi番目のコリジョンだよ
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
            int index = 0;
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] == self)
                {
                    index = i;
                }
            }

            // 当たったのはi番目のコリジョンだよ
            hits[index] = false;
        }
    }
}
