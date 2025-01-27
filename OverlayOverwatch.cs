using System;
using System.Collections.Generic;

using SmartOverlays;
using InventorySystem.Items;
using helpers.Extensions;
using Compendium;

namespace BasicOverlay {
    public class OverlayOverwatch : OverlayManager.Overlay {
        public ReferenceHub TargetHub;

        private List<Message> inventory;
        private Dictionary<ItemType, Message> ammo;
        public static int inventoryLine = -6;
        public static MessageAlign inventoryAlign = MessageAlign.Left;
        public static int ammoLine = -5;
        public static MessageAlign ammoAlign = MessageAlign.Right;


        public OverlayOverwatch() : base("Overwatch"/*, 10*/) {
            TargetHub = null;
            inventory = new List<Message>(8);
            ammo = new Dictionary<ItemType, Message>();
            int i;

            AddMessage(new Message(""), inventoryLine, inventoryAlign);
            for (i = 1; i <= 8; i++) {
                Message message = new Message("");
                inventory.Add(message);
                AddMessage(message, (float)inventoryLine - (0.7f * (float)i), inventoryAlign);
            }

            i = 1;
            AddMessage(new Message("<size=75%>Players ammo:</size>"), ammoLine, ammoAlign);
            foreach (var ammoType in new ItemType[] { ItemType.Ammo9x19, ItemType.Ammo556x45, ItemType.Ammo762x39, ItemType.Ammo12gauge, ItemType.Ammo44cal }) {
                Message message = new Message($"<pos=87%><size=50%>{ammoType.ToString().Substring(4)}: 0</size></pos>");
                ammo.Add(ammoType, message);
                AddMessage(message, (float)ammoLine - (0.7f * (float)i), MessageAlign.Left);
                i++;
            }
        }

        public override void UpdateMessages() {
            if (TargetHub == null) return;
            List<Tuple<ItemBase,int>> items = new List<Tuple<ItemBase,int>>();
            foreach (var item in TargetHub.GetItems()) {
                items.Add(new Tuple<ItemBase, int>(item, GetItemOrderNumber(item)));
            }
            items.Sort(new ItemSorter());

            int i = 0;
            foreach (var playerItem in items) {
                inventory[i].Content = $"<size=50%>{i+1}: <b><color={GetCategoryColor(playerItem.Item1.Category)}>{playerItem.Item1.ItemTypeId.ToString().SpaceByPascalCase()}</b></color></size>";
                i++;
            }
            for (; i < 8; i++) {
                inventory[i].Content = $"<size=50%>{i+1}:</size>";
            }

            foreach (var ammoPair in TargetHub.inventory.UserInventory.ReserveAmmo) {
                ammo[ammoPair.Key].Content = $"<pos=87%><size=50%>{ammoPair.Key.ToString().Substring(4)}: {ammoPair.Value}</size></pos>";
            }
            
        }

        private static int GetItemOrderNumber(ItemBase item) {
            switch (item.Category) {
                case ItemCategory.Keycard: return 0;
                case ItemCategory.Firearm: return 1;
                case ItemCategory.SpecialWeapon: return 2;
                case ItemCategory.Grenade: return 3;
                case ItemCategory.SCPItem: return 5;
                case ItemCategory.Medical: return 6;
                case ItemCategory.Armor: return 7;
                case ItemCategory.Radio:
                case ItemCategory.None:
                default: return 8;
            }
        }

        private static string GetCategoryColor(ItemCategory category) {
            switch (category) {
                case ItemCategory.Keycard: return "#ffe600";
                case ItemCategory.Firearm: return "#00e5ff";
                case ItemCategory.SpecialWeapon: return "#0091ff";
                case ItemCategory.Grenade: return "#0044ff";
                case ItemCategory.SCPItem: return "#ff00dd";
                case ItemCategory.Medical: return "#ff000d";
                case ItemCategory.Armor: return "#ff7700";
                case ItemCategory.Radio:
                case ItemCategory.None:
                default: return "#ffffff";
            }
        }
    }

    class ItemSorter : IComparer<Tuple<ItemBase, int>> {
        public int Compare(Tuple<ItemBase, int> left, Tuple<ItemBase, int> right) {
            if (left.Item2 != right.Item2) return left.Item2 - right.Item2;
            return left.Item1.ItemTypeId.ToString().CompareTo(right.Item1.ItemTypeId.ToString());
        }
    }
}
