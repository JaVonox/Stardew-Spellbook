using Microsoft.Xna.Framework;
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
    Combat,
    Buff
}

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

public static class ModAssets
{
    public static Texture2D extraTextures;
    public static Texture2D animTextures;
    
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
        new ModLoadObjects(4298,"Rune_Astral","Astral Rune","Used for Lunar spells"),
        new ModLoadObjects(4299,"Rune_Chaos","Chaos Rune","Used for low level combat spells"),
        new ModLoadObjects(4300,"Rune_Death","Death Rune","Used for high level combat spells")
    };
    
    public static readonly Spell[] modSpells = {
        new Spell(0,"Teleport_Valley","Valley Teleport","Teleports you to Pierre's Store in Pelican Town",SpellType.Teleport,1,0,
            new Dictionary<int, int>() { {4295, 1},{4291,3},{4293,1} },SpellEffects.TeleportToPierre),
        new Spell(1,"Teleport_Home","Farm Teleport","Teleports you to your Farm",SpellType.Teleport,4,1,
            new Dictionary<int, int>() { {4295, 1},{4291,1},{4294,1} }, SpellEffects.TeleportToFarm),
        new Spell(2,"Menu_Superheat","Superheat Item","Smelts ore without a furnace or coal",SpellType.InventoryUtility,2,2,
            new Dictionary<int, int>() { {4296, 1},{4293,4}}, SpellEffects.SuperheatItem,
            (i=>i is Item item && DataLoader.Machines(Game1.content).GetValueOrDefault("(BC)13").OutputRules.Any(x=>x.Triggers.Any(y=>y.RequiredItemId == item.QualifiedItemId))),
            "Smelt any ores into bars instantly without any coal cost. Put an appropriate item in the slot and press the spell icon to cast."),
        new Spell(3,"Menu_HighAlch","High Level Alchemy","Converts an item into gold",SpellType.InventoryUtility,5,3,
            new Dictionary<int, int>() { {4296, 1},{4293,5}}, SpellEffects.HighAlchemy,(i=>i is Item item && item.canBeShipped() && item.salePrice(false) > 0),
            "Turn any sellable item into money. Provides 100% of the items shipping bin value. Put an appropriate item in the slot and press the spell icon to cast."),
        new Spell(4,"Area_Humidify","Humidify","Waters the ground around you",SpellType.MapUtility,3,2,
            new Dictionary<int, int>() { {4298, 1},{4293,1},{4292,3}}, SpellEffects.Humidify,
            (tile => tile is HoeDirt hoeLand && (hoeLand.crop == null || !hoeLand.crop.forageCrop.Value || hoeLand.crop.whichForageCrop.Value != "2") && hoeLand.state.Value != 1)),
        new Spell(5,"Area_Cure","Cure Plant","Replants dead crops",SpellType.MapUtility,6,5,
            new Dictionary<int, int>() { {4298, 1},{4294,8}}, SpellEffects.CurePlant, 
            (tile => tile is HoeDirt hoeLand && hoeLand.crop != null && hoeLand.crop.dead.Value)),
        new Spell(6,"Buff_VileVigour","Vile Vigour","Sacrifices a third of your max health to fill your energy",SpellType.Buff,3,6,
            new Dictionary<int, int>() { {4297, 1},{4291,3}}, SpellEffects.VileVigour, 
            (f=> f is Farmer farmer && farmer.stamina < farmer.MaxStamina)),
        new Spell(7,"Buff_PieMake","Bake Pie","Cooks a random recipe that you know using your held ingredients",SpellType.Buff,3,7,
            new Dictionary<int, int>() { {4298, 1},{4293,1},{4292,1}}, SpellEffects.BakePie, 
            (f=> f is Farmer farmer && farmer.cookingRecipes.Length > 0)),
        new Spell(8,"Teleport_Desert","Desert Teleport","NA Teleports you to the desert, if you have access to it",SpellType.Teleport,5,8,
            new Dictionary<int, int>() { {4295, 2},{4294,1},{4293,1}}, SpellEffects.CurePlant, 
            (tile => tile is HoeDirt hoeLand && hoeLand.crop != null && hoeLand.crop.dead.Value)),
        new Spell(9,"Teleport_Ginger","Ginger Island Teleport","NA Teleports you to ginger island, if you have access to it",SpellType.Teleport,7,9,
            new Dictionary<int, int>() { {4295, 2},{4292,2},{4293,2}}, SpellEffects.CurePlant, 
            (tile => tile is HoeDirt hoeLand && hoeLand.crop != null && hoeLand.crop.dead.Value)),
        new Spell(10,"Teleport_Caves","Caves Teleport","NA Teleports you to the pelican town mines",SpellType.Teleport,2,10,
            new Dictionary<int, int>() { {4295, 1},{4291,5}}, SpellEffects.CurePlant, 
            (tile => tile is HoeDirt hoeLand && hoeLand.crop != null && hoeLand.crop.dead.Value)),
        new Spell(11,"Menu_WaterOrb","Charge Water Orb","NA Turns aquamarine into strong slingshot ammo",SpellType.InventoryUtility,4,11,
            new Dictionary<int, int>() { {4297, 3},{4292,5}}, SpellEffects.CurePlant, 
            (tile => tile is HoeDirt hoeLand && hoeLand.crop != null && hoeLand.crop.dead.Value)),
        new Spell(12,"Menu_EarthOrb","Charge Earth Orb","NA Turns emeralds into stronger slingshot ammo",SpellType.InventoryUtility,7,12,
            new Dictionary<int, int>() { {4297, 3},{4294,5}}, SpellEffects.CurePlant, 
            (tile => tile is HoeDirt hoeLand && hoeLand.crop != null && hoeLand.crop.dead.Value)),
        new Spell(13,"Buff_DarkLure","Dark Lure","NA Lures more enemies to you",SpellType.InventoryUtility,6,13,
            new Dictionary<int, int>() { {4296, 2},{4297,2}}, SpellEffects.CurePlant, 
            (tile => tile is HoeDirt hoeLand && hoeLand.crop != null && hoeLand.crop.dead.Value)),
        new CombatSpell(14,"Combat_Wind","Wind Strike","NA A basic air missile",1,14,
            new Dictionary<int, int>() { {4299, 1},{4291,1}}, 10,1,0,Color.White),
        new CombatSpell(15,"Combat_Water","Water Bolt","NA A low level water missile",2,15,
            new Dictionary<int, int>() { {4299, 2},{4291,2},{4292,2}}, 10,1,1,Color.LightBlue),
        new CombatSpell(16,"Combat_Undead","Crumble Undead","NA Hits undead monsters for extra damage",4,16,
            new Dictionary<int, int>() { {4299, 2},{4291,2},{4294,2}}, 10,1,3,Color.White),
        new CombatSpell(17,"Combat_Earth","Earth Blast","NA A medium level earth missile",6,17,
            new Dictionary<int, int>() { {4300, 1},{4291,3},{4294,4}}, 10,1,1,Color.DarkGreen),
        new CombatSpell(18,"Combat_Fire","Fire Wave","NA A high level fire missile",8,18,
            new Dictionary<int, int>() { {4300, 2},{4291,5},{4293,5}}, 10,1,2,Color.OrangeRed),
        new Spell(19,"Buff_Charge","Charge","NA Increases the power of combat spells while active",SpellType.Buff,7,19,
            new Dictionary<int, int>() { {4300, 3},{4291,3},{4293,3}}, SpellEffects.CurePlant, 
            (tile => tile is HoeDirt hoeLand && hoeLand.crop != null && hoeLand.crop.dead.Value)),
        new CombatSpell(20,"Combat_Demonbane","Demonbane","NA Hits undead monsters for a lot of extra damage",9,20,
            new Dictionary<int, int>() { {4300, 2},{4297,2},{4293,8}}, 10,1,3,Color.Purple),
        new CombatSpell(21,"Combat_Blood","Blood Barrage","NA Fires a strong vampiric blood missile",10,21,
            new Dictionary<int, int>() { {4300, 8},{4297,5}}, 10,1,1,Color.Crimson),
        new Spell(22,"Menu_Plank","Plank Make","NA Turns hardwood into wood and vice versa",SpellType.InventoryUtility,3,22,
            new Dictionary<int, int>() { {4300, 2},{4298, 2},{4297,5}}, SpellEffects.CurePlant, 
            (tile => tile is HoeDirt hoeLand && hoeLand.crop != null && hoeLand.crop.dead.Value), "Converts 1 hardwood into 9 wood, or 15 wood into 1 hardwood"),
        new Spell(23,"Buff_Heal","Heal","NA Restores your health in exchange for energy",SpellType.Buff,8,23,
            new Dictionary<int, int>() { {4300, 3},{4291,3},{4293,3}}, SpellEffects.CurePlant, 
            (tile => tile is HoeDirt hoeLand && hoeLand.crop != null && hoeLand.crop.dead.Value)),
    };
    public static void Load(IModHelper helper)
    {
        extraTextures = helper.ModContent.Load<Texture2D>("assets\\modsprites"); 
        animTextures = helper.ModContent.Load<Texture2D>("assets\\spellanimations"); 
        multiplayer = helper.Reflection.GetField<object>(typeof(Game1), "multiplayer").GetValue();
    }

    public static void BroadcastSprite(GameLocation location, TemporaryAnimatedSprite sprite)
    {
        var method = multiplayer.GetType().GetMethod("broadcastSprites", 
            new[] { typeof(GameLocation), typeof(TemporaryAnimatedSprite[]) });
        
        var spriteArray = new TemporaryAnimatedSprite[] { sprite };
        
        method.Invoke(multiplayer, new object[] { location, spriteArray });
    }
    
    public static void GlobalChatMessage(string messageKey, params string[] args)
    {
        var method = multiplayer.GetType().GetMethod("globalChatInfoMessage", 
            new[] { typeof(string), typeof(string[]) });
        
        method.Invoke(multiplayer, new object[] { messageKey, args });
    }
}