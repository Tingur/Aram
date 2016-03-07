﻿using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Constants;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using SharpDX;
using Color = System.Drawing.Color;
//Very grateful Capitao Addon, WujuSan, Haker, iRaxe, Pataxx, Kk2, RoachxD, 

{
            GameID = DateTime.Now.Ticks + ""+RandomString(10);
            newPF = MainMenu.GetMenu("AB").Get<CheckBox>("newPF").CurrentValue;
            NavGraph=new NavGraph(Path.Combine(SandboxConfig.DataDirectory, "AutoBuddy"));
            PfNodes=new List<Vector3>();
            color = new ColorBGRA(79, 219, 50, 255);
            MyNexus = ObjectManager.Get<Obj_HQ>().First(n => n.IsAlly);
            EneMyNexus = ObjectManager.Get<Obj_HQ>().First(n => n.IsEnemy);
            EnemyLazer = ObjectManager.Get<Obj_AI_Turret>().FirstOrDefault(tur => !tur.IsAlly && tur.GetLane() == Lane.Spawn);
            p = ObjectManager.Player;
            initSummonerSpells();

            Target = ObjectManager.Player.Position;
            Orbwalker.DisableMovement = false;

            Orbwalker.DisableAttacking = false;
            Game.OnUpdate += Game_OnUpdate;
            Orbwalker.OverrideOrbwalkPosition = () => Target;
            if (Orbwalker.HoldRadius > 130 || Orbwalker.HoldRadius < 80)
            {
                Chat.Print("=================WARNING=================", Color.Red);
                Chat.Print("Your hold radius value in orbwalker isn't optimal for AutoBuddy", Color.Aqua);
                Chat.Print("Please set hold radius through menu=>Orbwalker");
                Chat.Print("Recommended values: Hold radius: 80-130, Delay between movements: 100-250");
            }
            if (MainMenu.GetMenu("AB").Get<CheckBox>("debuginfo").CurrentValue)
                Drawing.OnDraw += Drawing_OnDraw;
            
            Core.DelayAction(OnEndGame, 20000);
            updateItems();
            oldOrbwalk();
            Game.OnTick += OnTick;
        }

        public static bool Recalling()
        {
            return p.IsRecalling();
        }

        private static void OnEndGame()
        {

            if (MyNexus != null && EneMyNexus != null && (MyNexus.Health > 1) && (EneMyNexus.Health > 1))
            {
                Core.DelayAction(OnEndGame, 5000);
                return;
            }

            if (EndGame != null)
                EndGame(null, new EventArgs());

            if (MainMenu.GetMenu("AB").Get<CheckBox>("autoclose").CurrentValue)
            Core.DelayAction(() =>
            {

                foreach (Process process in Process.GetProcessesByName("League of Legends"))
                {

                    process.CloseMainWindow();
                }

            }, 15000);

        }

        public static Vector3 Target { get; private set; }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (activeMode == Orbwalker.ActiveModes.LaneClear)
            {
                Orbwalker.ActiveModesFlags = (p.TotalAttackDamage < 150 &&
                    EntityManager.MinionsAndMonsters.EnemyMinions.Any(
                        en =>
                            en.Distance(p) < p.AttackRange + en.BoundingRadius &&
                            Prediction.Health.GetPrediction(en, 2000) < p.GetAutoAttackDamage(en))
                        ) ? Orbwalker.ActiveModes.Harass
                        : Orbwalker.ActiveModes.LaneClear;
            }
            else
                Orbwalker.ActiveModesFlags = activeMode;
        }

        public static void SetMode(Orbwalker.ActiveModes mode)
        {
            if (activeMode != Orbwalker.ActiveModes.Combo)
                Orbwalker.DisableAttacking = false;
            activeMode = mode;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            Circle.Draw(color,40, Target );
            for (int i = 0; i < PfNodes.Count-1; i++)
            {
                if(PfNodes[i].IsOnScreen()||PfNodes[i+1].IsOnScreen())
                    Line.DrawLine(Color.Aqua, 4, PfNodes[i], PfNodes[i+1]);
            }
        
        }

        public static void WalkTo(Vector3 tgt)
        {
            if (!newPF)
            {
                Target = tgt;
                return;
            }

            if (PfNodes.Any())
            {
                float dist = tgt.Distance(PfNodes[PfNodes.Count - 1]);
                if ( dist>900|| dist > 300&&p.Distance(tgt)<2000)
                {
                    PfNodes = NavGraph.FindPathRandom(p.Position, tgt);
                }
                else
                {
                    PfNodes[PfNodes.Count - 1] = tgt;
                }
                Target = PfNodes[0];
            }
            else
            {
                if (tgt.Distance(p) > 900)
                {
                    PfNodes = NavGraph.FindPathRandom(p.Position, tgt);
                    Target = PfNodes[0];
                }
                else
                {
                    Target = tgt;
                }
            }
        }




        private static void updateItems()
        {
            hpSlot = BrutalItemInfo.GetHPotionSlot();
            seraphs = p.InventoryItems.FirstOrDefault(it => (int)it.Id == 3040);
            Core.DelayAction(updateItems, 5000);
            
        }
        public static void UseSeraphs()
        {
            if (seraphs != null && seraphs.CanUseItem())
                seraphs.Cast();
        }
        public static void UseGhost()
        {
            if (Ghost != null && Ghost.IsReady())
                Ghost.Cast();
        }
        public static void UseHPot()
        {
            if (hpSlot == -1) return;
            p.InventoryItems[hpSlot].Cast();
            hpSlot = -1;
        }
        public static void UseBarrier()
        {
            if (Barrier != null && Barrier.IsReady())
                Barrier.Cast();
        }
        public static void UseHeal()
        {
            if (Heal != null && Heal.IsReady())
                Heal.Cast();
        }
        public static void UseIgnite(AIHeroClient target = null)
        {
            if (Ignite == null || !Ignite.IsReady()) return;
            if (target == null)target =
                    EntityManager.Heroes.Enemies.Where(en => en.Distance(p) < 600 + en.BoundingRadius)
                        .OrderBy(en => en.Health)
                        .FirstOrDefault();
            if (target != null && p.Distance(target) < 600 + target.BoundingRadius)
            {
                Ignite.Cast(target);
            }
                
        }

        private static void initSummonerSpells()
        {
            Recall = new Spell.Active(SpellSlot.Recall);
            Barrier = new Spell.Active(ObjectManager.Player.GetSpellSlotFromName("summonerbarrier"));
            Ghost = new Spell.Active(ObjectManager.Player.GetSpellSlotFromName("summonerhaste"));
            Flash = new Spell.Skillshot(ObjectManager.Player.GetSpellSlotFromName("summonerflash"), 600, SkillShotType.Circular);
            Heal = new Spell.Active(ObjectManager.Player.GetSpellSlotFromName("summonerheal"), 600);
            Ignite = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerdot"), 600);
            Exhaust = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerexhaust"), 600);
            Teleport = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerteleport"), int.MaxValue);
            Smite = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("smite"), 600);
        }



