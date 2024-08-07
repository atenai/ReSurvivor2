using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MotionViewerRectangleRenderCreateCube : MonoBehaviour
{
    [Tooltip("モーションビュアシーンスクリプト")]
    [SerializeField] MotionViewerManager _motionViewerManager;
    Vector3 _startMousePos = Vector3.zero;
    Vector3 _currentMousePos = Vector3.zero;
    Rect _rect = new Rect(0, 0, 0, 0);
    Texture2D _texture;//白い枠線を表示する為のテクスチャ
    [SerializeField] Material transparentRedMaterial;// マテリアルの参照を追加

    void Start()
    {
        _texture = new Texture2D(1, 1);//テクスチャをインスタンス
        _texture.SetPixel(0, 0, new Color(1, 1, 1, 0.2f));//色を白の透明に設定（色情報バッファをテクスチャに反映らしい）
        _texture.Apply();//SetPixels関数による変更を実際に適用する
    }

    void Update()
    {
        if (_motionViewerManager.IsDragging == false)
        {
            return;
        }

        if (EventSystem.current.IsPointerOverGameObject()) return;

        if (Input.GetMouseButtonDown(0))//マウスボタンクリックした時
        {
            SetStartMousePos(Input.mousePosition);
        }

        if (Input.GetMouseButton(0))//マウスボタンクリックしている間
        {
            SetCurrentMousePos(Input.mousePosition);
        }

        if (Input.GetMouseButtonUp(0))//マウスボタンクリックが離れた時
        {
            CreateCubeFromRect();
        }
    }

    void OnGUI()
    {
        if (_motionViewerManager.IsDragging == false)
        {
            return;
        }

        if (EventSystem.current.IsPointerOverGameObject()) return;

        if (Input.GetMouseButton(0))//マウスボタンクリックしている間
        {
            DrawRectangle();
        }
    }

    /// <summary>
    /// 最初にマウスボタンをクリックした際の座標を取得し、変数に格納
    /// </summary>
    void SetStartMousePos(Vector3 mousePos)
    {
        _startMousePos = mousePos;
    }

    /// <summary>
    /// マウスボタンが押されている際の座標を取得し、変数に格納
    /// </summary>
    void SetCurrentMousePos(Vector3 mousePos)
    {
        _currentMousePos = mousePos;
    }

    /// <summary>
    /// 白い枠線を描画する関数
    /// </summary>
    void DrawRectangle()
    {
        float width = _currentMousePos.x - _startMousePos.x;//最初にマウスボタンを押した座標と現在のマウスボタンの押されている座標から横の長さを計算
        float height = _currentMousePos.y - _startMousePos.y;//最初にマウスボタンを押した座標と現在のマウスボタンの押されている座標から縦の長さを計算

        Vector3 start = _startMousePos;//最初にマウスボタンを押した座標をstart変数に格納

        if (width < 0)//もし横の長さが-なら中身を実行する
        {
            width = Mathf.Abs(width);//横の長さを絶対値にして再度格納（ようは+にする）
            start.x = _currentMousePos.x;//マウスボタンが押されている際のx座標を最初にマウスボタンが押された座標に格納（ようは描画するスタート位置を現在のマウスボタン座標の位置から始めるようにする）
        }

        if (height < 0)//もし縦の長さが-なら中身を実行する
        {
            height = Mathf.Abs(height);//縦の長さを絶対値にして再度格納（ようは+にする）
            start.y = _currentMousePos.y;//マウスボタンが押されている際のy座標を最初にマウスボタンが押された座標に格納（ようは描画するスタート位置を現在のマウスボタン座標の位置から始めるようにする）
        }

        //テクスチャの描画位置と大きさ設定
        _rect.x = start.x;//x座標の描画初めの位置
        _rect.y = Screen.height - start.y - height;//Y軸は逆方向なので調整//y座標の描画初めの位置
        _rect.width = width;//横の長さ
        _rect.height = height;//縦の長さ

        GUI.color = Color.white;//色
        GUI.DrawTexture(_rect, _texture);//テクスチャを描画
    }

    /// <summary>
    /// キューブを作成する関数
    /// </summary>
    void CreateCubeFromRect()
    {
        _motionViewerManager.DestroyAttackCollision();

        float cameraPosZ = Mathf.Abs(Camera.main.transform.position.z);//z座標0に生成したいからカメラのz位置を取得する

        Vector3 worldStart = Camera.main.ScreenToWorldPoint(new Vector3(_rect.x, Screen.height - _rect.y - _rect.height, cameraPosZ));//始まりの位置をカメラ座標からワールド座標に変換
        Vector3 worldEnd = Camera.main.ScreenToWorldPoint(new Vector3(_rect.x + _rect.width, Screen.height - _rect.y, cameraPosZ));//終わりの位置をカメラ座標からワールド座標に変換

        Vector3 cubePosition = (worldStart + worldEnd) / 2;//キューブの座標の中心点を計算
        Vector3 cubeScale = worldEnd - worldStart;//キューブの大きさを計算

        _motionViewerManager.AttackCollision = GameObject.CreatePrimitive(PrimitiveType.Cube);//キューブを作成
        _motionViewerManager.AttackCollision.transform.position = cubePosition;//作成したキューブに上で計算した座標を入れる
        _motionViewerManager.AttackCollision.transform.localScale = new Vector3(cubeScale.x, cubeScale.y, 2);//作成したキューブに上で計算した大きさを入れる
        _motionViewerManager.AttackCollision.transform.SetParent(_motionViewerManager.CurrentActiveModel.transform);

        //マテリアルを設定
        Renderer cubeRenderer = _motionViewerManager.AttackCollision.GetComponent<Renderer>();
        cubeRenderer.material = transparentRedMaterial;
    }
}