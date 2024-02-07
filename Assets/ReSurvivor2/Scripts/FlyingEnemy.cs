using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEnemy : MonoBehaviour
{
    [SerializeField] FlyingEnemyController flyingEnemyController;
    public FlyingEnemyController FlyingEnemyController
    {
        get { return flyingEnemyController; }
        private set { flyingEnemyController = value; }
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

    // public void OnTriggerEnterHit1(Collider collider)
    // {
    //     if (collider.tag == "Player")
    //     {
    //         //Debug.Log("<color=red>オントリガーエンター</color>");
    //         //Debug.Log("<color=red>プレイヤーを発見!1</color>");
    //         flyingEnemyController.IsHit1 = true;

    //         //flyingEnemyController.IsMoveForward = true;
    //         flyingEnemyController.IsMoveBack = true;
    //     }
    // }

    // public void OnTriggerStayHit1(Collider collider)
    // {
    //     if (collider.tag == "Player")
    //     {
    //         //Debug.Log("<color=blue>オントリガーステイ</color>");
    //         //Debug.Log("<color=blue>プレイヤーを発見!1</color>");
    //         flyingEnemyController.IsHit1 = true;
    //     }
    // }

    // public void OnTriggerExitHit1(Collider collider)
    // {
    //     if (collider.tag == "Player")
    //     {
    //         //Debug.Log("<color=green>オントリガーイグジスト</color>");
    //         //Debug.Log("<color=green>プレイヤーを発見!1</color>");
    //         flyingEnemyController.IsHit1 = false;
    //     }
    // }

    // public void OnTriggerEnterHit2(Collider collider)
    // {
    //     if (collider.tag == "Player")
    //     {
    //         //Debug.Log("<color=red>オントリガーエンター</color>");
    //         //Debug.Log("<color=red>プレイヤーを発見!2</color>");
    //         flyingEnemyController.IsHit2 = true;
    //     }
    // }

    // public void OnTriggerStayHit2(Collider collider)
    // {
    //     if (collider.tag == "Player")
    //     {
    //         //Debug.Log("<color=blue>オントリガーステイ</color>");
    //         //Debug.Log("<color=blue>プレイヤーを発見!2</color>");
    //         flyingEnemyController.IsHit2 = true;
    //     }
    // }

    // public void OnTriggerExitHit2(Collider collider)
    // {
    //     if (collider.tag == "Player")
    //     {
    //         //Debug.Log("<color=green>オントリガーイグジスト</color>");
    //         //Debug.Log("<color=green>プレイヤーを発見!2</color>");
    //         flyingEnemyController.IsHit2 = false;
    //     }
    // }

    // public void OnTriggerEnterHit3(Collider collider)
    // {
    //     if (collider.tag == "Player")
    //     {
    //         //Debug.Log("<color=red>オントリガーエンター</color>");
    //         //Debug.Log("<color=red>プレイヤーを発見!3</color>");
    //         flyingEnemyController.IsHit3 = true;
    //     }
    // }

    // public void OnTriggerStayHit3(Collider collider)
    // {
    //     if (collider.tag == "Player")
    //     {
    //         //Debug.Log("<color=blue>オントリガーステイ</color>");
    //         //Debug.Log("<color=blue>プレイヤーを発見!3</color>");
    //         flyingEnemyController.IsHit3 = true;
    //     }
    // }

    // public void OnTriggerExitHit3(Collider collider)
    // {
    //     if (collider.tag == "Player")
    //     {
    //         //Debug.Log("<color=green>オントリガーイグジスト</color>");
    //         //Debug.Log("<color=green>プレイヤーを発見!3</color>");
    //         flyingEnemyController.IsHit3 = false;
    //     }
    // }

    // public void OnTriggerEnterHit4(Collider collider)
    // {
    //     if (collider.tag == "Player")
    //     {
    //         //Debug.Log("<color=red>オントリガーエンター</color>");
    //         //Debug.Log("<color=red>プレイヤーを発見!4</color>");
    //         flyingEnemyController.IsHit4 = true;

    //         flyingEnemyController.IsRotateToDirectionPlayer = true;
    //     }
    // }

    // public void OnTriggerStayHit4(Collider collider)
    // {
    //     if (collider.tag == "Player")
    //     {
    //         //Debug.Log("<color=blue>オントリガーステイ</color>");
    //         //Debug.Log("<color=blue>プレイヤーを発見!4</color>");
    //         flyingEnemyController.IsHit4 = true;
    //     }
    // }

    // public void OnTriggerExitHit4(Collider collider)
    // {
    //     if (collider.tag == "Player")
    //     {
    //         //Debug.Log("<color=green>オントリガーイグジスト</color>");
    //         //Debug.Log("<color=green>プレイヤーを発見!4</color>");
    //         flyingEnemyController.IsHit4 = false;
    //     }
    // }
}
