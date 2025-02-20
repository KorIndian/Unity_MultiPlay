using System.Collections.Generic;
using UnityEngine;

public class UI_LoginScene : UI_Scene
{
	public UI_LoginWindow LoginWindow { get; private set; }
	public UI_RegistrationWindow RegistrationWindow { get; private set; }

	public override void AwakeInit()
	{
		base.AwakeInit();

		LoginWindow = GetComponentInChildren<UI_LoginWindow>(true);
		LoginWindow.gameObject.SetActive(true);

		RegistrationWindow = GetComponentInChildren<UI_RegistrationWindow>(true);
		RegistrationWindow.gameObject.SetActive(false);
	}

	public void OpenLoginWindow()
	{
		LoginWindow.gameObject.SetActive(true);
		RegistrationWindow.gameObject.SetActive(false);
	}

	public void OpenRegistWindow()
	{
		LoginWindow.gameObject.SetActive(false);
		RegistrationWindow.gameObject.SetActive(true);
	}
}
