using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

/// <summary>
/// ショットガンモデル
/// MVPパターンのModel担当
/// </summary>
[Serializable]
public class ShotGunModel : GunModelBase
{
	[Header("ショットガン")]

	[Tooltip("ショットガンの散乱角度")]
	[SerializeField] float shotGunRandomAngle = 5.0f;

	[Tooltip("ショットガンが一度で出る弾の数")]
	[SerializeField] int shotGunBullet = 10;

	[Tooltip("ショットガンの最大マガジン数")]
	readonly int shotGunMagazineCapacity = 8;

	[Tooltip("ショットガンの初期残弾数")]
	readonly int initCurrentShotGunAmmoDefine = 40;

	[Tooltip("ショットガンの最大残弾数")]
	readonly int maxShotGunAmmo = 80;//将来的には拡張マガジンポーチを取得すると増える的なものを入れるかも

	//※ショットガンは弾を1発ずつ込める。込める弾の数（マガジンの空き数と残弾数の少ない方）だけ「ポーチから弾を取る → 装填口に込める」を繰り返し、
	//  込めるたびにマガジンの弾が1発増える。リロード中でも構えて撃てば、そこでリロードをやめて撃てる（込め終わった弾はそのまま）
	[Tooltip("ショットガンのリロードの最初に、銃を弾を込める姿勢にして、左手が腰の後ろのポーチまで弾を取りに行くまでの時間（秒）")]
	readonly float shotGunReloadStartTime = 0.6f;

	[Tooltip("ショットガンの1発目の弾を、腰の後ろのポーチから装填口まで持っていって込めるまでの時間（秒）（2発目以降より手の動く距離が長いので、長めにする）")]
	readonly float shotGunFirstShellInsertTime = 0.4f;

	[Tooltip("ショットガンの弾を込めた後、次の弾を取りに腰のベルトへ戻るまでの時間（秒）")]
	readonly float shotGunShellReturnTime = 0.2f;

	[Tooltip("ショットガンの2発目以降の弾を、腰のベルトから装填口まで持っていって込めるまでの時間（秒）")]
	readonly float shotGunShellInsertTime = 0.25f;

	[Tooltip("ショットガンの最後の弾を込めてから、元の構えに戻るまでの時間（秒）")]
	readonly float shotGunReloadEndTime = 0.7f;

	[Tooltip("ショットガンのマガジンが空になってから、オートリロードを始めるまでの待ち時間（秒）（この間に撃つボタンを押し直すと、ハンドガンに持ち替えて撃てる）")]
	readonly float shotGunAutoReloadWaitTimeDefine = 0.5f;

	[Tooltip("今回のリロードで込める弾の数")]
	int shellCountToLoad = 0;

	[Tooltip("今回のリロードで込め終わった弾の数")]
	int loadedShellCount = 0;

	[Tooltip("リロードを始めた時に、銃に弾が1発も入っていなかったか？（入っていなかった時は、最後の弾を込めた後に弾を送り込む音を鳴らす）")]
	bool isEmptyAtReloadStart = false;

	[Tooltip("1回で込める弾の数")]
	const int ShellCountPerLoad = 1;

	[Tooltip("弾が1発も入っていない時のマガジンの弾数")]
	const int EmptyMagazineCount = 0;

	/// <summary>
	/// リロードにかかる時間（秒）（込める弾の数で変わる）
	/// </summary>
	protected override float ReloadTime => GetReloadEndTime();

	/// <summary>
	/// マガジンが空になってから、オートリロードを始めるまでの待ち時間（秒）
	/// </summary>
	protected override float AutoReloadWaitTime => shotGunAutoReloadWaitTimeDefine;

	/// <summary>
	/// コンストラクタ
	/// </summary>
	public ShotGunModel()
	{
		this.gunType = EnumManager.GunTYPE.ShotGun;
	}

	/// <summary>
	/// セーブ
	/// </summary>
	public override void Save()
	{
		Debug.Log("<color=green>ショットガンセーブ</color>");
		ES3.Save<int>("ShotGunCurrentMagazine", currentMagazine);
		ES3.Save<int>("CurrentShotGunAmmo", currentAmmo);
	}

