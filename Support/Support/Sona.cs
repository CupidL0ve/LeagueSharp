using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace Support {
    internal class Sona : Champion {

        public Spell Q;
        public Spell W;
        public Spell E;
        public Spell R;
        public bool Debug = false;
        PredictionOutput pred;

        // Items Consants.
        public const int FrostQueen = 3092;

        public Sona() {
            Utils.PrintMessage("Sona loaded.");

            Q = new Spell(SpellSlot.Q, 700f);
            W = new Spell(SpellSlot.W, 1000f);
            E = new Spell(SpellSlot.E, 1000f);
            R = new Spell(SpellSlot.R, 1000f);
            

            // Just setting R skillshot values.
            R.SetSkillshot(0.5f, 125f, float.MaxValue, false, SkillshotType.SkillshotLine);

        }

        public override void Game_OnGameUpdate(EventArgs args) {

            var autoMana = ((ObjectManager.Player.Mana / ObjectManager.Player.MaxMana) * 100) > GetValue<Slider>("autoMana").Value;

            if (GetValue<bool>("AutoQ") && autoMana) {
                if (Q.IsReady()) {
                    // Cast Q when target is in range
                    var t = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Physical);
                    if (Vector3.Distance(ObjectManager.Player.Position, t.Position) < Q.Range) {
                        ObjectManager.Player.Spellbook.CastSpell(SpellSlot.Q);
                    }
                }
            }

            if (GetValue<bool>("AutoW") && autoMana) {
                if (W.IsReady()) {
                    // Casts when an Ally is below the set HP threshold.
                    if (Utils.AllyBelowHP(GetValue<Slider>("AutoHeal").Value, W.Range)) {
                        ObjectManager.Player.Spellbook.CastSpell(SpellSlot.W);
                    }
                }
            }
 
            if ((!ComboActive && !HarassActive) || (!Orbwalking.CanMove(100)))
                return;
             
            // Vars that grab values based on the menu
            var useQ = GetValue<bool>("UseQ" + (ComboActive ? "C" : "H"));
            var useW = GetValue<bool>("UseW" + (ComboActive ? "C" : "H"));
            var useE = GetValue<bool>("UseEC");
            var useR = GetValue<bool>("UseRC");

            if (useQ && Q.IsReady()) {
                // Cast Q when target is in range
                var t = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Physical);
                if (Vector3.Distance(ObjectManager.Player.Position, t.Position) < Q.Range) {
                    ObjectManager.Player.Spellbook.CastSpell(SpellSlot.Q);
                }
            }

            if (useW && W.IsReady()) {
                // Casts when an Ally is below the set HP threshold.
                if (Utils.AllyBelowHP(GetValue<Slider>("AutoHeal").Value, W.Range)) {
                    ObjectManager.Player.Spellbook.CastSpell(SpellSlot.W);
                }
            }

            if (useE && E.IsReady() && ComboActive) {
                // This uses E just to proc passive :D
                ObjectManager.Player.Spellbook.CastSpell(SpellSlot.E);
            }

            if (useR && R.IsReady() && ComboActive) {
                var t = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Physical);
                var ultTar = Utils.GetEnemyHitByR(R, GetValue<Slider>("CountR").Value);
                if (ultTar != null) {
                    R.Cast(ultTar, true);
                    // Attempt at faster cast.
                    //R.CastOnUnit(ultTar, true);
                }
            }

            // Item menu
            if (ComboActive) {
                // Gets the target within frostQueens range (850).
                var target = SimpleTs.GetTarget(840, SimpleTs.DamageType.Physical);
                if (Items.HasItem(FrostQueen) && Items.CanUseItem(FrostQueen)) {
                    // Grab the prediction based on arbitrary values ^.^
                    pred = Prediction.GetPrediction(target, 0.5f, 50f, 1200f);
                    foreach (var slot in ObjectManager.Player.InventoryItems.Where(slot => slot.Id == (ItemId)FrostQueen)) {
                        if (pred.Hitchance >= HitChance.Medium) {
                            slot.UseItem(pred.CastPosition);
                        }
                    }
                }
            }

        }

        public override void Interrupter_OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell) {
            if (spell.DangerLevel == InterruptableDangerLevel.High) {
                R.Cast(unit);
            }
        }

        public override void Drawing_OnDraw(EventArgs args) {
            Spell[] spellList = { Q, W, R };
            foreach (var spell in spellList) {
                var menuItem = GetValue<Circle>("Draw" + spell.Slot);
                if (menuItem.Active)
                    Utility.DrawCircle(ObjectManager.Player.Position, spell.Range, menuItem.Color);
            }
            if (Debug) {
                //Utils.rect.ToPolygon().Draw(System.Drawing.Color.White, 2);
                Utility.DrawCircle(pred.CastPosition, 40, System.Drawing.Color.White);
            }

        }

        public override void ManaMenu(Menu config) {
            config.AddItem(new MenuItem("autoMana" + Id, "Auto Mana %").SetValue(new Slider(30, 100, 0)));    
        }
        
        public override void ItemMenu(Menu config) {
            config.AddItem(new MenuItem("frostQueen" + Id, "Use Frost Queen on Combo").SetValue(true));
        }

        public override void ComboMenu(Menu config) {
            config.AddItem(new MenuItem("UseQC" + Id, "Use Q").SetValue(true));
            config.AddItem(new MenuItem("UseWC" + Id, "Use W").SetValue(true));
            config.AddItem(new MenuItem("UseEC" + Id, "Use E").SetValue(true));
            config.AddItem(new MenuItem("UseRC" + Id, "Use R").SetValue(true));
            config.AddItem(new MenuItem("spacer", "--- Options ---"));
            config.AddItem(new MenuItem("CountR" + Id, "R If Can Hit X").SetValue(new Slider(2, 5, 0)));
        }

        public override void HarassMenu(Menu config) {
            config.AddItem(new MenuItem("UseQH" + Id, "Use Q").SetValue(false));
            config.AddItem(new MenuItem("UseWH" + Id, "Use W").SetValue(false));
        }

        public override void DrawingMenu(Menu config) {
            config.AddItem(
                new MenuItem("DrawQ" + Id, "Q range").SetValue(new Circle(true,
                    System.Drawing.Color.FromArgb(100, 255, 0, 255))));
            config.AddItem(
                new MenuItem("DrawW" + Id, "W range").SetValue(new Circle(true,
                    System.Drawing.Color.FromArgb(100, 255, 0, 255))));
            config.AddItem(
                new MenuItem("DrawR" + Id, "R range").SetValue(new Circle(false,
                    System.Drawing.Color.FromArgb(100, 255, 0, 255))));
        }

        public override void MiscMenu(Menu config) {
            config.AddItem(new MenuItem("AutoQ" + Id, "Auto cast Q").SetValue(true));
            config.AddItem(new MenuItem("AutoW" + Id, "Auto cast W").SetValue(true));
            config.AddItem(new MenuItem("AutoHeal" + Id, "Auto W when below % hp").SetValue(new Slider(60, 100, 0)));
        }

    
    }
}
