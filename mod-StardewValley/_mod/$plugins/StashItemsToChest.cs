using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using Mono.Cecil;
using Mono.Cecil.Cil;

using StardewValley;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace dotNetMT
{
    [PluginTag("StashItemsToChest", "KuroBear", @"Stash items to chest by key (TAB)...")]
    public class StashItemsToChest : PluginBase
    {
        //PhthaloBlue: these blocks of codes below are from Chest Pooling mod by mralbobo
        //repo link here: https://github.com/mralbobo/stardew-chest-pooling, they are useful so I use them
        static StardewValley.Objects.Chest getOpenChest()
        {
            if (StardewValley.Game1.activeClickableMenu == null)  
                return (null);

            if (StardewValley.Game1.activeClickableMenu is StardewValley.Menus.ItemGrabMenu)
            {
                StardewValley.Menus.ItemGrabMenu menu = StardewValley.Game1.activeClickableMenu as StardewValley.Menus.ItemGrabMenu;
                if (menu.behaviorOnItemGrab != null && menu.behaviorOnItemGrab.Target is StardewValley.Objects.Chest)
                    return (menu.behaviorOnItemGrab.Target as StardewValley.Objects.Chest);
            }
            //else
            //{
            //    if (StardewValley.Game1.activeClickableMenu.GetType().Name == "ACAMenu")
            //    {
            //        dynamic thing = (dynamic)StardewValley.Game1.activeClickableMenu;
            //        if (thing != null && thing.chestItems != null)
            //        {
            //            StardewValley.Objects.Chest aChest = new StardewValley.Objects.Chest(true);
            //            aChest.items = thing.chestItems;
            //            return (aChest);
            //        }
            //    }
            //}
            return (null);
        }

        static bool isChestFull(StardewValley.Objects.Chest inputChest)
        {
            return inputChest.items.Count >= StardewValley.Objects.Chest.capacity;
        }

        static void StashUp()
        {
            List<Item> PlayerInventory = Game1.player.items;
            StardewValley.Objects.Chest OpenChest = getOpenChest();

            if (OpenChest == null)
                return;

            if (OpenChest.isEmpty())
                return;

            Game1.playSound("coin");

            List<Item> OpenChestItemList = OpenChest.items;
            foreach (Item playerItem in PlayerInventory)
            {	
				if (playerItem == null)
					continue;
			
                foreach (Item chestItem in OpenChestItemList)
                {
					if (chestItem == null)
						continue;
				
                    if (playerItem.canStackWith(chestItem))
                    {
                        if (isChestFull(OpenChest) && (
                            //chestItem.maximumStackSize() == -1 ||
                            chestItem.getStack() + playerItem.getStack() > chestItem.maximumStackSize()))
                        {
                            continue;
                        }
                        OpenChest.grabItemFromInventory(playerItem, Game1.player);
                        break;
                    }
                }
            }
        }

        public override void OnPreUpdate()
        {
            KeyboardState stateKeyboard = Keyboard.GetState();

            if (stateKeyboard.IsKeyDown(Keys.Tab) && !Game1.oldKBState.IsKeyDown(Keys.Tab))
                StashUp();
        }
    }
}