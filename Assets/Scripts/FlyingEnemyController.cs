using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FlyingEnemyController
{
    [SerializeField] GameObject target;
    public GameObject Target
    {
        get { return target; }
        set { target = value; }
    }

    [SerializeField] Rigidbody rb;
    public Rigidbody Rigidbody
    {
        get { return rb; }
        private set { rb = value; }
    }

    bool isHit1;
    public bool IsHit1
    {
        get { return isHit1; }
        set { isHit1 = value; }
    }

    bool isHit2;
    public bool IsHit2
    {
        get { return isHit2; }
        set { isHit2 = value; }
    }

    bool isHit3;
    public bool IsHit3
    {
        get { return isHit3; }
        set { isHit3 = value; }
    }

    bool isHit4;
    public bool IsHit4
    {
        get { return isHit4; }
        set { isHit4 = value; }
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

}