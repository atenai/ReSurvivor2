using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// フライングエネミーが揺らぎ待機をするタスク
/// </summary>
[TaskCategory("FlyingEnemy")]
public class FlyingEnemySwayWaitAction : Action
{
    FlyingEnemyController flyingEnemyController;

    [UnityEngine.Tooltip("加速度")]
    [SerializeField] float acceleration = 0.001f;
    [UnityEngine.Tooltip("エネミーが止まってほしい座標位置の範囲")]
    [SerializeField] float stopPos = 0.1f;
    [UnityEngine.Tooltip("エネミーがx軸で移動するランダム座標位置の範囲")]
    [SerializeField] float xMoveRange = 0.25f;
    [UnityEngine.Tooltip("エネミーがy軸で移動するランダム座標位置の範囲")]
    [SerializeField] float yMoveRange = 0.25f;

    //ターゲット座標位置の変数系
    Vector3 targetPos;
    Vector3 originPos;//原点の座標位置
    bool isOriginMove = false;
    bool isEnd = false;

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
    [SerializeField] GameObject originPosObj;//プレハブをGameObject型で取得（デバッグ用）
    [SerializeField] GameObject targetPosObj;//プレハブをGameObject型で取得（デバッグ用）
#endif

    //移動処理の変数系
    Vector3 velocity;//現在速度
    float spareGoalDistance = 0.0f;//ゴールの座標をすり抜けた際の移動を止める為の予備距離
    float halfDistance = 0.0f;//当たった距離の半分

    // Taskが処理される直前に呼ばれる
    public override void OnStart()
    {
        flyingEnemyController = this.GetComponent<FlyingEnemy>().FlyingEnemyController;
        //Debug.Log("<color=blue>OnStart</color>");
        velocity = Vector3.zero;
        flyingEnemyController.Rigidbody.velocity = velocity;
        TargetPos();
        InitMove(targetPos);
        isOriginMove = false;
        isEnd = false;
    }

    public void TargetPos()
    {
        originPos = this.transform.position;
        //Debug.Log("<color=red>originPos : " + originPos + "</color>");
#if UNITY_EDITOR
        //UnityEngine.Object.Instantiate(originPosObj, originPos, Quaternion.identity);//プレハブを元に、インスタンスを生成（デバッグ用）
#endif

        float xPos = UnityEngine.Random.Range(-xMoveRange, xMoveRange);
        //Debug.Log("<color=red>xPos : " + xPos + "</color>");
        float yPos = UnityEngine.Random.Range(-yMoveRange, yMoveRange);
        //Debug.Log("<color=blue>yPos : " + yPos + "</color>");
        int zPos = 0;
        //Debug.Log("<color=yellow>zPos : " + zPos + "</color>");
        targetPos = new Vector3(this.transform.position.x + xPos, this.transform.position.y + yPos, this.transform.position.z + zPos);
        //Debug.Log("<color=orange>endPos : " + endPos + "</color>");
#if UNITY_EDITOR
        //UnityEngine.Object.Instantiate(targetPosObj, targetPos, Quaternion.identity);//プレハブを元に、インスタンスを生成（デバッグ用）
#endif
    }

