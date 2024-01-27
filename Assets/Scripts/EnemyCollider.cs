using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCollider : MonoBehaviour
{
    [SerializeField] EnemyController enemyController;
    public EnemyController EnemyController
    {
        get { return enemyController; }
        set { enemyController = value; }
    }

    float count = 0;
    bool isOnce = false;
    float jumpForce = 250.0f;

    bool isHit;
    public bool IsHit
    {
        get { return isHit; }
        private set { isHit = value; }
    }

    void Start()
    {

    }


    void Update()
    {

    }

    public void OnTriggerEnterHitObject(Collider collider)
    {
        //Debug.Log("<color=red>OnTriggerEnterHitObject</color>");

        if (collider.tag == "Object")
        {
            //Debug.Log("<color=red>オブジェクトを発見! count : " + count + "</color>");
            enemyController.Rigidbody.AddForce(new Vector3(0, jumpForce, 0));
        }
    }

    public void OnTriggerStayHitObject(Collider collider)
    {
        //Debug.Log("<color=blue>OnTriggerStayHitObject</color>");

        if (collider.tag == "Object")
        {
            count = count + (Time.deltaTime * 10.0f);
            //Debug.Log("<color=blue>オブジェクトを発見! count : " + count + "</color>");
            if (10.0f <= count && isOnce == false)
            {
                //Debug.Log("<color=blue>ジャンプ！</color>");
                isOnce = true;
                enemyController.Rigidbody.AddForce(new Vector3(0, jumpForce * 2, 0));
                //将来的には次の巡回ポイントに移動する処理を呼び出す方が良いと思われる
            }

        }
    }

    public void OnTriggerExitHitObject(Collider collider)
    {
        //Debug.Log("<color=green>OnTriggerExitHitObject</color>");

        if (collider.tag == "Object")
        {
            count = 0;
            isOnce = false;
            //Debug.Log("<color=green>オブジェクトを発見! count : " + count + "</color>");
        }
    }

    public void OnTriggerEnterHitPlayer(Collider collider)
    {
        //Debug.Log("<color=red>OnTriggerEnterHitPlayer</color>");

        if (collider.tag == "Player")
        {
            //Debug.Log("<color=red>プレイヤーを発見!</color>");
            isHit = true;
        }
    }

    public void OnTriggerStayHitPlayer(Collider collider)
    {
        //Debug.Log("<color=blue>OnTriggerStayHitPlayer</color>");

        if (collider.tag == "Player")
        {
            //Debug.Log("<color=blue>プレイヤーを発見!</color>");
            isHit = true;
        }
    }

    public void OnTriggerExitHitPlayer(Collider collider)
    {
        //Debug.Log("<color=green>OnTriggerExitHitPlayer</color>");

        if (collider.tag == "Player")
        {
            //Debug.Log("<color=green>プレイヤーを発見!</color>");
            isHit = false;
        }
    }
}
