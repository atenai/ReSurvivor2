using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRayCast : MonoBehaviour
{
    [SerializeField] EnemyController enemyController;
    public EnemyController EnemyController
    {
        get { return enemyController; }
        set { enemyController = value; }
    }

    void Start()
    {
        enemyController.Target = Player.SingletonInstance.gameObject;
    }


    void Update()
    {

    }
}
