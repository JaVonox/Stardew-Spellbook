using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Inventories;
using StardewValley.Menus;
using StardewValley.Objects;

namespace RunescapeSpellbook;

/// <summary>
/// Handler for menu that allows you to store packs
/// </summary>
public static class PouchInventoryHandler
{
    public static void LoadMenu()
    {
        Game1.activeClickableMenu = new ItemGrabMenu(GetItemsForPlayer(), reverseGrab: false, showReceivingMenu: true,
            highlightPacks, grabItemFromInventory, null, grabItemFromChest, snapToBottom: false,
            canBeExitedWithKey: true, playRightClickSound: true, allowRightClick: true, showOrganizeButton: true, 1,
            null, -1, null);
    }
    public static IInventory GetItemsForPlayer()
    { 
        return Game1.player.team.GetOrCreateGlobalInventory("Tofu.RunescapeSpellbook.Pouch_" + Game1.player.UniqueMultiplayerID);
    }
    public static void grabItemFromInventory(Item item, Farmer who)
    {
        if (item.Stack == 0)
        {
            item.Stack = 1;
        }
        Item tmp = addItem(item);
        if (tmp == null)
        {
            who.removeItemFromInventory(item);
        }
        else
        {
            tmp = who.addItemToInventory(tmp);
        }
        GetItemsForPlayer().RemoveEmptySlots();
        LoadMenu();
    }
    public static Item addItem(Item item)
    {
        item.resetState();
        GetItemsForPlayer().RemoveEmptySlots();
        IInventory item_list = GetItemsForPlayer();
        for (int i = 0; i < item_list.Count; i++)
        {
            if (item_list[i] != null && item_list[i].canStackWith(item))
            {
                int toRemove = item.Stack - item_list[i].addToStack(item);
                if (item.ConsumeStack(toRemove) == null)
                {
                    return null;
                }
            }
        }
        if (item_list.Count < 27)
        {
            item_list.Add(item);
            return null;
        }
        return item;
    }
    
    public static void grabItemFromChest(Item item, Farmer who)
    {
        if (who.couldInventoryAcceptThisItem(item))
        {
            GetItemsForPlayer().Remove(item);
            GetItemsForPlayer().RemoveEmptySlots();
            LoadMenu();
        }
    }
    
    public static bool highlightPacks(Item i)
    {
        return i.Category == -28 && ModAssets.modItems.ContainsKey(i.ItemId) && ModAssets.modItems[i.ItemId] is PackObject;
    }
}