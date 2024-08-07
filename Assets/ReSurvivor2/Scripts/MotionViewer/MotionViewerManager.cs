using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEditor;


/// <summary>
/// モーションビュアシーンのクラス
/// </summary>
public class MotionViewerManager : MonoBehaviour
{
    #region 定数

    const float _maxAnimationSpeed = 2.0f;
    const float _minAnimationSpeed = 0.5f;
    const float _defaultspecifiedTime = 0.01f;
    const float _defaultSpawnForwardDistance = 1.5f;
    const float _defaultSpawnUpDistance = 0.0f;
    const float _defaultAttackCollisionScaleX = 2.0f;
    const float _defaultAttackCollisionScaleY = 2.0f;
    const float _defaultAttackCollisionScaleZ = 2.0f;
    const float _defaultSpawnWaitingTime = 0.4f;
    const float _defaultAttackDurationTime = 0.18f;

    #endregion

    #region 非公開変数

    [SerializeField] GameObject _contentModel;
    [SerializeField] GameObject _contentAnimation;
    [SerializeField] GameObject _copyModelButton;
    [SerializeField] GameObject _copyAnimationButton;
    [SerializeField] Slider _currentAnimationFrameBar;
    [SerializeField] TextMeshProUGUI _allAnimationFrameParameter;
    [SerializeField] TextMeshProUGUI _currentAnimationFrameParameter;
    [SerializeField] TextMeshProUGUI _animationSpeedParameter;
    [UnityEngine.Tooltip("進める秒数指定のインプットフィールド")]
    [SerializeField] TMP_InputField _specifySecondsInputField;
    [SerializeField] TMP_InputField _spawnForwardDistanceInputField;
    [SerializeField] TMP_InputField _spawnUpDistanceInputField;
    [UnityEngine.Tooltip("攻撃コリジョンのスケールX設定")]
    [SerializeField] TMP_InputField _attackCollisionScaleXInputField;
    [UnityEngine.Tooltip("攻撃コリジョンのスケールY設定")]
    [SerializeField] TMP_InputField _attackCollisionScaleYInputField;
    [UnityEngine.Tooltip("攻撃コリジョンのスケールZ設定")]
    [SerializeField] TMP_InputField _attackCollisionScaleZInputField;
    [UnityEngine.Tooltip("攻撃コリジョンのスケール値をローカルにするか？")]
    [SerializeField] bool _isLocalScale = false;
    [SerializeField] TMP_InputField _spawnWaitingTimeInputField;
    [SerializeField] TMP_InputField _attackDurationTimeInputField;
    [UnityEngine.Tooltip("攻撃コリジョンがエネミーの前方からどれくらい離れているか？のパラメーター")]
    [SerializeField] TextMeshProUGUI _forwardDistanceParameter;
    [UnityEngine.Tooltip("攻撃コリジョンがエネミーの上からどれくらい離れているか？のパラメーター")]
    [SerializeField] TextMeshProUGUI _upDistanceParameter;
    [UnityEngine.Tooltip("攻撃コリジョンのスケールXのパラメーター")]
    [SerializeField] TextMeshProUGUI _scaleXParameter;
    [UnityEngine.Tooltip("攻撃コリジョンのスケールYのパラメーター")]
    [SerializeField] TextMeshProUGUI _scaleYParameter;
    [UnityEngine.Tooltip("攻撃コリジョンのスケールZのパラメーター")]
    [SerializeField] TextMeshProUGUI _scaleZParameter;
    [Tooltip("表示したいエネミーのモデルをセットするリスト")]
    [SerializeField] List<GameObject> _enemyModelList = new List<GameObject>();

    List<GameObject> _modelList = new List<GameObject>();
    GameObject _currentActiveModel;
    Animator _mainBodyAnimator;
    List<Button> _allAnimationButtonList = new List<Button>();
    int _animationClipLength = 0;
    bool _isPlayAnimation = false;
    int _loopCount = 0;
    int _currentMainBodyAnimationHash = 0;
    [Tooltip("アニメーションのスピードを変える為の変数")]
    float _animationSpeed = 1.0f;
    [Tooltip("指定秒数アニメーションを進める為の変数")]
    float _specifiedAnimationTime = 0.01f;

