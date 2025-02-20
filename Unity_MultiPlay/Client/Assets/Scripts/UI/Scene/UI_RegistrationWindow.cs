using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_RegistrationWindow : UI_Base
{
	[SerializeField]
	InputField InputUserName;

	[SerializeField]
	InputField InputPassword;

	[SerializeField]
	InputField InputConfirmPassword;

	[SerializeField]
	InputField InputEmail;

	[SerializeField]
	Button BtnRegistration;

	[SerializeField]
	Button BtnCancel;

	public override void AwakeInit()
	{
		
	}

	private UI_LoginWindow GetLoginWindow()
	{
		if (Managers.UI.SceneUI is UI_LoginScene loginSceneUI)
		{
			return loginSceneUI.LoginWindow;
		}
		return null;
	}

	public void OnClickRegistration()
	{
		string accountName = InputUserName.text;
		string password = InputPassword.text;

		WebRequest.SendCreateAccount(accountName, password, (res)=>
		{
			if(res.CreateOk == true)
			{
				InputUserName.text = "";
				InputPassword.text = "";
				ChangeToLoginWindow();
			}
			else
			{
				Debug.Log($"사용 할 수 없는 accountName 입니다. :{accountName}");
			}
		});
	}

	public void OnClickCancle()
	{
		ChangeToLoginWindow();
	}

	public void ChangeToLoginWindow()
	{
		if (Managers.UI.SceneUI is UI_LoginScene loginSceneUI)
		{
			loginSceneUI.OpenLoginWindow();
		}
	}

}
