using UnityEngine;

public class LoadingNextStageScene : MonoBehaviour
{
	[Tooltip("次のステージ名")]
	[SerializeField] EnumManager.StageTYPE nextStage;
	public EnumManager.StageTYPE NextStage => nextStage;
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
				bool isAccepted = SceneLoadManager.SingletonInstance.LoadScene(nextStage.ToString(), ScreenUIManagerPresenter.SingletonInstance.SetLoadingSliderValue, ScreenUIManagerPresenter.SingletonInstance.HideLoadingPanel);
				if (isAccepted == false)
				{
					return;
				}

				isLoadOnce = true;
				//ロード中にプレイヤーが移動してロードトリガーに触り連続ロードを行わないようにするための処理
				InGameManager.SingletonInstance.IsGamePlayReady = false;
				SetPlayerSpawnPos(collider, spawnPos);
				ScreenUIManagerPresenter.SingletonInstance.ShowLoadingPanel();
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
}