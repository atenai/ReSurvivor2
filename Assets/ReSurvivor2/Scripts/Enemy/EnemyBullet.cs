using System;
using UnityEngine;

/// <summary>
/// エネミーの弾（弾道処理と弾道エフェクト）
/// ・毎フレーム「今の位置 → 次の位置」の区間をレイキャストで調べてから進むので、弾がどれだけ速くても障害物をすり抜けない
/// ・プレイヤーへのダメージは、弾が実際にプレイヤーまで届いた時に与える（壁に当たった弾はダメージを与えない）
/// ・弾道エフェクト（光の筋）は LineRenderer で描き、着弾点より先には描かない
/// </summary>
public class EnemyBullet : MonoBehaviour
{
	[Tooltip("弾道エフェクト（光の筋）")]
	[SerializeField] LineRenderer lineRenderer;
	[Tooltip("光の筋の長さ")]
	[SerializeField] float tracerLength = 3.5f;

	[Tooltip("1回のレイキャストで受け取る当たりの最大数")]
	const int HitBufferSize = 16;
	static readonly RaycastHit[] hitBuffer = new RaycastHit[HitBufferSize];
	static readonly Collider[] overlapBuffer = new Collider[HitBufferSize];

	[Tooltip("弾の先端に重なったコライダーを調べ直す時に、どれだけ手前からレイを撃つか（カプセルの直径より長くする）")]
	const float BacktrackDistance = 1.5f;

	[Tooltip("弾道エフェクト用のレイヤー（BulletEffect）")]
	const int BulletEffectLayer = 11;

	/// <summary>
	/// 弾が当たるレイヤー
	/// Ignore Raycast（トリガーやフェンスの移動ブロッカー）と BulletEffect レイヤーには当たらない
	/// </summary>
	public static int HitLayerMask => Physics.DefaultRaycastLayers & ~(1 << BulletEffectLayer);

	[Tooltip("撃ったエネミー")]
	GroundEnemy shooter;
	[Tooltip("撃ったエネミー自身のコライダーには当たらないようにするための親")]
	Transform shooterRoot;
	[Tooltip("ダメージ")]
	float damage;
	[Tooltip("弾の速度")]
	float speed;
	[Tooltip("弾が進める残りの距離")]
	float remainingRange;
	[Tooltip("弾の進行方向")]
	Vector3 direction;
	[Tooltip("発射位置")]
	Vector3 origin;
	[Tooltip("弾の先端の位置")]
	Vector3 head;
	[Tooltip("光の筋の後端の位置")]
	Vector3 tail;
	[Tooltip("飛んでいる最中か（false なら着弾済みで光の筋を消している最中）")]
	bool isFlying = false;
	[Tooltip("使用中か")]
	bool isActive = false;
	[Tooltip("風切り音を鳴らしたか（1発につき1回だけ）")]
	bool isFlybyPlayed = false;

	[Tooltip("風切り音が鳴る、プレイヤーと弾の距離")]
	const float FlybyRadius = 2.5f;
	[Tooltip("風切り音の判定に使うプレイヤーの高さ（頭のあたり）")]
	const float FlybyHeight = 1.5f;
	[Tooltip("弾が消えた時に呼ぶ処理（プールへの返却）")]
	Action<EnemyBullet> onFinished;

	public bool IsActive => isActive;

	/// <summary>
	/// 弾を発射する
	/// </summary>
	public void Launch(GroundEnemy shooter, Vector3 origin, Vector3 direction, float speed, float range, float damage, Action<EnemyBullet> onFinished)
	{
		this.shooter = shooter;
		this.shooterRoot = shooter != null ? shooter.transform : null;
		this.origin = origin;
		this.direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
		this.speed = Mathf.Max(speed, 0.1f);
		this.remainingRange = Mathf.Max(range, 0.0f);
		this.damage = damage;
		this.onFinished = onFinished;

		head = origin;
		tail = origin;
		isFlying = true;
		isActive = true;
		isFlybyPlayed = false;

		this.transform.position = origin;
		this.transform.rotation = Quaternion.LookRotation(this.direction);

		if (lineRenderer != null)
		{
			lineRenderer.positionCount = 2;
			lineRenderer.enabled = false;//まだ長さが0なので表示しない
		}
	}

	void Update()
	{
		Tick(Time.deltaTime);
	}

