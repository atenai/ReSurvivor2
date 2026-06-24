using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// スタミナ
/// </summary>
[Serializable]
public class Stamina
{
    [Tooltip("現在のスタミナ")]
    float currentStamina = 1000.0f;
    public float CurrentStamina => currentStamina;
    [Tooltip("スタミナの最大値")]
    public static readonly float Max_Stamina = 1000.0f;
    [Tooltip("疲れてダッシュできなくなる時のスタミナ値")]
    public static readonly float Tired_Stamina = 100.0f;
    [Tooltip("疲れたか？")]
    public bool IsTired
    {
        get { return currentStamina <= Tired_Stamina; }
    }

    [Tooltip("スタミナを消費修了処理")]
    UnityAction consumeStaminaFinalize = null;

    [Tooltip("スタミナを回復中間処理")]
    UnityAction restoresStaminaProcessing = null;
    [Tooltip("スタミナを回復終了処理")]
    UnityAction restoresStaminaFinalize = null;

    public Stamina(float initStamina)
    {
        currentStamina = initStamina;
    }

    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <param name="consumeStaminaFinalize">スタミナを消費修了処理</param>
    /// <param name="restoresStaminaProcessing">スタミナを回復中間処理</param>
    /// <param name="restoresStaminaFinalize">スタミナを回復終了処理</param>
    public void Initialize(UnityAction consumeStaminaFinalize = null, UnityAction restoresStaminaProcessing = null, UnityAction restoresStaminaFinalize = null)
    {
        this.consumeStaminaFinalize = consumeStaminaFinalize;
        this.restoresStaminaProcessing = restoresStaminaProcessing;
        this.restoresStaminaFinalize = restoresStaminaFinalize;
    }

    /// <summary>
    /// スタミナを消費
    /// </summary> 
    public void ConsumeStamina(float amount)
    {
        if (currentStamina <= 0.0f)
        {
            currentStamina = 0.0f;
            return;
        }

        currentStamina = currentStamina - amount;

        consumeStaminaFinalize.Invoke();
    }

    /// <summary>
    /// スタミナを回復
    /// </summary>
    public void RestoresStamina()
    {
        if (Max_Stamina <= currentStamina)
        {
            return;
        }

        restoresStaminaProcessing.Invoke();

        currentStamina = Max_Stamina;

        restoresStaminaFinalize.Invoke();
    }
}
