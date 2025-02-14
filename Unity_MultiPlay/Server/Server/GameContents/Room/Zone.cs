using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Server.GameContents;

//Room하나를 쪼개는 기본단위 
public class Zone
{
	public int IndexY { get; private set; }
	public int IndexX { get; private set; }

	
	public HashSet<Player> Players = new HashSet<Player>();
	public HashSet<Monster> Monsters { get; set; } = new HashSet<Monster>();
	public HashSet<Projectile> Projectiles { get; set; } = new HashSet<Projectile>();

	public HashSet<GameObject> Objects { get; set; } = new HashSet<GameObject>();


	public Zone(int YIndex, int XIndex)
	{
		IndexY = YIndex;
		IndexX = XIndex;
	}

	public bool AddObject(GameObject gameObject)
	{
		return Objects.Add(gameObject);
	}

	public bool RemoveObject(GameObject gameObject)
	{
		return Objects.Remove(gameObject);
	}

	public bool AddPlayer(Player player)
	{
		return Players.Add(player);
	}

	public bool RemovePlayer(Player player)
	{
		return Players.Remove(player);
	}

	public bool AddMonster(Monster monster)
	{
		return Monsters.Add(monster);
	}

	public bool RemoveMonster(Monster monster)
	{
		return Monsters.Remove(monster);
	}

	public bool AddProjectile(Projectile projectile)
	{
		return Projectiles.Add(projectile);
	}

	public bool RemoveProjectile(Projectile projectile)
	{
		return Projectiles.Remove(projectile);
	}

	public Player FindPlayer(Func<Player, bool> condition)
	{
		foreach (Player p in Players)
		{
			if (condition.Invoke(p))
				return p;
		}

		return null;
	}

	public List<Player> FindPlayers(Func<Player, bool> condition)
	{
		List<Player> players = new List<Player>();
		foreach (Player p in Players)
		{
			if (condition.Invoke(p))
				players.Add(p);
		}

		return players;
	}
}