	/// <summary>
	/// ロード
	/// </summary>
	public override void Load()
	{
		//Debug.Log("<color=green>ショットガンロード</color>");

		currentMagazine = ES3.Load<int>("ShotGunCurrentMagazine", shotGunMagazineCapacity);
		//Debug.Log("<color=purple>ショットガンマガジン : " + currentMagazine + "</color>");

		currentAmmo = ES3.Load<int>("CurrentShotGunAmmo", initCurrentShotGunAmmoDefine);
		//Debug.Log("<color=purple>ショットガン残弾数 : " + currentAmmo + "</color>");
	}

	/// <summary>
	/// 一連の全ての処理
	/// </summary>
	public override void AllSystem(bool isAim)
	{
		if (IsFireButtonDown() == true)//左クリックまたはEnterを押した場合に中身を実行する
		{
			InterruptReload(isAim);
			Shoot(isAim);
		}
		ResetFireCountTimer();
		AutoReloadTrigger();
		if (IsReloadButtonPressed() == true)
		{
			ManualReloadTrigger(shotGunMagazineCapacity);
		}
		ReloadSystem();
	}

	/// <summary>
	/// リロード（弾を1発ずつ込める）
	/// </summary>
	protected override void ReloadSystem()
	{
		if (isReloadTimeActive == false)
		{
			return;
		}

		if (reloadCountTimer == 0)
		{
			//今回込める弾の数（マガジンの空き数と残弾数の少ない方）
			shellCountToLoad = Mathf.Min(shotGunMagazineCapacity - currentMagazine, currentAmmo);
			loadedShellCount = 0;
			isEmptyAtReloadStart = currentMagazine == EmptyMagazineCount;
		}

		reloadCountTimer += Time.deltaTime;//リロードタイムをプラス

		//込めるタイミングを過ぎた弾を、1発ずつマガジンに入れる
		while (loadedShellCount < shellCountToLoad && GetShellInsertedTime(loadedShellCount) <= reloadCountTimer)
		{
			LoadShell();
		}

		if (GetReloadEndTime() <= reloadCountTimer)
		{
			FinishReload();
		}
	}

	/// <summary>
	/// 弾を1発込める
	/// </summary>
	void LoadShell()
	{
		currentMagazine = currentMagazine + ShellCountPerLoad;
		currentAmmo = currentAmmo - ShellCountPerLoad;
		loadedShellCount = loadedShellCount + ShellCountPerLoad;
		ShotGunShellInsertSE();

		//空の銃に最後の弾まで込めたら、弾を送り込む音を鳴らす
		if (loadedShellCount == shellCountToLoad && isEmptyAtReloadStart == true)
		{
			ShotGunReloadSE();
		}
	}

	/// <summary>
	/// 指定した弾を込め終わる時間（リロードを始めてからの秒数）を求める
	/// </summary>
	/// <param name="shellIndex">何発目の弾か（0が1発目）</param>
	/// <returns>その弾を込め終わる時間（秒）</returns>
	float GetShellInsertedTime(int shellIndex)
	{
		//最初 → 1発目を込める → （次の弾を取りに戻る → 込める）を2発目以降の数だけ
		return shotGunReloadStartTime + shotGunFirstShellInsertTime + (shotGunShellReturnTime + shotGunShellInsertTime) * shellIndex;
	}

	/// <summary>
	/// 今回のリロードで最後に込める弾の番号を求める
	/// </summary>
	/// <returns>最後の弾の番号（0が1発目）</returns>
	int GetLastShellIndex()
	{
		return shellCountToLoad - ShellCountPerLoad;
	}

	/// <summary>
	/// リロードが終わる時間（リロードを始めてからの秒数）を求める
	/// </summary>
	/// <returns>リロードが終わる時間（秒）</returns>
	float GetReloadEndTime()
	{
		return GetShellInsertedTime(GetLastShellIndex()) + shotGunReloadEndTime;
	}

