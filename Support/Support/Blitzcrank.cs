#region

using System;
using System.Linq;
using System.Drawing;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

#endregion

namespace Support {
    internal class Blitzcrank : Champion {
        //Internal version 0.1

        public Spell Q;
        public Spell W;
        public Spell E;
        public Spell R;

        public Blitzcrank() {
            Utils.PrintMessage("Blitzcrank Loaded");

            Q = new Spell(SpellSlot.Q, 1000f);
            W = new Spell(SpellSlot.W, 125f);
            E = new Spell(SpellSlot.E, 125f);
            R = new Spell(SpellSlot.R, 600f);

            Q.SetSkillshot(0.5f, 70f, 1000f, true, SkillshotType.SkillshotLine);
        }

        public override void Drawing_OnDraw(EventArgs args) {
            Spell[] spellList = { Q, R };
            foreach (var spell in spellList) {
                var menuItem = GetValue<Circle>("Draw" + spell.Slot);
                if (menuItem.Active)
                    Utility.DrawCircle(ObjectManager.Player.Position, spell.Range, menuItem.Color);
            }
        }

        public override void Interrupter_OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell) {
            if (!GetValue<bool>("InterruptSpells")) return;

            if (ObjectManager.Player.Distance(unit) < Q.Range && Q.IsReady()) {
                Q.Cast(unit);

            } else if (ObjectManager.Player.Distance(unit) < R.Range && R.IsReady()) {
                R.Cast();
            }
        }

        public override void Game_OnGameUpdate(EventArgs args) {
            var useQ = Config.Item("UseQCombo").GetValue<bool>();
            var useW = Config.Item("UseWCombo").GetValue<bool>();
            var useE = Config.Item("UseECombo").GetValue<bool>();
            var useR = Config.Item("UseRCombo").GetValue<bool>();
            var useRKS = Config.Item("UseRKSCombo").GetValue<bool>();

            if (useQ && Q.IsReady()) {
                if (ComboActive) {
                    var t = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
                    if (t != null) {
                        //Q.Cast(Q.GetPrediction(t).Position.To2D().Extend(ObjectManager.Player.ServerPosition.To2D(), -Vector3.Distance(ObjectManager.Player.Position, t.Position) + 30));
                        Q.Cast(t);
                    }
                }
            }

            if (useE && E.IsReady()) {
                var t = SimpleTs.GetTarget(125, SimpleTs.DamageType.Magical);
                if (t != null) {
                    E.Cast();
                    Orbwalking.ResetAutoAttackTimer();
                    ObjectManager.Player.IssueOrder(GameObjectOrder.AttackUnit, t);
                }
            }



            if (useR && R.IsReady() && Utils.EnemyInRange(Config.Item("UseRACombo").GetValue<Slider>().Value, R.Range)) {
                ObjectManager.Player.Spellbook.CastSpell(SpellSlot.R);
            }

            if (useR && useRKS && R.IsReady()) {
                foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != ObjectManager.Player.Team && enemy.IsValidTarget() && enemy.IsVisible)) {
                    if (enemy != null) {
                        var RDMG = ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.Q) - 10;
                        if (enemy.Health < RDMG) {
                            R.Cast();
                        }
                    }
                }
            }
        }



        public override void ComboMenu(Menu config) {

            Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "Use R").SetValue(true));
            config.AddItem(new MenuItem("spacer", "--- Options ---"));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseRKSCombo", "Auto R - KS").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseRACombo", "Auto R - Enemys in Range").SetValue(new Slider(1, 5, 0)));
        }


        public override void DrawingMenu(Menu config) {
            config.AddItem(
                new MenuItem("DrawQ" + Id, "Q range").SetValue(new Circle(true,
                    System.Drawing.Color.FromArgb(100, 255, 0, 255))));
            config.AddItem(
                new MenuItem("DrawW" + Id, "W range").SetValue(new Circle(true,
                    System.Drawing.Color.FromArgb(100, 255, 0, 255))));
            config.AddItem(
                new MenuItem("DrawE" + Id, "E range").SetValue(new Circle(false,
                    System.Drawing.Color.FromArgb(100, 255, 0, 255))));
            config.AddItem(
                new MenuItem("DrawR" + Id, "R range").SetValue(new Circle(false,
                    System.Drawing.Color.FromArgb(100, 255, 0, 255))));
        }

        public override void MiscMenu(Menu config) {
            config.AddItem(new MenuItem("InterruptSpells" + Id, "Use E to Interrupt Spells").SetValue(true));
        }
    }
}