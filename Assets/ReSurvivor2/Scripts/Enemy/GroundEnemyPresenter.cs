using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Knife.Effects;
using Cinemachine;
using UnityEngine.AI;

/// <summary>
/// 地上敵
/// MVPパターンのPresenter担当
/// </summary>
public class GroundEnemyPresenter : MonoBehaviour, IEnemy
{
	[Header("地上敵MVPパターン")]
	/// <summary>地上敵モデル</summary>
	[SerializeField] GroundEnemyModel groundEnemyModel = new GroundEnemyModel();
	public GroundEnemyModel GroundEnemyModel => groundEnemyModel;

	[Tooltip("地上敵キャラクタービュー")]
	[SerializeField] GroundEnemyCharacterView groundEnemyCharacterView;
	public GroundEnemyCharacterView GroundEnemyCharacterView => groundEnemyCharacterView;

	[Tooltip("地上敵UIビュー")]
	[SerializeField] GroundEnemyUIView groundEnemyUIView;
	public GroundEnemyUIView GroundEnemyUIView => groundEnemyUIView;

	[Tooltip("ナビメッシュ")]
	[SerializeField] NavMeshAgent navMeshAgent;
	public NavMeshAgent NavMeshAgent => navMeshAgent;

	[Tooltip("物理")]
	[SerializeField] Rigidbody enemyRigidbody;
	public Rigidbody Rigidbody => enemyRigidbody;

	[Tooltip("カバーポイント")]
	CoverPoint[] coverPoints;
	public CoverPoint[] GetCoverPoints => coverPoints;
	public void SetCoverPoints(CoverPoint[] coverPoints)
	{
		this.coverPoints = coverPoints;
	}

	[Tooltip("プレイヤー")]
	GameObject targetPlayer;
	public GameObject TargetPlayer => targetPlayer;

	[Header("コライダー")]

	[Tooltip("エネミー本体のコライダー")]
	[SerializeField] Collider enemyCollider;
	public Collider EnemyCollider
	{
		get { return enemyCollider; }
		set { enemyCollider = value; }
	}

	[Tooltip("センサーコライダー用の変数")]
	[SerializeField] ColliderEventHandler[] colliders = default;
	bool[] hits;
	public bool GetHit(int index) => hits[index];
	Collider hitCollider = null;
	public Collider HitCollider => hitCollider;

	[Tooltip("エネミー追跡範囲の中心点")]
	[SerializeField] GameObject centerPos;
	public GameObject CenterPos => centerPos;

	[Header("パトロール")]
	[Tooltip("パトロールポイントの位置")]
	[SerializeField] List<GameObject> patrolPoints = new List<GameObject>();
	public List<GameObject> PatrolPoints => patrolPoints;
	[Tooltip("現在のパトロールポイントのナンバー")]
	int patrolPointNumber = 0;
	public int PatrolPointNumber
	{
		get { return patrolPointNumber; }
		set { patrolPointNumber = value; }
	}

	[Header("地面")]
	[Tooltip("地面の中心点")]
	[SerializeField] Transform groundCheckCenter;
	[Tooltip("地面となるレイヤー")]
	[SerializeField] LayerMask groundLayers;
	[Tooltip("地面検知範囲")]
	[SerializeField] float groundedRadius = 0.2f;
	[Tooltip("地面と接地しているか？")]
	bool isGrounded = false;
	public bool IsGrounded => isGrounded;

	//↓アセットストアのプログラム↓//
	[Tooltip("マズルフラッシュと薬莢")]
	[SerializeField] ParticleGroupEmitter[] shotEmitters;
	[Tooltip("硝煙")]
	[SerializeField] ParticleGroupPlayer afterFireSmoke;
	//↑アセットストアのプログラム↑//

	[Header("射撃")]
	[Tooltip("銃口（弾の発射位置）")]
	[SerializeField] Transform muzzle;
	[Tooltip("プレイヤーのどの高さを狙うか（足元からの高さ）")]
	[SerializeField] float aimHeight = 1.2f;
	[Tooltip("正面から何度までならプレイヤーの方へ狙って撃てるか（それ以上は正面に撃つ）")]
	[SerializeField] float maxAimAngle = 30.0f;
	[Tooltip("銃口が壁の向こうに出ていないかを調べる、体の中の基準点の高さ")]
	readonly float bodyCenterHeight = 1.2f;
	[Tooltip("これより短いベクトルは向きが決まらないものとして扱う（長さの2乗）")]
	const float MinSqrLength = 0.0001f;
	[Tooltip("体の基準点と銃口がこれより近ければ、銃口の位置をそのまま使う")]
	const float MinMuzzleDistance = 0.001f;
	[Tooltip("プレイヤーとの水平距離がこれより近ければ、正面に撃つ")]
	const float MinAimDistance = 0.01f;

