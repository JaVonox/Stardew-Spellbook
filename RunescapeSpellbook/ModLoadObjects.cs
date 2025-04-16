using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Objects;
using StardewValley.GameData.Weapons;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Network;
using StardewValley.TerrainFeatures;

namespace RunescapeSpellbook;

public struct ItemDrop
{
    public string itemID;
    public int amount;
    public double chance;

    public int minAmount;
    public int maxAmount;
    public ItemDrop(string itemID, int amount, double chance = 1.0)
    {
        this.itemID = itemID;
        this.amount = amount;
        this.chance = chance;
        
        this.minAmount = amount;
        this.maxAmount = amount;
    }
    
    public ItemDrop(string itemID, int minAmount, int maxAmount, double weight = 1.0) 
    {
        this.itemID = itemID;
        this.amount = minAmount;
        this.minAmount = minAmount;
        this.maxAmount = maxAmount;
        this.chance = weight;
    }
}
public class ModLoadObjects : ObjectData
{
    public int id;

    public ModLoadObjects(int id, string name, string displayName, string description, string type = "Basic", int category = -2)
    {
        this.id = id;
        base.Name = name;
        base.DisplayName = displayName;
        base.Description = description;
        base.Type = type;
        base.Texture = ModEntry.CustomTextureKey;
        base.SpriteIndex = id - 4290;
        base.Category = category;
    }

    public void AppendObject(string CustomTextureKey, IDictionary<string,ObjectData> ObjectsSet)
    {
        ObjectsSet[$"{id}"] = this;
    }
}

public class RunesObjects : ModLoadObjects
{
    public RunesObjects(int id, string name, string displayName, string description, string type = "Basic", int category = 0) : 
        base(id,name,displayName,description,type,category)
    {
        
    }
}

public class SlingshotItem : ModLoadObjects
{
    public SlingshotItem(int id, string name, string displayName, string description, int spriteID, string type = "Basic", int category = 0) : 
        base(id,name,displayName,description,type,category)
    {
        base.SpriteIndex = spriteID;
    }
}

public class TreasureObjects : ModLoadObjects
{
    public TreasureObjects(int id, string name, string displayName, string description, int spriteID,
        List<ItemDrop> itemDrops, string type = "Basic", int category = 0) :
        base(id, name, displayName, description, type, category)
    {
        base.SpriteIndex = spriteID;
        List<ObjectGeodeDropData> objects = new List<ObjectGeodeDropData>();

        double totalWeight = itemDrops.Sum(itemDrop => itemDrop.chance);

        int dropID = 0;
        foreach (ItemDrop item in itemDrops)
        {
            ObjectGeodeDropData geodeItem = new ObjectGeodeDropData();
            geodeItem.Id = dropID.ToString();
            geodeItem.ItemId = item.itemID;

            geodeItem.MinStack = item.minAmount;
            geodeItem.MaxStack = item.maxAmount;
            
            geodeItem.Chance = BalanceItemPercentage(itemDrops,dropID,totalWeight);
            geodeItem.Precedence = 0;
            objects.Add(geodeItem);
            dropID++;
        }
        
        base.GeodeDropsDefaultItems = false;
        base.GeodeDrops = objects;
    }


    public TreasureObjects(int id, string name, string displayName, string description, int spriteID, string type = "Basic", int category = 0) : 
        base(id,name,displayName,description,type,category)
    {
        base.SpriteIndex = spriteID;
        List<ObjectGeodeDropData> objects = new List<ObjectGeodeDropData>();
        
        ObjectGeodeDropData geodeItem = new ObjectGeodeDropData();
        geodeItem.Id = "0";
        geodeItem.ItemId = "4291";
        geodeItem.Chance = 1.0;
        geodeItem.MinStack = 10;
        geodeItem.MaxStack = 20;
        geodeItem.Precedence = 0;
        objects.Add(geodeItem);

        base.GeodeDropsDefaultItems = false;
        base.GeodeDrops = objects;

    }

