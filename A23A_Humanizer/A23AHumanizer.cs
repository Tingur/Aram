using System;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Constants;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using SharpDX;
using Color = System.Drawing.Color;

namespace A23A_Humanizer
{
    internal static class A23AHumanizer
    {
        internal static void Main(string[] args)
        {
            Loading.OnLoadingComplete += StartGame;
        }

        internal static void StartGame(EventArgs args)
        {
            Hmenu();
            Obj_AI_Base.OnSpellCast += CheckSkill;
            if (HumMenu["enbInf"].Cast<CheckBox>().CurrentValue) Drawing.OnEndScene += Infos;
            Player.OnIssueOrder += ConteClick;
            Game.OnTick += ContClick;
            EnaHum = HumMenu["atvHum"].Cast<CheckBox>().CurrentValue;
            HumMenu["atvHum"].Cast<CheckBox>().OnValueChange +=
                (x, y) => { EnaHum = HumMenu["atvHum"].Cast<CheckBox>().CurrentValue; };
            HumMenu["enbInf"].Cast<CheckBox>().OnValueChange += (x, y) =>
            {
                if (HumMenu["enbInf"].Cast<CheckBox>().CurrentValue) Drawing.OnEndScene += Infos;
                else Drawing.OnEndScene -= Infos;
            };
        }

        internal static Menu HumMenu;
        internal static bool EnaHum;
        internal static int UltCast;
        internal static int UltClick;
        internal static int Cliks;
        internal static int ClickLastSeg;
        internal static int ClickInit;
        internal static int TimeInit;
        internal static int MaxClick;
        internal static int BlockCkick;

        internal static void Hmenu()
        {
            HumMenu = MainMenu.AddMenu("A23A_Humanizer", "HumMenu");
            HumMenu.AddGroupLabel("Created by andre23andre");
            HumMenu.AddGroupLabel("Options:");
            HumMenu.AddSeparator();
            HumMenu.Add("atvHum", new CheckBox("Enable Humanization"));
            HumMenu.Add("enbInf", new CheckBox("Enable Draw Infos"));
            HumMenu.AddSeparator();
            HumMenu.Add("maxClick", new Slider("Maximo clicks per second", 6, 4, 10));
            HumMenu.AddSeparator();
            var intCl = HumMenu.Add("intClick", new Slider("", 120, 80, 600));
            intCl.DisplayName = "Set Value for increase Your Ping it is Delay clicks (M/s), Current Value:  " +
                                (Game.Ping + HumMenu["intClick"].Cast<Slider>().CurrentValue);
            intCl.OnValueChange +=
                delegate(ValueBase<int> a, ValueBase<int>.ValueChangeArgs b)
                {
                    a.DisplayName = "Set Value for increase Your Ping it is Delay clicks (M/s), Current Value:  " +
                                    (Game.Ping + HumMenu["intClick"].Cast<Slider>().CurrentValue);
                };
            HumMenu.AddSeparator();
            var intCa = HumMenu.Add("intCast", new Slider("", 150, 110, 600));
            intCa.DisplayName = "Set Value for increase Your Ping it is Delay cast (M/s), Current Value:  " +
                                (Game.Ping + HumMenu["intCast"].Cast<Slider>().CurrentValue);
            intCa.OnValueChange +=
                delegate(ValueBase<int> a, ValueBase<int>.ValueChangeArgs b)
                {
                    a.DisplayName = "Set Value for increase Your Ping it is Delay cast (M/s), Current Value:  " +
                                    (Game.Ping + HumMenu["intCast"].Cast<Slider>().CurrentValue);
                };
            HumMenu.AddGroupLabel("Recomendo Max Clicks/s = 6 , Min Delay Cliks = 120, Min Delay Cast = 150");
            HumMenu.AddGroupLabel("More values, more safe. Enjoy.");
        }

        internal static void ContClick(EventArgs args)
        {
            var time = (int) Game.Time;
            if (time > TimeInit)
            {
                ClickInit = Cliks;
                TimeInit = time;
            }
            else
            {
                ClickLastSeg = Cliks - ClickInit;
                if (Cliks - ClickInit > MaxClick) MaxClick = Cliks - ClickInit;
            }
        }

        internal static bool Safe
        {
            get
            {
                if (EnaHum &&
                    (Environment.TickCount <= UltClick + HumMenu["intClick"].Cast<Slider>().CurrentValue + Game.Ping ||
                     Environment.TickCount <= UltCast + HumMenu["intCast"].Cast<Slider>().CurrentValue + Game.Ping ||
                     ClickLastSeg > HumMenu["maxClick"].Cast<Slider>().CurrentValue)) return false;
                return true;
            }
        }

        internal static void Infos(EventArgs args)
        {
            Drawing.DrawText(Drawing.Width - 200, 140, Color.Cyan, "Block Click: " + BlockCkick);
            Drawing.DrawText(Drawing.Width - 200, 160, Color.Coral, "Amount Cliks/Cast : " + Cliks);
            Drawing.DrawText(Drawing.Width - 200, 180,
                ClickLastSeg <= HumMenu["maxClick"].Cast<Slider>().CurrentValue ? Color.LawnGreen : Color.Crimson,
                "Cliks/Cast Last Second: " + ClickLastSeg);
            //Drawing.DrawText(Drawing.Width - 200, 200, ClickLastSeg <= 7 ? Color.LawnGreen : Color.Crimson,"Safe: " + Safe + " OnHumanizer: " + EnaHum);
            Drawing.DrawText(Drawing.Width - 200, 200, Color.AliceBlue,
                "Max Cliks/Cast Second: " + MaxClick);
        }

        internal static void CheckSkill(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe || args.IsAutoAttack()) return;
            if (!Player.Instance.Spellbook.GetSpell(args.Slot).IsReady)
            {
                args.Process = false;
                BlockCkick = BlockCkick + 1;
                return;
            }
            if (!Safe)
            {
                args.Process = false;
                BlockCkick = BlockCkick + 1;
                return;
            }
            Cliks = Cliks + 1;
            UltCast = Environment.TickCount;
        }

        internal static void ConteClick(Obj_AI_Base sender, PlayerIssueOrderEventArgs args)
        {
            if (!sender.IsMe) return;
            if (!Safe)
            {
                args.Process = false;
                BlockCkick = BlockCkick + 1;
                return;
            }
            if (args.Order == GameObjectOrder.AttackUnit || args.Order == GameObjectOrder.AttackTo)
            {
                Cliks = Cliks + 1;
                UltClick = Environment.TickCount;
                return;
            }
            if (args.Order != GameObjectOrder.MoveTo) return;
            if (!args.TargetPosition.IsValid())
            {
                args.Process = false;
                BlockCkick = BlockCkick + 1;
                Player.IssueOrder(GameObjectOrder.MoveTo, RenewMov(args.TargetPosition, -15, 15));
                return;
            }
            Cliks = Cliks + 1;
            UltClick = Environment.TickCount;
        }

        internal static Vector3 RenewMov(Vector3 a, int b, int c)
        {
            var r = new Random(Environment.TickCount);
            return a + new Vector2(r.Next(b, c), r.Next(b, c)).To3D();
        }
    }
}