	[Tooltip("シネマシーンインパルス")]
	[SerializeField] CinemachineImpulseSource cinemachineImpulseSource;

	[Tooltip("デバッグ")]
	[SerializeField] DebugEnemy debugEnemy;
	public DebugEnemy DebugEnemy => debugEnemy;

	/// <summary>エネミーのゲームオブジェクトを取得する</summary>
	public GameObject GetEnemyGameObject()
	{
		return this.gameObject;
	}

	/// <summary>HPを取得する</summary>
	public HitPoint GetHitPoint()
	{
		return groundEnemyModel.HP;
	}

	void Awake()
	{
		InitNavMeshAgent();
	}

	/// <summary>
	/// Awake で NavMeshAgent の自動位置・回転更新をオフにする。
	/// これにより NavMeshAgent は経路計算（path, steeringTarget 等）専用になり、
	/// 実際の移動は Rigidbody.velocity によって行う。
	/// </summary>
	void InitNavMeshAgent()
	{
		// NavMeshAgent による Transform 更新を無効化
		navMeshAgent.updatePosition = false;
		navMeshAgent.updateRotation = false;
		// ★これが重要：Agent内部位置を物理位置に同期
		navMeshAgent.nextPosition = enemyRigidbody.position;
	}

	/// <summary>
	/// 初期化処理
	/// </summary>
	void Start()
	{
		//Debug.Log("<color=orange>GroundEnemyクラスを初期化</color>");
		InitSensorCollider();
		Initialize();
	}

	/// <summary>
	/// センサーコライダーの初期化処理
	/// </summary> 
	void InitSensorCollider()
	{
		hits = new bool[colliders.Length];

		foreach (var i in colliders)
		{
			i.OnTriggerEnterEvent.AddListener(OnTriggerEnterHit);
			i.OnTriggerExitEvent.AddListener(OnTriggerExitHit);
		}
	}

	/// <summary>
	/// リスポーンした際の初期化処理
	/// </summary>
	void Initialize()
	{
		groundEnemyModel.Initialize();
		groundEnemyModel.HP.Initialize(DamageEffect, Dead);
		groundEnemyUIView.InitHP(groundEnemyModel.HP.CurrentHp);
		//敵マーカー作成
		EnemyIndicatorManager.SingletonInstance.InstanceIndicator(this);
		EnemyManager.SingletonInstance.ChaseEvent.AddListener(groundEnemyModel.ChaseOn);

		if (targetPlayer == null)
		{
			targetPlayer = PlayerManagerPresenter.SingletonInstance.gameObject;
		}

		groundEnemyCharacterView.InitRootMotion();
		enemyRigidbody.useGravity = true;
		enemyCollider.enabled = true;

		ResetSensorCollider();

		patrolPointNumber = 0;
		isGrounded = false;

		GroundCheck();
	}

	/// <summary>
	/// ダメージエフェクト
	/// </summary>
	public void DamageEffect()
	{
		groundEnemyUIView.SliderHp.value = (float)groundEnemyModel.HP.CurrentHp / (float)HitPoint.Max_Hp;
	}

	/// <summary>
	/// 死んだ際の処理
	/// </summary>
	public void Dead()
	{
		EnemyManager.SingletonInstance.ChaseEvent.RemoveListener(groundEnemyModel.ChaseOn);
		//敵マーカー削除
		EnemyIndicatorManager.SingletonInstance.DeleteIndicator(this);
	}

	/// <summary>
	/// センサーコライダーの状態をリセットする
	/// </summary>
	void ResetSensorCollider()
	{
		//コライダーが何も当たっていない状態にする
		if (hits != null)
		{
			for (int i = 0; i < colliders.Length; i++)
			{
				hits[i] = false;
			}
		}
		hitCollider = null;
	}



	/// <summary>
	/// 地面に接触しているか？をチェックする関数
	/// </summary>
	void GroundCheck()
	{
		Vector3 spherePosition = groundCheckCenter.transform.position;
		bool centerChecker = Physics.CheckSphere(spherePosition, groundedRadius, groundLayers, QueryTriggerInteraction.Ignore);
		isGrounded = centerChecker;
	}

