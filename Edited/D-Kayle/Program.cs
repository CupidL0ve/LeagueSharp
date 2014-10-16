using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace D_Kayle
{
    class Program
    {
        private const string ChampionName = "Kayle";

        private static Orbwalking.Orbwalker _orbwalker;

        private static Spell _q, _w, _e, _r;

        private static readonly List<Spell> SpellList = new List<Spell>();

        private static SpellSlot _igniteSlot;

        private static Menu _config;

        private static Obj_AI_Hero _player;

        private static bool _recall;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            _player = ObjectManager.Player;
            if (ObjectManager.Player.BaseSkinName != ChampionName) return;


            _q = new Spell(SpellSlot.Q, 650f);
            _w = new Spell(SpellSlot.W, 900f);
            _e = new Spell(SpellSlot.E, 0f);
            _r = new Spell(SpellSlot.R, 900f);


            SpellList.Add(_q);
            SpellList.Add(_w);
            SpellList.Add(_e);
            SpellList.Add(_r);

            _igniteSlot = _player.GetSpellSlot("SummonerDot");

            //D Nidalee
            _config = new Menu("D-Kayle", "D-Kayle", true);

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
            _config.SubMenu("Combo").AddItem(new MenuItem("ActiveCombo", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));
            _config.SubMenu("Combo").AddItem(new MenuItem("Espace", "Escapes key").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));


            //Extra
            _config.AddSubMenu(new Menu("Heal", "Heal"));
            _config.SubMenu("Heal").AddItem(new MenuItem("onmeW", "W Self")).SetValue(true);
            _config.SubMenu("Heal").AddItem(new MenuItem("healper", "Self Health %")).SetValue(new Slider(40, 1, 100));
            _config.SubMenu("Heal").AddItem(new MenuItem("allyW", "W Ally")).SetValue(true);
            _config.SubMenu("Heal").AddItem(new MenuItem("allyhealper", "Ally Health %")).SetValue(new Slider(40, 1, 100));

            //Extra
            _config.AddSubMenu(new Menu("Ulti", "Ulti"));
            _config.SubMenu("Ulti").AddItem(new MenuItem("onmeR", "R Self Use")).SetValue(true);
            _config.SubMenu("Ulti").AddItem(new MenuItem("ultiSelfHP", "Self Health %")).SetValue(new Slider(40, 1, 100));
            _config.SubMenu("Ulti").AddItem(new MenuItem("allyR", "R Ally Use")).SetValue(true);
            _config.SubMenu("Ulti").AddItem(new MenuItem("ultiallyHP", "Ally Health %")).SetValue(new Slider(40, 1, 100));


            //Harass
            _config.AddSubMenu(new Menu("Harass", "Harass"));
            _config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Use Q")).SetValue(true);
            _config.SubMenu("Harass").AddItem(new MenuItem("UseEHarass", "Use E")).SetValue(true);
            _config.SubMenu("Harass").AddItem(new MenuItem("ActiveHarass", "Harass key").SetValue(new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));
            _config.SubMenu("Harass").AddItem(new MenuItem("Harrasmana", "Minimum Mana").SetValue(new Slider(60, 1, 100)));

            //Farm
            _config.AddSubMenu(new Menu("Lane Clear", "Lane"));
            _config.SubMenu("Lane").AddItem(new MenuItem("UseELane", "Use E")).SetValue(true);
            _config.SubMenu("Lane").AddItem(new MenuItem("LaneClear", "Clear key").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
            _config.SubMenu("Lane").AddItem(new MenuItem("Lanemana", "Minimum Mana").SetValue(new Slider(60, 1, 100)));

            //JUngleClear
            _config.AddSubMenu(new Menu("JungleClear", "JungleClear"));
            _config.SubMenu("JungleClear").AddItem(new MenuItem("UseQjungle", "Use Q")).SetValue(true);
            _config.SubMenu("JungleClear").AddItem(new MenuItem("UseEjungle", "Use E")).SetValue(true);
            _config.SubMenu("JungleClear").AddItem(new MenuItem("JungleClear", "Clear key").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
            _config.SubMenu("JungleClear").AddItem(new MenuItem("Junglemana", "Minimum Mana").SetValue(new Slider(60, 1, 100)));

            //Kill Steal
            _config.AddSubMenu(new Menu("KillSteal", "Ks"));
            _config.SubMenu("Ks").AddItem(new MenuItem("ActiveKs", "Use KillSteal")).SetValue(true);
            _config.SubMenu("Ks").AddItem(new MenuItem("UseQKs", "Use Q")).SetValue(true);
            _config.SubMenu("Ks").AddItem(new MenuItem("UseIgnite", "Use Ignite")).SetValue(true);


            //Drawings
            _config.AddSubMenu(new Menu("Drawings", "Drawings"));
            _config.SubMenu("Drawings").AddItem(new MenuItem("DrawQ", "Draw Q")).SetValue(true);
            _config.SubMenu("Drawings").AddItem(new MenuItem("CircleLag", "Lag Free Circles").SetValue(true));
            _config.SubMenu("Drawings").AddItem(new MenuItem("CircleQuality", "Circles Quality").SetValue(new Slider(100, 100, 10)));
            _config.SubMenu("Drawings").AddItem(new MenuItem("CircleThickness", "Circles Thickness").SetValue(new Slider(1, 10, 1)));

            _config.AddToMainMenu();

            Game.OnGameUpdate += Game_OnGameUpdate;
            Obj_AI_Hero.OnCreate += OnCreateObj;
            Obj_AI_Hero.OnDelete += OnDeleteObj;
            Drawing.OnDraw += Drawing_OnDraw;
            Game.PrintChat("<font color='#881df2'>Kayle By Diabaths </font>Loaded!");
        }



        private static void Game_OnGameUpdate(EventArgs args)
        {

            _player = ObjectManager.Player;
            _orbwalker.SetAttack(true);
            AutoW();
            AutoR();
            AllyR();
            AllyW();
            if (_config.Item("Espace").GetValue<KeyBind>().Active)
            {
                Escape();
            }
            if (_config.Item("ActiveCombo").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            if (_config.Item("ActiveHarass").GetValue<KeyBind>().Active)
            {
                Harass();
            }
            if (_config.Item("LaneClear").GetValue<KeyBind>().Active)
            {
                Farm();
            }
            if (_config.Item("JungleClear").GetValue<KeyBind>().Active)
            {
                JungleFarm();
            }
            if (_config.Item("ActiveKs").GetValue<bool>())
            {
                KillSteal();
            }
        }

        private static void AutoR()
        {
            if (_player.HasBuff("Recall")) return;
            if (_config.Item("onmeR").GetValue<bool>() && _config.Item("onmeR").GetValue<bool>() && (_player.Health / _player.MaxHealth) * 100 <= _config.Item("ultiSelfHP").GetValue<Slider>().Value && _r.IsReady() && Utility.CountEnemysInRange(650) > 0)
            {
                _r.Cast(_player);
            }
        }

        private static void AllyR()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly))
            {
                if (_player.HasBuff("Recall")) return;
                if (_config.Item("allyR").GetValue<bool>() && (hero.Health / hero.MaxHealth) * 100 <= _config.Item("ultiallyHP").GetValue<Slider>().Value && _r.IsReady() && Utility.CountEnemysInRange(1000) > 0 && hero.Distance(_player.ServerPosition) <= _r.Range)
                {
                    _r.Cast(hero);
                }
            }
        }

        private static void Combo()
        {
            var target = SimpleTs.GetTarget(_q.Range, SimpleTs.DamageType.Magical);

            if (target != null)
            {
                if (_config.Item("UseQCombo").GetValue<bool>() && _q.IsReady())
                {
                    _q.Cast(target);

                }
                if (_config.Item("UseECombo").GetValue<bool>() && _e.IsReady() && Utility.CountEnemysInRange(650) > 0)
                {
                    _e.Cast();
                }

                if (_config.Item("UseWCombo").GetValue<bool>() && (_player.Health / _player.MaxHealth) * 100 >=
                _config.Item("healper").GetValue<Slider>().Value && _w.IsReady() && Utility.CountEnemysInRange(650) > 0)
                {
                    _w.Cast(_player);
                }
            }
        }


        private static void Escape()
        {
            var target = SimpleTs.GetTarget(_q.Range, SimpleTs.DamageType.Magical);
            if (_player.Spellbook.CanUseSpell(SpellSlot.W) == SpellState.Ready && _player.IsMe)
            {
                if (target != null && Utility.CountEnemysInRange(1200) > 0)
                {
                    _player.Spellbook.CastSpell(SpellSlot.W, _player);
                }
            }
            if (_player.Distance(target) <= _w.Range && (target != null) && _q.IsReady())
            {
                _q.Cast(target);
            }
        }

        private static void Harass()
        {
            var target = SimpleTs.GetTarget(_q.Range, SimpleTs.DamageType.Magical);
            if (target != null)
            {

                if (_config.Item("UseQHarass").GetValue<bool>() && _q.IsReady() && (100 * (_player.Mana / _player.MaxMana)) > _config.Item("Harrasmana").GetValue<Slider>().Value)
                {
                    _q.Cast(target);
                }

                if (_config.Item("UseEHarass").GetValue<bool>() && (100 * (_player.Mana / _player.MaxMana)) > _config.Item("Harrasmana").GetValue<Slider>().Value)
                    _e.Cast();
            }
        }

        private static void Farm()
        {
            if (!Orbwalking.CanMove(40)) return;
            var Minions = MinionManager.GetMinions(_player.ServerPosition, _q.Range);
            if (Minions.Count < 2) return;
            if (_config.Item("UseELane").GetValue<bool>() && _e.IsReady() && (100 * (_player.Mana / _player.MaxMana)) > _config.Item("Lanemana").GetValue<Slider>().Value)
            {

                foreach (var minion in Minions)
                {
                    if (minion.IsValidTarget())
                    {

                        _e.Cast();
                        return;
                    }
                }
            }
        }
        private static void JungleFarm()
        {
            if (!Orbwalking.CanMove(40)) return;
            var minions = MinionManager.GetMinions(_player.ServerPosition, 650, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (_config.Item("UseQjungle").GetValue<bool>() && _q.IsReady() && (100 * (_player.Mana / _player.MaxMana)) > _config.Item("Junglemana").GetValue<Slider>().Value)
            {

                foreach (var minion in minions)
                {
                    if (minion.IsValidTarget() && _player.Distance(minion) <= _q.Range)
                    {

                        _q.Cast(minion);
                        return;
                    }
                }
            }
            else if (_config.Item("UseEjungle").GetValue<bool>() && _e.IsReady() && (100 * (_player.Mana / _player.MaxMana)) > _config.Item("Junglemana").GetValue<Slider>().Value)
            {
                foreach (var minion in minions)
                {
                    if (minion.IsValidTarget())
                    {

                        _e.Cast();
                        return;
                    }
                }
            }
        }
        private static void AutoW()
        {
            if (_player.Spellbook.CanUseSpell(SpellSlot.W) == SpellState.Ready && _player.IsMe)
            {

                if (_player.HasBuff("Recall")) return;

                if (_config.Item("onmeW").GetValue<bool>() && _player.Health <= (_player.MaxHealth * (_config.Item("healper").GetValue<Slider>().Value) / 100))
                {
                    _player.Spellbook.CastSpell(SpellSlot.W, _player);
                }
            }
        }

        private static void AllyW()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly && !hero.IsMe))
            {
                if (_player.HasBuff("Recall") || hero.HasBuff("Recall")) return;
                if (_config.Item("allyW").GetValue<bool>() && (hero.Health / hero.MaxHealth) * 100 <= _config.Item("allyhealper").GetValue<Slider>().Value && _w.IsReady() && Utility.CountEnemysInRange(1200) > 0 && hero.Distance(_player.ServerPosition) <= _w.Range)
                {
                    _w.Cast(hero);
                }
            }
        }
        private static void KillSteal()
        {
            var target = SimpleTs.GetTarget(_q.Range, SimpleTs.DamageType.Magical);
            var igniteDmg = _player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
            var qhDmg = _player.GetSpellDamage(target, SpellSlot.Q);

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
                    _q.Cast(target);
                }
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (_config.Item("CircleLag").GetValue<bool>())
            {
                if (_config.Item("DrawQ").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, _q.Range, System.Drawing.Color.Orange,
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
            }
        }

        private static void OnCreateObj(GameObject sender, EventArgs args)
        {
            //Recall
            if (!(sender is Obj_GeneralParticleEmmiter)) return;
            var obj = (Obj_GeneralParticleEmmiter)sender;
            if (obj.IsMe && obj.Name == "TeleportHome")
            {
                _recall = true;
            }
        }

        private static void OnDeleteObj(GameObject sender, EventArgs args)
        {
            //Recall
            if (!(sender is Obj_GeneralParticleEmmiter)) return;
            var obj = (Obj_GeneralParticleEmmiter)sender;
            if (obj.IsMe && obj.Name == "TeleportHome")
            {
                _recall = false;
            }
        }
    }
}