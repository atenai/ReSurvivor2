using UnityEngine;

/// <summary>
/// プレイヤー位置に対して適切なカバーポイントを選ぶ
/// </summary>
public class EnemyCoverSelector : MonoBehaviour
{
	[Header("参照")]
	[SerializeField] Transform player;

	[Tooltip("カバーポイント")]
	[SerializeField] CoverPoint3[] coverPoints;
	public CoverPoint3[] CoverPoints => coverPoints;

	[Header("探索設定")]
	[Tooltip("この距離以内のカバーポイントを候補にする")]
	[SerializeField] float searchRange = 30.0f;

	[Tooltip("どれくらいの頻度で再探索するか")]
	[SerializeField] float recalculateInterval = 0.25f;

	[Header("評価設定")]
	[Tooltip("エネミーから近いほど高評価")]
	[SerializeField] float distanceWeight = 1.0f;

	[Tooltip("プレイヤーの射線が遮られているほど高評価")]
	[SerializeField] float coverWeight = 10.0f;

	[Tooltip("プレイヤーとの距離が近すぎるカバーを避ける")]
	[SerializeField] float minDistanceFromPlayer = 3.0f;

	[Tooltip("プレイヤーの目線高さ補正")]
	[SerializeField] float playerEyeHeight = 1.6f;

	[Tooltip("エネミーの目線高さ補正")]
	[SerializeField] float enemyEyeHeight = 1.4f;

	[Tooltip("遮蔽物判定に使うレイヤー")]
	[SerializeField] LayerMask coverLayerMask = Physics.DefaultRaycastLayers;

	Vector3 resultCoverPosition;
	public Vector3 ResultCoverPosition
	{
		get { return resultCoverPosition; }
		set { resultCoverPosition = value; }
	}

	CoverPoint3 currentBestCoverPoint;
	public CoverPoint3 CurrentBestCoverPoint => currentBestCoverPoint;

	float timer = 0.0f;

	void Start()
	{
		if (player == null)
		{
			GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
			if (playerObject != null)
			{
				player = playerObject.transform;
			}
		}

		SearchBestCoverPoint();
	}

	void Update()
	{
		if (player == null)
		{
			return;
		}

		timer += Time.deltaTime;

		if (timer >= recalculateInterval)
		{
			timer = 0.0f;
			SearchBestCoverPoint();
		}
	}

	/// <summary>
	/// 最適なカバーポイントを探す
	/// </summary>
	void SearchBestCoverPoint()
	{
		if (coverPoints == null || coverPoints.Length == 0)
		{
			return;
		}

		float bestScore = float.MinValue;
		CoverPoint3 bestPoint = null;
		Vector3 bestPosition = transform.position;

		for (int i = 0; i < coverPoints.Length; i++)
		{
			CoverPoint3 point = coverPoints[i];

			if (point == null)
			{
				continue;
			}

			if (point.IsOccupied == true)
			{
				continue;
			}

			Vector3 candidatePosition = point.HidePosition;

			float enemyDistance = Vector3.Distance(transform.position, candidatePosition);
			if (enemyDistance > searchRange)
			{
				continue;
			}

			float playerDistance = Vector3.Distance(player.position, candidatePosition);
			if (playerDistance < minDistanceFromPlayer)
			{
				continue;
			}

			float score = EvaluateCoverPoint(point, candidatePosition, enemyDistance);

			if (score > bestScore)
			{
				bestScore = score;
				bestPoint = point;
				bestPosition = candidatePosition;
			}
		}

		currentBestCoverPoint = bestPoint;
		ResultCoverPosition = bestPosition;
	}

	/// <summary>
	/// カバーポイントの評価
	/// </summary>
	float EvaluateCoverPoint(CoverPoint3 point, Vector3 candidatePosition, float enemyDistance)
	{
		float score = 0.0f;

		// 1. 近いほど高評価
		float distanceScore = 1.0f / (enemyDistance + 0.1f);
		score += distanceScore * distanceWeight;

		// 2. プレイヤーから見て遮蔽物で隠れているなら大きく加点
		bool isProtectedFromPlayer = IsProtectedFromPlayer(candidatePosition);
		if (isProtectedFromPlayer == true)
		{
			score += coverWeight;
		}

		// 3. プレイヤーと一直線すぎる危険な位置は少し減点
		Vector3 toPlayer = (player.position - candidatePosition).normalized;
		Vector3 toEnemy = (transform.position - candidatePosition).normalized;
		float dot = Vector3.Dot(toPlayer, toEnemy);

		// プレイヤーとエネミーが候補地点を挟んで一直線気味なら少し危険とみなす
		score -= Mathf.Max(0.0f, dot) * 0.5f;

		return score;
	}

	/// <summary>
	/// プレイヤーから候補地点への射線が遮蔽物で遮られているか
	/// </summary>
	bool IsProtectedFromPlayer(Vector3 candidatePosition)
	{
		Vector3 playerEyePosition = player.position + Vector3.up * playerEyeHeight;
		Vector3 coverTargetPosition = candidatePosition + Vector3.up * enemyEyeHeight;

		Vector3 direction = coverTargetPosition - playerEyePosition;
		float distance = direction.magnitude;

		if (distance <= 0.01f)
		{
			return false;
		}

		direction.Normalize();

		RaycastHit hit;
		bool isHit = Physics.Raycast(playerEyePosition, direction, out hit, distance, coverLayerMask, QueryTriggerInteraction.Ignore);

		if (isHit == true)
		{
			// 自分自身やプレイヤー自身ではなく、途中に何か遮蔽物があればカバー成立
			if (hit.transform != transform && hit.transform != player)
			{
				return true;
			}
		}

		return false;
	}

	void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.cyan;
		Gizmos.DrawWireSphere(transform.position, searchRange);

		Gizmos.color = Color.green;
		Gizmos.DrawSphere(ResultCoverPosition, 0.2f);

		if (player != null)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawLine(player.position + Vector3.up * playerEyeHeight, ResultCoverPosition + Vector3.up * enemyEyeHeight);
		}
	}
}