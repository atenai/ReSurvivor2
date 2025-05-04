using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Target : MonoBehaviour
{
	float currentHp = 100.0f;
	[SerializeField] float maxHp = 100.0f;
	[UnityEngine.Tooltip("キャンバス")]
	[SerializeField] Canvas canvas;
	public Canvas Canvas => canvas;
	[SerializeField] Slider sliderHp;
	bool isDead = false;
	public bool IsDead
	{
		get { return isDead; }
		set { isDead = value; }
	}

	public void Start()
	{
		//Debug.Log("<color=orange>Targetクラスを初期化</color>");
		sliderHp.value = 1;
		currentHp = maxHp;
		isDead = false;
	}

	public void Update()
	{
		//常にキャンバスをメインカメラの方を向かせる
		canvas.transform.rotation = Camera.main.transform.rotation;
	}

	/// <summary>
	/// ダメージ処理
	/// </summary>
	public virtual void TakeDamage(float amount)
	{
		currentHp = currentHp - amount;
		//Debug.Log("<color=orange>currentHp : " + currentHp + "</color>");
		sliderHp.value = (float)currentHp / (float)maxHp;
		if (currentHp <= 0.0f)
		{
			isDead = true;
			Destroy(this.gameObject);
		}
	}
}
