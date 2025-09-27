using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace RunescapeSpellbook;
public static class ModAssets
{
    public static Texture2D extraTextures; //Includes spells + basic icons
    public static Texture2D animTextures;

    public static PlayerLocalData localFarmerData;
    
    public const int spellsY = 16;
    public const int spellsSize = 80;
    private static object multiplayer;
    
    public const int animFrames = 4; 
    
    //Config getters
    public static Func<int> GetSpellBaseExpMultiplier { get; set; }
    
    public static Dictionary<string,ModLoadObjects> modItems = new(){
        {"Tofu.RunescapeSpellbook_RuneSpellbook",new RunesObjects("Tofu.RunescapeSpellbook_RuneSpellbook","RuneSpellbook",0,-999)},
        {"Tofu.RunescapeSpellbook_RuneAir",new RunesObjects("Tofu.RunescapeSpellbook_RuneAir","RuneAir",1,-429,
            new Dictionary<string, PrefType>(){{"Emily",PrefType.Neutral}})},
        {"Tofu.RunescapeSpellbook_RuneWater",new RunesObjects("Tofu.RunescapeSpellbook_RuneWater","RuneWater",2,-429,
            new(){{"Emily",PrefType.Neutral},{"Willy",PrefType.Neutral},{"Elliott",PrefType.Neutral}})},
        {"Tofu.RunescapeSpellbook_RuneFire",new RunesObjects("Tofu.RunescapeSpellbook_RuneFire","RuneFire",3,-429,
            new(){{"Emily",PrefType.Neutral},{"Sam",PrefType.Neutral},{"Vincent",PrefType.Neutral}})},
        {"Tofu.RunescapeSpellbook_RuneEarth",new RunesObjects("Tofu.RunescapeSpellbook_RuneEarth","RuneEarth",4,-429,
            new(){{"Emily",PrefType.Neutral},{"Dwarf",PrefType.Neutral},{"Demetrius",PrefType.Neutral}})},
        {"Tofu.RunescapeSpellbook_RuneLaw",new RunesObjects("Tofu.RunescapeSpellbook_RuneLaw","RuneLaw",5,-431,
            new(){{"Emily",PrefType.Neutral},{"Wizard",PrefType.Like}})},
        {"Tofu.RunescapeSpellbook_RuneNature",new RunesObjects("Tofu.RunescapeSpellbook_RuneNature","RuneNature",6,-431,
            new(){{"Emily",PrefType.Neutral},{"Leo",PrefType.Neutral},{"Linus",PrefType.Neutral},{"Wizard",PrefType.Neutral}})},
        {"Tofu.RunescapeSpellbook_RuneCosmic",new RunesObjects("Tofu.RunescapeSpellbook_RuneCosmic","RuneCosmic",7,-431,
            new(){{"Emily",PrefType.Neutral},{"Maru",PrefType.Like},{"Wizard",PrefType.Neutral}})},
        {"Tofu.RunescapeSpellbook_RuneAstral",new RunesObjects("Tofu.RunescapeSpellbook_RuneAstral","RuneAstral",8,-431,
            new(){{"Emily",PrefType.Like},{"Maru",PrefType.Neutral},{"Wizard",PrefType.Neutral}})},
        {"Tofu.RunescapeSpellbook_RuneChaos",new RunesObjects("Tofu.RunescapeSpellbook_RuneChaos","RuneChaos",9,-430,
            new(){{"Emily",PrefType.Hate},{"Kent",PrefType.Hate},{"Wizard",PrefType.Neutral}})},
        {"Tofu.RunescapeSpellbook_RuneDeath",new RunesObjects("Tofu.RunescapeSpellbook_RuneDeath","RuneDeath",10,-430,
            new(){{"Sebastian",PrefType.Like},{"Emily",PrefType.Hate},{"George",PrefType.Hate},{"Evelyn",PrefType.Hate},{"Wizard",PrefType.Neutral}})},

        {"Tofu.RunescapeSpellbook_AmmoFire",new SlingshotItem("Tofu.RunescapeSpellbook_AmmoFire","AmmoFire",30,15,2)},
        {"Tofu.RunescapeSpellbook_AmmoEarth",new SlingshotItem("Tofu.RunescapeSpellbook_AmmoEarth","AmmoEarth",31,25,1,true)},
        
        {"Tofu.RunescapeSpellbook_TreasureElemental",new TreasureObjects("Tofu.RunescapeSpellbook_TreasureElemental","TreasureElemental",19,
            new()
            {
                new ItemDrop("Tofu.RunescapeSpellbook_RuneAir", 10, 12, 1),
                new ItemDrop("Tofu.RunescapeSpellbook_RuneAir", 13, 23, 0.6),
                new ItemDrop("Tofu.RunescapeSpellbook_RuneAir", 25, 35, 0.35),
                
                new ItemDrop("Tofu.RunescapeSpellbook_RuneWater", 10, 12, 1),
                new ItemDrop("Tofu.RunescapeSpellbook_RuneWater", 13, 23, 0.5),
                new ItemDrop("Tofu.RunescapeSpellbook_RuneWater", 25, 35, 0.25),
                
                new ItemDrop("Tofu.RunescapeSpellbook_RuneEarth", 10, 12, 1),
                new ItemDrop("Tofu.RunescapeSpellbook_RuneEarth", 13, 23, 0.5),
                new ItemDrop("Tofu.RunescapeSpellbook_RuneEarth", 25, 35, 0.25),
                
                new ItemDrop("Tofu.RunescapeSpellbook_RuneFire", 10, 12, 1),
                new ItemDrop("Tofu.RunescapeSpellbook_RuneFire", 13, 23, 0.5),
                new ItemDrop("Tofu.RunescapeSpellbook_RuneFire", 25, 35, 0.25),
            },40)},
        
        {"Tofu.RunescapeSpellbook_TreasureCatalytic",new TreasureObjects("Tofu.RunescapeSpellbook_TreasureCatalytic","TreasureCatalytic",20,
            new()
            {
                new ItemDrop("Tofu.RunescapeSpellbook_RuneLaw", 10, 15, 1),
                
                new ItemDrop("Tofu.RunescapeSpellbook_RuneNature", 10, 15, 1),
                
                new ItemDrop("Tofu.RunescapeSpellbook_RuneCosmic", 10, 15, 1),
                
                new ItemDrop("Tofu.RunescapeSpellbook_RuneAstral", 10, 15, 1),
                
                new ItemDrop("Tofu.RunescapeSpellbook_RuneChaos", 10, 15, 1),
                new ItemDrop("Tofu.RunescapeSpellbook_RuneChaos", 16, 23, 0.5),
                new ItemDrop("Tofu.RunescapeSpellbook_RuneChaos", 25, 35, 0.25),
                
                new ItemDrop("Tofu.RunescapeSpellbook_RuneDeath", 10, 15, 0.5),
                new ItemDrop("Tofu.RunescapeSpellbook_RuneDeath", 16, 23, 0.25),
                new ItemDrop("Tofu.RunescapeSpellbook_RuneDeath", 25, 35, 0.1),
            },70)},
        
        {"Tofu.RunescapeSpellbook_EasyCasket", new TreasureObjects("Tofu.RunescapeSpellbook_EasyCasket","TreasureEasy",21,
            new()
            {
                new ItemDrop("Tofu.RunescapeSpellbook_TreasureElemental",5,10,0.3),
                new ItemDrop("Tofu.RunescapeSpellbook_TreasureCatalytic",5,10,0.3),
                
                new ItemDrop("Tofu.RunescapeSpellbook_StaffAir",1,1,0.8),
                new ItemDrop("Tofu.RunescapeSpellbook_StaffWater",1,1,0.8),
                new ItemDrop("Tofu.RunescapeSpellbook_StaffEarth",1,1,0.8),
                new ItemDrop("Tofu.RunescapeSpellbook_StaffFire",1,1,0.8),
                
                new ItemDrop("Tofu.RunescapeSpellbook_HardCasket",1,1,0.05),
            },200,new(){{"Abigail",PrefType.Like}})},
        
        {"Tofu.RunescapeSpellbook_HardCasket",new TreasureObjects("Tofu.RunescapeSpellbook_HardCasket","TreasureHard",22,
            new()
            {
                new ItemDrop("Tofu.RunescapeSpellbook_TreasureElemental",10,15,0.5),
                new ItemDrop("Tofu.RunescapeSpellbook_TreasureCatalytic",10,15,0.5),
                new ItemDrop("Tofu.RunescapeSpellbook_PotHunter",1,3,0.3),
                new ItemDrop("Tofu.RunescapeSpellbook_PotBattlemage",1,3,0.3),
                new ItemDrop("Tofu.RunescapeSpellbook_PotGuthix",1,3,0.1,2),
                new ItemDrop("Tofu.RunescapeSpellbook_PotGuthix",1,3,0.1,1),
                new ItemDrop("Tofu.RunescapeSpellbook_PotGuthix",1,3,0.1,0),
                
                new ItemDrop("Tofu.RunescapeSpellbook_StaffMysticAir",1,1,0.6),
                new ItemDrop("Tofu.RunescapeSpellbook_StaffMysticWater",1,1,0.6),
                new ItemDrop("Tofu.RunescapeSpellbook_StaffMysticEarth",1,1,0.6),
                new ItemDrop("Tofu.RunescapeSpellbook_StaffMysticFire",1,1,0.6),
                new ItemDrop("Tofu.RunescapeSpellbook_StaffAncient",1,1,0.4),
                
                new ItemDrop("Tofu.RunescapeSpellbook_BarrowsCasket",1,1,0.1),
            },500,new(){{"Abigail",PrefType.Love}})},
        
        {"Tofu.RunescapeSpellbook_BarrowsCasket",new TreasureObjects("Tofu.RunescapeSpellbook_BarrowsCasket","TreasureBarrows",23,
            new()
            {
                new ItemDrop("Tofu.RunescapeSpellbook_PotSara",1,3,0.1,4),
                new ItemDrop("Tofu.RunescapeSpellbook_PotSara",1,4,0.1,2),
                new ItemDrop("Tofu.RunescapeSpellbook_PotSara",2,5,0.1,1),
                new ItemDrop("Tofu.RunescapeSpellbook_PotSara",2,6,0.1,0),
                
                new ItemDrop("Tofu.RunescapeSpellbook_StaffAncient",1,1,1),
                new ItemDrop("Tofu.RunescapeSpellbook_StaffAhrims",1,1,0.5),
                new ItemDrop("Tofu.RunescapeSpellbook_StaffBlueMoon",1,1,0.5),
            },1000,new(){{"Abigail",PrefType.Love}})},
        
        {"Tofu.RunescapeSpellbook_TreasureAirPack",new PackObject("Tofu.RunescapeSpellbook_TreasureAirPack","RuneAir",24,"Tofu.RunescapeSpellbook_RuneAir",3)},
        {"Tofu.RunescapeSpellbook_TreasureWaterPack",new PackObject("Tofu.RunescapeSpellbook_TreasureWaterPack","RuneWater",25,"Tofu.RunescapeSpellbook_RuneWater",1)},
        {"Tofu.RunescapeSpellbook_TreasureFirePack",new PackObject("Tofu.RunescapeSpellbook_TreasureFirePack","RuneFire",26,"Tofu.RunescapeSpellbook_RuneFire")},
        {"Tofu.RunescapeSpellbook_TreasureEarthPack",new PackObject("Tofu.RunescapeSpellbook_TreasureEarthPack","RuneEarth",27,"Tofu.RunescapeSpellbook_RuneEarth")},
        {"Tofu.RunescapeSpellbook_TreasureChaosPack",new PackObject("Tofu.RunescapeSpellbook_TreasureChaosPack","RuneChaos",28,"Tofu.RunescapeSpellbook_RuneChaos")},
        {"Tofu.RunescapeSpellbook_TreasureDeathPack",new PackObject("Tofu.RunescapeSpellbook_TreasureDeathPack","RuneDeath",29,"Tofu.RunescapeSpellbook_RuneDeath")},
        
        {"Tofu.RunescapeSpellbook_FishKaram",new FishObject("Tofu.RunescapeSpellbook_FishKaram","FishKaram",32,45,600,1800,new(){Season.Spring,Season.Summer},"sunny",new()
            { "Beach" },4,2,30,5,2,Color.Cyan,"color_sea_green",1,7,
            new()
            {
                {5,
                    new()
                    {
                        "152 5 10","395","684 10 20","766 10 20"
                    }
                },
                {8,
                    new()
                    {
                        "88 3 5","306 2 4","206 2 4"
                    }
                    
                }
            }, new()
            {
                {9,new ItemDrop("Tofu.RunescapeSpellbook_EasyCasket",1,1,0.05)},
                {1,new ItemDrop("Tofu.RunescapeSpellbook_RuneWater",20,30,0.5)},
                {2,new ItemDrop("Tofu.RunescapeSpellbook_RuneAir",20,30,0.3)},
                {5,new ItemDrop("Tofu.RunescapeSpellbook_RuneEarth",20,30,0.3)},
                {6,new ItemDrop("Tofu.RunescapeSpellbook_RuneFire",20,30,0.2)},
                {0,new ItemDrop("812",1,1,1.0)}
            },new(){{"Willy",PrefType.Neutral}}
            )},
        {"Tofu.RunescapeSpellbook_FishMonk",new FishObject("Tofu.RunescapeSpellbook_FishMonk","FishMonk",33,60,1600,2300,new(){Season.Fall,Season.Winter,Season.Spring},"rainy",new()
            { "Beach" },4,5,75,20,3,Color.NavajoWhite,"color_sand",25,40,
            new()
            {
                {5,
                    new()
                    {
                        "198","227","228"
                    }
                },
                {8,
                    new()
                    {
                        "213","242","728","787","Tofu.RunescapeSpellbook_EasyCasket"
                    }
                    
                }
            }, new()
            {
                {9,new ItemDrop("Tofu.RunescapeSpellbook_HardCasket",1,1,0.05)},
                {7,new ItemDrop("Tofu.RunescapeSpellbook_RuneDeath",20,30,0.4)},
                {3,new ItemDrop("Tofu.RunescapeSpellbook_RuneChaos",20,30,0.5)},
                {0,new ItemDrop("812",1,1,1.0)}
            },new(){{"Willy",PrefType.Neutral}}
        )},
        {"Tofu.RunescapeSpellbook_FishManta",new FishObject("Tofu.RunescapeSpellbook_FishManta","FishManta",34,75,1200,1800,new(){Season.Summer},"sunny",new()
                { "Beach" },4,6,150,30,4,Color.RoyalBlue,"color_red",118,216,
            new()
            {
                {5,
                    new()
                    {
                        "346 10","456"
                    }
                },
                {8,
                    new()
                    {
                        "Tofu.RunescapeSpellbook_TreasureElemental 10","832","Tofu.RunescapeSpellbook_EasyCasket"
                    }
                    
                }
            }, new()
            {
                {6,new ItemDrop("Tofu.RunescapeSpellbook_RuneNature",4,10,0.5)},
                {2,new ItemDrop("Tofu.RunescapeSpellbook_RuneCosmic",4,10,0.3)},
                {5,new ItemDrop("Tofu.RunescapeSpellbook_RuneAstral",4,10,0.3)},
                {0,new ItemDrop("Tofu.RunescapeSpellbook_RuneLaw",4,10,1.0)},
            },new(){{"Willy",PrefType.Like}}
        )},
        {"Tofu.RunescapeSpellbook_FishSword",new FishObject("Tofu.RunescapeSpellbook_FishSword","FishSword",35,95,2000,2600,new(){Season.Spring,Season.Summer,Season.Fall,Season.Winter},"both",new()
                { "IslandSouth","IslandWest","IslandSouthEast" },3,8,280,30,4,Color.HotPink,"color_pink",100,130,
            new()
            {
                {4,
                    new()
                    {
                        "874 4 7","225"
                    }
                },
                {8,
                    new()
                    {
                        "852 3 5","904"
                    }
                    
                }
            }, new()
            {
                {9,new ItemDrop("74",1,1,0.01)},
                {4,new ItemDrop("Tofu.RunescapeSpellbook_RuneDeath",30,50,0.6)},
                {2,new ItemDrop("773",1,3,0.5)},
                {0,new ItemDrop("812",1,1,1.0)},
            },new(){{"Willy",PrefType.Love}}
        )},
        {"Tofu.RunescapeSpellbook_SeedHarra", new SeedObject("Tofu.RunescapeSpellbook_SeedHarra","SeedHarra",36,50)},
        {"Tofu.RunescapeSpellbook_CropHarra",new CropObject("Tofu.RunescapeSpellbook_CropHarra","CropHarra",
            "Tofu.RunescapeSpellbook_SeedHarra",new(){Season.Fall},3,0,37,105,-50,"color_green",-75,1,0.25f,
            new(){{"Emily",PrefType.Like}})},
        {"Tofu.RunescapeSpellbook_SeedLanta", new SeedObject("Tofu.RunescapeSpellbook_SeedLanta","SeedLanta",38,50)},
        {"Tofu.RunescapeSpellbook_CropLanta",new CropObject("Tofu.RunescapeSpellbook_CropLanta","CropLanta",
            "Tofu.RunescapeSpellbook_SeedLanta",new(){Season.Winter},3,1,39,110,-50,"color_blue",-75,1,0.25f,
            new(){{"Emily",PrefType.Love}})},
        {"Tofu.RunescapeSpellbook_PotGuthix", new PotionObject("Tofu.RunescapeSpellbook_PotGuthix","PotGuthix",40,400,0.3f,0.15f,"Tofu.RunescapeSpellbook_CropHarra",5000,
            new(){{"Caroline",PrefType.Love},{"Lewis",PrefType.Love},{"Sandy",PrefType.Love},{"Harvey",PrefType.Like},{"Jas",PrefType.Hate},{"Vincent",PrefType.Hate}})},
        {"Tofu.RunescapeSpellbook_PotSara", new PotionObject("Tofu.RunescapeSpellbook_PotSara","PotSara",44,550,0.6f,0.25f,"Tofu.RunescapeSpellbook_CropLanta",6000,
            new(){{"Caroline",PrefType.Love},{"Lewis",PrefType.Love},{"Sandy",PrefType.Love},{"Harvey",PrefType.Like},{"Jas",PrefType.Hate},{"Vincent",PrefType.Hate}})},
        {"Tofu.RunescapeSpellbook_PotHarraDye", new PotionObject("Tofu.RunescapeSpellbook_PotHarraDye","PotHarraDye",41,250,"Tofu.RunescapeSpellbook_CropHarra",4000,"color_green",
            new(){{"Emily",PrefType.Love},{"Elliott",PrefType.Love},{"Leah",PrefType.Like}})},
        {"Tofu.RunescapeSpellbook_PotLantaDye", new PotionObject("Tofu.RunescapeSpellbook_PotLantaDye","PotLantaDye",45,350,"Tofu.RunescapeSpellbook_CropLanta",4000,"color_blue",
            new(){{"Emily",PrefType.Love},{"Elliott",PrefType.Love},{"Sebastian",PrefType.Love},{"Leah",PrefType.Like}})},
        {"Tofu.RunescapeSpellbook_PotHunter", new PotionObject("Tofu.RunescapeSpellbook_PotHunter","PotHunter",43,300,"Tofu.RunescapeSpellbook_CropHarra 1 300 1 881 5",new(){"Tofu.RunescapeSpellbook_BuffHunters"},
            new(){{"Willy",PrefType.Like},{"Linus",PrefType.Like},{"Penny",PrefType.Hate}})},
        {"Tofu.RunescapeSpellbook_PotBattlemage", new PotionObject("Tofu.RunescapeSpellbook_PotBattlemage","PotBattlemage",46,500,"Tofu.RunescapeSpellbook_CropLanta 1 807 1 90 3",new(){"Tofu.RunescapeSpellbook_BuffBattlemage"},
            new(){{"Wizard",PrefType.Love}})},
    };
    
