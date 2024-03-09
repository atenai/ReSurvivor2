using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    //シングルトンで作成（ゲーム中に１つのみにする）
    public static UI singletonInstance = null;
    [SerializeField] Image imageCrosshair;
    [SerializeField] public GameObject panel_Loading;

    void Awake()
    {
        //staticな変数instanceはメモリ領域は確保されていますが、初回では中身が入っていないので、中身を入れます。
        if (singletonInstance == null)
        {
            singletonInstance = this;//thisというのは自分自身のインスタンスという意味になります。この場合、UIのインスタンスという意味になります。
            DontDestroyOnLoad(this.gameObject);//シーンを切り替えた時に破棄しない
        }
        else
        {
            Destroy(this.gameObject);//中身がすでに入っていた場合、自身のインスタンスがくっついているゲームオブジェクトを破棄します。
        }
    }

    void Start()
    {

    }

    void Update()
    {
        if (Player.SingletonInstance.IsAim == false)
        {
            imageCrosshair.gameObject.SetActive(false);
        }
        else
        {
            imageCrosshair.gameObject.SetActive(true);

            if (PlayerCamera.singletonInstance.IsTargethit == true)
            {
                imageCrosshair.color = new Color32(255, 0, 0, 150);
            }
            else
            {
                imageCrosshair.color = new Color32(255, 255, 255, 150);
            }
        }
    }
}
