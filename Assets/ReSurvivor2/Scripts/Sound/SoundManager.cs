using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// サウンドマネージャー
/// </summary>
public class SoundManager : MonoBehaviour
{
	/// <summary>
	/// シングルトンで作成（ゲーム中に１つのみにする）
	/// </summary>
	static SoundManager singletonInstance = null;
	public static SoundManager SingletonInstance => singletonInstance;

	[SerializeField] HitSEPool hitSEPool;
	public HitSEPool HitSEPool => hitSEPool;

	[SerializeField] HandGunShootSEPool handGunShootSEPool;
	public HandGunShootSEPool HandGunShootSEPool => handGunShootSEPool;

	[SerializeField] HandGunBulletCasingSEPool handGunBulletCasingSEPool;
	public HandGunBulletCasingSEPool HandGunBulletCasingSEPool => handGunBulletCasingSEPool;

	[SerializeField] HandGunReloadSEPool handGunReloadSEPool;
	public HandGunReloadSEPool HandGunReloadSEPool => handGunReloadSEPool;

	[SerializeField] AssaultRifleShootSEPool assaultRifleShootSEPool;
	public AssaultRifleShootSEPool AssaultRifleShootSEPool => assaultRifleShootSEPool;

	[SerializeField] AssaultRifleBulletCasingSEPool assaultRifleBulletCasingSEPool;
	public AssaultRifleBulletCasingSEPool AssaultRifleBulletCasingSEPool => assaultRifleBulletCasingSEPool;

	[SerializeField] AssaultRifleReloadSEPool assaultRifleReloadSEPool;
	public AssaultRifleReloadSEPool AssaultRifleReloadSEPool => assaultRifleReloadSEPool;

	[SerializeField] ShotGunShootSEPool shotGunShootSEPool;
	public ShotGunShootSEPool ShotGunShootSEPool => shotGunShootSEPool;

	[SerializeField] ShotGunBulletCasingSEPool shotGunBulletCasingSEPool;
	public ShotGunBulletCasingSEPool ShotGunBulletCasingSEPool => shotGunBulletCasingSEPool;

	[SerializeField] ShotGunReloadSEPool shotGunReloadSEPool;
	public ShotGunReloadSEPool ShotGunReloadSEPool => shotGunReloadSEPool;

	void Awake()
	{
		//staticな変数instanceはメモリ領域は確保されていますが、初回では中身が入っていないので、中身を入れます。
		if (singletonInstance == null)
		{
			singletonInstance = this;//thisというのは自分自身のインスタンスという意味になります。この場合、Playerのインスタンスという意味になります。
			DontDestroyOnLoad(this.gameObject);//シーンを切り替えた時に破棄しない
		}
		else
		{
			Destroy(this.gameObject);//中身がすでに入っていた場合、自身のインスタンスがくっついているゲームオブジェクトを破棄します。
		}
	}
}
