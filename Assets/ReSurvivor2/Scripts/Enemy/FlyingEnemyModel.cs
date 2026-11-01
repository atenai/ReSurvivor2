using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 空中敵モデル
/// MVPパターンのModel担当
/// </summary>
[Serializable]
public class FlyingEnemyModel
{
    [UnityEngine.Tooltip("HP")]
    [SerializeField] HitPoint hp;
    public HitPoint HP => hp;


    [Header("レイキャスト")]
    /// <summary>視界な長さ</summary>
    float rayDistance = 20.0f;
    /// <summary>スフィアレイキャストの半径</summary>
    float sphereRayCastRadius = 2.0f;
    /// <summary>左右の最大角</summary>
    float halfAngle = 60f;
    /// <summary>度/秒（例：90なら1秒で90度回る）</summary>
    float sweepSpeed = 180f;
    /// <summary>現在の角度</summary>
    float currentAngle = 0f;
    /// <summary>角度の回転方向（+1で右へ、-1で左へ）</summary>
    float angleDir = 1f;

    /// <summary>
    /// リスポーンした際の初期化処理
    /// </summary>
    public void Initialize()
    {
        hp = new HitPoint(HitPoint.Max_Hp);
    }

    /// <summary>
    /// レイキャストによる視界
    /// </summary>
    public bool Eyesight(Vector3 pos, Vector3 forward, Vector3 right)
    {
        if (hp.IsDead == true)
        {
            return false;
        }
        // 角度を進める
        currentAngle = currentAngle + angleDir * sweepSpeed * Time.deltaTime;

        // 端で反転（往復）
        if (halfAngle < currentAngle)
        {
            currentAngle = halfAngle;
            angleDir = -1f;
        }
        else if (currentAngle < -halfAngle)
        {
            currentAngle = -halfAngle;
            angleDir = 1f;
        }

        if (forward.sqrMagnitude < 0.0001f)
        {
            forward = Vector3.forward;
        }
        forward.Normalize();

        // ローカルの右方向（head.transform.right）を回転軸にして上下にスイープ
        Vector3 dir = Quaternion.AngleAxis(currentAngle, right) * forward;

        Debug.DrawRay(pos, dir * rayDistance, Color.yellow);

        RaycastHit hit;
        if (Physics.SphereCast(pos, sphereRayCastRadius, dir, out hit, rayDistance))
        {
            // ヒット時の処理（例：プレイヤー検知など）
            if (hit.collider.CompareTag("Player"))
            {
                return true;
            }
        }

        return false;
    }
}
