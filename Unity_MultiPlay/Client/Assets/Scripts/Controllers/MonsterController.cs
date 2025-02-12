using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;
using static Define;

public class MonsterController : CreatureController
{
	Coroutine _coSkill;

	protected override void Init()
	{
		base.Init();
	}

	protected override void UpdateIdle()
	{
		base.UpdateIdle();
	}

	public override void OnDamaged(CreatureController attacker, int damage)
	{
		base.OnDamaged(attacker, damage);
		//Managers.Object.Remove(Id);
	}

    public override void UseSkill(int skillId)
    {
		State = CreatureState.Skill;
		switch (skillId)
        {
            case 1:
                {
					
				}
                break;
            case 2:
                {

                }
                break;

        }
    }

}
