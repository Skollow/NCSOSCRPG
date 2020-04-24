using System;
using Engine.EventArgs;
using Engine.Services;
using Engine.Models;
using Engine.Actions;

namespace Engine.Models
{
    public class Battle : IDisposable
    {
        private readonly MessageBroker _messageBroker = MessageBroker.GetInstance();
        private readonly Player _player;
        private readonly Monster _opponent;
        public event EventHandler<CombatVictoryEventArgs> OnCombatVictory;

        public Battle(Player player, Monster opponent)
        {
            _player = player;
            _opponent = opponent;

            _player.OnActionPerformed += OnCombatantActionPerformed;
            _opponent.OnActionPerformed += OnCombatantActionPerformed;
            _opponent.OnKilled += OnOpponentKilled;
            _opponent.OnEscaped += OnPlayerEscaped;

            _messageBroker.RaiseMessage("");
            _messageBroker.RaiseMessage($"You see a {_opponent.Name} here!");

            if (CombatService.FirstAttacker(_player, _opponent) == CombatService.Combatant.Opponent)
            {
                AttackPlayer();
            }
        }

        //need to fix the "enough stamina" part, for now it's 2 instead of StaminaCost

        public void AttackOpponent()
        {
            if (_player.CurrentStamina < 2)
            {
                _messageBroker.RaiseMessage("You must have enough Stamina, to use a weapon.");
                return;
            }
            if (_player.CurrentWeapon == null)
            {
                _messageBroker.RaiseMessage("You must select a weapon, to attack.");
                return;
            }

            if (_player.CurrentWeapon != null)
            _player.UseCurrentWeaponOn(_opponent);

            if (_opponent.IsAlive)
            {
                AttackPlayer();
            }
        }
        //need to fix the "enough mana" part, for now it's 2 instead of manaCost
        public void SpellAttackOpponent()
        {
            if (_player.CurrentMana < 2 )
            {
                _messageBroker.RaiseMessage("You must have enough mana, to use a spell.");
                return;
            }
            if ( _player.CurrentSpell == null)
            {
                _messageBroker.RaiseMessage("You must select a spell, to attack.");
                return;
            }
            if (_player.CurrentSpell != null)
                _player.UseCurrentSpellOn(_opponent);

            if (_opponent.IsAlive)
            {
                AttackPlayer();
            }
        }

        public void EscapeFromOpponent()
        {
            if (CombatService.EscapeOpponent(_player, _opponent) == CombatService.Combatant.Opponent)
            {
                _messageBroker.RaiseMessage($"You managed to run away from the {_opponent.Name}!");
                _opponent.EscapedFrom();
            }
            else if (CombatService.EscapedButHarmed(_player, _opponent) == CombatService.Combatant.Opponent)
            {
                _messageBroker.RaiseMessage($"You escaped from the {_opponent.Name}, but the monster was quick enough to hit you one last time!");
                AttackPlayer();
                _opponent.EscapedFrom();
            }
            else
            {
                _messageBroker.RaiseMessage("You were too slow to escape, the monster attacked you!");
                AttackPlayer();
            }
        }


        public void Dispose()
        {
            _player.OnActionPerformed -= OnCombatantActionPerformed;
            _opponent.OnActionPerformed -= OnCombatantActionPerformed;
            _opponent.OnKilled -= OnOpponentKilled;
        }

        private void OnPlayerEscaped(object sender, System.EventArgs e)
        {
            OnCombatVictory?.Invoke(this, new CombatVictoryEventArgs());
        }

        private void OnOpponentKilled(object sender, System.EventArgs e)
        {
            _messageBroker.RaiseMessage("");
            _messageBroker.RaiseMessage($"You defeated the {_opponent.Name}!");

            _messageBroker.RaiseMessage($"You receive {_opponent.RewardExperiencePoints} experience points.");
            _player.AddExperience(_opponent.RewardExperiencePoints);

            _messageBroker.RaiseMessage($"You receive {_opponent.Gold} gold.");
            _player.ReceiveGold(_opponent.Gold);

            foreach (GameItem gameItem in _opponent.Inventory.Items)
            {
                _messageBroker.RaiseMessage($"You receive one {gameItem.Name}.");
                _player.AddItemToInventory(gameItem);
            }

            OnCombatVictory?.Invoke(this, new CombatVictoryEventArgs());
        }

        private void AttackPlayer()
        {
            _opponent.UseCurrentWeaponOn(_player);
        }

        private void OnCombatantActionPerformed(object sender, string result)
        {
            _messageBroker.RaiseMessage(result);
        }
    }
}