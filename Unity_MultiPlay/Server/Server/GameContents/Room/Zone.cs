using Google.Protobuf.Protocol;
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

	object _lock = new object();

	public Zone(int YIndex, int XIndex)
	{
		IndexY = YIndex;
		IndexX = XIndex;
	}

	public List<Player> GetPlayersSafe()
	{
		List<Player> players = new List<Player>();
		lock (_lock)
		{
			players.AddRange(Players);
		}
		return players;
	}

	public List<Monster> GetMonstersSafe()
	{
		List<Monster> monsters = new List<Monster>();
		lock (_lock)
		{
			monsters.AddRange(Monsters.ToList());
		}
		return monsters;
	}

	public List<Projectile> GetProjectilesSafe()
	{
		List<Projectile> projectiles = new List<Projectile>();
		lock (_lock)
		{
			projectiles.AddRange(Projectiles.ToList());
		}
		return projectiles;
	}

	public bool AddObject(GameObject gameObject)
	{
		GameObjectType type = ObjectManager.GetObjectTypeById(gameObject.ObjectId);
		switch (type)
		{
			case GameObjectType.Player:
				return AddPlayer((Player)gameObject);
			case GameObjectType.Monster:
				return AddMonster((Monster)gameObject);
			case GameObjectType.Projectile:
				return AddProjectile((Projectile)gameObject);
		}
		return false;
	}

	public bool RemoveObject(GameObject gameObject)
	{
		GameObjectType type = ObjectManager.GetObjectTypeById(gameObject.ObjectId);
		switch (type)
		{
			case GameObjectType.Player:
				return RemovePlayer((Player)gameObject);
			case GameObjectType.Monster:
				return RemoveMonster((Monster)gameObject);
			case GameObjectType.Projectile:
				return RemoveProjectile((Projectile)gameObject);
		}
		return false;
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
