using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;//ローカライゼーションをスクリプトから使用する為のインクルード
using TMPro;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;

public class LocalizationImage : MonoBehaviour
{
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
        //LocalizationImageSystem();

        StartCoroutine("LocalizationAssetSystem");
    }

    IEnumerator LocalizationAssetSystem()
    {
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
        };
        //↓
        //ロードしたアセットを反映させる
        yield return null;
    }

    /// <summary>
    /// このやり方だとUpdate関数で国を切り替えた際にエラーになる
    /// </summary>
    void LocalizationImageSystem()
    {
        var entry = LocalizationSettings.AssetDatabase.GetLocalizedAsset<Sprite>("KashiwabaraTable", "Flag");
        Debug.Log("<color=red>" + entry + "</color>");
        image.sprite = entry;
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
