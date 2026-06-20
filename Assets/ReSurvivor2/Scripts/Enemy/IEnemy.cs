using UnityEngine;

/// <summary>
/// エネミーインターフェース
/// </summary>
public interface IEnemy
{
    /// <summary>
    /// エネミーのゲームオブジェクトを取得する
    /// </summary>
    /// <returns></returns>
    public GameObject GetEnemyGameObject();

    /// <summary>
    /// ヒットポイントを取得する
    /// </summary>
    /// <returns></returns>
    public HitPoint GetHitPoint();
}
