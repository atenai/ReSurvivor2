using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// コンピューターメニューを管理するマネージャークラス
/// MVPパターンのPresenter担当
/// </summary>
public class ComputerMenuPresenter : MonoBehaviour
{
    [SerializeField] private ComputerMenuView view;
    public ComputerMenuView View => view;

    ComputerMenuModel model = new ComputerMenuModel();
    public ComputerMenuModel Model => model;

    void Start()
    {

    }

    void Update()
    {

    }
}
