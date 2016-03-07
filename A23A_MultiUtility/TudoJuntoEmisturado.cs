using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Constants;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using SharpDX;
using Color = System.Drawing.Color;
//Very grateful Capitao Addon, WujuSan, Haker, iRaxe, Pataxx, Kk2, RoachxD, 

namespace A23A_MultiUtility
{
    internal class TudoJuntoEmisturado
    {
        internal static Menu A23A;
        internal static Menu Multi;
        internal static Menu Utility;
        internal static bool Fugir;
        internal static bool ToNaMerda;
        internal static int UltCast;
        internal static int UltClick;
        internal static Vector3 LocalDano;
        internal static Obj_AI_Base MeAtacou;
        internal static AIHeroClient Eu = Player.Instance;
         public static string GameID;
        public static Spell.Active Ghost, Barrier, Heal, Recall;
        public static Spell.Skillshot Flash;
        public static Spell.Targeted Teleport, Ignite, Smite, Exhaust;
        public static readonly Obj_HQ MyNexus;
        public static readonly Obj_HQ EneMyNexus;
        public static readonly AIHeroClient p;
        public static readonly Obj_AI_Turret EnemyLazer;
        private static Orbwalker.ActiveModes activeMode = Orbwalker.ActiveModes.None;
        private static InventorySlot seraphs;
        private static int hpSlot;
        private static readonly ColorBGRA color;
        private static List<Vector3> PfNodes;
        private static readonly NavGraph NavGraph;
        private static bool oldWalk;
        public static bool newPF;
        public static EventHandler EndGame;
        static AutoWalker()

        internal static InventorySlot Pothp
        {
            get { return Eu.InventoryItems.FirstOrDefault(a => a.Id == ItemId.Health_Potion); }
        }

        internal static InventorySlot Potrefil
        {
            get { return Eu.InventoryItems.FirstOrDefault(b => b.Id == (ItemId) 2031); }
        }

        internal static InventorySlot Potjung
        {
            get { return Eu.InventoryItems.FirstOrDefault(c => c.Id == (ItemId) 2032); }
        }

        internal static InventorySlot Potcorrup
        {
            get { return Eu.InventoryItems.FirstOrDefault(d => d.Id == (ItemId) 2033); }
        }

        internal static AIHeroClient Inimigoperto
        {
            get { return EntityManager.Heroes.Enemies.FirstOrDefault(t => t.Distance(Eu) <= 1100 && t.IsVisible); }
        }

        internal static Obj_AI_Turret Tip
        {
            get
            {
                return
                    ObjectManager.Get<Obj_AI_Turret>()
                        .Where(a => a.IsEnemy)
                        .OrderBy(b => b.Distance(Eu.Position))
                        .First(c => !c.IsDead);
            }
        }

        public static int TickDano { get; private set; }

        internal static AIHeroClient Escolhido()
        {
            var ele =
                EntityManager.Heroes.Allies.Where(it => !it.IsMe)
                    .FirstOrDefault(x => Multi["sg" + x.ChampionName].Cast<CheckBox>().CurrentValue);
            return (ele);
        }

        internal static Obj_AI_Turret Torreperto()
        {
            var tmp =
                ObjectManager.Get<Obj_AI_Turret>()
                    .Where(a => a.IsAlly)
                    .OrderBy(b => b.Distance(Eu.Position))
                    .First(c => !c.IsDead);
            return tmp;
        }

        internal static Obj_AI_Minion Minionperto()
        {
            var mmp =
                ObjectManager.Get<Obj_AI_Minion>()
                    .Where(a => a.IsAlly)
                    .OrderBy(b => b.Distance(Eu))
                    .FirstOrDefault(
                        c =>
                            !c.IsDead && c.Distance(Baliada().Position) > Eu.Distance(Baliada().Position) &&
                            c.Distance(Binimiga().Position) < Eu.Distance(Binimiga().Position) &&
                            c.IsInRange(Eu.Position, 3000));
            return mmp;
        }

        internal static Obj_SpawnPoint Baliada()
        {
            var mb = ObjectManager.Get<Obj_SpawnPoint>().First(ta => ta.IsAlly);
            return mb;
        }

        internal static Obj_SpawnPoint Binimiga()
        {
            var bi = ObjectManager.Get<Obj_SpawnPoint>().First(ta => ta.IsEnemy);
            return bi;
        }