    [Header("Collision")]
    [UnityEngine.Tooltip("上攻撃用コリジョンオブジェクトのプレハブ")]
    [SerializeField] GameObject _attackCollisionPrefab;
    [UnityEngine.Tooltip("エネミーの前方からどのくらい離れた位置に攻撃用コリジョンオブジェクトを生成するか？の距離")]
    float _spawnForwardDistance = 0.0f;
    [UnityEngine.Tooltip("エネミーの上からどのくらい離れた位置に攻撃用コリジョンオブジェクトを生成するか？の距離")]
    float _spawnUpDistance = 0.0f;
    [UnityEngine.Tooltip("攻撃コリジョンのスケールX設定")]
    float _attackCollisionScaleX = 0.0f;
    [UnityEngine.Tooltip("攻撃コリジョンのスケールY設定")]
    float _attackCollisionScaleY = 0.0f;
    [UnityEngine.Tooltip("攻撃コリジョンのスケールZ設定")]
    float _attackCollisionScaleZ = 0.0f;
    [UnityEngine.Tooltip("攻撃コリジョンを何秒待ってから生成するか？の時間")]
    float _spawnWaitingTime = 0.0f;
    [UnityEngine.Tooltip("攻撃コリジョンの持続時間")]
    float _attackDurationTime = 0.0f;
    [UnityEngine.Tooltip("攻撃コリジョン")]
    GameObject _attackCollision = null;

    bool _isDragging = false;
    float _forwardDistanceParameterTemporary = 0.0f;
    float _upDistanceParameterTemporary = 0.0f;
    float _scaleXParameterTemporary = 0.0f;
    float _scaleYParameterTemporary = 0.0f;
    float _scaleZParameterTemporary = 0.0f;
    float _spawnWaitingTimeTemporary = 0.0f;
    #endregion

    #region プロパティ

    public GameObject CurrentActiveModel => _currentActiveModel;
    public bool IsPlayAnimation => _isPlayAnimation;
    public bool IsDragging
    {
        get { return _isDragging; }
        set { _isDragging = value; }
    }
    public GameObject AttackCollision
    {
        get { return _attackCollision; }
        set { _attackCollision = value; }
    }

    #endregion

    #region Monobehaviour Override関数

    void Awake()
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN//Unityエディター上または端末がPCだった場合の処理

        //Screen.SetResolution(1920, 1080, true, 60);

#endif //終了
    }

    void Start()
    {
        Debug.Log("Start");

        InitInputSpecifySeconds();
        InitInputSpawnForwardDistance();
        InitInputSpawnUpDistance();
        InitInputAttackCollisionScale();
        InitInputSpawnWaitingTime();
        InitInputAttackDurationTime();
        InitSlider();
        CreateModel();
    }

    void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN//Unityエディター上または端末がPCだった場合の処理
        //Escapeキーでゲーム終了
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Quit();//ゲーム終了
        }
