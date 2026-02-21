using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

/// <summary>
/// アイテム取得ログ
/// </summary>
public class ItemOutPutLog : MonoBehaviour
{
	struct TextProperty
	{
		float alfa;
		public float Alfa // 透明度、0未満なら0にする
		{
			get
			{
				return alfa;
			}
			set
			{
				alfa = value < 0 ? 0 : value;
			}
		}
		public float ElapsedTime { get; set; } // ログが出力されてからの経過時間
	}
	TextProperty[] textProperty;

	[Tooltip("ログのテキスト")]
	[SerializeField] TextMeshProUGUI[] logText;
	[Tooltip("完全に透明になるまでにかかる時間(秒)")]
	[SerializeField] float fadeoutTime = 1f;
	[Tooltip("透明化が始まるまでにかかる時間(秒)")]
	[SerializeField] float fadeoutStartTime = 5f;

	void Start()
	{
		textProperty = new TextProperty[logText.Count()];
		for (int i = 0; i < logText.Count(); i++)
		{
			logText[i].color = new Color(logText[i].color.r, logText[i].color.g, logText[i].color.b, 0f);
			textProperty[i].Alfa = 0f;
			textProperty[i].ElapsedTime = 0f;
		}
	}

	void Update()
	{
		// 一番上のテキストは強制的に透明化開始させる
		if (textProperty[0].Alfa == 1)
		{
			textProperty[0].ElapsedTime = fadeoutStartTime;
		}

		for (int i = logText.Count() - 1; 0 <= i; i--)
		{
			if (0 < textProperty[i].Alfa)
			{
				// 経過時間がfadeoutStartTime未満なら時間をカウント
				// そうでなければ透明度を下げる
				if (textProperty[i].ElapsedTime < fadeoutStartTime)
				{
					textProperty[i].ElapsedTime += Time.deltaTime;
				}
				else
				{
					textProperty[i].Alfa -= Time.deltaTime / fadeoutTime;
					logText[i].color = new Color(logText[i].color.r, logText[i].color.g, logText[i].color.b, textProperty[i].Alfa);
				}
			}
			else
			{
				break;
			}
		}
	}

	/// <summary>
	/// ログ出力
	/// </summary>
	/// <param name="itemName">ここの文字列を変えればログの文章が変わります</param>
	public void OutputLog(string itemName)
	{
		if (textProperty[logText.Count() - 1].Alfa > 0)
		{
			UplogText();
		}
		logText[logText.Count() - 1].text = itemName;
		ResetTextPropety();
	}

	/// <summary>
	/// ログを一つ上にずらす
	/// </summary>
	void UplogText()
	{
		// 古いほうからずらす
		for (int i = 0; i < logText.Count() - 1; i++)
		{
			logText[i].text = logText[i + 1].text;
			textProperty[i].Alfa = textProperty[i + 1].Alfa;
			textProperty[i].ElapsedTime = textProperty[i + 1].ElapsedTime;
			logText[i].color = new Color(logText[i].color.r, logText[i].color.g, logText[i].color.b, textProperty[i].Alfa);
		}
	}

	/// <summary>
	/// ログの初期化
	/// </summary>
	void ResetTextPropety()
	{
		textProperty[logText.Count() - 1].Alfa = 1f;
		textProperty[logText.Count() - 1].ElapsedTime = 0f;
		logText[logText.Count() - 1].color = new Color(logText[logText.Count() - 1].color.r, logText[logText.Count() - 1].color.g, logText[logText.Count() - 1].color.b, textProperty[logText.Count() - 1].Alfa);
	}
}

