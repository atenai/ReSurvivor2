using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// インジケーター
/// </summary>
public class Indicator : MonoBehaviour
{
	[SerializeField] Image image;
	Transform enemy;
	Sequence sequence;

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

	public void Show()
	{
		sequence.Kill();
		sequence.Append(image.DOFade(1, 0));
		sequence.Append(image.DOFade(0, 5));
	}
}
