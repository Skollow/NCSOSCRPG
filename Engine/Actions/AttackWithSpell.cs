using System;
using Engine.Services;
using Engine.Models;

namespace Engine.Actions
{
    public class AttackWithSpell : BaseAction, IAction
    {
        private readonly int _maximumDamage;
        private readonly int _minimumDamage;
        public int _manaCost;

        public AttackWithSpell(GameItem itemInUse, int minimumDamage, int maximumDamage, int manaCost)
            : base(itemInUse)
        {
            if (itemInUse.Category != GameItem.ItemCategory.Spell)
            {
                throw new ArgumentException($"{itemInUse.Name} is not a Spell");
            }

            if (minimumDamage < 0)
            {
                throw new ArgumentException("minimumDamage must be 0 or larger");
            }

            if (maximumDamage < minimumDamage)
            {
                throw new ArgumentException("maximumDamage must be >= minimumDamage");
            }

            _minimumDamage = minimumDamage;
            _maximumDamage = maximumDamage;
            _manaCost = manaCost;
        }
        public void Execute(LivingEntity actor, LivingEntity target)
        {
            string actorName = (actor is Player) ? "You" : $"The {actor.Name.ToLower()}";
            string targetName = (target is Player) ? "you" : $"the {target.Name.ToLower()}";

            if (CombatService.AttackSucceeded(actor, target))
            {
                int manaLost = _manaCost;

                actor.LoseMana(manaLost);

                if (CombatService.CriticalHit(actor, target))
                {
                    int damage = 2* RandomNumberGenerator.NumberBetween(_minimumDamage, _maximumDamage);
                    target.TakeDamage(damage);
                    ReportResult($"{actorName} hit {targetName} with a critial hit, dealing  {damage} point{(damage > 1 ? "s" : "")}.");
                }
                else
                {
                    int damage = RandomNumberGenerator.NumberBetween(_minimumDamage, _maximumDamage);
                    ReportResult($"{actorName} hit {targetName} for {damage} point{(damage > 1 ? "s" : "")}.");
                    target.TakeDamage(damage);
                }

            }
            else
            {
                int manaLost = _manaCost;

                actor.LoseMana(manaLost);

                ReportResult($"{actorName} missed {targetName}.");
            }
        }
    }
}