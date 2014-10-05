using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Color = System.Drawing.Color;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
namespace Riven
{
    public struct WallHopPosition
    {
        public SharpDX.Vector3 pA;
        public SharpDX.Vector3 pB;
        public WallHopPosition(SharpDX.Vector3 pA, SharpDX.Vector3 pB)
        {
            this.pA = pA;
            this.pB = pB;
        }
    }
	internal class Program
	{
		public static string ChampionName = "Riven";
		public static Spell Q;
		public static Spell Q3;
		public static Spell W;
		public static Spell E;
		public static Spell R;
		public static Spell RQ;
		public static Spell RQ3;
		public static Spell RW;
		public static bool ROn = false;
		public static Menu Config;
		public static Orbwalking.Orbwalker Orbwalker;
        public static List<WallHopPosition> jumpPositions = new List<WallHopPosition>();
        public static SharpDX.Vector3 startPoint;
        public static SharpDX.Vector3 endPoint;
        public static SharpDX.Vector3 directionVector;
        private static SharpDX.Vector3 directionPos;
        public static bool busy = false;
        private static int rotateMultiplier = 15;
        private static bool IsSR = false;
        public static int minRange = 100;
        public static int a = 2;
		private static void Main(string[] args)
		{
			CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
		}
		private static void Game_OnGameLoad(EventArgs args)
		{
			if (ObjectManager.Player.ChampionName != ChampionName)
			{
				return;
			}
            if (Utility.Map.GetMap()._MapType == Utility.Map.MapType.SummonersRift)
                IsSR = true;
			Game.PrintChat("Riven");
			Q = new Spell(SpellSlot.Q, 260f);
			RQ = new Spell(SpellSlot.Q, 325f);
			Q3 = new Spell(SpellSlot.Q, 300f);
			RQ3 = new Spell(SpellSlot.Q, 400f);
			W = new Spell(SpellSlot.W, 250f);
			RW = new Spell(SpellSlot.W, 270f);
			E = new Spell(SpellSlot.E, 390f);
			R = new Spell(SpellSlot.R, 900f);
			Q.SetSkillshot(0.25f, 50f, 780f, false,SkillshotType.SkillshotCircle);
            RQ.SetSkillshot(0.25f, 50f, 780f, false, SkillshotType.SkillshotCircle);
            Q3.SetSkillshot(0.4f, 50f, 565f, false, SkillshotType.SkillshotCircle);
            RQ3.SetSkillshot(0.4f, 50f, 565f, false, SkillshotType.SkillshotCircle);
			E.SetSkillshot(0.25f, 100f, 1235f, false, SkillshotType.SkillshotLine);
			R.SetSkillshot(0.25f, 60f, 2000f, false, SkillshotType.SkillshotCone);
			Config = new Menu(ChampionName, ChampionName, true);
			Menu menu = new Menu("Target Selector", "Target Selector", false);
			SimpleTs.AddToMenu(menu);
			Config.AddSubMenu(menu);
			Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking", false));
			Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));
			Config.AddSubMenu(new Menu("Combo", "Combo", false));
			Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q").SetValue<bool>(true));
			Config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W").SetValue<bool>(true));
			Config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E").SetValue<bool>(true));
			Config.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "Use R").SetValue<bool>(true));
			Config.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "Combo!").SetValue<KeyBind>(new KeyBind(32u, KeyBindType.Press, false)));
			Config.SubMenu("Combo").AddItem(new MenuItem("FleeActive", "Flee!").SetValue<KeyBind>(new KeyBind((uint)"S".ToCharArray()[0], KeyBindType.Press, false)));
			Config.AddSubMenu(new Menu("Drawings", "Drawings", false));
			Config.SubMenu("Drawings").AddItem(new MenuItem("DrawQRange", "Draw Q Range").SetValue<Circle>(new Circle(true, System.Drawing.Color.FromArgb(100, 255, 0, 255))));
			Config.SubMenu("Drawings").AddItem(new MenuItem("DrawWRange", "Draw W Range").SetValue<Circle>(new Circle(false, System.Drawing.Color.FromArgb(100, 255, 255, 255))));
			Config.SubMenu("Drawings").AddItem(new MenuItem("DrawERange", "Draw E Range").SetValue<Circle>(new Circle(true, System.Drawing.Color.FromArgb(100, 255, 255, 255))));
			Config.SubMenu("Drawings").AddItem(new MenuItem("DrawQRRange", "Draw Q Range").SetValue<Circle>(new Circle(true, System.Drawing.Color.FromArgb(100, 255, 0, 255))));
			Config.SubMenu("Drawings").AddItem(new MenuItem("DrawWRRange", "Draw W Range").SetValue<Circle>(new Circle(false, System.Drawing.Color.FromArgb(100, 255, 255, 255))));
			Config.SubMenu("Drawings").AddItem(new MenuItem("DrawRRange", "Draw R Range").SetValue<Circle>(new Circle(false, System.Drawing.Color.FromArgb(100, 255, 255, 255))));
            if (IsSR)
            {
                Config.SubMenu("Drawings").AddItem(new MenuItem("DrawJumps", "Draw Jump spots (always)").SetValue(false));
                Config.SubMenu("Drawings").AddItem(new MenuItem("DrawJumps2", "Draw Jump spots").SetValue(new KeyBind(71, KeyBindType.Press)));
                Config.SubMenu("Drawings").AddItem(new MenuItem("DrawJumpsRange", "Draw Jumps Range").SetValue(new Slider(1000, 200, 10000)));
                Config.AddItem(new MenuItem("WallJump", "Wall Jump").SetValue(new KeyBind(71, KeyBindType.Press)));

                PopulateList();
            }
            Config.AddToMainMenu();
			Game.OnGameUpdate += Game_OnGameUpdate;
			Drawing.OnDraw += Drawing_OnDraw;
			Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
			Game.OnGameSendPacket += Game_OnSendPacket;
			Game.OnGameProcessPacket += Game_OnProcessPacket;
            if (IsSR)
                Game.OnGameUpdate += Wallhopper_OnGameUpdate;
		}
		private static void Game_OnGameUpdate(EventArgs args)
		{
			bool value = Config.Item("UseWCombo").GetValue<bool>();
			bool value2 = Config.Item("UseECombo").GetValue<bool>();
			bool value3 = Config.Item("UseRCombo").GetValue<bool>();
            if (!Q.IsReady())
                a = 0;
			if (Config.Item("FleeActive").GetValue<KeyBind>().Active)
			{
				MoveTo(Game.CursorPos);
				if (Buffmanager.qStacks == 0 && Q.IsReady())
				{
					Q.Cast(Game.CursorPos, false);
				}
				if (Buffmanager.qStacks > 0 && E.IsReady())
				{
					if (Buffmanager.qStacks == 2)
					{
						if (ObjectManager.Player.Spellbook.CanUseSpell(SpellSlot.E) == SpellState.Ready)
						{
							E.Cast(Game.CursorPos, false);
						}
					}
					else
					{
						if (Buffmanager.qStacks != 2 && Q.IsReady())
						{
							Q.Cast(Game.CursorPos, false);
						}
					}
				}
				if ((ObjectManager.Player.Spellbook.CanUseSpell(SpellSlot.E) == SpellState.Cooldown || ObjectManager.Player.Spellbook.CanUseSpell(SpellSlot.E) == SpellState.NotLearned) && Q.IsReady())
				{
					Q.Cast(Game.CursorPos, false);
				}
				if (!Q.IsReady() && Buffmanager.qStacks == 0 && E.IsReady())
				{
					E.Cast(Game.CursorPos, false);
				}
			}
			if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
			{
				Obj_AI_Hero target = SimpleTs.GetTarget(1000f, SimpleTs.DamageType.Physical);
				if (target != null)
				{
					double comboDamage = GetComboDamage(target);
					double rComboDamage = GetRComboDamage(target);
					double noAAComboDamage = GetNoAAComboDamage(target);
					bool flag = false;
					if (noAAComboDamage >= (double)target.Health && !Buffmanager.ROn && Vector3.Distance(target.ServerPosition, ObjectManager.Player.ServerPosition) <= E.Range + target.BoundingRadius / 2f)
					{
						if (E.IsReady())
						{
							E.Cast(target.ServerPosition, true);
						}
						ObjectManager.Player.Spellbook.CastSpell(SpellSlot.R);
						Items.UseItem(3074, null);
						Items.UseItem(3077, null);
						if (W.IsReady() && Vector3.Distance(target.ServerPosition, ObjectManager.Player.ServerPosition) <= RW.Range)
						{
							ObjectManager.Player.Spellbook.CastSpell(SpellSlot.W);
						}
						Q.Cast(target.ServerPosition, true);
						R.Cast(target.ServerPosition, true);
						Q.Cast(target.ServerPosition, true);
						Q.Cast(target.ServerPosition, true);
					}
					if (comboDamage < (double)target.Health && rComboDamage >= (double)target.Health)
					{
						flag = true;
					}
					if (Buffmanager.windSlashReady)
					{
                        var damage = ObjectManager.Player.GetSpellDamage(target, SpellSlot.R);
						if (damage >= target.Health && Vector3.Distance(ObjectManager.Player.ServerPosition, target.ServerPosition) <= R.Range)
						{
							R.Cast(target.ServerPosition, false);
						}
					}
					if (flag && value2 && E.IsReady() && value3 && R.IsReady() && !Orbwalking.InAutoAttackRange(target) && Vector3.Distance(target.ServerPosition, ObjectManager.Player.ServerPosition) < E.Range + target.BoundingRadius + 75f)
					{
						E.Cast(target.ServerPosition, true);
						ObjectManager.Player.Spellbook.CastSpell(SpellSlot.R);
					}
					if (!flag && value2 && E.IsReady() && !Orbwalking.InAutoAttackRange(target) && Vector3.Distance(target.ServerPosition, ObjectManager.Player.ServerPosition) < E.Range + target.BoundingRadius)
					{
						E.Cast(target.ServerPosition, true);
						ObjectManager.Player.Spellbook.CastSpell(SpellSlot.R);
					}
					if (ROn)
					{
						if (value && W.IsReady() && Vector3.Distance(ObjectManager.Player.ServerPosition, target.ServerPosition) <= RW.Range + target.BoundingRadius)
						{
							ObjectManager.Player.Spellbook.CastSpell(SpellSlot.W);
							return;
						}
					}
					else
					{
						if (!ROn && value && W.IsReady() && Vector3.Distance(ObjectManager.Player.ServerPosition, target.ServerPosition) <= W.Range + target.BoundingRadius)
						{
							ObjectManager.Player.Spellbook.CastSpell(SpellSlot.W);
						}
					}
				}
			}
		}
		private static void Drawing_OnDraw(EventArgs args)
		{
			if (ROn)
			{
				Circle value = Config.Item("DrawQRRange").GetValue<Circle>();
				if (value.Active)
				{
					Utility.DrawCircle(ObjectManager.Player.Position, RQ.Range, value.Color, 2, 30, false);
				}
				Circle value2 = Config.Item("DrawWRRange").GetValue<Circle>();
				if (value2.Active)
				{
					Utility.DrawCircle(ObjectManager.Player.Position, RW.Width, value2.Color, 2, 30, false);
				}
				Circle value3 = Config.Item("DrawRRange").GetValue<Circle>();
				if (value3.Active)
				{
					Utility.DrawCircle(ObjectManager.Player.Position, R.Width, value3.Color, 2, 30, false);
				}
			}
			else
			{
				Circle value4 = Config.Item("DrawQRange").GetValue<Circle>();
				if (value4.Active)
				{
					Utility.DrawCircle(ObjectManager.Player.Position, Q.Range, value4.Color, 2, 30, false);
				}
				Circle value5 = Config.Item("DrawWRange").GetValue<Circle>();
				if (value5.Active)
				{
					Utility.DrawCircle(ObjectManager.Player.Position, W.Width, value5.Color, 2, 30, false);
				}
			}
			Circle value6 = Config.Item("DrawERange").GetValue<Circle>();
			if (value6.Active)
			{
				Utility.DrawCircle(ObjectManager.Player.Position, E.Range, value6.Color, 2, 30, false);
			}
            if (IsSR && (Config.Item("DrawJumps").GetValue<bool>() || Config.Item("DrawJumps2").GetValue<KeyBind>().Active))
            {
                foreach (WallHopPosition pos in jumpPositions)
                {

                    if (ObjectManager.Player.Distance(pos.pA) <= Config.Item("DrawJumpsRange").GetValue<Slider>().Value || ObjectManager.Player.Distance(pos.pB) <= Config.Item("DrawJumpsRange").GetValue<Slider>().Value)
                    {
                        Utility.DrawCircle(pos.pA, 100, System.Drawing.Color.Green);
                        Utility.DrawCircle(pos.pB, 100, System.Drawing.Color.GreenYellow);
                    }
                }
            }
		}
		public static void AnimationCancel()
		{
			ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, ObjectManager.Player.ServerPosition + new Vector3(5f, 5f, 0f));
		}
		private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
		{
			if (!sender.IsMe)
			{
				return;
			}
			string name = args.SData.Name;
			if (Config.Item("FleeActive").GetValue<KeyBind>().Active && name == "RivenTriCleave")
			{
				AnimationCancel();
			}
		}
		private static double GetComboDamage(Obj_AI_Base target)
		{
			double num = 0.0;
			if (Q.IsReady() && !Buffmanager.hasQStacks())
			{
				num += ObjectManager.Player.GetSpellDamage(target, SpellSlot.Q);
			}
			if (W.IsReady())
			{
                num += ObjectManager.Player.GetSpellDamage(target, SpellSlot.W);
			}
			return num + ObjectManager.Player.GetAutoAttackDamage(target) * 4.0;
		}
		private static double GetRComboDamage(Obj_AI_Base target)
		{
			double num = 0.0;
			if (Q.IsReady() && !Buffmanager.hasQStacks())
			{
                num += ObjectManager.Player.GetSpellDamage(target, SpellSlot.Q);
			}
			if (W.IsReady())
			{
                num += ObjectManager.Player.GetSpellDamage(target, SpellSlot.W);
			}
			num += ObjectManager.Player.GetAutoAttackDamage(target) * 4.0;
			if (R.IsReady() && !Buffmanager.windSlashReady)
			{
				num += ObjectManager.Player.GetSpellDamage(target, SpellSlot.R);
			}
			return num;
		}
		private static double GetNoAAComboDamage(Obj_AI_Base target)
		{
			double num = 0.0;
			if (Q.IsReady() && !Buffmanager.hasQStacks())
			{
                num += ObjectManager.Player.GetSpellDamage(target, SpellSlot.Q);
			}
			if (W.IsReady())
			{
                num += ObjectManager.Player.GetSpellDamage(target, SpellSlot.W);
			}
			if (R.IsReady() && !Buffmanager.windSlashReady)
			{
                num += ObjectManager.Player.GetSpellDamage(target, SpellSlot.R);
			}
			return num;
		}
		private static void Game_OnSendPacket(GamePacketEventArgs args)
		{
			try
			{
				if (args.PacketData[0] == 154 && Orbwalker.ActiveMode.ToString() == "Combo")
				{
					Packet.C2S.Cast.Struct @struct = Packet.C2S.Cast.Decoded(args.PacketData);
					if (@struct.Slot >= SpellSlot.Q && @struct.Slot <= SpellSlot.Item1)
					{
						Utility.DelayAction.Add(Game.Ping, delegate
						{
							AnimationCancel();
						});
					}
					if (@struct.Slot == SpellSlot.Q)
					{
						Orbwalking.ResetAutoAttackTimer();
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}
		public static void Game_OnProcessPacket(GamePacketEventArgs args)
		{
			try
			{
				if (Orbwalker.ActiveMode.ToString() == "Combo" && args.PacketData[0] == 101 && Q.IsReady())
				{
					GamePacket gamePacket = new GamePacket(args.PacketData);
					gamePacket.Position = 5L;
					int num = (int)gamePacket.ReadByte();
					int networkId = gamePacket.ReadInteger();
					int num2 = gamePacket.ReadInteger();
					if (ObjectManager.Player.NetworkId == num2)
					{
						Obj_AI_Hero unitByNetworkId = ObjectManager.GetUnitByNetworkId<Obj_AI_Hero>(networkId);
						if (num == 12 || num == 3)
						{
							Q.Cast(unitByNetworkId.ServerPosition, false);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}
		private static void MoveTo(Vector3 position)
		{
			if (Geometry.Distance(ObjectManager.Player.ServerPosition, position) == 0f)
			{
				ObjectManager.Player.IssueOrder(GameObjectOrder.HoldPosition, ObjectManager.Player.ServerPosition);
				return;
			}
			Vector3 targetPos = ObjectManager.Player.ServerPosition + 400f * (position.To2D() - ObjectManager.Player.ServerPosition.To2D()).Normalized().To3D();
			ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, targetPos);
		}
        private static void Wallhopper_OnGameUpdate(EventArgs args)
        {
            if (!busy && Config.Item("WallJump").GetValue<KeyBind>().Active && Buffmanager.qStacks == 2)
            {
                var closest = minRange + 1f;
                foreach (WallHopPosition pos in jumpPositions)
                {
                    if (ObjectManager.Player.Distance(pos.pA) < closest || ObjectManager.Player.Distance(pos.pB) < closest)
                    {
                        busy = true;
                        if (ObjectManager.Player.Distance(pos.pA) < ObjectManager.Player.Distance(pos.pB))
                        {
                            closest = ObjectManager.Player.Distance(pos.pA);
                            startPoint = pos.pA;
                            endPoint = pos.pB;
                        }
                        else
                        {
                            closest = ObjectManager.Player.Distance(pos.pB);
                            startPoint = pos.pB;
                            endPoint = pos.pA;
                        }
                    }

                }
                if (busy)
                {
                    directionVector.X = startPoint.X - endPoint.X;
                    directionVector.Y = startPoint.Y - endPoint.Y;
                    ObjectManager.Player.IssueOrder(GameObjectOrder.HoldPosition, ObjectManager.Player.ServerPosition);
                    Packet.C2S.Move.Encoded(new Packet.C2S.Move.Struct(startPoint.X, startPoint.Y)).Send();
                    Utility.DelayAction.Add(180, delegate { changeDirection1(); });
                }
            }
        }
        public static void changeDirection1()
        {
            ObjectManager.Player.IssueOrder(GameObjectOrder.HoldPosition, ObjectManager.Player.ServerPosition);
            Packet.C2S.Move.Encoded(new Packet.C2S.Move.Struct(startPoint.X + directionVector.X / rotateMultiplier, startPoint.Y + directionVector.Y / rotateMultiplier)).Send();

            directionPos = new SharpDX.Vector3(startPoint.X, startPoint.Y, startPoint.Z);
            directionPos.X = startPoint.X + directionVector.X / rotateMultiplier;
            directionPos.Y = startPoint.Y + directionVector.Y / rotateMultiplier;
            directionPos.Z = startPoint.Z + directionVector.Z / rotateMultiplier;
            Utility.DelayAction.Add(60, delegate { changeDirection2(); });
        }
        public static void changeDirection2()
        {
            Packet.C2S.Move.Encoded(new Packet.C2S.Move.Struct(startPoint.X, startPoint.Y)).Send();
            Utility.DelayAction.Add(64, delegate { CastJump(); });
        }
        public static void CastJump()
        {
            Q.Cast(endPoint, true);
            ObjectManager.Player.IssueOrder(GameObjectOrder.HoldPosition, ObjectManager.Player.ServerPosition);
            Utility.DelayAction.Add(1000, delegate { freeFunction(); });
        }
        private static void freeFunction()
        {
            busy = false;
        }
        public static void PopulateList()
        {
            jumpPositions.Add(new WallHopPosition(new SharpDX.Vector3(6393.7299804688f, 8341.7451171875f, -63.87451171875f), new SharpDX.Vector3(6612.1625976563f, 8574.7412109375f, 56.018413543701f)));
            jumpPositions.Add(new WallHopPosition(new SharpDX.Vector3(7041.7885742188f, 8810.1787109375f, 0f), new SharpDX.Vector3(7296.0341796875f, 9056.4638671875f, 55.610824584961f)));
            jumpPositions.Add(new WallHopPosition(new SharpDX.Vector3(2805.4074707031f, 6140.130859375f, 55.182941436768f), new SharpDX.Vector3(2614.3215332031f, 5816.9438476563f, 60.193073272705f)));
            jumpPositions.Add(new WallHopPosition(new SharpDX.Vector3(6696.486328125f, 5377.4013671875f, 61.310482025146f), new SharpDX.Vector3(6868.6918945313f, 5698.1455078125f, 55.616455078125f)));
            jumpPositions.Add(new WallHopPosition(new SharpDX.Vector3(2809.3254394531f, 10178.6328125f, -58.759708404541f), new SharpDX.Vector3(2553.8962402344f, 9974.4677734375f, 53.364395141602f)));
            jumpPositions.Add(new WallHopPosition(new SharpDX.Vector3(5102.642578125f, 10322.375976563f, -62.845260620117f), new SharpDX.Vector3(5483f, 10427f, 54.5009765625f)));
            jumpPositions.Add(new WallHopPosition(new SharpDX.Vector3(6000.2373046875f, 11763.544921875f, 39.544124603271f), new SharpDX.Vector3(6056.666015625f, 11388.752929688f, 54.385917663574f)));
            jumpPositions.Add(new WallHopPosition(new SharpDX.Vector3(3319.087890625f, 7472.4760742188f, 55.027889251709f), new SharpDX.Vector3(3388.0522460938f, 7101.2568359375f, 54.486026763916f)));
            jumpPositions.Add(new WallHopPosition(new SharpDX.Vector3(3989.9423828125f, 7929.3422851563f, 51.94282913208f), new SharpDX.Vector3(3671.623046875f, 7723.146484375f, 53.906265258789f)));
            jumpPositions.Add(new WallHopPosition(new SharpDX.Vector3(4936.8452148438f, 10547.737304688f, -63.064865112305f), new SharpDX.Vector3(5156.7397460938f, 10853.216796875f, 52.951190948486f)));
            jumpPositions.Add(new WallHopPosition(new SharpDX.Vector3(5028.1235351563f, 10115.602539063f, -63.082695007324f), new SharpDX.Vector3(5423f, 10127f, 55.15357208252f)));
            jumpPositions.Add(new WallHopPosition(new SharpDX.Vector3(6035.4819335938f, 10973.666015625f, 53.918266296387f), new SharpDX.Vector3(6385.4013671875f, 10827.455078125f, 54.63500213623f)));
            jumpPositions.Add(new WallHopPosition(new SharpDX.Vector3(4747.0625f, 11866.421875f, 41.584358215332f), new SharpDX.Vector3(4743.23046875f, 11505.842773438f, 51.196254730225f)));
            jumpPositions.Add(new WallHopPosition(new SharpDX.Vector3(6749.4487304688f, 12980.83984375f, 44.903495788574f), new SharpDX.Vector3(6701.4965820313f, 12610.278320313f, 52.563804626465f)));
            jumpPositions.Add(new WallHopPosition(new SharpDX.Vector3(3114.1865234375f, 9420.5078125f, -42.718975067139f), new SharpDX.Vector3(2757f, 9255f, 53.77322769165f)));
            jumpPositions.Add(new WallHopPosition(new SharpDX.Vector3(2786.8354492188f, 9547.8935546875f, 53.645294189453f), new SharpDX.Vector3(3002.0930175781f, 9854.39453125f, -53.198081970215f)));
            jumpPositions.Add(new WallHopPosition(new SharpDX.Vector3(3803.9470214844f, 7197.9018554688f, 53.730079650879f), new SharpDX.Vector3(3664.1088867188f, 7543.572265625f, 54.18229675293f)));
            jumpPositions.Add(new WallHopPosition(new SharpDX.Vector3(2340.0886230469f, 6387.072265625f, 60.165466308594f), new SharpDX.Vector3(2695.6096191406f, 6374.0634765625f, 54.339839935303f)));
            jumpPositions.Add(new WallHopPosition(new SharpDX.Vector3(3249.791015625f, 6446.986328125f, 55.605854034424f), new SharpDX.Vector3(3157.4558105469f, 6791.4458007813f, 54.080295562744f)));
            jumpPositions.Add(new WallHopPosition(new SharpDX.Vector3(3823.6242675781f, 5923.9130859375f, 55.420352935791f), new SharpDX.Vector3(3584.2561035156f, 6215.4931640625f, 55.6123046875f)));
            jumpPositions.Add(new WallHopPosition(new SharpDX.Vector3(5796.4809570313f, 5060.4116210938f, 51.673671722412f), new SharpDX.Vector3(5730.3081054688f, 5430.1635742188f, 54.921173095703f)));
            jumpPositions.Add(new WallHopPosition(new SharpDX.Vector3(6007.3481445313f, 4985.3803710938f, 51.673641204834f), new SharpDX.Vector3(6388.783203125f, 4987f, 51.673400878906f)));
            jumpPositions.Add(new WallHopPosition(new SharpDX.Vector3(7040.9892578125f, 3964.6728515625f, 57.192108154297f), new SharpDX.Vector3(6668.0073242188f, 3993.609375f, 51.671356201172f)));
            jumpPositions.Add(new WallHopPosition(new SharpDX.Vector3(7763.541015625f, 3294.3481445313f, 54.872283935547f), new SharpDX.Vector3(7629.421875f, 3648.0581054688f, 56.908012390137f)));
            jumpPositions.Add(new WallHopPosition(new SharpDX.Vector3(4705.830078125f, 9440.6572265625f, -62.586814880371f), new SharpDX.Vector3(4779.9809570313f, 9809.9091796875f, -63.09009552002f)));
            jumpPositions.Add(new WallHopPosition(new SharpDX.Vector3(4056.7907714844f, 10216.12109375f, -63.152275085449f), new SharpDX.Vector3(3680.1550292969f, 10182.296875f, -63.701038360596f)));
            jumpPositions.Add(new WallHopPosition(new SharpDX.Vector3(4470.0883789063f, 12000.479492188f, 41.59789276123f), new SharpDX.Vector3(4232.9799804688f, 11706.015625f, 49.295585632324f)));
            jumpPositions.Add(new WallHopPosition(new SharpDX.Vector3(5415.5708007813f, 12640.216796875f, 40.682685852051f), new SharpDX.Vector3(5564.4409179688f, 12985.860351563f, 41.373748779297f)));
            jumpPositions.Add(new WallHopPosition(new SharpDX.Vector3(6053.779296875f, 12567.381835938f, 40.587882995605f), new SharpDX.Vector3(6045.4555664063f, 12942.313476563f, 41.211364746094f)));
            jumpPositions.Add(new WallHopPosition(new SharpDX.Vector3(4454.66015625f, 8057.1313476563f, 42.799690246582f), new SharpDX.Vector3(4577.8681640625f, 7699.3686523438f, 53.31339263916f)));
            jumpPositions.Add(new WallHopPosition(new SharpDX.Vector3(7754.7700195313f, 10449.736328125f, 52.890430450439f), new SharpDX.Vector3(8096.2885742188f, 10288.80078125f, 53.66955947876f)));
            jumpPositions.Add(new WallHopPosition(new SharpDX.Vector3(7625.3139648438f, 9465.7001953125f, 55.008113861084f), new SharpDX.Vector3(7995.986328125f, 9398.1982421875f, 53.530490875244f)));
            jumpPositions.Add(new WallHopPosition(new SharpDX.Vector3(9767f, 8839f, 53.044532775879f), new SharpDX.Vector3(9653.1220703125f, 9174.7626953125f, 53.697280883789f)));
            jumpPositions.Add(new WallHopPosition(new SharpDX.Vector3(10775.653320313f, 7612.6943359375f, 55.35241317749f), new SharpDX.Vector3(10665.490234375f, 7956.310546875f, 65.222145080566f)));
            jumpPositions.Add(new WallHopPosition(new SharpDX.Vector3(10398.484375f, 8257.8642578125f, 66.200691223145f), new SharpDX.Vector3(10176.104492188f, 8544.984375f, 64.849853515625f)));
            jumpPositions.Add(new WallHopPosition(new SharpDX.Vector3(11198.071289063f, 8440.4638671875f, 67.641044616699f), new SharpDX.Vector3(11531.436523438f, 8611.0087890625f, 53.454048156738f)));
            jumpPositions.Add(new WallHopPosition(new SharpDX.Vector3(11686.700195313f, 8055.9624023438f, 55.458232879639f), new SharpDX.Vector3(11314.19140625f, 8005.4946289063f, 58.438243865967f)));
            jumpPositions.Add(new WallHopPosition(new SharpDX.Vector3(10707.119140625f, 7335.1752929688f, 55.350387573242f), new SharpDX.Vector3(10693f, 6943f, 54.870254516602f)));
            jumpPositions.Add(new WallHopPosition(new SharpDX.Vector3(10395.380859375f, 6938.5009765625f, 54.869094848633f), new SharpDX.Vector3(10454.955078125f, 7316.7041015625f, 55.308219909668f)));
            jumpPositions.Add(new WallHopPosition(new SharpDX.Vector3(10358.5859375f, 6677.1704101563f, 54.86909866333f), new SharpDX.Vector3(10070.067382813f, 6434.0815429688f, 55.294486999512f)));
            jumpPositions.Add(new WallHopPosition(new SharpDX.Vector3(11161.98828125f, 5070.447265625f, 53.730766296387f), new SharpDX.Vector3(10783f, 4965f, -63.57177734375f)));
            jumpPositions.Add(new WallHopPosition(new SharpDX.Vector3(11167.081054688f, 4613.9829101563f, -62.898971557617f), new SharpDX.Vector3(11501f, 4823f, 54.571090698242f)));
            jumpPositions.Add(new WallHopPosition(new SharpDX.Vector3(11743.823242188f, 4387.4672851563f, 52.005855560303f), new SharpDX.Vector3(11379f, 4239f, -61.565242767334f)));
            jumpPositions.Add(new WallHopPosition(new SharpDX.Vector3(10388.120117188f, 4267.1796875f, -63.61775970459f), new SharpDX.Vector3(10033.036132813f, 4147.1669921875f, -60.332069396973f)));
            jumpPositions.Add(new WallHopPosition(new SharpDX.Vector3(8964.7607421875f, 4214.3833007813f, -63.284225463867f), new SharpDX.Vector3(8569f, 4241f, 55.544258117676f)));
            jumpPositions.Add(new WallHopPosition(new SharpDX.Vector3(5554.8657226563f, 4346.75390625f, 51.680099487305f), new SharpDX.Vector3(5414.0634765625f, 4695.6860351563f, 51.611679077148f)));
            jumpPositions.Add(new WallHopPosition(new SharpDX.Vector3(7311.3393554688f, 10553.6015625f, 54.153884887695f), new SharpDX.Vector3(6938.5209960938f, 10535.8515625f, 54.441242218018f)));
            jumpPositions.Add(new WallHopPosition(new SharpDX.Vector3(7669.353515625f, 5960.5717773438f, -64.488967895508f), new SharpDX.Vector3(7441.2182617188f, 5761.8989257813f, 54.347793579102f)));
            jumpPositions.Add(new WallHopPosition(new SharpDX.Vector3(7949.65625f, 2647.0490722656f, 54.276401519775f), new SharpDX.Vector3(7863.0063476563f, 3013.7814941406f, 55.178623199463f)));
            jumpPositions.Add(new WallHopPosition(new SharpDX.Vector3(8698.263671875f, 3783.1169433594f, 57.178703308105f), new SharpDX.Vector3(9041f, 3975f, -63.242683410645f)));
            jumpPositions.Add(new WallHopPosition(new SharpDX.Vector3(9063f, 3401f, 68.192077636719f), new SharpDX.Vector3(9275.0751953125f, 3712.8935546875f, -63.257461547852f)));
            jumpPositions.Add(new WallHopPosition(new SharpDX.Vector3(12064.340820313f, 6424.11328125f, 54.830627441406f), new SharpDX.Vector3(12267.9375f, 6742.9453125f, 54.83561706543f)));
            jumpPositions.Add(new WallHopPosition(new SharpDX.Vector3(11913.165039063f, 5373.34375f, 54.050819396973f), new SharpDX.Vector3(11569.1953125f, 5211.7143554688f, 57.787326812744f)));
            jumpPositions.Add(new WallHopPosition(new SharpDX.Vector3(7324.2783203125f, 1461.2199707031f, 52.594970703125f), new SharpDX.Vector3(7357.3852539063f, 1837.4309082031f, 54.282878875732f)));
        }
	}
}
