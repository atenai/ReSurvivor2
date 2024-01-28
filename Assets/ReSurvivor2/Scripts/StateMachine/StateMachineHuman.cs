using UnityEngine;

public class StateMachineHuman : MonoBehaviour
{
    enum State
    {
        sleep,
        gotoToilet,
        washHand,
        doNothing,
        eat,
    }

    float sleepDesire = 0;//眠気 0 ~ 1
    float sleepDesireUpSpeed = 15f;
    float sleepDesireDownSpeed = 5f;

    float toiletDesire = 0f;//トイレに行きたい度 0 ~ 1
    float toiletDesireUpSpeed = 8f;
    float toiletTime = 2f;//トイレにかかる時間
    float handBacteria = 0f;//手の汚れ 0 ~ 1 トイレから出たら1になる
    float handWashSpeed = 2f;//手を洗うスピード

    float eatDesire = 0f;//空腹度
    float hungrySpeed = 10f;
    float eatSpeed = 3f;

    //ステートマシン
    State currentState = State.doNothing;
    bool stateEnter = true;

    void ChangeState(State newState)
    {
        currentState = newState;
        stateEnter = true;
    }

    void Update()
    {
        if (currentState != State.sleep)
        {
            sleepDesire += Time.deltaTime / sleepDesireUpSpeed;
        }

        if (currentState != State.gotoToilet)
        {
            toiletDesire += Time.deltaTime / toiletDesireUpSpeed;
        }

        if (currentState != State.eat)
        {
            eatDesire += Time.deltaTime / hungrySpeed;
        }

        switch (currentState)
        {
            case State.doNothing:
                #region//これを書くとコードが折りたためるようになるらしい 

                if (stateEnter)
                {
                    stateEnter = false;
                    Debug.Log("暇だなー。やることないなー");
                }

                if (1 <= eatDesire)
                {
                    ChangeState(State.eat);
                    return;
                }

                if (1 <= toiletDesire)
                {
                    ChangeState(State.gotoToilet);
                    return;
                }

                if (1 <= sleepDesire)
                {
                    ChangeState(State.sleep);
                    return;
                }

                #endregion
                break;

            case State.eat:
                #region//これを書くとコードが折りたためるようになるらしい 

                if (stateEnter == true)
                {
                    stateEnter = false;
                    Debug.Log("腹が減ったから飯をくおう！もぐもぐもぐ");
                }

                eatDesire -= Time.deltaTime / eatSpeed;

                if (eatDesire <= 0)
                {
                    ChangeState(State.doNothing);
                    return;
                }

                #endregion
                break;

            case State.gotoToilet:
                #region 

                if (stateEnter == true)
                {
                    stateEnter = false;
                    Debug.Log("トイレ行こう！！ジャアアアアアアアアア（水を流す音）");
                    handBacteria += 1;
                }

                toiletDesire -= Time.deltaTime / toiletTime;

                if (toiletDesire <= 0)
                {
                    ChangeState(State.washHand);
                    return;
                }

                #endregion
                break;

            case State.washHand:
                #region 

                if (stateEnter == true)
                {
                    stateEnter = false;
                    Debug.Log("すっきりした！！手を洗おう！！");
                }

                handBacteria -= Time.deltaTime / handWashSpeed;

                if (handBacteria <= 0)
                {
                    ChangeState(State.doNothing);
                    return;
                }

                #endregion
                break;

            case State.sleep:
                #region 

                if (stateEnter == true)
                {
                    stateEnter = false;
                    Debug.Log("寝るか・・・zzzzzzzz");
                }

                sleepDesire -= Time.deltaTime / sleepDesireDownSpeed;

                if (sleepDesire <= 0)
                {
                    ChangeState(State.doNothing);
                    return;
                }

                #endregion
                break;
        }
    }
}
