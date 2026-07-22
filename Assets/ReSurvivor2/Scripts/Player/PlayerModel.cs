using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレイヤー
/// MVPパターンのModel担当
/// </summary>
[Serializable]
public class PlayerModel
{
	[Header("プレイヤーキャラクターの移動関連")]
	/// <summary>縦入力</summary>
	float inputVertical;
	public float InputVertical => inputVertical;
	/// <summary>横入力</summary>
	float inputHorizontal;
	public float InputHorizontal => inputHorizontal;
	/// <summary>プレイヤーの前方向</summary>
	Vector3 moveForward;
	public Vector3 MoveForward
	{
		get { return moveForward; }
		set { moveForward = value; }
	}
	/// <summary>カメラの前方向</summary>
	Vector3 cameraForward;
	public Vector3 CameraForward
	{
		get { return cameraForward; }
		set { cameraForward = value; }
	}
	/// <summary>ダッシュするか？</summary>
	bool isDash = false;
	public bool IsDash
	{
		get { return isDash; }
		set { isDash = value; }
	}
	/// <summary>エイムしているか？</summary>
	bool isAim = false;
	public bool IsAim => isAim;
	[Tooltip("プレイヤーキャラクターの元気な時の通常移動速度")]
	[SerializeField] float energeticNormalMoveSpeed = 5.0f;
	[Tooltip("プレイヤーキャラクターの元気な時のエイム中移動速度")]
	[SerializeField] float energeticWeaponMoveSpeed = 3.0f;
	[Tooltip("プレイヤーキャラクターの疲れた時の通常移動速度")]
	float tiredNormalMoveSpeed = 4.0f;
	[Tooltip("プレイヤーキャラクターの疲れた時のエイム中移動速度")]
	float tiredWeaponMoveSpeed = 2.0f;
	[Tooltip("プレイヤーキャラクターの通常移動速度")]
	float normalMoveSpeed = 5.0f;
	public float NormalMoveSpeed => normalMoveSpeed;
	[Tooltip("プレイヤーキャラクターのエイム中移動速度")]
	float weaponMoveSpeed = 3.0f;
	public float WeaponMoveSpeed => weaponMoveSpeed;

	[Tooltip("スタミナ")]
	[SerializeField] Stamina stamina;
	public Stamina Stamina => stamina;

	public PlayerModel()
	{
		ResetMove();
	}

	public void ResetMove()
	{
		inputHorizontal = 0.0f;
		inputVertical = 0.0f;
		moveForward = Vector3.zero;
		isAim = false;
		isDash = false;
	}

	/// <summary>
	/// セーブ
	/// </summary>
	public void Save()
	{
		Debug.Log("<color=cyan>プレイヤーセーブ</color>");
		ES3.Save<float>("Stamina", stamina.CurrentStamina);
	}

	/// <summary>
	/// ロード
	/// </summary>
	public void Load()
	{
		//Debug.Log("<color=purple>プレイヤーロード</color>");
		float loadStamina = ES3.Load<float>("Stamina", Stamina.Max_Stamina);
		stamina = new Stamina(loadStamina);
		//Debug.Log("<color=purple>スタミナ : " + currentStamina + "</color>");
	}

	public void AfterUpdate()
	{
		inputHorizontal = Input.GetAxisRaw("Horizontal");
		inputVertical = Input.GetAxisRaw("Vertical");

		//右ボタンまたはレフトシフトが押されていたら中身を実行
		if (Input.GetMouseButton(1) || Input.GetKey(KeyCode.LeftControl) || XInputManager.SingletonInstance.XInputTriggerHandler.IsPressedLT)
		{
			isAim = true;
		}
		else
		{
			isAim = false;
		}

		if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetButtonDown("XInput L_Stick Click"))
		{
			isDash = true;
		}
	}

	/// <summary>
	/// 移動スピードを変える
	/// </summary>
	public void ChangeMoveSpeed()
	{
		if (isAim == true)
		{
			isDash = false;
		}

		if (stamina.IsTired == true)
		{
			isDash = false;
		}

		if (isDash == true)
		{
			//元気な際の移動速度
			normalMoveSpeed = energeticNormalMoveSpeed;
			weaponMoveSpeed = energeticWeaponMoveSpeed;
		}
		else
		{
			//疲れた際の移動速度
			normalMoveSpeed = tiredNormalMoveSpeed;
			weaponMoveSpeed = tiredWeaponMoveSpeed;
		}
	}
}
