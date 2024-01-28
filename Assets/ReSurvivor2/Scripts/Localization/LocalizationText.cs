using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;//ローカライゼーションをスクリプトから使用する為のインクルード
using TMPro;
using System.Linq;

public class LocalizationText : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;

    IEnumerator Start()
    {
        //ローカライゼーションを初期化する（これをしないと国識別を変更することができずエラーになる）
        yield return LocalizationSettings.InitializationOperation;

        SwitchLocale("en");//英語に変更する
        //SwitchLocale("es");//スペイン語に変更する
    }

    void Update()
    {
        LocalizationTextSystem();
    }


    /// <summary>
    /// TextTableに記載してあるキーがCountryの各言語の文字列を取得する
    /// https://www.hanachiru-blog.com/entry/2022/03/14/120000
    /// </summary>
    void LocalizationTextSystem()
    {
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
