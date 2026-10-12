using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 地上敵UIビュー
/// MVPパターンのView担当
/// </summary>
public class GroundEnemyUIView : MonoBehaviour
{
    [UnityEngine.Tooltip("キャンバス")]
    [SerializeField] Canvas canvas;
    public Canvas Canvas => canvas;

    [UnityEngine.Tooltip("HPバー")]
    [SerializeField] Slider sliderHp;
    public Slider SliderHp
    {
        get { return sliderHp; }
        set { sliderHp = value; }
    }

    [Tooltip("追跡中か？のアラートイメージ")]
    [SerializeField] GameObject alert;
    public GameObject Alert => alert;

    /// <summary>
    /// 常にキャンバスをメインカメラの方を向かせる
    /// </summary>
    public void RotCanvas()
    {
        canvas.transform.rotation = Camera.main.transform.rotation;
    }

    /// <summary>
	/// HPの初期化処理
	/// </summary>
	public void InitHP(float currentHp)
    {
        Debug.Log("InitHP");
        sliderHp.value = currentHp / HitPoint.Max_Hp;
    }

    void Start()
    {

    }

    void Update()
    {

    }
}
