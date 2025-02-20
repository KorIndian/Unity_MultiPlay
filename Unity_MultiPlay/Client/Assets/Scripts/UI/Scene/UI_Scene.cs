using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Scene : UI_Base
{
	public override void AwakeInit()
	{
		Managers.UI.SetCanvas(gameObject, false);
		//SetCanvas함수 내부에서 SortOrder를 명시적으로 Init이 불려진 순서대로
		//1씩 증가시킨 후 set해주고 있다.
		//sort를 false로 넣었을 경우에는, sortorder를 0으로 만들어서
		//가장밑에 오게한다.
	}

}
