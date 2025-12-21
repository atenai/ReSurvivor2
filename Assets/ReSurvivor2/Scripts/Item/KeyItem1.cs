using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// キーアイテム1
/// </summary>
public class KeyItem1 : MonoBehaviour
{
	void Start()
	{
		if (InGameManager.SingletonInstance.KeyItem1 == true)
		{
			Destroy(this.gameObject);
		}
	}

	void OnTriggerEnter(Collider collider)
	{
		if (collider.CompareTag("Player"))
		{
			InGameManager.SingletonInstance.KeyItem1 = true;
			Destroy(this.gameObject);//このオブジェクトを削除            
		}
	}
}