    /// <summary>
    /// Finds what the new percentage should be for an index in the array to match its expected percentage chance -
    /// assuming that we work sequentially
    /// </summary>
    /// <returns></returns>
    private static double BalanceItemPercentage(List<ItemDrop> items, int calculatedIndex, double totalWeight)
    {
        double desiredChance = (items[calculatedIndex].chance / totalWeight); //The specified chance of this item drop occuring divided by the total chances
        if (calculatedIndex == 0)
        {
            return desiredChance; //If its the first index we always have the desired chance
        }
        else if (calculatedIndex == items.Count - 1)
        {
            return 1; //final item will always have 100% chance
        }

        double divisor = 1;

        for(int i = calculatedIndex - 1; i >= 0;i--)
        {
            divisor *= (1 - BalanceItemPercentage(items,i,totalWeight)); 
        }
        
        return desiredChance / divisor;
    }
}
public class PerkData
{
    public int perkID;
    public string perkName;
    public string perkDisplayName;
    public string perkDescription;
    public string perkDescriptionLine2;
    public PerkData(int perkID, string perkName, string perkDisplayName, string perkDescription, string perkDescriptionLine2 = "")
    {
        this.perkID = perkID;
        this.perkName = perkName;
        this.perkDisplayName = perkDisplayName;
        this.perkDescription = perkDescription;
        this.perkDescriptionLine2 = perkDescriptionLine2;
    }

    public bool HasPerk(Farmer farmer)
    {
        return ModAssets.HasPerk(farmer, this.perkID);
    }
}

public static class ModAssets
{
    public static Texture2D extraTextures; //Includes spells + basic icons
    public static Texture2D animTextures;

    public static PlayerModData localFarmerData;
    
    public const int spellsY = 16;
    public const int spellsSize = 80;
    private static object multiplayer;
    
