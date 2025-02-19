using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScene : BaseScene
{
    UI_GameScene sceneUI;

    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Game;

        //temp test
        WebRequest.SendCreateAccount("donny", "5680");


        Managers.Map.LoadMap(1);

        Screen.SetResolution(640, 480, false);

        sceneUI = Managers.UI.ShowSceneUI<UI_GameScene>();//엄밀히 말하면Instantiate에 가깝다.

        Managers.Inventory.SetInventoryUI(sceneUI.InvenUI);
	}

    public override void Clear()
    {
        
    }
}
