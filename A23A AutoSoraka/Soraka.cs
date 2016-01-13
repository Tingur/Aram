using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;

namespace A23A_MultiUtility
{
    internal class Soraka
    {
        internal static Spell.Skillshot Q = new Spell.Skillshot(SpellSlot.Q, 950, SkillShotType.Circular, 283, 1100, 210);
        internal static Spell.Targeted W = new Spell.Targeted(SpellSlot.W, 550);
        internal static Spell.Skillshot E = new Spell.Skillshot(SpellSlot.E, 925, SkillShotType.Circular, 500, 1750, 70);
        internal static Spell.Active R = new Spell.Active(SpellSlot.R, int.MaxValue);
        internal static int[] LevelUps = { 0, 1, 2, 1, 1, 3, 1, 0, 1, 0, 3, 0, 0, 2, 2, 3, 2, 2 };
        internal static AIHeroClient Jho = Player.Instance;
        internal static int TickQ;
        internal static int TickE;
        internal static int TickR;
        internal static int TickW;

        public static void BigodeGrosso()
        {
            int[] levels = { 0, 0, 0, 0 };
            for (var i = 0; i < Jho.Level; i++)
            {
                levels[LevelUps[i]] = levels[LevelUps[i]] + 1;
            }
            if (Jho.Spellbook.GetSpell(SpellSlot.Q).Level < levels[0]) Jho.Spellbook.LevelSpell(SpellSlot.Q);
            if (Jho.Spellbook.GetSpell(SpellSlot.W).Level < levels[1]) Jho.Spellbook.LevelSpell(SpellSlot.W);
            if (Jho.Spellbook.GetSpell(SpellSlot.E).Level < levels[2]) Jho.Spellbook.LevelSpell(SpellSlot.E);
            if (Jho.Spellbook.GetSpell(SpellSlot.R).Level < levels[3]) Jho.Spellbook.LevelSpell(SpellSlot.R);
        }
        public static void PepecaDuMal()
        {
            var alvoe = TargetSelector.GetTarget(E.Range, DamageType.Magical);
            if (alvoe == null || !alvoe.IsValid() || !alvoe.IsValidTarget() || !E.IsReady() || Jho.ManaPercent <= 30) return;
            var prede = E.GetPrediction(alvoe);
            if (prede.HitChance != HitChance.High) return;
            E.Cast(alvoe.Position);
            TickE = Environment.TickCount;
        }
        public static void RespeitaUmoço()
        {
            var alvoq = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (alvoq == null ||!alvoq.IsValid() || !alvoq.IsValidTarget() || !Q.IsReady() || Jho.ManaPercent <= 10) return;
            var predq = Q.GetPrediction(alvoq);
            if (predq.HitChance != HitChance.High) return;
            Q.Cast(alvoq.Position);
            TickQ = Environment.TickCount;
        }
        public static void AmuniçocaPica()
        {
            var morrendo = EntityManager.Heroes.Allies.OrderBy(a => a.Hero).FirstOrDefault(a => !a.IsDead && a.HealthPercent <= 50 && !a.IsRecalling() && !a.IsInShopRange());
            if (morrendo == null || !R.IsReady() || Jho.Mana <= 100) return;
            R.Cast();
            TickR = Environment.TickCount;
        }
        public static void PatenteAlta()
        {
            var ferido = EntityManager.Heroes.Allies.OrderBy(a => a.Hero).FirstOrDefault(a => W.IsInRange(a) && a.HealthPercent <= 70 && !a.IsRecalling() && !a.IsInShopRange() && !a.IsMe && !a.IsZombie && a.IsValid && a.IsTargetableToTeam );
            if (ferido == null || !W.IsReady() || Jho.ManaPercent <= 10 || Jho.HealthPercent <= 10) return;
            W.Cast(ferido);
            TickW = Environment.TickCount;
        }
    }
}
