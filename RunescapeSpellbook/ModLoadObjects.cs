using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.GameData.FishPonds;
using StardewValley.GameData.Locations;
using StardewValley.GameData.Objects;
using StardewValley.GameData.Shops;
using StardewValley.Objects;
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
    public RunesObjects(int id, string name, string displayName, string description,int category,Dictionary<string, PrefType>? characterPreferences = null) : 
        base(id,name,displayName,description,characterPreferences,"Basic",category)
    {
        base.Price = 2;
    }
    
}

public class SlingshotItem : ModLoadObjects
{
    public SlingshotItem(int id, string name, string displayName, string description, int spriteID, Dictionary<string, PrefType>? characterPreferences = null) : 
        base(id,name,displayName,description,characterPreferences,"Basic",-2)
    {
        base.SpriteIndex = spriteID;
    }
}

public class TreasureObjects : ModLoadObjects
{
    public TreasureObjects(int id, string name, string displayName, string description, int spriteID,
        List<ItemDrop> itemDrops, int sellprice = 35, Dictionary<string, PrefType>? characterPreferences = null) :
        base(id, name, displayName, description,characterPreferences, "Basic", -28)
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
        base(id,name,displayName,description,characterPreferences,"Basic",-28)
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
public class FishObject : ModLoadObjects
{
    private int dartChance;
    private int minDayTime;
    private int maxDayTime;
    private List<Season> seasons;
    private string weather;
    private List<string> locations;
    private int catchChance;
    private int minFishingLevel;
    private Color fishTypeWaterColour;
    private Dictionary<int, List<string>> populationGates;
    private Dictionary<int, ItemDrop> rewards;
    private int spawnTime;
    
    public FishObject(int id, string name,string displayName, string description, int spriteID, int dartChance, int minDayTime, 
        int maxDayTime, List<Season> seasons, string weather, List<string> locations, int catchChance, int minFishingLevel, int price, int edibility, int spawnTime,
        Color waterColour, string roeColour, Dictionary<int, List<string>> populationGates, Dictionary<int, ItemDrop> rewards)
        : base(id, name, displayName, description,null,"Basic",-4)
    {
        this.dartChance = dartChance;
        this.minDayTime = minDayTime;
        this.maxDayTime = maxDayTime;
        this.seasons = seasons;
        this.weather = weather;
        this.locations = locations;
        this.catchChance = catchChance;
        this.minFishingLevel = minFishingLevel;
        base.Price = price;
        base.SpriteIndex = spriteID;
        base.ExcludeFromFishingCollection = true;
        base.Edibility = edibility;
        base.ContextTags = new List<string>() {$"item_{name}",roeColour};
        
        this.fishTypeWaterColour = waterColour;
        this.populationGates = populationGates;
        this.rewards = rewards;
        this.spawnTime = spawnTime;
    }

    public void AppendFishData(IDictionary<string,string> fishDict)
    {
        string seasonsText = "";
        foreach (Season season in seasons)
        {
            if (seasonsText != "") { seasonsText += " ";}
            seasonsText += season.ToString();
        }

        fishDict.Add($"{this.id}", $"{this.Name}/{this.dartChance}/dart/1/36/{this.minDayTime} {this.maxDayTime}/{seasonsText}/{this.weather}/690 .4 685 .1/2/.{this.catchChance}/.5/{this.minFishingLevel}/false");
    }

