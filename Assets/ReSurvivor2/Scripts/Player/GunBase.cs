using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 銃のベース
/// </summary>
public abstract class GunBase
{
	[Header("銃のベース")]
	protected EnumManager.GunTYPE gunType;
	public EnumManager.GunTYPE GetGunType => gunType;

	[Tooltip("銃のダメージ")]
	[SerializeField] protected float damage = 10.0f;
	[Tooltip("着弾した物体を後ろに押す力")]
	[SerializeField] protected float impactForce = 30.0f;
	[Tooltip("何秒間隔で撃つか")]
	[SerializeField] protected float fireRate = 0.1f;
	[Tooltip("射撃間隔時間用のカウントタイマー")]
	protected float fireCountTimer = 0.0f;
	[Tooltip("現在のマガジンの弾数")]
	protected int currentMagazine;
	public int CurrentMagazine => currentMagazine;
	[Tooltip("現在の残弾数")]
	protected int currentAmmo = 40;
	public int CurrentAmmo => currentAmmo;

	[Tooltip("リロードのオン・オフ")]
	protected bool isReloadTimeActive = false;
	public bool IsReloadTimeActive => isReloadTimeActive;

	[Tooltip("リロード時間用のカウントタイマー")]
	protected float reloadCountTimer = 0.0f;

	/// <summary>
	/// セーブ
	/// </summary>
	public abstract void Save();

	/// <summary>
	/// ロード
	/// </summary>
	public abstract void Load();

	/// <summary>
	/// 一連の全ての処理
	/// </summary>
	public abstract void AllSystem(bool isAim);

	/// <summary>
	/// 射撃
	/// </summary> 
	protected void Shoot(bool isAim)
	{
		if (isAim == false)
		{
			return;
		}

		if (currentMagazine == 0)
		{
			return;
		}

		if (isReloadTimeActive == true)
		{
			return;
		}

		if (fireCountTimer <= 0.0f)//カウントタイマーが0以下の場合は中身を実行する
		{
			currentMagazine = currentMagazine - 1;//現在のマガジンの弾数を-1する
			Fire();
			fireCountTimer = fireRate;//カウントタイマーに射撃を待つ時間を入れる
		}
	}

	/// <summary>
	/// 射撃カウントタイマーリセット
	/// </summary>
	protected void ResetFireCountTimer()
	{
		//カウントタイマーが0以上なら中身を実行する
		if (0.0f < fireCountTimer)
		{
			//カウントタイマーを減らす
			fireCountTimer = fireCountTimer - Time.deltaTime;
		}
	}

	/// <summary>
	/// オートリロード
	/// </summary> 
	protected void AutoReloadTrigger()
	{
		//弾が0以下なら切り上げ
		if (currentAmmo <= 0)
		{
			return;
		}

		//残弾数が0以下なら
		if (currentMagazine <= 0)
		{
			isReloadTimeActive = true;//リロードのオン
		}
	}

	/// <summary>
	/// 手動リロード
	/// </summary>
	/// <param name="magazineCapacity">銃の最大残弾数</param>
	protected void ManualReloadTrigger(int magazineCapacity)
	{
		//残弾数が満タンなら切り上げ
		if (currentMagazine == magazineCapacity)
		{
			return;
		}

		//弾が0以下なら切り上げ
		if (currentAmmo <= 0)
		{
			return;
		}

		isReloadTimeActive = true;//リロードのオン
	}

	/// <summary>
	/// リロード
	/// </summary> 
	protected abstract void ReloadSystem();

	/// <summary>
	/// 弾を発射
	/// </summary>
	protected abstract void Fire();

	[Tooltip("弾道エフェクト（光の筋）の速度")]
	protected float tracerSpeed = 250.0f;
	[Tooltip("銃口が壁などの向こうに出ていないかを調べる、プレイヤーの体の中の基準点の高さ")]
	const float BodyCenterHeight = 1.2f;

