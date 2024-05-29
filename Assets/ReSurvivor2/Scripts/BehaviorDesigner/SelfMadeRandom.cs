using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Collections.Generic;

/// <summary>
/// Composite Task
/// 子に複数のタスクを接続できるタスク
/// </summary>
[TaskCategory("Kashiwabara")]
public class SelfMadeRandom : Composite//Compositeにより複数の子タスクを持つことができる
{
    //各子タスクの確率値のリスト
    public List<float> probabilitiesList = new List<float>();
    //ランダムな子インデックスの実行順序
    private int selectedChildIndex = -1;
    //最後に実行された子のタスクのステータス
    private TaskStatus executionStatus = TaskStatus.Inactive;

    /// <summary>
    /// タスクが目覚めた際に呼ばれる
    /// </summary>
    public override void OnAwake()
    {
        // 確率の数と子タスクの数が一致するかをチェック
        if (probabilitiesList.Count != children.Count)
        {
            Debug.LogError("子タスクの数と確率の数が一致しません");
        }
    }

    /// <summary>
    /// タスクは開始されると呼び出される
    /// </summary>
    public override void OnStart()
    {
        // 子タスクを確率に基づいて選択
        selectedChildIndex = SelectRandomChild();
    }

    /// <summary>
    /// 現在の子タスクのインデックスを返す
    /// </summary>
    public override int CurrentChildIndex()
    {
        //実行する子タスクのインデックスを返す
        return selectedChildIndex;
    }

    /// <summary>
    /// 子タスクが実行可能かどうかを判断する(trueなら実行を続ける、falseなら実行をやめる)
    /// </summary>
    public override bool CanExecute()
    {
        // どのタスクも成功を返さず、選択された子タスクがまだ実行されていない場合は、処理を続行する
        return executionStatus != TaskStatus.Success && executionStatus != TaskStatus.Failure;
    }

    /// <summary>
    /// 子タスクが実行された後に呼び出される
    /// </summary>
    public override void OnChildExecuted(TaskStatus childStatus)
    {
        //実行ステータスを更新する
        executionStatus = childStatus;
    }

    /// <summary>
    /// 条件付き中止が発生したときに呼び出される
    /// </summary>
    public override void OnConditionalAbort(int childIndex)
    {
        executionStatus = TaskStatus.Inactive;//TaskStatus.Inactive;はタスクが現在アクティブではない状態を表します。
        selectedChildIndex = SelectRandomChild();
    }

    /// <summary>
    /// タスクが終了したときに呼び出される
    /// </summary>
    public override void OnEnd()
    {
        executionStatus = TaskStatus.Inactive;//TaskStatus.Inactive;はタスクが現在アクティブではない状態を表します。
        selectedChildIndex = -1;
    }

    public override void OnReset()
    {
        probabilitiesList.Clear();
    }

    /// <summary>
    /// 子タスクをランダムで選択する
    /// </summary>
    private int SelectRandomChild()
    {
        //合計確率値
        float totalProbability = 0f;
        //確率リストの数だけ回す
        foreach (var probability in probabilitiesList)
        {
            //全ての確率値の合計を出す
            totalProbability = totalProbability + probability;
        }

        float randomPoint = Random.value * totalProbability;

        //確率リストの数だけ回す
        for (int i = 0; i < probabilitiesList.Count; i++)
        {
            //確率リストi番目よりランダムで出した数値が小さければtrue
            if (randomPoint < probabilitiesList[i])
            {
                //i番目を返す
                return i;
            }
            else
            {
                //ランダム数値から確率リストi番目を引いた数値をランダム数値に入れる
                randomPoint = randomPoint - probabilitiesList[i];
            }
        }

        return probabilitiesList.Count - 1; // 何かの理由で選択されなかった場合、最後の子タスクを返す
    }
}