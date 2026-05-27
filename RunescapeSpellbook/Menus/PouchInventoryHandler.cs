using Force.DeepCloner;
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
    private static int pouchLimit = 50;
    public static void LoadMenu()
    {
        RecalculateItemCap(Game1.player);
        Game1.activeClickableMenu = new ItemGrabMenu(GetItemsForPlayer(), reverseGrab: false, showReceivingMenu: true,
            highlightPacks, grabItemFromInventory, null, grabItemFromChest, snapToBottom: false,
            canBeExitedWithKey: true, playRightClickSound: true, allowRightClick: true, showOrganizeButton: false, 1,
            null, -1, null);
    }
    public static IInventory GetItemsForPlayer()
    { 
        return Game1.player.team.GetOrCreateGlobalInventory("Tofu.RunescapeSpellbook.Pouch_" + Game1.player.UniqueMultiplayerID);
    }
    public static void grabItemFromInventory(Item item, Farmer who)
    {
        IInventory localInv = GetItemsForPlayer();
        int slotsRemaining = localInv.Any(j=>j != null) ? pouchLimit - localInv.Sum(x => x.Stack) : pouchLimit;
        if (slotsRemaining <= 0)
        {
            Game1.showRedMessage(KeyTranslator.GetTranslation("ui.Pouch.text"));
            return;
        }
        
        if (item.Stack == 0)
        {
            item.Stack = 1;
        }
        Item tmp = addItem(item,slotsRemaining);
        if (tmp == null)
        {
            who.removeItemFromInventory(item);
        }
        else
        {
            tmp = who.addItemToInventory(tmp);
            Game1.showRedMessage(KeyTranslator.GetTranslation("ui.Pouch.text"));
        }
        GetItemsForPlayer().RemoveEmptySlots();
        LoadMenu();
    }
    
    public static Item addItem(Item item, int itemsRemaining)
    {
        item.resetState();
        GetItemsForPlayer().RemoveEmptySlots();
        IInventory item_list = GetItemsForPlayer();
        int dupeStack = Math.Min(item.Stack, itemsRemaining); //the amount of items we can put in or the full item stack. Whichever is smaller.
        
        Item duplicateItem = item.getOne();
        duplicateItem.Stack = dupeStack; //Make a new stack that is sized to be exactly how many we can remove
        item = item.ConsumeStack(duplicateItem.Stack); //Remove the amount of stack size we allocated to the newest value
        
        for (int i = 0; i < item_list.Count; i++)
        {
            if (item_list[i] != null && item_list[i].canStackWith(duplicateItem))
            {
                int toRemove = duplicateItem.Stack - item_list[i].addToStack(duplicateItem);
                if (duplicateItem.ConsumeStack(toRemove) == null)
                {
                    return item;
                }
            }
        }
        
        if (item_list.Count < 27)
        {
            item_list.Add(duplicateItem);
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
        return i.Category == -28 && ModAssets.modItems.TryGetValue(i.ItemId, out ModLoadObjects val) && val is PackObject;
    }
    
    public static Item GroundCollect(Item item, Farmer who)
    {
        RecalculateItemCap(who);
        IInventory localInv = GetItemsForPlayer();
        int slotsRemaining = localInv.Any(j=>j != null) ? pouchLimit - localInv.Sum(x => x.Stack) : pouchLimit;
        if (slotsRemaining <= 0)
        {
            return item;
        }

        Item tmp = addItem(item,slotsRemaining);
        return tmp;
    }

    private static void RecalculateItemCap(Farmer who)
    {
        pouchLimit = 50 * (LevelsHandler.GetFarmerMagicLevel(who) * 10);
    }
}