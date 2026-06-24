using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

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
				isLoadOnce = true;
				//ロード中にプレイヤーが移動してロードトリガーに触り連続ロードを行わないようにするための処理
				InGameManager.SingletonInstance.IsGamePlayReady = false;
				SetPlayerSpawnPos(collider, spawnPos);
				StartCoroutine(LoadScene());
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
	/// シーンをロードする
	/// </summary>
	IEnumerator LoadScene()
	{
		//不透明にする
		InGameManager.SingletonInstance.ScreenUIManager.FadeOut();

		//スライダーの値を最低にする
		InGameManager.SingletonInstance.ScreenUIManager.SliderLoading.value = float.MinValue;
		//ロードUIをOnにする
		InGameManager.SingletonInstance.ScreenUIManager.PanelLoading.SetActive(true);

		//シーンをロード
		AsyncOperation async = SceneManager.LoadSceneAsync(nextStage.ToString());
		//シーンが勝手に切り替わらないようにする
		async.allowSceneActivation = false;
		//シーンをロードするまでのループ処理
		while (async.isDone == false)
		{
			//ロード数値をスライダーに反映
			InGameManager.SingletonInstance.ScreenUIManager.SliderLoading.value = async.progress;
			Debug.Log("<color=red>読み込み進捗: " + async.progress * 100 + "%</color>");

			//ロード数値が0.9より同じかそれ以上大きくなったら中身を実行する
			if (0.9f <= async.progress)
			{
				//スライダーの値を最大にする
				InGameManager.SingletonInstance.ScreenUIManager.SliderLoading.value = float.MaxValue;

				//フレームのラストまで待つ
				yield return new WaitForEndOfFrame();

				//ロードUIをOffにする
				InGameManager.SingletonInstance.ScreenUIManager.PanelLoading.SetActive(false);

				//シーンを切り替える
				async.allowSceneActivation = true;
			}

			//1フレーム待つ
			yield return null;
		}
	}
}