    public static readonly ModLoadObjects[] modItems = {
        new RunesObjects(4290,"Rune_Blank","Pure Essence","An unimbued rune of extra capability."),
        new RunesObjects(4291,"Rune_Air","Air Rune","One of the 4 basic elemental Runes"),
        new RunesObjects(4292,"Rune_Water","Water Rune","One of the 4 basic elemental Runes"),
        new RunesObjects(4293,"Rune_Fire","Fire Rune","One of the 4 basic elemental Runes"),
        new RunesObjects(4294,"Rune_Earth","Earth Rune","One of the 4 basic elemental Runes"),
        new RunesObjects(4295,"Rune_Law","Law Rune","Used for teleport spells"),
        new RunesObjects(4296,"Rune_Nature","Nature Rune","Used for alchemy spells"),
        new RunesObjects(4297,"Rune_Cosmic","Cosmic Rune","Used for enchant spells"),
        new RunesObjects(4298,"Rune_Astral","Astral Rune","Used for Lunar spells"),
        new RunesObjects(4299,"Rune_Chaos","Chaos Rune","Used for low level combat spells"),
        new RunesObjects(4300,"Rune_Death","Death Rune","Used for high level combat spells"),
        
        
        //TODO fix the icons for these + add spell to spawn them
        new SlingshotItem(4301,"Ammo_Water","Water Orb","Slingshot ammo enchanted with the power of water",30),
        new SlingshotItem(4302,"Ammo_Earth","Earth Orb","Slingshot ammo enchanted with the power of earth",31),
        
        new TreasureObjects(4359,"Treasure_Elemental","Elemental Geode","Contains some elemental Runes",19,
            new List<ItemDrop>()
            {
                new ItemDrop("4291",10,15,2.3),
                new ItemDrop("4291",15,20,0.5),
                new ItemDrop("4292",5,10,2),
                new ItemDrop("4292",15,20,0.5),
                new ItemDrop("4293",5,10,2),
                new ItemDrop("4293",15,20,0.5),
                new ItemDrop("4294",5,10,2),
                new ItemDrop("4294",15,20,0.5),
            }),
        
        new TreasureObjects(4360,"Treasure_Catalytic","Catalytic Geode","Contains some catalytic Runes",20,
            new List<ItemDrop>()
            {
                new ItemDrop("4295",5,10,1.2),
                new ItemDrop("4296",5,10,1.2),
                new ItemDrop("4297",5,10,1),
                new ItemDrop("4298",5,10,1),
                new ItemDrop("4299",5,15,3),
                new ItemDrop("4300",5,15,1.5),
            }),
        
        new TreasureObjects(4361,"Treasure_EasyCasket","Low Level Casket","Contains some magical goodies",21,
            new List<ItemDrop>()
            {
                new ItemDrop("4364",2,4,0.8),
                new ItemDrop("4365",1,3,0.5),
                new ItemDrop("4366",1,3,0.5),
                new ItemDrop("4367",1,3,0.5),
                new ItemDrop("4368",1,3,0.5),
                new ItemDrop("4369",3,3,0.5),
                
                new ItemDrop("4295",5,15,0.5),
                new ItemDrop("4296",5,15,0.5),
                new ItemDrop("4297",5,15,0.5),
                new ItemDrop("4298",5,15,0.5),
                
                new ItemDrop("4300",10,25,1.1),
                new ItemDrop("4359",1,3,1),
                new ItemDrop("4360",2,4,1),
                
                new ItemDrop("4351",1,1,0.8),
                new ItemDrop("4352",1,1,0.3),
                new ItemDrop("4353",1,1,0.3),
                new ItemDrop("4354",1,1,0.3),
                new ItemDrop("4355",1,1,0.3),
                
                new ItemDrop("4362",1,1,0.05),
            }),
        
        new TreasureObjects(4362,"Treasure_HardCasket","High Level Casket","Contains some valuable magical goodies",22,
            new List<ItemDrop>()
            {
                new ItemDrop("4364",2,6,0.8),
                new ItemDrop("4365",2,5,0.5),
                new ItemDrop("4366",2,5,0.5),
                new ItemDrop("4367",2,5,0.5),
                new ItemDrop("4368",2,5,0.5),
                new ItemDrop("4369",2,5,0.5),
                
                new ItemDrop("4295",10,20,0.5),
                new ItemDrop("4296",10,20,0.5),
                new ItemDrop("4297",10,20,0.5),
                new ItemDrop("4298",10,20,0.5),
                
                new ItemDrop("4359",2,4,1),
                new ItemDrop("4360",3,5,1),
                
                new ItemDrop("4352",1,1,0.7),
                new ItemDrop("4353",1,1,0.7),
                new ItemDrop("4354",1,1,0.7),
                new ItemDrop("4355",1,1,0.7),
                new ItemDrop("4356",1,1,0.2),
                new ItemDrop("4363",1,1,0.05),
            }),
        
        new TreasureObjects(4363,"Treasure_BarrowsCasket","Barrows Casket","Contains some very valuable magical goodies",23,
            new List<ItemDrop>()
            {
                new ItemDrop("4356",1,1,2),
                new ItemDrop("4357",1,1,0.4),
                new ItemDrop("4358",1,1,0.4),
                new ItemDrop("4368",3,6,1),
                new ItemDrop("4369",3,6,1),
                new ItemDrop("4360",5,7,1),
            }),
        
        new TreasureObjects(4364,"Treasure_AirPack","Air Rune Pack","A pack containing many air Runes",24,
            new List<ItemDrop>()
            {
                new ItemDrop("4291",10,15,1.5),
                new ItemDrop("4291",20,30,0.5),
                new ItemDrop("4291",40,50,0.25),
            }),
        new TreasureObjects(4365,"Treasure_WaterPack","Water Rune Pack","A pack containing many water Runes",25,
            new List<ItemDrop>()
            {
                new ItemDrop("4292",10,15,1.5),
                new ItemDrop("4292",20,30,0.5),
                new ItemDrop("4292",40,50,0.25),
            }),
        new TreasureObjects(4366,"Treasure_FirePack","Fire Rune Pack","A pack containing many fire Runes",26,
            new List<ItemDrop>()
            {
                new ItemDrop("4293",10,15,1.5),
                new ItemDrop("4293",20,30,0.5),
                new ItemDrop("4293",40,50,0.25),
            }),
        new TreasureObjects(4367,"Treasure_EarthPack","Earth Rune Pack","A pack containing many earth Runes",27,
            new List<ItemDrop>()
            {
                new ItemDrop("4294",10,15,1.5),
                new ItemDrop("4294",20,30,0.5),
                new ItemDrop("4294",40,50,0.25),
            }),
        new TreasureObjects(4368,"Treasure_ChaosPack","Chaos Rune Pack","A pack containing many chaos Runes",28,
            new List<ItemDrop>()
            {
                new ItemDrop("4299",5,15,1.5),
                new ItemDrop("4299",15,20,0.5),
                new ItemDrop("4299",40,50,0.1),
            }),
        new TreasureObjects(4369,"Treasure_DeathPack","Death Rune Pack","A pack containing many death Runes",29,
        new List<ItemDrop>()
        {
            new ItemDrop("4300",5,15,1.5),
            new ItemDrop("4300",15,20,0.5),
            new ItemDrop("4300",40,50,0.1),
        }),
    };
    
