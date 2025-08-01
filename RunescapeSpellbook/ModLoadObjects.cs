﻿using HarmonyLib;
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
    public int itemID;
    public int amount;
    public double chance;

    public int minAmount;
    public int maxAmount;
    public ItemDrop(int itemID, int amount, double chance = 1.0)
    {
        this.itemID = itemID;
        this.amount = amount;
        this.chance = chance;
        
        this.minAmount = amount;
        this.maxAmount = amount;
    }
    public ItemDrop(int itemID, int minAmount, int maxAmount, double weight = 1.0) 
    {
        this.itemID = itemID;
        this.amount = minAmount;
        this.minAmount = minAmount;
        this.maxAmount = maxAmount;
        this.chance = weight;
    }
}

public enum PrefType
{
    Hate,
    Dislike,
    Neutral,
    Like,
    Love
}
public class ModLoadObjects : ObjectData
{
    public int id;
    public Dictionary<string, PrefType>? characterPreferences;
    public ModLoadObjects(int id, string name, string displayName, string description, Dictionary<string, PrefType>? characterPreferences, string type = "Basic", int category = -2)
    {
        this.id = id;
        base.Name = name;
        base.DisplayName = displayName;
        base.Description = description;
        base.Type = type;
        base.Texture = "Mods.RunescapeSpellbook.Assets.modsprites";
        base.SpriteIndex = id - 4290;
        base.Category = category;
        base.ExcludeFromRandomSale = true;
        this.characterPreferences = characterPreferences ?? new Dictionary<string, PrefType>();
        base.Price = 1;
    }

    public void AppendObject(string CustomTextureKey, IDictionary<string,ObjectData> ObjectsSet)
    {
        ObjectsSet[$"{id}"] = this;
    }
}

public class RunesObjects : ModLoadObjects
{
    public RunesObjects(int id, string name, string displayName, string description,Dictionary<string, PrefType>? characterPreferences = null) : 
        base(id,name,displayName,description,characterPreferences,"Basic",0)
    {
        base.Price = 2;
    }
    
}

public class SlingshotItem : ModLoadObjects
{
    public SlingshotItem(int id, string name, string displayName, string description, int spriteID, Dictionary<string, PrefType>? characterPreferences = null) : 
        base(id,name,displayName,description,characterPreferences,"Basic",0)
    {
        base.SpriteIndex = spriteID;
    }
}

public class TreasureObjects : ModLoadObjects
{
    public TreasureObjects(int id, string name, string displayName, string description, int spriteID,
        List<ItemDrop> itemDrops, int sellprice = 35, Dictionary<string, PrefType>? characterPreferences = null) :
        base(id, name, displayName, description,characterPreferences, "Basic", 0)
    {
        base.SpriteIndex = spriteID;
        List<ObjectGeodeDropData> objects = new List<ObjectGeodeDropData>();

        double totalWeight = itemDrops.Sum(itemDrop => itemDrop.chance);

        int dropID = 0;
        foreach (ItemDrop item in itemDrops)
        {
            ObjectGeodeDropData geodeItem = new ObjectGeodeDropData();
            geodeItem.Id = dropID.ToString();
            geodeItem.ItemId = item.itemID.ToString();

            geodeItem.MinStack = item.minAmount;
            geodeItem.MaxStack = item.maxAmount;
            
            geodeItem.Chance = BalanceItemPercentage(itemDrops,dropID,totalWeight);
            geodeItem.Precedence = 0;
            objects.Add(geodeItem);
            dropID++;
        }
        
        base.GeodeDropsDefaultItems = false;
        base.GeodeDrops = objects;
        base.Price = sellprice;
    }


