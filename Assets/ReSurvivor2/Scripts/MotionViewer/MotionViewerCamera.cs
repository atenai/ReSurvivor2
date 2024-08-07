using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// モーションビュアのカメラ制御
/// </summary>
public class MotionViewerCamera : MonoBehaviour
{
    #region 非公開変数

    [Tooltip("モーションビュアシーンスクリプト")]
    [SerializeField] MotionViewerManager _motionViewerManager;
    [Tooltip("カメラの回転速度")]
    [SerializeField] float _cameraRotationSpeed = 5.0f;
    [Tooltip("カメラの見上げる角度の上限")]
    [SerializeField] float _lookingUpLimit = 360.0f;
    [Tooltip("カメラの見下げる角度の上限")]
    [SerializeField] float _lookingDownLimit = 79.0f;
    float _zoom;

    #endregion

    #region Monobehaviour Override関数

    void Update()
    {
        if (_motionViewerManager.CurrentActiveModel == null)
        {
            return;
        }

        if (Input.GetMouseButton(1))
        {
            CameraRotation();
        }

        CameraZoom();
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 30;
        GUIStyleState styleState = new GUIStyleState();
        styleState.textColor = Color.green;
        style.normal = styleState;

        // GUI.Box(new Rect(10, 0, 100, 50), "Input.GetAxis(Mouse Y)", style);
        // GUI.Box(new Rect(500, 0, 100, 50), Input.GetAxis("Mouse Y").ToString(), style);
        // GUI.Box(new Rect(10, 50, 100, 50), "this.transform.localEulerAngles.x", style);
        // GUI.Box(new Rect(500, 50, 100, 50), this.transform.localEulerAngles.x.ToString(), style);
    }

    #endregion

    #region 公開関数

    public void OnClickCameraReset()
    {
        this.transform.position = new Vector3(0.0f, 1.0f, -5.0f);
        this.transform.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
    }

    #endregion

    #region 非公開関数

    /// <summary>
    /// カメラの回転
    /// </summary>
    void CameraRotation()
    {
        //マウスの移動量を取得
        float x = Input.GetAxis("Mouse X");
        float y = Input.GetAxis("Mouse Y");

        //X方向に一定量移動していれば横回転
        if (0.1f < Mathf.Abs(x))
        {
            this.transform.RotateAround(_motionViewerManager.CurrentActiveModel.transform.position, Vector3.up, x * _cameraRotationSpeed);
        }

        //Y方向に一定量移動していれば縦回転
        if (0.1f < Mathf.Abs(y))
        {
            float cameraAngles = this.transform.localEulerAngles.x;
            //(モデルを見上げる角度の上限 || モデルを見下げる角度の上限)
            if (324 < cameraAngles && cameraAngles < _lookingUpLimit || -10 < cameraAngles && cameraAngles < _lookingDownLimit)
            {
                this.transform.RotateAround(_motionViewerManager.CurrentActiveModel.transform.position, -this.transform.right, y * _cameraRotationSpeed);
            }
            else
            {
                if (300 < cameraAngles)
                {
                    //マウスの縦操作の入力値が0よりも小さい場合、中身を実行する(つまりマウスが↓に移動操作させた時のみ回転するようにする)
                    if (Input.GetAxis("Mouse Y") < 0)
                    {
                        Debug.Log("<color=red>モデルを見上げる角度の限界</color>");
                        this.transform.RotateAround(_motionViewerManager.CurrentActiveModel.transform.position, -this.transform.right, y * _cameraRotationSpeed);
                    }
                }
                else
                {
                    //マウスの縦操作の入力値が0よりも大きい場合、中身を実行する(つまりマウスが↑に移動操作させた時のみ回転するようにする)
                    if (0 < Input.GetAxis("Mouse Y"))
                    {
                        Debug.Log("<color=blue>モデルを見下げる角度の限界</color>");
                        this.transform.RotateAround(_motionViewerManager.CurrentActiveModel.transform.position, -this.transform.right, y * _cameraRotationSpeed);
                    }
                }
            }
        }
    }

    /// <summary>
    /// カメラの拡大縮小
    /// </summary>
    void CameraZoom()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        // ゲームウィンドウがフォーカスされている場合のみズーム処理を行う
        if (Application.isFocused == true)
        {
            _zoom = Input.GetAxis("Mouse ScrollWheel");
            Vector3 offset = new Vector3(0, 0, 0);
            Vector3 pos = _motionViewerManager.CurrentActiveModel.transform.position - this.transform.position;

            if (0 < _zoom)
            {
                offset = pos.normalized * 1;
            }
            else if (_zoom < 0)
            {
                offset = -pos.normalized * 1;

            }

            transform.position = transform.position + offset;
        }
    }

    #endregion
}