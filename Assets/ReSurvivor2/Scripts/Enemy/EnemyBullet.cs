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

	[Tooltip("光の筋（LineRenderer）の点の数（後端と先端の2点）")]
	const int TracerPointCount = 2;
	[Tooltip("光の筋の後端の点の番号")]
	const int TracerTailIndex = 0;
	[Tooltip("光の筋の先端の点の番号")]
	const int TracerHeadIndex = 1;

	[Tooltip("弾の先端に重なったコライダーを調べ直す時に、どれだけ手前からレイを撃つか（カプセルの直径より長くする）")]
	const float BacktrackDistance = 1.5f;
	[Tooltip("手前から撃ち直すレイの余裕（先端より少し先まで調べる）")]
	const float BacktrackMargin = 0.05f;
	[Tooltip("弾の先端がコライダーに重なっていないか調べる球の半径")]
	const float HeadOverlapRadius = 0.01f;
	[Tooltip("弾の最低速度")]
	const float MinSpeed = 0.1f;
	[Tooltip("これより短いベクトルは向きが決まらないものとして扱う（長さの2乗）")]
	const float MinSqrLength = 0.0001f;
	[Tooltip("光の筋の長さがこれ以下になったら消えたものとして扱う")]
	const float TracerEndLength = 0.001f;

	[Tooltip("弾道エフェクト用のレイヤー名（このレイヤーのコライダーには当たらない）")]
	const string BulletEffectLayerName = "BulletEffect";

	/// <summary>
	/// 弾が当たるレイヤー（Ignore Raycast 以外のすべて。トリガーやフェンスの移動ブロッカーは Ignore Raycast なので当たらない）
	/// BulletEffect レイヤーは IsIgnoredCollider で除外する
	/// </summary>
	public static int HitLayerMask => Physics.DefaultRaycastLayers;

	[Tooltip("撃ったエネミー")]
	GroundEnemy shooter;
	[Tooltip("撃ったエネミー自身のコライダーには当たらないようにするための親")]
	Transform shooterRoot;
	[Tooltip("この弾を返却するプール（プール無しで撃った時は null）")]
	EnemyBulletPool pool;
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
	[Tooltip("区間の長さ（2乗）がこれより短い時は、風切り音の判定をしない")]
	const float MinFlybySegmentSqrLength = 0.000001f;
	[Tooltip("区間の始点を表す割合")]
	const float SegmentStart = 0.0f;
	[Tooltip("区間の終点を表す割合")]
	const float SegmentEnd = 1.0f;

	public bool IsActive => isActive;

	/// <summary>
	/// 弾を発射する
	/// </summary>
	/// <param name="pool">弾が消えた時に返却するプール（null の時は非表示にするだけ）</param>
	public void Launch(GroundEnemy shooter, Vector3 origin, Vector3 direction, float speed, float range, float damage, EnemyBulletPool pool)
	{
		this.shooter = shooter;
		this.shooterRoot = shooter != null ? shooter.transform : null;
		this.pool = pool;
		this.origin = origin;
		this.direction = MinSqrLength < direction.sqrMagnitude ? direction.normalized : Vector3.forward;
		this.speed = Mathf.Max(speed, MinSpeed);
		this.remainingRange = Mathf.Max(range, 0.0f);
		this.damage = damage;

		head = origin;
		tail = origin;
		isFlying = true;
		isActive = true;
		isFlybyPlayed = false;

		this.transform.position = origin;
		this.transform.rotation = Quaternion.LookRotation(this.direction);

		if (lineRenderer != null)
		{
			lineRenderer.positionCount = TracerPointCount;
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

		if (isFlying == false && traveled - tailDistance <= TracerEndLength)
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
		if (lengthSqr < MinFlybySegmentSqrLength)
		{
			return;
		}

		//一番近づく位置（区間の始点から終点までの割合 t）
		//区間の端の時は、まだ近づいている途中か、もう離れていくところなので鳴らさない
		float t = Vector3.Dot(listener - from, segment) / lengthSqr;
		if (t <= SegmentStart || SegmentEnd <= t)
		{
			return;
		}

		Vector3 closest = from + segment * t;
		if (FlybyRadius * FlybyRadius < (closest - listener).sqrMagnitude)
		{
			return;
		}

		isFlybyPlayed = true;
		SoundManager.SingletonInstance.BulletFlybySEPool.Play(closest);
	}

	/// <summary>
	/// 区間内で一番手前にある当たり（撃ったエネミー自身・BulletEffect レイヤー・トリガーは除く）を探す
	/// </summary>
	bool FindHit(Vector3 from, Vector3 dir, float distance, out RaycastHit nearest)
	{
		nearest = new RaycastHit();//見つからなかった時の空の結果
		int count = Physics.RaycastNonAlloc(from, dir, hitBuffer, distance, HitLayerMask, QueryTriggerInteraction.Ignore);
		//バッファがいっぱいの時は一番手前の当たりが入っていない可能性があるので、全部取り直す（大きくフレームが飛んだ時など）
		RaycastHit[] hits = hitBuffer;
		if (HitBufferSize <= count)
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
			if (IsShooterCollider(hit.collider) == true || IsIgnoredCollider(hit.collider) == true)
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
	/// 弾の先端がコライダーの中に入っていないか調べる（撃ったエネミー自身と BulletEffect レイヤーは除く）
	/// 入っていれば、そのコライダーだけに手前からレイを撃って着弾点と法線を求める
	/// </summary>
	bool FindOverlapAtHead(out RaycastHit hit, out bool hasHitInfo, out Collider overlapCollider)
	{
		hit = new RaycastHit();//着弾点が求まらなかった時の空の結果
		hasHitInfo = false;
		overlapCollider = null;

		//弾の先端に置いた小さな球に重なっているコライダーを、用意しておいた配列 overlapBuffer に入れてもらう（戻り値はその数）
		int count = Physics.OverlapSphereNonAlloc(head, HeadOverlapRadius, overlapBuffer, HitLayerMask, QueryTriggerInteraction.Ignore);
		for (int i = 0; i < count; i++)
		{
			Collider collider = overlapBuffer[i];
			if (collider == null || IsShooterCollider(collider) == true || IsIgnoredCollider(collider) == true)
			{
				continue;
			}

			overlapCollider = collider;
			Ray ray = new Ray(head - direction * BacktrackDistance, direction);
			if (collider.Raycast(ray, out hit, BacktrackDistance + BacktrackMargin) == true)
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
	/// 弾が当たらないコライダーか？（弾道エフェクト用の BulletEffect レイヤー）
	/// </summary>
	public static bool IsIgnoredCollider(Collider collider)
	{
		return collider.gameObject.layer == LayerMask.NameToLayer(BulletEffectLayerName);
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
			PlayerManagerPresenter player = collider.GetComponentInParent<PlayerManagerPresenter>();
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

		bool visible = MinSqrLength < (head - tail).sqrMagnitude;
		lineRenderer.enabled = visible;
		if (visible == true)
		{
			lineRenderer.SetPosition(TracerTailIndex, tail);
			lineRenderer.SetPosition(TracerHeadIndex, head);
		}
		this.transform.position = head;
	}

	/// <summary>
	/// 弾を消す（プールがあればプールに返却、無ければ非表示にする）
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

		EnemyBulletPool returnPool = pool;
		pool = null;
		if (returnPool != null)
		{
			returnPool.ReleaseBullet(this);
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
