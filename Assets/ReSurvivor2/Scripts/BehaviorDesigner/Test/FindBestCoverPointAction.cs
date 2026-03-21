using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// プレイヤー位置に対して最適なカバーポイントを探すタスク
/// </summary>
[TaskCategory("Enemy/Cover")]
public class FindBestCoverPointAction : Action
{
	[Header("探索設定")]
	[SerializeField] float searchRadius = 20.0f;
	[SerializeField] float minDistanceFromPlayer = 5.0f;
	[SerializeField] float maxDistanceFromPlayer = 25.0f;
	[SerializeField] float minDistanceFromEnemy = 2.0f;

	[Header("判定設定")]
	[SerializeField] LayerMask obstacleLayer;
	[SerializeField] float eyeHeight = 1.4f;
	[SerializeField] float arriveSampleDistance = 2.0f;

	[Header("スコア設定")]
	[SerializeField] float distanceWeight = 1.0f;
	[SerializeField] float coverWeight = 10.0f;

	Target target = null;
	CoverPoint[] coverPoints;

	public override void OnStart()
	{
		target = this.GetComponent<Target>();
		coverPoints = target.CoverPoints;
	}

	public override TaskStatus OnUpdate()
	{
		if (coverPoints == null || coverPoints.Length == 0)
		{
			Debug.LogError("カバーポイントがない");
			return TaskStatus.Failure;
		}

		Vector3 enemyPosition = target.transform.position;
		Vector3 playerPosition = Player.SingletonInstance.transform.position;

		CoverPoint bestCoverPoint = null;
		float bestScore = float.MinValue;

		for (int i = 0; i < coverPoints.Length; i++)
		{
			CoverPoint currentCoverPoint = coverPoints[i];

			if (currentCoverPoint == null)
			{
				Debug.Log("currentCoverPoint == null");
				continue;
			}

			Vector3 coverPosition = currentCoverPoint.CoverPosition;

			float distanceFromEnemy = Vector3.Distance(enemyPosition, coverPosition);
			float distanceFromPlayer = Vector3.Distance(playerPosition, coverPosition);

			if (searchRadius < distanceFromEnemy)
			{
				Debug.Log("searchRadius < distanceFromEnemy");
				continue;
			}

			if (distanceFromEnemy < minDistanceFromEnemy)
			{
				Debug.Log("distanceFromEnemy < minDistanceFromEnemy");
				continue;
			}

			if (distanceFromPlayer < minDistanceFromPlayer || maxDistanceFromPlayer < distanceFromPlayer)
			{
				Debug.Log("distanceFromPlayer < minDistanceFromPlayer || maxDistanceFromPlayer < distanceFromPlayer");
				continue;
			}

			if (IsNavMeshPathInvalid(coverPosition) == true)
			{
				Debug.Log("IsNavMeshPathInvalid(coverPosition) == true");
				continue;
			}

			bool isCoveredFromPlayer = IsCoveredFromPlayer(playerPosition, coverPosition);
			if (isCoveredFromPlayer == false)
			{
				Debug.Log("isCoveredFromPlayer == false");
				continue;
			}

			float score = CalculateScore(enemyPosition, playerPosition, coverPosition, isCoveredFromPlayer);

			if (bestScore < score)
			{
				bestScore = score;
				bestCoverPoint = currentCoverPoint;
			}

			bestCoverPoint = currentCoverPoint;
		}

		if (bestCoverPoint == null)
		{
			Debug.Log("ベストカバーポイントがnull");
			return TaskStatus.Failure;
		}

		target.ResultCoverPosition = bestCoverPoint.CoverPosition;
		target.ResultCoverObject = bestCoverPoint.gameObject;

		return TaskStatus.Success;
	}

	/// <summary>
	/// プレイヤーからそのカバーポジションが遮蔽されているか判定
	/// </summary>
	bool IsCoveredFromPlayer(Vector3 playerPosition, Vector3 coverPosition)
	{
		Vector3 playerEyePosition = playerPosition + Vector3.up * eyeHeight;
		Vector3 coverEyePosition = coverPosition + Vector3.up * eyeHeight;
		Vector3 direction = coverEyePosition - playerEyePosition;
		float distance = direction.magnitude;

		if (distance <= 0.01f)
		{
			return false;
		}

		RaycastHit hitInfo;
		bool isHit = Physics.Raycast(playerEyePosition, direction.normalized, out hitInfo, distance, obstacleLayer);

		return isHit;
	}

	/// <summary>
	/// NavMesh上でその地点へ到達可能か判定
	/// </summary>
	bool IsNavMeshPathInvalid(Vector3 targetPosition)
	{
		NavMeshPath navMeshPath = new NavMeshPath();
		bool hasPath = target.NavMeshAgent.CalculatePath(targetPosition, navMeshPath);

		if (hasPath == false)
		{
			return true;
		}

		if (navMeshPath.status != NavMeshPathStatus.PathComplete)
		{
			return true;
		}

		if (navMeshPath.corners == null || navMeshPath.corners.Length == 0)
		{
			return true;
		}

		float remainingDistance = 0.0f;
		for (int i = 1; i < navMeshPath.corners.Length; i++)
		{
			remainingDistance += Vector3.Distance(navMeshPath.corners[i - 1], navMeshPath.corners[i]);
		}

		if (remainingDistance <= arriveSampleDistance)
		{
			return false;
		}

		return false;
	}

	/// <summary>
	/// カバー候補のスコア計算
	/// </summary>
	float CalculateScore(Vector3 enemyPosition, Vector3 playerPosition, Vector3 coverPosition, bool isCoveredFromPlayer)
	{
		float distanceFromEnemy = Vector3.Distance(enemyPosition, coverPosition);
		float distanceFromPlayer = Vector3.Distance(playerPosition, coverPosition);

		float score = 0.0f;

		if (isCoveredFromPlayer == true)
		{
			score += coverWeight;
		}

		float idealDistanceFromEnemy = searchRadius * 0.5f;
		float enemyDistanceScore = 1.0f - Mathf.Clamp01(Mathf.Abs(distanceFromEnemy - idealDistanceFromEnemy) / idealDistanceFromEnemy);

		float idealDistanceFromPlayer = (minDistanceFromPlayer + maxDistanceFromPlayer) * 0.5f;
		float playerDistanceScore = 1.0f - Mathf.Clamp01(Mathf.Abs(distanceFromPlayer - idealDistanceFromPlayer) / idealDistanceFromPlayer);

		score += enemyDistanceScore * distanceWeight;
		score += playerDistanceScore * distanceWeight;

		return score;
	}
}