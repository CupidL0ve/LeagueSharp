using LeagueSharp;
using System;
namespace Riven
{
	internal class Buffmanager
	{
		public static int qStacks;
		public static int aaStacks;
		public static bool windSlashReady;
		public static bool ROn;
		static Buffmanager()
		{
            Game.OnGameUpdate += Game_OnGameUpdate;
		}
		private static void Game_OnGameUpdate(EventArgs args)
		{
			Buffmanager.ROn = Buffmanager.RIsOn();
			BuffInstance[] buffs = ObjectManager.Player.Buffs;
			BuffInstance[] array = buffs;
			for (int i = 0; i < array.Length; i++)
			{
				BuffInstance buffInstance = array[i];
				if (buffInstance.Name == "RivenTriCleave")
				{
					Buffmanager.qStacks = buffInstance.Count;
				}
				if (buffInstance.Name == "rivenpassiveaaboost")
				{
					Buffmanager.aaStacks = buffInstance.Count;
				}
				if (buffInstance.Name == "rivenwindslashready")
				{
					Buffmanager.windSlashReady = true;
				}
			}
			if (!Buffmanager.hasQStacks())
			{
				Buffmanager.qStacks = 0;
			}
			if (!Buffmanager.hasWindSlash())
			{
				Buffmanager.windSlashReady = false;
			}
		}
		public static bool hasQStacks()
		{
			BuffInstance[] buffs = ObjectManager.Player.Buffs;
			BuffInstance[] array = buffs;
			for (int i = 0; i < array.Length; i++)
			{
				BuffInstance buffInstance = array[i];
				if (buffInstance.Name == "RivenTriCleave")
				{
					return true;
				}
			}
			return false;
		}
		private static bool hasWindSlash()
		{
			BuffInstance[] buffs = ObjectManager.Player.Buffs;
			BuffInstance[] array = buffs;
			for (int i = 0; i < array.Length; i++)
			{
				BuffInstance buffInstance = array[i];
				if (buffInstance.Name == "rivenwindslashready")
				{
					return true;
				}
			}
			return false;
		}
		private static bool RIsOn()
		{
			BuffInstance[] buffs = ObjectManager.Player.Buffs;
			BuffInstance[] array = buffs;
			for (int i = 0; i < array.Length; i++)
			{
				BuffInstance buffInstance = array[i];
				if (buffInstance.Name == "RivenFengShuiEngine")
				{
					return true;
				}
			}
			return false;
		}
	}
}