	/// <summary>
	/// 弾を進める（Time.timeScale = 0 のポーズ中は deltaTime が 0 なので止まる）
	/// </summary>
	public void Tick(float deltaTime)
	{
		if (isActive == false || deltaTime <= 0.0f)
		{
			return;
		}

		float step = speed * deltaTime;

		if (isFlying == true)
		{
			MoveHead(step);
		}

		//光の筋の後端を進める（先端からの長さは tracerLength まで、発射位置より後ろにはしない）
		float traveled = Vector3.Distance(origin, head);
		float tailDistance = Mathf.Max(0.0f, traveled - tracerLength);
		if (isFlying == false)
		{
			//着弾後は後端だけが先端（着弾点）に追いついて光の筋が消える
			float currentTailDistance = Vector3.Distance(origin, tail);
			tailDistance = Mathf.Min(traveled, Mathf.Max(tailDistance, currentTailDistance + step));
		}
		tail = origin + direction * tailDistance;

		UpdateTracer();

		if (isFlying == false && traveled - tailDistance <= 0.001f)
		{
			Finish();
		}
	}

	/// <summary>
	/// 弾の先端を進める
	/// 移動する区間を先にレイキャストで調べ、当たった物があればそこで止める
	/// </summary>
	void MoveHead(float step)
	{
		float distance = Mathf.Min(step, remainingRange);
		if (distance <= 0.0f)
		{
			isFlying = false;
			return;
		}

		//前のフレームから今までの間に、動いているもの（プレイヤーやエネミー）が弾の先端に重なっていないか
		//（レイは始点を含むコライダーには当たらないので、先に調べておかないと体をすり抜けてしまう）
		if (FindOverlapAtHead(out RaycastHit overlapHit, out bool hasHitInfo, out Collider overlapCollider) == true)
		{
			isFlying = false;
			if (hasHitInfo == true)
			{
				head = overlapHit.point;
				OnHit(overlapHit);
			}
			else
			{
				DamagePlayer(overlapCollider);
			}
			return;
		}

		if (FindHit(head, direction, distance, out RaycastHit hit) == true)
		{
			//プレイヤー以外に当たった時は、当たるまでの間にプレイヤーの近くを通ったか調べる
			if (hit.collider.CompareTag("Player") == false)
			{
				CheckFlyby(head, hit.point);
			}
			head = hit.point;
			isFlying = false;
			OnHit(hit);
			return;
		}

		CheckFlyby(head, head + direction * distance);
		head = head + direction * distance;
		remainingRange = remainingRange - distance;
		if (remainingRange <= 0.0f)
		{
			//射程の終わりまで飛んだ
			isFlying = false;
		}
	}

	/// <summary>
	/// 弾がプレイヤーをかすめたら風切り音を鳴らす（1発につき1回だけ）
	/// この区間の中でプレイヤーの頭に一番近づき、その距離が FlybyRadius 以内なら、一番近づいた位置で鳴らす
	/// </summary>
	void CheckFlyby(Vector3 from, Vector3 to)
	{
		if (isFlybyPlayed == true)
		{
			return;
		}
		if (PlayerManagerPresenter.SingletonInstance == null || SoundManager.SingletonInstance == null || SoundManager.SingletonInstance.BulletFlybySEPool == null)
		{
			return;
		}

		Vector3 listener = PlayerManagerPresenter.SingletonInstance.transform.position + Vector3.up * FlybyHeight;
		Vector3 segment = to - from;
		float lengthSqr = segment.sqrMagnitude;
		if (lengthSqr < 0.000001f)
		{
			return;
		}

		//一番近づく位置（区間の端の時は、まだ近づいている途中か、もう離れていくところなので鳴らさない）
		float t = Vector3.Dot(listener - from, segment) / lengthSqr;
		if (t <= 0.0f || 1.0f <= t)
		{
			return;
		}

		Vector3 closest = from + segment * t;
		if ((closest - listener).sqrMagnitude > FlybyRadius * FlybyRadius)
		{
			return;
		}

		isFlybyPlayed = true;
		SoundManager.SingletonInstance.BulletFlybySEPool.Play(closest);
	}