    public TreasureObjects(int id, string name, string displayName, string description, int spriteID, Dictionary<string, PrefType>? characterPreferences) : 
        base(id,name,displayName,description,characterPreferences,"Basic",0)
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
        base.Price = 35;
    }

    /// <summary>
    /// Finds what the new percentage should be for an index in the array to match its expected percentage chance -
    /// assuming that we work sequentially
    /// </summary>
    /// <returns></returns>
    protected static double BalanceItemPercentage(List<ItemDrop> items, int calculatedIndex, double totalWeight)
    {
        double desiredChance = (items[calculatedIndex].chance / totalWeight); //The specified chance of this item drop occuring divided by the total chances
        if (calculatedIndex == 0)
        {
            return desiredChance; //If its the first index we always have the desired chance
        }
        
        if (calculatedIndex == items.Count - 1)
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

public class PackObject : TreasureObjects
{
    public int packItem;
    public PackObject(int id, string name, string displayName, string description, int spriteID, int packItem) :
        base(id, name, displayName, description, spriteID,null)
    {
        this.packItem = packItem;
        List<ItemDrop> itemDrops = new List<ItemDrop>()
        {
            new ItemDrop(packItem, 7, 12, 1.5),
            new ItemDrop(packItem, 13, 23, 0.5),
            new ItemDrop(packItem, 25, 35, 0.25),
        };
        
        base.SpriteIndex = spriteID;
        List<ObjectGeodeDropData> objects = new List<ObjectGeodeDropData>();

        double totalWeight = itemDrops.Sum(itemDrop => itemDrop.chance);

        int dropID = 0;
        foreach (ItemDrop item in itemDrops)
        {
            ObjectGeodeDropData geodeItem = new ObjectGeodeDropData();
            geodeItem.Id = dropID.ToString();
            geodeItem.ItemId = item.itemID.ToString();

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

    public static PlayerLocalData localFarmerData;
    
    public const int spellsY = 16;
    public const int spellsSize = 80;
    private static object multiplayer;
    
    public const int animFrames = 4; 
    
    public static Dictionary<int,ModLoadObjects> modItems = new Dictionary<int,ModLoadObjects>{
        {4290,new RunesObjects(4290,"Rune_Spellbook","Spellbook","Debug object.")},
        {4291,new RunesObjects(4291,"Rune_Air","Air Rune","One of the 4 basic elemental Runes")},
        {4292,new RunesObjects(4292,"Rune_Water","Water Rune","One of the 4 basic elemental Runes",
            new Dictionary<string, PrefType>(){{"Willy",PrefType.Neutral},{"Elliott",PrefType.Neutral}})},
        {4293,new RunesObjects(4293,"Rune_Fire","Fire Rune","One of the 4 basic elemental Runes",
            new Dictionary<string, PrefType>(){{"Sam",PrefType.Neutral},{"Vincent",PrefType.Neutral}})},
        {4294,new RunesObjects(4294,"Rune_Earth","Earth Rune","One of the 4 basic elemental Runes",
            new Dictionary<string, PrefType>(){{"Dwarf",PrefType.Neutral},{"Demetrius",PrefType.Neutral}})},
        {4295,new RunesObjects(4295,"Rune_Law","Law Rune","Used for teleport spells",
            new Dictionary<string, PrefType>(){{"Wizard",PrefType.Like}})},
        {4296,new RunesObjects(4296,"Rune_Nature","Nature Rune","Used for alchemy spells",
            new Dictionary<string, PrefType>(){{"Leo",PrefType.Neutral},{"Linus",PrefType.Neutral},{"Wizard",PrefType.Neutral}})},
        {4297,new RunesObjects(4297,"Rune_Cosmic","Cosmic Rune","Used for enchant spells",
            new Dictionary<string, PrefType>(){{"Emily",PrefType.Neutral},{"Maru",PrefType.Like},{"Wizard",PrefType.Neutral}})},
        {4298,new RunesObjects(4298,"Rune_Astral","Astral Rune","Used for Lunar spells",
            new Dictionary<string, PrefType>(){{"Emily",PrefType.Like},{"Maru",PrefType.Neutral},{"Wizard",PrefType.Neutral}})},
        {4299,new RunesObjects(4299,"Rune_Chaos","Chaos Rune","Used for low level combat spells",
            new Dictionary<string, PrefType>(){{"Emily",PrefType.Hate},{"Kent",PrefType.Hate},{"Wizard",PrefType.Neutral}})},
        {4300,new RunesObjects(4300,"Rune_Death","Death Rune","Used for high level combat spells",
            new Dictionary<string, PrefType>(){{"Sebastian",PrefType.Like},{"Emily",PrefType.Hate},{"George",PrefType.Hate},{"Evelyn",PrefType.Hate},{"Wizard",PrefType.Neutral}})},

        {4301,new SlingshotItem(4301,"Ammo_Water","Water Orb","Slingshot ammo enchanted with the power of water",30)},
        {4302,new SlingshotItem(4302,"Ammo_Earth","Earth Orb","Slingshot ammo enchanted with the power of earth",31)},
        
        {4359,new TreasureObjects(4359,"Treasure_Elemental","Elemental Geode","Contains some elemental Runes. Clint might be able to open it.",19,
            new List<ItemDrop>()
            {
                new ItemDrop(4291, 10, 12, 1),
                new ItemDrop(4291, 13, 23, 0.6),
                new ItemDrop(4291, 25, 35, 0.35),
                
                new ItemDrop(4292, 10, 12, 1),
                new ItemDrop(4292, 13, 23, 0.5),
                new ItemDrop(4292, 25, 35, 0.25),
                
                new ItemDrop(4293, 10, 12, 1),
                new ItemDrop(4293, 13, 23, 0.5),
                new ItemDrop(4293, 25, 35, 0.25),
                
                new ItemDrop(4294, 10, 12, 1),
                new ItemDrop(4294, 13, 23, 0.5),
                new ItemDrop(4294, 25, 35, 0.25),
            })},
        
        {4360,new TreasureObjects(4360,"Treasure_Catalytic","Catalytic Geode","Contains some catalytic Runes. Clint might be able to open it.",20,
            new List<ItemDrop>()
            {
                new ItemDrop(4295, 5, 12, 1),
                
                new ItemDrop(4296, 7, 12, 1),
                
                new ItemDrop(4297, 7, 12, 1),
                
                new ItemDrop(4298, 7, 12, 1),
                
                new ItemDrop(4299, 10, 15, 1),
                new ItemDrop(4299, 16, 23, 0.5),
                new ItemDrop(4299, 25, 35, 0.25),
                
                new ItemDrop(4300, 10, 15, 0.5),
                new ItemDrop(4300, 16, 23, 0.25),
                new ItemDrop(4300, 25, 35, 0.1),
            })},
        
        {4361, new TreasureObjects(4361,"Treasure_EasyCasket","Low Level Casket","Contains some magical goodies. Clint might be able to open it.",21,
            new List<ItemDrop>()
            {
                new ItemDrop(4364,3,6,0.8),
                new ItemDrop(4365,3,5,0.5),
                new ItemDrop(4366,3,5,0.5),
                new ItemDrop(4367,3,5,0.5),
                new ItemDrop(4368,3,6,0.5),
                new ItemDrop(4369,1,3,0.5),
                
                new ItemDrop(4295,10,15,0.5),
                new ItemDrop(4296,10,15,0.5),
                new ItemDrop(4297,10,15,0.5),
                new ItemDrop(4298,10,15,0.5),
                
                new ItemDrop(4359,5,7,1),
                new ItemDrop(4360,3,7,1),
                
                new ItemDrop(4351,1,1,0.8),
                new ItemDrop(4352,1,1,0.3),
                new ItemDrop(4353,1,1,0.3),
                new ItemDrop(4354,1,1,0.3),
                new ItemDrop(4355,1,1,0.3),
                
                new ItemDrop(4362,1,1,0.05),
            },500,new Dictionary<string, PrefType>(){{"Abigail",PrefType.Like}})},
        
        {4362,new TreasureObjects(4362,"Treasure_HardCasket","High Level Casket","Contains some valuable magical goodies. Clint might be able to open it.",22,
            new List<ItemDrop>()
            {
                new ItemDrop(4364,5,9,0.8),
                new ItemDrop(4365,5,9,0.5),
                new ItemDrop(4366,5,9,0.5),
                new ItemDrop(4367,5,9,0.5),
                new ItemDrop(4368,5,9,0.5),
                new ItemDrop(4369,3,7,0.5),
                
                new ItemDrop(4295,20,30,0.5),
                new ItemDrop(4296,20,30,0.5),
                new ItemDrop(4297,20,30,0.5),
                new ItemDrop(4298,20,30,0.5),
                
                new ItemDrop(4359,8,13,1),
                new ItemDrop(4360,5,10,1),
                
                new ItemDrop(4352,1,1,0.7),
                new ItemDrop(4353,1,1,0.7),
                new ItemDrop(4354,1,1,0.7),
                new ItemDrop(4355,1,1,0.7),
                new ItemDrop(4356,1,1,0.4),
                new ItemDrop(4363,1,1,0.1),
            },1500,new Dictionary<string, PrefType>(){{"Abigail",PrefType.Love}})},
        
        {4363,new TreasureObjects(4363,"Treasure_BarrowsCasket","Barrows Casket","Contains some very valuable magical goodies. Clint might be able to open it.",23,
            new List<ItemDrop>()
            {
                new ItemDrop(4356,1,1,2),
                new ItemDrop(4357,1,1,0.4),
                new ItemDrop(4358,1,1,0.4),
                new ItemDrop(4368,5,10,1),
                new ItemDrop(4369,5,10,1),
                new ItemDrop(4360,10,20,1),
            },2500,new Dictionary<string, PrefType>(){{"Abigail",PrefType.Love}})},
        
        {4364,new PackObject(4364,"Treasure_AirPack","Air Rune Pack","A pack containing many air Runes. Clint might be able to open it.",24,4291)},
        {4365,new PackObject(4365,"Treasure_WaterPack","Water Rune Pack","A pack containing many water Runes. Clint might be able to open it.",25,4292)},
        {4366,new PackObject(4366,"Treasure_FirePack","Fire Rune Pack","A pack containing many fire Runes. Clint might be able to open it.",26,4293)},
        {4367,new PackObject(4367,"Treasure_EarthPack","Earth Rune Pack","A pack containing many earth Runes. Clint might be able to open it.",27,4294)},
        {4368,new PackObject(4368,"Treasure_ChaosPack","Chaos Rune Pack","A pack containing many chaos Runes. Clint might be able to open it.",28,4299)},
        {4369,new PackObject(4369,"Treasure_DeathPack","Death Rune Pack","A pack containing many death Runes. Clint might be able to open it.",29,4300)},
    };
    
    //These are custom melee weapons that use 
    public static readonly StaffWeaponData[] staffWeapons =
    {
        new StaffWeaponData(4351, "Staff_Magic", "Magic Staff", "A magical battlestaff", 5, 10, 11),
        new StaffWeaponData(4352, "Staff_Air", "Staff of Air",
            "A magical battlestaff imbued with air magic. Provides air runes for combat spells.", 20, 30, 12,
            1.2f, 4291),
        new StaffWeaponData(4353, "Staff_Water", "Staff of Water",
            "A magical battlestaff imbued with water magic. Provides water runes for combat spells.", 20, 30, 13,
            1.2f, 4292),
        new StaffWeaponData(4354, "Staff_Earth", "Staff of Earth",
            "A magical battlestaff imbued with earth magic. Provides earth runes for combat spells.", 20, 30, 14,
            1.2f, 4294),
        new StaffWeaponData(4355, "Staff_Fire", "Staff of Fire",
            "A magical battlestaff imbued with fire magic. Provides fire runes for combat spells.", 20, 30, 15,
            1.2f, 4293),
        new StaffWeaponData(4356, "Staff_Ancient", "Ancient Staff", "A magical battlestaff of ancient origin...",
            25, 40, 16,
            1.4f,-1,0,0,0,0.05f),
        new StaffWeaponData(4357, "Staff_Ahrims", "Ahrims Staff", "Ahrim the Blighted's quarterstaff", 30, 45, 17,
            1.6f),
        new StaffWeaponData(4358, "Staff_Bluemoon", "Blue moon spear",
            "An ancient battlestaff that doubles as a spear", 60, 80, 18,
            1.5f)
    };

    //This dictionary provides a quick reference for which weapons provide what rune
    public static Dictionary<int, List<string>> infiniteRuneReferences;
    
    public static readonly Spell[] modSpells = {
        new TeleportSpell(0,"Teleport_Valley","Valley Teleport","Teleports you to Pierre's Store in Pelican Town",0,
            new Dictionary<int, int>() { {4295, 1},{4291,3},{4292,2} },8,"Town", 43, 60,0),
        
        new TeleportSpell(1,"Teleport_Home","Farm Teleport","Teleports you outside your Farm",4,
            new Dictionary<int, int>() { {4295, 1},{4291,3},{4294,3} },10, "BusStop", 19, 23,2),
        
        new InventorySpell(2,"Menu_Superheat","Superheat Item","Smelts ore without a furnace or coal",1,
            new Dictionary<int, int>() { {4296, 1},{4293,4}},15,
            (i=>i is Item item && DataLoader.Machines(Game1.content).GetValueOrDefault("(BC)13").OutputRules.Any(x=>x.Triggers.Any(y=>y.RequiredItemId == item.QualifiedItemId))),
            SpellEffects.SuperheatItem,"Smelt any ores into bars instantly without any coal cost. Put an appropriate item in the slot and press the spell icon to cast.",1,"Superheat"),
        
        new InventorySpell(3,"Menu_HighAlch","High Level Alchemy","Converts an item into 1.5x its sell price",5,
            new Dictionary<int, int>() { {4296, 1},{4293,5}},15,(i=>i is Item item && item.canBeShipped() && item.salePrice(false) > 0),
            SpellEffects.HighAlchemy,"Turn any sellable item into money. Provides 150% of the items value. Put an appropriate item in the slot and press the spell icon to cast.",0,"HighAlch"),
        
        new TilesSpell(4,"Area_Humidify","Humidify","Waters the ground around you",0,
            new Dictionary<int, int>() { {4298, 1},{4293,1},{4292,3}}, 0.3f,SpellEffects.Humidify, 10,5,"Humidify",
            (tile => tile is HoeDirt hoeLand && (hoeLand.crop == null || !hoeLand.crop.forageCrop.Value || hoeLand.crop.whichForageCrop.Value != "2") && hoeLand.state.Value != 1)),
        
        new TilesSpell(5,"Area_Cure","Cure Plant","Replants dead crops",6,
            new Dictionary<int, int>() { {4298, 1},{4294,8}},0.5f, SpellEffects.CurePlant, 10,6,"Cure",
            (tile => tile is HoeDirt hoeLand && hoeLand.crop != null && hoeLand.crop.dead.Value)),
        
        new BuffSpell(6,"Buff_VileVigour","Vile Vigour","Sacrifices a third of your max health to fill your energy",3,
            new Dictionary<int, int>() { {4297, 1},{4291,3}},3, (f=> f is Farmer farmer && farmer.stamina < farmer.MaxStamina), SpellEffects.VileVigour,
            7,"Vile","My energy is already full"),
        
        new BuffSpell(7,"Buff_PieMake","Bake Pie","Cooks a random recipe that you know using your held ingredients",3,
            new Dictionary<int, int>() { {4298, 1},{4293,1},{4292,1}}, 15,
            (f=> f is Farmer farmer && farmer.cookingRecipes.Length > 0), SpellEffects.BakePie, 8,"BakePie","I don't know enough recipes"),
        
        new TeleportSpell(8,"Teleport_Desert","Desert Teleport","Teleports you to the desert, if you have access to it",5,
            new Dictionary<int, int>() { {4295, 2},{4294,5},{4293,5}}, 15,"Desert", 19, 34,2,
            ((farmer => Game1.MasterPlayer.mailReceived.Contains("ccVault")))),
        
        new TeleportSpell(9,"Teleport_Ginger","Ginger Island Teleport","Teleports you to ginger island, if you have access to it",7,
            new Dictionary<int, int>() { {4295, 2},{4292,5},{4293,5}},15, "IslandSouth",21,37,0,
            ((farmer => Game1.MasterPlayer.hasOrWillReceiveMail("willyBoatFixed")))),
        
        new TeleportSpell(10,"Teleport_Caves","Caves Teleport","Teleports you to the pelican town mines",1,
            new Dictionary<int, int>() { {4295, 1},{4291,5}},10, "Mountain",54,7,0, 
            ((farmer => Game1.MasterPlayer.hasOrWillReceiveMail("landslideDone")))),
        
        new InventorySpell(11,"Menu_EnchantSapphire","Enchant Sapphire Bolt","Turns blue rocks and gemstones into strong slingshot ammo",4,
            new Dictionary<int, int>() { {4297, 2},{4292,3}},10,(i => i is Item item && SpellEffects.blueGemsEnchants.ContainsKey(item.ItemId)),SpellEffects.EnchantSapphireBolt,
            "Convert any blue gems or rocks into enchanted ammo for the slingshot",2,"EnchantBolt"),
        
        new InventorySpell(12,"Menu_EnchantEmerald","Enchant Emerald Bolt","Turns green gemstones into stronger slingshot ammo",7,
            new Dictionary<int, int>() { {4297, 2},{4294,3}},15,(i => i is Item item && SpellEffects.greenGemsEnchants.ContainsKey(item.ItemId)),SpellEffects.EnchantEmeraldBolt,
            "Convert any green gems into enchanted ammo for the slingshot",2,"EnchantBolt"),
        
        new BuffSpell(13,"Buff_DarkLure","Dark Lure","Summons more enemies, and makes them prioritise you over other farmers for 3 minutes",6,
            new Dictionary<int, int>() { {4296, 2},{4297,2}},10,(f=> f is Farmer farmer && !farmer.hasBuff("430")),SpellEffects.DarkLure, 9,"DarkLure","I'm already luring monsters!"),
        
        new CombatSpell(14,"Combat_Wind","Wind Strike","A basic air missile",0,
            new Dictionary<int, int>() { {4299, 1},{4291,1}}, 1,40,15,0,Color.White,"WindStrike"),
       
        new CombatSpell(15,"Combat_Water","Water Bolt","A low level water missile",2,
            new Dictionary<int, int>() { {4299, 2},{4291,2},{4292,2}},2, 70,16,1,Color.DarkCyan,"WaterBolt"),
        
        new CombatSpell(16,"Combat_Undead","Crumble Undead","Hits undead monsters for extra damage",4,
            new Dictionary<int, int>() { {4299, 2},{4291,2},{4294,2}},4, 60,13,3,Color.Yellow,"CrumbleUndead",SpellEffects.DealUndeadDamage),
        
        new CombatSpell(17,"Combat_Earth","Earth Blast","A medium level earth missile",6,
            new Dictionary<int, int>() { {4300, 1},{4291,3},{4294,3}},4, 90,16,1,Color.DarkGreen,"EarthBlast"),
        
        new CombatSpell(18,"Combat_Fire","Fire Wave","A high level fire missile",8,
            new Dictionary<int, int>() { {4300, 2},{4291,3},{4293,4}},5, 120,15,2,Color.OrangeRed,"FireWave"),
        
        new BuffSpell(19,"Buff_Charge","Charge","Spells cast three projectiles for 60 seconds",7,
            new Dictionary<int, int>() { {4300, 3},{4291,3},{4293,3}},10,(f=> f is Farmer farmer && !farmer.hasBuff("429")),SpellEffects.Charge, 10,"Charge","I'm already charged!"),
        
        new CombatSpell(20,"Combat_Demonbane","Demonbane","Hits undead monsters for a lot of extra damage",9,
            new Dictionary<int, int>() { {4300, 2},{4298,2},{4293,4}},6, 100,13,3,Color.Purple,"CrumbleUndead",SpellEffects.DealDemonbaneDamage),
        
        new CombatSpell(21,"Combat_Blood","Blood Barrage","Fires a strong vampiric blood missile",10,
            new Dictionary<int, int>() { {4300, 4},{4297,3}}, 10,100,15,1,Color.Crimson, "BloodBarrage",SpellEffects.DealVampiricDamage),
        
        new InventorySpell(22,"Menu_Plank","Plank Make","Turns wood into hardwood and vice versa and uncrafts wooden items into wood",3,
            new Dictionary<int, int>() { {4298, 1},{4297,1}},5,
            (i => i is Item item && (item.itemId.Value == "388" || item.itemId.Value == "709" || 
                                     (CraftingRecipe.craftingRecipes.ContainsKey(item.Name) 
                                      && CraftingRecipe.craftingRecipes[item.Name].Split(' ').ToList() is List<string> recipes 
                                      && ((recipes.IndexOf("388") != -1 && (recipes.IndexOf("388") + 1) % 2 != 0) || (recipes.IndexOf("709") != -1 && recipes.IndexOf("709") + 1 % 2 != 0) ) )))
            ,SpellEffects.PlankMake,
            "Breaks down wooden items into wood, and converts 15 wood into 1 hardwood and vice versa. For recipes that require more than wood, it will only return the wood.",3,"Degrime"),
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
        //Caves (Basic)
        { "Big Slime", new List<ItemDrop>(){ 
            new ItemDrop(4295,2,0.08f),
            new ItemDrop(4296,2,0.1f),
            new ItemDrop(4359,1,0.02f),
        } },
        { "Prismatic Slime", new List<ItemDrop>(){
            new ItemDrop(4295,4,0.9f),
            new ItemDrop(4298,5,0.9f),
        } },
        { "Green Slime", new List<ItemDrop>(){
            new ItemDrop(4295,2,0.08f),
            new ItemDrop(4359,1,0.02f),
        } },
        { "Fly", new List<ItemDrop>(){
            new ItemDrop(4364,1,0.2f),
            new ItemDrop(4368,1,0.05f),
        } },
        { "Rock Crab", new List<ItemDrop>(){
            new ItemDrop(4359,1,0.3f),
        } },
        { "Grub", new List<ItemDrop>(){
            new ItemDrop(4296,2,0.1f),
        } },
        { "Bug", new List<ItemDrop>(){
            new ItemDrop(4368,1,0.08f),
            new ItemDrop(4359,1,0.2f),
        } },
        { "Bat", new List<ItemDrop>(){
            new ItemDrop(4364,1,0.15f),
            new ItemDrop(4368,1,0.25f),
        } },
        { "Stone Golem", new List<ItemDrop>(){
            new ItemDrop(4367,1,0.2f),
            new ItemDrop(4360,1,0.1f),
            new ItemDrop(4361,1,0.04f),
        } },
        { "Dust Spirit", new List<ItemDrop>(){
            new ItemDrop(4364,1,0.04f),
            new ItemDrop(4296,1,0.02f),
        } },
        { "Frost Bat", new List<ItemDrop>(){
            new ItemDrop(4364,1,0.05f),
            new ItemDrop(4365,1,0.15f),
            new ItemDrop(4368,1,0.3f),
        } },
        { "Ghost", new List<ItemDrop>(){
            new ItemDrop(4295,3,0.1f),
            new ItemDrop(4297,2,0.05f),
            new ItemDrop(4360,1,0.15f),
        } },
        { "Frost Jelly", new List<ItemDrop>(){
            new ItemDrop(4365,1,0.1f),
            new ItemDrop(4359,1,0.2f),
        } },
        { "Skeleton", new List<ItemDrop>(){
            new ItemDrop(4368,2,0.3f),
            new ItemDrop(4369,1,0.02f),
            new ItemDrop(4360,1,0.05f),
            new ItemDrop(4361,1,0.08f),
        } },
        { "Lava Bat", new List<ItemDrop>(){
            new ItemDrop(4364,2,0.15f),
            new ItemDrop(4366,2,0.15f),
        } },
        { "Lava Crab", new List<ItemDrop>(){
            new ItemDrop(4365,1,0.15f),
            new ItemDrop(4366,1,0.3f),
            new ItemDrop(4359,2,0.3f),
        } },
        { "Shadow Shaman", new List<ItemDrop>(){
            new ItemDrop(4296,3,0.2f),
            new ItemDrop(4298,2,0.2f),
            new ItemDrop(4360,2,0.2f),
        } },
        { "Metal Head", new List<ItemDrop>(){
            new ItemDrop(4367,2,0.3f),
            new ItemDrop(4361,1,0.1f),
        } },
        { "Shadow Brute", new List<ItemDrop>(){
            new ItemDrop(4364,2,0.1f),
            new ItemDrop(4368,2,0.3f),
            new ItemDrop(4360,1,0.1f),
        } },
        { "Squid Kid", new List<ItemDrop>(){
            new ItemDrop(4364,3,0.2f),
            new ItemDrop(4297,2,0.2f),
            new ItemDrop(4359,2,0.2f),
        } }, //Skull Cavern 
        { "Sludge", new List<ItemDrop>(){
            new ItemDrop(4295,2,0.2f),
            new ItemDrop(4359,1,0.2f),
        } },
        { "Serpent", new List<ItemDrop>(){
            new ItemDrop(4364,2,0.25f),
            new ItemDrop(4297,6,0.1f),
            new ItemDrop(4361,1,0.1f),
            new ItemDrop(4368,2,0.1f),
        } },
        { "Carbon Ghost", new List<ItemDrop>(){
            new ItemDrop(4295,4,0.3f),
            new ItemDrop(4298,4,0.2f),
            new ItemDrop(4369,2,0.1f),
        } },
        { "Iridium Crab", new List<ItemDrop>(){
            new ItemDrop(4365,5,0.6f),
            new ItemDrop(4359,6,0.6f),
            new ItemDrop(4360,3,0.4f),
        } },
        { "Pepper Rex", new List<ItemDrop>(){
            new ItemDrop(4366,3,1f),
            new ItemDrop(4296,3,0.5f),
            new ItemDrop(4361,1,0.5f),
        } },
        { "Mummy", new List<ItemDrop>(){
            new ItemDrop(4367,2,0.2f),
            new ItemDrop(4295,3,0.3f),
            new ItemDrop(4368,3,0.3f),
            new ItemDrop(4369,1,0.2f),
            new ItemDrop(4362,1,0.1f),
        } },
        { "Iridium Bat", new List<ItemDrop>(){
            new ItemDrop(4364,3,0.5f),
            new ItemDrop(4369,2,0.2f),
            new ItemDrop(4362,1,0.2f),
        } },
        { "Haunted Skull", new List<ItemDrop>(){ //Quarry Mine
            new ItemDrop(4297,3,0.4f),
            new ItemDrop(4298,3,0.3f),
            new ItemDrop(4361,1,0.05f),
            new ItemDrop(4362,1,0.02f),
        } },
        { "Hot Head", new List<ItemDrop>(){ //Ginger Island/Volcano
            new ItemDrop(4366,2,0.3f),
            new ItemDrop(4369,3,0.2f),
            new ItemDrop(4360,2,0.2f),
        } },
        { "Tiger Slime", new List<ItemDrop>(){
            new ItemDrop(4365,2,0.1f),
            new ItemDrop(4367,1,0.2f),
            new ItemDrop(4296,5,0.3f),
            new ItemDrop(4368,3,0.3f),
            new ItemDrop(4369,2,0.1f),
        } },
        { "Magma Sprite", new List<ItemDrop>(){
            new ItemDrop(4364,3,0.2f),
            new ItemDrop(4366,2,0.3f),
        } },
        { "Dwarvish Sentry", new List<ItemDrop>(){
            new ItemDrop(4295,4,0.24f),
            new ItemDrop(4297,10,0.2f),
            new ItemDrop(4369,3,0.3f),
            new ItemDrop(4363,1,0.05f),
        } },
        { "Magma Duggy", new List<ItemDrop>(){
            new ItemDrop(4366,2,0.3f),
            new ItemDrop(4359,5,0.3f),
            new ItemDrop(4363,1,0.05f),
        } },
        { "Magma Sparker", new List<ItemDrop>(){
            new ItemDrop(4366,2,0.3f),
        } },
        { "False Magma Cap", new List<ItemDrop>(){
            new ItemDrop(4367,1,0.2f),
            new ItemDrop(4297,10,0.2f),
        } },
    };

    //Mail to be loaded into the game
    public static Dictionary<string, string> loadableMail = new Dictionary<string, string>()
    {
        {
            "RSSpellMailGet",
            "Dear @,^^I had forgotten one last thing about runic magic. Combat spells require a focus. In layman's terms, a battlestaff." +
            "^I've included one with this letter, and warned the mailcarrier of the consequences if you do not receive it in one piece. " +
            "^^   -M. Rasmodius, Wizard[letterbg 2]" +
            "%item object 4351 1 %%" +
            "[#]Wizard's Battlestaff Gift"
        },
        {
            "summer_15_1",
            "@,^Have you come across some strange packages in the mines lately? They seem to be full of those weird painted rocks that Emily likes." +
            "^^They're pretty hard to open, but my geode hammer seems to do the trick. If you find any, swing by and I'll help you open it" +
            "^^   -Clint^^P.S I've included some samples with this letter" +
            "%item object 4364 3 %%" +
            "[#]Clint's Pack Opening Service"
        },
        {
            "summer_10_2",
            "Ahoy @,^This was floating around in the ocean so I fished it up, some people have no respect for the seas." +
            "^^It seems like something ya might get some use out of, it'd make some fine firewood!" +
            "^^   -Willy" +
            "%item object 4362 1 %%" +
            "[#]Willy's Casket"
        },
        {
            "summer_1_3",
            "@,^I sent some of these to Emily as an anonymous gift but came in yesterday and sold them to my shop.^^She said the design made her uncomfortable." +
            "^^Maybe you'll get something out of them." +
            "^^   -Clint" +
            "%item object 4300 60 %%" +
            "[#]Clint's Terrible Gift"
        },
        {
            "spring_9_2",
            "@,^An old friend gave me some of these, but I don't have enough space to keep all of them." +
            "^^I hope you'll think of the great outdoors when you use them." +
            "^^   -Linus" +
            "%item object 4296 40 %%" +
            "[#]Linus' Nature Stones"
        },
        {
            "fall_26_3",
            "Coco,^^Beef Soup" +
            "^^   -Tofu" +
            "%item object 4293 150 %%" +
            "[#]Letter For Someone Else"
        }
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
        localFarmerData = new PlayerLocalData();

        infiniteRuneReferences = new Dictionary<int, List<string>>();
        //Generate the lookup dictionary for determining what weapons give infinite values for each rune
        foreach (StaffWeaponData weapon in staffWeapons.Where(x=>x.providesRune != -1))
        {
            if (!infiniteRuneReferences.ContainsKey(weapon.providesRune))
            {
                infiniteRuneReferences.Add(weapon.providesRune,new List<string>(){weapon.id.ToString()});
            }
            else
            {
                infiniteRuneReferences[weapon.providesRune].Add(weapon.id.ToString());
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

    public static bool HasMagic(Farmer farmer)
    {
        int hasMagic = 0;
        int.TryParse(farmer.modData["HasUnlockedMagic"],out hasMagic);
        if (hasMagic == 1)
        {
            return true;
        }
        
        if (farmer.eventsSeen.Contains("RS.0"))
        {
            farmer.modData["HasUnlockedMagic"] = "1";
            return true;
        }
        return false;
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
                Game1.player.playNearbySoundLocal("RunescapeSpellbook.MagicLevel");
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

public class PlayerLocalData
{
    public int selectedSpellID;
    public PlayerLocalData()
    {
        selectedSpellID = -1;
    }
    public void Reset()
    {
        selectedSpellID = -1;
    }
    
}