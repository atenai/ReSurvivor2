using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeIndicator : MonoBehaviour
{
	Transform grenade;

	public void Init(GameObject gameObject)
	{
		grenade = gameObject.transform;
	}

	void Update()
	{
		if (grenade != null)
		{
			this.gameObject.SetActive(true);
			UnityEngine.Quaternion rot = Quaternion.LookRotation(grenade.position - Player.SingletonInstance.gameObject.transform.position);
			float angle = (PlayerCamera.SingletonInstance.gameObject.transform.eulerAngles - rot.eulerAngles).y;
			this.GetComponent<RectTransform>().localEulerAngles = new Vector3(0, 0, angle);
		}
		else
		{
			this.gameObject.SetActive(false);
		}
	}
}
