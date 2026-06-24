using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

/// <summary>
/// プレイヤーの方向へゆっくり向くタスク
/// </summary>
[TaskCategory("GroundEnemy")]
public class FaceTargetAction : Action
{
	[SerializeField] float rotateSpeed = 8.0f;
	[SerializeField] float angleTolerance = 5.0f;

	public override TaskStatus OnUpdate()
	{
		var player = PlayerManager.SingletonInstance;
		if (player == null)
		{
			return TaskStatus.Failure;
		}

		Vector3 direction = player.transform.position - this.transform.position;
		direction.y = 0.0f;

		if (direction.sqrMagnitude <= 0.001f)
		{
			return TaskStatus.Success;
		}

		Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
		this.transform.rotation = Quaternion.Slerp(
			this.transform.rotation,
			targetRotation,
			Time.deltaTime * rotateSpeed
		);

		float angle = Quaternion.Angle(this.transform.rotation, targetRotation);
		if (angle <= angleTolerance)
		{
			return TaskStatus.Success;
		}

		return TaskStatus.Running;
	}
}