using UnityEngine;

/// <summary>
/// 体にしまっている銃（腰のアサルトライフル・背中のショットガン）を、移動した時に少しだけ揺らす
/// ※しまっている銃は、どんなアニメーションでも体に対して動かないように骨に付けてあるので、その位置・向きを中心に揺らす
///   体（銃を付けている骨）が動いて速さが変わると、銃の重心に慣性の力がかかり、銃が留め具（支点）を中心に少し遅れて揺れて、ばねのように元の向きへ戻る
/// ※武器を切り替えている間は揺らさない
///   （しまい終わった時・取り出し始めた時に、手の銃としまってある銃がぴったり重なるように、切り替えのアニメーションクリップを作ってあるため）
/// ※GunAnimationFacadeが銃ごとに持ち、アニメーションの後（LateUpdate）に動かす
/// </summary>
[System.Serializable]
public class StowedGunSway
{
	[Tooltip("留め具の位置（しまっている銃から見た位置。ここを中心に揺れる）")]
	[SerializeField] Vector3 pivotLocalPosition = Vector3.zero;
	[Tooltip("銃の重心の位置（しまっている銃から見た位置。体の速さが変わると、この位置に慣性の力がかかる）")]
	[SerializeField] Vector3 centerOfMassLocalPosition = Vector3.zero;
	[Tooltip("揺れの速さ（1秒に揺れる回数）")]
	[SerializeField] float frequency = 2.5f;
	[Tooltip("揺れの収まりやすさ（0で揺れ続け、1で揺り返さずに元の向きへ戻る）")]
	[SerializeField] float dampingRatio = 0.35f;
	[Tooltip("揺れの大きさの倍率（体の速さの変わり方から受ける力の倍率）")]
	[SerializeField] float swayScale = 1.0f;
	[Tooltip("揺れる角度の上限（度）")]
	[SerializeField] float maxAngle = 6.0f;

	[Tooltip("武器を切り替え始めてから、揺れを0にするまでの時間（秒）（切り替え終わった後も、同じ時間で揺れを戻す）")]
	const float SwitchFadeTime = 0.15f;
	[Tooltip("1回の計算で進める時間の上限（秒）（フレームの時間が長い時は、何回かに分けて計算する。揺れの計算が暴れないようにするため）")]
	const float MaxStepTime = 1.0f / 60.0f;
	[Tooltip("留め具がこの速さ（m/秒）より速く動いた時は、ステージを移った時などの瞬間移動とみなして、揺れを止めて計算し直す")]
	const float TeleportSpeed = 20.0f;
	[Tooltip("体の速さの変わり方（加速度。m/秒²）の上限（ぶつかった時などの急な動きで、銃が大きく揺れないようにする）")]
	const float MaxAcceleration = 40.0f;
	[Tooltip("揺らしていない時の重み")]
	const float NoSwayWeight = 0.0f;
	[Tooltip("揺らしている時の重み")]
	const float FullSwayWeight = 1.0f;
	[Tooltip("揺れていない時の角度")]
	const float NoAngle = 0.0f;
	[Tooltip("フレームの時間が0の時（一時停止中）は計算しない")]
	const float NoDeltaTime = 0.0f;
	[Tooltip("ばねの戻る力に使う、揺れの速さ（1秒に揺れる回数）から角速度への倍率（2π）")]
	const float TwoPi = 2.0f * Mathf.PI;
	[Tooltip("減衰の力に使う倍率（減衰の力 = 2 × 収まりやすさ × 角速度 × 揺れの角速度）")]
	const float DampingFactor = 2.0f;

	[Tooltip("しまっている銃（初期化される前はnull）")]
	Transform stowTransform;
	[Tooltip("しまっている銃の、揺れていない時の位置（付けている骨から見た位置）")]
	Vector3 restLocalPosition = Vector3.zero;
	[Tooltip("しまっている銃の、揺れていない時の向き（付けている骨から見た向き）")]
	Quaternion restLocalRotation = Quaternion.identity;
	[Tooltip("前のフレームの留め具の位置（ワールド）")]
	Vector3 previousPivotPosition = Vector3.zero;
	[Tooltip("前のフレームの留め具の速さ（ワールド。m/秒）")]
	Vector3 previousPivotVelocity = Vector3.zero;
	[Tooltip("前のフレームの留め具の位置が入っているか？（初めのフレーム・銃を表示した最初のフレームは、速さが分からないので揺らさない）")]
	bool hasPreviousPivot = false;
	[Tooltip("今の揺れの角度（付けている骨から見た回転軸の向き × 角度。ラジアン）")]
	Vector3 swayAngle = Vector3.zero;
	[Tooltip("今の揺れの角速度（付けている骨から見た回転軸の向き × 角速度。ラジアン/秒）")]
	Vector3 swayAngularVelocity = Vector3.zero;
	[Tooltip("揺れを効かせる割合（武器を切り替えている間は0へ、切り替えていない時は1へ、少しずつ変える）")]
	float swayWeight = FullSwayWeight;

	/// <summary>
	/// コンストラクタ（銃ごとの、留め具と重心の位置を決める）
	/// </summary>
	/// <param name="pivotLocalPosition">留め具の位置（しまっている銃から見た位置）</param>
	/// <param name="centerOfMassLocalPosition">銃の重心の位置（しまっている銃から見た位置）</param>
	public StowedGunSway(Vector3 pivotLocalPosition, Vector3 centerOfMassLocalPosition)
	{
		this.pivotLocalPosition = pivotLocalPosition;
		this.centerOfMassLocalPosition = centerOfMassLocalPosition;
	}

