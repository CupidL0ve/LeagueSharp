using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using LeagueSharp;
using LeagueSharp.Common;

namespace Follow
{
    class Program
    {
        public static Spell recall = new Spell(SpellSlot.Recall);
        public static Menu config;
        public static string name;
        private static Obj_AI_Hero target;
        public static bool dem = true;
        public static float time;
        public static Vector3 dichuyen;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += onload;
        }
        private static void onload(EventArgs args)
        {
            time = Environment.TickCount;
            Game.PrintChat("Follow");
            dichuyen.X = 12751;
            dichuyen.Y = 1903;
            dichuyen.Z = 48;
            try
            {
                config = new Menu("Follow", "Follow", true);
                foreach (var ally in ObjectManager.Get<Obj_AI_Hero>().Where(ally => ally.Team == ObjectManager.Player.Team))
                {
                    config.AddItem(new MenuItem(ally.ChampionName, ally.ChampionName).SetValue(false));
                }
                config.AddItem(new MenuItem("active", "Active").SetValue<KeyBind>(new KeyBind('L',KeyBindType.Toggle)));
                config.AddToMainMenu();
            }
            catch { }
            Drawing.OnDraw += draw;
            Game.OnGameUpdate += onupdate; 
            
        }
        private static void onupdate(EventArgs args)
        {
            Obj_AI_Turret turrt = ObjectManager.Get<Obj_AI_Turret>().Where(tur => tur.IsAlly && tur.Health > 0).OrderBy(tur => tur.Distance(dichuyen)).First();
            if ((Environment.TickCount - time) / 1000 > 5 && (Environment.TickCount - time) / 1000 < 10)
            {
                if (!Items.HasItem(3340))
                    ObjectManager.Player.BuyItem((ItemId)3340);
                ObjectManager.Player.BuyItem((ItemId)2004);
                ObjectManager.Player.BuyItem((ItemId)2004);

            }
            if ((Environment.TickCount - time) / 1000 > 10 && (Environment.TickCount - time) / 1000 < 20)
            {
                foreach (var ally in ObjectManager.Get<Obj_AI_Hero>().Where(ally => ally.Team == ObjectManager.Player.Team))
                {
                    config.Item(ally.ChampionName).SetValue(false);
                }
                ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, turrt.Position);
                return;
            }
            if ((Environment.TickCount - time) / 1000 > 40)
            {
                foreach (var ally in ObjectManager.Get<Obj_AI_Hero>().Where(ally => ally.Team == ObjectManager.Player.Team))
                {
                    if (dem)
                    {
                        Obj_AI_Hero hero = ObjectManager.Get<Obj_AI_Hero>().Where(tur => tur.IsAlly && !tur.IsMe && !tur.IsDead).OrderBy(tur => tur.Distance(dichuyen)).First();
                        config.Item(hero.ChampionName).SetValue(true);
                        if(ally.ChampionName != hero.ChampionName)
                            config.Item(ally.ChampionName).SetValue(false);
                    }
                    if (config.Item(ally.ChampionName).GetValue<bool>())
                    {
                        dem = false;
                        foreach (var ally1 in ObjectManager.Get<Obj_AI_Hero>().Where(ally1 => ally1.Team == ObjectManager.Player.Team))
                        {
                            if (ally.ChampionName != ally1.ChampionName)
                                config.Item(ally1.ChampionName).SetValue(false);
                        }
                        if (havebuff(ObjectManager.Player)) return;
                        if (config.Item("active").GetValue<KeyBind>().Active)
                        {
                            if (ally.IsDead || ally.Health < 1)
                            {
                                Obj_Shop shop3 = ObjectManager.Get<Obj_Shop>().Where(ter => ter.IsAlly).OrderBy(tur => ObjectManager.Player.Distance(tur.Position)).First();
                                if (ObjectManager.Player.Distance(shop3.Position) < 600)
                                    return;
                                Obj_AI_Turret turret = ObjectManager.Get<Obj_AI_Turret>().Where(tur => tur.IsAlly && tur.Health > 0).OrderBy(tur => tur.Distance(ObjectManager.Player.ServerPosition)).First();
                                if (ObjectManager.Player.Distance(turret) > 200)
                                    ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, turret);
                                if (ObjectManager.Player.Distance(turret) < 200)
                                    recall.Cast();
                                
                            }
                            if (havebuff(ally))
                            {
                                recall.Cast();
                            }
                            if (ObjectManager.Player.Distance(ally) > 410 && !ally.IsDead) 
                                moveto(ally);
                        }
                    }
                }
            }
            Obj_Shop shop1 = ObjectManager.Get<Obj_Shop>().Where(ter => ter.IsAlly).OrderBy(tur => ObjectManager.Player.Distance(tur.Position)).First();
            if (ObjectManager.Player.IsDead || (ObjectManager.Player.Distance(shop1.Position) < 1200))
            {
              /*  #region buy truong thien su
                if (!Items.HasItem(3048) && !Items.HasItem(3003))   // item truong thien su
                {
                    if (Items.HasItem(3027))
                        ObjectManager.Player.BuyItem((ItemId)3003);
                    if (Items.HasItem(3003) || Items.HasItem(3048))
                        return;
                    if (!Items.HasItem(3070))
                    {
                        ObjectManager.Player.BuyItem((ItemId)3070);
                        if (Items.HasItem(3070))
                            return;
                        if (!Items.HasItem(1027))
                            ObjectManager.Player.BuyItem((ItemId)1027);

                    }
                    if (!Items.HasItem(1026) && Items.HasItem(3027))
                        ObjectManager.Player.BuyItem((ItemId)1026);
                }
                #endregion  //
                #region buy truong truong sinh
                if (!Items.HasItem(3027) && Items.HasItem(3070) && (Items.HasItem(1001) || Items.HasItem(3020)))
                {
                    ObjectManager.Player.BuyItem((ItemId)3027);
                    if (Items.HasItem(3027))
                        return;
                    if (!Items.HasItem(3010))
                    {
                        ObjectManager.Player.BuyItem((ItemId)3010);
                        return;
                    }
                    if (!Items.HasItem(1026) && Items.HasItem(3010))
                        ObjectManager.Player.BuyItem((ItemId)1026);
                }
                #endregion 
                #region buy giay
                if (!Items.HasItem(3020))
                {
                    ObjectManager.Player.BuyItem((ItemId)3020);
                    if (Items.HasItem(3020))
                        return;
                    if (!Items.HasItem(1001) && Items.HasItem(3070))
                    {
                        ObjectManager.Player.BuyItem((ItemId)1001);
                        return;
                    }
                }
                #endregion 
                #region buy Tim bang
                if (!Items.HasItem(3110) && (Items.HasItem(3048) || Items.HasItem(3003)))
                {
                    ObjectManager.Player.BuyItem((ItemId)3110);
                    if (Items.HasItem(3110))
                        return;
                    if (!Items.HasItem(3082))
                    {
                        ObjectManager.Player.BuyItem((ItemId)3082);
                    }
                    if (!Items.HasItem(3024))
                        ObjectManager.Player.BuyItem((ItemId)3024);
                }
                #endregion
                #region buy di thu co
                if (!Items.HasItem(3152) && Items.HasItem(3110))
                {
                    ObjectManager.Player.BuyItem((ItemId)3152);
                    if (Items.HasItem(3152))
                        return;
                    if (!Items.HasItem(3145))
                    {
                        ObjectManager.Player.BuyItem((ItemId)3145);

                    }
                    if (!Items.HasItem(3108) && Items.HasItem(3145))
                        ObjectManager.Player.BuyItem((ItemId)3108);
                }
                #endregion
                #region buy truong hu vo
                if (Items.HasItem(3152) && !Items.HasItem(3135))
                {
                    ObjectManager.Player.BuyItem((ItemId)3135);
                    if (Items.HasItem(3135))
                        return;
                    if (!Items.HasItem(1026))
                    {
                        ObjectManager.Player.BuyItem((ItemId)1026);

                    }
                }
                #endregion */
                #region buy truong thien su
                if (!Items.HasItem(3048) && !Items.HasItem(3003))   // item truong thien su
                {
                    if (Items.HasItem(3003) || Items.HasItem(3048))
                        return;
                    if (Items.HasItem(3027))
                        buy(3003);
                    if (Items.HasItem(3003) || Items.HasItem(3048))
                        return;
                    if (!Items.HasItem(3003) && !Items.HasItem(3048))
                    {
                        if (!Items.HasItem(3070))
                        {
                            buy(3070);
                            buy(1027);
                        }

                    }
                    if (Items.HasItem(3027))
                        buy(1026);
                }
                #endregion
                #region buy truong truong sinh
                if (!Items.HasItem(3027) && Items.HasItem(3070) && (Items.HasItem(1001) || Items.HasItem(3020)))
                {
                    buy(3027);
                    buy(3010);
                    if (Items.HasItem(3010))
                        buy(1026);
                }
                #endregion
                #region buy giay
                if (!Items.HasItem(3020))
                {
                    buy(3020);
                    if (Items.HasItem(3070))
                    {
                        buy(1001);
                    }
                }
                #endregion
                #region buy Tim bang
                if (!Items.HasItem(3110) && (Items.HasItem(3048) || Items.HasItem(3003)))
                {
                    buy(3110);
                    buy(3082);
                    buy(3024);
                }
                #endregion
                #region buy di thu co
                if (!Items.HasItem(3152) && Items.HasItem(3110))
                {
                    buy(3152);
                    buy(3145);
                    if (Items.HasItem(3145))
                        buy(3108);
                }
                #endregion
                #region buy truong hu vo
                if (Items.HasItem(3152) && !Items.HasItem(3135))
                {
                    buy(3135);
                    if (Items.HasItem(3135))
                        return;
                    buy(1026);
                }
                #endregion
            }
        }
        private static void draw(EventArgs args)
        {
            foreach (var ally in ObjectManager.Get<Obj_AI_Hero>().Where(ally => ally.Team == ObjectManager.Player.Team))
            {
                if (config.Item(ally.ChampionName).GetValue<bool>())
                    Drawing.DrawCircle(ally.Position, 400, System.Drawing.Color.Green);
            }
        }
        private static void moveto(Obj_AI_Base ally)
        {
            Obj_Shop shop = ObjectManager.Get<Obj_Shop>().Where(ter => ter.IsAlly).OrderBy(tur => ObjectManager.Player.Distance(tur.Position)).First();
      //      Obj_AI_Turret turret = ObjectManager.Get<Obj_AI_Turret>().Where(tur => tur.IsAlly && tur.Health > 0).OrderBy(tur => tur.Distance(ObjectManager.Player.ServerPosition)).First();
            var pos = ally.ServerPosition + Vector3.Normalize(shop.Position - ally.ServerPosition) *(400);
            ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, pos);
        }
        private static bool havebuff(Obj_AI_Hero hero)
        {
            if (hero.HasBuff("Recall", true))
                return true;
            return false;
        }
        private static void buy(int id)
        {
            if (Items.HasItem(id))
                return;
            if (!Items.HasItem(id))
                ObjectManager.Player.BuyItem((ItemId)id);
        }
    }
}