#endif //終了  

        InputSpecifySeconds();
        InputSpawnForwardDistance();
        InputSpawnUpDistance();
        InputAttackCollisionScaleX();
        InputAttackCollisionScaleY();
        InputAttackCollisionScaleZ();
        InputSpawnWaitingTime();
        InputAttackDurationTime();
        ProcessForAnimationLoopCount();
        AnimationUI();
        AttackCollisionParameterDisplay();
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 30;
        GUIStyleState styleState = new GUIStyleState();
        styleState.textColor = Color.red;
        style.normal = styleState;

        // GUI.Box(new Rect(10, 100, 100, 50), "_currentMainBodyAnimationTime", style);
        // GUI.Box(new Rect(500, 100, 100, 50), _currentMainBodyAnimationTime.ToString(), style);
        // GUI.Box(new Rect(10, 150, 100, 50), "_currentWeaponAnimationTime", style);
        // GUI.Box(new Rect(500, 150, 100, 50), _currentWeaponAnimationTime.ToString(), style);
        // GUI.Box(new Rect(10, 200, 100, 50), "_currentMainBodyAnimationHash", style);
        // GUI.Box(new Rect(500, 200, 100, 50), _currentMainBodyAnimationHash.ToString(), style);
        // GUI.Box(new Rect(10, 250, 100, 50), "_currentWeaponAnimationHash", style);
        // GUI.Box(new Rect(500, 250, 100, 50), _currentWeaponAnimationHash.ToString(), style);
        //GUI.Box(new Rect(10, 300, 100, 50), "_specifiedAnimationTime", style);
        //GUI.Box(new Rect(500, 300, 100, 50), _specifiedAnimationTime.ToString(), style);
    }

    #endregion

    #region 公開関数

    /// <summary>
    /// アニメーションの一時停止ボタン
    /// </summary>
    public void OnClickAnimationPause()
    {
        MainBodyAnimationPause();
    }

    /// <summary>
    /// アニメーションの再生ボタン
    /// </summary>
    public void OnClickAnimationPlay()
    {
        MainBodyAnimationPlay();
    }

    /// <summary>
    /// アニメーションスピードを速くする
    /// </summary>
    public void OnClickFastAnimationSpeed()
    {
        if (_mainBodyAnimator == null)
        {
            Debug.Log("_mainBodyAnimatorがnullです。");
            return;
        }

        if (_animationSpeed < _maxAnimationSpeed)
        {
            _animationSpeed = _animationSpeed + 0.5f;
        }

        _mainBodyAnimator.SetFloat("animationSpeed", _animationSpeed);
    }

    /// <summary>
    /// アニメーションスピードを遅くする
    /// </summary>
    public void OnClickSlowAnimationSpeed()
    {
        if (_mainBodyAnimator == null)
        {
            Debug.Log("_mainBodyAnimatorがnullです。");
            return;
        }

        if (_minAnimationSpeed < _animationSpeed)
        {
            _animationSpeed = _animationSpeed - 0.5f;
        }

        _mainBodyAnimator.SetFloat("animationSpeed", _animationSpeed);
    }

    /// <summary>
    /// アニメーションを指定秒数進める
    /// </summary>
    public void OnClickAdvanceAnimation()
    {
        StartCoroutine(MainBodyAdvanceAnimation());
    }

    /// <summary>
    /// アニメーションを指定秒数巻き戻す
    /// </summary>
    public void OnClickRewindAnimation()
    {
        StartCoroutine(MainBodyRewindAnimation());
    }

    /// <summary>
    /// 現在のアニメーション時間またはフレームのスライダー数値をリアルタイムに反映
    /// </summary>
    public void CurrentAnimationFrameUpdate()
    {
        if (_mainBodyAnimator != null)
        {
            //アニメーションタイムで表示する
            //float calculation = _mainBodyAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime * _mainBodyAnimator.GetCurrentAnimatorStateInfo(0).length;
            float calculation = _currentAnimationFrameBar.value * _mainBodyAnimator.GetCurrentAnimatorStateInfo(0).length;//↑のコメントアウトしている方が計算式として正しいが、こちらは今回のスライダーの値を適用したタイプになる
                                                                                                                          //_currentAnimationFrameParameter.text = calculation.ToString();

            //アニメーションフレームで表示する
            AnimatorClipInfo clipInfo = _mainBodyAnimator.GetCurrentAnimatorClipInfo(0)[0];//引数はLayer番号、配列の0番目
            AnimationClip clip = clipInfo.clip;

            float clipLength = clip.length;//2.133、アニメーションクリップの長さが取得できる
            float sampleRate = clip.frameRate;//60、アニメーションクリップのサンプルレートが取得できる
            float frameCount = Mathf.Abs(clipLength * sampleRate);//128、アニメーションのフレーム数が取得できる

            //Debug.Log("Animation Length: " + clipLength);
            //Debug.Log("Sample Rate: " + sampleRate);
            //Debug.Log("Frame Count: " + frameCount);

            //float currentClipLength = _mainBodyAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime * _mainBodyAnimator.GetCurrentAnimatorStateInfo(0).length;
            float currentClipLength = _currentAnimationFrameBar.value * _mainBodyAnimator.GetCurrentAnimatorStateInfo(0).length;//↑のコメントアウトしている方が計算式として正しいが、こちらは今回のスライダーの値を適用したタイプになる
            float currentFrameCount = Mathf.Abs(currentClipLength * sampleRate);

            if (_currentAnimationFrameParameter != null)
            {
                _currentAnimationFrameParameter.text = currentFrameCount.ToString();
            }

            _spawnWaitingTimeTemporary = currentClipLength;
        }
    }

    /// <summary>
    /// スライダーの位置のアニメーションを反映させる
    /// </summary>
    public IEnumerator SliderReflectAnimation(float value)
    {
        _mainBodyAnimator.enabled = true;

        //Debug.Log("value : " + value);
        // スライダーの値に応じてアニメーションの再生位置を設定
        float normalizedTime = Mathf.Clamp01(value); // スライダーの値を0〜1の範囲にクランプ
        _mainBodyAnimator.Play(_currentMainBodyAnimationHash, 0, normalizedTime);

        //1フレームだけ待つ
        yield return null;

        _mainBodyAnimator.enabled = false;
    }

    /// <summary>
    /// 攻撃用コリジョンの生成ボタン
    /// </summary>
    public void OnCreateAttackCollision()
    {
        CreateAttackCollision();
    }

    /// <summary>
    /// 攻撃コリジョンの破棄ボタン
    /// </summary>
    public void OnDestroyAttackCollision()
    {
        DestroyAttackCollision();
    }

    /// <summary>
    /// アニメーションを最初からおこなうボタン
    /// </summary>
    public void OnReStartAnimation()
    {
        ReStartAnimation();
    }

    /// <summary>
    /// アニメーションとコリジョン生成を同時に行うボタン
    /// </summary>
    public void OnSimultaneousAnimationAndCollision()
    {
        StartCoroutine(SimultaneousAnimationAndCollision());
    }

    /// <summary>
    /// ドラッグアンドドロップでコリジョンを生成するのをアクティブにする
    /// </summary>
    public void OnDragAndDropCreateCollisionActive()
    {
        _isDragging = true;
    }

    /// <summary>
    /// ドラッグアンドドロップでコリジョンを生成するのを非アクティブにする
    /// </summary>
    public void OnDragAndDropCreateCollisionInactive()
    {
        _isDragging = false;
    }

    /// <summary>
    /// 攻撃コリジョンの一時保存データをコピー
    /// </summary>
    public void OnCopyTemporaryParameter()
    {
        CopyTemporaryParameter();
    }

    #endregion

    #region 非公開関数

    /// <summary>
    /// 進める秒数指定のインプットフィールドの初期化処理
    /// </summary>
    void InitInputSpecifySeconds()
    {
        if (_specifySecondsInputField == null)
        {
            return;
        }

        _specifySecondsInputField.text = _defaultspecifiedTime.ToString();
    }

    /// <summary>
    /// エネミーの前方からどのくらい離れた位置に攻撃用コリジョンオブジェクトを生成するか？の距離を入力するインプットフィールドの初期化処理
    /// </summary>
    void InitInputSpawnForwardDistance()
    {
        if (_spawnForwardDistanceInputField == null)
        {
            return;
        }

        _spawnForwardDistanceInputField.text = _defaultSpawnForwardDistance.ToString();
    }

    /// <summary>
    /// エネミーの上からどのくらい離れた位置に攻撃用コリジョンオブジェクトを生成するか？の距離を入力するインプットフィールドの初期化処理
    /// </summary>
    void InitInputSpawnUpDistance()
    {
        if (_spawnUpDistanceInputField == null)
        {
            return;
        }

        _spawnUpDistanceInputField.text = _defaultSpawnUpDistance.ToString();
    }

    /// <summary>
    /// 攻撃コリジョンのスケール値を入力するインプットフィールドの初期化処理
    /// </summary>
    void InitInputAttackCollisionScale()
    {
        if (_attackCollisionScaleXInputField == null || _attackCollisionScaleYInputField == null || _attackCollisionScaleZInputField == null)
        {
            return;
        }

        _attackCollisionScaleXInputField.text = _defaultAttackCollisionScaleX.ToString();
        _attackCollisionScaleYInputField.text = _defaultAttackCollisionScaleY.ToString();
        _attackCollisionScaleZInputField.text = _defaultAttackCollisionScaleZ.ToString();
    }

    /// <summary>
    /// 攻撃コリジョンを何秒待ってから生成するか？の時間を入力するインプットフィールドの初期化処理
    /// </summary>
    void InitInputSpawnWaitingTime()
    {
        if (_spawnWaitingTimeInputField == null)
        {
            return;
        }

        _spawnWaitingTimeInputField.text = _defaultSpawnWaitingTime.ToString();
    }

    /// <summary>
    /// 攻撃コリジョンの持続時間を入力するインプットフィールドの初期化処理
    /// </summary>
    void InitInputAttackDurationTime()
    {
        if (_attackDurationTimeInputField == null)
        {
            return;
        }

        _attackDurationTimeInputField.text = _defaultAttackDurationTime.ToString();
    }

    /// <summary>
    /// スライダーの初期化処理
    /// </summary>
    void InitSlider()
    {
        _currentAnimationFrameBar.minValue = 0;
        _currentAnimationFrameBar.maxValue = 1;
    }

    /// <summary>
    /// モデルを生成
    /// </summary>
    void CreateModel()
    {
        for (int i = 0; i < _enemyModelList.Count; i++)
        {
            GameObject prefabInstance = Instantiate(_enemyModelList[i]);
            prefabInstance.transform.rotation = Quaternion.Euler(0.0f, 180, 0.0f);
            prefabInstance.gameObject.SetActive(false);

            _modelList.Add(prefabInstance);
            //Debug.Log("<color=red>エネミーの生成完了！</color>");

            CreateModelButtons(i);
        }
    }

    /// <summary>
    /// モデルのボタンを作成
    /// </summary>
    void CreateModelButtons(int id)
    {
        //Debug.Log("enemyID : " + i);

        //ボタンクリエイト
        GameObject buttonGameObject = Instantiate(_copyModelButton, _contentModel.transform);
        buttonGameObject.gameObject.SetActive(true);
        Button button = buttonGameObject.GetComponent<Button>();

        //ボタンに名前の付与
        button.GetComponent<MotionViewerButtonInfo>().ButtonName.text = _modelList[id].name;

        int index = id;
        button.onClick.AddListener(() =>
        {
            ChangeModel(index);
            //Debug.Log("<color=red>モデルボタンをクリック！</color>");
        });

        //Debug.Log("<color=red>ボタン作成終了</color>");
    }

    /// <summary>
    /// モデルの切り替え
    /// </summary>
    void ChangeModel(int id)
    {
        //Debug.Log("<color=blue>_modelList[0] : " + _modelList[0] + "</color>");
        //Debug.Log("<color=blue>_modelList[1] : " + _modelList[1] + "</color>");

        foreach (var model in _modelList)
        {
            model.gameObject.SetActive(false);
        }

        _modelList[id].gameObject.SetActive(true);
        _currentActiveModel = _modelList[id];

        ResetAnimationButton();
        AcquisitionAnimatorController(_modelList[id].gameObject);
        InitAnimation();
    }

    /// <summary>
    /// アニメーションボタンのリセット
    /// </summary>
    void ResetAnimationButton()
    {
        _allAnimationButtonList.Clear();
        // 親オブジェクトのTransformコンポーネント内のすべての子オブジェクトを削除
        foreach (Transform child in _contentAnimation.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// アニメーターを取得
    /// </summary>
    void AcquisitionAnimatorController(GameObject model)
    {
        _mainBodyAnimator = model.GetComponent<Animator>();

        //animationのeventを無効化
        _mainBodyAnimator.fireEvents = false;
    }

    /// <summary>
    /// 各種アニメーションの初期化処理
    /// </summary>
    void InitAnimation()
    {
        _mainBodyAnimator.enabled = false;
        GetMainBodyAllAnimationClipsNameAndHash(_mainBodyAnimator);
    }

    /// <summary>
    /// メインボディの全てのアニメーションクリップの名前とハッシュ値を取得する
    /// </summary>
    void GetMainBodyAllAnimationClipsNameAndHash(Animator animator)
    {
        //Debug.Log("<color=orange>animator :" + animator + "</color>");
        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        _animationClipLength = clips.Length;
        for (int i = 0; i < _animationClipLength; i++)
        {
            //ボタンクリエイト
            GameObject buttonGameObject = Instantiate(_copyAnimationButton, _contentAnimation.transform);
            buttonGameObject.gameObject.SetActive(true);
            Button button = buttonGameObject.GetComponent<Button>();

            //ボタンに名前の付与
            //Debug.Log("<color=green>clip.name :" + clips[i].name + "</color>");
            if (i != 0)
            {
                button.GetComponent<MotionViewerButtonInfo>().ButtonName.text = clips[i].name;
            }
            else
            {
                button.GetComponent<MotionViewerButtonInfo>().ButtonName.text = "Entry";
            }

            //ボタンにアニメーションクリップを追加
            int animationHash = Animator.StringToHash(clips[i].name);
            Debug.Log("<color=blue>animationHash :" + animationHash + "</color>");

            button.onClick.AddListener(() =>
            {
                PlayMainBodyAnimation(animationHash);
            });

            //ボタンリストに追加
            _allAnimationButtonList.Add(button);
        }
    }

    /// <summary>
    /// 本体アニメーションを再生する関数
    /// </summary>
    void PlayMainBodyAnimation(int hash)
    {
        if (_mainBodyAnimator == null)
        {
            Debug.Log("_mainBodyAnimatorがnullです。");
            return;
        }

        _currentMainBodyAnimationHash = hash;

        //本体側アニメーション
        _mainBodyAnimator.enabled = true;

        //アニメーションをプレイ
        _mainBodyAnimator.Play(hash);
        _mainBodyAnimator.Update(0.0f);

        _isPlayAnimation = true;
    }

    /// <summary>
    /// アニメーションのループ回数に応じた処理を行う
    /// </summary>
    void ProcessForAnimationLoopCount()
    {
        if (_mainBodyAnimator != null)
        {
            //ここでアニメーションクリップの時間がアニメーションクリップの長さを超えていたら
            int currentLoopCount = (int)_mainBodyAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            //Debug.Log("<color=orange>currentLoopCount : " + currentLoopCount + "</color>");

            if (currentLoopCount == 0)
            {
                _loopCount = 0;
            }
            else
            {
                //Debug.Log("<color=red>同じアニメーションだよ！</color>");
            }

            if (_loopCount < currentLoopCount)
            {
                //Debug.Log("<color=#00a1e9>アニメーション終了</color>");
                _mainBodyAnimator.enabled = false;
                _loopCount = currentLoopCount;
                _isPlayAnimation = false;
            }
        }
    }

    /// <summary>
    /// アニメーション関連のUI処理
    /// </summary>
    void AnimationUI()
    {
        if (_animationSpeedParameter != null)
        {
            _animationSpeedParameter.text = _animationSpeed.ToString();
        }

        if (_mainBodyAnimator != null)
        {
            if (_mainBodyAnimator.enabled == true)
            {
                _currentAnimationFrameBar.value = _mainBodyAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            }
        }

        AllAnimationFrameUpdate();
    }

    /// <summary>
    /// モーション時間またはフレームの全体の長さを更新
    /// </summary>
    void AllAnimationFrameUpdate()
    {
        if (_mainBodyAnimator != null)
        {
            //アニメーションタイムで表示する
            //_allAnimationFrameParameter.text = _mainBodyAnimator.GetCurrentAnimatorStateInfo(0).length.ToString();

            //アニメーションフレームで表示する
            AnimatorClipInfo clipInfo = _mainBodyAnimator.GetCurrentAnimatorClipInfo(0)[0];//引数はLayer番号、配列の0番目
            AnimationClip clip = clipInfo.clip;

            float clipLength = clip.length;//2.133、アニメーションクリップの長さが取得できる
            float sampleRate = clip.frameRate;//60、アニメーションクリップのサンプルレートが取得できる
            float frameCount = Mathf.Abs(clipLength * sampleRate);//128、アニメーションのフレーム数が取得できる

            //Debug.Log("Animation Length: " + clipLength);
            //Debug.Log("Sample Rate: " + sampleRate);
            //Debug.Log("Frame Count: " + frameCount);

            float currentClipLength = _mainBodyAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime * _mainBodyAnimator.GetCurrentAnimatorStateInfo(0).length;
            float currentFrameCount = Mathf.Abs(currentClipLength * sampleRate);

            float calculationFrame = frameCount / _animationSpeed;

            if (_allAnimationFrameParameter != null)
            {
                _allAnimationFrameParameter.text = calculationFrame.ToString();
            }
        }
    }

    /// <summary>
    /// 攻撃コリジョンのパラメータを表示する
    /// </summary>
    void AttackCollisionParameterDisplay()
    {
        if (_attackCollision == null)
        {
            return;
        }

        _forwardDistanceParameter.text = _attackCollision.transform.localPosition.z.ToString();
        _upDistanceParameter.text = _attackCollision.transform.localPosition.y.ToString();
        _scaleXParameter.text = _attackCollision.transform.localScale.x.ToString();
        _scaleYParameter.text = _attackCollision.transform.localScale.y.ToString();
        _scaleZParameter.text = _attackCollision.transform.localScale.z.ToString();

        _forwardDistanceParameterTemporary = RoundToTwoDecimalPlaces(_attackCollision.transform.localPosition.z);
        _upDistanceParameterTemporary = RoundToTwoDecimalPlaces(_attackCollision.transform.localPosition.y);
        _scaleXParameterTemporary = RoundToTwoDecimalPlaces(_attackCollision.transform.localScale.x);
        _scaleYParameterTemporary = RoundToTwoDecimalPlaces(_attackCollision.transform.localScale.y);
        _scaleZParameterTemporary = RoundToTwoDecimalPlaces(_attackCollision.transform.localScale.z);
    }

    /// <summary>
    /// 小数点第2以下を四捨五入する関数
    /// </summary>
    float RoundToTwoDecimalPlaces(float number)
    {
        return Mathf.Round(number * 100f) / 100f;
    }

    /// <summary>
    /// メインボディアニメーションの一時停止(ボタン)
    /// </summary>
    void MainBodyAnimationPause()
    {
        if (_mainBodyAnimator == null)
        {
            Debug.Log("_mainBodyAnimatorがnullです。");
            return;
        }

        _mainBodyAnimator.enabled = false;

        _isPlayAnimation = false;
    }

    /// <summary>
    /// メインボディアニメーションの再生(ボタン)
    /// </summary>
    void MainBodyAnimationPlay()
    {
        if (_mainBodyAnimator == null)
        {
            Debug.Log("_mainBodyAnimatorがnullです。");
            return;
        }

        if (_mainBodyAnimator.enabled == true)
        {
            return;
        }
        _mainBodyAnimator.enabled = true;
        _mainBodyAnimator.Play(_currentMainBodyAnimationHash, 0, _mainBodyAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime);

        _isPlayAnimation = true;
    }

    /// <summary>
    /// アニメーションを指定秒進める
    /// </summary>
    IEnumerator MainBodyAdvanceAnimation()
    {
        _currentAnimationFrameBar.value = _mainBodyAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime + _specifiedAnimationTime;

        _mainBodyAnimator.enabled = true;

        _mainBodyAnimator.Play(_currentMainBodyAnimationHash, 0, _mainBodyAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime + _specifiedAnimationTime);

        //1フレームだけ待つ
        yield return null;

        _mainBodyAnimator.enabled = false;
    }

    /// <summary>
    /// アニメーション指定秒数巻き戻す
    /// </summary>
    IEnumerator MainBodyRewindAnimation()
    {
        _currentAnimationFrameBar.value = _mainBodyAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime - _specifiedAnimationTime;

        _mainBodyAnimator.enabled = true;

        _mainBodyAnimator.Play(_currentMainBodyAnimationHash, 0, _mainBodyAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime - _specifiedAnimationTime);

        //1フレームだけ待つ
        yield return null;

        _mainBodyAnimator.enabled = false;
    }

    /// <summary>
    /// 秒数指定
    /// </summary>
    void InputSpecifySeconds()
    {
        if (_specifySecondsInputField == null)
        {
            return;
        }

        //インプットフィールドの入力欄が空白なら中身を実行する
        if ("" == _specifySecondsInputField.text)
        {
            //デフォルト数値を入れる
            _specifiedAnimationTime = _defaultspecifiedTime;
            return;
        }

        //入力した文字の値を数値化して入れる
        _specifiedAnimationTime = float.Parse(_specifySecondsInputField.text);
    }

    /// <summary>
    /// エネミーの前方からどのくらい離れた位置に攻撃用コリジョンオブジェクトを生成するか？の距離を入力するインプットフィールドの数値指定
    /// </summary>
    void InputSpawnForwardDistance()
    {
        if (_spawnForwardDistanceInputField == null)
        {
            return;
        }

        //インプットフィールドの入力欄が空白なら中身を実行する
        if ("" == _spawnForwardDistanceInputField.text)
        {
            //デフォルト数値を入れる
            _spawnForwardDistance = _defaultSpawnForwardDistance;
            return;
        }

        //入力した文字の値を数値化して入れる
        float number;
        bool isValid = float.TryParse(_spawnForwardDistanceInputField.text, out number);

        if (isValid == true)
        {
            //Debug.Log("入力された数値: " + number);
            _spawnForwardDistance = number;
        }
        else
        {
            Debug.LogWarning("無効な入力です。");
        }
    }

    /// <summary>
    /// エネミーの上からどのくらい離れた位置に攻撃用コリジョンオブジェクトを生成するか？の距離を入力するインプットフィールドの数値指定
    /// </summary>
    void InputSpawnUpDistance()
    {
        if (_spawnUpDistanceInputField == null)
        {
            return;
        }

        //インプットフィールドの入力欄が空白なら中身を実行する
        if ("" == _spawnUpDistanceInputField.text)
        {
            //デフォルト数値を入れる
            _spawnUpDistance = _defaultSpawnUpDistance;
            return;
        }

        //入力した文字の値を数値化して入れる
        float number;
        bool isValid = float.TryParse(_spawnUpDistanceInputField.text, out number);

        if (isValid == true)
        {
            //Debug.Log("入力された数値: " + number);
            _spawnUpDistance = number;
        }
        else
        {
            Debug.LogWarning("無効な入力です。");
        }
    }

    /// <summary>
    /// 攻撃コリジョンXのスケール値を入力するインプットフィールドの数値指定
    /// </summary>
    void InputAttackCollisionScaleX()
    {
        if (_attackCollisionScaleXInputField == null)
        {
            return;
        }

        //インプットフィールドの入力欄が空白なら中身を実行する
        if ("" == _attackCollisionScaleXInputField.text)
        {
            //デフォルト数値を入れる
            _attackCollisionScaleX = _defaultAttackCollisionScaleX;
            return;
        }

        //入力した文字の値を数値化して入れる
        _attackCollisionScaleX = float.Parse(_attackCollisionScaleXInputField.text);
    }

    /// <summary>
    /// 攻撃コリジョンYのスケール値を入力するインプットフィールドの数値指定
    /// </summary>
    void InputAttackCollisionScaleY()
    {
        if (_attackCollisionScaleYInputField == null)
        {
            return;
        }

        //インプットフィールドの入力欄が空白なら中身を実行する
        if ("" == _attackCollisionScaleYInputField.text)
        {
            //デフォルト数値を入れる
            _attackCollisionScaleY = _defaultAttackCollisionScaleY;
            return;
        }

        //入力した文字の値を数値化して入れる
        _attackCollisionScaleY = float.Parse(_attackCollisionScaleYInputField.text);
    }

    /// <summary>
    /// 攻撃コリジョンZのスケール値を入力するインプットフィールドの数値指定
    /// </summary>
    void InputAttackCollisionScaleZ()
    {
        if (_attackCollisionScaleZInputField == null)
        {
            return;
        }

        //インプットフィールドの入力欄が空白なら中身を実行する
        if ("" == _attackCollisionScaleZInputField.text)
        {
            //デフォルト数値を入れる
            _attackCollisionScaleZ = _defaultAttackCollisionScaleZ;
            return;
        }

        //入力した文字の値を数値化して入れる
        _attackCollisionScaleZ = float.Parse(_attackCollisionScaleZInputField.text);
    }

    /// <summary>
    /// アニメーションを最初からおこなう
    /// </summary>
    void ReStartAnimation()
    {
        if (_mainBodyAnimator == null)
        {
            Debug.Log("_mainBodyAnimatorがnullです。");
            return;
        }

        _mainBodyAnimator.enabled = true;

        _mainBodyAnimator.Play(_currentMainBodyAnimationHash, 0, 0);
    }

    /// <summary>
    /// 攻撃用コリジョンの生成
    /// </summary>
    void CreateAttackCollision()
    {
        if (_currentActiveModel == null)
        {
            return;
        }

        if (_attackCollision != null)
        {
            UnityEngine.Object.Destroy(_attackCollision);
        }
        // キャラクターの前方に攻撃用コリジョンオブジェクトを生成
        Vector3 spawnPosition = _currentActiveModel.transform.position + (_currentActiveModel.transform.forward * _spawnForwardDistance) + (_currentActiveModel.transform.up * _spawnUpDistance);
        if (_isLocalScale == true)
        {
            _attackCollision = UnityEngine.Object.Instantiate(_attackCollisionPrefab, spawnPosition, _currentActiveModel.transform.rotation);
        }
        else
        {
            _attackCollision = UnityEngine.Object.Instantiate(_attackCollisionPrefab, spawnPosition, Quaternion.identity);
        }
        _attackCollision.transform.localScale = new Vector3(_attackCollisionScaleX, _attackCollisionScaleY, _attackCollisionScaleZ);
        _attackCollision.transform.SetParent(_currentActiveModel.transform);
    }

    /// <summary>
    /// 攻撃コリジョンの破棄
    /// </summary>
    public void DestroyAttackCollision()
    {
        if (_attackCollision != null)
        {
            UnityEngine.Object.Destroy(_attackCollision);
        }
    }

    /// <summary>
    /// アニメーションとコリジョン生成を同時に行う
    /// </summary>
    IEnumerator SimultaneousAnimationAndCollision()
    {
        DestroyAttackCollision();
        ReStartAnimation();

        //_spawnWaitingTime 秒後に攻撃コリジョンを生成
        yield return new WaitForSeconds(_spawnWaitingTime);
        CreateAttackCollision();
        //_attackDurationTime 秒後に攻撃コリジョンを削除（持続時間）
        yield return new WaitForSeconds(_attackDurationTime);
        DestroyAttackCollision();
    }

    /// <summary>
    /// 攻撃コリジョンを何秒待ってから生成するか？の時間を入力するインプットフィールドの数値指定
    /// </summary>
    void InputSpawnWaitingTime()
    {
        if (_spawnWaitingTimeInputField == null)
        {
            return;
        }

        //インプットフィールドの入力欄が空白なら中身を実行する
        if ("" == _spawnWaitingTimeInputField.text)
        {
            //デフォルト数値を入れる
            _spawnWaitingTime = _defaultSpawnWaitingTime;
            return;
        }

        //入力した文字の値を数値化して入れる
        _spawnWaitingTime = float.Parse(_spawnWaitingTimeInputField.text);
    }

    /// <summary>
    /// 攻撃コリジョンの持続時間を入力するインプットフィールドの数値指定
    /// </summary>
    void InputAttackDurationTime()
    {
        if (_attackDurationTimeInputField == null)
        {
            return;
        }

        //インプットフィールドの入力欄が空白なら中身を実行する
        if ("" == _attackDurationTimeInputField.text)
        {
            //デフォルト数値を入れる
            _attackDurationTime = _defaultAttackDurationTime;
            return;
        }

        //入力した文字の値を数値化して入れる
        _attackDurationTime = float.Parse(_attackDurationTimeInputField.text);
    }

    /// <summary>
    /// 攻撃コリジョンの一時保存データをコピー
    /// </summary>
    void CopyTemporaryParameter()
    {
        _spawnForwardDistance = _forwardDistanceParameterTemporary;
        _spawnUpDistance = _upDistanceParameterTemporary;
        _attackCollisionScaleX = _scaleXParameterTemporary;
        _attackCollisionScaleY = _scaleYParameterTemporary;
        _attackCollisionScaleZ = _scaleZParameterTemporary;

        _spawnForwardDistanceInputField.text = _forwardDistanceParameterTemporary.ToString();
        _spawnUpDistanceInputField.text = _upDistanceParameterTemporary.ToString();
        _attackCollisionScaleXInputField.text = _scaleXParameterTemporary.ToString();
        _attackCollisionScaleYInputField.text = _scaleYParameterTemporary.ToString();
        _attackCollisionScaleZInputField.text = _scaleZParameterTemporary.ToString();

        _spawnWaitingTime = _spawnWaitingTimeTemporary;
        _spawnWaitingTimeInputField.text = _spawnWaitingTimeTemporary.ToString();
    }

    void Quit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#elif UNITY_STANDALONE
        UnityEngine.Application.Quit();
#endif
    }

    #endregion
}
