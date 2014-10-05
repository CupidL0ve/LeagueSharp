using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace BaseUlt2
{
	internal class Helper
	{
		public static T GetSafeMenuItem<T>(MenuItem item)
		{
			if (item != null)
			{
				return item.GetValue<T>();
			}
			return default(T);
		}
		public static float GetTargetHealth(PlayerInfo playerInfo)
		{
			if (playerInfo.Champ.IsVisible)
			{
				return playerInfo.Champ.Health;
			}
			float num = playerInfo.Champ.Health + playerInfo.Champ.HPRegenRate * ((float)(Environment.TickCount - playerInfo.LastSeen + playerInfo.GetRecallCountdown()) / 1000f);
			if (num <= playerInfo.Champ.MaxHealth)
			{
				return num;
			}
			return playerInfo.Champ.MaxHealth;
		}
		public static float GetSpellTravelTime(Obj_AI_Hero source, float speed, float delay, Vector3 targetpos)
		{
			if (source.ChampionName == "Karthus")
			{
				return delay * 1000f;
			}
			float num = Vector3.Distance(source.ServerPosition, targetpos);
			float num2 = speed;
			if (source.ChampionName == "Jinx" && num > 1350f)
			{
				float num3 = 0.3f;
				float num4 = num - 1350f;
				if (num4 > 150f)
				{
					num4 = 150f;
				}
				float num5 = num - 1500f;
				num2 = (1350f * speed + num4 * (speed + num3 * num4) + num5 * 2200f) / num;
			}
			return (num / num2 + delay) * 1000f;
		}
		public static bool IsCollidingWithChamps(Obj_AI_Hero source, Vector3 targetpos, float width)
		{
			PredictionInput predictionInput = new PredictionInput
			{
				Radius = width,
				Unit = source
			};
			predictionInput.CollisionObjects[0] = CollisionableObjects.Heroes;
			return LeagueSharp.Common.Collision.GetCollision(new List<Vector3>
			{
				targetpos
			}, predictionInput).Any<Obj_AI_Base>();
		}
		public static Packet.S2C.Recall.Struct RecallDecode(byte[] data)
		{
			BinaryReader binaryReader = new BinaryReader(new MemoryStream(data));
			Packet.S2C.Recall.Struct result = default(Packet.S2C.Recall.Struct);
			binaryReader.ReadByte();
			binaryReader.ReadInt32();
			result.UnitNetworkId = binaryReader.ReadInt32();
			binaryReader.ReadBytes(66);
			result.Status = Packet.S2C.Recall.RecallStatus.Unknown;
			bool flag = false;
			if (BitConverter.ToString(binaryReader.ReadBytes(6)) != "00-00-00-00-00-00")
			{
				if (BitConverter.ToString(binaryReader.ReadBytes(3)) != "00-00-00")
				{
					result.Status = Packet.S2C.Recall.RecallStatus.TeleportStart;
					flag = true;
				}
				else
				{
					result.Status = Packet.S2C.Recall.RecallStatus.RecallStarted;
				}
			}
			binaryReader.Close();
			Obj_AI_Hero unitByNetworkId = ObjectManager.GetUnitByNetworkId<Obj_AI_Hero>(result.UnitNetworkId);
			if (unitByNetworkId != null)
			{
				if (flag)
				{
					result.Duration = 3500;
				}
				else
				{
					result.Duration = ((Program.Map == Utility.Map.MapType.CrystalScar) ? 4500 : 8000);
					if (unitByNetworkId.Masteries.Any((Mastery x) => x.Page == MasteryPage.Utility && x.Id == 65 && x.Points == 1))
					{
						result.Duration -= ((Program.Map == Utility.Map.MapType.CrystalScar) ? 500 : 1000);
					}
				}
				int num = Environment.TickCount - Game.Ping;
				if (!Program.RecallT.ContainsKey(result.UnitNetworkId))
				{
					Program.RecallT.Add(result.UnitNetworkId, num);
				}
				else
				{
					if (Program.RecallT[result.UnitNetworkId] == 0)
					{
						Program.RecallT[result.UnitNetworkId] = num;
					}
					else
					{
						if (num - Program.RecallT[result.UnitNetworkId] > result.Duration - 75)
						{
							result.Status = (flag ? Packet.S2C.Recall.RecallStatus.TeleportEnd : Packet.S2C.Recall.RecallStatus.RecallFinished);
						}
						else
						{
							result.Status = (flag ? Packet.S2C.Recall.RecallStatus.TeleportAbort : Packet.S2C.Recall.RecallStatus.RecallAborted);
						}
						Program.RecallT[result.UnitNetworkId] = 0;
					}
				}
			}
			return result;
		}
		public static bool IsCompatibleChamp(string championName)
		{
			return championName != null && (championName == "Ashe" || championName == "Ezreal" || championName == "Draven" || championName == "Jinx" || championName == "Karthus");
		}
		public static double GetUltDamage(Obj_AI_Hero source, Obj_AI_Hero enemy)
		{
			string championName;
			if ((championName = source.ChampionName) != null)
			{
				if (championName == "Ashe")
				{
					return Helper.CalcMagicDmg((double)(75 + source.Spellbook.GetSpell(SpellSlot.R).Level * 175) + 1.0 * (double)source.FlatMagicDamageMod, source, enemy);
				}
				if (championName == "Draven")
				{
					return Helper.CalcPhysicalDmg((double)(75 + source.Spellbook.GetSpell(SpellSlot.R).Level * 100) + 1.1 * (double)source.FlatPhysicalDamageMod, source, enemy);
				}
				if (championName == "Jinx")
				{
					double num = Helper.CalcPhysicalDmg((double)((enemy.MaxHealth - enemy.Health) / 100f * (float)(20 + 5 * source.Spellbook.GetSpell(SpellSlot.R).Level)), source, enemy);
					return num + Helper.CalcPhysicalDmg((double)(150 + source.Spellbook.GetSpell(SpellSlot.R).Level * 100) + 1.0 * (double)source.FlatPhysicalDamageMod, source, enemy);
				}
				if (championName == "Ezreal")
				{
					return Helper.CalcMagicDmg((double)(200 + source.Spellbook.GetSpell(SpellSlot.R).Level * 150) + 1.0 * (double)(source.FlatPhysicalDamageMod + source.BaseAttackDamage) + 0.9 * (double)source.FlatMagicDamageMod, source, enemy);
				}
				if (championName == "Karthus")
				{
					return Helper.CalcMagicDmg((double)(100 + source.Spellbook.GetSpell(SpellSlot.R).Level * 150) + 0.6 * (double)source.FlatMagicDamageMod, source, enemy);
				}
			}
			return 0.0;
		}
		public static double CalcPhysicalDmg(double dmg, Obj_AI_Hero source, Obj_AI_Base enemy)
		{
			bool flag = false;
			bool flag2 = false;
			int num = 0;
			Mastery[] masteries = source.Masteries;
			for (int i = 0; i < masteries.Length; i++)
			{
				Mastery mastery = masteries[i];
				if (mastery.Page == MasteryPage.Offense)
				{
					byte id = mastery.Id;
					if (id <= 68)
					{
						if (id != 65)
						{
							if (id == 68)
							{
								byte arg_89_0 = mastery.Points;
							}
						}
						else
						{
							flag = (mastery.Points == 1);
						}
					}
					else
					{
						if (id != 100)
						{
							if (id != 132)
							{
								if (id == 146)
								{
									flag2 = (mastery.Points == 1);
								}
							}
							else
							{
								byte arg_77_0 = mastery.Points;
							}
						}
						else
						{
							num = (int)mastery.Points;
						}
					}
				}
			}
			double num2 = 0.0;
			if (flag)
			{
				if (source.CombatType == GameObjectCombatType.Melee)
				{
					num2 += dmg * 0.02;
				}
				else
				{
					num2 += dmg * 0.015;
				}
			}
			if (flag2)
			{
				num2 += dmg * 0.03;
			}
			if (num > 0)
			{
				if (num == 1)
				{
					if (enemy.Health / enemy.MaxHealth * 100f < 20f)
					{
						num2 += dmg * 0.05;
					}
				}
				else
				{
					if (num == 2)
					{
						if (enemy.Health / enemy.MaxHealth * 100f < 35f)
						{
							num2 += dmg * 0.05;
						}
					}
					else
					{
						if (num == 3 && enemy.Health / enemy.MaxHealth * 100f < 50f)
						{
							num2 += dmg * 0.05;
						}
					}
				}
			}
			double num3 = (double)(enemy.Armor * source.PercentArmorPenetrationMod);
			double num4 = 100.0 / (100.0 + num3 - (double)source.FlatArmorPenetrationMod);
			return (dmg + num2) * num4;
		}
		public static double CalcMagicDmg(double dmg, Obj_AI_Hero source, Obj_AI_Base enemy)
		{
			bool flag = false;
			bool flag2 = false;
			int num = 0;
			Mastery[] masteries = source.Masteries;
			for (int i = 0; i < masteries.Length; i++)
			{
				Mastery mastery = masteries[i];
				if (mastery.Page == MasteryPage.Offense)
				{
					byte id = mastery.Id;
					if (id <= 68)
					{
						if (id != 65)
						{
							if (id == 68)
							{
								byte arg_89_0 = mastery.Points;
							}
						}
						else
						{
							flag = (mastery.Points == 1);
						}
					}
					else
					{
						if (id != 100)
						{
							if (id != 132)
							{
								if (id == 146)
								{
									flag2 = (mastery.Points == 1);
								}
							}
							else
							{
								byte arg_77_0 = mastery.Points;
							}
						}
						else
						{
							num = (int)mastery.Points;
						}
					}
				}
			}
			double num2 = 0.0;
			if (flag)
			{
				if (source.CombatType == GameObjectCombatType.Melee)
				{
					num2 = dmg * 0.02;
				}
				else
				{
					num2 = dmg * 0.015;
				}
			}
			if (flag2)
			{
				num2 += dmg * 0.03;
			}
			if (num > 0)
			{
				if (num == 1)
				{
					if (enemy.Health / enemy.MaxHealth * 100f < 20f)
					{
						num2 += dmg * 0.05;
					}
				}
				else
				{
					if (num == 2)
					{
						if (enemy.Health / enemy.MaxHealth * 100f < 35f)
						{
							num2 += dmg * 0.05;
						}
					}
					else
					{
						if (num == 3 && enemy.Health / enemy.MaxHealth * 100f < 50f)
						{
							num2 += dmg * 0.05;
						}
					}
				}
			}
			double num3 = (double)(enemy.SpellBlock * source.PercentMagicPenetrationMod);
			double num4 = 100.0 / (100.0 + num3 - (double)source.FlatMagicPenetrationMod);
			return (dmg + num2) * num4;
		}
	}
}
