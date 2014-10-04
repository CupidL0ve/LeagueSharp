using LeagueSharp;
using LeagueSharp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Support {
    class Leona : Champion {

        public Spell Q;
        public Spell W;
        public Spell E;
        public Spell R;
        // Items Consants.
        public const int FrostQueen = 3092;

        public Leona() {
            Utils.PrintMessage("Leona Loaded");

            Q = new Spell(SpellSlot.Q, 0f);
            W = new Spell(SpellSlot.W, 0f);
            E = new Spell(SpellSlot.E, 900f);
            R = new Spell(SpellSlot.R, 1200f);

            E.SetSkillshot(0f, 100f, 2000f, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.7f, 315, float.MaxValue, false, SkillshotType.SkillshotCircle);

        }

        public override void Drawing_OnDraw(EventArgs args) {
            Spell[] spellList = { Q, E, R };
            foreach (var spell in spellList) {
                var menuItem = GetValue<Circle>("Draw" + spell.Slot);
                if (menuItem.Active)
                    Utility.DrawCircle(ObjectManager.Player.Position, spell.Range, menuItem.Color);
            }
        }

        public override void Game_OnGameUpdate(EventArgs args) {
            if ((!ComboActive && !HarassActive) || (!Orbwalking.CanMove(100)))
                return;
 
            var useQ = GetValue<bool>("UseQ" + (ComboActive ? "C" : "H"));
            var useW = GetValue<bool>("UseW" + (ComboActive ? "C" : "H"));
            var useE = GetValue<bool>("UseE" + (ComboActive ? "C" : "H"));
            var useR = GetValue<bool>("UseRC");

            if (GetValue<bool>("UseQE")) {
                if (useQ && Q.IsReady() && E.IsReady()) {
                    var t = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Physical);
                    if (t.IsValidTarget()) { 
                        Q.Cast();
                    }
                }
            } else {
                if (useQ && Q.IsReady()) {
                    var t = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Physical);
                    if (t.IsValidTarget()) {
                        Q.Cast();
                    }
                } 
            }

            if (useW && W.IsReady()) {
                var t = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Physical);
                if (t.IsValidTarget()) {
                    W.Cast();
                }
            }

            if (useE && E.IsReady()) {
                var t = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Physical);
                E.Cast(t, GetValue<bool>("Packets"));
            }

            if (useR && R.IsReady()) {
                var t = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Physical);
                var predOut = Prediction.GetPrediction(t, R.Delay, R.Width, R.Speed);
                if (predOut.AoeTargetsHitCount <= GetValue<Slider>("CountR").Value) {
                    R.Cast(predOut.CastPosition, GetValue<bool>("Packets"));
                }
            }

            // Item menu
            if (ComboActive) {
                // Gets the target within frostQueens range (850).
                var target = SimpleTs.GetTarget(840, SimpleTs.DamageType.Physical);
                if (Items.HasItem(FrostQueen) && Items.CanUseItem(FrostQueen)) {
                    // Grab the prediction based on arbitrary values ^.^
                    var pred = Prediction.GetPrediction(target, 0.5f, 50f, 1200f);
                    foreach (var slot in ObjectManager.Player.InventoryItems.Where(slot => slot.Id == (ItemId)FrostQueen)) {
                        if (pred.Hitchance >= HitChance.Low) {
                            slot.UseItem(pred.CastPosition);
                        }
                    }
                }
            }

            
        }

        public override void ComboMenu(Menu config) {
            config.AddItem(new MenuItem("UseQC" + Id, "Use Q").SetValue(true));
            config.AddItem(new MenuItem("UseWC" + Id, "Use W").SetValue(true));
            config.AddItem(new MenuItem("UseEC" + Id, "Use E").SetValue(true));
            config.AddItem(new MenuItem("UseRC" + Id, "Use R").SetValue(true));
            config.AddItem(new MenuItem("spacer", "--- Options ---"));
            config.AddItem(new MenuItem("CountR" + Id, "Num of Enemy in Range to Ult").SetValue(new Slider(1, 5, 0)));
        }

        public override void HarassMenu(Menu config) {
            config.AddItem(new MenuItem("UseQH" + Id, "Use Q").SetValue(true));
            config.AddItem(new MenuItem("UseWH" + Id, "Use W").SetValue(true));
            config.AddItem(new MenuItem("UseEH" + Id, "Use E").SetValue(true));
        }

        public override void DrawingMenu(Menu config) {
            config.AddItem(
                new MenuItem("DrawE" + Id, "E range").SetValue(new Circle(true,
                    System.Drawing.Color.FromArgb(100, 255, 0, 255))));
            config.AddItem(
                new MenuItem("DrawR" + Id, "R range").SetValue(new Circle(false,
                    System.Drawing.Color.FromArgb(100, 255, 0, 255))));
        }

        public override void MiscMenu(Menu config) {
            config.AddItem(new MenuItem("UseQE" + Id, "Use Q only with E").SetValue(true));
            config.AddItem(new MenuItem("Packets" + Id, "Use Packets").SetValue(true));
        } 

    }
}
