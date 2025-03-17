using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Objects;
using StardewValley.TerrainFeatures;

namespace StardewTestMod;

public enum SpellType
{
    Teleport,
    MapUtility,
    InventoryUtility,
    Combat
}

public delegate KeyValuePair<bool, string> SpellMethod(ref Item? i, Predicate<object>? p);

public struct ModLoadObjects
{
    private int id;
    private string name;
    private string displayName;
    private string description;
    private string type;
    private int spriteIndex;
    private int category;

    public ModLoadObjects(int id, string name, string displayName, string description, string type = "Basic", int category = -2)
    {
        this.id = id;
        this.name = name;
        this.displayName = displayName;
        this.description = description;
        this.type = type;
        this.spriteIndex = id - 4290;
        this.category = category;
    }

    public void AppendObject(string CustomTextureKey, IDictionary<string,ObjectData> ObjectsSet)
    {
        ObjectData newItem = new ObjectData();
        newItem.Name = this.name;
        newItem.DisplayName = this.displayName;
        newItem.Description = this.description;
        newItem.Type = this.type;
        newItem.Texture = CustomTextureKey;
        newItem.SpriteIndex = this.spriteIndex;
        newItem.Category = this.category;
        ObjectsSet[$"{id}"] = newItem;
    }
}

public struct Spell
{
    public int id;
    public string name;
    public string displayName;
    public string description;
    public SpellType spellType;
    public int magicLevelRequirement;
    public int spriteIndex;
    
    public SpellMethod DoAction; //The function to use. returns a bool for if it was successful, string as any output args, and may take an item as an Input
    public Dictionary<int,int> requiredItems; //Set of IDs for the required runes - add duplicates to designate more than 1 item required
    public Predicate<object>? castPredicate; //Miscellanious predicate to determine if there is any extra conditions that must be met - spell effects dictate where to use this
    public Spell(int id, string name, string displayName, string description, SpellType spellType, int magicLevelRequirement, int spriteIndex, Dictionary<int,int> requiredItems, SpellMethod DoAction, Predicate<object>? castPredicate = null)
    {
        this.id = id;
        this.name = name;
        this.displayName = displayName;
        this.description = description;
        this.spellType = spellType;
        this.magicLevelRequirement = magicLevelRequirement;
        this.spriteIndex = spriteIndex;
        this.requiredItems = requiredItems;
        this.DoAction = DoAction;
        this.castPredicate = castPredicate;
    }

    public KeyValuePair<bool,string> CanCastSpell()
    {
        //TODO add magic level checking
        foreach (int runeID in requiredItems.Keys)
        {
            if (!HasRuneCost(runeID))
            {
                return new KeyValuePair<bool,string>(false, "You do not have enough runes to perform this spell");
            }
        }
        return new KeyValuePair<bool,string>(true, "");;
    }

    public bool HasRuneCost(int runeID)
    {
        return (Game1.player.Items.CountId($"{runeID}") >= requiredItems[runeID]);
    }

    public KeyValuePair<bool,string> CastSpell(bool isInventorySpellMenu, ref Item? itemArgs)
    {
        KeyValuePair<bool,string> actionResult = CanCastSpell();
        if (actionResult.Key) //First pass of action result checks if we can actually cast the selected spell - either due to level or rune cost etc.
        {
            switch (spellType)
            {
                //If the spell is teleport or map utility, we can just cast the spell immediately
                case SpellType.InventoryUtility: //If we are using an inventory utility spell, we need to open the inventory spell menu first
                    if (!isInventorySpellMenu) //If we're not in the inventory menu,
                    {
                        Game1.activeClickableMenu = new InventorySpellMenu(this,castPredicate);
                        break;
                    }
                    goto default; //Fallthrough if we are already in the inventory spell menu
                case SpellType.Combat:
                    break;
                case SpellType.Teleport:
                case SpellType.MapUtility:
                default:
                    actionResult = DoAction(ref itemArgs,castPredicate);
            
                    if(actionResult.Key) //Second pass checks if there are any spell specific issues - like how teleporting is forbidden on festival days
                    {
                        foreach (KeyValuePair<int, int> runeCost in requiredItems) //Remove runes if we have successfully cast the spell
                        {
                            Game1.player.Items.ReduceId($"{runeCost.Key}", runeCost.Value); 
                        }
                    }

                    break;
            }
        }

        return actionResult;
    }
}

