using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Human : MonoBehaviour
{
    enum State
    {
        sleep,
        gotoToilet,
        washHand,
        doNothing,
        eat,
        starveToDeath,
    }

    enum DesireType
    {
        sleep,
        eat,
        toilet,
    }

    class Desire
    {
        public DesireType type { get; private set; }
        public float value;

        public Desire(DesireType _type)
        {
            type = _type;
            value = 0f;
        }
    }

    class Desires
    {
        public List<Desire> desireList { get; private set; } = new List<Desire>();

        public Desire GetDesire(DesireType type)
        {
            foreach (Desire desire in desireList)
            {
                if (desire.type == type)
                {
                    return desire;
                }
            }

            return null;
        }

        public void SortDesire()
        {
            desireList.Sort((desire1, desire2) => desire2.value.CompareTo(desire1.value));
        }

        //コンストラクタ
        public Desires()
        {
            int desireNum = System.Enum.GetNames(typeof(DesireType)).Length;

            for (int i = 0; i < desireNum; i++)
            {
                DesireType type = (DesireType)System.Enum.ToObject(typeof(DesireType), i);
                Desire newDesire = new Desire(type);

                desireList.Add(newDesire);
            }
        }
    }

    Desires desires = new Desires();

    float sleepDesireUpSpeed = 15f;
    float sleepDesireDownSpeed = 5f;

    float toiletDesireUpSpeed = 2f;
    float toiletTime = 2f;//トイレにかかる時間

    float handBacteria = 0f;//手の汚れ 0 ~ 1 トイレから出たら1になる
    float handWashSpeed = 2f;//手を洗うスピード

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
            desires.GetDesire(DesireType.sleep).value += Time.deltaTime / sleepDesireUpSpeed;
        }

        if (currentState != State.gotoToilet)
        {
            desires.GetDesire(DesireType.toilet).value += Time.deltaTime / toiletDesireUpSpeed;
        }

        if (currentState != State.eat)
        {
            desires.GetDesire(DesireType.eat).value += Time.deltaTime / hungrySpeed;
        }

        if (currentState != State.starveToDeath && 3.0f <= desires.GetDesire(DesireType.eat).value)
        {
            ChangeState(State.starveToDeath);
            return;
        }

        switch (currentState)
        {
            case State.starveToDeath:
                #region 

                if (stateEnter == true)
                {
                    stateEnter = false;
                    Debug.Log("腹が減ってしんだゾ");
                }

                #endregion
                break;

            case State.doNothing:
                #region//これを書くとコードが折りたためるようになるらしい 

                if (stateEnter)
                {
                    stateEnter = false;
                    Debug.Log("暇だなー。やることないなー");
                }

                desires.SortDesire();
                if (1 <= desires.desireList[0].value)
                {
                    Desire desire = desires.desireList[0];
                    switch (desire.type)
                    {
                        case DesireType.eat:
                            ChangeState(State.eat);
                            return;
                        case DesireType.sleep:
                            ChangeState(State.sleep);
                            return;
                        case DesireType.toilet:
                            ChangeState(State.gotoToilet);
                            return;
                    }
                }

                #endregion
                break;

            case State.eat:
                #region//これを書くとコードが折りたためるようになるらしい 

                if (stateEnter == true)
                {
                    stateEnter = false;
                    Debug.Log("腹が減ったから飯をくおう！もぐもぐもぐ");
                    desires.GetDesire(DesireType.eat).value = 1;
                }

                desires.GetDesire(DesireType.eat).value -= Time.deltaTime / eatSpeed;

                if (desires.GetDesire(DesireType.eat).value <= 0)
                {
                    desires.SortDesire();
                    if (1 <= desires.desireList[0].value)
                    {
                        Desire desire = desires.desireList[0];
                        switch (desire.type)
                        {
                            case DesireType.eat:
                                ChangeState(State.eat);
                                return;
                            case DesireType.sleep:
                                ChangeState(State.sleep);
                                return;
                            case DesireType.toilet:
                                ChangeState(State.gotoToilet);
                                return;
                        }
                    }
                    ChangeState(State.doNothing);
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
                    desires.GetDesire(DesireType.toilet).value = 1;
                }

                desires.GetDesire(DesireType.toilet).value -= Time.deltaTime / toiletTime;

                if (desires.GetDesire(DesireType.toilet).value <= 0)
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
                    desires.SortDesire();
                    if (1 <= desires.desireList[0].value)
                    {
                        Desire desire = desires.desireList[0];
                        switch (desire.type)
                        {
                            case DesireType.eat:
                                ChangeState(State.eat);
                                return;
                            case DesireType.sleep:
                                ChangeState(State.sleep);
                                return;
                            case DesireType.toilet:
                                ChangeState(State.gotoToilet);
                                return;
                        }
                    }
                    ChangeState(State.doNothing);
                }

                #endregion
                break;

            case State.sleep:
                #region 

                if (stateEnter == true)
                {
                    stateEnter = false;
                    Debug.Log("寝るか・・・zzzzzzzz");
                    desires.GetDesire(DesireType.sleep).value = 1;
                }

                desires.GetDesire(DesireType.sleep).value -= Time.deltaTime / sleepDesireDownSpeed;

                if (desires.GetDesire(DesireType.sleep).value <= 0)
                {
                    desires.SortDesire();
                    if (1 <= desires.desireList[0].value)
                    {
                        Desire desire = desires.desireList[0];
                        switch (desire.type)
                        {
                            case DesireType.eat:
                                ChangeState(State.eat);
                                return;
                            case DesireType.sleep:
                                ChangeState(State.sleep);
                                return;
                            case DesireType.toilet:
                                ChangeState(State.gotoToilet);
                                return;
                        }
                    }
                    ChangeState(State.doNothing);
                }

                #endregion
                break;
        }
    }
}
