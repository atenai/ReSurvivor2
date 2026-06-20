using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HitPoint
{
    [Tooltip("現在のHP")]
    float currentHp = 100.0f;
    public float CurrentHp
    {
        get { return currentHp; }
        set { currentHp = value; }
    }

    [Tooltip("HPの最大値")]
    float maxHp = 100.0f;
    public float MaxHp => maxHp;

    [Tooltip("死んだか？")]
    public bool IsDead
    {
        get { return currentHp <= 0.0f; }
    }

    [Tooltip("ダメージ中間処理")]
    UnityAction damageProcessing = null;
    [Tooltip("ダメージ修了処理")]
    UnityAction damageFinalize = null;

    [Tooltip("ヒール中間処理")]
    UnityAction healProcessing = null;
    [Tooltip("ヒール修了処理")]
    UnityAction healFinalize = null;

    public HitPoint()
    {

    }

    public void Init(UnityAction damageProcessing, UnityAction damageFinalize, UnityAction healProcessing, UnityAction healFinalize)
    {
        this.damageProcessing = damageProcessing;
        this.damageFinalize = damageFinalize;
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
            damageFinalize?.Invoke();
        }
    }

    /// <summary>
	/// HPを回復
	/// </summary>
	public void Heal()
    {
        if (maxHp <= currentHp)
        {
            return;
        }

        healProcessing?.Invoke();

        currentHp = maxHp;

        healFinalize?.Invoke();
    }

}
