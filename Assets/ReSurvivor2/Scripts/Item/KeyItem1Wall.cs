using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// キーアイテム1で開く壁
/// </summary> 
public class KeyItem1Wall : MonoBehaviour
{
    void Start()
    {
        if (InGameManager.SingletonInstance.KeyItem1 == 1)
        {
            Destroy(this.gameObject);
        }
    }

    void Update()
    {

    }
}
