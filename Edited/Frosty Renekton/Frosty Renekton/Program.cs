using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
namespace Frosty_Renekton
{
    class Program
    {
        public static string wready = "RenektonPreExecute";
        public static bool oldw = false;
        public static bool curw = false;
        public static Items.Item TIA;
        public static Items.Item HYD;
        public static HitChance test;
        public static string ChampName = "Renekton";
        public static Orbwalking.Orbwalker Orbwalker;
        public static Obj_AI_Base Player = ObjectManager.Player;
        public static GrassObject x;
        public static Spell Q, W, E, R, AA;
        public static Menu RenektonWrapper; 
        static void testW()
        {
            foreach (var buff in ObjectManager.Player.Buffs)
            {
                if (buff.Name == wready)
                {
                    curw = true;
                }
                else
                {
                    curw = false;
                }
            }
            if (curw == false && oldw == true)
            {
                onLoseW();
            }
            oldw = curw;
        }
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            TIA = new Items.Item(3077, 400);
            HYD = new Items.Item(3074, 400);
            if (Player.BaseSkinName != ChampName) return;
            Q = new Spell(SpellSlot.Q, 250);
            W = new Spell(SpellSlot.W, Player.AttackRange);
            E = new Spell(SpellSlot.E, 460);
            R = new Spell(SpellSlot.R, float.MaxValue);
            W.SetSkillshot(0.5f, 50.0f, 20.0f, false, SkillshotType.SkillshotLine);

            RenektonWrapper = new Menu(ChampName, ChampName, true);