        internal static void ComeçouAbrincadeira(EventArgs args)
        {
            Bootstrap.Init(null);
            QuemÉoCantor();
            Obj_AI_Base.OnSpellCast += ChecarSendoatacado;
            AttackableUnit.OnDamage += ChecarRecebendoDano;
            Game.OnTick += Checagem;
            Game.OnUpdate += AutoOrbe;
            Orbwalker.OnPreAttack += Naoatacarminion;
            Player.OnIssueOrder += UltimoClick;
            Multi["AtvSeg"].Cast<KeyBind>().OnValueChange += FecharOrb;
            if (Utility["RangeTorre"].Cast<KeyBind>().CurrentValue) Drawing.OnDraw += Rangeligado;
            else Drawing.OnDraw -= Rangeligado;
            Utility["RangeTorre"].Cast<KeyBind>().OnValueChange += Rangedastorres;
            /*
            Utility["AtvZoom"].Cast<KeyBind>().OnValueChange += AtivarZoom;
            Utility["ValZoom"].Cast<Slider>().OnValueChange += ValorZoom;
            Hacks.ZoomHack = Utility["AtvZoom"].Cast<KeyBind>().CurrentValue;
            Camera.SetZoomDistance(Hacks.ZoomHack ? Utility["ValZoom"].Cast<Slider>().CurrentValue : 2250);
            */
        }

        internal static void Naoatacarminion(AttackableUnit target, Orbwalker.PreAttackArgs args)
        {
            if (target.Type == GameObjectType.obj_AI_Minion)
                args.Process = !Utility["NaM"].Cast<CheckBox>().CurrentValue;

            if (Multi["AtvSeg"].Cast<KeyBind>().CurrentValue && target.Type == GameObjectType.AIHeroClient)
            {
                if (Eu.Distance(Tip) <= 890) args.Process = false;
            }
        }

        internal static int Aleatorio(int min, int max)
        {
            var grandon = new Random().Next(min, max);
            return grandon;
        }


        internal static void Checagem(EventArgs args)
        {
            if (Utility["useHP"].Cast<CheckBox>().CurrentValue &&
                Eu.HealthPercent <= Utility["hpSlider"].Cast<Slider>().CurrentValue && !Eu.IsInShopRange())
            {
                if (!Eu.HasBuff("RegenerationPotion") && !Eu.HasBuff("ItemCrystalFlask") &&
                    !Eu.HasBuff("ItemDarkCrystalFlask") && !Eu.HasBuff("ItemCrystalFlaskJungle")) AutoPot();

            }
            if (Eu.IsDead || !Multi["AtvSeg"].Cast<KeyBind>().CurrentValue) return;
            if (Multi["AutRec"].Cast<CheckBox>().CurrentValue || Multi["AutRec2"].Cast<CheckBox>().CurrentValue)
            {
                if (Eu.Distance(Baliada().Position) < 950)
                {
                    if (Environment.TickCount <= UltClick + 300 || Environment.TickCount <= UltCast + 300) return;
                    if (Multi["AutRec"].Cast<CheckBox>().CurrentValue && Eu.HealthPercent < 85) return;
                    if (Multi["AutRec2"].Cast<CheckBox>().CurrentValue && Eu.ManaPercent < 85) return;
                    ToNaMerda = false;
                }
                if (ToNaMerda)
                {
                    if (Environment.TickCount <= UltClick + 300 || Environment.TickCount <= UltCast + 300) return;
                    if (Eu.Distance(Baliada().Position) > 950 && !Eu.IsRecalling())
                    {
                        var mip =
                            ObjectManager.Get<Obj_AI_Minion>()
                                .Where(a => a.IsEnemy)
                                .OrderBy(b => b.Distance(Eu))
                                .FirstOrDefault(c => !c.IsDead && c.IsInRange(Eu.Position, 2000));
                        if (Eu.CountEnemiesInRange(2000) == 0 && mip == null)
                        {
                            if (Eu.Distance(Baliada().Position) > 4500)
                            {
                                Eu.Spellbook.CastSpell(SpellSlot.Recall);
                                return;
                            }
                            Player.IssueOrder(GameObjectOrder.MoveTo, Baliada().Position);
                        }
                        if (Eu.Distance(Baliada().Position) <= 4500)
                            Player.IssueOrder(GameObjectOrder.MoveTo, Baliada().Position);
                        if (Eu.Distance(Torreperto().Position.Extend(Baliada().Position, 230).To3D()) < 10)
                        {
                            Eu.Spellbook.CastSpell(SpellSlot.Recall);
                            return;
                        }
                        Player.IssueOrder(GameObjectOrder.MoveTo,
                            Torreperto().Position.Extend(Baliada().Position, 230).To3D());
                    }
                }
                if (Multi["AutRec2"].Cast<CheckBox>().CurrentValue)
                {
                    if (Eu.ManaPercent < 20)
                    {
                        ToNaMerda = true;
                        return;
                    }
                    ToNaMerda = false;
                }
                if (Multi["AutRec"].Cast<CheckBox>().CurrentValue)
                {
                    if (Eu.HealthPercent < 30)
                    {
                        ToNaMerda = true;
                        return;
                    }
                    ToNaMerda = false;
                }
            }
            else ToNaMerda = false;
            if (Fugir)
            {
                AutoFuga();
                return;
            }
            ChecarSeguir();
        }

