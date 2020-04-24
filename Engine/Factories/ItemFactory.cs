﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Engine.Actions;
using Engine.Models;
using Engine.Shared;

namespace Engine.Factories
{
    public static class ItemFactory
    {
        private const string GAME_DATA_FILENAME = ".\\GameData\\GameItems.xml";

        private static readonly List<GameItem> _standardGameItems = new List<GameItem>();

        static ItemFactory()
        {
            if (File.Exists(GAME_DATA_FILENAME))
            {
                XmlDocument data = new XmlDocument();
                data.LoadXml(File.ReadAllText(GAME_DATA_FILENAME));

                LoadItemsFromNodes(data.SelectNodes("/GameItems/Weapons/Weapon"));
                LoadItemsFromNodes(data.SelectNodes("/GameItems/StatChangeItems/StatChangeItem"));
                LoadItemsFromNodes(data.SelectNodes("/GameItems/MiscellaneousItems/MiscellaneousItem"));
                LoadItemsFromNodes(data.SelectNodes("/GameItems/Spells/Spell")); 
            }
            else
            {
                throw new FileNotFoundException($"Missing data file: {GAME_DATA_FILENAME}");
            }
        }

        public static GameItem CreateGameItem(int itemTypeID)
        {
            return _standardGameItems.FirstOrDefault(item => item.ItemTypeID == itemTypeID)?.Clone();
        }

        public static string ItemName(int itemTypeID)
        {
            return _standardGameItems.FirstOrDefault(i => i.ItemTypeID == itemTypeID)?.Name ?? "";
        }

        private static void LoadItemsFromNodes(XmlNodeList nodes)
        {
            if (nodes == null)
            {
                return;
            }

            foreach (XmlNode node in nodes)
            {
                GameItem.ItemCategory itemCategory = DetermineItemCategory(node.Name);

                GameItem gameItem =
                    new GameItem(itemCategory,
                                 node.AttributeAsInt("ID"),
                                 node.AttributeAsString("Name"),
                                 node.AttributeAsInt("Price"),
                                 itemCategory == GameItem.ItemCategory.Weapon);

                if (itemCategory == GameItem.ItemCategory.Weapon)
                {
                    gameItem.Action =
                        new AttackWithWeapon(gameItem,
                                             node.AttributeAsInt("MinimumDamage"),
                                             node.AttributeAsInt("MaximumDamage"),
                                             node.AttributeAsInt("StaminaCost"));
                }

                else if (itemCategory == GameItem.ItemCategory.Spell)
                {
                    gameItem.Action =
                        new AttackWithSpell(gameItem,
                                           node.AttributeAsInt("MinimumDamage"),
                                           node.AttributeAsInt("MaximumDamage"),
                                           node.AttributeAsInt("ManaCost"));
                }

                else if (itemCategory == GameItem.ItemCategory.Consumable)
                {
                    gameItem.Action =
                        new StatChange(gameItem, 
                                           node.AttributeAsInt("HitPointsToHeal"),
                                           node.AttributeAsInt("StaminaToHeal"),
                                           node.AttributeAsInt("ManaToHeal"),
                                           node.AttributeAsInt("MaxHitPointsToIncrease"));
                }

                _standardGameItems.Add(gameItem);
            }
        }

        private static GameItem.ItemCategory DetermineItemCategory(string itemType)
        {
            switch (itemType)
            {
                case "Weapon":
                    return GameItem.ItemCategory.Weapon;
                case "StatChangeItem":
                    return GameItem.ItemCategory.Consumable;
                case "Spell":
                    return GameItem.ItemCategory.Spell;
                default:
                    return GameItem.ItemCategory.Miscellaneous;
            }
        }
    }
}