    //These are custom melee weapons that use 
    public static readonly StaffWeaponData[] staffWeapons =
    {
        new StaffWeaponData("Tofu.RunescapeSpellbook_StaffMagic", "StaffMagic", 5, 10, 11),
        new StaffWeaponData("Tofu.RunescapeSpellbook_StaffAir", "StaffAir", 20, 30, 12,
            11,1.15f, "Tofu.RunescapeSpellbook_RuneAir"),
        new StaffWeaponData("Tofu.RunescapeSpellbook_StaffWater",  "StaffWater", 20, 30, 13,
            11,1.15f, "Tofu.RunescapeSpellbook_RuneWater"),
        new StaffWeaponData("Tofu.RunescapeSpellbook_StaffEarth",  "StaffEarth", 20, 30, 14,
            11,1.15f, "Tofu.RunescapeSpellbook_RuneEarth"),
        new StaffWeaponData("Tofu.RunescapeSpellbook_StaffFire", "StaffFire", 20, 30, 15,
            11,1.15f, "Tofu.RunescapeSpellbook_RuneFire"),
        
        new StaffWeaponData("Tofu.RunescapeSpellbook_StaffMysticAir", "StaffMysticAir", 25, 35, 48,
            12,1.25f, "Tofu.RunescapeSpellbook_RuneAir"),
        new StaffWeaponData("Tofu.RunescapeSpellbook_StaffMysticWater",  "StaffMysticWater", 25, 35, 49,
            12,1.25f, "Tofu.RunescapeSpellbook_RuneWater"),
        new StaffWeaponData("Tofu.RunescapeSpellbook_StaffMysticEarth",  "StaffMysticEarth", 25, 35, 50,
            12,1.25f, "Tofu.RunescapeSpellbook_RuneEarth"),
        new StaffWeaponData("Tofu.RunescapeSpellbook_StaffMysticFire", "StaffMysticFire", 25, 35, 51,
            12,1.25f, "Tofu.RunescapeSpellbook_RuneFire"),
        new StaffWeaponData("Tofu.RunescapeSpellbook_StaffAncient",  "StaffAncient",
            25, 40, 16,
            13,1.4f,"",0,0,0,0.05f),
        
        
        new StaffWeaponData("Tofu.RunescapeSpellbook_StaffAhrims", "StaffAhrims", 30, 45, 17,
            15,1.6f),
        new StaffWeaponData("Tofu.RunescapeSpellbook_StaffBlueMoon", "StaffBlueMoon", 70, 90, 18,
            15,1.5f)
    };

