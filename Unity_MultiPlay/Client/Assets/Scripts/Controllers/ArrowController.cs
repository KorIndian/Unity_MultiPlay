using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;
using static Define;

public class ArrowController : BaseController
{
	protected override void StartInit()
	{
		switch (Dir)
		{
			case MoveDir.Up:
				transform.rotation = Quaternion.Euler(0, 0, 0);
				break;
			case MoveDir.Down:
				transform.rotation = Quaternion.Euler(0, 0, -180);
				break;
			case MoveDir.Left:
				transform.rotation = Quaternion.Euler(0, 0, 90);
				break;
			case MoveDir.Right:
				transform.rotation = Quaternion.Euler(0, 0, -90);
				break;
		}

		State = CreatureState.Moving;
		base.StartInit();
	}

	protected override void UpdateAnimation()
	{

	}
    //MoveToNextPos() 기존에 클라이언트에서 자체적으로 화살을 움직이던 코드는 지워주어야한다.
    //왜냐하면 투사체의 시뮬레이팅도 서버에서 한다음 move패킷을 받아서 처리하게 될 것이기 때문이다.
    
}