    public void AppendPondData(IList<FishPondData> pondData)
    {
        FishPondData newPondData = new FishPondData();
        newPondData.Id = this.Name.ToString();
        newPondData.RequiredTags = new List<string>(){$"item_{base.Name}"};
        newPondData.PopulationGates = this.populationGates;
        newPondData.ProducedItems = new List<FishPondReward>();
        newPondData.SpawnTime = spawnTime;
            
        foreach (KeyValuePair<int,ItemDrop> pondDrop in rewards)
        {
            FishPondReward pondReward = new FishPondReward();
            pondReward.RequiredPopulation = pondDrop.Key;
            pondReward.Chance = (float)pondDrop.Value.chance;
            pondReward.MinStack = pondDrop.Value.minAmount;
            pondReward.MaxStack = pondDrop.Value.maxAmount;
            pondReward.ItemId = $"{pondDrop.Value.itemID}";
            newPondData.ProducedItems.Add(pondReward);
        }
        
        FishPondWaterColor waterColour = new FishPondWaterColor();
        waterColour.Color = $"{fishTypeWaterColour.R} {fishTypeWaterColour.G} {fishTypeWaterColour.B}";
        waterColour.MinPopulation = 0;
        waterColour.MinUnlockedPopulationGate = 0;
        
        newPondData.WaterColor = new List<FishPondWaterColor>() { waterColour };
        pondData.Add(newPondData);
    }
    public void AppendLocationData(IDictionary<string, LocationData> locationSet)
    {
        foreach (string loc in this.locations)
        {
            SpawnFishData fishData = new SpawnFishData();
            fishData.ItemId = $"(O){base.id}";
            fishData.Chance = float.Parse($"0.{this.catchChance}"); 
            fishData.MinFishingLevel = this.minFishingLevel;
            fishData.MinDistanceFromShore = 2;
            fishData.MaxDistanceFromShore = -1;
            
            string seasonsText = "";
            foreach (Season season in seasons)
            {
                if (seasonsText != "") { seasonsText += " ";}
                seasonsText += season.ToString();
            }
            
            fishData.Condition = $"SEASON {seasonsText}";
            
            locationSet[loc].Fish.Add(fishData);
        }
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

public class ShopListings
{
    public readonly ShopItemData itemData;
    public readonly int insertIndex;
    public ShopListings(string tradeID, string qualifiedID, int price,int newInsertIndex = 0, int minStack = -1, int maxstack = -1, string condition = "",
        int toolUpgradeLevel = -1)
    {
        itemData = new ShopItemData();
        itemData.Id = qualifiedID;
        itemData.ItemId = qualifiedID;
        itemData.Price = price;
        itemData.MinStack = minStack;
        itemData.MaxStack = maxstack;
        itemData.Condition = condition == "" ? null : condition;
        itemData.ToolUpgradeLevel = toolUpgradeLevel;
        insertIndex = newInsertIndex;
    }

    public ShopListings(string tradeID, string qualifiedID,string tradeItemID, int tradeAmount,int newInsertIndex = 0, int minStack = -1, int maxStack = -1, string condition = "")
    {
        itemData = new ShopItemData();
        itemData.Id = tradeID;
        itemData.ItemId = qualifiedID;
        itemData.Price = -1;
        itemData.MinStack = minStack;
        itemData.MaxStack = maxStack;
        itemData.Condition = condition == "" ? null : condition;
        itemData.ToolUpgradeLevel = -1;
        itemData.TradeItemId = tradeItemID;
        itemData.TradeItemAmount = tradeAmount;
        insertIndex = newInsertIndex;
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
        {4290,new RunesObjects(4290,"Rune_Spellbook","Spellbook","Debug object.",-999)},
        {4291,new RunesObjects(4291,"Rune_Air","Air Rune","One of the 4 basic elemental Runes",-429)},
        {4292,new RunesObjects(4292,"Rune_Water","Water Rune","One of the 4 basic elemental Runes",-429,
            new Dictionary<string, PrefType>(){{"Willy",PrefType.Neutral},{"Elliott",PrefType.Neutral}})},
        {4293,new RunesObjects(4293,"Rune_Fire","Fire Rune","One of the 4 basic elemental Runes",-429,
            new Dictionary<string, PrefType>(){{"Sam",PrefType.Neutral},{"Vincent",PrefType.Neutral}})},
        {4294,new RunesObjects(4294,"Rune_Earth","Earth Rune","One of the 4 basic elemental Runes",-429,
            new Dictionary<string, PrefType>(){{"Dwarf",PrefType.Neutral},{"Demetrius",PrefType.Neutral}})},
        {4295,new RunesObjects(4295,"Rune_Law","Law Rune","Used for teleport spells",-431,
            new Dictionary<string, PrefType>(){{"Wizard",PrefType.Like}})},
        {4296,new RunesObjects(4296,"Rune_Nature","Nature Rune","Used for alchemy spells",-431,
            new Dictionary<string, PrefType>(){{"Leo",PrefType.Neutral},{"Linus",PrefType.Neutral},{"Wizard",PrefType.Neutral}})},
        {4297,new RunesObjects(4297,"Rune_Cosmic","Cosmic Rune","Used for enchant spells",-431,
            new Dictionary<string, PrefType>(){{"Emily",PrefType.Neutral},{"Maru",PrefType.Like},{"Wizard",PrefType.Neutral}})},
        {4298,new RunesObjects(4298,"Rune_Astral","Astral Rune","Used for Lunar spells",-431,
            new Dictionary<string, PrefType>(){{"Emily",PrefType.Like},{"Maru",PrefType.Neutral},{"Wizard",PrefType.Neutral}})},
        {4299,new RunesObjects(4299,"Rune_Chaos","Chaos Rune","Used for low level combat spells",-430,
            new Dictionary<string, PrefType>(){{"Emily",PrefType.Hate},{"Kent",PrefType.Hate},{"Wizard",PrefType.Neutral}})},
        {4300,new RunesObjects(4300,"Rune_Death","Death Rune","Used for high level combat spells",-430,
            new Dictionary<string, PrefType>(){{"Sebastian",PrefType.Like},{"Emily",PrefType.Hate},{"George",PrefType.Hate},{"Evelyn",PrefType.Hate},{"Wizard",PrefType.Neutral}})},

        {4301,new SlingshotItem(4301,"Ammo_Fire","Fire Orb","Enchanted ammo that burns enemies in a radius around a hit enemy. Fire cannot finish off enemies.",30)},
        {4302,new SlingshotItem(4302,"Ammo_Earth","Earth Orb","Enchanted ammo that explodes and poisons enemies in a radius around a hit enemy. Poison cannot finish off enemies.",31)},
        
        {4359,new TreasureObjects(4359,"Treasure_Elemental","Elemental Geode","Contains some elemental Runes. A blacksmith might be able to open it.",19,
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
            },40)},
        
        {4360,new TreasureObjects(4360,"Treasure_Catalytic","Catalytic Geode","Contains some catalytic Runes. A blacksmith might be able to open it.",20,
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
            },70)},
        
        {4361, new TreasureObjects(4361,"Treasure_EasyCasket","Low Level Casket","Contains some magical goodies. A blacksmith might be able to open it.",21,
            new List<ItemDrop>()
            {
                new ItemDrop(4359,5,10,0.5),
                new ItemDrop(4360,5,10,0.5),
                
                new ItemDrop(4352,1,1,0.3),
                new ItemDrop(4353,1,1,0.3),
                new ItemDrop(4354,1,1,0.3),
                new ItemDrop(4355,1,1,0.3),
                
                new ItemDrop(4362,1,1,0.05),
            },200,new Dictionary<string, PrefType>(){{"Abigail",PrefType.Like}})},
        
        {4362,new TreasureObjects(4362,"Treasure_HardCasket","High Level Casket","Contains some valuable magical goodies. A blacksmith might be able to open it.",22,
            new List<ItemDrop>()
            {
                new ItemDrop(4359,10,15,0.5),
                new ItemDrop(4360,10,15,0.5),
                
                new ItemDrop(4352,1,1,0.7),
                new ItemDrop(4353,1,1,0.7),
                new ItemDrop(4354,1,1,0.7),
                new ItemDrop(4355,1,1,0.7),
                new ItemDrop(4356,1,1,0.5),
                new ItemDrop(4363,1,1,0.1),
            },500,new Dictionary<string, PrefType>(){{"Abigail",PrefType.Love}})},
        
        {4363,new TreasureObjects(4363,"Treasure_BarrowsCasket","Barrows Casket","Contains some very valuable magical goodies. A blacksmith might be able to open it.",23,
            new List<ItemDrop>()
            {
                new ItemDrop(4356,1,1,1),
                new ItemDrop(4357,1,1,0.5),
                new ItemDrop(4358,1,1,0.5),
            },1000,new Dictionary<string, PrefType>(){{"Abigail",PrefType.Love}})},
        
        {4364,new PackObject(4364,"Treasure_AirPack","Air Rune Pack","A pack containing many air Runes. A blacksmith might be able to open it.",24,4291)},
        {4365,new PackObject(4365,"Treasure_WaterPack","Water Rune Pack","A pack containing many water Runes. A blacksmith might be able to open it.",25,4292)},
        {4366,new PackObject(4366,"Treasure_FirePack","Fire Rune Pack","A pack containing many fire Runes. A blacksmith might be able to open it.",26,4293)},
        {4367,new PackObject(4367,"Treasure_EarthPack","Earth Rune Pack","A pack containing many earth Runes. A blacksmith might be able to open it.",27,4294)},
        {4368,new PackObject(4368,"Treasure_ChaosPack","Chaos Rune Pack","A pack containing many chaos Runes. A blacksmith might be able to open it.",28,4299)},
        {4369,new PackObject(4369,"Treasure_DeathPack","Death Rune Pack","A pack containing many death Runes. A blacksmith might be able to open it.",29,4300)},
        
        {4370,new FishObject(4370,"Fish_Karam","Karambwanji","A small brightly coloured tropical fish. Traditionally associated with elemental magic",32,45,600,1800,new List<Season>{Season.Spring,Season.Summer},"sunny",new List<string>()
            { "Beach" },6,2,20,5,2,Color.Cyan,"color_sea_green",
            new Dictionary<int, List<string>>()
            {
                {5,
                    new List<string>()
                    {
                        "152 5 10","395","684 10 20","766 10 20"
                    }
                },
                {8,
                    new List<string>()
                    {
                        "88 3 5","62 1 3","787"
                    }
                    
                }
            }, new Dictionary<int, ItemDrop>()
            {
                {9,new ItemDrop(4361,1,1,0.05)},
                {1,new ItemDrop(4292,10,16,0.5)},
                {2,new ItemDrop(4291,10,16,0.3)},
                {5,new ItemDrop(4294,10,15,0.3)},
                {6,new ItemDrop(4293,10,15,0.2)},
                {0,new ItemDrop(812,1,1,1.0)}
            }
            )},
        {4371,new FishObject(4371,"Fish_Monk","Monkfish","An anglerfish known for its toothy smile. Traditionally associated with combat magic",33,60,1600,2300,new List<Season>{Season.Fall,Season.Winter,Season.Spring},"rainy",new List<string>()
            { "Beach" },4,5,60,50,3,Color.NavajoWhite,"color_sand",
            new Dictionary<int, List<string>>()
            {
                {5,
                    new List<string>()
                    {
                        "198","227","228"
                    }
                },
                {8,
                    new List<string>()
                    {
                        "213","242","728","787","4361"
                    }
                    
                }
            }, new Dictionary<int, ItemDrop>()
            {
                {9,new ItemDrop(4362,1,1,0.05)},
                {7,new ItemDrop(4300,10,30,0.4)},
                {3,new ItemDrop(4299,10,30,0.5)},
                {0,new ItemDrop(812,1,1,1.0)}
            }
        )},
        {4372,new FishObject(4372,"Fish_Manta","Manta Ray","A large and intelligent fish that feeds on plankton. Traditionally associated with catalytic magic",34,75,1200,1800,new List<Season>{Season.Summer},"sunny",new List<string>()
                { "Beach" },6,6,100,70,4,Color.RoyalBlue,"color_black",
            new Dictionary<int, List<string>>()
            {
                {5,
                    new List<string>()
                    {
                        "346 10","456"
                    }
                },
                {8,
                    new List<string>()
                    {
                        "4359 10","832","4361"
                    }
                    
                }
            }, new Dictionary<int, ItemDrop>()
            {
                {2,new ItemDrop(4295,3,5,0.5)},
                {6,new ItemDrop(4297,3,5,0.3)},
                {5,new ItemDrop(4298,3,5,0.3)},
                {0,new ItemDrop(4296,3,5,1.0)},
            }
        )}
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
        new StaffWeaponData(4358, "Staff_Bluemoon", "Blue Moon Spear",
            "An ancient battlestaff that doubles as a spear", 70, 90, 18,
            1.5f)
    };

