using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 読みこんだエクセルをスクリプトで使えるクラスの作成（２番目）
/// </summary>

//注:
//上記ExcelAssetスクリプトのフィールド名、「Entities」の部分はExcelファイル内のシート名と紐づいています。シート名を変更した場合はこちらの定義も変更する必要があります。
//またクラス・スクリプトファイル名の「MstItems」はExcelファイル名と紐づいています。こちらも、Excelファイルの名称を変更した場合は追従して変更する必要があります。
[ExcelAsset]
public class MasterMission : ScriptableObject
{
	public List<MasterMissionEntity> Sheet1; // シート名とフィールド名を合わせる
}
