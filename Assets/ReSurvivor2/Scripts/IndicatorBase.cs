using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public class IndicatorBase : MonoBehaviour
{
	[SerializeField] Transform Enemy;
	[SerializeField] RectTransform Indicator;

	void Update()
	{
		if (Enemy != null)
		{
			Indicator.gameObject.SetActive(true);
			UnityEngine.Quaternion rot = Quaternion.LookRotation(Enemy.position - Player.SingletonInstance.gameObject.transform.position);
			float angle = (PlayerCamera.SingletonInstance.gameObject.transform.eulerAngles - rot.eulerAngles).y;
			Indicator.localEulerAngles = new Vector3(0, 0, angle);
		}
		else
		{
			Indicator.gameObject.SetActive(false);
		}
	}
}