        internal static void ChecarSeguir()
        {
            if (Fugir || ToNaMerda || Eu.IsRecalling()) return;
            if (Escolhido() == null || Escolhido().IsDead || Escolhido().IsInShopRange() || Escolhido().IsRecalling())
            {
                ChecarMinion();
                return;
            }
            if (Eu.Distance(Escolhido()) + Escolhido().Distance(Baliada().Position) >=
                Eu.Distance(Baliada().Position) + 500) AutoSeguir();
            if (Escolhido().Distance(Binimiga().Position) + 250 > Eu.Distance(Binimiga().Position) &&
                Eu.Distance(Torreperto()) - 245 < Escolhido().Distance(Eu)) return;
            if (Escolhido().Distance(Binimiga().Position) < Eu.Distance(Binimiga().Position) &&
                Eu.Distance(Escolhido().ServerPosition.Extend(Torreperto(), 250).To3D()) <= 350) return;
            if (Eu.Distance(Torreperto()) < 400 && Escolhido().Distance(Torreperto()) < 400) return;
            AutoSeguir();
        }

        internal static void ChecarMinion()
        {
            if (Fugir) return;
            if (Minionperto() == null) return;
            if (Eu.Distance(Minionperto().Position.Extend(Torreperto(), 20).To3D()) <= 150) return;
            if (Environment.TickCount <= UltClick + 500 || Environment.TickCount <= UltCast + 300) return;
            Player.IssueOrder(GameObjectOrder.MoveTo,
                Minionperto().Position.Extend(Torreperto(), 20 + Aleatorio(3, 10)).To3D());
        }

        internal static void AutoSeguir()
        {
            if (Fugir) return;
            if (Environment.TickCount <= UltClick + 500 || Environment.TickCount <= UltCast + 300) return;
            Player.IssueOrder(GameObjectOrder.MoveTo,
                Escolhido().ServerPosition.Extend(Torreperto(), 250 + Aleatorio(4, 15)).To3D());
        }

        internal static void AutoFuga()
        {
            if (Orbwalker.ActiveModesFlags != Orbwalker.ActiveModes.None)
                Orbwalker.ActiveModesFlags = Orbwalker.ActiveModes.None;
            if (MeAtacou is Obj_AI_Turret)
            {
                if (MeAtacou.Health > 1 && Eu.Distance(MeAtacou) <= 900)
                {
                    if (Environment.TickCount <= UltClick + 500 || Environment.TickCount <= UltCast + 300) return;
                    if (!MeAtacou.Position.Extend(Eu, 1200).To3D().IsValid())
                    {
                        Player.IssueOrder(GameObjectOrder.MoveTo,
                            Torreperto().Position.Extend(Baliada().Position, 230 + Aleatorio(5, 15)).To3D());
                    }
                    Player.IssueOrder(GameObjectOrder.MoveTo,
                        MeAtacou.Position.Extend(Eu, 1200 + Aleatorio(4, 15)).To3D());
                }
                else Fugir = false;
                return;
            }
            if (MeAtacou is Obj_AI_Minion)
            {
                if (Inimigoperto == null && Eu.CountEnemiesInRange(550) <= 3)
                {
                    if (MeAtacou.HealthPercent < 20)
                    {
                        Player.IssueOrder(GameObjectOrder.AttackUnit, MeAtacou);
                        Fugir = false;
                    }
                }
            }
            if (Environment.TickCount <= UltClick + 500 || Environment.TickCount <= UltCast + 300) return;
            Player.IssueOrder(GameObjectOrder.MoveTo,
                Torreperto().Position.Extend(Baliada().Position, 250 + Aleatorio(6, 20)).To3D());
            if (!MeAtacou.IsDead && Eu.Distance(MeAtacou) < 560) return;
            if (Eu.Distance(LocalDano) < 560 && Environment.TickCount <= TickDano + 600) return;
            Fugir = false;
        }

