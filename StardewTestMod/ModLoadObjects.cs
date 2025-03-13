using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
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
        ObjectsSet[$"(O){id}"] = newItem;
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
    public Dictionary<int,int> requiredItems; //Set of IDs for the required runes - add duplicates to designate more than 1 item required

    public Spell(int id, string name, string displayName, string description, int magicLevelRequirement, int spriteIndex, Dictionary<int,int> requiredItems)
    {
        this.id = id;
        this.name = name;
        this.displayName = displayName;
        this.description = description;
        this.magicLevelRequirement = magicLevelRequirement;
        this.spriteIndex = spriteIndex;
        this.requiredItems = requiredItems;
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
            new Dictionary<int, int>() { {4295, 1},{4291,3},{4293,1} }),
        new Spell(1,"Teleport_Home","Farm Teleport","Teleports you to the Farm",1,1,
            new Dictionary<int, int>() { {4295, 1},{4291,1},{4294,1} })
    };
    public static void Load(IModHelper helper)
    {
        extraTextures = helper.ModContent.Load<Texture2D>("assets\\modsprites"); 
    }
}