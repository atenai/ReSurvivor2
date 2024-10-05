using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Knife.Effects;

public class PlayerCamera : MonoBehaviour
{
    //シングルトンで作成（ゲーム中に１つのみにする）
    static PlayerCamera singletonInstance = null;
    public static PlayerCamera SingletonInstance => singletonInstance;

    [Header("カメラ")]
    [Tooltip("エディターで実行ロード時にマウスの座標が入力されてカメラが動いてしまう問題の対処用")]
    bool isActiveCamera = false;
    public bool IsActiveCamera => isActiveCamera;
    [Tooltip("X軸のカメラの回転スピード")]
    [Range(50, 150)][SerializeField] float normalCameraSpeedX = 100;
    [Tooltip("Y軸のカメラの回転スピード")]
    [Range(25, 125)][SerializeField] float normalCameraSpeedY = 50;

    [Tooltip("X軸のカメラの回転スピード")]
    [Range(50, 150)][SerializeField] float aimCameraSpeedX = 50;
    [Tooltip("Y軸のカメラの回転スピード")]
    [Range(25, 125)][SerializeField] float aimCameraSpeedY = 25;

    [Tooltip("X軸のカメラの入力デッドゾーン")]
    [Range(0.001f, 0.1f)][SerializeField] float deadZoneX = 0.1f;
    [Tooltip("Y軸のカメラの入力デッドゾーン")]
    [Range(0.001f, 0.1f)][SerializeField] float deadZoneY = 0.1f;

    [Tooltip("レティクルの中心点（レイキャスト）にターゲットがヒットしてるか？")]
    bool isTargethit = false;
    public bool IsTargethit => isTargethit;
    [Tooltip("ローカルで計算する為のX軸のカメラの回転スピード")]
    float localCameraSpeedX;
    [Tooltip("ローカルで計算する為のY軸のカメラの回転スピード")]
    float localCameraSpeedY;
    [Tooltip("カメラのスピードを遅くする")]
    [Range(1.0f, 4.0f)] float slowDownCameraSpeed = 2.0f;

    [Tooltip("通常カメラのy位置")]
    const float normalUpPos = 1.6f;
    public float NormalUpPos => normalUpPos;
    [Tooltip("通常カメラのz位置")]
    const float normalForwardPos = -4.0f;

    [Tooltip("肩越しカメラのx位置")]
    const float aimRightPos = 0.5f;
    [Tooltip("肩越しカメラのy位置")]
    const float aimUpPos = 1.6f;
    public float AimUpPos => aimUpPos;
    [Tooltip("肩越しカメラのz位置")]
    const float aimForwardPos = -0.6f;

    [Header("レイキャスト")]
    [Tooltip("ヒットしたオブジェクトの名前")]
    string hitName = "";
    [Tooltip("レイの長さ")]
    [SerializeField] float range = 100.0f;
    [Tooltip("銃のダメージ")]
    [SerializeField] float Damage = 10.0f;
    [Tooltip("着弾した物体を後ろに押す")]
    [SerializeField] float impactForce = 30.0f;

    [Header("ハンドガン")]
    [Tooltip("ハンドガンを何秒間隔で撃つか")]
    [SerializeField] float handGunFireRate = 0.1f;
    [Tooltip("ハンドガンの射撃間隔の時間カウント用のタイマー")]
    float handGunCountTimer = 0.0f;
    [Tooltip("ハンドガンの現在のマガジンの弾数")]
    int handGunCurrentMagazine;
    public int HandGunCurrentMagazine => handGunCurrentMagazine;
    [Tooltip("ハンドガンの最大マガジン数")]
    readonly int handGunMagazineCapacity = 7;
    [Tooltip("ハンドガンの残弾数")]
    int handGunAmmo = 35;
    public int HandGunAmmo => handGunAmmo;

    [Tooltip("ハンドガンのリロードのオン・オフ")]
    bool isHandGunReloadTimeActive = false;
    public bool IsHandGunReloadTimeActive => isHandGunReloadTimeActive;
    float handGunReloadTime = 0.0f;
    [Tooltip("ハンドガンのリロード時間")]
    readonly float handGunReloadTimeDefine = 1.5f;

    //↓アセットストアのプログラム↓//
    [Tooltip("ハンドガンのマズルフラッシュと薬莢")]
    [SerializeField] ParticleGroupEmitter[] handGunShotEmitters;
    [Tooltip("ハンドガンの硝煙")]
    [SerializeField] ParticleGroupPlayer handGunAfterFireSmoke;
    //↑アセットストアのプログラム↑//

