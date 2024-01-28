using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnemyController
{
    [SerializeField] GameObject target;
    public GameObject Target
    {
        get { return target; }
        private set { target = value; }
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

    [SerializeField] float jumpForce = 20.0f;
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
}
