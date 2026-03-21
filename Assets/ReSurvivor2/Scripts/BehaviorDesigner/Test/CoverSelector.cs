using UnityEngine;

public class CoverSelector : MonoBehaviour
{
	[Tooltip("カバーポイント")]
	[SerializeField] CoverPoint2[] coverPoints;
	public CoverPoint2[] CoverPoints => coverPoints;

	[Tooltip("プレイヤー Transform。未設定時は Tag=Player で検索します")]
	[SerializeField] Transform player;

	[Tooltip("敵からの最大探索距離")]
	[SerializeField] float maxDistance = 30f;

	[Tooltip("遮蔽物判定に使うレイヤーマスク")]
	[SerializeField] LayerMask obstructionMask = ~0;

	[Tooltip("自動で毎フレーム検索するか")]
	[SerializeField] bool autoFind = true;

	Vector3 resultCoverPosition;
	public Vector3 ResultCoverPosition { get { return resultCoverPosition; } set { resultCoverPosition = value; } }

	void Reset()
	{
		if (player == null)
			FindPlayer();
		if (coverPoints == null || coverPoints.Length == 0)
			coverPoints = FindObjectsOfType<CoverPoint2>();
	}

	void Awake()
	{
		if (player == null)
			FindPlayer();
		if (coverPoints == null || coverPoints.Length == 0)
			coverPoints = FindObjectsOfType<CoverPoint2>();
		resultCoverPosition = transform.position;
	}

	void Update()
	{
		if (autoFind)
			FindBestCover();
	}

	void FindPlayer()
	{
		var go = GameObject.FindWithTag("Player");
		if (go != null) player = go.transform;
	}

	// プレイヤー視点から遮られている（＝隠れられる）カバーポイントを探す
	// 見つかれば ResultCoverPosition に代入して true を返す
	public bool FindBestCover()
	{
		if (player == null || coverPoints == null || coverPoints.Length == 0)
		{
			resultCoverPosition = transform.position;
			return false;
		}

		float bestDist = float.MaxValue;
		CoverPoint2 best = null;

		foreach (var cp in coverPoints)
		{
			if (cp == null) continue;
			float distToEnemy = Vector3.Distance(transform.position, cp.Position);
			if (distToEnemy > maxDistance) continue;

			Vector3 dir = cp.Position - player.position;
			float dist = dir.magnitude;
			if (dist <= 0.01f) continue;

			RaycastHit hit;
			if (Physics.Raycast(player.position, dir.normalized, out hit, dist, obstructionMask))
			{
				// プレイヤーからカバーポイントへの途中で何かに当たる -> 視線が遮られている
				if (hit.collider != null && hit.collider.gameObject != cp.gameObject)
				{
					if (distToEnemy < bestDist)
					{
						bestDist = distToEnemy;
						best = cp;
					}
				}
			}
		}

		if (best != null)
		{
			resultCoverPosition = best.Position;
			return true;
		}

		// フォールバック: プレイヤーからもっとも遠いカバーポイントを使う
		float bestScore = -1f;
		foreach (var cp in coverPoints)
		{
			if (cp == null) continue;
			float dE = Vector3.Distance(transform.position, cp.Position);
			if (dE > maxDistance) continue;
			float dP = Vector3.Distance(player.position, cp.Position);
			if (dP > bestScore)
			{
				bestScore = dP;
				best = cp;
			}
		}

		if (best != null)
		{
			resultCoverPosition = best.Position;
			return true;
		}

		resultCoverPosition = transform.position;
		return false;
	}

	void OnDrawGizmosSelected()
	{
		if (coverPoints != null)
		{
			Gizmos.color = Color.yellow;
			foreach (var cp in coverPoints)
				if (cp != null) Gizmos.DrawSphere(cp.Position, 0.15f);
		}

		Gizmos.color = Color.green;
		Gizmos.DrawSphere(resultCoverPosition, 0.2f);
		Gizmos.DrawLine(transform.position, resultCoverPosition);
	}
}
