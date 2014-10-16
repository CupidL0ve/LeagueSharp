using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace D_Diana
{
    class Program
    {
        private const string ChampionName = "Diana";

        private static Orbwalking.Orbwalker _orbwalker;

        private static Spell _q, _w, _e, R;

        private static Obj_SpellMissile _qpos;

        private static bool _qcreated = false;

        private static Menu _config;

        private static Items.Item _dfg;

        private static Obj_AI_Hero _player;

        private static readonly List<Spell> SpellList = new List<Spell>();

        private static SpellSlot _igniteSlot;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            _player = ObjectManager.Player;
            if (ObjectManager.Player.BaseSkinName != ChampionName) return;

            _q = new Spell(SpellSlot.Q, 830f);
            _w = new Spell(SpellSlot.W, 200f);
            _e = new Spell(SpellSlot.E, 420f);
            R = new Spell(SpellSlot.R, 825f);

            _q.SetSkillshot(0.35f, 200f, 1800, false, SkillshotType.SkillshotCircle);

            SpellList.Add(_q);
            SpellList.Add(_w);
            SpellList.Add(_e);
            SpellList.Add(R);

            _dfg = new Items.Item(3128, 750f);

            _igniteSlot = _player.GetSpellSlot("SummonerDot");

            //D Diana
            _config = new Menu("D-Diana", "D-Diana", true);

            //TargetSelector
            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            SimpleTs.AddToMenu(targetSelectorMenu);
            _config.AddSubMenu(targetSelectorMenu);

            //Orbwalker
            _config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            _orbwalker = new Orbwalking.Orbwalker(_config.SubMenu("Orbwalking"));

            //Combo
            _config.AddSubMenu(new Menu("Combo", "Combo"));
            _config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "Use R")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("UseRSecond", "Use Second R")).SetValue(false);
            _config.SubMenu("Combo").AddItem(new MenuItem("UseItems", "Use Items")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("ActiveCombo", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));
            //_config.SubMenu("Combo").AddItem(new MenuItem("ActiveCombo2", "Combo2!").SetValue(new KeyBind(32, KeyBindType.Press)));

            //Extra
            _config.AddSubMenu(new Menu("Extra", "Extra"));
            _config.SubMenu("Extra").AddItem(new MenuItem("usePackets", "Usepackes")).SetValue(true);
            _config.SubMenu("Extra").AddItem(new MenuItem("AutoShield", "Auto W")).SetValue(true);
            _config.SubMenu("Extra").AddItem(new MenuItem("Shieldper", "Self Health %")).SetValue(new Slider(40, 1, 100));
            _config.SubMenu("Extra").AddItem(new MenuItem("Escape", "Escape Key!").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
            _config.SubMenu("Extra").AddItem(new MenuItem("Inter_E", "Interrupter E")).SetValue(true);
            _config.SubMenu("Extra").AddItem(new MenuItem("Gap_W", "GapClosers W")).SetValue(true);
            
            //Harass
            _config.AddSubMenu(new Menu("Harass", "Harass"));
            _config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Use Q")).SetValue(true);
            _config.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "Use W")).SetValue(true);
            _config.SubMenu("Harass").AddItem(new MenuItem("ActiveHarass", "Harass key").SetValue(new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));
            _config.SubMenu("Harass").AddItem(new MenuItem("harasstoggle", "Harass(toggle)").SetValue(new KeyBind("G".ToCharArray()[0], KeyBindType.Toggle)));
            _config.SubMenu("Harass").AddItem(new MenuItem("Harrasmana", "Minimum Mana").SetValue(new Slider(60, 1, 100)));

            //Harass
            _config.AddSubMenu(new Menu("Lane", "Lane"));
            _config.SubMenu("Lane").AddItem(new MenuItem("UseQLane", "Use Q")).SetValue(true);
            _config.SubMenu("Lane").AddItem(new MenuItem("UseWLane", "Use W")).SetValue(true);
            _config.SubMenu("Lane").AddItem(new MenuItem("ActiveLane", "Farm key").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
            _config.SubMenu("Lane").AddItem(new MenuItem("Lanemana", "Minimum Mana").SetValue(new Slider(60, 1, 100)));

            //jungle
            _config.AddSubMenu(new Menu("Jungle", "Jungle"));
            _config.SubMenu("Jungle").AddItem(new MenuItem("UseQJungle", "Use Q")).SetValue(true);
            _config.SubMenu("Jungle").AddItem(new MenuItem("UseWJungle", "Use W")).SetValue(true);
            _config.SubMenu("Jungle").AddItem(new MenuItem("ActiveJungle", "Jungle key").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
            _config.SubMenu("Jungle").AddItem(new MenuItem("Junglemana", "Minimum Mana").SetValue(new Slider(60, 1, 100)));

            //Kill Steal
            _config.AddSubMenu(new Menu("KillSteal", "Ks"));
            _config.SubMenu("Ks").AddItem(new MenuItem("ActiveKs", "Use KillSteal")).SetValue(true);
            _config.SubMenu("Ks").AddItem(new MenuItem("UseQKs", "Use Q")).SetValue(true);
            _config.SubMenu("Ks").AddItem(new MenuItem("UseRKs", "Use R")).SetValue(true);
            _config.SubMenu("Ks").AddItem(new MenuItem("TargetRange", "R use if range >").SetValue(new Slider(400, 200, 600)));
            _config.SubMenu("Ks").AddItem(new MenuItem("UseIgnite", "Use Ignite")).SetValue(true);

            //Drawings
            _config.AddSubMenu(new Menu("Drawings", "Drawings"));
            _config.SubMenu("Drawings").AddItem(new MenuItem("DrawQ", "Draw Q")).SetValue(true);
            _config.SubMenu("Drawings").AddItem(new MenuItem("DrawW", "Draw W")).SetValue(true);
            _config.SubMenu("Drawings").AddItem(new MenuItem("DrawE", "Draw E")).SetValue(true);
            _config.SubMenu("Drawings").AddItem(new MenuItem("DrawR", "Draw R")).SetValue(true);
            _config.SubMenu("Drawings").AddItem(new MenuItem("ShowPassive", "Show Passive")).SetValue(true);
            _config.SubMenu("Drawings").AddItem(new MenuItem("CircleLag", "Lag Free Circles").SetValue(true));
            _config.SubMenu("Drawings").AddItem(new MenuItem("CircleQuality", "Circles Quality").SetValue(new Slider(100, 100, 10)));
            _config.SubMenu("Drawings").AddItem(new MenuItem("CircleThickness", "Circles Thickness").SetValue(new Slider(1, 10, 1)));

            _config.AddToMainMenu();

            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            GameObject.OnCreate += OnCreate;
            GameObject.OnDelete += OnDelete;
            Game.PrintChat("<font color='#881df2'>Diana By Diabaths With Misaya Combo by xSalice </font>Loaded!");
           // Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            _player = ObjectManager.Player;

            _orbwalker.SetAttack(true);

            if (_config.Item("ActiveCombo").GetValue<KeyBind>().Active)
            {
               Misaya();
            }
           /* if (_config.Item("ActiveCombo2").GetValue<KeyBind>().Active)
            {
                misaya2();
            }*/
            if ((_config.Item("ActiveHarass").GetValue<KeyBind>().Active || _config.Item("harasstoggle").GetValue<KeyBind>().Active) && (100 * (_player.Mana / _player.MaxMana)) > _config.Item("Harrasmana").GetValue<Slider>().Value)
            {
                Harass();
            }
            if (_config.Item("ActiveLane").GetValue<KeyBind>().Active && (100 * (_player.Mana / _player.MaxMana)) > _config.Item("Lanemana").GetValue<Slider>().Value)
            {
                Farm();
            }
            if (_config.Item("ActiveJungle").GetValue<KeyBind>().Active && (100 * (_player.Mana / _player.MaxMana)) > _config.Item("Junglemana").GetValue<Slider>().Value)
            {
                JungleClear();
            }
            if (_config.Item("Escape").GetValue<KeyBind>().Active)
            {
                Tragic();
            }
            if (_config.Item("ActiveKs").GetValue<bool>())
            {
                KillSteal();
            }
            if (_config.Item("AutoShield").GetValue<bool>() && !_config.Item("ActiveCombo").GetValue<KeyBind>().Active)
            {
                AutoW();
            }
         }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (_w.IsReady() && gapcloser.Sender.IsValidTarget(_w.Range) && _config.Item("Gap_W").GetValue<bool>())
            {
                _w.Cast(gapcloser.Sender, Packets());
            }
        }
        private static void Interrupter_OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (_e.IsReady() && unit.IsValidTarget(_e.Range) && _config.Item("Inter_E").GetValue<bool>())
                _e.Cast();
        }

        private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                Game.PrintChat("Spell name: " + args.SData.Name.ToString());
            }
        }
        //misaya combo by xSalice
        private static void Misaya()
        {
            var target = SimpleTs.GetTarget(_q.Range, SimpleTs.DamageType.Magical);
            if (target != null)
            {
                if (_player.Distance(target) <= _dfg.Range && _config.Item("UseItems").GetValue<bool>() && _dfg.IsReady() && target.Health <= ComboDamage(target))
                {
                    _dfg.Cast(target);
                }

                if (_player.Distance(target) <= _q.Range && _config.Item("UseQCombo").GetValue<bool>() && _q.IsReady() && _q.GetPrediction(target).Hitchance >= HitChance.High)
                {
                    _q.CastIfHitchanceEquals(target, HitChance.High, Packets());
                }
                if (_player.Distance(target) <= R.Range && _config.Item("UseRCombo").GetValue<bool>() && R.IsReady() && ((_qcreated == true)
                    || target.HasBuff("dianamoonlight", true)))
                {
                    R.Cast(target, Packets());
                }
                if (_player.Distance(target) <= _w.Range && _config.Item("UseWCombo").GetValue<bool>() && _w.IsReady() && !_q.IsReady())
                {
                    _w.Cast();
                }
                if (_player.Distance(target) <= _e.Range && _player.Distance(target) >= _w.Range && _config.Item("UseECombo").GetValue<bool>() && _e.IsReady() && !_w.IsReady())
                {
                    _e.Cast();
                }
                if (_player.Distance(target) <= R.Range && _config.Item("UseRSecond").GetValue<bool>() && R.IsReady() && !_w.IsReady() && !_q.IsReady())
                {
                    R.Cast(target, Packets());
                }
            }
        }

      /*  public static void misaya2()
        {
            var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            if (target != null)
            {
                if (Player.Distance(target) <= DFG.Range && _config.Item("UseItems").GetValue<bool>() && DFG.IsReady() && target.Health <= ComboDamage(target))
                {
                    DFG.Cast(target);
                }

                if (Player.Distance(target) <= R.Range && _config.Item("UseRCombo").GetValue<bool>() && R.IsReady() && Q.IsReady())
                {
                    R.Cast(target, true);
                    Q.Cast(target);
                    return;
                }
                if (Player.Distance(target) <= W.Range && _config.Item("UseWCombo").GetValue<bool>() && W.IsReady() && !Q.IsReady())
                {
                    W.Cast();
                }
                if (Player.Distance(target) <= E.Range && Player.Distance(target) >= W.Range && _config.Item("UseECombo").GetValue<bool>() && E.IsReady() && !W.IsReady())
                {
                    E.Cast();
                }
              
              }
        }*/

        private static float ComboDamage(Obj_AI_Hero hero)
        {
           
            var dmg = 0d;

            if (_q.IsReady())
                dmg += _player.GetSpellDamage(hero, SpellSlot.Q);
            if (_w.IsReady())
                dmg += _player.GetSpellDamage(hero, SpellSlot.W);
            if (R.IsReady())
                dmg += _player.GetSpellDamage(hero, SpellSlot.R) * 2;
             if (Items.HasItem(3128))
            {
                dmg += _player.GetItemDamage(hero, Damage.DamageItems.Dfg);
                dmg = dmg * 1.2;
            }
            if (ObjectManager.Player.GetSpellSlot("SummonerIgnite") != SpellSlot.Unknown)
            {
                dmg += _player.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Ignite);
            }
            return (float)dmg;
        }

        private static void Harass()
        {
            var target = SimpleTs.GetTarget(_q.Range, SimpleTs.DamageType.Magical);
            if (target != null)
            {
                if (_player.Distance(target) <= _q.Range && _config.Item("UseQHarass").GetValue<bool>() && _q.IsReady())
                {
                    _q.CastIfHitchanceEquals(target, HitChance.Medium, Packets());
                }
                if (_player.Distance(target) <= 200 && _config.Item("UseWHarass").GetValue<bool>() && _w.IsReady())
                {
                    _w.Cast();
                }
            }
        }

        private static void Farm()
        {
            if (!Orbwalking.CanMove(40)) return;

            var rangedMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _q.Range + _q.Width + 30,
            MinionTypes.Ranged);
            var allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _q.Range + _q.Width + 30,
            MinionTypes.All);
            var allMinionsW = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _w.Range, MinionTypes.All);

            var useQ = _config.Item("UseQLane").GetValue<bool>();
            var useW = _config.Item("UseWLane").GetValue<bool>();
            if (_q.IsReady() && useQ)
            {
                var fl1 = _q.GetCircularFarmLocation(rangedMinionsQ, _q.Width);
                var fl2 = _q.GetCircularFarmLocation(allMinionsQ, _q.Width);

                if (fl1.MinionsHit >= 3)
                {
                    _q.Cast(fl1.Position);
                }
                else if (fl2.MinionsHit >= 2 || allMinionsQ.Count == 1)
                {
                    _q.Cast(fl2.Position);
                }
                else
                    foreach (var minion in allMinionsQ)
                        if (!Orbwalking.InAutoAttackRange(minion) &&
                        minion.Health < 0.75 * _player.GetSpellDamage(minion, SpellSlot.Q))
                            _q.Cast(minion);
            }
            if (_w.IsReady() && useW && allMinionsW.Count > 2)
            {
                _w.Cast(allMinionsW[0]);
            }
        }

        private static void Tragic()
        {
            var allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _q.Range + _q.Width + 30,
            MinionTypes.All);
            var mobs = MinionManager.GetMinions(_player.ServerPosition, _q.Range,
            MinionTypes.All,
            MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            if (_q.IsReady()) _q.Cast(Game.CursorPos);
            if (R.IsReady())
            {
                if (mobs.Count > 0)
                {
                    var mob = mobs[0];

                    R.CastOnUnit(mob);
                }
                else
                    if (allMinionsQ.Count >= 1)
                    {
                        R.Cast(allMinionsQ[0]);
                    }
            }
        }
        private static void JungleClear()
        {
            var mobs = MinionManager.GetMinions(_player.ServerPosition, _q.Range,
  MinionTypes.All,
  MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            if (mobs.Count > 0)
            {
                var mob = mobs[0];
                _q.CastOnUnit(mob);
                _w.CastOnUnit(mob);
            }
        }

        private static void KillSteal()
        {
            var target = SimpleTs.GetTarget(_q.Range, SimpleTs.DamageType.Magical);
            var igniteDmg = _player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
            var qhDmg = _player.GetSpellDamage(target, SpellSlot.Q);
            var rhDmg = _player.GetSpellDamage(target, SpellSlot.R);
            var rRange = (_player.Distance(target) >= _config.Item("TargetRange").GetValue<Slider>().Value);
            if (target != null && _config.Item("UseIgnite").GetValue<bool>() && _igniteSlot != SpellSlot.Unknown &&
            _player.SummonerSpellbook.CanUseSpell(_igniteSlot) == SpellState.Ready)
            {
                if (igniteDmg > target.Health)
                {
                    _player.SummonerSpellbook.CastSpell(_igniteSlot, target);
                }
            }

            if (_q.IsReady() && _player.Distance(target) <= _q.Range && target != null && _config.Item("UseQKs").GetValue<bool>())
            {
                if (target.Health <= qhDmg)
                {
                    _q.Cast(target, Packets());
                }
            }

            if (R.IsReady() && _player.Distance(target) <= R.Range && rRange && target != null && _config.Item("UseRKs").GetValue<bool>())
            {
                if (target.Health <= rhDmg)
                {
                    R.Cast(target, Packets());
                }
            }
        }

        private static void AutoW()
        {
            if (_player.HasBuff("Recall")) return;
            if (_w.IsReady() && _player.Health <= (_player.MaxHealth * (_config.Item("Shieldper").GetValue<Slider>().Value) / 100))
            {
                _w.Cast();
            }

        }
       private static bool Packets()
        {
            return _config.Item("usePackets").GetValue<bool>();
        }

        private static void OnCreate(GameObject sender, EventArgs args)
        {
            var spell = (Obj_SpellMissile)sender;
            var unit = spell.SpellCaster.Name;
            var name = spell.SData.Name;

            //debug
           //if (unit == ObjectManager.Player.Name)


            if (unit == ObjectManager.Player.Name && (name == "dianaarcthrow"))
            {
               // Game.PrintChat("Spell: " + name);
                _qpos = spell;
                _qcreated = true;
                return;
            }
        }
       
        //misaya by xSalice
        private static void OnDelete(GameObject sender, EventArgs args)
        {
            var spell = (Obj_SpellMissile)sender;
            var unit = spell.SpellCaster.Name;
            var name = spell.SData.Name;

            if (unit == ObjectManager.Player.Name && (name == "dianaarcthrow"))
            {
                _qpos = null;
                _qcreated = false;
                return;
            }
        }
    
        private static void Drawing_OnDraw(EventArgs args)
        {
            var diana = Drawing.WorldToScreen(_player.Position);
            if (_config.Item("CircleLag").GetValue<bool>())
            {
                if (_qpos != null)
                    Utility.DrawCircle(_qpos.Position, _qpos.BoundingRadius, System.Drawing.Color.Red, 5, 30, false);
                if (_config.Item("ShowPassive").GetValue<bool>())
                {
                    if (_player.HasBuff("dianaarcready"))
                        Drawing.DrawText(diana[0] - 10, diana[1], Color.White, "P On");
                    else
                        Drawing.DrawText(diana[0] - 10, diana[1], Color.Orange, "P Off");
                }

                if (_config.Item("DrawQ").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, _q.Range, System.Drawing.Color.White,
                        _config.Item("CircleThickness").GetValue<Slider>().Value,
                        _config.Item("CircleQuality").GetValue<Slider>().Value);
                }
                if (_config.Item("DrawW").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, _w.Range, System.Drawing.Color.White,
                        _config.Item("CircleThickness").GetValue<Slider>().Value,
                        _config.Item("CircleQuality").GetValue<Slider>().Value);
                }
                if (_config.Item("DrawE").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, _e.Range, System.Drawing.Color.White,
                        _config.Item("CircleThickness").GetValue<Slider>().Value,
                        _config.Item("CircleQuality").GetValue<Slider>().Value);
                }
                if (_config.Item("DrawR").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, R.Range, System.Drawing.Color.White,
                        _config.Item("CircleThickness").GetValue<Slider>().Value,
                        _config.Item("CircleQuality").GetValue<Slider>().Value);
                }
            }
            else
            {
                if (_config.Item("DrawQ").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, _q.Range, System.Drawing.Color.White);
                }
                if (_config.Item("DrawW").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, _w.Range, System.Drawing.Color.White);
                }
                if (_config.Item("DrawE").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, _e.Range, System.Drawing.Color.White);
                }

                if (_config.Item("DrawR").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, R.Range, System.Drawing.Color.White);
                }

            }
        }
    }
}
