using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Knife.Effects;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] GameObject player;

    [Tooltip("X軸のカメラの回転スピード")]
    [Range(50, 150)] public float cameraSpeedX = 100;
    [Tooltip("Y軸のカメラの回転スピード")]
    [Range(50, 150)] public float cameraSpeedY = 50;

    [Tooltip("X軸のカメラの入力デッドゾーン")]
    [Range(0.001f, 0.1f)] public float deadZoneX = 0.1f;
    [Tooltip("Y軸のカメラの入力デッドゾーン")]
    [Range(0.001f, 0.1f)] public float deadZoneY = 0.1f;

    string hitName = "";
    [Tooltip("レイの長さ")]
    [SerializeField] float range = 100.0f;
    [Tooltip("銃のダメージ")]
    [SerializeField] float Damage = 10.0f;

    [Tooltip("マズルフラッシュ")]
    [SerializeField] ParticleGroupEmitter[] shotEmitters;
    [Tooltip("硝煙")]
    [SerializeField] ParticleGroupPlayer afterFireSmoke;

#if UNITY_EDITOR
    bool isActiveDebug = false;//エディターで実行ロード時にマウスの座標が入力されてカメラが動いてしまう問題の対処用
#endif

    IEnumerator Start()
    {
#if UNITY_EDITOR
        yield return new WaitForSeconds(1.0f);
        isActiveDebug = true;
#endif

        yield return null;
    }

    void Update()
    {
        if (player.GetComponent<Player>().IsAim == false)
        {

        }
        else
        {
            Shoot();
        }
    }

    void FixedUpdate()
    {
#if UNITY_EDITOR
        if (isActiveDebug == false)
        {
            return;
        }
#endif

        //SRT
        if (player.GetComponent<Player>().IsAim == false)
        {
            CameraNormalMove();
        }
        else
        {
            CameraWeaponMove();
        }

        CameraRot();
    }

    void Shoot()
    {
        if (Input.GetMouseButtonDown(0))
        {
            MuzzleFlash();
            Smoke();

            Ray ray = new Ray(this.transform.position, this.transform.forward);
            Debug.DrawRay(ray.origin, ray.direction * 20.0f, Color.red, 10.0f);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, range) == true) // もしRayを投射して何らかのコライダーに衝突したら
            {
                hitName = hit.collider.gameObject.name; // 衝突した相手オブジェクトの名前を取得

                Target target = hit.transform.GetComponent<Target>();
                if (target != null)
                {
                    target.TakeDamage(Damage);
                }
            }
        }
    }

    void MuzzleFlash()
    {
        if (shotEmitters != null)
        {
            foreach (var e in shotEmitters)
                e.Emit(1);
        }
    }

    void Smoke()
    {
        if (afterFireSmoke != null)
            afterFireSmoke.Play();
    }

    void CameraNormalMove()
    {
        //通常のカメラ位置をプレイヤーの座標位置から計算
        Vector3 cameraPos = player.transform.position + (Vector3.up * 2) + (this.transform.forward * -5);
        //カメラの位置を移動させる
        this.transform.position = Vector3.Lerp(transform.position, cameraPos, player.GetComponent<Player>().NormalMoveSpeed * 10 * Time.deltaTime);
    }

    void CameraWeaponMove()
    {
        //通常のカメラ位置をプレイヤーの座標位置から計算
        Vector3 cameraPos = player.transform.position + (player.transform.right * 0.5f) + (Vector3.up * 1.6f) + (this.transform.forward * -0.5f);
        //カメラの位置を移動させる
        this.transform.position = Vector3.Lerp(transform.localPosition, cameraPos, player.GetComponent<Player>().WeaponMoveSpeed * 10 * Time.deltaTime);
    }

    void CameraRot()
    {
        // マウスの移動量を取得
        float x_Rotation = Input.GetAxis("Mouse X");
        float y_Rotation = Input.GetAxis("Mouse Y");

        //Mathf.Absは絶対値を返す(例)Mathf.Abs(10)なら10、Mathf.Abs(-10)なら10と+だろうが-だろうがプラスの値を返す
        //RotateAround(中心となるワールド座標, 回転軸, 回転角度(度数))関数は、指定の座標を中心にオブジェクトを回転させる関数

        // X方向に一定量移動していれば横回転
        if (deadZoneX < Mathf.Abs(x_Rotation))
        {
            // 回転軸はワールド座標のY軸
            this.transform.RotateAround(player.transform.position, Vector3.up, x_Rotation * Time.deltaTime * cameraSpeedX);
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
                this.transform.RotateAround(player.transform.position, -transform.right, y_Rotation * Time.deltaTime * cameraSpeedY);
            }
            else
            {
                if (300 < cameraAngles)
                {
                    if (Input.GetAxis("Mouse Y") < 0)
                    {
                        //マウスYの入力量 × カメラのスピード × 時間 = の値をY回転の量にする
                        this.transform.RotateAround(player.transform.position, -transform.right, y_Rotation * Time.deltaTime * cameraSpeedY);

                    }
                }
                else
                {
                    if (0 < Input.GetAxis("Mouse Y"))
                    {
                        //マウスYの入力量 × カメラのスピード × 時間 = の値をY回転の量にする
                        this.transform.RotateAround(player.transform.position, -transform.right, y_Rotation * Time.deltaTime * cameraSpeedY);

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
    }
}