    //This dictionary provides a quick reference for which weapons provide what rune
    public static Dictionary<int, List<string>> infiniteRuneReferences;
    
    public static readonly Spell[] modSpells = {
        new TeleportSpell(0,"Teleport_Valley","Valley Teleport","Teleports you to Pierre's Store in Pelican Town",0,
            new Dictionary<int, int>() { {4295, 1},{4291,3},{4292,2} },8,"Town", 43, 60,0),
        
        new TeleportSpell(1,"Teleport_Home","Farm Teleport","Teleports you outside the main house on your farm",4,
            new Dictionary<int, int>() { {4295, 1},{4291,3},{4294,3} },10, "FarmHouse"),
        
        new InventorySpell(2,"Menu_Superheat","Superheat Item","Smelts ore without a furnace or coal, or burns wood into coal at a discount",1,
            new Dictionary<int, int>() { {4296, 1},{4293,4}},10,
            (i=>i is Item item && (item.QualifiedItemId == "(O)388" || DataLoader.Machines(Game1.content).GetValueOrDefault("(BC)13").OutputRules.Any(x=>x.Triggers.Any(y=>y.RequiredItemId == item.QualifiedItemId)))),
            SpellEffects.SuperheatItem,"Smelt any ores into bars instantly without any coal cost, or smelt wood into coal. Put an appropriate item in the slot and press the spell icon to cast.",1,"Superheat"),
        
        new InventorySpell(3,"Menu_HighAlch","High Level Alchemy","Converts an item into 1.5x its sell price",5,
            new Dictionary<int, int>() { {4296, 1},{4293,5}},15,(i=>i is Item item && item.canBeShipped() && item.salePrice(false) > 0),
            SpellEffects.HighAlchemy,"Turn any sellable item into money. Provides 150% of the items value. Put an appropriate item in the slot and press the spell icon to cast.",0,"HighAlch"),
        
        new TilesSpell(4,"Area_Humidify","Humidify","Waters the ground around you",1,
            new Dictionary<int, int>() { {4298, 1},{4293,1},{4292,3}}, 0.4f,SpellEffects.Humidify, 10,5,"Humidify",
            (tile => tile is HoeDirt hoeLand && (hoeLand.crop == null || !hoeLand.crop.forageCrop.Value || hoeLand.crop.whichForageCrop.Value != "2") && hoeLand.state.Value != 1)),
        
        new TilesSpell(5,"Area_Cure","Cure Plant","Replants dead crops",6,
            new Dictionary<int, int>() { {4298, 1},{4294,8}},0.6f, SpellEffects.CurePlant, 10,6,"Cure",
            (tile => tile is HoeDirt hoeLand && hoeLand.crop != null && hoeLand.crop.dead.Value)),
        
        new BuffSpell(6,"Buff_VileVigour","Vile Vigour","Sacrifices a third of your max health to fill your energy",3,
            new Dictionary<int, int>() { {4297, 1},{4291,3}},5, (f=> f is Farmer farmer && farmer.stamina < farmer.MaxStamina), SpellEffects.VileVigour,
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
        
        new TeleportSpell(10,"Teleport_Caves","Caves Teleport","Teleports you to the pelican town mines",2,
            new Dictionary<int, int>() { {4295, 1},{4291,5}},10, "Mountain",54,7,0, 
            ((farmer => Game1.MasterPlayer.hasOrWillReceiveMail("landslideDone")))),
        
        new InventorySpell(11,"Menu_EnchantRuby","Enchant Ruby Bolt","Convert any red or orange stones into fiery ammo",4,
            new Dictionary<int, int>() { {4297, 1},{4293,3}},10,(i => i is Item item && SpellEffects.redGemsEnchants.ContainsKey(item.ItemId)),SpellEffects.EnchantRubyBolts,
            "Convert any red gems or rocks into fiery ammo for the slingshot. On hitting an enemy, fire spreads to nearby enemies. Fire cannot finish off enemies.",2,"EnchantBolt"),
        
        new InventorySpell(12,"Menu_EnchantEmerald","Enchant Emerald Bolt","Convert any green stones into explosive poisonous ammo",8,
            new Dictionary<int, int>() { {4297, 2},{4294,3}},15,(i => i is Item item && SpellEffects.greenGemsEnchants.ContainsKey(item.ItemId)),SpellEffects.EnchantEmeraldBolt,
            "Convert any green gems into explosive poisonous ammo for the slingshot. On hitting an enemy, poison spreads to nearby enemies. Poison cannot finish off enemies.",2,"EnchantBolt"),
        
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
            new Dictionary<int, int>() { {4298, 1},{4297,1}},10,
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
        new PerkData(3,"Dragonstone","Dragonstone","20% chance of combat spells firing extra projectiles","Does not stack with charge, charge takes precedent")
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

    //Items to be put in shops
    public static Dictionary<string, List<ShopListings>> loadableShops = new Dictionary<string, List<ShopListings>>()
    {
        {"AdventureShop", new List<ShopListings>()
        {
            new ShopListings("Marlon_Battlestaff","(W)4351",2000,2,-1,-1,"PLAYER_HAS_SEEN_EVENT Current RS.0")
        }},
        {"DesertTrade", new List<ShopListings>()
        {
            new ShopListings("Desert_AirRunes","(O)4291","(O)60",1,4,40,40,"PLAYER_HAS_SEEN_EVENT Current RS.0")
        }}
    };
    
    /// <summary>
    /// mail + notes to load into the game
    /// <remarks>bool: true is mail, false is secret note</remarks>
    /// </summary>
    public static Dictionary<string, Tuple<bool,string>> loadableText = new Dictionary<string, Tuple<bool,string>>()
    {
        {
            "RSSpellMailGet",
            new Tuple<bool, string>(true,"Dear @,^^I had forgotten one last thing about runic magic. Combat spells require a focus. In layman's terms, a battlestaff." +
           "^I've included one with this letter, and warned the mailcarrier of the consequences if you do not receive it in one piece. " +
           "^^   -M. Rasmodius, Wizard[letterbg 2]" +
           "%item object 4351 1 %%" +
           "[#]Wizard's Battlestaff Gift")
        },
        {
            "summer_15_1",
            new Tuple<bool, string>(true,"@,^Have you come across some strange packages in the mines lately? They seem to be full of those weird painted rocks that Emily likes." +
             "^^They're pretty hard to open, but my geode hammer seems to do the trick. If you find any, swing by and I'll help you open it" +
             "^^   -Clint^^P.S I've included some samples with this letter" +
             "%item object 4364 3 %%" +
             "[#]Clint's Pack Opening Service")
        },
        {
            "summer_10_2",
            new Tuple<bool, string>(true,"Ahoy @,^This was floating around in the ocean so I fished it up, some people have no respect for the seas." +
             "^^It seems like something ya might get some use out of, it'd make some fine firewood!" +
             "^^   -Willy" +
             "%item object 4362 1 %%" +
             "[#]Willy's Casket")
        },
        {
            "summer_1_3",
            new Tuple<bool, string>(true,"@,^I sent some of these to Emily as an anonymous gift but came in yesterday and sold them to my shop.^^She said the design made her uncomfortable." +
             "^^Maybe you'll get something out of them." +
             "^^   -Clint" +
             "%item object 4300 60 %%" +
             "[#]Clint's Terrible Gift")
        },
        {
            "spring_9_2",
            new Tuple<bool, string>(true,"@,^An old friend gave me some of these, but I don't have enough space to keep all of them." +
             "^^I hope you'll think of the great outdoors when you use them." +
             "^^   -Linus" +
             "%item object 4296 40 %%" +
             "[#]Linus' Nature Stones")
        },
        {
            "fall_27_2",
            new Tuple<bool, string>(true,"Coco,^^Beef Soup" +
             "^^   -Tofu" +
             "%item object 4362 1 %%" +
             "[#]Letter For Someone Else")
        },
        {
            "419", //Hints for catalytic magic
            new Tuple<bool, string>(false,"In a past life, the men of the desert practiced runic magic." +
                                          "^^Their Flesh inherited their strength." +
                                          "^^Their Souls inherited their wisdom." +
                                          "^^Their Visage, sealed within the crypt of death, inherited the light of the stars." +
                                          "^^Their Shadows, those who escaped the jaws of the ancient beasts, stole away the secrets of the world.")
        },
        {
            "429", //Hints for elemental magic
            new Tuple<bool, string>(false,"Once, the great druid brought balance to the world." +
                                          "^^As he slept, the world splintered, and the spirits became restless." +
                                          "^^The spirits sought those with those whom they shared affinity." +
                                          "^^The great snakes of the desert were granted mastery of the winds." +
                                          "^^The spiders of the sea spread the ocean inland." +
                                          "^^The men learned to till the soil, in exchange for their dead."+
                                          "^^The flame spread to the depths and tropics, creating life where there was none."+
                                          "^^That what remained found refuge in the primordial slurry, which became the slime.")
        },
        {
            "439", //Hints for treasure
            new Tuple<bool, string>(false,"The ancient men, blessed with the power of creation, made tools of war." +
                                          "^^When the ancient empires fell, their weapons became scattered." +
                                          "^^The first casket hid the cornucopia of elements." +
                                          "^^Stone Golem, Skeleton, Metal Head, Serpent, Pepper Rex, Haunted Skull." + 
                                          "^^The second casket contained the secrets of the elements and the symbol of their lord."+
                                          "^^Mummy, Iridium Bat, Haunted Skull."+
                                          "^^The final casket held the forbidden knowledge of the god slayer, granted to his wights."+
                                          "^^Hard Casket, Dwarvish Sentry, Magma Duggy.")
        }
        
    };

    public static Dictionary<string, Dictionary<string,string>> loadableEvents = new Dictionary<string, Dictionary<string,string>>()
    {
        {
            "Data/Events/Farm", new Dictionary<string,string>()
            {
                {
                    "RS.0/f Wizard 1000/t 600 1200",
                    "continue/64 15/farmer 64 16 2 Wizard 64 18 0" +
                    "/pause 1500/speak Wizard \"Greetings, @. I hope I am not interrupting your work on the farm.\"" +
                    "/speak Wizard \"I've made great progress with my research as of late, thanks to your generous gifts.\"" +
                    "/speak Wizard \"As thanks, I wanted to give you this old tome of runic magic from my personal library, I have no use for it anymore.\"" +
                    "/stopMusic /itemAboveHead 4290 /pause 1500 /glow 24 107 97 /playsound RunescapeSpellbook.MagicLevel /pause 2000 /mail RSSpellMailGet" +
                    "/speak Wizard \"This form of magic should be suitable for a novice. You need only some runestones, I'm sure you've come across some in the mines already.\"/pause 600" +
                    "/speak Wizard \"Well, that was all. I'll be on my way now.\"" +
                    "/pause 300/end"
                }
            }
        },
        {
            "Data/Events/ArchaeologyHouse", new Dictionary<string,string>()
            {
                {
                    "RS.1/n RSRunesFound",
                    "continue/11 9/farmer 50 50 0 Gunther 11 9 0 Marlon 12 9 3" +
                    "/skippable /pause 1000/speak Gunther \"Marlon, you know I can't accept a sword as payment for your late return fees...\"" +
                    "/speak Marlon \"This is an antique! I've been using this blade for decades now!\"" +
                    "/warp farmer 3 14 /playSound doorClose /pause 1000" +
                    "/move farmer 0 -1 1 /move farmer 3 0 2 /move farmer 0 1 1 /move farmer 5 0 0 /move Marlon 0 0 2 /move farmer 0 -3 0" +
                    "/move Gunther 0 0 2 /speak Gunther \"Ah! Welcome! Just let me finish putting these books away and I'll be right with you!\"" +
                    "/move Gunther 0 0 0 /pause 500 /jump Gunther 8 /pause 500 /textAboveHead Gunther \"*huff* *puff*\" /pause 2000 /move Gunther 0 0 2"+
                    "/speak Gunther \"Perhaps the books can wait. What do you need today, @?\"" +
                    "/question null \"#I found this underground#Can you tell me about this?\"" +
                    "/speak Gunther \"Let me have a look...\" /pause 1000"+
                    "/speak Gunther \"Hmm... I'm not quite sure what that is... \""+
                    "/speak Gunther \"The runes aren't any I recognise either...\""+
                    "/move Marlon 0 0 3 /speak Marlon \"Ah, well isn't that nostalgic.\""+
                    "/move Gunther 0 0 1 /speak Gunther \"You're familiar with these?\""+
                    "/speak Marlon \"Not myself, but an old friend of mine used to be obsessed with them.\""+
                    "/move Marlon 0 0 2 /speak Marlon \"Could you bring it over here, @? I'd like to have a closer look.\""+
                    "/move farmer 1 0 0 /move Marlon 0 0 2 /move farmer 0 -1 0 /pause 1000 /move farmer 0 1 0 /pause 1000" +
                    "/speak Marlon \"As I suspected, these are definitely guthixian runestones. Or rather, they contain guthixian runestones.\"" +
                    "/speak Marlon \"Ol' Ras used to spend hours trying to crack these things open, until I showed up. Turns out a strike with the trusty hammer does the job in seconds.\""+
                    "/move Marlon 0 1 2 /move Gunther 0 0 2" +
                    "/speak Marlon \"I'd take these down to the blacksmith, If he's worth his prices, he'll be able to open them.\""+
                    "/speak Marlon \"If you want to actually use the things, you'll have to pry it out of Rasmodius. He's a secretive old man, but get on his good side and he'll talk your ear off.\""+
                    "/pause 500 /end"
                }
            }
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
        if (farmer.mailReceived.Contains("TofuHasUnlockedMagic"))
        {
            return true;
        }
        
        if (farmer.eventsSeen.Contains("RS.0"))
        {
            farmer.mailReceived.Add("TofuHasUnlockedMagic");
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