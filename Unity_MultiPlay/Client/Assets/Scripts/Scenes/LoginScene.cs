using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginScene : BaseScene
{
	UI_LoginScene sceneUI;

	protected override void Init()
	{
		base.Init();

		SceneType = Define.Scene.Login;

		Screen.SetResolution(640, 480, false);

		sceneUI = Managers.UI.ShowSceneUI<UI_LoginScene>();

	}

	public override void Clear()
	{

	}

	public void TestLog()
	{
		Debug.Log("Click Button");
	}
}
