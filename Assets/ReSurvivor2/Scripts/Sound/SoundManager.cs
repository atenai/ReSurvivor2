using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
	//シングルトンで作成（ゲーム中に１つのみにする）
	static SoundManager singletonInstance = null;
	public static SoundManager SingletonInstance => singletonInstance;

	[SerializeField] HitSEPool hitSEPool;
	public HitSEPool HitSEPool => hitSEPool;

	[SerializeField] HandGunShootSEPool handGunShootSEPool;
	public HandGunShootSEPool HandGunShootSEPool => handGunShootSEPool;

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

	void Update()
	{

	}
}
