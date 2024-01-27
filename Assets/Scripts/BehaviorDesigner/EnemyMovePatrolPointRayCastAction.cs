using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

/// <summary>
/// レイキャストを使い指定座標をパトロールするタスク
/// </summary>
[TaskCategory("Kashiwabara")]
public class EnemyMovePatrolPointRayCastAction : Action
{
    EnemyController enemyController;
    bool isActiveOnce = false;
    float rayDistance = 3.5f;
    private int pointNumber = 0;
    private Vector3 patrolPointPos;


    // Behavior Treeが有効になった時に呼ばれる
    public override void OnAwake()
    {
        //Debug.Log("<color=red>OnAwake</color>");
    }

    // Taskが処理される直前に呼ばれる
    public override void OnStart()
    {
        enemyController = this.GetComponent<EnemyRayCast>().EnemyController;

        if (isActiveOnce == false)
        {
            //Debug.Log("<color=red>OnStart</color>");
            NextPatrolPoint();
            isActiveOnce = true;
        }
    }

    // Tick毎に呼ばれる
    public override TaskStatus OnUpdate()
    {
        MovePatrolPoint();
        return TaskStatus.Success;
    }

    void NextPatrolPoint()
    {
        //巡回地点が設定されていなければ
        if (enemyController.PatrolPoints.Length == 0)
        {
            //切り上げる
            return;
        }
        //現在選択されている配列の座標を巡回地点の座標に代入
        patrolPointPos = enemyController.PatrolPoints[pointNumber].transform.position;
        //配列の中から次の巡回地点を選択（必要に応じて繰り返し）
        pointNumber = (pointNumber + 1) % enemyController.PatrolPoints.Length;
    }

    void Eyesight()
    {
        Ray ray = new Ray(this.transform.position, this.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, rayDistance))
        {
            Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.green, 1);
            if (hit.collider.tag == "Object")
            {
                enemyController.Rigidbody.AddForce(new Vector3(0, enemyController.JumpForce, 0));
            }
        }
    }

    void MovePatrolPoint()
    {
        Eyesight();

        //対象オブジェクトの位置 – 自分のオブジェクトの位置 = 対象オブジェクトの向きベクトルが求められる
        var direction = patrolPointPos - this.transform.position;
        //単純に左右だけを見るようにしたいので、y軸の数値を0にする
        direction.y = 0;

        //第一引数に向きたい方向の向きベクトルを入れてあげる、それによってどのくらい回転させれば良いのか？の数値を求めることができる
        var lookRotation = Quaternion.LookRotation(direction, Vector3.up);
        //↑で求めたどのくらい回転させれば良いのか？の数値を元に回転させる
        this.transform.rotation = Quaternion.Lerp(this.transform.rotation, lookRotation, Time.deltaTime * enemyController.RotationSpeed * 10f);

        this.transform.Translate(Vector3.forward * enemyController.MoveSpeed * Time.deltaTime);

        float distanceToTarget = Vector3.SqrMagnitude(this.transform.position - patrolPointPos);

        if (distanceToTarget < 0.5f)
        {
            NextPatrolPoint();
        }
    }
}