    //These are custom melee weapons that use 
    public static readonly StaffWeaponData[] staffWeapons =
    {
        new StaffWeaponData("4351", "Staff_Magic", "Magic Staff", "A magical battlestaff", 5, 10, 11),
        new StaffWeaponData("4352", "Staff_Air", "Staff of Air",
            "A magical battlestaff imbued with air magic. Provides air runes for combat spells.", 20, 30, 12,
            1.2f, 4291),
        new StaffWeaponData("4353", "Staff_Water", "Staff of Water",
            "A magical battlestaff imbued with water magic. Provides water runes for combat spells.", 20, 30, 13,
            1.2f, 4292),
        new StaffWeaponData("4354", "Staff_Earth", "Staff of Earth",
            "A magical battlestaff imbued with earth magic. Provides earth runes for combat spells.", 20, 30, 14,
            1.2f, 4294),
        new StaffWeaponData("4355", "Staff_Fire", "Staff of Fire",
            "A magical battlestaff imbued with fire magic. Provides fire runes for combat spells.", 20, 30, 15,
            1.2f, 4293),
        new StaffWeaponData("4356", "Staff_Ancient", "Ancient Staff", "A magical battlestaff of ancient origin...",
            25, 40, 16,
            1.4f,-1,0,0,0,0.05f),
        new StaffWeaponData("4357", "Staff_Ahrims", "Ahrims Staff", "Ahrim the Blighted's quarterstaff", 30, 45, 17,
            1.6f),
        new StaffWeaponData("4358", "Staff_Bluemoon", "Blue moon spear",
            "An ancient battlestaff that doubles as a spear", 60, 80, 18,
            1.5f)
    };

    //This dictionary provides a quick reference for which weapons provide what rune
    public static Dictionary<int, List<string>> infiniteRuneReferences;
    
