using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public class IndicatorTarget : MonoBehaviour
{
	void Start()
	{
		IndicatorManager.SingletonInstance.InstanceIndicator(this);
	}

	void OnDisable()
	{
		//敵表示マーカー削除
		IndicatorManager.SingletonInstance.DeleteIndicator(this);
	}
}
