

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Security.AccessControl;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace D_Udyr
{
    internal class Program
    {

        public const string ChampionName = "Udyr";

        private static Orbwalking.Orbwalker _orbwalker;

        private static readonly List<Spell> SpellList = new List<Spell>();

        private static Spell _q;

        private static Spell _w;

        private static Spell _e;

        private static Spell _r;

        private static Menu _config;

        private static Items.Item _bilgeCut;

        private static Items.Item _boTrk;

        private static Items.Item _ravHydra;

        private static Items.Item _tiamat;

        private static Items.Item _ranOmen;

        private static Items.Item _lotis;

        private static Obj_AI_Hero _player;


        //Tiger Style
        private static readonly Spell[] Tiger = { _q, _w, _e, _q, _q, _e, _q, _e, _q, _e, _e, _w, _w, _w, _w, _r, _r, _r };
        //Phoenix Style
        private static readonly Spell[] Phoenix = { _r, _q, _e, _r, _r, _e, _r, _e, _r, _e, _e, _w, _q, _q, _q, _w, _w, _w };

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            _player = ObjectManager.Player;
            //if (ObjectManager.Player.BaseSkinName != ChampionName) return;


            _q = new Spell(SpellSlot.Q, 200);
            _w = new Spell(SpellSlot.W, 200);
            _e = new Spell(SpellSlot.E, 200);
            _r = new Spell(SpellSlot.R, 200);

            SpellList.Add(_q);
            SpellList.Add(_w);
            SpellList.Add(_e);
            SpellList.Add(_r);

            _bilgeCut = new Items.Item(3144, 475f);
            _boTrk = new Items.Item(3153, 425f);
            _ravHydra = new Items.Item(3074, 375f);
            _tiamat = new Items.Item(3077, 375f);
            _ranOmen = new Items.Item(3143, 490f);
            _lotis = new Items.Item(3190, 590f);


            //Udyr
            _config = new Menu("D-Udyr", "D-Udyr", true);

            //TargetSelector
            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            SimpleTs.AddToMenu(targetSelectorMenu);
            _config.AddSubMenu(targetSelectorMenu);

            //Orbwalker
            _config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            _orbwalker = new Orbwalking.Orbwalker(_config.SubMenu("Orbwalking"));

            //Auto Level
            _config.AddSubMenu(new Menu("Style", "Style"));
            _config.SubMenu("Style").AddItem(new MenuItem("AutoLevel", "Auto Level")).SetValue(false);
            _config.SubMenu("Style").AddItem(new MenuItem("Style", "")).SetValue(new StringList(new string[] { "Tiger", "Pheonix" }));


            //Combo
            _config.AddSubMenu(new Menu("Main", "Main"));
            _config.SubMenu("Main").AddItem(new MenuItem("AutoShield", "Auto Shield")).SetValue(true);
            _config.SubMenu("Main").AddItem(new MenuItem("AutoShield%", "AutoShield HP %").SetValue(new Slider(50, 100, 0)));
            _config.SubMenu("Main").AddItem(new MenuItem("TargetRange", "Range to Use E").SetValue(new Slider(1000, 600, 1500)));
            _config.SubMenu("Main").AddItem(new MenuItem("ActiveCombo", "Combo Key").SetValue(new KeyBind(32, KeyBindType.Press)));
            _config.SubMenu("Main").AddItem(new MenuItem("StunCycle", "Stun Cycle").SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Press)));

            //Forest gump
            _config.AddSubMenu(new Menu("Forest Gump", "Forest Gump"));
            _config.SubMenu("Forest Gump").AddItem(new MenuItem("ForestE", "Use E")).SetValue(true);
            _config.SubMenu("Forest Gump").AddItem(new MenuItem("ForestW", "Use W")).SetValue(true);
            _config.SubMenu("Forest Gump").AddItem(new MenuItem("Forest", "Forest gump(Toggle)").SetValue(new KeyBind("G".ToCharArray()[0], KeyBindType.Toggle)));
            _config.SubMenu("Forest Gump").AddItem(new MenuItem("Forest-Mana", "Forest gump Mana").SetValue(new Slider(50, 100, 0)));


            //Harass
            _config.AddSubMenu(new Menu("Items", "Items"));
            _config.SubMenu("Items").AddItem(new MenuItem("BilgeCut", "Bilgewater Cutlass")).SetValue(true);
            _config.SubMenu("Items").AddItem(new MenuItem("BoTRK", "BoT Ruined King")).SetValue(true);
            _config.SubMenu("Items").AddItem(new MenuItem("RavHydra", "Ravenous Hydra")).SetValue(true);
            _config.SubMenu("Items").AddItem(new MenuItem("RanOmen", "Randuin's Omen")).SetValue(true);
            _config.SubMenu("Items").AddItem(new MenuItem("Tiamat", "Tiamat")).SetValue(true);


            //Farm
            _config.AddSubMenu(new Menu("Lane", "Lane"));
            _config.SubMenu("Lane").AddItem(new MenuItem("Use-Q-Farm", "Use Q")).SetValue(true);
            _config.SubMenu("Lane").AddItem(new MenuItem("Use-W-Farm", "Use W")).SetValue(true);
            _config.SubMenu("Lane").AddItem(new MenuItem("Use-E-Farm", "Use E")).SetValue(true);
            _config.SubMenu("Lane").AddItem(new MenuItem("Use-R-Farm", "Use R")).SetValue(true);
            _config.SubMenu("Lane").AddItem(new MenuItem("Farm-Mana", "Mana Limit").SetValue(new Slider(50, 100, 0)));
            _config.SubMenu("Lane").AddItem(new MenuItem("ActiveLane", "Lane Key").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            //Jungle
            _config.AddSubMenu(new Menu("Jungle", "Jungle"));
            _config.SubMenu("Jungle").AddItem(new MenuItem("Use-Q-Jungle", "Use Q")).SetValue(true);
            _config.SubMenu("Jungle").AddItem(new MenuItem("Use-W-Jungle", "Use W")).SetValue(true);
            _config.SubMenu("Jungle").AddItem(new MenuItem("Use-E-Jungle", "Use E")).SetValue(true);
            _config.SubMenu("Jungle").AddItem(new MenuItem("Use-R-Jungle", "Use R")).SetValue(true);
            _config.SubMenu("Jungle").AddItem(new MenuItem("Jungle-Mana", "Mana Limit").SetValue(new Slider(50, 100, 0)));
            _config.SubMenu("Jungle").AddItem(new MenuItem("ActiveJungle", "Jungle Key").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));


            _config.AddToMainMenu();

            Game.OnGameUpdate += OnGameUpdate;


            Game.PrintChat("<font color='#881df2'>Udyr By Diabaths </font>Loaded!");
            Game.PrintChat("<font color='#881df2'>StunCycle by xcxooxl");
        } 

        private static void OnGameUpdate(EventArgs args)
        {

            _player = ObjectManager.Player;
            if (_config.Item("ActiveCombo").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            if (_config.Item("StunCycle").GetValue<KeyBind>().Active)
            {
                StunCycle();
            }
            if (_config.Item("ActiveLane").GetValue<KeyBind>().Active && (100 * (_player.Mana / _player.MaxMana)) > _config.Item("Farm-Mana").GetValue<Slider>().Value)
            {
                Farm();
            }
            if (_config.Item("ActiveJungle").GetValue<KeyBind>().Active && (100 * (_player.Mana / _player.MaxMana)) > _config.Item("Jungle-Mana").GetValue<Slider>().Value)
            {
                JungleClear();
            }
            if (_config.Item("AutoShield").GetValue<bool>() && !_config.Item("ActiveCombo").GetValue<KeyBind>().Active)
            {
                AutoW();
            }
            if (_config.Item("Forest").GetValue<KeyBind>().Active && (100 * (_player.Mana / _player.MaxMana)) > _config.Item("Forest-Mana").GetValue<Slider>().Value)
            {
                Forest();
            }
            _orbwalker.SetAttack(true);

            _orbwalker.SetMovement(true);
        }

        public static void Unit_OnLevelUp(LeagueSharp.Obj_AI_Base sender, LeagueSharp.Common.CustomEvents.Unit.OnLevelUpEventArgs args)
        {
            if (sender.IsMe)
            {
                if (!_config.Item("AutoLevel").GetValue<bool>()) return;
                if (_config.Item("Style").GetValue<StringList>().SelectedIndex == 0)
                    _player.Spellbook.LevelUpSpell(Tiger[args.NewLevel - 1].Slot);
                else if (_config.Item("Style").GetValue<StringList>().SelectedIndex == 1)
                    _player.Spellbook.LevelUpSpell(Phoenix[args.NewLevel - 1].Slot);
            }
        }
        private static void Farm()
        {
            if (!Orbwalking.CanMove(40)) return;
            var minions = MinionManager.GetMinions(_player.ServerPosition, 500.0F);
            if (minions.Count < 3) return;


            if (_config.Item("Use-R-Farm").GetValue<bool>() && _r.IsReady())
            {
                _r.Cast();
            }
            if (_config.Item("Use-Q-Farm").GetValue<bool>() && _q.IsReady())
            {
                _q.Cast();
            }
            if (_config.Item("Use-W-Farm").GetValue<bool>() && _w.IsReady())
            {
                _w.Cast();
            }
            if (_config.Item("Use-E-Farm").GetValue<bool>() && _e.IsReady())
            {
                _e.Cast();
            }
            if (_config.Item("RavHydra").GetValue<bool>() && _ravHydra.IsReady())
            {
                _ravHydra.Cast();
            }
            if (_config.Item("Tiamat").GetValue<bool>() && _tiamat.IsReady())
            {
                _tiamat.Cast();
            }
        }

        private static void Forest()
        {
            if (_player.HasBuff("Recall")) return;
            if (_e.IsReady() && _config.Item("ForestE").GetValue<bool>())
            {
                _e.Cast();
            }
            if (_w.IsReady() && _config.Item("ForestW").GetValue<bool>())
            {
                _w.Cast();
            }
        }
        private static void AutoW()
        {
            if (_w.IsReady())
            {

                if (_player.HasBuff("Recall")) return;

                if (_player.Health <= (_player.MaxHealth * (_config.Item("AutoShield%").GetValue<Slider>().Value) / 100))
                {
                    _w.Cast();
                }

            }


        }
        private static void JungleClear()
        {

            if (!Orbwalking.CanMove(40)) return;
            var minions = MinionManager.GetMinions(_player.ServerPosition, 400, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (_config.Item("RavHydra").GetValue<bool>() && _ravHydra.IsReady())
            {
                foreach (var minion in minions)
                {
                    if (minion.IsValidTarget())
                    {
                        _ravHydra.Cast();
                    }
                }
            }
            if (_config.Item("Tiamat").GetValue<bool>() && _tiamat.IsReady())
            {
                foreach (var minion in minions)
                {
                    if (minion.IsValidTarget())
                    {
                        _tiamat.Cast();
                    }
                }
            }
            if (_config.Item("Use-Q-Jungle").GetValue<bool>() && _q.IsReady())
            {

                foreach (var minion in minions)
                {
                    if (minion.IsValidTarget())
                    {

                        _q.Cast();
                        return;
                    }
                }
            }

            else if (_config.Item("Use-R-Jungle").GetValue<bool>() && _r.IsReady())
            {

                foreach (var minion in minions)
                {
                    if (minion.IsValidTarget())
                    {

                        _r.Cast();
                        return;
                    }
                }
            }
            else if (_config.Item("Use-W-Jungle").GetValue<bool>() && _w.IsReady())
            {

                foreach (var minion in minions)
                {
                    if (minion.IsValidTarget())
                    {

                        _w.Cast();
                        return;
                    }
                }
            }
            else if (_config.Item("Use-E-Jungle").GetValue<bool>() && _e.IsReady())
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

        private static void Combo()
        {
            //Create target

            var target = SimpleTs.GetTarget(_config.Item("TargetRange").GetValue<Slider>().Value, SimpleTs.DamageType.Magical);

            if (target != null && _player.Distance(target) <= _config.Item("TargetRange").GetValue<Slider>().Value)
            {
                if (_e.IsReady() && !target.HasBuff("udyrbearstuncheck", true))
                {
                    _e.Cast();
                    return;
                }
                if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Level >=
                ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).Level)
                    if (_q.Cast()) return;

                if (_r.IsReady() && target.HasBuff("udyrbearstuncheck", true))
                {
                    _r.Cast();
                    return;
                }

                if (_q.IsReady() && target.HasBuff("udyrbearstuncheck", true))
                {
                    _q.Cast();
                    return;
                }

                if (_w.IsReady() && target.HasBuff("udyrbearstuncheck", true))
                {
                    _w.Cast(); return;
                }


                if (_config.Item("BoTRK").GetValue<bool>() && _boTrk.IsReady())
                {
                    _boTrk.Cast(target);
                }
                if (_config.Item("RavHydra").GetValue<bool>() && _ravHydra.IsReady() && (_player.Distance(target) <= _ravHydra.Range))
                {
                    _ravHydra.Cast(target);
                }
                if (_config.Item("Tiamat").GetValue<bool>() && _tiamat.IsReady() && (_player.Distance(target) <= _tiamat.Range))
                {
                    _tiamat.Cast(target);
                }
                if (_config.Item("BilgeCut").GetValue<bool>() && _bilgeCut.IsReady())
                {
                    _bilgeCut.Cast(target);
                }

                if (_config.Item("RanOmen").GetValue<bool>() && _ranOmen.IsReady() && (_player.Distance(target) <= 490))
                {
                    _ranOmen.Cast();
                }


            }

        }

        private static void StunCycle()
        {
            Obj_AI_Hero closestEnemy = null;

            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (enemy.IsValidTarget(800) && !enemy.HasBuff("udyrbearstuncheck"))
                {
                    if (_e.IsReady())
                    {
                        _e.Cast();
                    }
                    if (closestEnemy == null)
                    {
                        closestEnemy = enemy;
                    }
                    else if (_player.Distance(closestEnemy) < _player.Distance(enemy))
                    {
                        closestEnemy = enemy;
                    }
                    else if (enemy.HasBuff("udyrbearstuncheck"))
                    {
                        Game.PrintChat(closestEnemy.BaseSkinName + " has buff already !!!");
                        closestEnemy = enemy;
                        Game.PrintChat(enemy.BaseSkinName + "is the new target");

                    }
                    if (!enemy.HasBuff("udyrbearstuncheck"))
                    {
                        _player.IssueOrder(GameObjectOrder.AttackUnit, closestEnemy);
                    }

                }
            }
        }

    }
}

