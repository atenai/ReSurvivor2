using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class SwitchCinemachineVirtualCamera : MonoBehaviour
{
	[SerializeField]
	[Tooltip("切り替え後のカメラ")]
	private CinemachineVirtualCamera virtualCamera;
	// 切り替え後のカメラの元々のPriorityを保持しておく
	private int defaultPriority;

	void Start()
	{
		defaultPriority = virtualCamera.Priority;
	}

	private void OnTriggerEnter(Collider collider)
	{
		// 当たった相手に"Player"タグが付いていた場合
		if (collider.gameObject.CompareTag("Player"))
		{
			PlayerCamera.SingletonInstance.IsCinemachineActive = true;
			//PlayerCamera.SingletonInstance.CinemachineBrain.enabled = true;
			StartCoroutine(ChangePriority());
		}
	}

	IEnumerator ChangePriority()
	{
		//1フレーム停止
		yield return null;

		// 他のvirtualCameraよりも高い優先度にすることで切り替わる
		virtualCamera.Priority = 150;
	}

	private void OnTriggerExit(Collider collider)
	{
		// 当たった相手に"Player"タグが付いていた場合
		if (collider.gameObject.CompareTag("Player"))
		{
			PlayerCamera.SingletonInstance.IsCinemachineActive = false;
			//PlayerCamera.SingletonInstance.CinemachineBrain.enabled = false;
			// 元のpriorityに戻す
			virtualCamera.Priority = defaultPriority;
		}
	}
}
