using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// プレイヤーモデル
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

	//※コントローラーの移動スティックを少しだけ倒した時は、歩く（キーボードは、いつも倒しきった時と同じ入力になるので、走る）
	[Tooltip("移動スティックを少しだけ倒した時（歩き）の、通常移動速度（m/s）（構えていない時の歩くアニメーションの速さ。アニメーターの歩きの位置は、この速さ ÷ 通常移動速度にしてある）")]
	const float NormalWalkMoveSpeed = 1.3f;
	[Tooltip("移動スティックを少しだけ倒した時（歩き）の、エイム中移動速度（m/s）（構えている時の歩くアニメーションの速さ。アニメーターの歩きの位置は、この速さ ÷ エイム中移動速度にしてある）")]
	const float WeaponWalkMoveSpeed = 1.0f;
	[Tooltip("移動スティックをこの倒し具合（0〜1）までは、歩く")]
	const float WalkInputMax = 0.5f;
	[Tooltip("移動スティックをこの倒し具合（0〜1）から先は、走る（歩きとの間は、倒し具合に合わせてなめらかに速くする）")]
	const float RunInputMin = 0.9f;
	[Tooltip("移動の入力がない時の、入力の大きさ")]
	const float NoMoveInput = 0.0f;
	[Tooltip("止まっている時の、移動速度の割合")]
	const float StopMoveRate = 0.0f;
	[Tooltip("走っている時（移動スティックを倒しきった時・キーボードの時）の、移動速度の割合")]
	const float RunMoveRate = 1.0f;

	/// <summary>
	/// 移動の入力の向き（長さ1。入力がない時は長さ0）
	/// </summary>
	public Vector2 MoveInputDirection => new Vector2(inputHorizontal, inputVertical).normalized;

	/// <summary>
	/// 移動速度の割合（0で止まる、1で走る。移動スティックを少しだけ倒した時は、歩く速さの割合）
	/// ※ダッシュしている時は、いつも走る
	/// </summary>
	public float MoveRate
	{
		get
		{
			float inputMagnitude = Mathf.Clamp01(new Vector2(inputHorizontal, inputVertical).magnitude);
			if (inputMagnitude <= NoMoveInput)
			{
				return StopMoveRate;
			}

			if (isDash == true)
			{
				return RunMoveRate;
			}

			float walkMoveRate = isAim == true ? WeaponWalkMoveSpeed / weaponMoveSpeed : NormalWalkMoveSpeed / normalMoveSpeed;
			return Mathf.Lerp(walkMoveRate, RunMoveRate, Mathf.InverseLerp(WalkInputMax, RunInputMin, inputMagnitude));
		}
	}

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
	public int CurrentMine => currentMine;
	[Tooltip("地雷の所持できる最大数")]
	int maxMine = 3;
	public int MaxMine => maxMine;
	/// <summary>地雷を置いた後、立ち上がるのにかける時間（秒）</summary>
	public static readonly float Mine_Stand_Up_Time = 0.8f;
	[Tooltip("地雷を置く動きの今の段階")]
	EnumManager.MinePlacePhaseTYPE minePlacePhase = EnumManager.MinePlacePhaseTYPE.None;
	public EnumManager.MinePlacePhaseTYPE MinePlacePhase
	{
		get { return minePlacePhase; }
		set { minePlacePhase = value; }
	}
	[Tooltip("地雷を置いた後、立ち上がり始めてからの時間（秒）")]
	float mineStandUpCount = 0.0f;
	public float MineStandUpCount
	{
		get { return mineStandUpCount; }
		set { mineStandUpCount = value; }
	}
	/// <summary>地雷を置いているか？（しゃがんでいる間と立ち上がっている間は、移動・構え・ダッシュができない）</summary>
	public bool IsPlacingMine => minePlacePhase != EnumManager.MinePlacePhaseTYPE.None;

	//※プレイヤーの状態が変わったことを知らせるだけで、HPのゲージや数の表示などの見た目は、知らせを受け取った側が出す
	//  （このクラスは、プレイヤーの見た目やUIの側を知らない）
	/// <summary>
	/// ダメージを受けてHPが減った時の知らせ
	/// </summary>
	public event UnityAction OnDamaged;

	/// <summary>
	/// HPを回復した時の知らせ
	/// </summary>
	public event UnityAction OnHealed;

	/// <summary>
	/// スタミナを使った時の知らせ
	/// </summary>
	public event UnityAction OnStaminaConsumed;

	/// <summary>
	/// スタミナを回復した時の知らせ
	/// </summary>
	public event UnityAction OnStaminaRestored;

	/// <summary>
	/// アーマープレートの数が変わった時の知らせ（変わった後の数）
	/// </summary>
	public event UnityAction<int> OnArmorPlateChanged;

	/// <summary>
	/// 食料の数が変わった時の知らせ（変わった後の数）
	/// </summary>
	public event UnityAction<int> OnFoodChanged;

	/// <summary>
	/// 地雷の数が変わった時の知らせ（変わった後の数）
	/// </summary>
	public event UnityAction<int> OnMineCountChanged;

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
		//地雷を置いている途中でも、置くのをやめて立ち姿に戻す
		minePlacePhase = EnumManager.MinePlacePhaseTYPE.None;
		mineSpawnCount = 0.0f;
		mineStandUpCount = 0.0f;
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

	/// <summary>
	/// HPとスタミナの初期化処理（HPを回復する時はアーマープレートを1つ、スタミナを回復する時は食料を1つ使う）
	/// </summary>
	/// <param name="dead">HPが0になった時の処理</param>
	/// <param name="isInvincible">無敵判定（trueを返す間はダメージを受けない）</param>
	public void InitializeHPAndStamina(UnityAction dead, Func<bool> isInvincible)
	{
		hp.Initialize(Damaged, dead, UseArmorPlate, Healed, isInvincible);
		stamina.Initialize(StaminaConsumed, UseFood, StaminaRestored);
	}

	/// <summary>
	/// ダメージを受けてHPが減った時の処理（ダメージ画像エフェクトを出して、知らせる）
	/// </summary>
	void Damaged()
	{
		isDamage = true;
		OnDamaged?.Invoke();
	}

	/// <summary>
	/// HPを回復した時の処理（HP回復画像エフェクトを出して、知らせる）
	/// </summary>
	void Healed()
	{
		isHpHeal = true;
		OnHealed?.Invoke();
	}

	/// <summary>
	/// スタミナを使った時の処理（知らせる）
	/// </summary>
	void StaminaConsumed()
	{
		OnStaminaConsumed?.Invoke();
	}

	/// <summary>
	/// スタミナを回復した時の処理（スタミナ回復画像エフェクトを出して、知らせる）
	/// </summary>
	void StaminaRestored()
	{
		isStaminaHeal = true;
		OnStaminaRestored?.Invoke();
	}

	public void AfterUpdate()
	{
		inputHorizontal = Input.GetAxisRaw("Horizontal");
		inputVertical = Input.GetAxisRaw("Vertical");

		//右ボタンまたはレフトシフトが押されていたら中身を実行（地雷を置いている間は構えない）
		if ((Input.GetMouseButton(1) || Input.GetKey(KeyCode.LeftControl) || XInputManager.SingletonInstance.XInputTriggerHandler.IsPressedLT) && IsPlacingMine == false)
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

		if (IsPlacingMine == true)
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
	void UseArmorPlate()
	{
		if (currentArmorPlate <= 0)
		{
			return;
		}

		currentArmorPlate = currentArmorPlate - 1;
		OnArmorPlateChanged?.Invoke(currentArmorPlate);
	}

	/// <summary>
	/// アーマープレートを取得
	/// </summary> 
	public void AcquireArmorPlate()
	{
		if (maxArmorPlate <= currentArmorPlate)
		{
			return;
		}

		currentArmorPlate = currentArmorPlate + 1;
		OnArmorPlateChanged?.Invoke(currentArmorPlate);
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
	void UseFood()
	{
		if (currentFood <= 0)
		{
			return;
		}

		currentFood = currentFood - 1;
		OnFoodChanged?.Invoke(currentFood);
	}

	/// <summary>
	/// 食料を取得
	/// </summary> 
	public void AcquireFood()
	{
		if (maxFood <= currentFood)
		{
			return;
		}

		currentFood = currentFood + 1;
		OnFoodChanged?.Invoke(currentFood);
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

	/// <summary>
	/// 地雷を1つ使う
	/// </summary>
	/// <returns>使えたか？（地雷を持っていない時は使えない）</returns>
	public bool UseMine()
	{
		if (currentMine <= 0)
		{
			return false;
		}

		currentMine = currentMine - 1;
		OnMineCountChanged?.Invoke(currentMine);
		return true;
	}

	/// <summary>
	/// 地雷を取得
	/// </summary> 
	public void AcquireMine()
	{
		if (maxMine <= currentMine)
		{
			return;
		}

		currentMine = currentMine + 1;
		OnMineCountChanged?.Invoke(currentMine);
	}
}