    public static readonly Spell[] modSpells = {
        new TeleportSpell(0,"Teleport_Valley","Valley Teleport","Teleports you to Pierre's Store in Pelican Town",0,
            new Dictionary<int, int>() { {4295, 1},{4291,3},{4293,1} },8,"Town", 43, 60,0),
        
        new TeleportSpell(1,"Teleport_Home","Farm Teleport","Teleports you outside your Farm",4,
            new Dictionary<int, int>() { {4295, 1},{4291,1},{4294,1} },10, "BusStop", 19, 23,2),
        
        new InventorySpell(2,"Menu_Superheat","Superheat Item","Smelts ore without a furnace or coal",1,
            new Dictionary<int, int>() { {4296, 1},{4293,4}},15,
            (i=>i is Item item && DataLoader.Machines(Game1.content).GetValueOrDefault("(BC)13").OutputRules.Any(x=>x.Triggers.Any(y=>y.RequiredItemId == item.QualifiedItemId))),
            SpellEffects.SuperheatItem,"Smelt any ores into bars instantly without any coal cost. Put an appropriate item in the slot and press the spell icon to cast.","Superheat",1),
        
        new InventorySpell(3,"Menu_HighAlch","High Level Alchemy","Converts an item into gold",5,
            new Dictionary<int, int>() { {4296, 1},{4293,5}},15,(i=>i is Item item && item.canBeShipped() && item.salePrice(false) > 0),
            SpellEffects.HighAlchemy,"Turn any sellable item into money. Provides 100% of the items shipping bin value. Put an appropriate item in the slot and press the spell icon to cast.","HighAlch",0),
        
        new TilesSpell(4,"Area_Humidify","Humidify","Waters the ground around you",2,
            new Dictionary<int, int>() { {4298, 1},{4293,1},{4292,3}}, 0.3f,SpellEffects.Humidify, 10,"Humidify",
            (tile => tile is HoeDirt hoeLand && (hoeLand.crop == null || !hoeLand.crop.forageCrop.Value || hoeLand.crop.whichForageCrop.Value != "2") && hoeLand.state.Value != 1)),
        
        new TilesSpell(5,"Area_Cure","Cure Plant","Replants dead crops",6,
            new Dictionary<int, int>() { {4298, 1},{4294,8}},0.5f, SpellEffects.CurePlant, 10,"Cure",
            (tile => tile is HoeDirt hoeLand && hoeLand.crop != null && hoeLand.crop.dead.Value)),
        
        new BuffSpell(6,"Buff_VileVigour","Vile Vigour","Sacrifices a third of your max health to fill your energy",3,
            new Dictionary<int, int>() { {4297, 1},{4291,3}},3, (f=> f is Farmer farmer && farmer.stamina < farmer.MaxStamina), SpellEffects.VileVigour,
            "Vile","My energy is already full"),
        
        new BuffSpell(7,"Buff_PieMake","Bake Pie","Cooks a random recipe that you know using your held ingredients",3,
            new Dictionary<int, int>() { {4298, 1},{4293,1},{4292,1}}, 15,
            (f=> f is Farmer farmer && farmer.cookingRecipes.Length > 0), SpellEffects.BakePie, "BakePie","I don't know enough recipes"),
        
        new TeleportSpell(8,"Teleport_Desert","Desert Teleport","Teleports you to the desert, if you have access to it",5,
            new Dictionary<int, int>() { {4295, 2},{4294,1},{4293,1}}, 15,"Desert", 19, 34,2,
            ((farmer => Game1.MasterPlayer.mailReceived.Contains("ccVault")))),
        
        new TeleportSpell(9,"Teleport_Ginger","Ginger Island Teleport","Teleports you to ginger island, if you have access to it",7,
            new Dictionary<int, int>() { {4295, 2},{4292,2},{4293,2}},15, "IslandSouth",21,37,0,
            ((farmer => Game1.MasterPlayer.hasOrWillReceiveMail("willyBoatFixed")))),
        
        new TeleportSpell(10,"Teleport_Caves","Caves Teleport","Teleports you to the pelican town mines",1,
            new Dictionary<int, int>() { {4295, 1},{4291,5}},10, "Mountain",54,7,0, 
            ((farmer => Game1.MasterPlayer.hasOrWillReceiveMail("landslideDone")))),
        
        new InventorySpell(11,"Menu_EnchantSapphire","Enchant Sapphire Bolt","Turns blue rocks and gemstones into strong slingshot ammo",4,
            new Dictionary<int, int>() { {4297, 2},{4292,3}},10,(i => i is Item item && SpellEffects.blueGemsEnchants.ContainsKey(item.ItemId)),SpellEffects.EnchantSapphireBolt,
            "Convert any blue gems or rocks into enchanted ammo for the slingshot","EnchantBolt",2),
        
        new InventorySpell(12,"Menu_EnchantEmerald","Enchant Emerald Bolt","Turns green gemstones into stronger slingshot ammo",7,
            new Dictionary<int, int>() { {4297, 2},{4294,3}},15,(i => i is Item item && SpellEffects.greenGemsEnchants.ContainsKey(item.ItemId)),SpellEffects.EnchantEmeraldBolt,
            "Convert any green gems into enchanted ammo for the slingshot","EnchantBolt",2),
        
        new BuffSpell(13,"Buff_DarkLure","Dark Lure","Summons more enemies, and makes them prioritise you over other farmers for 3 minutes",6,
            new Dictionary<int, int>() { {4296, 2},{4297,2}},10,(f=> f is Farmer farmer && !farmer.hasBuff("430")),SpellEffects.DarkLure, "DarkLure","I'm already luring monsters!"),
        
        new CombatSpell(14,"Combat_Wind","Wind Strike","A basic air missile",0,
            new Dictionary<int, int>() { {4299, 1},{4291,1}}, 1,25,15,0,Color.White,"WindStrike"),
       
        new CombatSpell(15,"Combat_Water","Water Bolt","A low level water missile",2,
            new Dictionary<int, int>() { {4299, 2},{4291,2},{4292,2}},2, 35,16,1,Color.DarkCyan,"WaterBolt"),
        
        new CombatSpell(16,"Combat_Undead","Crumble Undead","Hits undead monsters for extra damage",4,
            new Dictionary<int, int>() { {4299, 2},{4291,2},{4294,2}},4, 30,13,3,Color.Yellow,"CrumbleUndead",SpellEffects.DealUndeadDamage),
        
        new CombatSpell(17,"Combat_Earth","Earth Blast","A medium level earth missile",6,
            new Dictionary<int, int>() { {4300, 1},{4291,3},{4294,4}},4, 60,16,1,Color.DarkGreen,"EarthBlast"),
        
        new CombatSpell(18,"Combat_Fire","Fire Wave","A high level fire missile",8,
            new Dictionary<int, int>() { {4300, 2},{4291,5},{4293,5}},5, 95,15,2,Color.OrangeRed,"FireWave"),
        
        new BuffSpell(19,"Buff_Charge","Charge","Spells cast three projectiles for 30 seconds",7,
            new Dictionary<int, int>() { {4300, 3},{4291,3},{4293,3}},10,(f=> f is Farmer farmer && !farmer.hasBuff("429")),SpellEffects.Charge, "Charge","I'm already charged!"),
        
        new CombatSpell(20,"Combat_Demonbane","Demonbane","Hits undead monsters for a lot of extra damage",9,
            new Dictionary<int, int>() { {4300, 2},{4297,2},{4293,8}},6, 65,13,3,Color.Purple,"CrumbleUndead",SpellEffects.DealDemonbaneDamage),
        
        new CombatSpell(21,"Combat_Blood","Blood Barrage","Fires a strong vampiric blood missile",10,
            new Dictionary<int, int>() { {4300, 8},{4297,5}}, 10,80,15,1,Color.Crimson, "BloodBarrage",SpellEffects.DealVampiricDamage),
        
        new InventorySpell(22,"Menu_Plank","Plank Make","Transmutes wooden items into wood",3,
            new Dictionary<int, int>() { {4298, 1},{4297,1}},5,
            (i => i is Item item && (item.itemId.Value == "388" || 
                                     (CraftingRecipe.craftingRecipes.ContainsKey(item.Name) 
                                      && CraftingRecipe.craftingRecipes[item.Name].Split(' ').ToList() is List<string> recipes 
                                      && ((recipes.IndexOf("388") != -1 && recipes.IndexOf("388") + 1 % 2 != 0) || (recipes.IndexOf("709") != -1 && recipes.IndexOf("709") + 1 % 2 != 0) ) )))
            ,SpellEffects.PlankMake,
            "Breaks down wooden items into wood, and converts 15 wood into 1 hardwood. For recipes that require more than wood, it will only return the wood.","Degrime",3),
    };
    
