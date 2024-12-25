using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyItem1 : MonoBehaviour
{
    void Start()
    {
        if (InGameManager.SingletonInstance.KeyItem1 == 1)
        {
            Destroy(this.gameObject);
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Player"))
        {
            InGameManager.SingletonInstance.KeyItem1 = 1;//0 = false , 1 = true

            Destroy(this.gameObject);//このオブジェクトを削除            
        }
    }
}
