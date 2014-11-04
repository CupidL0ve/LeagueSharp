

        #region References

// It was working like 30 seconds ago, i reverted code changes and it didnt fix it
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

// By iSnorflake
namespace SFKatarina
{
    internal class Program // How the fuck??
    {
#endregion

        #region Declares
        public static string ChampionName = "Katarina";

        //Orbwalker instance
        public static Orbwalking.Orbwalker Orbwalker;

        //Spells
        public static List<Spell> SpellList = new List<Spell>();
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static Items.Item DFG;

        //Menu
        public static Menu Config;
        private static Obj_AI_Hero Player;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }
        #endregion

        #region OnGameLoad
        private static void Game_OnGameLoad(EventArgs args)
        {

            Player = ObjectManager.Player;
            if (Player.BaseSkinName != ChampionName) return;
            Q = new Spell(SpellSlot.Q, 675);
            W = new Spell(SpellSlot.W, 375);
            E = new Spell(SpellSlot.E, 700);
            R = new Spell(SpellSlot.R, 550);

            Game.PrintChat(ChampionName + " Loaded! By iSnorflake V2");
            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
            //Create the menu
            Config = new Menu(ChampionName, ChampionName, true);

            //Orbwalker submenu
            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));
            //Add the targer selector to the menu.
            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            SimpleTs.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);



            //Combo menu
            Config.AddSubMenu(new Menu("Combo", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "Use R").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));

            Config.AddSubMenu(new Menu("Farm", "Farm")); // creds tc-crew
            Config.SubMenu("Farm")
                .AddItem(
                    new MenuItem("UseQFarm", "Use Q").SetValue(
                        true));
            Config.SubMenu("Farm")
                .AddItem(
                    new MenuItem("UseWFarm", "Use W").SetValue(
                        true));
            Config.SubMenu("Farm")
                .AddItem(
                    new MenuItem("FreezeActive", "Freeze!").SetValue(
                        new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            var waveclear = new Menu("Waveclear", "WaveclearMenu");
            waveclear.AddItem(new MenuItem("useQW", "Use Q?").SetValue(true));
            waveclear.AddItem(new MenuItem("useWW", "Use W?").SetValue(true));
            waveclear.AddItem(new MenuItem("Waveclear", "Waveclear").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
            Config.AddSubMenu(waveclear); // Thanks to ChewyMoon for the idea of doing the menu this way

            // Misc
            Config.AddSubMenu(new Menu("Misc", "Misc"));
            Config.SubMenu("Misc").AddItem(new MenuItem("KillstealQ", "Killsteal with Q").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("Escape", "Escape").SetValue(new KeyBind("G".ToCharArray()[0], KeyBindType.Press)));

            // Drawings
            /* Config.AddSubMenu(new Menu("Drawings", "Drawings"));
             Config.SubMenu("Drawings").AddItem(new MenuItem("QRange", "Q Range").SetValue(new Circle(true, Color.FromArgb(150, Color.DodgerBlue))));*/
            Config.AddSubMenu(new Menu("Exploits", "Exploits"));
             Config.SubMenu("Exploits").AddItem(new MenuItem("QNFE", "Q No-Face").SetValue(true));
            // Config.SubMenu("Drawings").AddItem(new MenuItem("ERange", "E Range").SetValue(new Circle(true, Color.FromArgb(150, Color.DodgerBlue))));

            Config.AddToMainMenu();
            //Add the events we are going to use
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;



        }
        #endregion

        #region OnGameUpdate
        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead) return;
            if (IsEnemyInRange()) // If an enemy is in range and im ultimating - dont cancel the ult before their dead
                if (ObjectManager.Player.IsChannelingImportantSpell()) return;
            if (!IsEnemyInRange() && ObjectManager.Player.IsChannelingImportantSpell()) // If the ult isnt hitting anyone
            {
                ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, ObjectManager.Player); // Cancels ult
            }
            Orbwalker.SetAttack(true);
            Orbwalker.SetMovement(true);
            var useQKS = Config.Item("KillstealQ").GetValue<bool>() && Q.IsReady();
            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            if (useQKS)
                Killsteal();
            if (Config.Item("FreezeActive").GetValue<KeyBind>().Active)
                Farm();
            if (Config.Item("Waveclear").GetValue<KeyBind>().Active)
            {
                WaveClear();
            }
            Escape();
        }
        #endregion

        #region Farm
        private static void Farm() // Credits TC-CREW
        {
            if (!Orbwalking.CanMove(40)) return;
            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range);
            var useQ = Config.Item("UseQFarm").GetValue<bool>();
            var useW = Config.Item("UseWFarm").GetValue<bool>();
            if (useQ && Q.IsReady())
            {
                foreach (var minion in allMinions.Where(minion => minion.IsValidTarget() && HealthPrediction.GetHealthPrediction(minion, (int)(ObjectManager.Player.Distance(minion) * 1000 / 1400))
                < 0.75 * ObjectManager.Player.GetSpellDamage(minion, SpellSlot.Q, 1)))
                {
                    Q.Cast(minion);
                    return;
                }
            }
            else if (useW && W.IsReady())
            {
                if (!allMinions.Any(minion => minion.IsValidTarget(W.Range) && minion.Health < 0.75 * Player.GetSpellDamage(minion, SpellSlot.W))) return;

                W.Cast();
                return;

            }
        }
        #endregion

        #region WaveClear
        public static void WaveClear()
        {
            if (!Orbwalking.CanMove(40)) return;

            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range);
            var allJungle = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range);
            var useQ = Config.Item("useQW").GetValue<bool>();
            var useW = Config.Item("useWW").GetValue<bool>();
            if (useQ && Q.IsReady())
            {
                foreach (var minion in allMinions.Where(minion => minion.IsValidTarget(Q.Range)))
                {
                    Q.CastOnUnit(minion, Config.Item("QNFE").GetValue<bool>());
                    return;
                }
            }
            else if (useW && W.IsReady())
            {
                if (!allMinions.Any(minion => minion.IsValidTarget(W.Range))) return;
                W.Cast();
                return;
            }
        }
        #endregion

        #region Combo
        private static void Combo()
        {
            Orbwalker.SetAttack(true);
            var target = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);
            if (target == null) return;

            if (GetDamage(target) > target.Health)
            {
                if (!Player.IsChannelingImportantSpell())
                {
                
                if (Q.IsReady() && Player.Distance(target) < Q.Range + target.BoundingRadius)
                    Q.CastOnUnit(target, Config.Item("QNFE").GetValue<bool>());
                if (E.IsReady() && Player.Distance(target) < E.Range + target.BoundingRadius)
                    Q.CastOnUnit(target, Config.Item("QNFE").GetValue<bool>());
                if (W.IsReady() && Player.Distance(target) < W.Range)
                    W.Cast();
            }
            if (R.IsReady() && Player.Distance(target) < R.Range)
                        R.Cast();
                    
                    
                
                

            }
            else
            {
                if (ObjectManager.Player.Distance(target) < Q.Range && Q.IsReady())
                    Q.CastOnUnit(target, true);

                if (Config.Item("ComboActive").GetValue<KeyBind>().Active &&
                    ObjectManager.Player.Distance(target) < E.Range && E.IsReady())
                    E.CastOnUnit(target);

                if (ObjectManager.Player.Distance(target) < W.Range && W.IsReady())
                    W.Cast();
            }
        }
        #endregion

        #region OnDraw
        private static void Drawing_OnDraw(EventArgs args)
        {
            /*foreach (var spell in SpellList)
            {
                var menuItem = Config.Item(spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active)
                    Utility.DrawCircle(Player.Position, spell.Range, menuItem.Color);
                // Drawing.DrawText(playerPos[0] - 65, playerPos[1] + 20, drawUlt.Color, "Hit R To kill " + UltTarget + "!");

            }*/
            //Drawing tempoarily disabled
        }
        #endregion

        #region Killsteal
        private static void Killsteal() // Creds to TC-Crew
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(Q.Range)))
            {
                if (Q.IsReady() && hero.Distance(ObjectManager.Player) <= Q.Range && Player.GetSpellDamage(hero, SpellSlot.Q) >= hero.Health)
                {
                    Q.CastOnUnit(hero, Config.Item("QNFE").GetValue<bool>());

                }
            }
        }
        #endregion

        #region Escape
        private static void Escape()
        {
            if (Config.Item("Escape").GetValue<KeyBind>().Active)
            {
                ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                if (E.IsReady())
                {
                    foreach (Obj_AI_Base esc in ObjectManager.Get<Obj_AI_Base>())
                    {
                        if (esc.IsAlly && esc.Distance(ObjectManager.Player) <= E.Range &&
                            Vector2.Distance(Game.CursorPos.To2D(), esc.ServerPosition.To2D()) <= 175)
                        {

                            E.CastOnUnit(esc);

                        }
                        else
                        {
                            var ward = FindBestWardItem();
                            if (ward != null)
                            {
                                ward.UseItem(Game.CursorPos);
                            }
                        }

                    }
                }
            }
        }
        #endregion

        #region Ward jump stuff

        private static SpellDataInst GetItemSpell(InventorySlot invSlot)
        {
            return ObjectManager.Player.Spellbook.Spells.FirstOrDefault(spell => (int)spell.Slot == invSlot.Slot + 4);
        }
        private static InventorySlot FindBestWardItem()
        {
            InventorySlot slot = Items.GetWardSlot();
            if (slot == default(InventorySlot)) return null;

            SpellDataInst sdi = GetItemSpell(slot);

            if (sdi != default(SpellDataInst) && sdi.State == SpellState.Ready)
            {
                return slot;
            }
            return null;
        }
        #endregion

        #region GetDamage
        private static double GetDamage(Obj_AI_Base enemy) // Creds to TC-Crew
        {
            var damage = 0d;

            if (Q.IsReady())
                damage += ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.Q);

            if (W.IsReady())
                damage += ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.W);

            if (E.IsReady())
                damage += ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.E);

            if (R.IsReady())
                damage += ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.R, 1);
            return (float)damage;
        }
        private static bool IsEnemyInRange() // Checks if an enemy is in range of my ultimate.
        {
            var target = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Magical);
            if (target == null) return false;
            return true;
        }


    }
}
        #endregion