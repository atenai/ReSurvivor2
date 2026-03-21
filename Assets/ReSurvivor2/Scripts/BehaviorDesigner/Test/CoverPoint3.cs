using UnityEngine;

/// <summary>
/// カバーポイント情報
/// </summary>
public class CoverPoint3 : MonoBehaviour
{
	[Header("隠れる位置")]
	[Tooltip("未設定ならこのオブジェクトの位置を使う")]
	[SerializeField] Transform hidePoint;

	[Header("設定")]
	[Tooltip("このカバーポイントが使用中かどうか")]
	[SerializeField] bool isOccupied = false;

	public bool IsOccupied
	{
		get { return isOccupied; }
		set { isOccupied = value; }
	}

	/// <summary>
	/// 実際に隠れる位置
	/// </summary>
	public Vector3 HidePosition
	{
		get
		{
			if (hidePoint != null)
			{
				return hidePoint.position;
			}

			return transform.position;
		}
	}
}