    [Header("アサルトライフル")]
    [Tooltip("アサルトライフルを何秒間隔で撃つか")]
    [SerializeField] float assaultRifleFireRate = 0.1f;
    [Tooltip("アサルトライフルの射撃間隔の時間カウント用のタイマー")]
    float assaultRifleCountTimer = 0.0f;
    [Tooltip("アサルトライフルの散乱角度")]
    [SerializeField] float assaultRifleRandomAngle = 1.0f;
    [Tooltip("アサルトライフルの現在のマガジンの弾数")]
    int assaultRifleCurrentMagazine;
    public int AssaultRifleCurrentMagazine => assaultRifleCurrentMagazine;
    [Tooltip("アサルトライフルの最大マガジン数")]
    readonly int assaultRifleMagazineCapacity = 30;
    [Tooltip("アサルトライフルの残弾数")]
    int assaultRifleAmmo = 150;
    public int AssaultRifleAmmo => assaultRifleAmmo;

    [Tooltip("アサルトライフルのリロードのオン・オフ")]
    bool isAssaultRifleReloadTimeActive = false;
    public bool IsAssaultRifleReloadTimeActive => isAssaultRifleReloadTimeActive;
    float assaultRifleReloadTime = 0.0f;
    [Tooltip("アサルトライフルのリロード時間")]
    readonly float assaultRifleReloadTimeDefine = 1.5f;

    //↓アセットストアのプログラム↓//
    [Tooltip("アサルトライフルのマズルフラッシュと薬莢")]
    [SerializeField] ParticleGroupEmitter[] assaultRifleShotEmitters;
    [Tooltip("アサルトライフルの硝煙")]
    [SerializeField] ParticleGroupPlayer assaultRifleAfterFireSmoke;
    //↑アセットストアのプログラム↑//

    [Header("ショットガン")]
    [Tooltip("ショットガンを何秒間隔で撃つか")]
    [SerializeField] float shotGunFireRate = 0.1f;
    [Tooltip("ショットガンの射撃間隔の時間カウント用のタイマー")]
    float shotGunCountTimer = 0.0f;
    [Tooltip("ショットガンの散乱角度")]
    [SerializeField] float shotGunRandomAngle = 5.0f;
    [Tooltip("ショットガンが一度で出る弾の数")]
    [SerializeField] int shotGunBullet = 10;
    [Tooltip("ショットガンの現在のマガジンの弾数")]
    int shotGunCurrentMagazine;
    public int ShotGunCurrentMagazine => shotGunCurrentMagazine;
    [Tooltip("ショットガンの最大マガジン数")]
    readonly int shotGunMagazineCapacity = 8;
    [Tooltip("ショットガンの残弾数")]
    int shotGunAmmo = 40;
    public int ShotGunAmmo => shotGunAmmo;

    [Tooltip("ショットガンのリロードのオン・オフ")]
    bool isShotGunReloadTimeActive = false;
    public bool IsShotGunReloadTimeActive => isShotGunReloadTimeActive;
    float shotGunReloadTime = 0.0f;
    [Tooltip("ショットガンのリロード時間")]
    readonly float shotGunReloadTimeDefine = 1.5f;

    //↓アセットストアのプログラム↓//
    [Tooltip("ショットガンのマズルフラッシュと薬莢")]
    [SerializeField] ParticleGroupEmitter[] shotGunShotEmitters;
    [Tooltip("ショットガンの硝煙")]
    [SerializeField] ParticleGroupPlayer shotGunAfterFireSmoke;
    //↑アセットストアのプログラム↑//

    [Header("着弾エフェクト")]
    //パス(Assets/Knife/PRO Effects FPS Muzzle flashes & Impacts/Particles/Prefabs/Impacts)
    [Tooltip("血の着弾エフェクト")]
    [SerializeField] GameObject bloodImpactEffect;
    [Tooltip("煙の着弾エフェクト")]
    [SerializeField] GameObject rockImpactEffect;

    public enum GunTYPE
    {
        HandGun = 1,
        AssaultRifle = 2,
        ShotGun = 3,
    }
    public GunTYPE gunTYPE = GunTYPE.AssaultRifle;

    void Awake()
    {
        //staticな変数instanceはメモリ領域は確保されていますが、初回では中身が入っていないので、中身を入れます。
        if (singletonInstance == null)
        {
            singletonInstance = this;//thisというのは自分自身のインスタンスという意味になります。この場合、PlayerCameraのインスタンスという意味になります。
            DontDestroyOnLoad(this.gameObject);//シーンを切り替えた時に破棄しない
        }
        else
        {
            Destroy(this.gameObject);//中身がすでに入っていた場合、自身のインスタンスがくっついているゲームオブジェクトを破棄します。
        }
    }

    IEnumerator Start()
    {
        //各種初期化処理
        InitHandGunMagazine();
        InitAssaultRifleMagazine();
        InitShotGunMagazine();

        //起動時のロードの際にマウスの入力でカメラが思わぬ方向に動いてしまうので、1秒間カメラ操作を受け付けなくする
        //（ここの処理をごまかす為、ゲーム開始時にフェードイン・フェードアウトを行い、フェードアウト後にisActiveCamera = true;にするように修正する必要がある）
        yield return new WaitForSeconds(1.0f);
        isActiveCamera = true;

        yield return null;
    }