    //Machines
    public static readonly MachinesObject[] machineItems =
    {
        new MachinesObject("Tofu.RunescapeSpellbook_PackShredder","PackShredder",100,0,
            new()
            {
                {"Tofu.RunescapeSpellbook_TreasureAirPack","Tofu.RunescapeSpellbook_RuneAir"},
                {"Tofu.RunescapeSpellbook_TreasureWaterPack","Tofu.RunescapeSpellbook_RuneWater"},
                {"Tofu.RunescapeSpellbook_TreasureFirePack","Tofu.RunescapeSpellbook_RuneFire"},
                {"Tofu.RunescapeSpellbook_TreasureEarthPack","Tofu.RunescapeSpellbook_RuneEarth"},
                {"Tofu.RunescapeSpellbook_TreasureChaosPack","Tofu.RunescapeSpellbook_RuneChaos"},
                {"Tofu.RunescapeSpellbook_TreasureDeathPack","Tofu.RunescapeSpellbook_RuneDeath"},
            }, 3,new Func<string,List<ItemDrop>>((itemID)=>((PackObject)modItems[itemID]).GetItemRanges()),"336 1 338 2 709 5",3,"(O)382","PackShredderFailMessage"
            )
    };
    
    //This dictionary provides a quick reference for which weapons provide what rune
    public static Dictionary<string, List<string>> infiniteRuneReferences;
    