            RenektonWrapper.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            Orbwalker = new Orbwalking.Orbwalker(RenektonWrapper.SubMenu("Orbwalker"));
            var ts = new Menu("Target Selector", "Target Selector");
            SimpleTs.AddToMenu(ts);
            RenektonWrapper.AddSubMenu(ts);
            RenektonWrapper.AddSubMenu(new Menu("Combo", "Combo"));
            RenektonWrapper.SubMenu("Combo").AddItem(new MenuItem("chance", "Use R Life?").SetValue(new Slider(40, 0, 100)));
            RenektonWrapper.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "COMBO!").SetValue(new KeyBind(32, KeyBindType.Press)));
            RenektonWrapper.SubMenu("Combo").AddItem(new MenuItem("useR", "Use R").SetValue(true));
            RenektonWrapper.SubMenu("Combo").AddItem(new MenuItem("useE2", "Use E twice").SetValue(true));
            RenektonWrapper.AddSubMenu(new Menu("Interrupt", "Interrupt"));
           RenektonWrapper.AddSubMenu(new Menu("Harrass", "Harrass"));
            RenektonWrapper.AddSubMenu(new Menu("Farm", "Farm"));
            RenektonWrapper.AddSubMenu(new Menu("Draw", "Drawing"));
            RenektonWrapper.SubMenu("Draw").AddItem(new MenuItem("DrawActive", "Enable drawings").SetValue(new KeyBind("L".ToCharArray()[0], KeyBindType.Toggle)));
            RenektonWrapper.SubMenu("Harrass").AddItem(new MenuItem("HarrassActive", "Harrass").SetValue(new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));
           RenektonWrapper.SubMenu("Farm").AddItem(new MenuItem("FarmActive", "Farm").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
           RenektonWrapper.SubMenu("Farm").AddItem(new MenuItem("useqf", "Use Q to Farm").SetValue(true));
            RenektonWrapper.AddSubMenu(new Menu("AntiGapclose", "AntiGapclose"));
            RenektonWrapper.SubMenu("AntiGapclose").AddItem(new MenuItem("useeg", "Use E to AntiGapclose").SetValue(true));
            RenektonWrapper.SubMenu("Interrupt").AddItem(new MenuItem("usew", "Use W to Interrupt").SetValue(true));
            RenektonWrapper.AddToMainMenu();

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameUpdate += Game_OnGameUpdate;
            AntiGapcloser.OnEnemyGapcloser += AntiGapclose;
            Game.PrintChat("Frosty " + ChampName + " by newchild 1.0");
            Interrupter.OnPosibleToInterrupt += OnPosibleToInterrupt;
            
        }
        static void onLoseW()
        {
            TIA.Cast();
            HYD.Cast();
        }
        static void Game_OnGameUpdate(EventArgs args)
        {
            testW();
            
            if (RenektonWrapper.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            if (RenektonWrapper.Item("FarmActive").GetValue<KeyBind>().Active)
           {
                Farm();
            }
            if (RenektonWrapper.Item("HarrassActive").GetValue<KeyBind>().Active)
            {
                Harrass();
            }
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (RenektonWrapper.Item("DrawActive").GetValue<KeyBind>().Active)
            {
                Utility.DrawCircle(Player.Position, Q.Range, Color.Azure);
                Utility.DrawCircle(Player.Position, E.Range, Color.Crimson);
            }
           
        }

        public static void Harrass()
        {
            var target = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Physical);
            if (target == null) return;

            if (target.IsValidTarget(E.Range) && E.IsReady())
            {
                E.Cast(target.ServerPosition, RenektonWrapper.Item("NFE").GetValue<bool>());
                while(Player.IsDashing()){
                }
                Q.Cast();
                W.Cast();

            }
        }

        public static void Combo()
        {
            

            var target = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Physical);
            if (target == null) return;

            if (target.IsValidTarget(E.Range) && E.IsReady())
            {
                E.Cast(target.ServerPosition, RenektonWrapper.Item("NFE").GetValue<bool>());
                if (RenektonWrapper.Item("useE2").GetValue<bool>())
                {
                    E.Cast(target.ServerPosition);
                }

            }
            if (target.IsValidTarget(Q.Range) && Q.IsReady())
            {
                Q.Cast();

            }
            if (target.IsValidTarget(Player.AttackRange) && W.IsReady())
            {
                W.Cast();
            }
            if (RenektonWrapper.Item("useR").GetValue<bool>() && target.IsValidTarget(R.Range) && R.IsReady() && RenektonWrapper.Item("chance").GetValue<Slider>().Value >= ((Player.Health/Player.MaxHealth)*100))
            {

                R.Cast();
            }
            
        }
        private static void OnPosibleToInterrupt(Obj_AI_Base target, InterruptableSpell spell)
        {
            if (RenektonWrapper.Item("usew").GetValue<bool>())
            {
                if (target.IsValidTarget(E.Range) && E.IsReady())
                {
                    E.Cast(target.ServerPosition);
                    while (Player.IsDashing())
                    {
                        Game.PrintChat("Dashing...");
                    }
                    Q.Cast();
                    W.Cast();
                    TIA.Cast();
                    HYD.Cast();

                }
            }
            
        }
        public static void Farm()
        {
            if (RenektonWrapper.Item("useqf").GetValue<bool>())
            {
                if (!Orbwalking.CanMove(40)) return;
                var Rangeminions = MinionManager.GetMinions(Player.ServerPosition, Q.Range);
                if (Q.IsReady())
                {
                    foreach (var minion in Rangeminions)
                    {
                        if (minion.IsValidTarget() && HealthPrediction.GetHealthPrediction(minion, (int)Player.Distance(minion) * 1000 / 14000) < 0.75 * Damage.GetSpellDamage(ObjectManager.Player, minion, SpellSlot.Q))
                        {
                            
                               Q.Cast();
                           
                        }
                            ;
                    }
                        ;
                }
            }
        }
        public static void AntiGapclose(ActiveGapcloser gapcloser)
        {
            if(RenektonWrapper.Item("useeg").GetValue<bool>()){

            
            var target = gapcloser.Start;
            if (E.IsReady())
            {
                E.Cast(target, RenektonWrapper.Item("NFE").GetValue<bool>());
                while (Player.IsDashing())
                {

                }
                Q.Cast();
                W.Cast();

            }
            }
        }
    }
}

