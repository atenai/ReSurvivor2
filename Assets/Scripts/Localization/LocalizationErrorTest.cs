using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;//ローカライゼーションをスクリプトから使用する為のインクルード
using TMPro;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;

public class LocalizationErrorTest : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;

    [SerializeField] Image image;
    [SerializeField] TextMeshProUGUI pathText;



    IEnumerator Start()
    {
        //ローカライゼーションを初期化する（これをしないと国識別を変更することができずエラーになる）
        yield return LocalizationSettings.InitializationOperation;

        //SwitchLocale("en");//英語に変更する
        SwitchLocale("es");//スペイン語に変更する
    }

    void Update()
    {
        //LocalizationAssetSystem();//なぜかこれを1番目にしないとエラーになる
        //LocalizationTextSystem();//なぜかこれを2番目にしないとエラーになる

        //反対にしてもコルーチンで数秒待たせたら上手くいった、つまりロード時間が終わるまで待つ処理が必要（というか毎度アップデートでロードしていたらメモリが死ぬからここの順番問題は深く考えなくていいかも）
        StartCoroutine("LocalizationTextSystem");
        StartCoroutine("LocalizationAssetSystem");
    }

    IEnumerator LocalizationAssetSystem()
    {
        //yield return new WaitForSeconds(1.0f);
        bool flg = false;

        //動的にimage画像を国ごとに替えたい場合
        //↓
        //アドレッサブルアセットに画像を登録しておく
        //↓
        //登録した画像のパスの文字列をローカライズシステムの国ごとにパスを入れておく
        //↓
        //国を切り替えたらパスの文字列を取得する
        string entry = LocalizationSettings.StringDatabase.GetLocalizedString("AssetPathTable", "Flag");
        Debug.Log("<color=red>" + entry + "</color>");
        pathText.text = entry;
        //↓
        //取得したパスの文字列が前のパスの文字列と違う場合はアドレッサブルアセットからそのパスの文字列を元にアセットをロードする
        // アドレスを指定して読み込む
        Addressables.LoadAssetAsync<Sprite>(entry).Completed += op =>
        {
            image.sprite = op.Result;
            flg = true;
            Debug.Log("<color=yellow>" + "アセットのロードが完了" + "</color>");
        };
        //↓
        //ロードしたアセットを反映させる

        yield return new WaitUntil(() => flg == true); // flg が true になるまで処理が止まる
    }

    IEnumerator LocalizationTextSystem()
    {
        yield return new WaitForSeconds(1.0f);

        const string tableName = "TextTable";//使用するテーブル名
        const string entryKey = "Country";//テーブル内に記載されているキー

        // ローカライズされた文字列を取得
        string entry = LocalizationSettings.StringDatabase.GetLocalizedString(tableReference: tableName, tableEntryReference: entryKey);
        Debug.Log("<color=red>" + entry + "</color>");

        text.text = entry;
    }

    /// <summary>
    /// 国識別を変更する（引数には英語の国識別コードの文字列を入れる）
    /// </summary>
    void SwitchLocale(string countryCode)
    {
        var locale = LocalizationSettings.AvailableLocales.Locales.FirstOrDefault(e => e.Identifier.Code == countryCode);
        LocalizationSettings.SelectedLocale = locale;
    }
}
