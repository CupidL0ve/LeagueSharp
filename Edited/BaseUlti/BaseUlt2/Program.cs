using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
namespace BaseUlt2
{
	internal class Program
	{
		private struct UltData
		{
			public float DamageMultiplicator;
			public float Delay;
			public float ManaCost;
			public float Range;
			public float Speed;
			public float Width;
		}
		private static Menu _menu;
		private static bool _compatibleChamp;
		private static IEnumerable<Obj_AI_Hero> _ownTeam;
		private static IEnumerable<Obj_AI_Hero> _enemyTeam;
		private static Vector3 _enemySpawnPos;
		private static List<PlayerInfo> _playerInfo = new List<PlayerInfo>();
		private static int _ultCasted;
		private static Spell _ult;
		public static Utility.Map.MapType Map;
		public static Dictionary<int, int> RecallT = new Dictionary<int, int>();
		private static readonly Dictionary<string, UltData> UltInfo = new Dictionary<string, UltData>
		{

			{
				"Jinx",
				new UltData
				{
					ManaCost = 100f,
					DamageMultiplicator = 1f,
					Width = 140f,
					Delay = 0.6f,
					Speed = 1700f,
					Range = 20000f
				}
			},

			{
				"Ashe",
				new UltData
				{
					ManaCost = 100f,
					DamageMultiplicator = 1f,
					Width = 130f,
					Delay = 0.25f,
					Speed = 1600f,
					Range = 20000f
				}
			},

			{
				"Draven",
				new UltData
				{
					ManaCost = 120f,
					DamageMultiplicator = 0.7f,
					Width = 160f,
					Delay = 0.4f,
					Speed = 2000f,
					Range = 20000f
				}
			},

			{
				"Ezreal",
				new UltData
				{
					ManaCost = 100f,
					DamageMultiplicator = 0.7f,
					Width = 160f,
					Delay = 1f,
					Speed = 2000f,
					Range = 20000f
				}
			},

			{
				"Karthus",
				new UltData
				{
					ManaCost = 150f,
					DamageMultiplicator = 1f,
					Width = 0f,
					Delay = 3.125f,
					Speed = 0f,
					Range = 20000f
				}
			}
		};
		private static void Main(string[] args)
		{
			Game.OnGameStart += new GameStart(Game_OnGameStart);
			if (Game.Mode == GameMode.Running)
			{
				Game_OnGameStart(new EventArgs());
			}
		}
		private static void Game_OnGameStart(EventArgs args)
		{
			(_menu = new Menu("BaseUlt2", "BaseUlt", true)).AddToMainMenu();
			_menu.AddItem(new MenuItem("showRecalls", "Show Recalls").SetValue<bool>(true));
			_menu.AddItem(new MenuItem("baseUlt", "Base Ult").SetValue<bool>(true));
			_menu.AddItem(new MenuItem("extraDelay", "Extra Delay").SetValue<Slider>(new Slider(0, -2000, 2000)));
			_menu.AddItem(new MenuItem("panicKey", "Panic key (hold for disable)").SetValue<KeyBind>(new KeyBind(32u, KeyBindType.Press, false)));
			_menu.AddItem(new MenuItem("regardlessKey", "No timelimit (hold)").SetValue<KeyBind>(new KeyBind(17u, KeyBindType.Press, false)));
			_menu.AddItem(new MenuItem("debugMode", "Debug (developer only)").SetValue<bool>(false).DontSave());
			Menu menu = _menu.AddSubMenu(new Menu("Team Baseult Friends", "TeamUlt", false));
			List<Obj_AI_Hero> source = ObjectManager.Get<Obj_AI_Hero>().ToList<Obj_AI_Hero>();
			_ownTeam = 
				from x in source
				where x.IsAlly
				select x;
			_enemyTeam = 
				from x in source
				where x.IsEnemy
				select x;
			_compatibleChamp = Helper.IsCompatibleChamp(ObjectManager.Player.ChampionName);
			if (_compatibleChamp)
			{
				foreach (Obj_AI_Hero current in 
					from x in _ownTeam
					where !x.IsMe && Helper.IsCompatibleChamp(x.ChampionName)
					select x)
				{
					menu.AddItem(new MenuItem(current.ChampionName, current.ChampionName + " friend with Baseult?").SetValue<bool>(false).DontSave());
				}
			}
			_enemySpawnPos = ObjectManager.Get<GameObject>().First((GameObject x) => x.Type == GameObjectType.obj_SpawnPoint && x.Team != ObjectManager.Player.Team).Position;
			Map = Utility.Map.GetMap()._MapType;
			_playerInfo = (
				from x in _enemyTeam
				select new PlayerInfo(x)).ToList<PlayerInfo>();
			_playerInfo.Add(new PlayerInfo(ObjectManager.Player));
			_ult = new Spell(SpellSlot.R, 20000f);
			Game.OnGameProcessPacket += new GameProcessPacket(Game_OnGameProcessPacket);
			Drawing.OnDraw += new Draw(Drawing_OnDraw);
			if (_compatibleChamp)
			{
				Game.OnGameUpdate += new GameUpdate(Game_OnGameUpdate);
			}
			Game.PrintChat("<font color=\"#1eff00\">BaseUlt -</font> <font color=\"#00BFFF\">Loaded (compatible champ: " + (_compatibleChamp ? "Yes" : "No") + ")</font>");
		}
		private static void Game_OnGameUpdate(EventArgs args)
		{
			int tickCount = Environment.TickCount;
			foreach (PlayerInfo current in 
				from x in _playerInfo
				where x.Champ.IsVisible
				select x)
			{
				current.LastSeen = tickCount;
			}
			if (!_menu.Item("baseUlt").GetValue<bool>())
			{
				return;
			}
			foreach (PlayerInfo current2 in 
				from x in _playerInfo
				where x.Champ.IsValid && !x.Champ.IsDead && x.Champ.IsEnemy && x.Recall.Status == Packet.S2C.Recall.RecallStatus.RecallStarted
				orderby x.GetRecallEnd()
				select x)
			{
				if (_ultCasted == 0 || Environment.TickCount - _ultCasted > 20000)
				{
					HandleRecallShot(current2);
				}
			}
		}
		private static float GetUltManaCost(Obj_AI_Hero source)
		{
			float num = UltInfo[source.ChampionName].ManaCost;
			if (source.ChampionName == "Karthus")
			{
				if (source.Level >= 11)
				{
					num += 25f;
				}
				if (source.Level >= 16)
				{
					num += 25f;
				}
			}
			return num;
		}
		private static void HandleRecallShot(PlayerInfo playerInfo)
		{
			bool flag = false;
			foreach (Obj_AI_Hero current in 
				from x in _ownTeam
				where x.IsValid && (x.IsMe || Helper.GetSafeMenuItem<bool>(_menu.Item(x.ChampionName))) && !x.IsDead && !x.IsStunned && (x.Spellbook.CanUseSpell(SpellSlot.R) == SpellState.Ready || (x.Spellbook.GetSpell(SpellSlot.R).Level > 0 && x.Spellbook.CanUseSpell(SpellSlot.R) == SpellState.Surpressed && x.Mana >= GetUltManaCost(x)))
				select x)
			{
				if (!(current.ChampionName != "Ezreal") || !(current.ChampionName != "Karthus") || !Helper.IsCollidingWithChamps(current, _enemySpawnPos, UltInfo[current.ChampionName].Width))
				{
					float num = Helper.GetSpellTravelTime(current, UltInfo[current.ChampionName].Speed, UltInfo[current.ChampionName].Delay, _enemySpawnPos) - (float)(_menu.Item("extraDelay").GetValue<Slider>().Value + 65);
					if (num - (float)playerInfo.GetRecallCountdown() <= 60f)
					{
						playerInfo.IncomingDamage[current.NetworkId] = (float)Helper.GetUltDamage(current, playerInfo.Champ) * UltInfo[current.ChampionName].DamageMultiplicator;
						if ((float)playerInfo.GetRecallCountdown() <= num && current.IsMe)
						{
							flag = true;
						}
					}
				}
			}
			float num2 = playerInfo.IncomingDamage.Values.Sum();
			float targetHealth = Helper.GetTargetHealth(playerInfo);
			if (!flag || _menu.Item("panicKey").GetValue<KeyBind>().Active)
			{
				if (_menu.Item("debugMode").GetValue<bool>())
				{
					Game.PrintChat("!SHOOT/PANICKEY {0} (Health: {1} TOTAL-UltDamage: {2})", new object[]
					{
						playerInfo.Champ.ChampionName,
						targetHealth,
						num2
					});
				}
				return;
			}
			playerInfo.IncomingDamage.Clear();
			int tickCount = Environment.TickCount;
			if (tickCount - playerInfo.LastSeen > 20000 && !_menu.Item("regardlessKey").GetValue<KeyBind>().Active)
			{
				if (num2 < playerInfo.Champ.MaxHealth)
				{
					if (_menu.Item("debugMode").GetValue<bool>())
					{
						Game.PrintChat("DONT SHOOT, TOO LONG NO VISION {0} (Health: {1} TOTAL-UltDamage: {2})", new object[]
						{
							playerInfo.Champ.ChampionName,
							targetHealth,
							num2
						});
					}
					return;
				}
			}
			else
			{
				if (num2 < targetHealth)
				{
					if (_menu.Item("debugMode").GetValue<bool>())
					{
						Game.PrintChat("DONT SHOOT {0} (Health: {1} TOTAL-UltDamage: {2})", new object[]
						{
							playerInfo.Champ.ChampionName,
							targetHealth,
							num2
						});
					}
					return;
				}
			}
			if (_menu.Item("debugMode").GetValue<bool>())
			{
				Game.PrintChat("SHOOT {0} (Health: {1} TOTAL-UltDamage: {2})", new object[]
				{
					playerInfo.Champ.ChampionName,
					targetHealth,
					num2
				});
			}
			_ult.Cast(_enemySpawnPos, true);
			_ultCasted = tickCount;
		}
		private static void Drawing_OnDraw(EventArgs args)
		{
			if (!_menu.Item("showRecalls").GetValue<bool>())
			{
				return;
			}
			int num = -1;
			foreach (PlayerInfo current in 
				from x in _playerInfo
				where (x.Recall.Status == Packet.S2C.Recall.RecallStatus.RecallStarted || x.Recall.Status == Packet.S2C.Recall.RecallStatus.TeleportStart) && x.Champ.IsValid && !x.Champ.IsDead && x.GetRecallCountdown() > 0 && (x.Champ.IsEnemy || _menu.Item("debugMode").GetValue<bool>())
				orderby x.GetRecallEnd()
				select x)
			{
				num++;
				Drawing.DrawText((float)Drawing.Width * 0.73f, (float)Drawing.Height * 0.88f + (float)num * 15f, System.Drawing.Color.Red, current.ToString());
			}
		}
		private static void Game_OnGameProcessPacket(GamePacketEventArgs args)
		{
			if (args.PacketData[0] == Packet.S2C.Recall.Header)
			{
				Packet.S2C.Recall.Struct newRecall = Helper.RecallDecode(args.PacketData);
				PlayerInfo playerInfo = _playerInfo.Find((PlayerInfo x) => x.Champ.NetworkId == newRecall.UnitNetworkId).UpdateRecall(newRecall);
				if (_menu.Item("debugMode").GetValue<bool>())
				{
					Game.PrintChat(string.Concat(new object[]
					{
						playerInfo.Champ.ChampionName,
						": ",
						playerInfo.Recall.Status,
						" duration: ",
						playerInfo.Recall.Duration,
						" guessed health: ",
						Helper.GetTargetHealth(playerInfo),
						" lastseen: ",
						playerInfo.LastSeen,
						" health: ",
						playerInfo.Champ.Health,
						" own-ultdamage: ",
						(float)Helper.GetUltDamage(ObjectManager.Player, playerInfo.Champ) * UltInfo[ObjectManager.Player.ChampionName].DamageMultiplicator
					}));
				}
			}
		}
	}
}
