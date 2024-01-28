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

    void Start()
    {

    }

    void Update()
    {

    }

    public void OnTriggerEnterHit1(Collider collider)
    {
        if (collider.tag == "Player")
        {
            //Debug.Log("<color=red>オントリガーエンター</color>");
            //Debug.Log("<color=red>プレイヤーを発見!1</color>");
            flyingEnemyController.IsHit1 = true;

            flyingEnemyController.IsMoveForward = true;
            //flyingEnemyController.IsMoveBack = true;
        }
    }

    public void OnTriggerStayHit1(Collider collider)
    {
        if (collider.tag == "Player")
        {
            //Debug.Log("<color=blue>オントリガーステイ</color>");
            //Debug.Log("<color=blue>プレイヤーを発見!1</color>");
            flyingEnemyController.IsHit1 = true;
        }
    }

    public void OnTriggerExitHit1(Collider collider)
    {
        if (collider.tag == "Player")
        {
            //Debug.Log("<color=green>オントリガーイグジスト</color>");
            //Debug.Log("<color=green>プレイヤーを発見!1</color>");
            flyingEnemyController.IsHit1 = false;
        }
    }

    public void OnTriggerEnterHit2(Collider collider)
    {
        if (collider.tag == "Player")
        {
            //Debug.Log("<color=red>オントリガーエンター</color>");
            //Debug.Log("<color=red>プレイヤーを発見!2</color>");
            flyingEnemyController.IsHit2 = true;
        }
    }

    public void OnTriggerStayHit2(Collider collider)
    {
        if (collider.tag == "Player")
        {
            //Debug.Log("<color=blue>オントリガーステイ</color>");
            //Debug.Log("<color=blue>プレイヤーを発見!2</color>");
            flyingEnemyController.IsHit2 = true;
        }
    }

    public void OnTriggerExitHit2(Collider collider)
    {
        if (collider.tag == "Player")
        {
            //Debug.Log("<color=green>オントリガーイグジスト</color>");
            //Debug.Log("<color=green>プレイヤーを発見!2</color>");
            flyingEnemyController.IsHit2 = false;
        }
    }

    public void OnTriggerEnterHit3(Collider collider)
    {
        if (collider.tag == "Player")
        {
            //Debug.Log("<color=red>オントリガーエンター</color>");
            //Debug.Log("<color=red>プレイヤーを発見!3</color>");
            flyingEnemyController.IsHit3 = true;
        }
    }

    public void OnTriggerStayHit3(Collider collider)
    {
        if (collider.tag == "Player")
        {
            //Debug.Log("<color=blue>オントリガーステイ</color>");
            //Debug.Log("<color=blue>プレイヤーを発見!3</color>");
            flyingEnemyController.IsHit3 = true;
        }
    }

    public void OnTriggerExitHit3(Collider collider)
    {
        if (collider.tag == "Player")
        {
            //Debug.Log("<color=green>オントリガーイグジスト</color>");
            //Debug.Log("<color=green>プレイヤーを発見!3</color>");
            flyingEnemyController.IsHit3 = false;
        }
    }

    public void OnTriggerEnterHit4(Collider collider)
    {
        if (collider.tag == "Player")
        {
            //Debug.Log("<color=red>オントリガーエンター</color>");
            //Debug.Log("<color=red>プレイヤーを発見!4</color>");
            flyingEnemyController.IsHit4 = true;

            flyingEnemyController.IsRotateToDirectionPlayer = true;
        }
    }

    public void OnTriggerStayHit4(Collider collider)
    {
        if (collider.tag == "Player")
        {
            //Debug.Log("<color=blue>オントリガーステイ</color>");
            //Debug.Log("<color=blue>プレイヤーを発見!4</color>");
            flyingEnemyController.IsHit4 = true;
        }
    }

    public void OnTriggerExitHit4(Collider collider)
    {
        if (collider.tag == "Player")
        {
            //Debug.Log("<color=green>オントリガーイグジスト</color>");
            //Debug.Log("<color=green>プレイヤーを発見!4</color>");
            flyingEnemyController.IsHit4 = false;
        }
    }
}
