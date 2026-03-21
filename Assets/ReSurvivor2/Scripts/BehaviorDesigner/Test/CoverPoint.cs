using UnityEngine;

/// <summary>
/// カバー位置として使用するポイント
/// </summary>
public class CoverPoint : MonoBehaviour
{
	[Header("この位置に立つオフセット")]
	[SerializeField] Vector3 standOffset = Vector3.zero;

	/// <summary>
	/// 実際にエネミーが移動する座標
	/// </summary>
	public Vector3 CoverPosition
	{
		get
		{
			return transform.position + standOffset;
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.cyan;
		Gizmos.DrawSphere(CoverPosition, 0.2f);
		Gizmos.DrawLine(transform.position, CoverPosition);
	}
}