    public static readonly List<PerkData> perks = new List<PerkData>()
    {
        new PerkData(0,"Sapphire","Sapphire","All teleportation spells are free","Teleportation spells no longer grant experience"),
        new PerkData(1,"Emerald","Emerald","All spells no longer require air runes"),
        new PerkData(2,"Ruby","Ruby","20% chance of non-combat spells taking no runes"),
        new PerkData(3,"Dragonstone","Dragonstone","20% chance of combat spells firing three projectiles for free","Does not stack with charge, charge takes prescedent")
    };

    public static readonly Dictionary<string, List<ItemDrop>> monsterDrops = new Dictionary<string, List<ItemDrop>>()
    {
        { "Green Slime", new List<ItemDrop>(){
            new ItemDrop("4294",3,0.1f),
            new ItemDrop("4291",2,0.1f),
            new ItemDrop("4292",2,0.1f),
            new ItemDrop("4293",2,0.1f),
            new ItemDrop("4367",1,0.02f),
            new ItemDrop("4295", 1, 0.05f),
            new ItemDrop("4351",1,1f),
        } },
        { "Dust Spirit", new List<ItemDrop>(){
            new ItemDrop("4291", 1, 0.1f), 
            new ItemDrop("4293",2,0.1f)
        } },
        { "Bat", new List<ItemDrop>(){
            new ItemDrop("4291", 1, 0.05f),
            new ItemDrop("4364",1,0.02f),
            new ItemDrop("4299", 1, 0.05f)
        } },
        { "Frost Bat", new List<ItemDrop>(){
            new ItemDrop("4364",1,0.03f),
            new ItemDrop("4292", 3, 0.1f),
            new ItemDrop("4365", 1, 0.02f),
            new ItemDrop("4299", 2, 0.05f),
            new ItemDrop("4361", 1, 0.02f),
        } },
        { "Lava Bat", new List<ItemDrop>(){
            new ItemDrop("4364",1,0.03f),
            new ItemDrop("4293", 3, 0.2f),
            new ItemDrop("4366", 1, 0.02f),
            new ItemDrop("4300", 2, 0.05f),
            new ItemDrop("4362", 1, 0.02f),
        } },
        { "Stone Golem", new List<ItemDrop>(){
            new ItemDrop("4367", 1, 0.1f),
            new ItemDrop("4296", 5, 0.1f),
            new ItemDrop("4359", 1, 0.1f)
        } },
        { "Wilderness Golem", new List<ItemDrop>(){
            new ItemDrop("4299", 3, 0.05f),
            new ItemDrop("4300", 1, 0.05f)
        } },
        { "Grub", new List<ItemDrop>(){
            new ItemDrop("4296", 1, 0.1f),
            new ItemDrop("4291", 2, 0.1f)
        } },
        { "Fly", new List<ItemDrop>(){
            new ItemDrop("4291", 3, 0.3f),
            new ItemDrop("4364", 1, 0.05f),
        } },
        { "Frost Jelly", new List<ItemDrop>(){
            new ItemDrop("4292",4,0.1f),
            new ItemDrop("4295", 2, 0.1f),
            new ItemDrop("4359", 1, 0.05f)
        } },
        { "Shadow Guy", new List<ItemDrop>(){
            new ItemDrop("4295",2,0.2f),
            new ItemDrop("4297", 3, 0.2f),
            new ItemDrop("4300", 3, 0.3f),
            new ItemDrop("4360", 1, 0.05f),
            new ItemDrop("4361", 1, 0.05f)
        } },
        { "Ghost", new List<ItemDrop>(){
            new ItemDrop("4295",2,0.2f),
            new ItemDrop("4299", 3, 0.2f),
            new ItemDrop("4296",3,0.2f),
            new ItemDrop("4297",3,0.2f),
            new ItemDrop("4300", 3, 0.1f),
            new ItemDrop("4368", 1, 0.05f),
            new ItemDrop("4360", 1, 0.05f)
        } },
        { "Duggy", new List<ItemDrop>(){
            new ItemDrop("4359",2,0.2f),
            new ItemDrop("4294",5,0.01f),
        } },
        { "Rock Crab", new List<ItemDrop>(){
            new ItemDrop("4359",2,0.2f),
            new ItemDrop("4365",1,0.1f),
        } },
        { "Truffle Crab", new List<ItemDrop>(){
            new ItemDrop("4296",3,0.02f)
        } },
        { "Squid Kid", new List<ItemDrop>(){
            new ItemDrop("4360",2,0.1f),
            new ItemDrop("4359",4,0.1f),
            new ItemDrop("4361",2,0.1f),
        } },
        { "Skeleton", new List<ItemDrop>(){
            new ItemDrop("4368",1,0.05f),
            new ItemDrop("4369",1,0.02f),
            new ItemDrop("4361",1,0.06f),
        } },
        { "Metal Head", new List<ItemDrop>(){
            new ItemDrop("4362",1,0.05f),
        } },
        { "Shadow Brute", new List<ItemDrop>(){
            new ItemDrop("4361",1,0.05f),
            new ItemDrop("4362",1,0.05f),
            new ItemDrop("4295",5,0.1f),
            new ItemDrop("4296",5,0.1f),
            new ItemDrop("4368",2,0.1f),
        } },
        { "Shadow Shaman", new List<ItemDrop>(){
            new ItemDrop("4361",1,0.05f),
            new ItemDrop("4362",1,0.05f),
            new ItemDrop("4297",5,0.1f),
            new ItemDrop("4298",5,0.1f),
            new ItemDrop("4369",2,0.1f),
        } },
    };

