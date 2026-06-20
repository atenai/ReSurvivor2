using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// ヒットポイント
/// </summary>
public class HitPoint
{
    [Tooltip("現在のHP")]
    float currentHp = 100.0f;
    public float CurrentHp => currentHp;

    [Tooltip("HPの最大値")]
    public static readonly float MaxHp = 100.0f;

    [Tooltip("死んだか？")]
    public bool IsDead
    {
        get { return currentHp <= 0.0f; }
    }

    [Tooltip("ダメージ中間処理")]
    UnityAction damageProcessing = null;
    [Tooltip("死亡処理処理")]
    UnityAction dead = null;

    [Tooltip("ヒール中間処理")]
    UnityAction healProcessing = null;
    [Tooltip("ヒール修了処理")]
    UnityAction healFinalize = null;

    public HitPoint(float initHP)
    {
        currentHp = initHP;
    }

    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <param name="damageProcessing">ダメージ中間処理</param>
    /// <param name="dead">死亡処理処理</param>
    /// <param name="healProcessing">ヒール中間処理</param>
    /// <param name="healFinalize">ヒール修了処理</param>
    public void Initialize(UnityAction damageProcessing = null, UnityAction dead = null, UnityAction healProcessing = null, UnityAction healFinalize = null)
    {
        this.damageProcessing = damageProcessing;
        this.dead = dead;
        this.healProcessing = healProcessing;
        this.healFinalize = healFinalize;
    }

    /// <summary>
    /// ダメージ処理
    /// </summary>
    /// <param name="amount">ダメージ量</param>
    public void Damage(float amount)
    {
        currentHp = currentHp - amount;

        damageProcessing?.Invoke();

        if (IsDead == true)
        {
            dead?.Invoke();
        }
    }

    /// <summary>
	/// HPを回復
	/// </summary>
	public void Heal()
    {
        if (MaxHp <= currentHp)
        {
            return;
        }

        healProcessing?.Invoke();

        currentHp = MaxHp;

        healFinalize?.Invoke();
    }

}
