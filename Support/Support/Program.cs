#region

using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using System.IO;

#endregion

// Based on the popular marksman framework :D
namespace Support {
    internal class Program {

        public static Menu Config;
        public static Champion champClass = null;       

        static void Main(string[] args) {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args) {
            Config = new Menu("Support", "Support", true);

            champClass = new Champion();

            switch (ObjectManager.Player.ChampionName) {
                case "Thresh":
                    champClass = new Thresh();
                    break;
                case "Morgana":
                    champClass = new Morgana();
                    break;
                case "Blitzcrank":
                    champClass = new Blitzcrank();
                    break;
                case "Sona":
                    champClass = new Sona();
                    break;
                case "Leona":
                    champClass = new Leona();
                    break;
            }

            champClass.Id = ObjectManager.Player.BaseSkinName;
            champClass.Config = Config;

            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            SimpleTs.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            var orbwalking = Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            champClass.Orbwalker = new Orbwalking.Orbwalker(orbwalking);

            var items = Config.AddSubMenu(new Menu("Items", "Items"));
            champClass.ItemMenu(items);

            var combo = Config.AddSubMenu(new Menu("Combo", "Combo"));
            champClass.ComboMenu(combo); 

            var harass = Config.AddSubMenu(new Menu("Harass", "Harass"));
            champClass.HarassMenu(harass);

            // Mana sliders :D
            var mana = Config.AddSubMenu(new Menu("Mana Limiter", "Mana Limiter"));
            mana.AddItem(new MenuItem("comboMana", "Combo Mana %").SetValue(new Slider(1, 100, 0)));
            mana.AddItem(new MenuItem("harassMana", "Harass Mana %").SetValue(new Slider(30, 100, 0)));
            champClass.ManaMenu(mana);

            var misc = Config.AddSubMenu(new Menu("Misc", "Misc"));
            misc.AddItem(new MenuItem("AttMin", "Attack Minions?").SetValue(false));
            champClass.MiscMenu(misc);

            var drawing = Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            champClass.DrawingMenu(drawing);

            champClass.MainMenu(Config);

            Config.AddToMainMenu();

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            Interrupter.OnPosibleToInterrupt += Interrupter_OnPosibleToInterrupt;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Game.OnGameSendPacket += Game_OnGameSendPacket;

        }

        static void Game_OnGameSendPacket(GamePacketEventArgs args) {
            if (args.PacketData[0] == Packet.C2S.Move.Header) {
                var decodedPacket = Packet.C2S.Move.Decoded(args.PacketData);
                if (decodedPacket.MoveType == 3) {
                    if (champClass.Orbwalker.GetTarget().IsMinion && !Config.Item("AttMin").GetValue<bool>()) {
                        args.Process = false;
                    }
                }
            }
        }

        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser) {
            champClass.AntiGapcloser_OnEnemyGapCloser(gapcloser);
        }

        private static void Interrupter_OnPosibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell) {
            champClass.Interrupter_OnPossibleToInterrupt(unit, spell);
        }

        private static void Orbwalking_AfterAttack(Obj_AI_Base unit, Obj_AI_Base target) {
            champClass.Orbwalking_AfterAttack(unit, target);
        }

        private static void Drawing_OnDraw(EventArgs args) {
             champClass.Drawing_OnDraw(args);
        }

        private static void Game_OnGameUpdate(EventArgs args) {
            // Update the combo and harass values.
            champClass.ComboActive = champClass.Config.Item("Orbwalk").GetValue<KeyBind>().Active && (((ObjectManager.Player.Mana / ObjectManager.Player.MaxMana) * 100) > Config.Item("comboMana").GetValue<Slider>().Value);
            champClass.HarassActive = champClass.Config.Item("Farm").GetValue<KeyBind>().Active && (((ObjectManager.Player.Mana / ObjectManager.Player.MaxMana) * 100) > Config.Item("harassMana").GetValue<Slider>().Value);
            champClass.Game_OnGameUpdate(args);
        }
    }
}