        internal static void AutoOrbe(EventArgs args)
        {
            Orbwalker.DisableMovement = Multi["AtvSeg"].Cast<KeyBind>().CurrentValue;
            if (!Multi["AtvSeg"].Cast<KeyBind>().CurrentValue) return;
            if (Eu.IsDead || Fugir || ToNaMerda)
            {
                if (Orbwalker.ActiveModesFlags != Orbwalker.ActiveModes.None)
                {
                    Orbwalker.ActiveModesFlags = Orbwalker.ActiveModes.None;
                }
                return;
            }

            if (Inimigoperto != null)
            {
                if (!Inimigoperto.IsDead)
                {
                    if (Eu.Distance(Inimigoperto) >= 520)
                    {
                        if (Eu.Distance(Tip) >= 870)
                        {
                            if (Orbwalker.ActiveModesFlags != Orbwalker.ActiveModes.Combo)
                            {
                                Orbwalker.ActiveModesFlags = Orbwalker.ActiveModes.Combo;
                            }
                        }
                        else
                        {
                            if (Orbwalker.ActiveModesFlags != Orbwalker.ActiveModes.LaneClear)
                            {
                                Orbwalker.ActiveModesFlags = Orbwalker.ActiveModes.LaneClear;
                            }
                        }
                    }
                    else
                    {
                        if (Eu.Distance(Tip) >= 870)
                        {
                            if (Orbwalker.ActiveModesFlags != Orbwalker.ActiveModes.Combo)
                            {
                                Orbwalker.ActiveModesFlags = Orbwalker.ActiveModes.Combo;
                            }
                        }
                        else
                        {
                            {
                                if (Orbwalker.ActiveModesFlags != Orbwalker.ActiveModes.LaneClear)
                                {
                                    Orbwalker.ActiveModesFlags = Orbwalker.ActiveModes.LaneClear;
                                }
                            }
                        }

                    }
                }
                else
                {
                    if (Orbwalker.ActiveModesFlags != Orbwalker.ActiveModes.LaneClear)
                    {
                        Orbwalker.ActiveModesFlags = Orbwalker.ActiveModes.LaneClear;
                    }
                }
            }
            else
            {
                if (Orbwalker.ActiveModesFlags != Orbwalker.ActiveModes.LaneClear)
                {
                    Orbwalker.ActiveModesFlags = Orbwalker.ActiveModes.LaneClear;
                }
            }
        }

        internal static void ChecarRecebendoDano(AttackableUnit sender, AttackableUnitDamageEventArgs args)
        {
            if (!Multi["AtvSeg"].Cast<KeyBind>().CurrentValue) return;
            if (!args.Target.IsMe) return;
            if (Eu.HasBuffOfType(BuffType.Poison) || Eu.HasBuffOfType(BuffType.Damage)) return;
            if (Orbwalker.ActiveModesFlags != Orbwalker.ActiveModes.None)
            {
                Orbwalker.ActiveModesFlags = Orbwalker.ActiveModes.None;
            }
            Fugir = true;
            MeAtacou = (Obj_AI_Base) sender;
            LocalDano = args.Target.Position;
            TickDano = Environment.TickCount;
        }

        internal static void ChecarSendoatacado(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!Multi["AtvSeg"].Cast<KeyBind>().CurrentValue) return;
            if (sender.IsMe)
            {
                if (!args.IsAutoAttack())
                {
                    UltCast = Environment.TickCount;
                }
            }
            if (!sender.IsEnemy || sender.Distance(Eu) >= 1000) return;
            if (args.Target == null) return;
            if (!args.Target.IsMe) return;
            Fugir = true;
            MeAtacou = sender;
        }

        internal static void UltimoClick(Obj_AI_Base sender, PlayerIssueOrderEventArgs args)
        {
            if (!Multi["AtvSeg"].Cast<KeyBind>().CurrentValue || !sender.IsMe) return;
            if (args.Order == GameObjectOrder.MoveTo || args.Order == GameObjectOrder.AttackUnit ||
                args.Order == GameObjectOrder.AttackTo)
            {
                UltClick = Environment.TickCount;
            }
        }

        internal static void FecharOrb(ValueBase<bool> sender, ValueBase<bool>.ValueChangeArgs args)
        {
            Orbwalker.ActiveModesFlags = Orbwalker.ActiveModes.None;
        }

