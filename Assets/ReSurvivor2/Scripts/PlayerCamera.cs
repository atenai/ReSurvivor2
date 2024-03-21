using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Knife.Effects;

public class PlayerCamera : MonoBehaviour
{
    //シングルトンで作成（ゲーム中に１つのみにする）
    public static PlayerCamera singletonInstance = null;

    [Header("カメラ")]
    [Tooltip("エディターで実行ロード時にマウスの座標が入力されてカメラが動いてしまう問題の対処用")]
    bool isActiveCamera = false;
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

    [Header("アサルトライフル")]
    [Tooltip("アサルトライフルを何秒間隔で撃つか")]
    [SerializeField] float assaultRifleFireRate = 0.1f;
    [Tooltip("アサルトライフルの射撃間隔の時間カウント用のタイマー")]
    float assaultRifleCountTimer = 0.0f;
    [Tooltip("アサルトライフルの散乱角度")]
    [SerializeField] float assaultRifleRandomAngle = 1.0f;

    //↓アセットストアのプログラム↓//
    [Tooltip("アサルトライフルのマズルフラッシュと薬莢")]
    [SerializeField] ParticleGroupEmitter[] assaultRifleShotEmitters;
    [Tooltip("アサルトライフルの硝煙")]
    [SerializeField] ParticleGroupPlayer assaultRifleAfterFireSmoke;
    [Tooltip("アサルトライフルの着弾エフェクト")]
    [SerializeField] GameObject assaultRifleImpactEffect;
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

    enum GunTYPE
    {
        HandGun = 1,
        AssaultRifle = 2,
        ShotGun = 3,
    }
    [SerializeField] GunTYPE gunTYPE = GunTYPE.AssaultRifle;

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
        //起動時のロードの際にマウスの入力でカメラが思わぬ方向に動いてしまうので、1秒間カメラ操作を受け付けなくする（ここの処理をごまかす為、ゲーム開始時にフェードイン・フェードアウトを行う）
        yield return new WaitForSeconds(1.0f);
        isActiveCamera = true;

        yield return null;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("ハンドガン");
            gunTYPE = GunTYPE.HandGun;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("アサルトライフル");
            gunTYPE = GunTYPE.AssaultRifle;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Debug.Log("ショットガン");
            gunTYPE = GunTYPE.ShotGun;
        }

        switch (gunTYPE)
        {
            case GunTYPE.HandGun:
                HandGunShoot();
                break;
            case GunTYPE.AssaultRifle:
                AssaultRifleShoot();
                break;
            case GunTYPE.ShotGun:
                ShotGunShoot();
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
                    HandGunFire();
                    handGunCountTimer = handGunFireRate;//カウントタイマーに射撃を待つ時間を入れる
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
    /// 弾を発射
    /// </summary> 
    void HandGunFire()
    {
        AssaultRifleMuzzleFlash();
        AssaultRifleSmoke();

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

            AssaultRifleImpactEffect(hit);
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
                    AssaultRifleFire();
                    assaultRifleCountTimer = assaultRifleFireRate;//カウントタイマーに射撃を待つ時間を入れる
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
    /// 弾を発射
    /// </summary> 
    void AssaultRifleFire()
    {
        AssaultRifleMuzzleFlash();
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

            AssaultRifleImpactEffect(hit);
        }
    }

    /// <summary>
    /// マズルフラッシュのエフェクトを出す（アセットストアで買ったコードをそのままもってきている）
    /// </summary>
    void AssaultRifleMuzzleFlash()
    {
        if (assaultRifleShotEmitters != null)
        {
            foreach (var e in assaultRifleShotEmitters)
            {
                e.Emit(1);
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
    /// 着弾エフェクト
    /// </summary> 
    void AssaultRifleImpactEffect(RaycastHit _hit)
    {
        GameObject impactGameObject = Instantiate(assaultRifleImpactEffect, _hit.point, Quaternion.LookRotation(_hit.normal));
        Destroy(impactGameObject, 2.0f);
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
                    ShotGunFire();
                    shotGunCountTimer = shotGunFireRate;//カウントタイマーに射撃を待つ時間を入れる
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
    /// 弾を発射
    /// </summary> 
    void ShotGunFire()
    {
        AssaultRifleMuzzleFlash();
        AssaultRifleSmoke();

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

                AssaultRifleImpactEffect(hit);
            }
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
        //通常のカメラ位置をプレイヤーの座標位置から計算
        Vector3 cameraPos = Player.SingletonInstance.transform.position + (Vector3.up * 2) + (this.transform.forward * -5);
        //カメラの位置を移動させる
        this.transform.position = Vector3.Lerp(transform.position, cameraPos, Player.SingletonInstance.NormalMoveSpeed * 10 * Time.deltaTime);
    }

    /// <summary>
    /// 肩越しカメラ
    /// </summary>
    void CameraAimMove()
    {
        //通常のカメラ位置をプレイヤーの座標位置から計算
        const float aimRightPos = 0.6f;
        const float aimUpPos = 1.6f;
        const float aimForwardPos = -0.6f;
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
            const float lookingDownLimit = 79.0f;
            const float lookingUpLimit = 360.0f;
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
    }
}