	/// <summary>
	/// 初期化（しまっている銃の、揺れていない時の位置・向きを覚えておく）
	/// </summary>
	/// <param name="stowTransform">しまっている銃</param>
	public void Initialize(Transform stowTransform)
	{
		this.stowTransform = stowTransform;
		restLocalPosition = stowTransform.localPosition;
		restLocalRotation = stowTransform.localRotation;
		ResetSway();
	}

	/// <summary>
	/// しまっている銃を揺らす（アニメーションで体が動いた後に、毎フレーム呼ぶ）
	/// </summary>
	/// <param name="isSwayEnabled">揺らすか？（武器を切り替えている間はfalse）</param>
	/// <param name="deltaTime">前のフレームからの時間（秒）</param>
	public void UpdateSway(bool isSwayEnabled, float deltaTime)
	{
		if (stowTransform == null || deltaTime <= NoDeltaTime)
		{
			return;
		}

		//しまっている銃を表示していない時（その銃を手に持っている時）は、揺れを止めておく（表示した時に、元の向きから揺れ始める）
		if (stowTransform.gameObject.activeInHierarchy == false)
		{
			ResetSway();
			ApplySway();
			return;
		}

		float targetWeight = isSwayEnabled == true ? FullSwayWeight : NoSwayWeight;
		swayWeight = Mathf.MoveTowards(swayWeight, targetWeight, deltaTime / SwitchFadeTime);

		Transform parent = stowTransform.parent;
		Vector3 pivotParentPosition = GetPivotParentPosition();
		Vector3 pivotPosition = parent.TransformPoint(pivotParentPosition);
		if (hasPreviousPivot == false)
		{
			previousPivotPosition = pivotPosition;
			previousPivotVelocity = Vector3.zero;
			hasPreviousPivot = true;
			ApplySway();
			return;
		}

		Vector3 pivotVelocity = (pivotPosition - previousPivotPosition) / deltaTime;
		//ステージを移った時などの瞬間移動では、揺れを止めて計算し直す
		if (TeleportSpeed < pivotVelocity.magnitude)
		{
			ResetSway();
			previousPivotPosition = pivotPosition;
			hasPreviousPivot = true;
			ApplySway();
			return;
		}

		Vector3 pivotAcceleration = Vector3.ClampMagnitude((pivotVelocity - previousPivotVelocity) / deltaTime, MaxAcceleration);
		previousPivotPosition = pivotPosition;
		previousPivotVelocity = pivotVelocity;

		//付けている骨から見た加速度で、重心にかかる慣性の力から、留め具を中心に回ろうとする力（角加速度）を求める
		Vector3 parentAcceleration = Quaternion.Inverse(parent.rotation) * pivotAcceleration;
		Vector3 arm = restLocalRotation * (centerOfMassLocalPosition - pivotLocalPosition);
		Vector3 inertiaAngularAcceleration = Vector3.Cross(arm, -parentAcceleration) / arm.sqrMagnitude * swayScale;

		//ばねで元の向きへ戻しながら、減衰で揺れを収める（フレームの時間が長い時は、何回かに分けて計算する）
		float angularFrequency = TwoPi * frequency;
		float remainingTime = deltaTime;
		while (NoDeltaTime < remainingTime)
		{
			float stepTime = Mathf.Min(remainingTime, MaxStepTime);
			Vector3 angularAcceleration = inertiaAngularAcceleration - angularFrequency * angularFrequency * swayAngle - DampingFactor * dampingRatio * angularFrequency * swayAngularVelocity;
			swayAngularVelocity += angularAcceleration * stepTime;
			swayAngle += swayAngularVelocity * stepTime;
			remainingTime -= stepTime;
		}
		swayAngle = Vector3.ClampMagnitude(swayAngle, maxAngle * Mathf.Deg2Rad);

		ApplySway();
	}

	/// <summary>
	/// 揺れを止める（元の向きにする）
	/// </summary>
	void ResetSway()
	{
		swayAngle = Vector3.zero;
		swayAngularVelocity = Vector3.zero;
		previousPivotVelocity = Vector3.zero;
		hasPreviousPivot = false;
	}

	/// <summary>
	/// 付けている骨から見た、留め具の位置を求める
	/// </summary>
	/// <returns>留め具の位置（付けている骨から見た位置）</returns>
	Vector3 GetPivotParentPosition()
	{
		return restLocalPosition + restLocalRotation * pivotLocalPosition;
	}

	/// <summary>
	/// 今の揺れの角度で、しまっている銃を留め具を中心に回す
	/// </summary>
	void ApplySway()
	{
		Vector3 appliedAngle = swayAngle * swayWeight;
		Quaternion swayRotation = Quaternion.identity;
		if (NoAngle < appliedAngle.magnitude)
		{
			swayRotation = Quaternion.AngleAxis(appliedAngle.magnitude * Mathf.Rad2Deg, appliedAngle.normalized);
		}
		Vector3 pivotParentPosition = GetPivotParentPosition();
		stowTransform.localRotation = swayRotation * restLocalRotation;
		stowTransform.localPosition = pivotParentPosition + swayRotation * (restLocalPosition - pivotParentPosition);
	}
}
