using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

[Serializable]
public class DebugEnemy
{
    [Header("デバッグ用変数")]

    #region 非公開変数
    [Tooltip("デバッグ用処理をONにするか？を判別する変数")]
    [SerializeField] bool _isDebugMode = false;

    List<GameObject> _debugTextList = new List<GameObject>();

    [Tooltip("生成するデバッグ用テキストのプレハブ")]
    [SerializeField] GameObject _debugTextPrefab = null;

    [Tooltip("デバッグ用テキストのグラデーションの設定")]
    [SerializeField] Gradient _debugGradient = default;

    bool _isCreateDebugText = false;

    [SerializeField] float _debugTextPosY = 60.0f;//GroundEnemy = 60.0f, FlyingEnemy = 20.0f

    int _count = 0;

    [Tooltip("デバッグ用ゲームオブジェクトを破棄する時間")]
    [SerializeField] float _debugGameObjectDestroyTime = 3.0f;

    [Tooltip("デバッグ用の赤いキューブ")]
    [SerializeField] GameObject _debugLeftRedCubePrefab = null;

    [Tooltip("デバッグ用の青いキューブ")]
    [SerializeField] GameObject _debugRightBlueCubePrefab = null;

    [Tooltip("デバッグ用の大スフィア")]
    [SerializeField] GameObject _debugLargeSpherePrefab = null;

    [Tooltip("デバッグ用の中スフィア")]
    [SerializeField] GameObject _debugMediumSpherePrefab = null;

    [Tooltip("デバッグ用の小スフィア")]
    [SerializeField] GameObject _debugSmallSpherePrefab = null;

    [Tooltip("追跡の際に出す「!」画像")]
    [SerializeField] GameObject _alert = null;

    [Tooltip("前方の地面チェックのメッシュ")]
    [SerializeField] MeshRenderer _meshRendererGroundCheckFront = null;
    #endregion

    #region プロパティ

    public bool IsDebugMode
    {
        get { return _isDebugMode; }
        set { _isDebugMode = value; }
    }

    public bool IsCreateDebugText => _isCreateDebugText;

    public float DebugGameObjectDestroyTime => _debugGameObjectDestroyTime;

    public GameObject DebugLeftRedCubePrefab => _debugLeftRedCubePrefab;

    public GameObject DebugRightBlueCubePrefab => _debugRightBlueCubePrefab;

    public GameObject DebugLargeSpherePrefab => _debugLargeSpherePrefab;

    public GameObject DebugMediumSpherePrefab => _debugMediumSpherePrefab;

    public GameObject DebugSmallSpherePrefab => _debugSmallSpherePrefab;

    #endregion

    #region 公開関数

    /// <summary>
    /// デバッグ関連の処理
    /// </summary>
    public void DebugSystem(bool isDebugMode, Canvas canvas, int count)
    {
        _count = count;

        if (isDebugMode == true)
        {
            DebugCreateText(canvas);

            if (_isCreateDebugText == true)
            {
                foreach (var text in _debugTextList)
                {
                    text.GetComponent<TextMeshProUGUI>().color = DebugChangeTextColor();
                }
            }
        }
        else
        {
            DestroyAllDebugText();
        }
    }

    /// <summary>
    /// デバッグ用テキストに文字列をセットする
    /// </summary>
    public void SetDebugText(string[] debugTexts)
    {
        if (_isCreateDebugText == true)
        {
            for (int i = 0; i < _count; i++)
            {
                _debugTextList[i].GetComponent<TextMeshProUGUI>().text = debugTexts[i];
            }
        }
    }

    /// <summary>
    /// 追跡状態か？でアラートUIの表示を変更
    /// </summary> 
    public void DebugCheckChase(bool isDebugMode, bool isChase)
    {
        if (_alert == null)
        {
            return;
        }

        if (isDebugMode == false)
        {
            _alert.gameObject.SetActive(false);
            return;
        }

        _alert.gameObject.SetActive(isChase);
    }

    /// <summary>
    /// 前方の地面チェックメッシュの表示を変更
    /// </summary>
    public void DebugGroundCheckFrontMesh(bool isDebugMode)
    {
        if (_meshRendererGroundCheckFront == null)
        {
            return;
        }

        if (isDebugMode == false)
        {
            _meshRendererGroundCheckFront.enabled = false;
            return;
        }

        _meshRendererGroundCheckFront.enabled = true;
    }

    #endregion

    #region 非公開関数

    /// <summary>
    /// デバッグ用テキストを作成する処理
    /// </summary>
    void DebugCreateText(Canvas canvas)
    {
        if (_isCreateDebugText == false)
        {
            for (int i = 0; i < _count; i++)
            {
                GameObject debugTextGameObject = UnityEngine.Object.Instantiate(_debugTextPrefab, canvas.transform);
                float heightCalculation = (i * 10);
                debugTextGameObject.GetComponent<RectTransform>().localPosition = new Vector3(debugTextGameObject.GetComponent<RectTransform>().localPosition.x, _debugTextPosY + heightCalculation, debugTextGameObject.GetComponent<RectTransform>().localPosition.z);
                _debugTextList.Add(debugTextGameObject);
                _debugTextList[i].GetComponent<TextMeshProUGUI>().color = _debugGradient.Evaluate(0);
            }

            _isCreateDebugText = true;
        }
    }

    /// <summary>
    /// デバッグテキストカラーの色を変更
    /// </summary>
    Color DebugChangeTextColor()
    {
        return _debugGradient.Evaluate(Mathf.PingPong(Time.time, 1.0f));
    }

    /// <summary>
    /// 全てのデバッグテキストを削除する
    /// </summary>
    void DestroyAllDebugText()
    {
        foreach (var text in _debugTextList)
        {
            UnityEngine.Object.Destroy(text);
        }

        _debugTextList.Clear();

        _isCreateDebugText = false;
    }

    #endregion
}