    public static readonly Spell[] modSpells = {
        new TeleportSpell(0,"Teleport_Valley","TeleportValley",0,
            new() { {"Tofu.RunescapeSpellbook_RuneLaw", 1},{"Tofu.RunescapeSpellbook_RuneAir",3},{"Tofu.RunescapeSpellbook_RuneWater",2} },8,"Town", 43, 60,0),
        
        new TeleportSpell(1,"Teleport_Home","TeleportHome",4,
            new() { {"Tofu.RunescapeSpellbook_RuneLaw", 1},{"Tofu.RunescapeSpellbook_RuneAir",3},{"Tofu.RunescapeSpellbook_RuneEarth",3} },10, "FarmHouse"),
        
        new InventorySpell(2,"Menu_Superheat","MenuSuperheat",1,
            new() { {"Tofu.RunescapeSpellbook_RuneNature", 1},{"Tofu.RunescapeSpellbook_RuneFire",4}},10,
            (i=>i is Item item && (item.QualifiedItemId == "(O)388" || DataLoader.Machines(Game1.content).GetValueOrDefault("(BC)13").OutputRules.Any(x=>x.Triggers.Any(y=>y.RequiredItemId == item.QualifiedItemId)))),
            SpellEffects.SuperheatItem,1,"Superheat"),
        
        new InventorySpell(3,"Menu_HighAlch","MenuHighAlch",8,
            new() { {"Tofu.RunescapeSpellbook_RuneNature", 2},{"Tofu.RunescapeSpellbook_RuneFire",5}},15,(i=>i is Item item && item.canBeShipped() && item.salePrice(false) > 0 && item.Category != -429 && item.Category != -430 && item.Category != -431),
            SpellEffects.HighAlchemy,0,"HighAlch"),
        
        new TilesSpell(4,"Area_Humidify","AreaHumidify",1,
            new() { {"Tofu.RunescapeSpellbook_RuneAstral", 1},{"Tofu.RunescapeSpellbook_RuneFire",1},{"Tofu.RunescapeSpellbook_RuneWater",3}}, 0.4f,SpellEffects.Humidify, 10,5,"Humidify",
            (tile => tile is HoeDirt hoeLand && (hoeLand.crop == null || !hoeLand.crop.forageCrop.Value || hoeLand.crop.whichForageCrop.Value != "2") && hoeLand.state.Value != 1)),
        
        new TilesSpell(5,"Area_Cure","AreaCure",6,
            new() { {"Tofu.RunescapeSpellbook_RuneAstral", 1},{"Tofu.RunescapeSpellbook_RuneEarth",8}},0.6f, SpellEffects.CurePlant, 10,6,"Cure",
            (tile => tile is HoeDirt hoeLand && hoeLand.crop != null && hoeLand.crop.dead.Value)),
        
        new BuffSpell(6,"Buff_VileVigour","BuffVileVigour",3,
            new() { {"Tofu.RunescapeSpellbook_RuneCosmic", 1},{"Tofu.RunescapeSpellbook_RuneAir",3}},5, (f=> f is Farmer farmer && farmer.stamina < farmer.MaxStamina), SpellEffects.VileVigour,
            7,"Vile","spell-error.BuffVileVigourEnergyFull.text"),
        
        new BuffSpell(7,"Buff_PieMake","BuffPieMake",3,
            new() { {"Tofu.RunescapeSpellbook_RuneAstral", 1},{"Tofu.RunescapeSpellbook_RuneFire",1},{"Tofu.RunescapeSpellbook_RuneWater",1}}, 15,
            (f=> f is Farmer farmer && farmer.cookingRecipes.Length > 0), SpellEffects.BakePie, 8,"BakePie","spell-error.BuffPieMakeNoRecipes.text"),
        
        new TeleportSpell(8,"Teleport_Desert","TeleportDesert",5,
            new() { {"Tofu.RunescapeSpellbook_RuneLaw", 2},{"Tofu.RunescapeSpellbook_RuneEarth",5},{"Tofu.RunescapeSpellbook_RuneFire",5}}, 15,"Desert", 19, 34,2,
            ((farmer => Game1.MasterPlayer.mailReceived.Contains("ccVault")))),
        
        new TeleportSpell(9,"Teleport_Ginger","TeleportGinger",7,
            new() { {"Tofu.RunescapeSpellbook_RuneLaw", 2},{"Tofu.RunescapeSpellbook_RuneWater",5},{"Tofu.RunescapeSpellbook_RuneFire",5}},15, "IslandSouth",21,37,0,
            ((farmer => Game1.MasterPlayer.hasOrWillReceiveMail("willyBoatFixed")))),
        
        new TeleportSpell(10,"Teleport_Caves","TeleportCaves",2,
            new() { {"Tofu.RunescapeSpellbook_RuneLaw", 1},{"Tofu.RunescapeSpellbook_RuneAir",5}},10, "Mountain",54,7,0, 
            ((farmer => Game1.MasterPlayer.hasOrWillReceiveMail("landslideDone")))),
        
        new InventorySpell(11,"Menu_EnchantRuby","MenuEnchantRuby",4,
            new() { {"Tofu.RunescapeSpellbook_RuneCosmic", 1},{"Tofu.RunescapeSpellbook_RuneFire",3}},10,(i => i is Item item && SpellEffects.redGemsEnchants.ContainsKey(item.ItemId)),SpellEffects.EnchantRubyBolts,2,"EnchantBolt"),
        
        new InventorySpell(12,"Menu_EnchantEmerald","MenuEnchantEmerald",8,
            new() { {"Tofu.RunescapeSpellbook_RuneCosmic", 2},{"Tofu.RunescapeSpellbook_RuneEarth",3}},15,(i => i is Item item && SpellEffects.greenGemsEnchants.ContainsKey(item.ItemId)),SpellEffects.EnchantEmeraldBolt,2,"EnchantBolt"),
        
        new BuffSpell(13,"Buff_DarkLure","BuffDarkLure",6,
            new() { {"Tofu.RunescapeSpellbook_RuneNature", 3},{"Tofu.RunescapeSpellbook_RuneDeath",3},{"Tofu.RunescapeSpellbook_RuneAir",3}},10,(f=> f is Farmer farmer && !farmer.hasBuff("Tofu.RunescapeSpellbook_BuffDark")),SpellEffects.DarkLure, 9,"DarkLure","spell-error.BuffDarkLureActive.text"),
        
        new CombatSpell(14,"Combat_Wind","CombatWind",0,
            new() { {"Tofu.RunescapeSpellbook_RuneChaos", 1},{"Tofu.RunescapeSpellbook_RuneAir",1}}, 1,40,15,0,Color.White,"WindStrike"),
       
        new CombatSpell(15,"Combat_Water","CombatWater",2,
            new() { {"Tofu.RunescapeSpellbook_RuneChaos", 2},{"Tofu.RunescapeSpellbook_RuneAir",2},{"Tofu.RunescapeSpellbook_RuneWater",2}},2, 70,16,1,Color.DarkCyan,"WaterBolt"),
        
        new CombatSpell(16,"Combat_Undead","CombatUndead",4,
            new() { {"Tofu.RunescapeSpellbook_RuneChaos", 2},{"Tofu.RunescapeSpellbook_RuneAir",2},{"Tofu.RunescapeSpellbook_RuneEarth",2}},4, 60,13,3,Color.Yellow,"CrumbleUndead",SpellEffects.DealUndeadDamage),
        
        new CombatSpell(17,"Combat_Earth","CombatEarth",6,
            new() { {"Tofu.RunescapeSpellbook_RuneDeath", 1},{"Tofu.RunescapeSpellbook_RuneAir",3},{"Tofu.RunescapeSpellbook_RuneEarth",3}},4, 90,16,1,Color.DarkGreen,"EarthBlast"),
        
        new CombatSpell(18,"Combat_Fire","CombatFire",8,
            new() { {"Tofu.RunescapeSpellbook_RuneDeath", 2},{"Tofu.RunescapeSpellbook_RuneAir",5},{"Tofu.RunescapeSpellbook_RuneFire",4}},5, 120,15,2,Color.OrangeRed,"FireWave"),
        
        new BuffSpell(19,"Buff_Charge","BuffCharge",7,
            new() { {"Tofu.RunescapeSpellbook_RuneDeath", 3},{"Tofu.RunescapeSpellbook_RuneAir",3},{"Tofu.RunescapeSpellbook_RuneFire",3}},10,(f=> f is Farmer farmer && !farmer.hasBuff("Tofu.RunescapeSpellbook_BuffCharge")),SpellEffects.Charge, 10,"Charge","spell-error.BuffChargeActive.text"),
        
        new CombatSpell(20,"Combat_Demonbane","CombatDemonbane",9,
            new() { {"Tofu.RunescapeSpellbook_RuneDeath", 2},{"Tofu.RunescapeSpellbook_RuneAstral",2},{"Tofu.RunescapeSpellbook_RuneFire",4}},6, 100,13,3,Color.Purple,"CrumbleUndead",SpellEffects.DealDemonbaneDamage),
        
        new CombatSpell(21,"Combat_Blood","CombatBlood",10,
            new() { {"Tofu.RunescapeSpellbook_RuneDeath", 4},{"Tofu.RunescapeSpellbook_RuneCosmic",3}}, 10,100,15,1,Color.Crimson, "BloodBarrage",SpellEffects.DealVampiricDamage),
        
        new InventorySpell(22,"Menu_Plank","MenuPlank",3,
            new() { {"Tofu.RunescapeSpellbook_RuneAstral", 1},{"Tofu.RunescapeSpellbook_RuneCosmic",1}},10,
            (i => i is Item item && (item.itemId.Value == "388" || item.itemId.Value == "709" || 
                                     (CraftingRecipe.craftingRecipes.ContainsKey(item.Name) 
                                      && CraftingRecipe.craftingRecipes[item.Name].Split(' ').ToList() is List<string> recipes 
                                      && ((recipes.IndexOf("388") != -1 && (recipes.IndexOf("388") + 1) % 2 != 0) || (recipes.IndexOf("709") != -1 && recipes.IndexOf("709") + 1 % 2 != 0) ) )))
            ,SpellEffects.PlankMake, 3,"Degrime"),
        
        new InventorySpell(23,"Menu_LowAlch","MenuLowAlch",5,
            new() { {"Tofu.RunescapeSpellbook_RuneNature", 1},{"Tofu.RunescapeSpellbook_RuneFire",3}},10,(i=>i is Item item && item.canBeShipped() && item.salePrice(false) > 0 && item.Category != -429 && item.Category != -430 && item.Category != -431),
            SpellEffects.LowAlchemy,0,"HighAlch"),

    };
    
