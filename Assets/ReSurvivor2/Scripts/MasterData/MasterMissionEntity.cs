/// <summary>
/// 読みこんだエクセルの数値の型を作成するクラス（１番目）
/// </summary>
[System.Serializable] // このアトリビュートの追加必要
public class MasterMissionEntity
{
	//publicでExcelデータの1行目と同じパラメータ
	//また名前をまったく同じにしないといけない
	//注:Excelファイルの1行目の名称は、Entityクラスのフィールド名と紐づいているため、どちらかを変更した場合はもう片方を合わせる必要があります。ちなみに並び順には影響されません。

	/// <summary>
	/// ミッションID
	/// </summary>
	public int MissionID;
	/// <summary>
	/// ミッションのスタートコンピューター名
	/// </summary>
	public EnumManager.ComputerTYPE StartComputerName;
	/// <summary>
	/// ミッションのエンドコンピューター名
	/// </summary>
	public EnumManager.ComputerTYPE EndComputerName;
	/// <summary>
	/// 分：カウントダウンタイマー用
	/// </summary>
	public int Minute;
	/// <summary>
	/// 秒：カウントダウンタイマー用
	/// </summary>
	public int Seconds;
	/// <summary>
	/// ミッション名
	/// </summary>
	public string MissionName;
	/// <summary>
	/// メールのタイトルメッセージ
	/// </summary>
	public string MailTitleMessage;
	/// <summary>
	/// メールのメインメッセージ
	/// </summary>	
	public string MailMainMessage;
}
