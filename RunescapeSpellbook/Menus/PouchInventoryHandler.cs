using Force.DeepCloner;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared.APIs;
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
        if (IsPouchInventoryFull(who, out int slotsRemaining))
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
    
    public static bool highlightPacks(string id)
    {
        return ModAssets.modItems.TryGetValue(id.Replace("(O)",""), out ModLoadObjects val) && val is PackObject && val.Category == -28;
    }
    
    public static Item GroundCollect(Item item, Farmer who)
    {
        RecalculateItemCap(who);
        if (IsPouchInventoryFull(who,out int slotsRemaining))
        {
            return item;
        }

        Item tmp = addItem(item,slotsRemaining);
        return tmp;
    }

    public static bool IsPouchInventoryFull(Farmer who)
    {
        return IsPouchInventoryFull(who, out int slotsRemaining);
    }
    
    public static bool IsPouchInventoryFull(Farmer who, out int slotsRemaining)
    {
        RecalculateItemCap(who);
        IInventory localInv = GetItemsForPlayer();
        slotsRemaining = localInv.Any(j=>j != null) ? pouchLimit - localInv.Sum(x => x?.Stack ?? 0) : pouchLimit;
        return slotsRemaining <= 0;
    }

    private static void RecalculateItemCap(Farmer who)
    {
        pouchLimit = 50 + (LevelsHandler.GetFarmerMagicLevel(who) * 10);
    }

    public static bool EvaluateCanItemEnterPouch(Farmer who, Debris inst, ISpaceCoreApi api)
    {
        return inst.debrisType.Value == Debris.DebrisType.OBJECT && inst.itemId.Value != null &&
               who.hasOrWillReceiveMail("Tofu.RunescapeSpellbook_RunesFound") &&
               api.GetItemInEquipmentSlot(who, "Tofu.RunescapeSpellbook.PouchSlot") != null && 
               PouchInventoryHandler.highlightPacks(inst.itemId.Value) &&
               !IsPouchInventoryFull(who);
    }
}