        /*
        internal static void AtivarZoom(ValueBase<bool> sender, ValueBase<bool>.ValueChangeArgs args)
        {
            Hacks.ZoomHack = Utility["AtvZoom"].Cast<KeyBind>().CurrentValue;
            Camera.SetZoomDistance(Hacks.ZoomHack ? Utility["ValZoom"].Cast<Slider>().CurrentValue : 2250);
        }

        internal static void ValorZoom(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs args)
        {
            if (Utility["AtvZoom"].Cast<KeyBind>().CurrentValue)
            {
                Camera.SetZoomDistance(Utility["ValZoom"].Cast<Slider>().CurrentValue);
            }
        }
        */

        internal static void AutoPot()
        {
            if (Pothp != null)
            {
                var potHpSlot = Pothp.SpellSlot;
                Player.CastSpell(potHpSlot);
            }
            else
            {
                if (Potrefil != null)
                {
                    var potRefilSlot = Potrefil.SpellSlot;
                    Player.CastSpell(potRefilSlot);
                }
                else
                {
                    if (Potjung != null)
                    {
                        var potJungSlot = Potjung.SpellSlot;
                        Player.CastSpell(potJungSlot);
                    }
                    else
                    {
                        if (Potcorrup != null)
                        {
                            var potCorrupSlot = Potcorrup.SpellSlot;
                            Player.CastSpell(potCorrupSlot);
                        }
                    }
                }
            }
        }

        internal static void Rangedastorres(ValueBase<bool> sender, ValueBase<bool>.ValueChangeArgs args)
        {
            if (Utility["RangeTorre"].Cast<KeyBind>().CurrentValue) Drawing.OnDraw += Rangeligado;
            else Drawing.OnDraw -= Rangeligado;
        }

        internal static void Rangeligado(EventArgs args)
        {
            foreach (
                var torres in
                    ObjectManager.Get<Obj_Turret>()
                        .Where(a => a.IsEnemy)
                        .Where(a => !a.Name.Contains("Shrine"))
                        .Where(a => !a.IsDead)
                        .Where(a => Eu.Distance(a) <= 2000)
                        .Where(a => a.Health >= 1))
            {
                Drawing.DrawCircle(torres.Position, 900, Color.Salmon);
            }
        }

        internal static void QuemÉoCantor()
        {
            A23A = MainMenu.AddMenu("Multi_Utility", "MenuFollow");
            A23A.AddSeparator();
            A23A.AddLabel("Created by andre23andre");
            A23A.AddSeparator();
            A23A.AddLabel("USE WISELY.");
            Multi = A23A.AddSubMenu("Auto-Follow", "optMenuFollow");
            Multi.AddGroupLabel("Follow Ally / Minion (Also enables Auto-OrbWalker and Auto-Escape):");
            Multi.AddSeparator();
            Multi.Add("AtvSeg", new KeyBind("Enable Auto-Follow:", false, KeyBind.BindTypes.PressToggle, 112));
            Multi.AddSeparator();
            Multi.Add("AutRec", new CheckBox("Auto-Recall HP below 30 %."));
            Multi.Add("AutRec2", new CheckBox("Auto-Recall Mana below 20 %.", false));
            Multi.AddLabel(
                "Disable Auto-Recall for Mana in heroes who use fury or energy or do not use anything to skill.");
            Multi.AddGroupLabel("Select ally to follow:");
            foreach (var aliado in EntityManager.Heroes.Allies.Where(x => !x.IsMe))
            {
                Multi.AddSeparator();
                Multi.Add("sg" + aliado.ChampionName, new CheckBox("Follow: " + aliado.ChampionName, false));
            }
            Utility = A23A.AddSubMenu("Utilitys", "uti");
            Utility.AddGroupLabel("Minion options");
            Utility.Add("NaM", new CheckBox("Not attack the Minions."));
            Utility.AddGroupLabel("Potion options");
            Utility.Add("useHP", new CheckBox("Use HealthPotion,RefillPot,CorruptPot,HuntersPot."));
            Utility.Add("hpSlider", new Slider("Percentage to use HP Potion :", 70, 1, 99));
            Utility.Add("RangeTorre",
                new KeyBind("Enable range enemy towers", false, KeyBind.BindTypes.PressToggle, 113));
            /*
            Utility.AddGroupLabel("Zoom options");
            Utility.Add("AtvZoom", new KeyBind("Enable ZoomHack", Hacks.ZoomHack, KeyBind.BindTypes.PressToggle, 114));
            Utility.Add("ValZoom", new Slider("Zoom value:", 2250, 1800, 5000));
            */
        }
    }
}
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
