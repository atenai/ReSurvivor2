using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

/// <summary>
/// フライングエネミーがプレイヤーを直線追尾するタスク
/// </summary>
[TaskCategory("FlyingEnemy")]
public class FlyingEnemyStraightLineTrackingAction : Action
{
    FlyingEnemy flyingEnemy;

    [UnityEngine.Tooltip("加速度")]
    [SerializeField] float acceleration = 0.001f;
    [UnityEngine.Tooltip("エネミーが止まってほしい座標位置の範囲")]
    [SerializeField] float stopPos = 0.1f;

    //ターゲット座標位置の変数系
    Vector3 endPos;

    //Xの移動パターン
    enum XMoveType
    {
        Plus,
        Zero,
        Minus,
    }
    XMoveType xMoveType = XMoveType.Zero;

    //Yの移動パターン
    enum YMoveType
    {
        Plus,
        Zero,
        Minus,
    }
    YMoveType yMoveType = YMoveType.Zero;

#if UNITY_EDITOR
    [SerializeField] GameObject obj;//プレハブをGameObject型で取得（デバッグ用）
#endif

    //回転処理の変数系
    float t = 0.0f;
    Quaternion lookRotation;
    private static float oldDirectionX = 1.0f;

    //移動処理の変数系
    Vector3 velocity;//現在速度
    float spareGoalDistance = 0.0f;//ゴールの座標をすり抜けた際の移動を止める為の予備距離
    float halfDistance = 0.0f;//当たった距離の半分

    [SerializeField] float MaxLimitPosX = 20.0f;
    [SerializeField] float MinLimitPosX = -20.0f;
    [SerializeField] float MaxLimitPosY = 10.0f;
    [SerializeField] float MinLimitPosY = 1.0f;
    bool isMoveEnd = false;
    bool isRotEnd = false;

    // Taskが処理される直前に呼ばれる
    public override void OnStart()
    {
        flyingEnemy = this.GetComponent<FlyingEnemy>();
        //Debug.Log("<color=blue>ランダム移動のOnStart</color>");
        velocity = Vector3.zero;
        flyingEnemy.Rigidbody.velocity = velocity;
        TargetPos();
        InitRotateToDirectionTarget();
        InitMove(endPos);
        isRotEnd = false;
        isMoveEnd = false;
    }

    void TargetPos()
    {
        float xPos = flyingEnemy.Target.transform.position.x;
        //Debug.Log("<color=red>xPos : " + xPos + "</color>");
        float yPos = flyingEnemy.Target.transform.position.y;
        //Debug.Log("<color=blue>yPos : " + yPos + "</color>");
        float zPos = 0.0f;
        //Debug.Log("<color=yellow>zPos : " + zPos + "</color>");
        endPos = new Vector3(xPos, yPos, zPos);

        //↓ここはステージ外の座標位置に移動しないようにするための上限範囲の設定処理
        if (endPos.x < MinLimitPosX)
        {
            endPos.x = MinLimitPosX;
        }
        else if (MaxLimitPosX < endPos.x)
        {
            endPos.x = MaxLimitPosX;
        }

        if (endPos.y < MinLimitPosY)
        {
            endPos.y = MinLimitPosY;
        }
        else if (MaxLimitPosY < endPos.y)
        {
            endPos.y = MaxLimitPosY;
        }
        //↑ここはステージ外の座標位置に移動しないようにするための上限範囲の設定処理

        //Debug.Log("<color=orange>endPos : " + endPos + "</color>");
#if UNITY_EDITOR
        UnityEngine.Object.Instantiate(obj, endPos, Quaternion.identity);//プレハブを元に、インスタンスを生成（デバッグ用）
#endif
    }

    void InitRotateToDirectionTarget()
    {
        t = 0.0f;
        //対象オブジェクトの位置 – 自分のオブジェクトの位置 = 対象オブジェクトの向きベクトルが求められる
        Vector3 direction = endPos - this.transform.position;
        //Debug.Log("<color=green>direction : " + direction + "</color>");
        if (direction.x == 0.0f)
        {
            direction.x = oldDirectionX;
        }
        else
        {
            oldDirectionX = direction.x;
        }
        //単純に左右だけを見るようにしたいので、y軸の数値を0にする
        direction.y = 0;
        //2点間の角度を求める
        //第一引数に向きたい方向の向きベクトルを入れてあげる、それによってどのくらい回転させれば良いのか？の数値を求めることができる
        lookRotation = Quaternion.LookRotation(direction, Vector3.up);
    }