	/// <summary>
	/// 区間内で一番手前にある当たり（撃ったエネミー自身とトリガーは除く）を探す
	/// </summary>
	bool FindHit(Vector3 from, Vector3 dir, float distance, out RaycastHit nearest)
	{
		nearest = default;
		int count = Physics.RaycastNonAlloc(from, dir, hitBuffer, distance, HitLayerMask, QueryTriggerInteraction.Ignore);
		//バッファがいっぱいの時は一番手前の当たりが入っていない可能性があるので、全部取り直す（大きくフレームが飛んだ時など）
		RaycastHit[] hits = hitBuffer;
		if (count >= HitBufferSize)
		{
			hits = Physics.RaycastAll(from, dir, distance, HitLayerMask, QueryTriggerInteraction.Ignore);
			count = hits.Length;
		}
		float nearestDistance = float.MaxValue;
		bool found = false;
		for (int i = 0; i < count; i++)
		{
			RaycastHit hit = hits[i];
			if (hit.collider == null)
			{
				continue;
			}
			if (IsShooterCollider(hit.collider) == true)
			{
				continue;
			}
			if (hit.distance < nearestDistance)
			{
				nearestDistance = hit.distance;
				nearest = hit;
				found = true;
			}
		}
		return found;
	}

	/// <summary>
	/// 弾の先端がコライダーの中に入っていないか調べる（撃ったエネミー自身は除く）
	/// 入っていれば、そのコライダーだけに手前からレイを撃って着弾点と法線を求める
	/// </summary>
	bool FindOverlapAtHead(out RaycastHit hit, out bool hasHitInfo, out Collider overlapCollider)
	{
		hit = default;
		hasHitInfo = false;
		overlapCollider = null;

		int count = Physics.OverlapSphereNonAlloc(head, 0.01f, overlapBuffer, HitLayerMask, QueryTriggerInteraction.Ignore);
		for (int i = 0; i < count; i++)
		{
			Collider collider = overlapBuffer[i];
			if (collider == null || IsShooterCollider(collider) == true)
			{
				continue;
			}

			overlapCollider = collider;
			Ray ray = new Ray(head - direction * BacktrackDistance, direction);
			if (collider.Raycast(ray, out hit, BacktrackDistance + 0.05f) == true)
			{
				hasHitInfo = true;
			}
			return true;
		}
		return false;
	}

	/// <summary>
	/// 撃ったエネミー自身のコライダーか？
	/// </summary>
	bool IsShooterCollider(Collider collider)
	{
		if (shooterRoot == null)
		{
			return false;
		}
		return collider.transform == shooterRoot || collider.transform.IsChildOf(shooterRoot);
	}

	/// <summary>
	/// 着弾した時の処理
	/// </summary>
	void OnHit(RaycastHit hit)
	{
		DamagePlayer(hit.collider);

		//着弾エフェクト
		if (EffectManager.SingletonInstance != null)
		{
			EffectManager.SingletonInstance.ImpactEffect(hit);
		}
	}

	/// <summary>
	/// 当たったのがプレイヤーならダメージを与える
	/// </summary>
	void DamagePlayer(Collider collider)
	{
		if (collider != null && collider.CompareTag("Player"))//※間違ってオブジェクトの設定にレイヤーとタグを間違えるなよおれｗ
		{
			//ダメージ
			var player = collider.GetComponentInParent<PlayerManagerPresenter>();
			if (player != null)
			{
				player.PlayerModel.HP.Damage(damage);

				//撃ったエネミーがまだ居ればカメラを揺らして敵マーカーを表示
				if (shooter != null)
				{
					shooter.CameraShaker();
					if (EnemyIndicatorManager.SingletonInstance != null)
					{
						EnemyIndicatorManager.SingletonInstance.ShowIndicator(shooter);
					}
				}
			}
		}
	}

	/// <summary>
	/// 光の筋の表示を更新
	/// </summary>
	void UpdateTracer()
	{
		if (lineRenderer == null)
		{
			return;
		}

		bool visible = (head - tail).sqrMagnitude > 0.0001f;
		lineRenderer.enabled = visible;
		if (visible == true)
		{
			lineRenderer.SetPosition(0, tail);
			lineRenderer.SetPosition(1, head);
		}
		this.transform.position = head;
	}

	/// <summary>
	/// 弾を消す（プールに返却）
	/// </summary>
	public void Finish()
	{
		if (isActive == false)
		{
			return;
		}

		isActive = false;
		isFlying = false;
		shooter = null;
		shooterRoot = null;
		if (lineRenderer != null)
		{
			lineRenderer.enabled = false;
		}

		var callback = onFinished;
		onFinished = null;
		if (callback != null)
		{
			callback(this);
		}
		else
		{
			this.gameObject.SetActive(false);
		}
	}

	public Vector3 Head => head;
	public Vector3 Tail => tail;
	public bool IsFlying => isFlying;
}
