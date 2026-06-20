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

    /// <summary>
	/// 追跡開始
	/// </summary>
	public void ChaseOn();

    /// <summary>
    /// カバーポイントをセット
    /// </summary>
    /// <param name="coverPoints">カバーポイント</param>
    public void SetCoverPoints(CoverPoint[] coverPoints);
}
