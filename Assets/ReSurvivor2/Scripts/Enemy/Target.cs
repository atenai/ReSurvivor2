using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Target : MonoBehaviour
{
	[UnityEngine.Tooltip("現在のHP")]
	float currentHp = 100.0f;
	public float CurrentHp
	{
		get { return currentHp; }
		set { currentHp = value; }
	}
	[UnityEngine.Tooltip("最大HP")]
	[SerializeField] float maxHp = 100.0f;
	public float MaxHp => maxHp;
	[UnityEngine.Tooltip("キャンバス")]
	[SerializeField] Canvas canvas;
	public Canvas Canvas => canvas;
	[UnityEngine.Tooltip("HPバー")]
	[SerializeField] Slider sliderHp;
	public Slider SliderHp
	{
		get { return sliderHp; }
		set { sliderHp = value; }
	}
	[UnityEngine.Tooltip("死んだか？")]
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
		//敵マーカー作成
		IndicatorManager.SingletonInstance.InstanceIndicator(this);
	}

	public void Update()
	{
		//常にキャンバスをメインカメラの方を向かせる
		canvas.transform.rotation = Camera.main.transform.rotation;
	}

	/// <summary>
	/// ゲームオブジェクトが非表示またはデストロイされた際に呼ばれる
	/// </summary>
	void OnDisable()
	{
		//敵マーカー削除
		IndicatorManager.SingletonInstance.DeleteIndicator(this);
	}

	/// <summary>
	/// ダメージ処理
	/// </summary>
	/// <param name="amount">ダメージ量</param>
	public virtual void TakeDamage(float amount)
	{
		Debug.Log("<color=orange>TargetのTakeDamage()</color>");
		currentHp = currentHp - amount;
		//Debug.Log("<color=orange>currentHp : " + currentHp + "</color>");
		sliderHp.value = (float)currentHp / (float)maxHp;
		if (currentHp <= 0.0f)
		{
			//敵マーカー削除
			IndicatorManager.SingletonInstance.DeleteIndicator(this);
			Destroy(this.gameObject);
		}
	}
}