public static class ModAssets
{
    public static Texture2D extraTextures;

    public const int spellsY = 16;
    public const int spellsSize = 80;
    private static object multiplayer;
    
    public static readonly ModLoadObjects[] modItems = {
        new ModLoadObjects(4290,"Rune_Blank","Pure Essence","An unimbued rune of extra capability."),
        new ModLoadObjects(4291,"Rune_Air","Air Rune","One of the 4 basic elemental Runes"),
        new ModLoadObjects(4292,"Rune_Water","Water Rune","One of the 4 basic elemental Runes"),
        new ModLoadObjects(4293,"Rune_Fire","Fire Rune","One of the 4 basic elemental Runes"),
        new ModLoadObjects(4294,"Rune_Earth","Earth Rune","One of the 4 basic elemental Runes"),
        new ModLoadObjects(4295,"Rune_Law","Law Rune","Used for teleport spells"),
        new ModLoadObjects(4296,"Rune_Nature","Nature Rune","Used for alchemy spells"),
        new ModLoadObjects(4297,"Rune_Cosmic","Cosmic Rune","Used for enchant spells"),
        new ModLoadObjects(4298,"Rune_Astral","Astral Rune","Used for Lunar spells")
    };
    
    public static readonly Spell[] modSpells = {
        new Spell(0,"Teleport_Valley","Valley Teleport","Teleports you to Pierre's Store in Pelican Town",SpellType.Teleport,0,0,
            new Dictionary<int, int>() { {4295, 1},{4291,3},{4293,1} },SpellEffects.TeleportToPierre),
        new Spell(1,"Teleport_Home","Farm Teleport","Teleports you to your Farm",SpellType.Teleport,1,1,
            new Dictionary<int, int>() { {4295, 1},{4291,1},{4294,1} }, SpellEffects.TeleportToFarm),
        new Spell(2,"Menu_Superheat","Superheat Item","Smelts ore without a furnace or coal",SpellType.InventoryUtility,1,2,
            new Dictionary<int, int>() { {4296, 1},{4293,4}}, SpellEffects.SuperheatItem),
        new Spell(3,"Menu_HighAlch","High Level Alchemy","Converts an item into gold",SpellType.InventoryUtility,1,3,
            new Dictionary<int, int>() { {4296, 1},{4293,5}}, SpellEffects.HighAlchemy,(i=>i is Item item && item.canBeShipped() && item.salePrice(false) > 0)),
        new Spell(4,"Area_Humidify","Humidify","Waters the ground around you",SpellType.MapUtility,1,4,
            new Dictionary<int, int>() { {4298, 1},{4293,1},{4292,3}}, SpellEffects.Humidify,
            (tile => tile is HoeDirt hoeLand && (hoeLand.crop == null || !hoeLand.crop.forageCrop.Value || hoeLand.crop.whichForageCrop.Value != "2") && hoeLand.state.Value != 1)),
        new Spell(5,"Area_Cure","Cure Plant","Replants dead crops",SpellType.MapUtility,1,5,
            new Dictionary<int, int>() { {4298, 1},{4294,8}}, SpellEffects.CurePlant, 
            (tile => tile is HoeDirt hoeLand && hoeLand.crop != null && hoeLand.crop.dead.Value)),
    };
    public static void Load(IModHelper helper)
    {
        extraTextures = helper.ModContent.Load<Texture2D>("assets\\modsprites"); 
        multiplayer = helper.Reflection.GetField<object>(typeof(Game1), "multiplayer").GetValue();
    }

    public static void BroadcastSprite(GameLocation location, TemporaryAnimatedSprite sprite)
    {
        var method = multiplayer.GetType().GetMethod("broadcastSprites", 
            new[] { typeof(GameLocation), typeof(TemporaryAnimatedSprite[]) });
        
        var spriteArray = new TemporaryAnimatedSprite[] { sprite };
        
        method.Invoke(multiplayer, new object[] { location, spriteArray });
    }
}