    void InitMove(Vector3 pos)
    {
        float hitPosDistance = Mathf.Abs(Vector3.Distance(this.transform.position, pos));
        //Debug.Log("<color=green>hitPosDistance : " + hitPosDistance + "</color>");
        const float spareNumber = 1.25f;
        spareGoalDistance = Mathf.Abs(hitPosDistance * spareNumber);//ゴールの座標をすり抜けた際の移動を止める為の予備距離
        const float halfNumber = 2.0f;
        halfDistance = Mathf.Abs(hitPosDistance / halfNumber);//当たった距離の半分を取得し保持

        if (this.transform.position.x < pos.x)
        {
            xMoveType = XMoveType.Plus;
        }
        else if (pos.x < this.transform.position.x)
        {
            xMoveType = XMoveType.Minus;
        }
        else if (this.transform.position.x == pos.x)
        {
            xMoveType = XMoveType.Zero;
        }

        if (this.transform.position.y < pos.y)
        {
            yMoveType = YMoveType.Plus;
        }
        else if (pos.y < this.transform.position.y)
        {
            yMoveType = YMoveType.Minus;
        }
        else if (this.transform.position.y == pos.y)
        {
            yMoveType = YMoveType.Zero;
        }

#if UNITY_EDITOR
        //DebugStart();
#endif
    }

    void DebugStart()
    {
        //右に移動
        if (xMoveType == XMoveType.Plus && yMoveType == YMoveType.Zero)
        {
            Debug.Log("<color=red>右に加速</color>");
        }

        //左に移動
        if (xMoveType == XMoveType.Minus && yMoveType == YMoveType.Zero)
        {
            Debug.Log("<color=orange>左に加速</color>");
        }

        //上に移動
        if (xMoveType == XMoveType.Zero && yMoveType == YMoveType.Plus)
        {
            Debug.Log("<color=red>上に加速</color>");
        }

        //下に移動
        if (xMoveType == XMoveType.Zero && yMoveType == YMoveType.Minus)
        {
            Debug.Log("<color=orange>下に加速</color>");
        }

        //右上に移動
        if (xMoveType == XMoveType.Plus && yMoveType == YMoveType.Plus)
        {
            Debug.Log("<color=red>右上に加速</color>");
        }

        //左上に移動
        if (xMoveType == XMoveType.Minus && yMoveType == YMoveType.Plus)
        {
            Debug.Log("<color=orange>左上に加速</color>");
        }

        //右下に移動
        if (xMoveType == XMoveType.Plus && yMoveType == YMoveType.Minus)
        {
            Debug.Log("<color=red>右下に加速</color>");
        }

        //左下に移動
        if (xMoveType == XMoveType.Minus && yMoveType == YMoveType.Minus)
        {
            Debug.Log("<color=orange>左下に加速</color>");
        }
    }

    // 更新時に呼ばれる
    public override TaskStatus OnUpdate()
    {
        //Debug.Log("<color=orange>isRotEnd : " + isRotEnd + "</color>");
        if (isRotEnd == false)
        {
            //Debug.Log("<color=orange>回転中</color>");
            RotateToDirectionTarget();
        }
        else if (isRotEnd == true)
        {
            //回転し終えた
            RandomMove(endPos);
            if (isMoveEnd == true)
            {
                //目的地にたどりついた
                return TaskStatus.Success;
            }
            else
            {
                //移動実行中
                return TaskStatus.Running;
            }
        }

        //回転実行中
        return TaskStatus.Running;
    }

    void RotateToDirectionTarget()
    {
        //Time.deltaTimeは0.1f
        t = t + Time.deltaTime;
        //Debug.Log("<color=green>t :" + t + "</color>");
        //↑で求めたどのくらい回転させれば良いのか？の数値を元に回転させる(Lerpのtは0～1)
        this.transform.rotation = Quaternion.Lerp(this.transform.rotation, lookRotation, t);

        const float endRot = 1.0f;
        if (endRot <= t)
        {
            //Debug.Log("<color=red>回転終了</color>");
            isRotEnd = true;
        }
    }

