using UnityEngine;

public class LoadingNextStageScene : MonoBehaviour
{
	[Tooltip("次のステージ名")]
	[SerializeField] EnumManager.StageTYPE nextStage;
	[SerializeField] GameObject spawnPos;
	[Tooltip("連続ロードしないための変数")]
	bool isLoadOnce = false;

	void OnTriggerEnter(Collider collider)
	{
		if (collider.tag == "Player")
		{
			if (isLoadOnce == false)
			{
				//シーンの読み込みはSceneLoadManagerに任せる（このオブジェクトが破棄されても切り替えは必ず行われる）
				//他のシーンに切り替え中などで受け付けられなかった場合は何もしない
				bool isAccepted = SceneLoadManager.SingletonInstance.LoadScene(nextStage.ToString(), SetLoadingSliderValue, null, HideLoadingPanel);
				if (isAccepted == false)
				{
					return;
				}

				isLoadOnce = true;
				//ロード中にプレイヤーが移動してロードトリガーに触り連続ロードを行わないようにするための処理
				InGameManager.SingletonInstance.IsGamePlayReady = false;
				SetPlayerSpawnPos(collider, spawnPos);
				ShowLoadingPanel();
			}
		}
	}

	/// <summary>
	/// シーン遷移した際にプレイヤーのスポーン位置を設定
	/// </summary>
	void SetPlayerSpawnPos(Collider collider, GameObject spawnPos = null)
	{
		if (spawnPos != null)
		{
			collider.gameObject.transform.position = spawnPos.transform.position;
		}
		else
		{
			collider.gameObject.transform.position = new Vector3(0, 1, 0);
		}
	}

	/// <summary>
	/// ロードUIを表示する
	/// </summary>
	static void ShowLoadingPanel()
	{
		ScreenUIManagerPresenter screenUIManagerPresenter = ScreenUIManagerPresenter.SingletonInstance;
		if (screenUIManagerPresenter == null)
		{
			return;
		}

		//不透明にする
		screenUIManagerPresenter.FadeOut();
		//スライダーの値を最低にする
		screenUIManagerPresenter.ScreenUIView.SliderLoading.value = float.MinValue;
		//ロードUIをOnにする
		screenUIManagerPresenter.ScreenUIView.PanelLoading.gameObject.SetActive(true);
	}

	/// <summary>
	/// ロード数値をスライダーに反映する
	/// </summary>
	static void SetLoadingSliderValue(float progress)
	{
		ScreenUIManagerPresenter screenUIManagerPresenter = ScreenUIManagerPresenter.SingletonInstance;
		if (screenUIManagerPresenter == null)
		{
			return;
		}

		screenUIManagerPresenter.ScreenUIView.SliderLoading.value = progress;
	}

	/// <summary>
	/// ロードUIを非表示にする（シーンを切り替える直前に呼ばれる）
	/// </summary>
	static void HideLoadingPanel()
	{
		ScreenUIManagerPresenter screenUIManagerPresenter = ScreenUIManagerPresenter.SingletonInstance;
		if (screenUIManagerPresenter == null)
		{
			return;
		}

		screenUIManagerPresenter.ScreenUIView.PanelLoading.gameObject.SetActive(false);
	}
}