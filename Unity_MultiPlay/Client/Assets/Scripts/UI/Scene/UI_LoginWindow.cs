using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_LoginWindow : UI_Base
{
	[SerializeField]
	InputField InputUserName;

	[SerializeField]
	InputField InputPassword;

	[SerializeField]
	Button BtnRegistration;

	[SerializeField]
	Button BtnLogin;

	public override void AwakeInit()
	{
		
	}

	public void OnClickLogin()
	{
		string accountName = InputUserName.text;
		string password = InputPassword.text;

		WebRequest.SendLoginAccount(accountName, password, (res) =>
		{
			if (res.LoginOk == true)
			{
				//SceneManager.LoadSceneAsync("Game",LoadSceneMode.Single);
				//TODO
			}
			else
			{
				Debug.Log($"사용 할 수 없는 accountName 입니다. :{accountName}");
			}
		});
	}

	public void OnClickRegistration()
	{
		if (Managers.UI.SceneUI is UI_LoginScene loginSceneUI)
		{
			loginSceneUI.OpenRegistWindow();
		}
	}
}