    void RandomMove(Vector3 pos)
    {
        float currentDistance = Mathf.Abs(Vector3.Distance(this.transform.position, pos));
        //Debug.Log("<color=red>currentDistance : " + currentDistance + "</color>");

        if (currentDistance <= stopPos)
        {
            EndProcess();
        }

        if (spareGoalDistance <= currentDistance)
        {
            EndProcess();
        }

        //向きベクトル
        Vector3 moveDirection = pos - this.transform.position;
#if UNITY_EDITOR
        Ray ray = new Ray(this.transform.position, moveDirection);
        Debug.DrawRay(ray.origin, ray.direction, Color.red);
#endif
        //Normalize()関数を使用すると2つのベクトルの長さを掛け合わせた正しい位置に自動で修正してくれる関数
        moveDirection.Normalize();

        //右に移動
        if (xMoveType == XMoveType.Plus && yMoveType == YMoveType.Zero)
        {
            if (halfDistance < currentDistance)//初期距離の半分より現在の距離が大きい場合は前進する
            {
                //Debug.Log("<color=red>右に加速</color>");

                //正規化したベクトルに加速度をかける
                Vector3 moveDirectionAcceleration = moveDirection * acceleration;
                velocity = velocity + moveDirectionAcceleration;

                flyingEnemy.Rigidbody.velocity = velocity;
            }
            else if (currentDistance <= halfDistance)//初期距離の半分より現在の距離が小さい場合は減速する
            {
                //Debug.Log("<color=blue>減速</color>");

                //正規化したベクトルに加速度をかける
                Vector3 moveDirectionAcceleration = moveDirection * -acceleration;
                velocity = velocity + moveDirectionAcceleration;

                //減速する際に速度がマイナスになって後進してほしくない為
                if (0 < velocity.x)
                {
                    flyingEnemy.Rigidbody.velocity = velocity;
                }
                else
                {
                    EndProcess();
                }
            }
        }

        //左に移動
        if (xMoveType == XMoveType.Minus && yMoveType == YMoveType.Zero)
        {
            if (halfDistance < currentDistance)//初期距離の半分より現在の距離が大きい場合は前進する
            {
                //Debug.Log("<color=orange>左に加速</color>");

                //正規化したベクトルに加速度をかける
                Vector3 moveDirectionAcceleration = moveDirection * acceleration;
                velocity = velocity + moveDirectionAcceleration;

                flyingEnemy.Rigidbody.velocity = velocity;
            }
            else if (currentDistance <= halfDistance)//初期距離の半分より現在の距離が小さい場合は減速する
            {
                //Debug.Log("<color=blue>減速</color>");

                //正規化したベクトルに加速度をかける
                Vector3 moveDirectionAcceleration = moveDirection * -acceleration;
                velocity = velocity + moveDirectionAcceleration;

                //減速する際に速度がマイナスになって後進してほしくない為
                if (velocity.x < 0)
                {
                    flyingEnemy.Rigidbody.velocity = velocity;
                }
                else
                {
                    EndProcess();
                }
            }
        }

        //上に移動
        if (xMoveType == XMoveType.Zero && yMoveType == YMoveType.Plus)
        {
            if (halfDistance < currentDistance)//初期距離の半分より現在の距離が大きい場合は前進する
            {
                //Debug.Log("<color=red>上に加速</color>");

                //正規化したベクトルに加速度をかける
                Vector3 moveDirectionAcceleration = moveDirection * acceleration;
                velocity = velocity + moveDirectionAcceleration;

                flyingEnemy.Rigidbody.velocity = velocity;
            }
            else if (currentDistance <= halfDistance)//初期距離の半分より現在の距離が小さい場合は減速する
            {
                //Debug.Log("<color=blue>減速</color>");

                //正規化したベクトルに加速度をかける
                Vector3 moveDirectionAcceleration = moveDirection * -acceleration;
                velocity = velocity + moveDirectionAcceleration;

                //減速する際に速度がマイナスになって後進してほしくない為
                if (0 < velocity.y)
                {
                    flyingEnemy.Rigidbody.velocity = velocity;
                }
                else
                {
                    EndProcess();
                }
            }
        }

        //下に移動
        if (xMoveType == XMoveType.Zero && yMoveType == YMoveType.Minus)
        {
            if (halfDistance < currentDistance)//初期距離の半分より現在の距離が大きい場合は前進する
            {
                //Debug.Log("<color=orange>下加速</color>");

                //正規化したベクトルに加速度をかける
                Vector3 moveDirectionAcceleration = moveDirection * acceleration;
                velocity = velocity + moveDirectionAcceleration;

                flyingEnemy.Rigidbody.velocity = velocity;
            }
            else if (currentDistance <= halfDistance)//初期距離の半分より現在の距離が小さい場合は減速する
            {
                //Debug.Log("<color=blue>減速</color>");

                //正規化したベクトルに加速度をかける
                Vector3 moveDirectionAcceleration = moveDirection * -acceleration;
                velocity = velocity + moveDirectionAcceleration;

                //減速する際に速度がマイナスになって後進してほしくない為
                if (velocity.y < 0)
                {
                    flyingEnemy.Rigidbody.velocity = velocity;
                }
                else
                {
                    EndProcess();
                }
            }
        }

        //右上に移動
        if (xMoveType == XMoveType.Plus && yMoveType == YMoveType.Plus)
        {
            if (halfDistance < currentDistance)//初期距離の半分より現在の距離が大きい場合は前進する
            {
                //Debug.Log("<color=red>右上に加速</color>");

                //正規化したベクトルに加速度をかける
                Vector3 moveDirectionAcceleration = moveDirection * acceleration;
                velocity = velocity + moveDirectionAcceleration;

                flyingEnemy.Rigidbody.velocity = velocity;
            }
            else if (currentDistance <= halfDistance)//初期距離の半分より現在の距離が小さい場合は減速する
            {
                //Debug.Log("<color=blue>減速</color>");

                //正規化したベクトルに加速度をかける
                Vector3 moveDirectionAcceleration = moveDirection * -acceleration;
                velocity = velocity + moveDirectionAcceleration;

                //減速する際に速度がマイナスになって後進してほしくない為
                if (0 < velocity.x && 0 < velocity.y)
                {
                    flyingEnemy.Rigidbody.velocity = velocity;
                }
                else
                {
                    EndProcess();
                }
            }
        }

        //左上に移動
        if (xMoveType == XMoveType.Minus && yMoveType == YMoveType.Plus)
        {
            if (halfDistance < currentDistance)//初期距離の半分より現在の距離が大きい場合は前進する
            {
                //Debug.Log("<color=orange>左上に加速</color>");

                //正規化したベクトルに加速度をかける
                Vector3 moveDirectionAcceleration = moveDirection * acceleration;
                velocity = velocity + moveDirectionAcceleration;

                flyingEnemy.Rigidbody.velocity = velocity;
            }
            else if (currentDistance <= halfDistance)//初期距離の半分より現在の距離が小さい場合は減速する
            {
                //Debug.Log("<color=blue>減速</color>");

                //正規化したベクトルに加速度をかける
                Vector3 moveDirectionAcceleration = moveDirection * -acceleration;
                velocity = velocity + moveDirectionAcceleration;

                //減速する際に速度がマイナスになって後進してほしくない為
                if (velocity.x < 0 && 0 < velocity.y)//キャラクターの向きが左を向いているとflyingEnemy.Rigidbody.transform.forwardはマイナスになる
                {
                    flyingEnemy.Rigidbody.velocity = velocity;
                }
                else
                {
                    EndProcess();
                }
            }
        }

        //右下に移動
        if (xMoveType == XMoveType.Plus && yMoveType == YMoveType.Minus)
        {
            if (halfDistance < currentDistance)//初期距離の半分より現在の距離が大きい場合は前進する
            {
                //Debug.Log("<color=red>右下に加速</color>");

                //正規化したベクトルに加速度をかける
                Vector3 moveDirectionAcceleration = moveDirection * acceleration;
                velocity = velocity + moveDirectionAcceleration;

                flyingEnemy.Rigidbody.velocity = velocity;
            }
            else if (currentDistance <= halfDistance)//初期距離の半分より現在の距離が小さい場合は減速する
            {
                //Debug.Log("<color=blue>減速</color>");

                //正規化したベクトルに加速度をかける
                Vector3 moveDirectionAcceleration = moveDirection * -acceleration;
                velocity = velocity + moveDirectionAcceleration;

                //減速する際に速度がマイナスになって後進してほしくない為
                if (0 < velocity.x && velocity.y < 0)
                {
                    flyingEnemy.Rigidbody.velocity = velocity;
                }
                else
                {
                    EndProcess();
                }
            }
        }

        //左下に移動
        if (xMoveType == XMoveType.Minus && yMoveType == YMoveType.Minus)
        {
            if (halfDistance < currentDistance)//初期距離の半分より現在の距離が大きい場合は前進する
            {
                //Debug.Log("<color=orange>左上に加速</color>");

                //正規化したベクトルに加速度をかける
                Vector3 moveDirectionAcceleration = moveDirection * acceleration;
                velocity = velocity + moveDirectionAcceleration;

                flyingEnemy.Rigidbody.velocity = velocity;
            }
            else if (currentDistance <= halfDistance)//初期距離の半分より現在の距離が小さい場合は減速する
            {
                //Debug.Log("<color=blue>減速</color>");

                //正規化したベクトルに加速度をかける
                Vector3 moveDirectionAcceleration = moveDirection * -acceleration;
                velocity = velocity + moveDirectionAcceleration;

                //減速する際に速度がマイナスになって後進してほしくない為
                if (velocity.x < 0 && velocity.y < 0)
                {
                    flyingEnemy.Rigidbody.velocity = velocity;
                }
                else
                {
                    EndProcess();
                }
            }
        }

        //原点
        if (xMoveType == XMoveType.Zero && yMoveType == YMoveType.Zero)
        {
            EndProcess();
        }
    }

    void EndProcess()
    {
        //Debug.Log("<color=cyan>移動の終了</color>");
        velocity = Vector3.zero;
        flyingEnemy.Rigidbody.velocity = velocity;
        isMoveEnd = true;
    }
}