	/// <summary>
	/// 今のリロードの段階と、その段階の進み具合を求める
	/// （最初 → 1発目を込める →「次の弾を取りに戻る → 込める」を残りの弾の数だけ繰り返す → 最後、の順に進む）
	/// </summary>
	/// <param name="phaseProgress">今の段階の進み具合（0が段階の始まり、1が終わり）</param>
	/// <returns>今のリロードの段階</returns>
	public EnumManager.ShotGunReloadPhaseTYPE GetReloadPhase(out float phaseProgress)
	{
		//最初：銃を弾を込める姿勢にして、左手がポーチまで弾を取りに行く
		if (reloadCountTimer < shotGunReloadStartTime)
		{
			phaseProgress = reloadCountTimer / shotGunReloadStartTime;
			return EnumManager.ShotGunReloadPhaseTYPE.Start;
		}

		//1発目：最初にポーチに着いた所から、装填口まで持っていって込める
		float firstShellTime = reloadCountTimer - shotGunReloadStartTime;
		if (firstShellTime < shotGunFirstShellInsertTime)
		{
			phaseProgress = firstShellTime / shotGunFirstShellInsertTime;
			return EnumManager.ShotGunReloadPhaseTYPE.FirstShellInsert;
		}

		//2発目以降：「次の弾を取りに戻る → 装填口まで持っていって込める」を、残りの弾の数だけ繰り返す
		float nextShellsTime = firstShellTime - shotGunFirstShellInsertTime;
		float shellCycleTime = shotGunShellReturnTime + shotGunShellInsertTime;
		int cycleIndex = Mathf.FloorToInt(nextShellsTime / shellCycleTime);
		if (cycleIndex < GetLastShellIndex())
		{
			float timeInCycle = nextShellsTime - shellCycleTime * cycleIndex;
			if (timeInCycle < shotGunShellReturnTime)
			{
				phaseProgress = timeInCycle / shotGunShellReturnTime;
				return EnumManager.ShotGunReloadPhaseTYPE.ShellReturn;
			}

			phaseProgress = (timeInCycle - shotGunShellReturnTime) / shotGunShellInsertTime;
			return EnumManager.ShotGunReloadPhaseTYPE.ShellInsert;
		}

		//最後：最後の弾を込めてから、元の構えに戻る
		phaseProgress = (reloadCountTimer - GetShellInsertedTime(GetLastShellIndex())) / shotGunReloadEndTime;
		return EnumManager.ShotGunReloadPhaseTYPE.End;
	}

	/// <summary>
	/// リロード中に撃とうとした時は、そこでリロードをやめる（込め終わった弾はそのまま）
	/// </summary>
	/// <param name="isAim">構えているか？</param>
	void InterruptReload(bool isAim)
	{
		if (isReloadTimeActive == false)
		{
			return;
		}

		//構えていない時と、まだ1発も入っていない時は撃てないので、リロードを続ける
		if (isAim == false || currentMagazine <= EmptyMagazineCount)
		{
			return;
		}

		FinishReload();
	}

	/// <summary>
	/// リロードを終える
	/// </summary>
	void FinishReload()
	{
		reloadCountTimer = 0.0f;//リロードタイムをリセット
		isReloadTimeActive = false;//リロードのオフ
	}

