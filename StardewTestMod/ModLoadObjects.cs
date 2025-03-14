using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Objects;

namespace StardewTestMod;

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
    public int magicLevelRequirement;
    public int spriteIndex;
    public Func<KeyValuePair<bool,string>> DoAction;
    public Dictionary<int,int> requiredItems; //Set of IDs for the required runes - add duplicates to designate more than 1 item required

    public Spell(int id, string name, string displayName, string description, int magicLevelRequirement, int spriteIndex, Dictionary<int,int> requiredItems, Func<KeyValuePair<bool,string>> DoAction)
    {
        this.id = id;
        this.name = name;
        this.displayName = displayName;
        this.description = description;
        this.magicLevelRequirement = magicLevelRequirement;
        this.spriteIndex = spriteIndex;
        this.requiredItems = requiredItems;
        this.DoAction = DoAction;
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

    public KeyValuePair<bool,string> CastSpell()
    {
        KeyValuePair<bool,string> actionResult = CanCastSpell();
        if (actionResult.Key) //First pass of action result checks if we can actually cast the selected spell - either due to level or rune cost etc.
        {
            actionResult = DoAction();
            
            if(actionResult.Key) //Second pass checks if there are any spell specific issues - like how teleporting is forbidden on festival days
            {
                foreach (KeyValuePair<int, int> runeCost in requiredItems) //Remove runes if we have successfully cast the spell
                {
                    Game1.player.Items.ReduceId($"{runeCost.Key}", runeCost.Value); 
                }
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
        new Spell(0,"Teleport_Valley","Valley Teleport","Teleports you to Pierre's Store in Pelican Town",0,0,
            new Dictionary<int, int>() { {4295, 1},{4291,3},{4293,1} },SpellEffects.TeleportToPierre),
        new Spell(1,"Teleport_Home","Farm Teleport","Teleports you to your Farm",1,1,
            new Dictionary<int, int>() { {4295, 1},{4291,1},{4294,1} }, SpellEffects.TeleportToFarm),
        new Spell(2,"Menu_Superheat","Superheat Item","Smelts ore without a furnace or coal",1,2,
            new Dictionary<int, int>() { {4296, 1},{4293,4}}, SpellEffects.TeleportToFarm),
        new Spell(3,"Menu_HighAlch","High Level Alchemy","Converts an item into gold",1,3,
            new Dictionary<int, int>() { {4296, 1},{4293,5}}, SpellEffects.TeleportToFarm),
        new Spell(4,"Area_Humidify","Humidify","Waters the ground around you",1,4,
            new Dictionary<int, int>() { {4298, 1},{4293,1},{4292,3}}, SpellEffects.TeleportToFarm),
        new Spell(5,"Area_Cure","Cure Plant","Replants Dead Crops",1,5,
            new Dictionary<int, int>() { {4298, 1},{4294,8}}, SpellEffects.TeleportToFarm),
    };
    public static void Load(IModHelper helper)
    {
        extraTextures = helper.ModContent.Load<Texture2D>("assets\\modsprites"); 
    }
}