    public static bool CheckHasPerkByName(Farmer farmer,string perkName)
    {
        PerkData? perk = perks.FirstOrDefault(x => x.perkName == perkName);
        return perk == null ? false : perk.HasPerk(farmer);
    }
    public static void Load(IModHelper helper)
    {
        extraTextures = helper.ModContent.Load<Texture2D>("assets\\modsprites"); 
        animTextures = helper.ModContent.Load<Texture2D>("assets\\spellanimations"); 
        multiplayer = helper.Reflection.GetField<object>(typeof(Game1), "multiplayer").GetValue();
        localFarmerData = new PlayerModData();

        infiniteRuneReferences = new Dictionary<int, List<string>>();
        //Generate the lookup dictionary for determining what weapons give infinite values for each rune
        foreach (StaffWeaponData weapon in staffWeapons.Where(x=>x.providesRune != -1))
        {
            if (!infiniteRuneReferences.ContainsKey(weapon.providesRune))
            {
                infiniteRuneReferences.Add(weapon.providesRune,new List<string>(){weapon.id});
            }
            else
            {
                infiniteRuneReferences[weapon.providesRune].Add(weapon.id);
            }
        }
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

    public static List<Farmer> GetFarmers()
    {
        List<Farmer> farmers = new List<Farmer>();
        farmers.Add(Game1.player);
        foreach (Farmer value in Game1.otherFarmers.Values)
        {
            farmers.Add(value);
        }
        
        return farmers;
    }

    public static int GetFarmerMagicLevel(Farmer farmer)
    {
        int level = -1;
        int.TryParse(farmer.modData["TofuMagicLevel"],out level);
        return level;
    }
    
    public static int GetFarmerExperience(Farmer farmer)
    {
        int experience = -1;
        int.TryParse(farmer.modData["TofuMagicExperience"],out experience);
        return experience;
    }

    public static void IncrementMagicExperience(Farmer farmer, int gainedExperience)
    {
        int experience = GetFarmerExperience(farmer);
        
        if (experience != -1 && experience <= Farmer.getBaseExperienceForLevel(10)) //If our exp should still be tracked then increment it
        {
            int newTotalExperience = (experience + gainedExperience);
            farmer.modData["TofuMagicExperience"] = newTotalExperience.ToString();
            int currentLevel = GetFarmerMagicLevel(farmer);
            int expTilNextLevel = Farmer.getBaseExperienceForLevel(currentLevel + 1);

            int messageTier = 0;
            if (newTotalExperience >= expTilNextLevel)
            {
                while (currentLevel + 1 <= 10 && newTotalExperience >= expTilNextLevel)
                {
                    currentLevel++;
                    expTilNextLevel = Farmer.getBaseExperienceForLevel(currentLevel + 1);
                    
                    if (currentLevel > 0 && currentLevel % 5 == 0)
                    {
                        messageTier = 2;
                    }
                    else
                    {
                        messageTier = messageTier != 2 ? 1 : 0;
                    }
                }
                
                farmer.modData["TofuMagicLevel"] = (currentLevel).ToString();
            }

            switch (messageTier)
            {
                case 1:
                    Game1.addHUDMessage(new HUDMessage("I feel like my magical power has reached new heights", 2));
                    break;
                case 2:
                    Game1.addHUDMessage(new HUDMessage("I feel like I can choose a new magical profession", 2));
                    break;
            }
        }
    }
    public static List<int> PerksAssigned(Farmer farmer)
    {
        List<int> perkIDs = new List<int>();
        int id1 = -1;
        int.TryParse(farmer.modData["TofuMagicProfession1"],out id1);
        if (id1 != -1)
        {
            perkIDs.Add(id1);
        }
        
        int id2 = -1;
        int.TryParse(farmer.modData["TofuMagicProfession2"],out id2);
        if (id2 != -1)
        {
            perkIDs.Add(id2);
        }
        
        return perkIDs;
    }

    public static bool HasPerk(Farmer farmer, int perkID)
    {
        int id1 = -1;
        int.TryParse(farmer.modData["TofuMagicProfession1"],out id1);
        int id2 = -1;
        int.TryParse(farmer.modData["TofuMagicProfession2"],out id2);
        
        return (perkID == id1 || perkID == id2);
    }

    public static bool GrantPerk(Farmer farmer, int perkID)
    {
        int id1 = -1;
        int.TryParse(farmer.modData["TofuMagicProfession1"],out id1);
        int id2 = -1;
        int.TryParse(farmer.modData["TofuMagicProfession2"],out id2);

        if (id1 == perkID || id2 == perkID)
        {
            return false;
        }
        
        bool successfulAssignment = false;
        if (id1 == -1)
        {
            farmer.modData["TofuMagicProfession1"] = perkID.ToString();
            successfulAssignment = true;
        }
        else if (id2 == -1)
        {
            farmer.modData["TofuMagicProfession2"] = perkID.ToString();
            successfulAssignment = true;
        }
        
        return successfulAssignment;
    }
}

public class PlayerModData
{
    public int selectedSpellID;
    public PlayerModData()
    {
        selectedSpellID = -1;
    }
    public void FirstGameTick()
    {
        selectedSpellID = -1;
    }
    
}