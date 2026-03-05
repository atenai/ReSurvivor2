using UnityEngine;

public class EnemySweepRay : MonoBehaviour
{
	float distance = 10f;
	float halfAngle = 45f;   // 左右の最大角
	float sweepSpeed = 180f;  // 度/秒（例：90なら1秒で90度回る）

	float currentAngle = 0f;
	float angleDir = 1f; // +1で右へ、-1で左へ

	private void Update()
	{
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

		Vector3 pos = this.transform.position;

		// 水平だけにするため、forward をXZ平面へ投影
		Vector3 forward = this.transform.forward;
		forward.y = 0f;

		if (forward.sqrMagnitude < 0.0001f)
		{
			forward = Vector3.forward;
		}
		forward.Normalize();

		// 今の角度だけ回した方向に1本Ray
		Vector3 dir = Quaternion.AngleAxis(currentAngle, Vector3.up) * forward;

		Debug.DrawRay(pos, dir * distance, Color.green);

		RaycastHit hit;
		if (Physics.Raycast(pos, dir, out hit, distance))
		{
			// ヒット時の処理（例：プレイヤー検知など）
			if (hit.collider.CompareTag("Player"))
			{
				Debug.Log("Player発見!");
			}
		}
	}
}