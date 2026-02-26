using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

/// <summary>
/// CinemachineVirtualCameraを切り替えるスクリプト
/// </summary>
public class SwitchCinemachineVirtualCamera : MonoBehaviour
{
	[Tooltip("切り替え後のカメラ")]
	[SerializeField] CinemachineVirtualCamera virtualCamera;
	// 切り替え後のカメラの元々のPriorityを保持しておく
	int defaultPriority;

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
			// 他のvirtualCameraよりも高い優先度にすることで切り替わる
			virtualCamera.Priority = 300;
		}
	}

	private void OnTriggerExit(Collider collider)
	{
		// 当たった相手に"Player"タグが付いていた場合
		if (collider.gameObject.CompareTag("Player"))
		{
			PlayerCamera.SingletonInstance.IsCinemachineActive = false;
			// 元のpriorityに戻す
			virtualCamera.Priority = defaultPriority;
		}
	}
}