	/// <summary>
	/// 射撃の当たり判定
	/// 1. カメラの中心から狙う先を調べる（カメラとプレイヤーの間にある物は無視）
	/// 2. 銃口が壁などにめり込んでいないか調べる（めり込んでいたらその壁に当たる）
	/// 3. 銃口から狙う先までの間に障害物が無いか調べる（あればその障害物に当たる）
	/// これで弾道エフェクトも当たり判定も障害物をすり抜けない
	/// </summary>
	/// <param name="direction">撃つ方向（カメラの向きに散乱を加えたもの）</param>
	/// <param name="muzzle">銃口</param>
	/// <param name="hit">当たった物</param>
	/// <param name="tracerStart">弾道エフェクトの始点</param>
	/// <param name="tracerEnd">弾道エフェクトの終点（着弾点、何にも当たらなければ射程の終わり）</param>
	/// <returns>何かに当たったか</returns>
	public static bool TraceShot(Vector3 direction, Transform muzzle, out RaycastHit hit, out Vector3 tracerStart, out Vector3 tracerEnd)
	{
		Transform cameraTransform = PlayerCameraManager.SingletonInstance.transform;
		float range = PlayerCameraManager.SingletonInstance.RaycastRange;
		Transform playerRoot = PlayerManagerPresenter.SingletonInstance.transform;
		direction.Normalize();
		Vector3 muzzlePosition = muzzle != null ? muzzle.position : cameraTransform.position;

		//1. カメラの中心から狙う先（カメラから銃口の深さまでは調べない）
		float skip = Mathf.Clamp(Vector3.Dot(muzzlePosition - cameraTransform.position, direction), 0.0f, range);
		Vector3 aimOrigin = cameraTransform.position + direction * skip;
		bool isAimHit = FindShotHit(aimOrigin, direction, range - skip, playerRoot, out RaycastHit aimHit);
		Vector3 aimPoint = isAimHit ? aimHit.point : aimOrigin + direction * (range - skip);

		tracerStart = muzzlePosition;

		//2. 体の中から銃口まで（銃口が壁にめり込んでいたら、その壁に当たる）
		Vector3 bodyCenter = playerRoot.position + Vector3.up * BodyCenterHeight;
		Vector3 toMuzzle = muzzlePosition - bodyCenter;
		float muzzleDistance = toMuzzle.magnitude;
		if (muzzleDistance > 0.001f && FindShotHit(bodyCenter, toMuzzle / muzzleDistance, muzzleDistance, playerRoot, out RaycastHit wallHit) == true)
		{
			hit = wallHit;
			tracerStart = wallHit.point;
			tracerEnd = wallHit.point;
			return true;
		}

		//3. 銃口から狙う先まで（途中に障害物があれば、そこに当たる）
		Vector3 toAim = aimPoint - muzzlePosition;
		float aimDistance = toAim.magnitude;
		if (aimDistance > 0.001f && FindShotHit(muzzlePosition, toAim / aimDistance, aimDistance + 0.05f, playerRoot, out RaycastHit muzzleHit) == true)
		{
			hit = muzzleHit;
			tracerEnd = muzzleHit.point;
			return true;
		}

		hit = aimHit;
		tracerEnd = aimPoint;
		return isAimHit;
	}

	/// <summary>
	/// 区間内で一番手前の当たりを探す
	/// プレイヤー自身のコライダーと、地雷・敵など以外のトリガー（コンピューターの調べる範囲など）は無視する
	/// </summary>
	static bool FindShotHit(Vector3 origin, Vector3 direction, float distance, Transform playerRoot, out RaycastHit nearest)
	{
		nearest = default;
		if (distance <= 0.0f)
		{
			return false;
		}

		RaycastHit[] hits = Physics.RaycastAll(origin, direction, distance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide);
		float nearestDistance = float.MaxValue;
		bool found = false;
		foreach (var hit in hits)
		{
			Collider collider = hit.collider;
			if (collider == null)
			{
				continue;
			}
			//プレイヤー自身
			if (playerRoot != null && (collider.transform == playerRoot || collider.transform.IsChildOf(playerRoot)))
			{
				continue;
			}
			//撃って反応する物以外のトリガー
			if (collider.isTrigger == true && IsShootableTrigger(collider) == false)
			{
				continue;
			}
			if (hit.distance < nearestDistance)
			{
				nearestDistance = hit.distance;
				nearest = hit;
				found = true;
			}
		}
		return found;
	}

	/// <summary>
	/// 撃って反応するトリガーか？（地雷など）
	/// </summary>
	static bool IsShootableTrigger(Collider collider)
	{
		return collider.CompareTag("Mine") || collider.CompareTag("Grenade") || collider.CompareTag("Enemy") || collider.CompareTag("FlyingEnemy") || collider.CompareTag("GroundEnemy");
	}

	/// <summary>
	/// 弾道エフェクト（光の筋）を銃口から着弾点まで飛ばす
	/// </summary>
	protected void FireTracer(Vector3 tracerStart, Vector3 tracerEnd)
	{
		if (EffectManager.SingletonInstance == null || EffectManager.SingletonInstance.PlayerBulletTracerPool == null)
		{
			return;
		}
		EffectManager.SingletonInstance.PlayerBulletTracerPool.Fire(tracerStart, tracerEnd, tracerSpeed);
	}

	/// <summary>
	/// 弾を取得
	/// </summary>
	/// <param name="amount">追加する弾数</param>
	/// <param name="unityAction">イベント</param>
	public abstract void AcquireAmmo(int amount = 10, UnityAction unityAction = null);
}
