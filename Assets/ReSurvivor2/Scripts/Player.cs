using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    //変数名は「小文字」から始める（test）
    //関数名は「大文字」から始める（Test）
    //引数名は「_小文字」から始める（_test）
    //アニメーション変数名は「型の最初の文字の_小文字から始める」（f_test）

    //シングルトンで作成（ゲーム中に１つのみにする）
    static Player singletonInstance = null;
    public static Player SingletonInstance => singletonInstance;

    [SerializeField] Animator animator;
    [SerializeField] Rigidbody rb;

    [Header("キャラクターの移動")]
    [SerializeField] float normalMoveSpeed = 1.0f;
    public float NormalMoveSpeed => normalMoveSpeed;
    [SerializeField] float weaponMoveSpeed = 1.0f;
    public float WeaponMoveSpeed => weaponMoveSpeed;
    float inputHorizontal;
    float inputVertical;
    Vector3 moveForward;
    Vector3 cameraForward;
    bool isAim = false;
    public bool IsAim => isAim;

    [Header("キャラクターモデル")]
    [Tooltip("キャラクターの脊椎ボーン")]
    [SerializeField] Transform spine_03;
    [Tooltip("キャラクターの脊椎ボーンの初期値")]
    float spine_03_InitEulerAnglesX;
    bool isAnimationRotInit = false;
    [Tooltip("キャラクターの右肩ボーン")]
    [SerializeField] Transform upperarm_r;

    //HP
    float currentHp = 100.0f;
    [SerializeField] float maxHp = 100.0f;
    [SerializeField] Canvas canvasPlayer;
    [SerializeField] Slider sliderHp;

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

    void Start()
    {
        InitBoneSpine03();
        InitHP();
    }

    /// <summary>
    /// キャラクターの脊椎ボーンの初期化処理
    /// </summary> 
    void InitBoneSpine03()
    {
        //キャラクターの脊椎ボーンの初期値を取得する（真正面に戻す際に必要なため）
        spine_03_InitEulerAnglesX = spine_03.eulerAngles.x;
    }

    void InitHP()
    {
        sliderHp.value = 1;
        currentHp = maxHp;
    }

    void Update()
    {
        inputHorizontal = Input.GetAxisRaw("Horizontal");
        inputVertical = Input.GetAxisRaw("Vertical");

        //右ボタンまたはレフトシフトが押されていたら中身を実行
        if (Input.GetMouseButton(1) || Input.GetKey(KeyCode.LeftShift))
        {
            isAim = true;
        }
        else
        {
            isAim = false;
        }

        NormalMoveAnimation();

        if (isAim == false)
        {
            //常にキャンバスをメインカメラの方を向かせる
            canvasPlayer.transform.rotation = Camera.main.transform.rotation;
        }
        else if (isAim == true)
        {
            //常にキャンバスをメインカメラの方を向かせる
            canvasPlayer.transform.rotation = Camera.main.transform.rotation;
        }
    }

    void NormalMoveAnimation()
    {
        animator.SetFloat("f_moveSpeedX", inputHorizontal);
        animator.SetFloat("f_moveSpeedY", inputVertical);
        animator.SetBool("b_isAim", isAim);
    }

    void LateUpdate()
    {
        //ボーンを曲げる際は必ずLateUpdateに書く必要がある！（これいつかメモする！）
        RotateBoneSpine03();
        RotateBoneUpperArmR();
    }

    /// <summary>
    /// キャラクターの脊椎ボーンを曲げる
    /// </summary> 
    void RotateBoneSpine03()
    {
        if (isAim == true)
        {
            const float aimAnimationRotX = 12.5f;
            //腰のボーンの角度をカメラの向きにする
            spine_03.rotation = Quaternion.Euler(PlayerCamera.singletonInstance.transform.localEulerAngles.x + aimAnimationRotX, spine_03.eulerAngles.y, spine_03.eulerAngles.z);
            isAnimationRotInit = true;
        }
        else if (isAim == false)
        {
            if (isAnimationRotInit == true)
            {
                //腰のボーンの角度を真正面（初期値）にする
                spine_03.rotation = Quaternion.Euler(spine_03_InitEulerAnglesX, spine_03.eulerAngles.y, spine_03.eulerAngles.z);
                isAnimationRotInit = false;
            }

        }
    }

    /// <summary>
    /// キャラクターの右肩ボーンを曲げる
    /// </summary> 
    void RotateBoneUpperArmR()
    {
        if (isAim == true)
        {
            //エイムアニメーションの銃の位置をカメラの中心に合わせる為の数値（アニメーション問題が解消されたらこの処理は消す！）
            const float aimAnimationRotX = 12.5f;
            const float aimAnimationRotY = 12.5f;
            //右肩のボーンの角度をカメラの向きにする
            upperarm_r.rotation = Quaternion.Euler(PlayerCamera.singletonInstance.transform.localEulerAngles.x + aimAnimationRotX, upperarm_r.eulerAngles.y + aimAnimationRotY, upperarm_r.eulerAngles.z);
        }
        else if (isAim == false)
        {
            //右肩のボーンの角度を真正面（初期値）にする
            upperarm_r.rotation = Quaternion.Euler(upperarm_r.eulerAngles.x, upperarm_r.eulerAngles.y, upperarm_r.eulerAngles.z);
        }
    }

    void FixedUpdate()
    {
        NormalMove();
        AimMove();
    }

    /// プレイヤーの移動はフルスクラッチする、何故ならTPSで通常カメラと肩越しカメラとで移動やアニメーションを切り替える必要がある為
    /// キャラクターの移動はRigidbody.velocityで行う、参考作品としてランアウェイシュートを参考にする

    /// <summary>
    /// 通常状態移動
    /// </summary>
    void NormalMove()
    {
        if (isAim == true)
        {
            return;
        }

        //カメラの方向から、XとZのベクトルを取得 0,0,1 * 1,0,1 = 1,0,1;
        cameraForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;

        //方向キーの入力値とカメラの向きから、移動方向のベクトルを算出する
        moveForward = cameraForward * inputVertical + Camera.main.transform.right * inputHorizontal;
        //移動方向のベクトルを正規化
        moveForward.Normalize();

        //移動方向にスピードを掛ける。ジャンプや落下がある場合は、別途Y軸方向の速度ベクトルを足す。
        rb.velocity = moveForward * normalMoveSpeed + new Vector3(0, rb.velocity.y, 0);

        //キャラクターの向きをキャラクターの進行方向にする
        if (moveForward != Vector3.zero)//向きベクトルがある場合は中身を実行する
        {
            this.transform.rotation = Quaternion.LookRotation(moveForward);
        }

#if UNITY_EDITOR
        VectorVisualizer();
#endif
    }

    /// <summary>
    /// 構えた状態移動
    /// </summary>
    void AimMove()
    {
        if (isAim == false)
        {
            return;
        }

        //カメラの方向から、XとZのベクトルを取得 0,0,1 * 1,0,1 = 1,0,1;
        cameraForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;

        //方向キーの入力値とカメラの向きから、移動方向のベクトルを算出する
        moveForward = cameraForward * inputVertical + Camera.main.transform.right * inputHorizontal;
        //移動方向のベクトルを正規化
        moveForward.Normalize();

        //移動方向にスピードを掛ける。ジャンプや落下がある場合は、別途Y軸方向の速度ベクトルを足す。
        rb.velocity = moveForward * weaponMoveSpeed + new Vector3(0, rb.velocity.y, 0);

        //キャラクターの向きをカメラの前方にする
        if (cameraForward != Vector3.zero)//向きベクトルがある場合は中身を実行する
        {
            this.transform.rotation = Quaternion.LookRotation(cameraForward);
        }
    }

    /// <summary>
    /// ベクトルの可視化
    /// </summary>
    void VectorVisualizer()
    {
        Ray debugRayVelocity = new Ray(this.transform.position, rb.velocity);
        Debug.DrawRay(debugRayVelocity.origin, debugRayVelocity.direction, Color.magenta);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "Enemy" || collision.collider.tag == "FlyingEnemy" || collision.collider.tag == "GroundEnemy")
        {
            TakeDamage(10.0f);
        }
    }

    /// <summary>
    /// ダメージ処理
    /// </summary>
    public void TakeDamage(float amount)
    {
        currentHp = currentHp - amount;
        //Debug.Log("<color=orange>currentHp : " + currentHp + "</color>");
        sliderHp.value = (float)currentHp / (float)maxHp;
        if (currentHp <= 0.0f)
        {
            //Destroy(this.gameObject);
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

        GUI.Box(new Rect(10, 0, 100, 50), "inputHorizontal", styleGreen);
        GUI.Box(new Rect(350, 0, 100, 50), inputHorizontal.ToString(), styleGreen);
        GUI.Box(new Rect(10, 50, 100, 50), "inputVertical", styleGreen);
        GUI.Box(new Rect(350, 50, 100, 50), inputVertical.ToString(), styleGreen);
        GUI.Box(new Rect(10, 100, 100, 50), "normalMoveSpeed", styleGreen);
        GUI.Box(new Rect(350, 100, 100, 50), normalMoveSpeed.ToString(), styleGreen);
        GUI.Box(new Rect(10, 150, 100, 50), "weaponMoveSpeed", styleGreen);
        GUI.Box(new Rect(350, 150, 100, 50), weaponMoveSpeed.ToString(), styleGreen);
        GUI.Box(new Rect(10, 200, 100, 50), "rb.velocity", styleRed);
        GUI.Box(new Rect(350, 200, 100, 50), rb.velocity.ToString(), styleRed);
        GUI.Box(new Rect(10, 250, 100, 50), "moveForward", styleRed);
        GUI.Box(new Rect(350, 250, 100, 50), moveForward.ToString(), styleRed);
        GUI.Box(new Rect(10, 300, 100, 50), "cameraForward", styleRed);
        GUI.Box(new Rect(350, 300, 100, 50), cameraForward.ToString(), styleRed);
        GUI.Box(new Rect(10, 350, 100, 50), "isAim", styleBlack);
        GUI.Box(new Rect(350, 350, 100, 50), isAim.ToString(), styleBlack);
        GUI.Box(new Rect(10, 500, 100, 50), "spine_03.eulerAngles", styleBlack);
        GUI.Box(new Rect(350, 500, 100, 50), spine_03.eulerAngles.ToString(), styleBlack);
    }
}
