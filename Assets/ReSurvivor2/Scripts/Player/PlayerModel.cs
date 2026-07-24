using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// プレイヤーモデル
/// MVPパターンのModel担当
/// </summary>
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
	float energeticNormalMoveSpeed = 5.0f;
	[Tooltip("プレイヤーキャラクターの元気な時のエイム中移動速度")]
	float energeticWeaponMoveSpeed = 3.0f;
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

	[Tooltip("HP")]
	[SerializeField] HitPoint hp;
	public HitPoint HP => hp;

	[Tooltip("スタミナ")]
	[SerializeField] Stamina stamina;
	public Stamina Stamina => stamina;

	[Tooltip("現在のアーマープレート数")]
	int currentArmorPlate = 2;
	public int CurrentArmorPlate => currentArmorPlate;
	[Tooltip("アーマープレートの所持できる最大数")]
	int maxArmorPlate = 3;
	public int MaxArmorPlate => maxArmorPlate;
	[Tooltip("アーマープレートの所持できる限界最大数")]
	int limitMaximumArmorPlate = 10;

	[Tooltip("現在の食料数")]
	int currentFood = 2;
	public int CurrentFood => currentFood;
	[Tooltip("食料の所持できる最大数")]
	int maxFood = 3;
	public int MaxFood => maxFood;
	[Tooltip("食料の所持できる限界最大数")]
	int limitMaximumFood = 10;

	[Tooltip("ダメージ画像エフェクトトリガー")]
	bool isDamage = false;
	public bool IsDamage
	{
		get { return isDamage; }
		set { isDamage = value; }
	}
	[Tooltip("HP回復画像エフェクトトリガー")]
	bool isHpHeal = false;
	public bool IsHpHeal
	{
		get { return isHpHeal; }
		set { isHpHeal = value; }
	}
	[Tooltip("スタミナ回復画像エフェクトトリガー")]
	bool isStaminaHeal = false;
	public bool IsStaminaHeal
	{
		get { return isStaminaHeal; }
		set { isStaminaHeal = value; }
	}

	[Header("リスポーンポイント")]
	[Tooltip("プレイヤーのリスポーンポイントの位置")]
	Vector3 respawnPosition = new Vector3(0.0f, 1.0f, 0.0f);
	public Vector3 RespawnPosition => respawnPosition;

	[Tooltip("プレイヤーのリスポーンポイントの回転")]
	Quaternion respawnRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
	public Quaternion RespawnRotation => respawnRotation;

	[Header("地雷")]
	[Tooltip("キャラクターからの地雷の生成距離")]
	float mineSpawnDistance = 1.0f;
	public float MineSpawnDistance => mineSpawnDistance;
	/// <summary>地雷を生成する際に長押しと判定する間隔、例：0.5秒なら（30フレーム ÷ 60fps = 0.5秒）に設定（60fpsの場合）</summary>
	public static readonly float Mine_Hold_Time = 30.0f / 60.0f;
	[Tooltip("地雷が再生成できるまでのカウント")]
	float mineSpawnCount = 0.0f;
	public float MineSpawnCount
	{
		get { return mineSpawnCount; }
		set { mineSpawnCount = value; }
	}
	[Tooltip("現在の地雷数")]
	int currentMine = 3;
	public int CurrentMine
	{
		get { return currentMine; }
		set { currentMine = value; }
	}
	[Tooltip("地雷の所持できる最大数")]
	int maxMine = 3;
	public int MaxMine => maxMine;

	/// <summary>
	/// コンストラクタ
	/// </summary>
	public PlayerModel()
	{
		ResetMove();
		StartDamageEffect();
		StartHpHealEffect();
		StartStaminaHealEffect();
	}

	/// <summary>
	/// 移動パラメーターをリセットする
	/// </summary>
	public void ResetMove()
	{
		inputHorizontal = 0.0f;
		inputVertical = 0.0f;
		moveForward = Vector3.zero;
		isAim = false;
		isDash = false;
	}

	/// <summary>
	/// ダメージ画像エフェクトの初期化処理
	/// </summary> 
	void StartDamageEffect()
	{
		isDamage = false;
	}

	/// <summary>
	/// HP回復画像エフェクトの初期化処理
	/// </summary> 
	void StartHpHealEffect()
	{
		isHpHeal = false;
	}

	/// <summary>
	/// スタミナ回復画像エフェクトの初期化処理
	/// </summary> 
	void StartStaminaHealEffect()
	{
		isStaminaHeal = false;
	}

	/// <summary>
	/// セーブ
	/// </summary>
	public void Save()
	{
		Debug.Log("<color=cyan>プレイヤーセーブ</color>");
		ES3.Save<float>("Hp", hp.CurrentHp);
		ES3.Save<float>("Stamina", stamina.CurrentStamina);
		ES3.Save<int>("ArmorPlate", currentArmorPlate);
		ES3.Save<int>("Food", currentFood);
		ES3.Save("PlayerPos", respawnPosition);
		ES3.Save("PlayerRot", respawnRotation);
		ES3.Save<int>("Mine", currentMine);
	}

	/// <summary>
	/// ロード
	/// </summary>
	public void Load()
	{
		//Debug.Log("<color=purple>プレイヤーロード</color>");

		float loadHp = ES3.Load<float>("Hp", HitPoint.Max_Hp);
		hp = new HitPoint(loadHp);
		//Debug.Log("<color=purple>HP : " + hp.CurrentHp + "</color>");

		float loadStamina = ES3.Load<float>("Stamina", Stamina.Max_Stamina);
		stamina = new Stamina(loadStamina);
		//Debug.Log("<color=purple>スタミナ : " + currentStamina + "</color>");

		currentArmorPlate = ES3.Load<int>("ArmorPlate", 2);
		//Debug.Log("<color=purple>アーマープレート : " + currentArmorPlate + "</color>");

		currentFood = ES3.Load<int>("Food", 2);
		//Debug.Log("<color=purple>食料 : " + currentFood + "</color>");

		//ステージが切り替わる度にリスポーン位置が呼ばれるため、リスポーンのオブジェクトは遷移先のステージで敵やオブジェクトにかぶらないように注意すること！！
		if (ES3.KeyExists("PlayerPos") == true)
		{
			respawnPosition = ES3.Load<Vector3>("PlayerPos");
			//Debug.Log("<color=purple>プレイヤー位置 : " + this.transform.position + "</color>");
		}
		if (ES3.KeyExists("PlayerRot") == true)
		{
			respawnRotation = ES3.Load<Quaternion>("PlayerRot");
			//Debug.Log("<color=purple>プレイヤー回転 : " + this.transform.rotation + "</color>");
		}

		currentMine = ES3.Load<int>("Mine", 3);
		//Debug.Log("<color=purple>地雷 : " + currentMine + "</color>");
	}

	/// <summary>
	/// プレイヤーのリスポーンポイントを設定する
	/// </summary>
	/// <param name="pos">プレイヤー位置</param>
	/// <param name="rot">プレイヤー回転</param>
	public void SetPlayerRespawnPoint(Vector3 pos, Quaternion rot)
	{
		respawnPosition = pos;
		respawnRotation = rot;
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

	/// <summary>
	/// HPを回復する
	/// </summary>
	public void Heal()
	{
		if (currentArmorPlate <= 0)
		{
			return;
		}
		hp.Heal();
	}

	/// <summary>
	/// スタミナを回復する
	/// </summary>
	public void RestoresStamina()
	{
		if (currentFood <= 0)
		{
			return;
		}
		stamina.RestoresStamina();
	}

	/// <summary>
	/// アーマープレートを使用
	/// </summary>
	public void UseArmorPlate(UnityAction<int> unityAction)
	{
		if (currentArmorPlate <= 0)
		{
			return;
		}

		currentArmorPlate = currentArmorPlate - 1;
		unityAction?.Invoke(currentArmorPlate);
	}

	/// <summary>
	/// アーマープレートを取得
	/// </summary> 
	public void AcquireArmorPlate(UnityAction<int> unityAction)
	{
		if (maxArmorPlate <= currentArmorPlate)
		{
			return;
		}

		currentArmorPlate = currentArmorPlate + 1;
		unityAction?.Invoke(currentArmorPlate);
	}

	/// <summary>
	/// アーマープレートの所持できる最大数を増加
	/// </summary>
	public void IncreaseMaxArmorPlate()
	{
		if (limitMaximumArmorPlate <= maxArmorPlate)
		{
			return;
		}

		maxArmorPlate = maxArmorPlate + 1;
	}

	/// <summary>
	/// 食料を使用
	/// </summary>
	public void UseFood(UnityAction<int> unityAction)
	{
		if (currentFood <= 0)
		{
			return;
		}

		currentFood = currentFood - 1;
		unityAction?.Invoke(currentFood);
	}

	/// <summary>
	/// 食料を取得
	/// </summary> 
	public void AcquireFood(UnityAction<int> unityAction)
	{
		if (maxFood <= currentFood)
		{
			return;
		}

		currentFood = currentFood + 1;
		unityAction?.Invoke(currentFood);
	}

	/// <summary>
	/// 食料の所持できる最大数を増加
	/// </summary>
	public void IncreaseMaxFood()
	{
		if (limitMaximumFood <= maxFood)
		{
			return;
		}

		maxFood = maxFood + 1;
	}
}
