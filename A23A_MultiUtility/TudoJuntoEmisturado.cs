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
