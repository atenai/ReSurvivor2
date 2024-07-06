using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyCollider : MonoBehaviour
{
    GameObject target;
    public GameObject Target
    {
        get { return target; }
        set { target = value; }
    }

    [SerializeField] float moveSpeed = 2.0f;
    public float MoveSpeed
    {
        get { return moveSpeed; }
        private set { moveSpeed = value; }
    }

    [SerializeField] float rotationSpeed = 1.0f;
    public float RotationSpeed
    {
        get { return rotationSpeed; }
        private set { rotationSpeed = value; }
    }

    [SerializeField] Rigidbody rb;
    public Rigidbody Rigidbody
    {
        get { return rb; }
        private set { rb = value; }
    }

    [SerializeField] float jumpForce = 250.0f;
    public float JumpForce
    {
        get { return jumpForce; }
        private set { jumpForce = value; }
    }

    [SerializeField] GameObject[] patrolPoints;
    public GameObject[] PatrolPoints
    {
        get { return patrolPoints; }
        private set { patrolPoints = value; }
    }

    [SerializeField] GameObject alert;
    public GameObject Alert
    {
        get { return alert; }
        private set { alert = value; }
    }

    bool isChase = false;
    public bool IsChase
    {
        get { return isChase; }
        set { isChase = value; }
    }

    [SerializeField] float chaseTime = 100.0f;
    public float ChaseTime
    {
        get { return chaseTime; }
        private set { chaseTime = value; }
    }

    float countTime;
    public float CountTime
    {
        get { return countTime; }
        set { countTime = value; }
    }

    [SerializeField] GameObject centerPos;
    public GameObject CenterPos
    {
        get { return centerPos; }
        private set { centerPos = value; }
    }

    float count = 0;
    bool isOnce = false;

    bool isHit;
    public bool IsHit
    {
        get { return isHit; }
        private set { isHit = value; }
    }

    void Start()
    {
        target = Player.SingletonInstance.gameObject;
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
            Rigidbody.AddForce(new Vector3(0, jumpForce, 0));
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
                Rigidbody.AddForce(new Vector3(0, jumpForce * 2, 0));
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
