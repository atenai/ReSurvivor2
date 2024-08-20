using UnityEngine;
using System;
using System.ComponentModel;


/// <summary>
/// SRデバッガー
/// </summary>
public partial class SROptions
{
    #region 定数

    /// <summary>
    /// 全般カテゴリ
    /// </summary>
    private const string GeneralCategory = "General";

    #endregion


    #region デバッグ機能

    [Category(GeneralCategory)]
    [DisplayName("TimeScale")]
    [Sort(0)]
    [Increment(0.1)]
    [NumberRange(0.0, 10.0)]
    public float TimeScale
    {
        get
        {
            return Time.timeScale;
        }
        set
        {
            Time.timeScale = value;
        }
    }

    [Category(GeneralCategory)]
    [DisplayName("DisplayDateTime")]
    [Sort(1)]
    public void DisplayDateTime()
    {
        Debug.Log(DateTime.Now.ToString("yyyy/MM/dd"));
    }

    [Category(GeneralCategory)]
    [DisplayName("LightEnabled")]
    [Sort(2)]
    public bool LightEnabled
    {
        get
        {
            return GameObject.FindObjectOfType<Light>().enabled;
        }
        set
        {
            GameObject.FindObjectOfType<Light>().enabled = value;
        }
    }

    [Category("Player")]
    [DisplayName("アーマーを増やす")]
    [Sort(0)]
    public void IncreaseArmorPlate()
    {
        Player.SingletonInstance.AcquireArmorPlate();
    }

    #endregion
}
