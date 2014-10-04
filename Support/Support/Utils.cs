using System.Linq;
using LeagueSharp;
using SharpDX;
using System.Collections;
using LeagueSharp.Common;
using System;


namespace Support {
    internal static class Utils {

        public static Geometry.Rectangle rect;

        public static void PrintMessage(string message) {
            Game.PrintChat("<font color='#1600DB'>Support:</font> <font color='#FFFFFF'>" + message + "</font>");
        }

        public static bool HasBuff(Obj_AI_Hero target, string buffName) {

            foreach (BuffInstance buff in target.Buffs) {
                //Utils.PrintMessage(buff.Name);
                if (buff.Name.ToLower() == buffName.ToLower()) {
                    return true;
                }
            }

            return false;
        }

        public static bool HasBuff(Obj_AI_Hero target, string buffName, bool debug) {

            foreach (BuffInstance buff in target.Buffs) {
                if (debug) {
                    Utils.PrintMessage(buff.Name);
                }
                if (buff.Name.ToLower() == buffName.ToLower()) {
                    return true;
                }
            }

            return false;
        }

        public static bool EnemyInRange(int numOfEnemy, float range) {

            ArrayList enemies = new ArrayList();
            // Throw all the enemies in a list.
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => Vector3.Distance(ObjectManager.Player.Position, enemy.Position) < range && enemy.IsEnemy)) {
                enemies.Add(enemy);
            }
            // Return true if the amount in range is good.
            if (enemies.Count >= numOfEnemy) {
                return true;
            } else {
                return false;
            }
        }

        public static bool AllyBelowHP(int percentHP, float range) {
            // Returns true if an ally in range has below a certain % hp.
            //PrintMessage(Convert.ToString((ObjectManager.Player.Health / ObjectManager.Player.MaxHealth) * 100));
            foreach (var ally in ObjectManager.Get<Obj_AI_Hero>()) {
                if (ally.IsMe) {
                    if (((ObjectManager.Player.Health / ObjectManager.Player.MaxHealth) * 100) < percentHP) {
                        return true;
                    }
                } else if (ally.IsAlly) {
                    if (Vector3.Distance(ObjectManager.Player.Position, ally.Position) < range && ((ally.Health / ally.MaxHealth) * 100) < percentHP) {
                        return true;
                    }
                }
            }

            return false;
        }
        // This is for sona only atm.
        public static Obj_AI_Hero GetEnemyHitByR(Spell R, int numHit) {
            int totalHit = 0;
            Obj_AI_Hero target = null;

            foreach (Obj_AI_Hero current in ObjectManager.Get<Obj_AI_Hero>()) {

                var prediction = R.GetPrediction(current, true);

                if (Vector3.Distance(ObjectManager.Player.Position, prediction.CastPosition) <= R.Range) {
                    
                    Vector2 extended = current.Position.To2D().Extend(ObjectManager.Player.Position.To2D(), -R.Range + Vector2.Distance(ObjectManager.Player.Position.To2D(), current.Position.To2D()));
                    rect = new Geometry.Rectangle(ObjectManager.Player.Position.To2D(), extended, R.Width);

                    if (!current.IsMe && current.IsEnemy) {
                        // SEt to 1 as the current target is hittable.
                        totalHit = 1;
                        foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>()) {
                            if (enemy.IsEnemy && current.ChampionName != enemy.ChampionName && !enemy.IsDead && !rect.ToPolygon().IsOutside(enemy.Position.To2D())) {
                                totalHit += 1;
                            }
                        }
                    }

                    if (totalHit >= numHit) {
                        target = current;
                        break;
                    }
                }

            }

            Console.WriteLine(Game.Time + " | Targets hit is: " + totalHit + " Out of " + numHit);
            return target;
        }
    }
}