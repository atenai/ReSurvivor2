using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

/// <summary>
/// プレイヤーをアサルトライフルで射撃するタスク
/// </summary>
[TaskCategory("GroundEnemy")]
public class GroundEnemyAssaultRifleFireAction : Action
{
    GroundEnemy groundEnemy;

    [UnityEngine.Tooltip("射撃間隔")]
    float shootTime = 1.5f;
    [UnityEngine.Tooltip("射撃カウント")]
    float count = 0.0f;
    [UnityEngine.Tooltip("レイの長さ")]
    [SerializeField] float range = 100.0f;
    [UnityEngine.Tooltip("銃のダメージ")]
    [SerializeField] float Damage = 10.0f;

    [UnityEngine.Tooltip("アサルトライフルの散乱角度")]
    float assaultRifleRandomAngle = 7.0f;

    // Taskが処理される直前に呼ばれる
    public override void OnStart()
    {
        groundEnemy = this.GetComponent<GroundEnemy>();

        InitAnimation();
        InitMove();
    }

    /// <summary>
    /// アニメーションの初期化処理
    /// </summary>
    void InitAnimation()
    {
        groundEnemy.Animator.SetBool("b_rifleAim", true);
        groundEnemy.Animator.SetBool("b_rifleFire", false);
    }

    /// <summary>
    /// 移動の初期化
    /// </summary> 
    void InitMove()
    {
        groundEnemy.Rigidbody.velocity = Vector3.zero;
    }

    // Tick毎に呼ばれる
    public override TaskStatus OnUpdate()
    {
        if (groundEnemy.IsChase == true)
        {
            //移動実行中
            return TaskStatus.Running;
        }

        groundEnemy.Animator.SetBool("b_rifleFire", false);
        groundEnemy.Animator.SetBool("b_rifleAim", false);
        //目的地にたどりついた
        return TaskStatus.Success;
    }

    public override void OnFixedUpdate()
    {
        RotateToDirectionTarget();
        Shot();
    }

    /// <summary>
    /// ターゲットの方を向く
    /// </summary> 
    void RotateToDirectionTarget()
    {
        //対象オブジェクトの位置 – 自分のオブジェクトの位置 = 対象オブジェクトの向きベクトルが求められる
        Vector3 direction = groundEnemy.Target.transform.position - groundEnemy.transform.position;
        //単純に左右だけを見るようにしたいので、y軸の数値を0にする
        direction.y = 0;
        //第一引数に向きたい方向の向きベクトルを入れてあげる、それによってどのくらい回転させれば良いのか？の数値を求めることができる
        Quaternion lookRotation = Quaternion.LookRotation(direction, Vector3.up);
        //↑で求めたどのくらい回転させれば良いのか？の数値を元に回転させる
        groundEnemy.transform.rotation = Quaternion.Lerp(groundEnemy.transform.rotation, lookRotation, Time.deltaTime * 10f);
    }

    void Shot()
    {
        groundEnemy.Animator.SetBool("b_rifleFire", false);

        count = count + Time.deltaTime;
        if (shootTime < count)
        {
            count = 0.0f;

            groundEnemy.Animator.SetBool("b_rifleFire", true);
            AssaultRifleFire();

            groundEnemy.HandGunCurrentMagazine = groundEnemy.HandGunCurrentMagazine - 1;//現在のマガジンの弾数を-1する
        }
    }

    /// <summary>
    /// 弾を発射
    /// </summary> 
    void AssaultRifleFire()
    {
        Vector3 pos = new Vector3(this.transform.position.x, this.transform.position.y + 1f, this.transform.position.z);

        Vector3 direction = this.transform.forward;
        direction = Quaternion.AngleAxis(Random.Range(-assaultRifleRandomAngle, assaultRifleRandomAngle), this.transform.up) * direction;
        direction = Quaternion.AngleAxis(Random.Range(-assaultRifleRandomAngle, assaultRifleRandomAngle), this.transform.right) * direction;

        Ray ray = new Ray(pos, direction);
        Debug.DrawRay(ray.origin, ray.direction * range, Color.red, 10.0f);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, range) == true)//もしRayを投射して何らかのコライダーに衝突したら
        {
            if (hit.collider.gameObject.CompareTag("Player"))//※間違ってオブジェクトの設定にレイヤーとタグを間違えるなよおれｗ
            {
                //ダメージ
                Player player = hit.transform.GetComponent<Player>();
                if (player != null)
                {
                    player.TakeDamage(Damage);
                }
            }
        }
    }
}