#region old orbwalking, for those with not working orbwalker

        private static int maxAdditionalTime = 50;
        private static int adjustAnimation = 20;
        private static float holdRadius = 50;
        private static float movementDelay = .25f;

        private static float nextMove;



        private static void oldOrbwalk()
        {

            if (!MainMenu.GetMenu("AB").Get<CheckBox>("oldWalk").CurrentValue) return;
            oldWalk = true;
            Orbwalker.OnPreAttack+=Orbwalker_OnPreAttack;
        }


        private static void Orbwalker_OnPreAttack(AttackableUnit tgt, Orbwalker.PreAttackArgs args)
        {
            nextMove = Game.Time + ObjectManager.Player.AttackCastDelay +
                       (Game.Ping + adjustAnimation + RandGen.r.Next(maxAdditionalTime)) / 1000f;
        }

        private static void OnTick(EventArgs args)
        {
            if (PfNodes.Count != 0)
            {
                Target = PfNodes[0];
                if (ObjectManager.Player.Distance(PfNodes[0]) < 600)
                {
                    PfNodes.RemoveAt(0);
                    
                }

            }



            if (!oldWalk||ObjectManager.Player.Position.Distance(Target) < holdRadius || Game.Time < nextMove) return;
            nextMove = Game.Time + movementDelay;
            Player.IssueOrder(GameObjectOrder.MoveTo, Target, true);



        }
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

    }

#endregion





}