    public static readonly List<PerkData> perks = new()
    {
        new PerkData(0,"Sapphire","Sapphire"),
        new PerkData(1,"Emerald","Emerald"),
        new PerkData(2,"Ruby","Ruby"),
        new PerkData(3,"Dragonstone","Dragonstone")
    };

    public static readonly Dictionary<string, List<ItemDrop>> monsterDrops = new()
    {
        //Caves (Basic)
        { "Big Slime", new(){ 
            new ItemDrop("Tofu.RunescapeSpellbook_RuneLaw",2,0.08f),
            new ItemDrop("Tofu.RunescapeSpellbook_RuneNature",2,0.1f),
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureElemental",1,0.02f),
        } },
        { "Prismatic Slime", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_RuneLaw",4,0.9f),
            new ItemDrop("Tofu.RunescapeSpellbook_RuneAstral",5,0.9f),
        } },
        { "Green Slime", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_RuneLaw",2,0.08f),
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureElemental",1,0.02f),
        } },
        { "Fly", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureAirPack",1,0.2f),
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureChaosPack",1,0.05f),
        } },
        { "Rock Crab", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureElemental",1,0.3f),
        } },
        { "Grub", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_RuneNature",2,0.1f),
        } },
        { "Bug", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureChaosPack",1,0.08f),
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureElemental",1,0.2f),
        } },
        { "Bat", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureAirPack",1,0.15f),
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureChaosPack",1,0.25f),
        } },
        { "Stone Golem", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureEarthPack",1,0.2f),
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureCatalytic",1,0.1f),
            new ItemDrop("Tofu.RunescapeSpellbook_EasyCasket",1,0.04f),
        } },
        { "Dust Spirit", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureAirPack",1,0.04f),
            new ItemDrop("Tofu.RunescapeSpellbook_RuneNature",1,0.02f),
        } },
        { "Frost Bat", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureAirPack",1,0.05f),
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureWaterPack",1,0.15f),
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureChaosPack",1,0.3f),
        } },
        { "Ghost", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureCatalytic",2,0.35f),
        } },
        { "Frost Jelly", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureWaterPack",1,0.1f),
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureElemental",1,0.2f),
        } },
        { "Skeleton", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureChaosPack",1,0.3f),
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureDeathPack",1,0.02f),
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureCatalytic",1,0.05f),
            new ItemDrop("Tofu.RunescapeSpellbook_EasyCasket",1,0.08f),
        } },
        { "Lava Bat", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureAirPack",2,0.15f),
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureFirePack",2,0.15f),
        } },
        { "Lava Crab", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureWaterPack",1,0.15f),
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureFirePack",1,0.3f),
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureElemental",2,0.3f),
        } },
        { "Shadow Shaman", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_RuneNature",3,0.2f),
            new ItemDrop("Tofu.RunescapeSpellbook_RuneAstral",2,0.2f),
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureCatalytic",2,0.2f),
            new ItemDrop("Tofu.RunescapeSpellbook_EasyCasket",1,0.1f),
        } },
        { "Metal Head", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureEarthPack",2,0.3f),
            new ItemDrop("Tofu.RunescapeSpellbook_EasyCasket",1,0.1f),
        } },
        { "Shadow Brute", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureAirPack",2,0.1f),
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureChaosPack",2,0.3f),
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureCatalytic",1,0.1f),
        } },
        { "Squid Kid", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureAirPack",3,0.2f),
            new ItemDrop("Tofu.RunescapeSpellbook_RuneCosmic",2,0.2f),
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureElemental",2,0.2f),
        } }, //Skull Cavern 
        { "Sludge", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_RuneLaw",2,0.2f),
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureElemental",1,0.2f),
        } },
        { "Serpent", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureAirPack",2,0.25f),
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureChaosPack",2,0.1f),
            new ItemDrop("Tofu.RunescapeSpellbook_EasyCasket",1,0.05f),
        } },
        { "Carbon Ghost", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureDeathPack",2,0.1f),
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureCatalytic",2,0.35f),
        } },
        { "Iridium Crab", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureWaterPack",5,0.6f),
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureElemental",6,0.6f),
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureCatalytic",3,0.4f),
        } },
        { "Pepper Rex", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureFirePack",3,1f),
            new ItemDrop("Tofu.RunescapeSpellbook_RuneNature",3,0.5f),
        } },
        { "Mummy", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureEarthPack",2,0.2f),
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureChaosPack",3,0.3f),
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureDeathPack",1,0.2f),
            new ItemDrop("Tofu.RunescapeSpellbook_HardCasket",1,0.07f),
        } },
        { "Iridium Bat", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureAirPack",3,0.5f),
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureDeathPack",2,0.2f),
            new ItemDrop("Tofu.RunescapeSpellbook_HardCasket",1,0.15f),
        } },
        { "Haunted Skull", new(){ //Quarry Mine
            new ItemDrop("Tofu.RunescapeSpellbook_RuneCosmic",3,0.4f),
            new ItemDrop("Tofu.RunescapeSpellbook_RuneAstral",3,0.3f),
            new ItemDrop("Tofu.RunescapeSpellbook_EasyCasket",1,0.05f),
            new ItemDrop("Tofu.RunescapeSpellbook_HardCasket",1,0.02f),
        } },
        { "Hot Head", new(){ //Ginger Island/Volcano
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureFirePack",2,0.3f),
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureDeathPack",3,0.2f),
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureCatalytic",2,0.2f),
        } },
        { "Tiger Slime", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureWaterPack",2,0.1f),
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureEarthPack",1,0.2f),
            new ItemDrop("Tofu.RunescapeSpellbook_RuneNature",5,0.3f),
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureChaosPack",3,0.3f),
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureDeathPack",2,0.1f),
        } },
        { "Magma Sprite", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureAirPack",3,0.2f),
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureFirePack",3,0.3f),
        } },
        { "Dwarvish Sentry", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_RuneLaw",4,0.24f),
            new ItemDrop("Tofu.RunescapeSpellbook_RuneCosmic",10,0.2f),
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureDeathPack",3,0.3f),
            new ItemDrop("Tofu.RunescapeSpellbook_BarrowsCasket",1,0.07f),
        } },
        { "Magma Duggy", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureFirePack",2,0.3f),
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureElemental",5,0.3f),
            new ItemDrop("Tofu.RunescapeSpellbook_BarrowsCasket",1,0.07f),
        } },
        { "Magma Sparker", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureFirePack",3,0.3f),
        } },
        { "False Magma Cap", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureEarthPack",1,0.2f),
            new ItemDrop("Tofu.RunescapeSpellbook_RuneCosmic",10,0.2f),
        } },
    };

    //Items to be put in shops
    public static Dictionary<string, List<ShopListings>> loadableShops = new()
    {
        {"AdventureShop", new()
        {
            new ShopListings("RS_Marlon_Battlestaff","(W)Tofu.RunescapeSpellbook_StaffMagic",2000,2,-1,-1,"PLAYER_HAS_SEEN_EVENT Current Tofu.RunescapeSpellbook_Event0")
        }},
        {"DesertTrade", new()
        {
            new ShopListings("RS_Desert_AirRunes","(O)Tofu.RunescapeSpellbook_RuneAir","(O)60",1,4,40,40,"PLAYER_HAS_SEEN_EVENT Current Tofu.RunescapeSpellbook_Event0")
        }},
        {"Sandy", new()
        {
            new ShopListings("RS_Seed_Harralander","(O)Tofu.RunescapeSpellbook_SeedHarra",100,4,-1,-1,"PLAYER_HAS_SEEN_EVENT Current Tofu.RunescapeSpellbook_Event0, PLAYER_BASE_FARMING_LEVEL Current 3"),
            new ShopListings("RS_Seed_Lantadyme","(O)Tofu.RunescapeSpellbook_SeedLanta",300,5,-1,-1,"PLAYER_HAS_SEEN_EVENT Current Tofu.RunescapeSpellbook_Event0, PLAYER_BASE_FARMING_LEVEL Current 8"),
            new ShopListings("RS_Recipe_Hunter","(O)Tofu.RunescapeSpellbook_PotHunter",8000,6,-1,-1,"PLAYER_HAS_SEEN_EVENT Current Tofu.RunescapeSpellbook_Event0, PLAYER_BASE_FARMING_LEVEL Current 3",true),
            new ShopListings("RS_Recipe_Battlemage","(O)Tofu.RunescapeSpellbook_PotBattlemage",8000,7,-1,-1,"PLAYER_HAS_SEEN_EVENT Current Tofu.RunescapeSpellbook_Event0, PLAYER_BASE_FARMING_LEVEL Current 8",true),
        }},
        {"Blacksmith", new()
        {
            new ShopListings("RS_Recipe_Shredder","(BC)Tofu.RunescapeSpellbook_PackShredder",2500,0,-1,-1,"",true),
        }}
    };
    
    public static List<LoadableText> loadableText = new()
    {
        {
            new LoadableMail("Tofu.RunescapeSpellbook_SpellMail","mail.SpellMail.text")
        },
        {
            new LoadableMail(15,Season.Summer,1,"mail.ClintPackService.text")
        },
        {
            new LoadableMail(3,Season.Spring,2,"mail.WillyCasket.text")
        },
        {
            new LoadableMail(1,Season.Summer,3,"mail.ClintTerribleGift.text")
        },
        {
            new LoadableMail(9,Season.Spring,2,"mail.LinusStones.text")
        },
        {
            new LoadableMail(27,Season.Fall,2,"mail.SomeoneElse.text")
        },
        {
            new Gobcast(21,Season.Spring,1, new()
            {
                "tvbroadcast.Gobcast1.text-1","tvbroadcast.Gobcast1.text-2","tvchannel.Gobcast.outro"
            })
        },
        {  
            new Gobcast(7,Season.Fall,1,new()
            {
                "tvbroadcast.Gobcast2.text-1","tvbroadcast.Gobcast2.text-2","tvchannel.Gobcast.outro"
            })
        },
        {  
            new Gobcast(4,Season.Summer,2,new()
            {
                "tvbroadcast.Gobcast3.text-1","tvbroadcast.Gobcast3.text-2","tvchannel.Gobcast.outro"
            })
        },
        {  
            new Gobcast(24,Season.Fall,1,new()
            {
                "tvbroadcast.Gobcast4.text-1","tvchannel.Gobcast.outro"
            })
        },
        {  
            new Gobcast(2,Season.Winter,1,new()
            {
                "tvbroadcast.Gobcast5.text-1","tvchannel.Gobcast.outro"
            })
        },
        {  
            new Gobcast(3,Season.Spring,2,new()
            {
                "tvbroadcast.Gobcast6.text-1","tvbroadcast.Gobcast6.text-2","tvchannel.Gobcast.outro"
            })
        },
        {  
            new Gobcast(11,Season.Winter,1,new()
            {
                "tvbroadcast.Gobcast7.text-1","tvbroadcast.Gobcast7.text-2","tvbroadcast.Gobcast7.text-3","tvchannel.Gobcast.outro"
            })
        },
        {  
            new Gobcast(2,Season.Summer,3,new()
            {
                "tvbroadcast.Gobcast8.text-1","tvbroadcast.Gobcast8.text-2","tvchannel.Gobcast.outro"
            })
        },
        {  
            new Gobcast(22,Season.Winter,3,new()
            {
                "tvbroadcast.Gobcast9.text-1","tvbroadcast.Gobcast9.text-2","tvbroadcast.Gobcast9.text-3"
            })
        }
        
        
    };

    public static Dictionary<string, List<LoadableEvent>> loadableEvents = new()
    {
        {
            "Data/Events/Farm", new()
            {
                {
                    new LoadableEvent("Tofu.RunescapeSpellbook_Event0/f Wizard 1000/t 600 1200","event.GetSpellbook.data")
                }
            }
        },
        {
            "Data/Events/ArchaeologyHouse", new()
            {
                {
                    new LoadableEvent("Tofu.RunescapeSpellbook_Event1/n Tofu.RunescapeSpellbook_RunesFound","event.RunesFound.data")
                }
            }
        }
    };

    public static List<CustomBuff> loadableBuffs = new()
    {
        new CustomBuff("Tofu.RunescapeSpellbook_BuffCharge", "Charge", 60_000, 0),
        new CustomBuff("Tofu.RunescapeSpellbook_BuffDark", "DarkLure", 180_000, 1),
        new CustomBuff("Tofu.RunescapeSpellbook_BuffHunters", "Hunters", 300_000, 2),
        new CustomBuff("Tofu.RunescapeSpellbook_BuffBattlemage", "Battlemage", 540_000, 3),
    };
    public static bool CheckHasPerkByName(Farmer farmer,string perkName)
    {
        PerkData? perk = perks.FirstOrDefault(x => x.perkName == perkName);
        return perk == null ? false : perk.HasPerk(farmer);
    }
    
    private static MethodInfo cachedBroadcastMethod;
    private static MethodInfo cachedGlobalChatMethod;
    public static void Load(IModHelper helper)
    {
        extraTextures = helper.ModContent.Load<Texture2D>("assets\\modsprites"); 
        animTextures = helper.ModContent.Load<Texture2D>("assets\\spellanimations"); 
        
        multiplayer = helper.Reflection.GetField<object>(typeof(Game1), "multiplayer").GetValue();
        cachedBroadcastMethod = multiplayer.GetType().GetMethod("broadcastSprites", new[] { typeof(GameLocation), typeof(TemporaryAnimatedSprite[]) });
        cachedGlobalChatMethod = multiplayer.GetType().GetMethod("globalChatInfoMessage", new[] { typeof(string), typeof(string[]) });

        KeyTranslator.TranslationFunc = (key, replacementsSet) => helper.Translation.Get(key, replacementsSet);
        
        //Apply Translations to all objects
        var translationTargets = new IEnumerable<ITranslatable>[] { modItems.Values, modSpells, machineItems, perks,loadableText,loadableEvents.Values.SelectMany(z=>z),loadableBuffs  }.SelectMany(x => x);
        foreach (var obj in translationTargets)
        {
            obj.ApplyTranslations();
        }
        
        infiniteRuneReferences = new();
        //Generate the lookup dictionary for determining what weapons give infinite values for each rune
        foreach (StaffWeaponData weapon in staffWeapons)
        {
            weapon.ApplyTranslations();
            if (weapon.providesRune != null)
            {
                if (!infiniteRuneReferences.ContainsKey(weapon.providesRune))
                {
                    infiniteRuneReferences.Add(weapon.providesRune, new() { weapon.id.ToString() });
                }
                else
                {
                    infiniteRuneReferences[weapon.providesRune].Add(weapon.id.ToString());
                }
            }
        }
        
        localFarmerData = new PlayerLocalData();
    }

    public static void BroadcastSprite(GameLocation location, TemporaryAnimatedSprite sprite)
    {
        var spriteArray = new TemporaryAnimatedSprite[] { sprite };
        cachedBroadcastMethod.Invoke(multiplayer, new object[] { location, spriteArray });
    }
    
    public static void GlobalChatMessage(string messageKey, params string[] args)
    {
        cachedGlobalChatMethod.Invoke(multiplayer, new object[] { messageKey, args });
    }

    public static List<Farmer> GetFarmers()
    {
        List<Farmer> farmers = new();
        farmers.Add(Game1.player);
        foreach (Farmer value in Game1.otherFarmers.Values)
        {
            farmers.Add(value);
        }
        
        return farmers;
    }
    
    public static void SetupModDataKeys(Farmer farmerInstance)
    {
        if (!farmerInstance.modData.ContainsKey("Tofu.RunescapeSpellbook_MagicLevel"))
        {
            farmerInstance.modData.Add("Tofu.RunescapeSpellbook_MagicLevel", "0");
        }

        if (!farmerInstance.modData.ContainsKey("Tofu.RunescapeSpellbook_MagicExp"))
        {
            farmerInstance.modData.Add("Tofu.RunescapeSpellbook_MagicExp","0");
        }
        
        if (!farmerInstance.modData.ContainsKey("Tofu.RunescapeSpellbook_MagicProf1"))
        {
            farmerInstance.modData.Add("Tofu.RunescapeSpellbook_MagicProf1","-1");
        }
        
        if (!farmerInstance.modData.ContainsKey("Tofu.RunescapeSpellbook_MagicProf2"))
        {
            farmerInstance.modData.Add("Tofu.RunescapeSpellbook_MagicProf2","-1");
        }
        
        if (!farmerInstance.modData.ContainsKey("Tofu.RunescapeSpellbook_Setting-MagicExpMultiplier"))
        {
            farmerInstance.modData.Add("Tofu.RunescapeSpellbook_Setting-MagicExpMultiplier",GetSpellBaseExpMultiplier().ToString());
        }
    }
    public static string TryGetModVariable(Farmer farmer, string dataKey)
    {
        string retVal;
        if (!farmer.modData.TryGetValue(dataKey, out retVal))
        {
            SetupModDataKeys(farmer);
            farmer.modData.TryGetValue(dataKey, out retVal);
        }
        
        return retVal;
    }

    public static void TrySetModVariable(Farmer farmer, string dataKey, string newValue)
    {
        if (!farmer.modData.ContainsKey(dataKey))
        {
            SetupModDataKeys(farmer);
        }

        if (farmer.modData.ContainsKey(dataKey))
        {
            farmer.modData[dataKey] = newValue;
        }
    }
    public static bool HasMagic(Farmer farmer)
    {
        if (farmer.mailReceived.Contains("Tofu.RunescapeSpellbook_HasUnlockedMagic"))
        {
            return true;
        }
        
        if (farmer.eventsSeen.Contains("Tofu.RunescapeSpellbook_Event0"))
        {
            farmer.mailReceived.Add("Tofu.RunescapeSpellbook_HasUnlockedMagic");
            return true;
        }
        return false;
    }
    public static int GetFarmerMagicLevel(Farmer farmer)
    {
        int level = -1;
        int.TryParse(TryGetModVariable(farmer,"Tofu.RunescapeSpellbook_MagicLevel"),out level);
        return level;
    }
    
    public static double GetFarmerExperience(Farmer farmer)
    {
        double experience = -1;
        double.TryParse(TryGetModVariable(farmer,"Tofu.RunescapeSpellbook_MagicExp"),out experience);
        return experience;
    }

    public static void IncrementMagicExperience(Farmer farmer, double gainedExperience, bool shouldUseMultiplier = true)
    {
        double experience = GetFarmerExperience(farmer);
        
        if (experience != -1 && experience <= Farmer.getBaseExperienceForLevel(10)) //If our exp should still be tracked then increment it
        {
            int expMultiplier = 100;
            int.TryParse(TryGetModVariable(Game1.player, "Tofu.RunescapeSpellbook_Setting-MagicExpMultiplier"),
                out expMultiplier);
            double multiplier = shouldUseMultiplier ? expMultiplier / 100.0 : 1.0;
            double newTotalExperience = (experience + (gainedExperience * multiplier));
            
            TrySetModVariable(farmer,"Tofu.RunescapeSpellbook_MagicExp",Math.Round(newTotalExperience,4).ToString());
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

                TrySetModVariable(farmer, "Tofu.RunescapeSpellbook_MagicLevel", (currentLevel).ToString());
                Game1.player.playNearbySoundLocal("RunescapeSpellbook.MagicLevel");
            }
            
            switch (messageTier)
            {
                case 1:
                    Game1.addHUDMessage(new HUDMessage(KeyTranslator.GetTranslation("ui.LevelUpNormal.text"), 2));
                    break;
                case 2:
                    Game1.addHUDMessage(new HUDMessage(KeyTranslator.GetTranslation("ui.LevelUpProfession.text"), 2));
                    break;
            }
        }
    }
    public static List<int> PerksAssigned(Farmer farmer)
    {
        List<int> perkIDs = new();
        int id1 = -1;
        int.TryParse(TryGetModVariable(farmer,"Tofu.RunescapeSpellbook_MagicProf1"),out id1);
        if (id1 != -1)
        {
            perkIDs.Add(id1);
        }
        
        int id2 = -1;
        int.TryParse(TryGetModVariable(farmer,"Tofu.RunescapeSpellbook_MagicProf2"),out id2);
        if (id2 != -1)
        {
            perkIDs.Add(id2);
        }
        
        return perkIDs;
    }

    public static bool HasPerk(Farmer farmer, int perkID)
    {
        int id1 = -1;
        int.TryParse(TryGetModVariable(farmer,"Tofu.RunescapeSpellbook_MagicProf1"),out id1);
        int id2 = -1;
        int.TryParse(TryGetModVariable(farmer,"Tofu.RunescapeSpellbook_MagicProf2"),out id2);
        
        return (perkID == id1 || perkID == id2);
    }

    public static bool GrantPerk(Farmer farmer, int perkID)
    {
        int id1 = -1;
        int.TryParse(TryGetModVariable(farmer,"Tofu.RunescapeSpellbook_MagicProf1"),out id1);
        int id2 = -1;
        int.TryParse(TryGetModVariable(farmer,"Tofu.RunescapeSpellbook_MagicProf2"),out id2);

        if (id1 == perkID || id2 == perkID)
        {
            return false;
        }
        
        bool successfulAssignment = false;
        if (id1 == -1)
        {
            TrySetModVariable(farmer, "Tofu.RunescapeSpellbook_MagicProf1", perkID.ToString());
            successfulAssignment = true;
        }
        else if (id2 == -1)
        {
            TrySetModVariable(farmer, "Tofu.RunescapeSpellbook_MagicProf2", perkID.ToString());
            successfulAssignment = true;
        }
        
        return successfulAssignment;
    }
}

public class PlayerLocalData
{
    public int selectedSpellID;
    public int bonusHealth;
    public PlayerLocalData()
    {
        selectedSpellID = -1;
        bonusHealth = 0;
    }
    public void Reset()
    {
        selectedSpellID = -1;
        bonusHealth = 0;
    }
    
}

public sealed class ModConfig
{
    public KeybindList SpellbookKey = KeybindList.Parse("J");
    public bool LockSpellbookStyle = false;
    public string SpellbookTabStyle = "Tab and Keybind";
    public int SpellBaseExpMultiplier = 100;
}