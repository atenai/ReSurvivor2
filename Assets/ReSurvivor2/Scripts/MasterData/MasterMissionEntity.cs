/// <summary>
/// 読みこんだエクセルの数値の型を作成するクラス（１番目）
/// </summary>
[System.Serializable] // このアトリビュートの追加必要
public class MasterMissionEntity
{
	//publicでExcelデータの1行目と同じパラメータ
	//また名前をまったく同じにしないといけない
	//注:Excelファイルの1行目の名称は、Entityクラスのフィールド名と紐づいているため、どちらかを変更した場合はもう片方を合わせる必要があります。ちなみに並び順には影響されません。
	public InGameManager.ComputerTYPE StartComputerName;
	public InGameManager.ComputerTYPE TargetComputerName;
	public int Minute;
	public int Seconds;
	public int MissionNumber;
}