    /// <summary>
    /// ハンドガンの弾数の初期化処理
    /// </summary>
    void InitHandGunMagazine()
    {
        //ハンドガンの残弾数が満タンなら切り上げ
        if (handGunCurrentMagazine == handGunMagazineCapacity)
        {
            return;
        }

        //弾が0以下なら切り上げ
        if (handGunAmmo <= 0)
        {
            return;
        }

        int localMagazine = handGunMagazineCapacity - handGunCurrentMagazine;
        int localAmmo = handGunAmmo - localMagazine;
        if (localAmmo < 0)
        {
            handGunCurrentMagazine = handGunAmmo;
            handGunAmmo = 0;
        }
        else
        {
            handGunCurrentMagazine = handGunMagazineCapacity;
            handGunAmmo = localAmmo;
        }
    }

    /// <summary>
    /// アサルトライフルの弾数の初期化処理
    /// </summary>
    void InitAssaultRifleMagazine()
    {
        //残弾数が満タンなら切り上げ
        if (assaultRifleCurrentMagazine == assaultRifleMagazineCapacity)
        {
            return;
        }

        //弾が0以下なら切り上げ
        if (assaultRifleAmmo <= 0)
        {
            return;
        }

        int localMagazine = assaultRifleMagazineCapacity - assaultRifleCurrentMagazine;
        int localAmmo = assaultRifleAmmo - localMagazine;
        if (localAmmo < 0)
        {
            assaultRifleCurrentMagazine = assaultRifleAmmo;
            assaultRifleAmmo = 0;
        }
        else
        {
            assaultRifleCurrentMagazine = assaultRifleMagazineCapacity;
            assaultRifleAmmo = localAmmo;
        }
    }

    /// <summary>
    /// ショットガンの弾数の初期化処理
    /// </summary>
    void InitShotGunMagazine()
    {
        //残弾数が満タンなら切り上げ
        if (shotGunCurrentMagazine == shotGunMagazineCapacity)
        {
            return;
        }

        //弾が0以下なら切り上げ
        if (shotGunAmmo <= 0)
        {
            return;
        }

        int localMagazine = shotGunMagazineCapacity - shotGunCurrentMagazine;
        int localAmmo = shotGunAmmo - localMagazine;
        if (localAmmo < 0)
        {
            shotGunCurrentMagazine = shotGunAmmo;
            shotGunAmmo = 0;
        }
        else
        {
            shotGunCurrentMagazine = shotGunMagazineCapacity;
            shotGunAmmo = localAmmo;
        }
    }

    void Update()
    {
        if (isActiveCamera == false)
        {
            return;
        }

        SwitchWeapon();
    }