	/// <summary>
	/// 弾を発射
	/// </summary> 
	protected override void Fire()
	{
		ShotGunBulletCasingSE();
		PlayerManagerPresenter.SingletonInstance.PlayerCharacterView.GunViewFacade.ShotGunView.ShotGunMuzzleFlashAndShell();
		PlayerManagerPresenter.SingletonInstance.PlayerCharacterView.GunViewFacade.ShotGunView.ShotGunSmoke();

		ShotGunFireSE();
		bool isOnceShotGunHitSE = false;

		for (int i = 0; i < shotGunBullet; i++)
		{
			Vector3 direction = PlayerCameraManager.SingletonInstance.transform.forward;
			direction = Quaternion.AngleAxis(UnityEngine.Random.Range(-shotGunRandomAngle, shotGunRandomAngle), PlayerCameraManager.SingletonInstance.transform.up) * direction;
			direction = Quaternion.AngleAxis(UnityEngine.Random.Range(-shotGunRandomAngle, shotGunRandomAngle), PlayerCameraManager.SingletonInstance.transform.right) * direction;

			Ray ray = new Ray(PlayerCameraManager.SingletonInstance.transform.position, direction);
			Debug.DrawRay(ray.origin, ray.direction * PlayerCameraManager.SingletonInstance.RaycastRange, Color.red, 10.0f);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, PlayerCameraManager.SingletonInstance.RaycastRange) == true) // もしRayを投射して何らかのコライダーに衝突したら
			{
#if UNITY_EDITOR//Unityエディター上での処理
				PlayerCameraManager.SingletonInstance.HitName = hit.collider.gameObject.name; // 衝突した相手オブジェクトの名前を取得
#endif //終了 
				if (hit.collider.gameObject.CompareTag("Enemy") || hit.collider.gameObject.CompareTag("FlyingEnemy") || hit.collider.gameObject.CompareTag("GroundEnemy") || hit.collider.gameObject.CompareTag("Mine") || hit.collider.gameObject.CompareTag("Grenade"))//※間違ってオブジェクトの設定にレイヤーとタグを間違えるなよおれｗ
				{
					//ダメージ
					IEnemy enemy = hit.transform.GetComponent<IEnemy>();
					if (enemy != null)
					{
						enemy.GetHitPoint().Damage(damage);
					}

					//着弾した物体を後ろに押す
					if (hit.rigidbody != null)
					{
						if (hit.collider.gameObject.CompareTag("FlyingEnemy") == false && hit.collider.gameObject.CompareTag("GroundEnemy") == false)
						{
							hit.rigidbody.AddForce(-hit.normal * impactForce);
						}
					}

					//ヒットレティクルを表示
					ScreenUIManagerPresenter.SingletonInstance.ScreenUIModel.IsHitReticule = true;

					if (isOnceShotGunHitSE == false)
					{
						isOnceShotGunHitSE = true;
						//ヒット音を再生
						SoundManager.SingletonInstance.HitSEPool.GetGameObject(PlayerCameraManager.SingletonInstance.transform);
					}

					//地雷を爆破
					Mine mine = hit.transform.GetComponent<Mine>();
					if (mine != null)
					{
						mine.Explosion();
					}

					//グレネードを爆破
					Grenade grenade = hit.transform.GetComponent<Grenade>();
					if (grenade != null)
					{
						grenade.Explosion();
					}
				}

				EffectManager.SingletonInstance.ImpactEffect(hit);
			}
		}

		//追跡開始
		EnemyManager.SingletonInstance.ChaseEvent.Invoke();
	}

	/// <summary>
	/// ショットガンの射撃SE
	/// </summary> 
	void ShotGunFireSE()
	{
		SoundManager.SingletonInstance.ShotGunShootSEPool.GetGameObject(PlayerManagerPresenter.SingletonInstance.PlayerCharacterView.GunViewFacade.ShotGunView.ShotGunMuzzleTransform);
	}

	/// <summary>
	/// ショットガンのリロードSE（空の銃に弾を込め終わった時に、弾を送り込む音として鳴らす）
	/// </summary>
	void ShotGunReloadSE()
	{
		SoundManager.SingletonInstance.ShotGunReloadSEPool.GetGameObject(PlayerManagerPresenter.SingletonInstance.PlayerCharacterView.GunViewFacade.ShotGunView.ShotGunBulletCasingTransform);
	}

	/// <summary>
	/// ショットガンの弾込めSE（弾を1発込めるたびに鳴らす）
	/// </summary>
	void ShotGunShellInsertSE()
	{
		SoundManager.SingletonInstance.ShotGunShellInsertSEPool.GetGameObject(PlayerManagerPresenter.SingletonInstance.PlayerCharacterView.GunViewFacade.ShotGunView.ShotGunBulletCasingTransform);
	}

	/// <summary>
	/// ショットガンの薬莢SE
	/// </summary>
	void ShotGunBulletCasingSE()
	{
		SoundManager.SingletonInstance.ShotGunBulletCasingSEPool.GetGameObject(PlayerManagerPresenter.SingletonInstance.PlayerCharacterView.GunViewFacade.ShotGunView.ShotGunBulletCasingTransform);
	}

	/// <summary>
	/// 弾を取得
	/// </summary> 
	/// <param name="amount">追加する弾数</param>
	/// <param name="unityAction">イベント</param>
	public override void AcquireAmmo(int amount = 10, UnityAction unityAction = null)
	{
		if (maxShotGunAmmo <= currentAmmo)
		{
			return;
		}

		currentAmmo = currentAmmo + amount;
		if (maxShotGunAmmo <= currentAmmo)
		{
			currentAmmo = maxShotGunAmmo;
		}

		unityAction?.Invoke();
	}
}
