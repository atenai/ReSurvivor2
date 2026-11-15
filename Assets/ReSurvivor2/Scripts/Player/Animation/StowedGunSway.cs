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
	[Tooltip("揺れの速さ（1秒に揺れる回数）（歩く・走る足のリズム（1秒に2.5〜3回）と同じにすると、揺れがどんどん大きくなるので、それより速くする）")]
	[SerializeField] float frequency = 4.0f;
	[Tooltip("揺れの収まりやすさ（0で揺れ続け、1で揺り返さずに元の向きへ戻る）")]
	[SerializeField] float dampingRatio = 0.5f;
	[Tooltip("揺れの大きさの倍率（体の速さの変わり方から受ける力の倍率）")]
	[SerializeField] float swayScale = 1.0f;
	[Tooltip("揺れる角度の上限（度）")]
	[SerializeField] float maxAngle = 10.0f;

	[Tooltip("武器を切り替え始めてから、揺れを0にするまでの時間（秒）（切り替え終わった後も、同じ時間で揺れを戻す）")]
	const float SwitchFadeTime = 0.15f;
	[Tooltip("1回の計算で進める時間の上限（秒）（フレームの時間が長い時は、何回かに分けて計算する。揺れの計算が暴れないようにするため）")]
	const float MaxStepTime = 1.0f / 60.0f;
	[Tooltip("留め具がキャラクターから見てこの速さ（m/秒）より速く動いた時は、アニメーションが切り替わった時などの瞬間移動とみなして、揺れを止めて計算し直す")]
	const float TeleportSpeed = 20.0f;
	[Tooltip("キャラクターの移動・回る速さをならす時間（秒）（移動・回転は物理の更新ごとに速さが変わるので、ならしてから速さの変わり方を求める）")]
	const float CharacterVelocitySmoothTime = 0.1f;
	[Tooltip("キャラクターがこの速さ（ラジアン/秒）より速く回った時は、向きを一瞬で変えた時（移動の向きを急に変えた時・ステージを移った時など）とみなして、回る速さに入れない")]
	const float MaxCharacterAngularSpeed = 20.0f;
	[Tooltip("四元数の半分の角度から、回る角度への倍率")]
	const float HalfAngleToAngle = 2.0f;
	[Tooltip("四元数の成分の0（wが0より小さい四元数は、回る角度が180°より大きい方の表し方。軸の成分の長さが0の時は回っていない）")]
	const float ZeroQuaternionComponent = 0.0f;
	[Tooltip("ならす割合の最大（1で、ならさずに今の速さにする）")]
	const float FullSmoothRate = 1.0f;
	[Tooltip("体の速さの変わり方（加速度。m/秒²）の上限（人の体の動きとして自然な、重力の2倍くらいまでにする）"
		+ "（移動の向きを急に変えてアニメーションの形が一気に変わった時や、走って足が着いた時の急な動きで、銃が大きく揺れないようにする）")]
	const float MaxAcceleration = 20.0f;
	[Tooltip("揺らしていない時の重み")]
	const float NoSwayWeight = 0.0f;
	[Tooltip("揺らしている時の重み")]
	const float FullSwayWeight = 1.0f;
	[Tooltip("揺れていない時の角度")]
	const float NoAngle = 0.0f;
	[Tooltip("揺れが止まっている時の角速度")]
	const float NoAngularSpeed = 0.0f;
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
	[Tooltip("前のフレームの留め具の位置（キャラクターから見た位置）")]
	Vector3 previousPivotPosition = Vector3.zero;
	[Tooltip("前のフレームの留め具の速さ（キャラクターから見た速さ。m/秒）")]
	Vector3 previousPivotVelocity = Vector3.zero;
	[Tooltip("ならしたキャラクターの移動の速さ（ワールド。m/秒）")]
	Vector3 smoothedCharacterVelocity = Vector3.zero;
	[Tooltip("前の物理の更新の時のキャラクターの向き（ワールド）")]
	Quaternion previousCharacterRotation = Quaternion.identity;
	[Tooltip("前の物理の更新の時間（秒）")]
	float previousPhysicsTime = 0.0f;
	[Tooltip("前の物理の更新の時のキャラクターの向きが入っているか？")]
	bool hasPreviousCharacterRotation = false;
	[Tooltip("キャラクターの回る速さ（ワールドの回転軸の向き × 角速度。ラジアン/秒）（物理の更新がなかったフレームは、前の速さのまま）")]
	Vector3 characterAngularVelocity = Vector3.zero;
	[Tooltip("ならしたキャラクターの回る速さ（ワールドの回転軸の向き × 角速度。ラジアン/秒）")]
	Vector3 smoothedCharacterAngularVelocity = Vector3.zero;
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
	/// ※体の動きは、2つに分けて求める（キャラクターの位置・向きは物理の更新ごとにしか変わらないので、
	///   描画のフレームごとの位置の差から求めると、動かないフレームと2回分動くフレームが混ざり、銃が細かく震えるため）
	///   ・アニメーションによる体の動き（歩く時の上下の揺れなど）：キャラクターから見た留め具の位置の変わり方
	///   ・キャラクターの移動（走り出し・止まる時など）：キャラクターの移動の速さ（物理の速さ）をならしてから、その変わり方
	///   ・キャラクターの回転（構えてカメラを回した時など）：物理の更新ごとの向きの変わり方から回る速さを求め、ならしてから、
	///     回り始め・回り終わりに留め具が後ろへ残る力と、回っている間に外へ振られる力
	/// </summary>
	/// <param name="isSwayEnabled">揺らすか？（武器を切り替えている間はfalse）</param>
	/// <param name="deltaTime">前のフレームからの時間（秒）</param>
	/// <param name="physicsTime">最後の物理の更新の時間（秒）（キャラクターの向きは物理の更新ごとに変わるので、回る速さをこの時間の差で求める）</param>
	/// <param name="characterTransform">キャラクター（アニメーターのゲームオブジェクト。体の動きは、ここから見た位置で求める）</param>
	/// <param name="characterVelocity">キャラクターの移動の速さ（ワールド。m/秒）</param>
	public void UpdateSway(bool isSwayEnabled, float deltaTime, float physicsTime, Transform characterTransform, Vector3 characterVelocity)
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
		Vector3 pivotWorldPosition = parent.TransformPoint(GetPivotParentPosition());
		//アニメーションによる体の動き：キャラクターから見た留め具の位置
		Vector3 pivotPosition = characterTransform.InverseTransformPoint(pivotWorldPosition);
		//キャラクターの移動・回転：移動の速さ・回る速さをならす（物理の更新ごとに変わる速さを、少しずつ追いかける）
		float smoothRate = FullSmoothRate - Mathf.Exp(-deltaTime / CharacterVelocitySmoothTime);
		Vector3 previousSmoothedCharacterVelocity = smoothedCharacterVelocity;
		smoothedCharacterVelocity = Vector3.Lerp(smoothedCharacterVelocity, characterVelocity, smoothRate);
		UpdateCharacterAngularVelocity(characterTransform.rotation, physicsTime);
		Vector3 previousSmoothedCharacterAngularVelocity = smoothedCharacterAngularVelocity;
		smoothedCharacterAngularVelocity = Vector3.Lerp(smoothedCharacterAngularVelocity, characterAngularVelocity, smoothRate);
		if (hasPreviousPivot == false)
		{
			previousPivotPosition = pivotPosition;
			previousPivotVelocity = Vector3.zero;
			hasPreviousPivot = true;
			ApplySway();
			return;
		}

		Vector3 pivotVelocity = (pivotPosition - previousPivotPosition) / deltaTime;
		//アニメーションが切り替わった時などの瞬間移動では、揺れを止めて計算し直す
		if (TeleportSpeed < pivotVelocity.magnitude)
		{
			ResetSway();
			previousPivotPosition = pivotPosition;
			hasPreviousPivot = true;
			ApplySway();
			return;
		}

		//体の加速度 = アニメーションによる体の動きの加速度（キャラクターの向きでワールドへ） + キャラクターの移動の加速度 + キャラクターの回転による加速度
		Vector3 animationAcceleration = characterTransform.rotation * ((pivotVelocity - previousPivotVelocity) / deltaTime);
		Vector3 characterAcceleration = (smoothedCharacterVelocity - previousSmoothedCharacterVelocity) / deltaTime;
		//回転による加速度 = 回る速さの変わり方 × 回る中心からの位置（回り始め・回り終わり） + 回っている間に外へ振られる力（遠心力の向き）
		Vector3 characterAngularAcceleration = (smoothedCharacterAngularVelocity - previousSmoothedCharacterAngularVelocity) / deltaTime;
		Vector3 pivotFromCharacter = pivotWorldPosition - characterTransform.position;
		Vector3 rotationAcceleration = Vector3.Cross(characterAngularAcceleration, pivotFromCharacter)
			+ Vector3.Cross(smoothedCharacterAngularVelocity, Vector3.Cross(smoothedCharacterAngularVelocity, pivotFromCharacter));
		Vector3 pivotAcceleration = Vector3.ClampMagnitude(animationAcceleration + characterAcceleration + rotationAcceleration, MaxAcceleration);
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
			LimitSwayAngle();
			remainingTime -= stepTime;
		}

		ApplySway();
	}

	/// <summary>
	/// 揺れる角度を上限までにする（上限に着いた時は、外へ揺れる角速度をなくす）
	/// ※角度だけを止めて角速度を残すと、上限に張り付いて止まった後に急に戻り始めるので、
	///   銃を留めているベルトが伸び切って止まったように、外へ揺れる動きだけを止めて、すぐに戻り始めるようにする
	/// </summary>
	void LimitSwayAngle()
	{
		float maxAngleRadian = maxAngle * Mathf.Deg2Rad;
		float angle = swayAngle.magnitude;
		if (angle <= maxAngleRadian)
		{
			return;
		}

		Vector3 outwardDirection = swayAngle / angle;
		swayAngle = outwardDirection * maxAngleRadian;
		float outwardAngularSpeed = Vector3.Dot(swayAngularVelocity, outwardDirection);
		if (outwardAngularSpeed <= NoAngularSpeed)
		{
			return;
		}
		swayAngularVelocity -= outwardDirection * outwardAngularSpeed;
	}

	/// <summary>
	/// 揺れを止める（元の向きにする）
	/// </summary>
	void ResetSway()
	{
		swayAngle = Vector3.zero;
		swayAngularVelocity = Vector3.zero;
		previousPivotVelocity = Vector3.zero;
		smoothedCharacterVelocity = Vector3.zero;
		characterAngularVelocity = Vector3.zero;
		smoothedCharacterAngularVelocity = Vector3.zero;
		hasPreviousCharacterRotation = false;
		hasPreviousPivot = false;
	}

	/// <summary>
	/// キャラクターの回る速さを求める（物理の更新があった時だけ、前の物理の更新からの向きの変わり方を、その時間で割る）
	/// ※キャラクターの向きは物理の更新ごとにしか変わらないので、描画のフレームごとの向きの差から求めると、
	///   回らないフレームと2回分回るフレームが混ざり、銃が細かく震えるため
	/// </summary>
	/// <param name="characterRotation">今のキャラクターの向き（ワールド）</param>
	/// <param name="physicsTime">最後の物理の更新の時間（秒）</param>
	void UpdateCharacterAngularVelocity(Quaternion characterRotation, float physicsTime)
	{
		if (hasPreviousCharacterRotation == false)
		{
			previousCharacterRotation = characterRotation;
			previousPhysicsTime = physicsTime;
			characterAngularVelocity = Vector3.zero;
			hasPreviousCharacterRotation = true;
			return;
		}

		//物理の更新がなかったフレームは、前の回る速さのままにする
		float physicsDeltaTime = physicsTime - previousPhysicsTime;
		if (physicsDeltaTime <= NoDeltaTime)
		{
			return;
		}

		characterAngularVelocity = ToRotationVector(characterRotation * Quaternion.Inverse(previousCharacterRotation)) / physicsDeltaTime;
		//向きを一瞬で変えた時（移動の向きを急に変えた時・ステージを移った時など）は、回る速さに入れない
		if (MaxCharacterAngularSpeed < characterAngularVelocity.magnitude)
		{
			characterAngularVelocity = Vector3.zero;
		}
		previousCharacterRotation = characterRotation;
		previousPhysicsTime = physicsTime;
	}

	/// <summary>
	/// 回転を、回転軸の向き × 回る角度（ラジアン）にする（同じ回転を表す2つの四元数のうち、回る角度が小さい方で求める）
	/// </summary>
	/// <param name="rotation">回転</param>
	/// <returns>回転軸の向き × 回る角度（ラジアン）（回っていない時は0）</returns>
	static Vector3 ToRotationVector(Quaternion rotation)
	{
		Quaternion shortRotation = rotation.w < ZeroQuaternionComponent ? new Quaternion(-rotation.x, -rotation.y, -rotation.z, -rotation.w) : rotation;
		Vector3 axis = new Vector3(shortRotation.x, shortRotation.y, shortRotation.z);
		float sinHalfAngle = axis.magnitude;
		if (sinHalfAngle <= ZeroQuaternionComponent)
		{
			return Vector3.zero;
		}
		float angle = HalfAngleToAngle * Mathf.Atan2(sinHalfAngle, shortRotation.w);
		return axis / sinHalfAngle * angle;
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