    public void InitMove(Vector3 pos)
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
            Debug.Log("<color=red>左に加速</color>");
        }

        //上に移動
        if (xMoveType == XMoveType.Zero && yMoveType == YMoveType.Plus)
        {
            Debug.Log("<color=red>上に加速</color>");
        }

        //下に移動
        if (xMoveType == XMoveType.Zero && yMoveType == YMoveType.Minus)
        {
            Debug.Log("<color=red>下に加速</color>");
        }

        //右上に移動
        if (xMoveType == XMoveType.Plus && yMoveType == YMoveType.Plus)
        {
            Debug.Log("<color=green>右上に加速</color>");
        }

        //左上に移動
        if (xMoveType == XMoveType.Minus && yMoveType == YMoveType.Plus)
        {
            Debug.Log("<color=orange>左上に加速</color>");
        }

        //右下に移動
        if (xMoveType == XMoveType.Plus && yMoveType == YMoveType.Minus)
        {
            Debug.Log("<color=green>右下に加速</color>");
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
        if (isOriginMove == false)
        {
            MoveSway(targetPos, NextInitializingProcess);
        }
        else if (isOriginMove == true)
        {
            MoveSway(originPos, EndProcess);
            if (isEnd == true)
            {
                //Debug.Log("<color=red>最初の座標位置についた！</color>");
                isOriginMove = false;
                //目的地にたどりついた
                return TaskStatus.Success;
            }
            else
            {
                //Debug.Log("<color=blue>元に戻る移動を実行中</color>");
                //移動実行中
                return TaskStatus.Running;
            }
        }

        //移動実行中
        return TaskStatus.Running;
    }

    void MoveSway(Vector3 pos, UnityAction unityAction)
    {
        float currentDistance = Mathf.Abs(Vector3.Distance(this.transform.position, pos));
        //Debug.Log("<color=red>currentDistance : " + currentDistance + "</color>");

        if (currentDistance <= stopPos)
        {
            //Debug.Log("<color=green>目的座標によって止まる</color>");
            unityAction.Invoke();
        }

        if (spareGoalDistance <= currentDistance)
        {
            //Debug.Log("<color=green>目的座標を通りこしてしまった場合のスペアの座標距離で止まる</color>");
            unityAction.Invoke();
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

                flyingEnemyController.Rigidbody.velocity = velocity;
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
                    flyingEnemyController.Rigidbody.velocity = velocity;
                }
                else
                {
                    unityAction.Invoke();
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

                flyingEnemyController.Rigidbody.velocity = velocity;
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
                    flyingEnemyController.Rigidbody.velocity = velocity;
                }
                else
                {
                    unityAction.Invoke();
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

                flyingEnemyController.Rigidbody.velocity = velocity;
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
                    flyingEnemyController.Rigidbody.velocity = velocity;
                }
                else
                {
                    unityAction.Invoke();
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

                flyingEnemyController.Rigidbody.velocity = velocity;
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
                    flyingEnemyController.Rigidbody.velocity = velocity;
                }
                else
                {
                    unityAction.Invoke();
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

                flyingEnemyController.Rigidbody.velocity = velocity;
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
                    flyingEnemyController.Rigidbody.velocity = velocity;
                }
                else
                {
                    unityAction.Invoke();
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

                flyingEnemyController.Rigidbody.velocity = velocity;
            }
            else if (currentDistance <= halfDistance)//初期距離の半分より現在の距離が小さい場合は減速する
            {
                //Debug.Log("<color=blue>減速</color>");

                //正規化したベクトルに加速度をかける
                Vector3 moveDirectionAcceleration = moveDirection * -acceleration;
                velocity = velocity + moveDirectionAcceleration;

                //減速する際に速度がマイナスになって後進してほしくない為
                if (velocity.x < 0 && 0 < velocity.y)//キャラクターの向きが左を向いているとマイナスになる
                {
                    flyingEnemyController.Rigidbody.velocity = velocity;
                }
                else
                {
                    unityAction.Invoke();
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

                flyingEnemyController.Rigidbody.velocity = velocity;
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
                    flyingEnemyController.Rigidbody.velocity = velocity;
                }
                else
                {
                    unityAction.Invoke();
                }
            }
        }

        //左下に移動
        if (xMoveType == XMoveType.Minus && yMoveType == YMoveType.Minus)
        {
            if (halfDistance < currentDistance)//初期距離の半分より現在の距離が大きい場合は前進する
            {
                //Debug.Log("<color=orange>左下に加速</color>");

                //正規化したベクトルに加速度をかける
                Vector3 moveDirectionAcceleration = moveDirection * acceleration;
                velocity = velocity + moveDirectionAcceleration;

                flyingEnemyController.Rigidbody.velocity = velocity;
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
                    flyingEnemyController.Rigidbody.velocity = velocity;
                }
                else
                {
                    unityAction.Invoke();
                }
            }
        }

        //原点
        if (xMoveType == XMoveType.Zero && yMoveType == YMoveType.Zero)
        {
            unityAction.Invoke();
        }
    }

    void NextInitializingProcess()
    {
        //Debug.Log("<color=green>velocity.xで止まる</color>");
        velocity = Vector3.zero;
        flyingEnemyController.Rigidbody.velocity = velocity;
        isOriginMove = true;
        InitMove(originPos);
    }

    void EndProcess()
    {
        //Debug.Log("<color=green>velocity.xで止まる</color>");
        velocity = Vector3.zero;
        flyingEnemyController.Rigidbody.velocity = velocity;
        isEnd = true;
    }
}