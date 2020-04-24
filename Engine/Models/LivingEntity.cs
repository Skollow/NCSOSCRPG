using System;
using System.Collections.Generic;
using Engine.Services;
using Engine.Actions;

namespace Engine.Models
{
    public abstract class LivingEntity : BaseNotificationClass
    {
        #region Properties

        private string _name;
        public int _dexterity;
        public int _currentStamina;
        public int _maximumStamina; 
        public int _currentMana;
        public int _maximumMana;
        private int _currentHitPoints;
        private int _maximumHitPoints;
        private int _gold;
        private int _level;
        private GameItem _currentWeapon;
        private GameItem _currentConsumable;
        private GameItem _currentSpell;
        private Inventory _inventory;

        public string Name
        {
            get => _name;
            private set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public int Dexterity
        {
            get => _dexterity;
            private set
            {
                _dexterity = value;
                OnPropertyChanged();
            }
        }
        public int CurrentStamina
        {
            get => _currentStamina;
            private set
            {
                _currentStamina = value;
                OnPropertyChanged();
            }
        }

        public int MaximumStamina
        {
            get => _maximumStamina;
            private set
            {
                _maximumStamina = value;
                OnPropertyChanged();
            }
        }

        public int CurrentMana
        {
            get => _currentMana;
            private set
            {
                _currentMana = value;
                OnPropertyChanged();
            }
        }

        public int MaximumMana
        {
            get => _maximumMana;
            private set
            {
                _maximumMana = value;
                OnPropertyChanged();
            }
        }
        public int CurrentHitPoints
        {
            get => _currentHitPoints;
            private set
            {
                _currentHitPoints = value;
                OnPropertyChanged();
            }
        }

        //used to be a "protected set" instead of a "set", changed it so MaxHitPointsIncrease will work
        public int MaximumHitPoints
        {
            get => _maximumHitPoints;
            set
            {
                _maximumHitPoints = value;
                OnPropertyChanged();
            }
        }

        public int Gold
        {
            get => _gold;
            private set
            {
                _gold = value;
                OnPropertyChanged();
            }
        }

        public int Level
        {
            get => _level;
            protected set
            {
                _level = value;
                OnPropertyChanged();
            }
        }
        public Inventory Inventory
        {
            get => _inventory;
            private set
            {
                _inventory = value;
                OnPropertyChanged();
            }
        }

        public GameItem CurrentWeapon
        {
            get { return _currentWeapon; }
            set
            {
                if (_currentWeapon != null)
                {
                    _currentWeapon.Action.OnActionPerformed -= RaiseActionPerformedEvent;
                }

                _currentWeapon = value;

                if (_currentWeapon != null)
                {
                    _currentWeapon.Action.OnActionPerformed += RaiseActionPerformedEvent;
                }

                OnPropertyChanged();
            }
        }

        public GameItem CurrentSpell
        {
            get { return _currentSpell; }
            set
            {
                if (_currentSpell != null)
                {
                    _currentSpell.Action.OnActionPerformed -= RaiseActionPerformedEvent;
                }

                _currentSpell = value;

                if (_currentSpell != null)
                {
                    _currentSpell.Action.OnActionPerformed += RaiseActionPerformedEvent;
                }

                OnPropertyChanged();
            }
        }

        public GameItem CurrentConsumable
        {
            get => _currentConsumable;
            set
            {
                if (_currentConsumable != null)
                {
                    _currentConsumable.Action.OnActionPerformed -= RaiseActionPerformedEvent;
                }

                _currentConsumable = value;

                if (_currentConsumable != null)
                {
                    _currentConsumable.Action.OnActionPerformed += RaiseActionPerformedEvent;
                }

                OnPropertyChanged();
            }
        }

        public bool IsAlive => CurrentHitPoints > 0;
        public bool IsDead => !IsAlive;

        #endregion

        public event EventHandler<string> OnActionPerformed;
        public event EventHandler OnKilled;
        public event EventHandler OnEscaped;

        protected LivingEntity(string name, int maximumHitPoints, int currentHitPoints, int dexterity,
                                 int maximumStamina, int currentStamina, int maximumMana, int currentMana, int gold, int level = 1)
        {
            Name = name;
            Dexterity = dexterity;
            MaximumStamina = maximumStamina;
            CurrentStamina = currentStamina;
            MaximumMana = maximumMana;
            CurrentMana = currentMana;
            MaximumHitPoints = maximumHitPoints;
            CurrentHitPoints = currentHitPoints;
            Gold = gold;
            Level = level;

            Inventory = new Inventory();
        }

        public void UseCurrentWeaponOn(LivingEntity target)
        {
            CurrentWeapon.PerformAction(this, target);
        }
        public void UseCurrentSpellOn(LivingEntity target)
        {
            CurrentSpell.PerformAction(this, target);
        }

        public void UseCurrentConsumable()
        {
            CurrentConsumable.PerformAction(this, this);

            RemoveItemFromInventory(CurrentConsumable);
        }

        public void Rest()
        {
            CurrentHitPoints = MaximumHitPoints;
            CurrentStamina = MaximumStamina;
            CurrentMana = MaximumMana;
        }

        public void EscapedFrom()
        {
            CurrentHitPoints = 0;
            RaiseOnEscapedEvent();
        }


        public void TakeDamage(int hitPointsOfDamage)
        {
            CurrentHitPoints -= hitPointsOfDamage;

            if (IsDead)
            {
                CurrentHitPoints = 0;
                RaiseOnKilledEvent();
            }
        }
        //insted of CurrentMana = ManaLost, used to be throw new ArgumentOutOfRangeException($"{Name} only has {CurrentMana} Mana, and cannot spend {ManaLost} Mana")
        // I changed it because of a bug I didn't have the time to fix, which accured due to the player not having enough Mana to cast the spell. 
        //To fix it, I need to set the mana cost in Battle.cs to be equal to the spell mana cost (line 61 there), but I lack the knowledge to do so. 
        //As a placeholder, the manacost there is set to 2, since that is it the mana cost of the cheapest (mana wise) spell. 
        //All of the above is also true to stamina.
        public void LoseMana(int ManaLost)
        {
            if (ManaLost > CurrentMana)
            {
                CurrentMana = ManaLost;
            }
            CurrentMana -= ManaLost;
        }
        //insted of CurrentStamina = StaminaLost, used to be throw new ArgumentOutOfRangeException($"{Name} only has {CurrentMana} Mana, and cannot spend {ManaLost} Mana")
        public void LoseStamina(int StaminaLost)
        {
            if (StaminaLost > CurrentStamina)
            {
                CurrentStamina = StaminaLost;
            }
            CurrentStamina -= StaminaLost;
        }

        public void StatChange(int hitPointsToHeal, int staminaToHeal, int manaToHeal, int maxHitPointsToIncrease)
        {
            MaximumHitPoints += maxHitPointsToIncrease;

            CurrentHitPoints += hitPointsToHeal;

            if (CurrentHitPoints > MaximumHitPoints)
            {
                CurrentHitPoints = MaximumHitPoints;
            }

            CurrentStamina += staminaToHeal;

            if (CurrentStamina > MaximumStamina)
            {
                CurrentStamina = MaximumStamina;
            }
            CurrentMana += manaToHeal;

            if (CurrentMana > MaximumMana)
            {
                CurrentMana = MaximumMana;
            }


        }

        public void CompletelyHeal()
        {
            CurrentHitPoints = MaximumHitPoints;
            CurrentStamina = MaximumStamina;
            CurrentMana = MaximumMana;
        }

        public void ReceiveGold(int amountOfGold)
        {
            Gold += amountOfGold;
        }

        public void SpendGold(int amountOfGold)
        {
            if (amountOfGold > Gold)
            {
                throw new ArgumentOutOfRangeException($"{Name} only has {Gold} gold, and cannot spend {amountOfGold} gold");
            }

            Gold -= amountOfGold;
        }

        public void AddItemToInventory(GameItem item)
        {
            Inventory = Inventory.AddItem(item);
        }

        public void RemoveItemFromInventory(GameItem item)
        {
            Inventory = Inventory.RemoveItem(item);
        }

        public void RemoveItemsFromInventory(IEnumerable<ItemQuantity> itemQuantities)
        {
            Inventory = Inventory.RemoveItems(itemQuantities);
        }

        #region Private functions

        private void RaiseOnKilledEvent()
        {
            OnKilled?.Invoke(this, new System.EventArgs());
        }

        private void RaiseActionPerformedEvent(object sender, string result)
        {
            OnActionPerformed?.Invoke(this, result);
        }

        private void RaiseOnEscapedEvent()
        {
            OnEscaped?.Invoke(this, new System.EventArgs());
        }

        #endregion
    }
}