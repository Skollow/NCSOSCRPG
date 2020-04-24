using System;
using Engine.Models;

namespace Engine.Actions
{
    public class StatChange : BaseAction, IAction
    {
        private readonly int _hitPointsToHeal;
        private readonly int _manaToHeal;
        private readonly int _staminaToHeal;
        private readonly int _maxHitPointsToIncrease;

        public StatChange(GameItem itemInUse, int hitPointsToHeal, int staminaToHeal, int manaToHeal, int maxHitPointsToIncrease)

            : base(itemInUse)
        {
            if (itemInUse.Category != GameItem.ItemCategory.Consumable)
            {
                throw new ArgumentException($"{itemInUse.Name} is not consumable");
            }

            _hitPointsToHeal = hitPointsToHeal;
            _staminaToHeal = staminaToHeal;
            _manaToHeal = manaToHeal;
            _maxHitPointsToIncrease = maxHitPointsToIncrease;
        }

        public void Execute(LivingEntity actor, LivingEntity target)
        {
            string actorName = (actor is Player) ? "You" : $"The {actor.Name.ToLower()}";
            string targetName = (target is Player) ? "yourself" : $"the {target.Name.ToLower()}";

            ReportResult($"{actorName} gain {targetName} for {_maxHitPointsToIncrease} point{(_maxHitPointsToIncrease > 1 ? "s" : "")}" + $"");
            target.StatChange(_hitPointsToHeal, _staminaToHeal, _manaToHeal, _maxHitPointsToIncrease);
        }
    }
}