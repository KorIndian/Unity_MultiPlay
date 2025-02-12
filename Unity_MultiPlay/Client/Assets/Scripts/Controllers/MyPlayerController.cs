using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.PlayerLoop;
using static Define;

public class MyPlayerController : PlayerController
{
	public int WeaponDamage { get; private set; } = 0;
	public int ArmorDefence { get; private set; } = 0;

	bool _moveKeyPressed = false;
    Coroutine _coInputCooltime = null;

	protected override void StartInit()
    {
        base.StartInit();
		ReCalcAdditionalStat();
	}

	protected override void UpdateKeyInput()
	{
		base.UpdateKeyInput();
		
        if (Input.GetKeyDown(KeyCode.I))
        {
			var InvenUI = Managers.Inventory.InventoryUI;
			if (InvenUI != null)
				InvenUI.ToggleUI();
		}
	}

	protected override void UpdateController()
    {
        switch (State)
        {
            case CreatureState.Idle:
                GetDirInput();
                break;
            case CreatureState.Moving:
                GetDirInput();
                break;
        }

        base.UpdateController();
    }

    protected override void UpdateIdle()
    {
        // 이동 상태로 갈지 확인
        if (_moveKeyPressed)
        {
            State = CreatureState.Moving;
            return;
        }

        // 스킬 상태로 갈지 확인
        if (_coInputCooltime == null && Input.GetKey(KeyCode.Space))
        {
            int skillid = 2;

            C_Skill skillPacket = new C_Skill() { Info = new SkillInfo() };
            skillPacket.Info.SkillId = skillid;
            Managers.Network.Send(skillPacket);

            _coInputCooltime = StartCoroutine(CoInputCooltime(0.2f));
        }
    }

    IEnumerator CoInputCooltime(float time)
    {
        yield return new WaitForSeconds(time);
        _coInputCooltime = null;
    }

    void LateUpdate()
    {
        Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
    }

    // 키보드 입력
    protected void GetDirInput()
    {
        _moveKeyPressed = true;

        if (Input.GetKey(KeyCode.W))
        {
            Dir = MoveDir.Up;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            Dir = MoveDir.Down;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            Dir = MoveDir.Left;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            Dir = MoveDir.Right;
        }
        else
        {
            _moveKeyPressed = false;
        }
    }

    //거의 도착했을 쯤 1,2회 불려지고 정수형 그리드좌표를 확정적으로 바꿔주는 함수이다. 
    protected override void MoveToNextPos()//따라서 이동패킷을 보내기에 가장 적잘타이밍이된다.
    {
        Debug.Log("MoveToNextPos()!!");
        if (_moveKeyPressed == false)
        {
            State = CreatureState.Idle;
            CheckUpdatedSyncMoveStatus();
            return;
        }

        Vector3Int destPos = Vector3Int.zero;

        switch (Dir)
        {
            case MoveDir.Up:
                destPos = CellPos + Vector3Int.up;
                break;
            case MoveDir.Down:
                destPos = CellPos + Vector3Int.down;
                break;
            case MoveDir.Left:
                destPos = CellPos + Vector3Int.left;
                break;
            case MoveDir.Right:
                destPos = CellPos + Vector3Int.right;
                break;
        }

        if (Managers.Map.CanGo(destPos))
        {
            if (Managers.Object.FindCreature(destPos) == null)
            {
                CellPos = destPos;
            }
        }
        CheckUpdatedSyncMoveStatus();
    }

    protected override void CheckUpdatedSyncMoveStatus()
    {
        if (_updated)
        {
            C_Move movePacket = new C_Move();
            movePacket.PosInfo = PosInfo;
            Managers.Network.Send(movePacket);
            _updated = false;
        }
    }

    public override void OnDamaged(CreatureController attacker, int damage)
    {
        base.OnDamaged(attacker, damage);

    }

	public void ReCalcAdditionalStat()
	{
		WeaponDamage = 0;
		ArmorDefence = 0;

		foreach (Item item in Managers.Inventory.Items.Values)
		{
			if (item.Equipped == false)
				continue;

			switch (item.ItemType)
			{
				case ItemType.Weapon:
					WeaponDamage += ((Weapon)item).Damage;
					break;
				case ItemType.Armor:
					ArmorDefence += ((Armor)item).Defence;
					break;
			}

		}
	}
}
