using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// コンピューターメニュー
/// </summary>
public class ComputerMenu : MonoBehaviour
{
	[SerializeField] Image image;
	public Image Image => image;
	[SerializeField] TextMeshProUGUI textMeshProUGUI;

	public void Initialize(string missionName)
	{
		textMeshProUGUI.text = missionName;
	}
}