	/// <summary>
	/// 地面に接触しているか？を可視化する関数
	/// </summary>
	void OnDrawGizmosSelected()
	{
		if (isGrounded == true)
		{
			//地面についている
			Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
			Gizmos.color = transparentGreen;
		}
		else
		{
			//地面についていない
			Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);
			Gizmos.color = transparentRed;
		}

		Gizmos.DrawSphere(new Vector3(groundCheckCenter.transform.position.x, groundCheckCenter.transform.position.y, groundCheckCenter.transform.position.z), groundedRadius);
	}

	/// <summary>
	/// 更新処理
	/// </summary>
	void Update()
	{
		groundEnemyUIView.RotCanvas();
		GroundCheck();

		if (groundEnemyModel.Eyesight(groundEnemyCharacterView.Head.transform.position, this.transform.forward, this.transform.up) == true)
		{
			//Debug.Log("<color=red>プレイヤーを発見!</color>");
			EnemyManager.SingletonInstance.ChaseEvent.Invoke();
		}
		Alert();
		groundEnemyModel.ChasePlayer();

#if UNITY_EDITOR
		//デバッグ関連の処理
		DebugText();
#endif
	}

	/// <summary>
	/// アラートイメージの表示・非表示
	/// </summary>
	void Alert()
	{
		if (groundEnemyModel.HP.IsDead == false)
		{
			groundEnemyUIView.Alert.gameObject.SetActive(groundEnemyModel.IsChase);
		}
		else
		{
			groundEnemyUIView.Alert.gameObject.SetActive(false);
		}
	}

	/// <summary>
	/// デバッグテキスト
	/// </summary> 
	void DebugText()
	{
		string[] debugTexts = new string[11];

		debugTexts[9] = "IsDead : " + groundEnemyModel.HP.IsDead.ToString();

		debugTexts[8] = "currentGrenade : " + groundEnemyModel.CurrentGrenade.ToString();

		debugTexts[7] = "isReloadTimeActive : " + groundEnemyModel.IsReloadTimeActive.ToString();
		debugTexts[6] = "currentMagazine : " + groundEnemyModel.CurrentMagazine.ToString();

		debugTexts[5] = "chaseCountTime : " + groundEnemyModel.ChaseCountTime.ToString();
		debugTexts[4] = "isChase : " + groundEnemyModel.IsChase.ToString();

		debugTexts[3] = "patrolPointNumber : " + patrolPointNumber.ToString();
		debugTexts[2] = "isGrounded : " + isGrounded.ToString();


		if (hitCollider != null)
		{
			debugTexts[1] = "hitCollider : " + hitCollider.ToString();
		}
		else
		{
			debugTexts[1] = "hitCollider : " + "null";
		}
		debugTexts[0] = "hits[0] : " + hits[0].ToString();

		debugEnemy.DebugSystem(debugEnemy.IsDebugMode, groundEnemyUIView.Canvas, debugTexts.Length);
		debugEnemy.SetDebugText(debugTexts);
	}

	/// <summary>
	/// センサーコライダーの当たり判定(触れた時)
	/// </summary>
	/// <param name="self"></param>
	/// <param name="collider"></param>
	void OnTriggerEnterHit(ColliderEventHandler self, Collider collider)
	{
		if (collider.tag == "Player")
		{
			if (collider != null)
			{
				//Debug.Log("<color=blue>プレイヤーを発見!2</color>");
				hitCollider = collider;
			}

			int index = 0;
			for (int i = 0; i < colliders.Length; i++)
			{
				if (colliders[i] == self)
				{
					index = i;
				}
			}

			hits[index] = true;
		}
	}

	/// <summary>
	/// センサーコライダーの当たり判定(離れた時)
	/// </summary>
	/// <param name="self"></param>
	/// <param name="collider"></param>
	void OnTriggerExitHit(ColliderEventHandler self, Collider collider)
	{
		if (collider.tag == "Player")
		{
			if (collider != null)
			{
				hitCollider = collider;
			}

			int index = 0;
			for (int i = 0; i < colliders.Length; i++)
			{
				if (colliders[i] == self)
				{
					index = i;
				}
			}

			hits[index] = false;
		}
	}

	/// <summary>
	/// 物理の当たり判定
	/// </summary>
	/// <param name="collision"></param>
	void OnCollisionEnter(Collision collision)
	{
		// Debug.Log("<color=red>当たった！ : " + collision.gameObject.name + "</color>");

		if (collision.collider.tag == "Player")
		{
			PlayerManagerPresenter.SingletonInstance.PlayerModel.HP.Damage(10.0f);
			CameraShaker();
		}
	}

	/// <summary>
	/// 地面に接触しているか？を判別する当たり判定(触れた時)
	/// </summary>
	public void OnTriggerEnterHitCliff(Collider collider)
	{
		//Debug.Log("<color=red>OnTriggerEnterHitGround</color>");
	}

	/// <summary>
	/// 地面に接触しているか？を判別する当たり判定(離れた時)
	/// </summary>
	public void OnTriggerExitHitCliff(Collider collider)
	{
		//Debug.Log("<color=green>OnTriggerExitHitGround</color>");
	}

	/// <summary>
	/// ハンドガンの射撃SE
	/// </summary> 
	public void HandGunFireSE()
	{
		//参考例
		//SoundManager.SingletonInstance.HandGunShootSEPool.GetGameObject(PlayerManager.SingletonInstance.GunModelFacade.HandGunModel.HandGunMuzzleTransform);
	}

	/// <summary>
	/// ハンドガンのリロードSE
	/// </summary> 
	public void HandGunReloadSE()
	{
		//参考例
		//SoundManager.SingletonInstance.HandGunReloadSEPool.GetGameObject(PlayerManager.SingletonInstance.GunModelFacade.HandGunModel.HandGunBulletCasingTransform);
	}

	/// <summary>
	/// ハンドガンの薬莢SE
	/// </summary>
	public void HandGunBulletCasingSE()
	{
		//参考例
		//SoundManager.SingletonInstance.HandGunBulletCasingSEPool.GetGameObject(PlayerManager.SingletonInstance.GunModelFacade.HandGunModel.HandGunBulletCasingTransform);
	}

	/// <summary>
	/// アサルトライフルの射撃SE
	/// </summary> 
	public void AssaultRifleFireSE()
	{
		SoundManager.SingletonInstance.AssaultRifleShootSEPool.GetGameObject(this.transform);
	}

	/// <summary>
	/// アサルトライフルのリロードSE
	/// </summary> 
	public void AssaultRifleReloadSE()
	{
		SoundManager.SingletonInstance.AssaultRifleReloadSEPool.GetGameObject(this.transform);
	}

	/// <summary>
	/// アサルトライフルの薬莢SE
	/// </summary>
	public void AssaultRifleBulletCasingSE()
	{
		SoundManager.SingletonInstance.AssaultRifleBulletCasingSEPool.GetGameObject(this.transform);
	}

	/// <summary>
	/// ショットガンの射撃SE
	/// </summary> 
	public void ShotGunFireSE()
	{
		SoundManager.SingletonInstance.ShotGunShootSEPool.GetGameObject(this.transform);
	}

	/// <summary>
	/// ショットガンのリロードSE
	/// </summary> 
	public void ShotGunReloadSE()
	{
		SoundManager.SingletonInstance.ShotGunReloadSEPool.GetGameObject(this.transform);
	}

	/// <summary>
	/// ショットガンの薬莢SE
	/// </summary>
	public void ShotGunBulletCasingSE()
	{
		SoundManager.SingletonInstance.ShotGunBulletCasingSEPool.GetGameObject(this.transform);
	}

	/// <summary>
	/// マズルフラッシュのエフェクトと薬莢を出す（アセットストアで買ったコードをそのままもってきている）
	/// </summary>
	public void MuzzleFlashAndShell()
	{
		if (shotEmitters != null)
		{
			foreach (var effect in shotEmitters)
			{
				effect.Emit(1);
			}
		}
	}

	/// <summary>
	/// 硝煙のエフェクトを出す（アセットストアで買ったコードをそのままもってきている）
	/// </summary>
	public void AfterFireSmoke()
	{
		if (afterFireSmoke != null)
		{
			afterFireSmoke.Play();
		}
	}

	/// <summary>
	/// 弾を1発撃つ（弾道処理と弾道エフェクトは EnemyBullet が行う）
	/// </summary>
	/// <param name="spreadAngle">散乱角度（左右・上下それぞれ ±spreadAngle 度）</param>
	/// <param name="range">射程</param>
	/// <param name="damage">ダメージ</param>
	/// <param name="speed">弾の速度</param>
	public void FireBullet(float spreadAngle, float range, float damage, float speed)
	{
		if (EffectManager.SingletonInstance == null || EffectManager.SingletonInstance.EnemyBulletPool == null)
		{
			return;
		}

		Vector3 origin = GetMuzzlePosition();
		Vector3 direction = GetAimDirection(origin);

		//散乱（今までの射撃と同じく、左右と上下にそれぞれランダムでずらす）
		Vector3 right = Vector3.Cross(Vector3.up, direction);
		if (right.sqrMagnitude < MinSqrLength)
		{
			right = this.transform.right;
		}
		right.Normalize();
		direction = Quaternion.AngleAxis(UnityEngine.Random.Range(-spreadAngle, spreadAngle), Vector3.up) * direction;
		direction = Quaternion.AngleAxis(UnityEngine.Random.Range(-spreadAngle, spreadAngle), right) * direction;
		direction.Normalize();

		EffectManager.SingletonInstance.EnemyBulletPool.Fire(this, origin, direction, speed, range, damage);
	}

	/// <summary>
	/// 弾の発射位置（銃口）
	/// 銃口が壁などの向こう側に出ている時は、壁越しに撃てないように体の中から撃つ
	/// </summary>
	Vector3 GetMuzzlePosition()
	{
		Vector3 bodyCenter = this.transform.position + Vector3.up * bodyCenterHeight;
		if (muzzle == null)
		{
			return bodyCenter;
		}

		Vector3 muzzlePosition = muzzle.position;
		Vector3 toMuzzle = muzzlePosition - bodyCenter;
		float distance = toMuzzle.magnitude;
		if (distance < MinMuzzleDistance)
		{
			return muzzlePosition;
		}

		RaycastHit[] hits = Physics.RaycastAll(bodyCenter, toMuzzle / distance, distance, EnemyBullet.HitLayerMask, QueryTriggerInteraction.Ignore);
		foreach (RaycastHit hit in hits)
		{
			//自分のコライダーと、弾が当たらないコライダー（BulletEffect レイヤー）は無視
			if (hit.collider.transform == this.transform || hit.collider.transform.IsChildOf(this.transform) || EnemyBullet.IsIgnoredCollider(hit.collider) == true)
			{
				continue;
			}
			//体と銃口の間に何かある（銃口が壁にめり込んでいる）
			return bodyCenter;
		}

		return muzzlePosition;
	}

	/// <summary>
	/// 狙う方向（プレイヤーの胸のあたり）
	/// 正面から maxAimAngle 度より外にいる時は、正面寄りに撃つ（真横や後ろには撃たない）
	/// </summary>
	Vector3 GetAimDirection(Vector3 origin)
	{
		Vector3 forward = this.transform.forward;
		Vector3 flatForward = new Vector3(forward.x, 0.0f, forward.z);
		if (flatForward.sqrMagnitude < MinSqrLength)
		{
			flatForward = Vector3.forward;
		}
		flatForward.Normalize();

		if (targetPlayer == null)
		{
			return flatForward;
		}

		Vector3 target = targetPlayer.transform.position + Vector3.up * aimHeight;
		Vector3 aim = target - origin;
		Vector3 flatAim = new Vector3(aim.x, 0.0f, aim.z);
		float flatDistance = flatAim.magnitude;
		if (flatDistance < MinAimDistance)
		{
			return flatForward;
		}

		//上下の角度（傾き）
		float slope = aim.y / flatDistance;

		//左右の角度を正面から maxAimAngle 度までにする
		Vector3 flatDirection = Vector3.RotateTowards(flatForward, flatAim / flatDistance, maxAimAngle * Mathf.Deg2Rad, 0.0f);

		return (flatDirection + Vector3.up * slope).normalized;
	}

	/// <summary>
	/// プレイヤーにダメージを与えた際にカメラを揺らす
	/// </summary>
	public void CameraShaker()
	{
		cinemachineImpulseSource.GenerateImpulse();
	}

	/// <summary>
	/// ゲームオブジェクトが非表示またはデストロイされた際に呼ばれる
	/// </summary>
	void OnDisable()
	{
		EnemyManager.SingletonInstance.ChaseEvent.RemoveListener(groundEnemyModel.ChaseOn);
		//敵マーカー削除
		EnemyIndicatorManager.SingletonInstance.DeleteIndicator(this);
		UnInitRigidbody();
	}

	/// <summary>
	/// コンポーネントやオブジェクトが無効化されたら速度をリセットする。
	/// これにより停止状態で無効化され、再有効化時に不意な移動が発生しない。
	/// </summary>
	void UnInitRigidbody()
	{
		enemyRigidbody.velocity = Vector3.zero;
	}
}
