using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public class Indicator : MonoBehaviour
{
	Transform enemy;

	public void Init(GameObject gameObject)
	{
		enemy = gameObject.transform;
	}

	void Update()
	{
		if (enemy != null)
		{
			this.gameObject.SetActive(true);
			UnityEngine.Quaternion rot = Quaternion.LookRotation(enemy.position - Player.SingletonInstance.gameObject.transform.position);
			float angle = (PlayerCamera.SingletonInstance.gameObject.transform.eulerAngles - rot.eulerAngles).y;
			this.GetComponent<RectTransform>().localEulerAngles = new Vector3(0, 0, angle);
		}
		else
		{
			this.gameObject.SetActive(false);
		}
	}
}