    /// <summary>
    /// 武器の切り替え
    /// </summary> 
    void SwitchWeapon()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            //Debug.Log("ハンドガン");
            gunTYPE = GunTYPE.HandGun;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            //Debug.Log("アサルトライフル");
            gunTYPE = GunTYPE.AssaultRifle;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            //Debug.Log("ショットガン");
            gunTYPE = GunTYPE.ShotGun;
        }

        switch (gunTYPE)
        {
            case GunTYPE.HandGun:
                HandGunShoot();
                HandGunAutoReload();
                HandGunManualReload();
                HandGunReload();
                break;
            case GunTYPE.AssaultRifle:
                AssaultRifleShoot();
                AssaultRifleAutoReload();
                AssaultRifleManualReload();
                AssaultRifleReload();
                break;
            case GunTYPE.ShotGun:
                ShotGunShoot();
                ShotGunAutoReload();
                ShotGunManualReload();
                ShotGunReload();
                break;
        }
    }

    /// <summary>
    /// ハンドガンで射撃
    /// </summary> 
    void HandGunShoot()
    {
        if (Player.SingletonInstance.IsAim == true)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return))//左クリックまたはEnterを押している場合に中身を実行する
            {
                if (handGunCountTimer <= 0.0f)//カウントタイマーが0以下の場合は中身を実行する
                {
                    if (handGunCurrentMagazine != 0)
                    {
                        if (isHandGunReloadTimeActive == false)
                        {
                            handGunCurrentMagazine = handGunCurrentMagazine - 1;//現在のマガジンの弾数を-1する
                            HandGunFire();
                            handGunCountTimer = handGunFireRate;//カウントタイマーに射撃を待つ時間を入れる
                        }
                    }
                }
            }
        }

        //カウントタイマーが0以上なら中身を実行する
        if (0.0f < handGunCountTimer)
        {
            //カウントタイマーを減らす
            handGunCountTimer = handGunCountTimer - Time.deltaTime;
        }
    }

    /// <summary>
    /// ハンドガンのオートリロード
    /// </summary> 
    void HandGunAutoReload()
    {
        //残弾数が0かつ弾薬が1発以上あるとき
        if (handGunCurrentMagazine == 0 && 0 < handGunAmmo)
        {
            isHandGunReloadTimeActive = true;//リロードのオン
        }
    }

    /// <summary>
    /// ハンドガンの手動リロード
    /// </summary> 
    void HandGunManualReload()
    {
        //残弾数が満タンなら切り上げ
        if (handGunCurrentMagazine == handGunMagazineCapacity)
        {
            return;
        }

        //弾が0以下なら切り上げ
        if (handGunAmmo <= 0)
        {
            return;
        }

        if (Input.GetKey(KeyCode.R))
        {
            isHandGunReloadTimeActive = true;//リロードのオン
        }
    }

    /// <summary>
    /// ハンドガンのリロード
    /// </summary> 
    void HandGunReload()
    {
        if (isHandGunReloadTimeActive == true)//リロードがオンになったら
        {
            if (handGunReloadTime == 0)
            {
                //ハンドガンのリロードアニメーションをオン
                Player.SingletonInstance.Animator.SetBool("b_isHandGunReload", true);
            }

            //リロード中画像
            handGunReloadTime += Time.deltaTime;//リロードタイムをプラス

            if (handGunReloadTimeDefine <= handGunReloadTime)//リロードタイムが10以上になったら
            {
                //弾リセット
                int oldMagazine = handGunCurrentMagazine;
                int localMagazine = handGunMagazineCapacity - handGunCurrentMagazine;
                int localAmmo = handGunAmmo - localMagazine;
                if (localAmmo < 0)
                {
                    if (handGunAmmo + oldMagazine < handGunMagazineCapacity)
                    {
                        handGunCurrentMagazine = handGunAmmo + oldMagazine;
                        handGunAmmo = 0;
                    }
                    else
                    {
                        handGunCurrentMagazine = handGunMagazineCapacity;
                        int totalAmmo = handGunAmmo + oldMagazine;
                        int resultAmmo = totalAmmo - handGunMagazineCapacity;
                        handGunAmmo = resultAmmo;
                    }
                }
                else
                {
                    handGunCurrentMagazine = handGunMagazineCapacity;
                    handGunAmmo = localAmmo;
                }

                handGunReloadTime = 0.0f;//リロードタイムをリセット
                isHandGunReloadTimeActive = false;//リロードのオフ
                //ハンドガンのリロードアニメーションをオフ
                Player.SingletonInstance.Animator.SetBool("b_isHandGunReload", false);
            }
        }
    }

    /// <summary>
    /// 弾を発射
    /// </summary> 
    void HandGunFire()
    {
        HandGunMuzzleFlashAndShell();
        HandGunSmoke();

        Ray ray = new Ray(this.transform.position, this.transform.forward);
        Debug.DrawRay(ray.origin, ray.direction * range, Color.red, 10.0f);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, range) == true) // もしRayを投射して何らかのコライダーに衝突したら
        {
            hitName = hit.collider.gameObject.name; // 衝突した相手オブジェクトの名前を取得

            if (hit.collider.gameObject.CompareTag("Enemy") || hit.collider.gameObject.CompareTag("FlyingEnemy") || hit.collider.gameObject.CompareTag("GroundEnemy"))//※間違ってオブジェクトの設定にレイヤーとタグを間違えるなよおれｗ
            {
                //ダメージ
                Target target = hit.transform.GetComponent<Target>();
                if (target != null)
                {
                    target.TakeDamage(Damage);
                }

                //着弾した物体を後ろに押す
                if (hit.rigidbody != null)
                {
                    hit.rigidbody.AddForce(-hit.normal * impactForce);
                }
            }

            ImpactEffect(hit);
        }
    }

    /// <summary>
    /// マズルフラッシュのエフェクトと薬莢を出す（アセットストアで買ったコードをそのままもってきている）
    /// </summary>
    void HandGunMuzzleFlashAndShell()
    {
        if (handGunShotEmitters != null)
        {
            foreach (var effect in handGunShotEmitters)
            {
                effect.Emit(1);
            }
        }
    }

    /// <summary>
    /// 硝煙のエフェクトを出す（アセットストアで買ったコードをそのままもってきている）
    /// </summary>
    void HandGunSmoke()
    {
        if (handGunAfterFireSmoke != null)
        {
            handGunAfterFireSmoke.Play();
        }
    }

    /// <summary>
    /// アサルトライフルで射撃
    /// </summary> 
    void AssaultRifleShoot()
    {
        if (Player.SingletonInstance.IsAim == true)
        {
            if (Input.GetMouseButton(0) || Input.GetKey(KeyCode.Return))//左クリックまたはEnterを押している場合に中身を実行する
            {
                if (assaultRifleCountTimer <= 0.0f)//カウントタイマーが0以下の場合は中身を実行する
                {
                    if (assaultRifleCurrentMagazine != 0)
                    {
                        if (isAssaultRifleReloadTimeActive == false)
                        {
                            assaultRifleCurrentMagazine = assaultRifleCurrentMagazine - 1;//現在のマガジンの弾数を-1する
                            AssaultRifleFire();
                            assaultRifleCountTimer = assaultRifleFireRate;//カウントタイマーに射撃を待つ時間を入れる
                        }
                    }
                }
            }
        }

        //カウントタイマーが0以上なら中身を実行する
        if (0.0f < assaultRifleCountTimer)
        {
            //カウントタイマーを減らす
            assaultRifleCountTimer = assaultRifleCountTimer - Time.deltaTime;
        }
    }

    /// <summary>
    /// アサルトライフルのオートリロード
    /// </summary> 
    void AssaultRifleAutoReload()
    {
        //残弾数が0かつの弾薬が1発以上あるとき
        if (assaultRifleCurrentMagazine == 0 && 0 < assaultRifleAmmo)
        {
            isAssaultRifleReloadTimeActive = true;//リロードのオン
        }
    }

    /// <summary>
    /// アサルトライフルの手動リロード
    /// </summary> 
    void AssaultRifleManualReload()
    {
        //残弾数が満タンなら切り上げ
        if (assaultRifleCurrentMagazine == assaultRifleMagazineCapacity)
        {
            return;
        }

        //弾が0以下なら切り上げ
        if (assaultRifleAmmo <= 0)
        {
            return;
        }

        if (Input.GetKey(KeyCode.R))
        {
            isAssaultRifleReloadTimeActive = true;//リロードのオン
        }
    }

    /// <summary>
    /// アサルトライフルのリロード
    /// </summary> 
    void AssaultRifleReload()
    {
        if (isAssaultRifleReloadTimeActive == true)//リロードがオンになったら
        {
            //リロード中画像
            assaultRifleReloadTime += Time.deltaTime;//リロードタイムをプラス

            if (assaultRifleReloadTimeDefine <= assaultRifleReloadTime)//リロードタイムが10以上になったら
            {
                //弾リセット
                int oldMagazine = assaultRifleCurrentMagazine;
                int localMagazine = assaultRifleMagazineCapacity - assaultRifleCurrentMagazine;
                int localAmmo = assaultRifleAmmo - localMagazine;
                if (localAmmo < 0)
                {
                    if (assaultRifleAmmo + oldMagazine < assaultRifleMagazineCapacity)
                    {
                        assaultRifleCurrentMagazine = assaultRifleAmmo + oldMagazine;
                        assaultRifleAmmo = 0;
                    }
                    else
                    {
                        assaultRifleCurrentMagazine = assaultRifleMagazineCapacity;
                        int totalAmmo = assaultRifleAmmo + oldMagazine;
                        int resultAmmo = totalAmmo - assaultRifleMagazineCapacity;
                        assaultRifleAmmo = resultAmmo;
                    }
                }
                else
                {
                    assaultRifleCurrentMagazine = assaultRifleMagazineCapacity;
                    assaultRifleAmmo = localAmmo;
                }

                assaultRifleReloadTime = 0.0f;//リロードタイムをリセット
                isAssaultRifleReloadTimeActive = false;//リロードのオフ
            }
        }
    }

    /// <summary>
    /// 弾を発射
    /// </summary> 
    void AssaultRifleFire()
    {
        AssaultRifleMuzzleFlashAndShell();
        AssaultRifleSmoke();

        Vector3 direction = this.transform.forward;
        direction = Quaternion.AngleAxis(Random.Range(-assaultRifleRandomAngle, assaultRifleRandomAngle), this.transform.up) * direction;
        direction = Quaternion.AngleAxis(Random.Range(-assaultRifleRandomAngle, assaultRifleRandomAngle), this.transform.right) * direction;

        Ray ray = new Ray(this.transform.position, direction);
        Debug.DrawRay(ray.origin, ray.direction * range, Color.red, 10.0f);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, range) == true) // もしRayを投射して何らかのコライダーに衝突したら
        {
            hitName = hit.collider.gameObject.name; // 衝突した相手オブジェクトの名前を取得

            if (hit.collider.gameObject.CompareTag("Enemy") || hit.collider.gameObject.CompareTag("FlyingEnemy") || hit.collider.gameObject.CompareTag("GroundEnemy"))//※間違ってオブジェクトの設定にレイヤーとタグを間違えるなよおれｗ
            {
                //ダメージ
                Target target = hit.transform.GetComponent<Target>();
                if (target != null)
                {
                    target.TakeDamage(Damage);
                }

                //着弾した物体を後ろに押す
                if (hit.rigidbody != null)
                {
                    hit.rigidbody.AddForce(-hit.normal * impactForce);
                }
            }

            ImpactEffect(hit);
        }
    }

    /// <summary>
    /// マズルフラッシュのエフェクトと薬莢を出す（アセットストアで買ったコードをそのままもってきている）
    /// </summary>
    void AssaultRifleMuzzleFlashAndShell()
    {
        if (assaultRifleShotEmitters != null)
        {
            foreach (var effect in assaultRifleShotEmitters)
            {
                effect.Emit(1);
            }
        }
    }

    /// <summary>
    /// 硝煙のエフェクトを出す（アセットストアで買ったコードをそのままもってきている）
    /// </summary>
    void AssaultRifleSmoke()
    {
        if (assaultRifleAfterFireSmoke != null)
        {
            assaultRifleAfterFireSmoke.Play();
        }
    }

    /// <summary>
    /// ショットガンで射撃
    /// </summary> 
    void ShotGunShoot()
    {
        if (Player.SingletonInstance.IsAim == true)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return))//左クリックまたはEnterを押している場合に中身を実行する
            {
                if (shotGunCountTimer <= 0.0f)//カウントタイマーが0以下の場合は中身を実行する
                {
                    if (shotGunCurrentMagazine != 0)
                    {
                        if (isShotGunReloadTimeActive == false)
                        {
                            shotGunCurrentMagazine = shotGunCurrentMagazine - 1;//現在のマガジンの弾数を-1する
                            ShotGunFire();
                            shotGunCountTimer = shotGunFireRate;//カウントタイマーに射撃を待つ時間を入れる
                        }
                    }
                }
            }
        }

        //カウントタイマーが0以上なら中身を実行する
        if (0.0f < shotGunCountTimer)
        {
            //カウントタイマーを減らす
            shotGunCountTimer = shotGunCountTimer - Time.deltaTime;
        }
    }

    /// <summary>
    /// ショットガンのオートリロード
    /// </summary> 
    void ShotGunAutoReload()
    {
        //残弾数が0かつの弾薬が1発以上あるとき
        if (shotGunCurrentMagazine == 0 && 0 < shotGunAmmo)
        {
            isShotGunReloadTimeActive = true;//リロードのオン
        }
    }

    /// <summary>
    /// ショットガンの手動リロード
    /// </summary> 
    void ShotGunManualReload()
    {
        //残弾数が満タンなら切り上げ
        if (shotGunCurrentMagazine == shotGunMagazineCapacity)
        {
            return;
        }

        //弾が0以下なら切り上げ
        if (shotGunAmmo <= 0)
        {
            return;
        }

        if (Input.GetKey(KeyCode.R))
        {
            isShotGunReloadTimeActive = true;//リロードのオン
        }
    }

    /// <summary>
    /// ショットガンのリロード
    /// </summary> 
    void ShotGunReload()
    {
        if (isShotGunReloadTimeActive == true)//リロードがオンになったら
        {
            //リロード中画像
            shotGunReloadTime += Time.deltaTime;//リロードタイムをプラス

            if (shotGunReloadTimeDefine <= shotGunReloadTime)//リロードタイムが10以上になったら
            {
                //弾リセット
                int oldMagazine = shotGunCurrentMagazine;
                int localMagazine = shotGunMagazineCapacity - shotGunCurrentMagazine;
                int localAmmo = shotGunAmmo - localMagazine;
                if (localAmmo < 0)
                {
                    if (shotGunAmmo + oldMagazine < shotGunMagazineCapacity)
                    {
                        shotGunCurrentMagazine = shotGunAmmo + oldMagazine;
                        shotGunAmmo = 0;
                    }
                    else
                    {
                        shotGunCurrentMagazine = shotGunMagazineCapacity;
                        int totalAmmo = shotGunAmmo + oldMagazine;
                        int resultAmmo = totalAmmo - shotGunMagazineCapacity;
                        shotGunAmmo = resultAmmo;
                    }
                }
                else
                {
                    shotGunCurrentMagazine = shotGunMagazineCapacity;
                    shotGunAmmo = localAmmo;
                }

                shotGunReloadTime = 0.0f;//リロードタイムをリセット
                isShotGunReloadTimeActive = false;//リロードのオフ
            }
        }
    }

    /// <summary>
    /// 弾を発射
    /// </summary> 
    void ShotGunFire()
    {
        ShotGunMuzzleFlashAndShell();
        ShotGunSmoke();

        for (int i = 0; i < shotGunBullet; i++)
        {
            Vector3 direction = this.transform.forward;
            direction = Quaternion.AngleAxis(Random.Range(-shotGunRandomAngle, shotGunRandomAngle), this.transform.up) * direction;
            direction = Quaternion.AngleAxis(Random.Range(-shotGunRandomAngle, shotGunRandomAngle), this.transform.right) * direction;

            Ray ray = new Ray(this.transform.position, direction);
            Debug.DrawRay(ray.origin, ray.direction * range, Color.red, 10.0f);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, range) == true) // もしRayを投射して何らかのコライダーに衝突したら
            {
                hitName = hit.collider.gameObject.name; // 衝突した相手オブジェクトの名前を取得

                if (hit.collider.gameObject.CompareTag("Enemy") || hit.collider.gameObject.CompareTag("FlyingEnemy") || hit.collider.gameObject.CompareTag("GroundEnemy"))//※間違ってオブジェクトの設定にレイヤーとタグを間違えるなよおれｗ
                {
                    //ダメージ
                    Target target = hit.transform.GetComponent<Target>();
                    if (target != null)
                    {
                        target.TakeDamage(Damage);
                    }

                    //着弾した物体を後ろに押す
                    if (hit.rigidbody != null)
                    {
                        hit.rigidbody.AddForce(-hit.normal * impactForce);
                    }
                }

                ImpactEffect(hit);
            }
        }
    }

    /// <summary>
    /// マズルフラッシュのエフェクトと薬莢を出す（アセットストアで買ったコードをそのままもってきている）
    /// </summary>
    void ShotGunMuzzleFlashAndShell()
    {
        if (shotGunShotEmitters != null)
        {
            foreach (var effect in shotGunShotEmitters)
            {
                effect.Emit(1);
            }
        }
    }

    /// <summary>
    /// 硝煙のエフェクトを出す（アセットストアで買ったコードをそのままもってきている）
    /// </summary>
    void ShotGunSmoke()
    {
        if (shotGunAfterFireSmoke != null)
        {
            shotGunAfterFireSmoke.Play();
        }
    }

    /// <summary>
    /// 着弾エフェクト
    /// </summary> 
    void ImpactEffect(RaycastHit hit)
    {
        if (hit.collider.gameObject.CompareTag("Enemy") || hit.collider.gameObject.CompareTag("FlyingEnemy") || hit.collider.gameObject.CompareTag("GroundEnemy"))//※間違ってオブジェクトの設定にレイヤーとタグを間違えるなよおれｗ
        {
            GameObject impactGameObject = Instantiate(bloodImpactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(impactGameObject, 2.0f);
        }
        else
        {
            GameObject impactGameObject = Instantiate(rockImpactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(impactGameObject, 2.0f);
        }
    }

    void FixedUpdate()
    {
        if (isActiveCamera == false)
        {
            return;
        }

        //SRT
        if (Player.SingletonInstance.IsAim == false)
        {
            CameraNormalMove();
        }
        else if (Player.SingletonInstance.IsAim == true)
        {
            CameraAimMove();
        }

        CameraRot();
    }

    /// <summary>
    /// 通常カメラ
    /// </summary>
    void CameraNormalMove()
    {
        //通常カメラ位置をプレイヤーの座標位置から計算
        Vector3 cameraPos = Player.SingletonInstance.transform.position + (Vector3.up * normalUpPos) + (this.transform.forward * normalForwardPos);
        //カメラの位置を移動させる
        this.transform.position = Vector3.Lerp(transform.position, cameraPos, Player.SingletonInstance.NormalMoveSpeed * 10 * Time.deltaTime);
    }

    /// <summary>
    /// 肩越しカメラ
    /// </summary>
    void CameraAimMove()
    {
        //肩越しカメラ位置をプレイヤーの座標位置から計算
        Vector3 cameraPos = Player.SingletonInstance.transform.position + (Player.SingletonInstance.transform.right * aimRightPos) + (Vector3.up * aimUpPos) + (this.transform.forward * aimForwardPos);
        //カメラの位置を移動させる
        this.transform.position = Vector3.Lerp(transform.localPosition, cameraPos, Player.SingletonInstance.WeaponMoveSpeed * 10 * Time.deltaTime);
    }

    /// <summary>
    /// カメラの回転
    /// </summary> 
    void CameraRot()
    {
        // マウスの移動量を取得
        float x_Rotation = Input.GetAxis("Mouse X");
        float y_Rotation = Input.GetAxis("Mouse Y");

        if (Player.SingletonInstance.IsAim == true)
        {
            localCameraSpeedX = aimCameraSpeedX;
            localCameraSpeedY = aimCameraSpeedY;
            isTargethit = false;

            //ターゲットにあたった際にカメラを遅くする処理
            Ray ray = new Ray(this.transform.position, this.transform.forward);
            Debug.DrawRay(ray.origin, ray.direction * range, Color.gray, 1.0f);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, range) == true) // もしRayを投射して何らかのコライダーに衝突したら
            {
                if (hit.collider.gameObject.CompareTag("Enemy") || hit.collider.gameObject.CompareTag("FlyingEnemy") || hit.collider.gameObject.CompareTag("GroundEnemy"))//※間違ってオブジェクトの設定にレイヤーとタグを間違えるなよおれｗ
                {
                    //カメラの速さを遅くする
                    localCameraSpeedX = aimCameraSpeedX / slowDownCameraSpeed;
                    localCameraSpeedY = aimCameraSpeedY / slowDownCameraSpeed;
                    isTargethit = true;
                }
            }
        }
        else if (Player.SingletonInstance.IsAim == false)
        {
            localCameraSpeedX = normalCameraSpeedX;
            localCameraSpeedY = normalCameraSpeedY;
            isTargethit = false;
        }

        //Mathf.Absは絶対値を返す(例)Mathf.Abs(10)なら10、Mathf.Abs(-10)なら10と+だろうが-だろうがプラスの値を返す
        //RotateAround(中心となるワールド座標, 回転軸, 回転角度(度数))関数は、指定の座標を中心にオブジェクトを回転させる関数

        // X方向に一定量移動していれば横回転
        if (deadZoneX < Mathf.Abs(x_Rotation))
        {
            // 回転軸はワールド座標のY軸
            this.transform.RotateAround(Player.SingletonInstance.transform.position, Vector3.up, x_Rotation * Time.deltaTime * localCameraSpeedX);
        }

        // Y方向に一定量移動していれば縦回転
        if (deadZoneY < Mathf.Abs(y_Rotation))
        {
            float cameraAngles = this.transform.localEulerAngles.x;
            const float lookingUpLimit = 360.0f;
            const float lookingDownLimit = 40.0f;
            if (324 < cameraAngles && cameraAngles < lookingUpLimit || -10 < cameraAngles && cameraAngles < lookingDownLimit)//ここの各左の数字を変えればカメラの上下の止まる限界値が変わる
            {
                // 回転軸はカメラ自身のX軸
                this.transform.RotateAround(Player.SingletonInstance.transform.position, -transform.right, y_Rotation * Time.deltaTime * localCameraSpeedY);
            }
            else
            {
                if (300 < cameraAngles)
                {
                    if (Input.GetAxis("Mouse Y") < 0)
                    {
                        //マウスYの入力量 × カメラのスピード × 時間 = の値をY回転の量にする
                        this.transform.RotateAround(Player.SingletonInstance.transform.position, -transform.right, y_Rotation * Time.deltaTime * localCameraSpeedY);
                    }
                }
                else
                {
                    if (0 < Input.GetAxis("Mouse Y"))
                    {
                        //マウスYの入力量 × カメラのスピード × 時間 = の値をY回転の量にする
                        this.transform.RotateAround(Player.SingletonInstance.transform.position, -transform.right, y_Rotation * Time.deltaTime * localCameraSpeedY);
                    }

                }
            }
        }
    }

    void OnGUI()
    {
        GUIStyle styleGreen = new GUIStyle();
        styleGreen.fontSize = 30;
        GUIStyleState styleStateGreen = new GUIStyleState();
        styleStateGreen.textColor = Color.green;
        styleGreen.normal = styleStateGreen;

        GUIStyle styleRed = new GUIStyle();
        styleRed.fontSize = 30;
        GUIStyleState styleStateRed = new GUIStyleState();
        styleStateRed.textColor = Color.red;
        styleRed.normal = styleStateRed;

        GUIStyle styleBlack = new GUIStyle();
        styleBlack.fontSize = 30;
        GUIStyleState styleStateBlack = new GUIStyleState();
        styleStateBlack.textColor = Color.black;
        styleBlack.normal = styleStateBlack;

        GUIStyle styleYellow = new GUIStyle();
        styleYellow.fontSize = 30;
        GUIStyleState styleStateYellow = new GUIStyleState();
        styleStateYellow.textColor = Color.yellow;
        styleYellow.normal = styleStateYellow;

        GUI.Box(new Rect(10, 400, 100, 50), "cameraPos", styleRed);
        GUI.Box(new Rect(350, 400, 100, 50), this.transform.position.ToString(), styleRed);
        GUI.Box(new Rect(10, 450, 100, 50), "hitName", styleRed);
        GUI.Box(new Rect(350, 450, 100, 50), hitName, styleRed);
        GUI.Box(new Rect(10, 550, 100, 50), "localCameraSpeedX", styleRed);
        GUI.Box(new Rect(350, 550, 100, 50), localCameraSpeedX.ToString(), styleRed);
        GUI.Box(new Rect(10, 600, 100, 50), "localCameraSpeedY", styleRed);
        GUI.Box(new Rect(350, 600, 100, 50), localCameraSpeedY.ToString(), styleRed);
        GUI.Box(new Rect(10, 650, 100, 50), "gunTYPE", styleRed);
        GUI.Box(new Rect(350, 650, 100, 50), gunTYPE.ToString(), styleRed);
    }
}
