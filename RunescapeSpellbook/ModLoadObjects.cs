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
    
    public static Dictionary<string,ModLoadObjects> modItems = new(){
        {"Tofu.RunescapeSpellbook_RuneSpellbook",new RunesObjects("Tofu.RunescapeSpellbook_RuneSpellbook","Spellbook","Debug object.",0,-999)},
        {"Tofu.RunescapeSpellbook_RuneAir",new RunesObjects("Tofu.RunescapeSpellbook_RuneAir","Air Rune","One of the 4 basic elemental Runes",1,-429,
            new Dictionary<string, PrefType>(){{"Emily",PrefType.Neutral}})},
        {"Tofu.RunescapeSpellbook_RuneWater",new RunesObjects("Tofu.RunescapeSpellbook_RuneWater","Water Rune","One of the 4 basic elemental Runes",2,-429,
            new(){{"Emily",PrefType.Neutral},{"Willy",PrefType.Neutral},{"Elliott",PrefType.Neutral}})},
        {"Tofu.RunescapeSpellbook_RuneFire",new RunesObjects("Tofu.RunescapeSpellbook_RuneFire","Fire Rune","One of the 4 basic elemental Runes",3,-429,
            new(){{"Emily",PrefType.Neutral},{"Sam",PrefType.Neutral},{"Vincent",PrefType.Neutral}})},
        {"Tofu.RunescapeSpellbook_RuneEarth",new RunesObjects("Tofu.RunescapeSpellbook_RuneEarth","Earth Rune","One of the 4 basic elemental Runes",4,-429,
            new(){{"Emily",PrefType.Neutral},{"Dwarf",PrefType.Neutral},{"Demetrius",PrefType.Neutral}})},
        {"Tofu.RunescapeSpellbook_RuneLaw",new RunesObjects("Tofu.RunescapeSpellbook_RuneLaw","Law Rune","Used for teleport spells",5,-431,
            new(){{"Emily",PrefType.Neutral},{"Wizard",PrefType.Like}})},
        {"Tofu.RunescapeSpellbook_RuneNature",new RunesObjects("Tofu.RunescapeSpellbook_RuneNature","Nature Rune","Used for alchemy spells",6,-431,
            new(){{"Emily",PrefType.Neutral},{"Leo",PrefType.Neutral},{"Linus",PrefType.Neutral},{"Wizard",PrefType.Neutral}})},
        {"Tofu.RunescapeSpellbook_RuneCosmic",new RunesObjects("Tofu.RunescapeSpellbook_RuneCosmic","Cosmic Rune","Used for enchant spells",7,-431,
            new(){{"Emily",PrefType.Neutral},{"Maru",PrefType.Like},{"Wizard",PrefType.Neutral}})},
        {"Tofu.RunescapeSpellbook_RuneAstral",new RunesObjects("Tofu.RunescapeSpellbook_RuneAstral","Astral Rune","Used for Lunar spells",8,-431,
            new(){{"Emily",PrefType.Like},{"Maru",PrefType.Neutral},{"Wizard",PrefType.Neutral}})},
        {"Tofu.RunescapeSpellbook_RuneChaos",new RunesObjects("Tofu.RunescapeSpellbook_RuneChaos","Chaos Rune","Used for low level combat spells",9,-430,
            new(){{"Emily",PrefType.Hate},{"Kent",PrefType.Hate},{"Wizard",PrefType.Neutral}})},
        {"Tofu.RunescapeSpellbook_RuneDeath",new RunesObjects("Tofu.RunescapeSpellbook_RuneDeath","Death Rune","Used for high level combat spells",10,-430,
            new(){{"Sebastian",PrefType.Like},{"Emily",PrefType.Hate},{"George",PrefType.Hate},{"Evelyn",PrefType.Hate},{"Wizard",PrefType.Neutral}})},

        {"Tofu.RunescapeSpellbook_AmmoFire",new SlingshotItem("Tofu.RunescapeSpellbook_AmmoFire","Fire Orb","Enchanted ammo that burns enemies in a radius around a hit enemy. Fire cannot finish off enemies.",30,15,2)},
        {"Tofu.RunescapeSpellbook_AmmoEarth",new SlingshotItem("Tofu.RunescapeSpellbook_AmmoEarth","Earth Orb","Enchanted ammo that explodes and poisons enemies in a radius around a hit enemy. Poison cannot finish off enemies.",31,25,1,true)},
        
        {"Tofu.RunescapeSpellbook_TreasureElemental",new TreasureObjects("Tofu.RunescapeSpellbook_TreasureElemental","Elemental Geode","Contains some elemental Runes. A blacksmith might be able to open it.",19,
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
        
        {"Tofu.RunescapeSpellbook_TreasureCatalytic",new TreasureObjects("Tofu.RunescapeSpellbook_TreasureCatalytic","Catalytic Geode","Contains some catalytic Runes. A blacksmith might be able to open it.",20,
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
        
        {"Tofu.RunescapeSpellbook_EasyCasket", new TreasureObjects("Tofu.RunescapeSpellbook_EasyCasket","Low Level Casket","Contains some magical goodies. A blacksmith might be able to open it.",21,
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
        
        {"Tofu.RunescapeSpellbook_HardCasket",new TreasureObjects("Tofu.RunescapeSpellbook_HardCasket","High Level Casket","Contains some valuable magical goodies. A blacksmith might be able to open it.",22,
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
        
        {"Tofu.RunescapeSpellbook_BarrowsCasket",new TreasureObjects("Tofu.RunescapeSpellbook_BarrowsCasket","Barrows Casket","Contains some very valuable magical goodies. A blacksmith might be able to open it.",23,
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
        
        {"Tofu.RunescapeSpellbook_TreasureAirPack",new PackObject("Tofu.RunescapeSpellbook_TreasureAirPack","Air Rune Pack","A pack containing many air Runes. A blacksmith might be able to open it.",24,"Tofu.RunescapeSpellbook_RuneAir",3)},
        {"Tofu.RunescapeSpellbook_TreasureWaterPack",new PackObject("Tofu.RunescapeSpellbook_TreasureWaterPack","Water Rune Pack","A pack containing many water Runes. A blacksmith might be able to open it.",25,"Tofu.RunescapeSpellbook_RuneWater",1)},
        {"Tofu.RunescapeSpellbook_TreasureFirePack",new PackObject("Tofu.RunescapeSpellbook_TreasureFirePack","Fire Rune Pack","A pack containing many fire Runes. A blacksmith might be able to open it.",26,"Tofu.RunescapeSpellbook_RuneFire")},
        {"Tofu.RunescapeSpellbook_TreasureEarthPack",new PackObject("Tofu.RunescapeSpellbook_TreasureEarthPack","Earth Rune Pack","A pack containing many earth Runes. A blacksmith might be able to open it.",27,"Tofu.RunescapeSpellbook_RuneEarth")},
        {"Tofu.RunescapeSpellbook_TreasureChaosPack",new PackObject("Tofu.RunescapeSpellbook_TreasureChaosPack","Chaos Rune Pack","A pack containing many chaos Runes. A blacksmith might be able to open it.",28,"Tofu.RunescapeSpellbook_RuneChaos")},
        {"Tofu.RunescapeSpellbook_TreasureDeathPack",new PackObject("Tofu.RunescapeSpellbook_TreasureDeathPack","Death Rune Pack","A pack containing many death Runes. A blacksmith might be able to open it.",29,"Tofu.RunescapeSpellbook_RuneDeath")},
        
        {"Tofu.RunescapeSpellbook_FishKaram",new FishObject("Tofu.RunescapeSpellbook_FishKaram","Karambwanji","A small brightly coloured tropical fish. Traditionally associated with elemental magic",32,45,600,1800,new(){Season.Spring,Season.Summer},"sunny",new()
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
        {"Tofu.RunescapeSpellbook_FishMonk",new FishObject("Tofu.RunescapeSpellbook_FishMonk","Monkfish","An anglerfish known for its toothy smile. Traditionally associated with combat magic",33,60,1600,2300,new(){Season.Fall,Season.Winter,Season.Spring},"rainy",new()
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
        {"Tofu.RunescapeSpellbook_FishManta",new FishObject("Tofu.RunescapeSpellbook_FishManta","Manta Ray","A large and intelligent fish that feeds on plankton. Traditionally associated with catalytic magic",34,75,1200,1800,new(){Season.Summer},"sunny",new()
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
        {"Tofu.RunescapeSpellbook_FishSword",new FishObject("Tofu.RunescapeSpellbook_FishSword","Swordfish","A predatory fish with a flat sword-like pointed bill",35,95,2000,2600,new(){Season.Spring,Season.Summer,Season.Fall,Season.Winter},"both",new()
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
        {"Tofu.RunescapeSpellbook_SeedHarra", new SeedObject("Tofu.RunescapeSpellbook_SeedHarra","Harralander Seed","Plant these in the fall. Takes 12 days to mature.",36,50)},
        {"Tofu.RunescapeSpellbook_CropHarra",new CropObject("Tofu.RunescapeSpellbook_CropHarra","Harralander","A herb that naturally grows in rocky crevices, named for its destructive nature.",
            "Tofu.RunescapeSpellbook_SeedHarra",new(){Season.Fall},3,0,37,105,-50,"color_green",-75,1,0.25f,
            new(){{"Emily",PrefType.Like}})},
        {"Tofu.RunescapeSpellbook_SeedLanta", new SeedObject("Tofu.RunescapeSpellbook_SeedLanta","Lantadyme Seed","Plant these in the winter. Takes 12 days to mature.",38,50)},
        {"Tofu.RunescapeSpellbook_CropLanta",new CropObject("Tofu.RunescapeSpellbook_CropLanta","Lantadyme","A herb that is said to resemble eyes when in bloom",
            "Tofu.RunescapeSpellbook_SeedLanta",new(){Season.Winter},3,1,39,110,-50,"color_blue",-75,1,0.25f,
            new(){{"Emily",PrefType.Love}})},
        {"Tofu.RunescapeSpellbook_PotGuthix", new PotionObject("Tofu.RunescapeSpellbook_PotGuthix","Guthix Rest","A relaxing cup of tea that restores some health. Can heal over your maximum health.",40,400,0.3f,0.15f,"Tofu.RunescapeSpellbook_CropHarra",5000,
            new(){{"Caroline",PrefType.Love},{"Lewis",PrefType.Love},{"Sandy",PrefType.Love},{"Harvey",PrefType.Like},{"Jas",PrefType.Hate},{"Vincent",PrefType.Hate}})},
        {"Tofu.RunescapeSpellbook_PotSara", new PotionObject("Tofu.RunescapeSpellbook_PotSara","Saradomin Brew","A relaxing cup of tea that restores a lot of health. Can heal over your maximum health.",44,550,0.6f,0.25f,"Tofu.RunescapeSpellbook_CropLanta",6000,
            new(){{"Caroline",PrefType.Love},{"Lewis",PrefType.Love},{"Sandy",PrefType.Love},{"Harvey",PrefType.Like},{"Jas",PrefType.Hate},{"Vincent",PrefType.Hate}})},
        {"Tofu.RunescapeSpellbook_PotHarraDye", new PotionObject("Tofu.RunescapeSpellbook_PotHarraDye","Harralander Dye","A green dye made from harralander. Prized by artists.",41,250,"Tofu.RunescapeSpellbook_CropHarra",4000,"color_green",
            new(){{"Emily",PrefType.Love},{"Elliott",PrefType.Love},{"Leah",PrefType.Like}})},
        {"Tofu.RunescapeSpellbook_PotLantaDye", new PotionObject("Tofu.RunescapeSpellbook_PotLantaDye","Lantadyme Dye","A blue dye made from lantadyme. Prized by artists.",45,350,"Tofu.RunescapeSpellbook_CropLanta",4000,"color_blue",
            new(){{"Emily",PrefType.Love},{"Elliott",PrefType.Love},{"Sebastian",PrefType.Love},{"Leah",PrefType.Like}})},
        {"Tofu.RunescapeSpellbook_PotHunter", new PotionObject("Tofu.RunescapeSpellbook_PotHunter","Hunter Potion","A potion that imbues the user with the power of a hunter. Adds a second smaller bar to fishing that gives 1.5x progress",43,300,"Tofu.RunescapeSpellbook_CropHarra 1 300 1 881 5",new(){"Tofu.RunescapeSpellbook_BuffHunters"},
            new(){{"Willy",PrefType.Like},{"Linus",PrefType.Like},{"Penny",PrefType.Hate}})},
        {"Tofu.RunescapeSpellbook_PotBattlemage", new PotionObject("Tofu.RunescapeSpellbook_PotBattlemage","Battlemage Potion","A potion that imbues the user with the power of a battlemage. Combat spells have a 10% chance of costing no runes. Spells cast with no cost grant no experience.",46,500,"Tofu.RunescapeSpellbook_CropLanta 1 807 1 90 3",new(){"Tofu.RunescapeSpellbook_BuffBattlemage"},
            new(){{"Wizard",PrefType.Love}})},
    };
    
    //These are custom melee weapons that use 
    public static readonly StaffWeaponData[] staffWeapons =
    {
        new StaffWeaponData("Tofu.RunescapeSpellbook_StaffMagic", "Magic Staff", "A magical battlestaff", 5, 10, 11),
        new StaffWeaponData("Tofu.RunescapeSpellbook_StaffAir", "Staff of Air",
            "A magical battlestaff imbued with weak air magic. Provides air runes for combat spells.", 20, 30, 12,
            11,1.15f, "Tofu.RunescapeSpellbook_RuneAir"),
        new StaffWeaponData("Tofu.RunescapeSpellbook_StaffWater",  "Staff of Water",
            "A magical battlestaff imbued with weak water magic. Provides water runes for combat spells.", 20, 30, 13,
            11,1.15f, "Tofu.RunescapeSpellbook_RuneWater"),
        new StaffWeaponData("Tofu.RunescapeSpellbook_StaffEarth",  "Staff of Earth",
            "A magical battlestaff imbued with weak earth magic. Provides earth runes for combat spells.", 20, 30, 14,
            11,1.15f, "Tofu.RunescapeSpellbook_RuneEarth"),
        new StaffWeaponData("Tofu.RunescapeSpellbook_StaffFire", "Staff of Fire",
            "A magical battlestaff imbued with weak fire magic. Provides fire runes for combat spells.", 20, 30, 15,
            11,1.15f, "Tofu.RunescapeSpellbook_RuneFire"),
        
        new StaffWeaponData("Tofu.RunescapeSpellbook_StaffMysticAir", "Mystic Air Staff",
            "A magical battlestaff imbued with strong air magic. Provides air runes for combat spells.", 25, 35, 48,
            12,1.25f, "Tofu.RunescapeSpellbook_RuneAir"),
        new StaffWeaponData("Tofu.RunescapeSpellbook_StaffMysticWater",  "Mystic Water Staff",
            "A magical battlestaff imbued with strong water magic. Provides water runes for combat spells.", 25, 35, 49,
            12,1.25f, "Tofu.RunescapeSpellbook_RuneWater"),
        new StaffWeaponData("Tofu.RunescapeSpellbook_StaffMysticEarth",  "Mystic Earth Staff",
            "A magical battlestaff imbued with strong earth magic. Provides earth runes for combat spells.", 25, 35, 50,
            12,1.25f, "Tofu.RunescapeSpellbook_RuneEarth"),
        new StaffWeaponData("Tofu.RunescapeSpellbook_StaffMysticFire", "Mystic Fire Staff",
            "A magical battlestaff imbued with strong fire magic. Provides fire runes for combat spells.", 25, 35, 51,
            12,1.25f, "Tofu.RunescapeSpellbook_RuneFire"),
        new StaffWeaponData("Tofu.RunescapeSpellbook_StaffAncient",  "Ancient Staff", "A magical battlestaff of ancient origin...",
            25, 40, 16,
            13,1.4f,"",0,0,0,0.05f),
        
        
        new StaffWeaponData("Tofu.RunescapeSpellbook_StaffAhrims", "Ahrims Staff", "Ahrim the Blighted's quarterstaff", 30, 45, 17,
            15,1.6f),
        new StaffWeaponData("Tofu.RunescapeSpellbook_StaffBlueMoon", "Blue Moon Spear",
            "An ancient battlestaff that doubles as a spear", 70, 90, 18,
            15,1.5f)
    };

    //Machines
    public static readonly MachinesObject[] machineItems =
    {
        new MachinesObject("Tofu.RunescapeSpellbook_PackShredder","Pack Shredder","Shreds three rune packs at once in exchange for coal",100,0,
            new()
            {
                {"Tofu.RunescapeSpellbook_TreasureAirPack","Tofu.RunescapeSpellbook_RuneAir"},
                {"Tofu.RunescapeSpellbook_TreasureWaterPack","Tofu.RunescapeSpellbook_RuneWater"},
                {"Tofu.RunescapeSpellbook_TreasureFirePack","Tofu.RunescapeSpellbook_RuneFire"},
                {"Tofu.RunescapeSpellbook_TreasureEarthPack","Tofu.RunescapeSpellbook_RuneEarth"},
                {"Tofu.RunescapeSpellbook_TreasureChaosPack","Tofu.RunescapeSpellbook_RuneChaos"},
                {"Tofu.RunescapeSpellbook_TreasureDeathPack","Tofu.RunescapeSpellbook_RuneDeath"},
            }, 3,new Func<string,List<ItemDrop>>((itemID)=>((PackObject)modItems[itemID]).GetItemRanges()),"336 1 338 2 709 5",3,"(O)382","I don't have enough coal for this"
            )
    };
    
    //This dictionary provides a quick reference for which weapons provide what rune
    public static Dictionary<string, List<string>> infiniteRuneReferences;
    
    public static readonly Spell[] modSpells = {
        new TeleportSpell(0,"Teleport_Valley","Valley Teleport","Teleports you to Pierre's Store in Pelican Town",0,
            new() { {"Tofu.RunescapeSpellbook_RuneLaw", 1},{"Tofu.RunescapeSpellbook_RuneAir",3},{"Tofu.RunescapeSpellbook_RuneWater",2} },8,"Town", 43, 60,0),
        
        new TeleportSpell(1,"Teleport_Home","Farm Teleport","Teleports you outside the main house on your farm",4,
            new() { {"Tofu.RunescapeSpellbook_RuneLaw", 1},{"Tofu.RunescapeSpellbook_RuneAir",3},{"Tofu.RunescapeSpellbook_RuneEarth",3} },10, "FarmHouse"),
        
        new InventorySpell(2,"Menu_Superheat","Superheat Item","Smelts ore without a furnace or coal, or burns wood into coal at a discount",1,
            new() { {"Tofu.RunescapeSpellbook_RuneNature", 1},{"Tofu.RunescapeSpellbook_RuneFire",4}},10,
            (i=>i is Item item && (item.QualifiedItemId == "(O)388" || DataLoader.Machines(Game1.content).GetValueOrDefault("(BC)13").OutputRules.Any(x=>x.Triggers.Any(y=>y.RequiredItemId == item.QualifiedItemId)))),
            SpellEffects.SuperheatItem,"Smelt any ores into bars instantly without any coal cost, or smelt wood into coal. Put an appropriate item in the slot and press the spell icon to cast.",1,"Superheat"),
        
        new InventorySpell(3,"Menu_HighAlch","High Level Alchemy","Converts an item into 1.5x its sell price",8,
            new() { {"Tofu.RunescapeSpellbook_RuneNature", 2},{"Tofu.RunescapeSpellbook_RuneFire",5}},15,(i=>i is Item item && item.canBeShipped() && item.salePrice(false) > 0 && item.Category != -429 && item.Category != -430 && item.Category != -431),
            SpellEffects.HighAlchemy,"Turn any sellable item into money. Provides 150% of the items value. Put an appropriate item in the slot and press the spell icon to cast.",0,"HighAlch"),
        
        new TilesSpell(4,"Area_Humidify","Humidify","Waters the ground around you",1,
            new() { {"Tofu.RunescapeSpellbook_RuneAstral", 1},{"Tofu.RunescapeSpellbook_RuneFire",1},{"Tofu.RunescapeSpellbook_RuneWater",3}}, 0.4f,SpellEffects.Humidify, 10,5,"Humidify",
            (tile => tile is HoeDirt hoeLand && (hoeLand.crop == null || !hoeLand.crop.forageCrop.Value || hoeLand.crop.whichForageCrop.Value != "2") && hoeLand.state.Value != 1)),
        
        new TilesSpell(5,"Area_Cure","Cure Plant","Replants dead crops",6,
            new() { {"Tofu.RunescapeSpellbook_RuneAstral", 1},{"Tofu.RunescapeSpellbook_RuneEarth",8}},0.6f, SpellEffects.CurePlant, 10,6,"Cure",
            (tile => tile is HoeDirt hoeLand && hoeLand.crop != null && hoeLand.crop.dead.Value)),
        
        new BuffSpell(6,"Buff_VileVigour","Vile Vigour","Sacrifices a third of your max health to fill your energy",3,
            new() { {"Tofu.RunescapeSpellbook_RuneCosmic", 1},{"Tofu.RunescapeSpellbook_RuneAir",3}},5, (f=> f is Farmer farmer && farmer.stamina < farmer.MaxStamina), SpellEffects.VileVigour,
            7,"Vile","My energy is already full"),
        
        new BuffSpell(7,"Buff_PieMake","Bake Pie","Cooks a random recipe that you know using your held ingredients",3,
            new() { {"Tofu.RunescapeSpellbook_RuneAstral", 1},{"Tofu.RunescapeSpellbook_RuneFire",1},{"Tofu.RunescapeSpellbook_RuneWater",1}}, 15,
            (f=> f is Farmer farmer && farmer.cookingRecipes.Length > 0), SpellEffects.BakePie, 8,"BakePie","I don't know enough recipes"),
        
        new TeleportSpell(8,"Teleport_Desert","Desert Teleport","Teleports you to the desert, if you have access to it",5,
            new() { {"Tofu.RunescapeSpellbook_RuneLaw", 2},{"Tofu.RunescapeSpellbook_RuneEarth",5},{"Tofu.RunescapeSpellbook_RuneFire",5}}, 15,"Desert", 19, 34,2,
            ((farmer => Game1.MasterPlayer.mailReceived.Contains("ccVault")))),
        
        new TeleportSpell(9,"Teleport_Ginger","Ginger Island Teleport","Teleports you to ginger island, if you have access to it",7,
            new() { {"Tofu.RunescapeSpellbook_RuneLaw", 2},{"Tofu.RunescapeSpellbook_RuneWater",5},{"Tofu.RunescapeSpellbook_RuneFire",5}},15, "IslandSouth",21,37,0,
            ((farmer => Game1.MasterPlayer.hasOrWillReceiveMail("willyBoatFixed")))),
        
        new TeleportSpell(10,"Teleport_Caves","Caves Teleport","Teleports you to the pelican town mines",2,
            new() { {"Tofu.RunescapeSpellbook_RuneLaw", 1},{"Tofu.RunescapeSpellbook_RuneAir",5}},10, "Mountain",54,7,0, 
            ((farmer => Game1.MasterPlayer.hasOrWillReceiveMail("landslideDone")))),
        
        new InventorySpell(11,"Menu_EnchantRuby","Enchant Ruby Bolt","Convert any red or orange stones into fiery ammo",4,
            new() { {"Tofu.RunescapeSpellbook_RuneCosmic", 1},{"Tofu.RunescapeSpellbook_RuneFire",3}},10,(i => i is Item item && SpellEffects.redGemsEnchants.ContainsKey(item.ItemId)),SpellEffects.EnchantRubyBolts,
            "Convert any red gems or rocks into fiery ammo for the slingshot. On hitting an enemy, fire spreads to nearby enemies. Fire cannot finish off enemies.",2,"EnchantBolt"),
        
        new InventorySpell(12,"Menu_EnchantEmerald","Enchant Emerald Bolt","Convert any green stones into explosive poisonous ammo",8,
            new() { {"Tofu.RunescapeSpellbook_RuneCosmic", 2},{"Tofu.RunescapeSpellbook_RuneEarth",3}},15,(i => i is Item item && SpellEffects.greenGemsEnchants.ContainsKey(item.ItemId)),SpellEffects.EnchantEmeraldBolt,
            "Convert any green gems into explosive poisonous ammo for the slingshot. On hitting an enemy, poison spreads to nearby enemies. Poison cannot finish off enemies.",2,"EnchantBolt"),
        
        new BuffSpell(13,"Buff_DarkLure","Dark Lure","Summons more enemies, and makes them prioritise you over other farmers for 3 minutes",6,
            new() { {"Tofu.RunescapeSpellbook_RuneNature", 3},{"Tofu.RunescapeSpellbook_RuneDeath",3},{"Tofu.RunescapeSpellbook_RuneAir",3}},10,(f=> f is Farmer farmer && !farmer.hasBuff("Tofu.RunescapeSpellbook_BuffDark")),SpellEffects.DarkLure, 9,"DarkLure","I'm already luring monsters!"),
        
        new CombatSpell(14,"Combat_Wind","Wind Strike","A basic air missile",0,
            new() { {"Tofu.RunescapeSpellbook_RuneChaos", 1},{"Tofu.RunescapeSpellbook_RuneAir",1}}, 1,40,15,0,Color.White,"WindStrike"),
       
        new CombatSpell(15,"Combat_Water","Water Bolt","A low level water missile",2,
            new() { {"Tofu.RunescapeSpellbook_RuneChaos", 2},{"Tofu.RunescapeSpellbook_RuneAir",2},{"Tofu.RunescapeSpellbook_RuneWater",2}},2, 70,16,1,Color.DarkCyan,"WaterBolt"),
        
        new CombatSpell(16,"Combat_Undead","Crumble Undead","Hits undead monsters for extra damage",4,
            new() { {"Tofu.RunescapeSpellbook_RuneChaos", 2},{"Tofu.RunescapeSpellbook_RuneAir",2},{"Tofu.RunescapeSpellbook_RuneEarth",2}},4, 60,13,3,Color.Yellow,"CrumbleUndead",SpellEffects.DealUndeadDamage),
        
        new CombatSpell(17,"Combat_Earth","Earth Blast","A medium level earth missile",6,
            new() { {"Tofu.RunescapeSpellbook_RuneDeath", 1},{"Tofu.RunescapeSpellbook_RuneAir",3},{"Tofu.RunescapeSpellbook_RuneEarth",3}},4, 90,16,1,Color.DarkGreen,"EarthBlast"),
        
        new CombatSpell(18,"Combat_Fire","Fire Wave","A high level fire missile",8,
            new() { {"Tofu.RunescapeSpellbook_RuneDeath", 2},{"Tofu.RunescapeSpellbook_RuneAir",5},{"Tofu.RunescapeSpellbook_RuneFire",4}},5, 120,15,2,Color.OrangeRed,"FireWave"),
        
        new BuffSpell(19,"Buff_Charge","Charge","Spells cast three projectiles for 60 seconds",7,
            new() { {"Tofu.RunescapeSpellbook_RuneDeath", 3},{"Tofu.RunescapeSpellbook_RuneAir",3},{"Tofu.RunescapeSpellbook_RuneFire",3}},10,(f=> f is Farmer farmer && !farmer.hasBuff("Tofu.RunescapeSpellbook_BuffCharge")),SpellEffects.Charge, 10,"Charge","I'm already charged!"),
        
        new CombatSpell(20,"Combat_Demonbane","Demonbane","Hits undead monsters for a lot of extra damage",9,
            new() { {"Tofu.RunescapeSpellbook_RuneDeath", 2},{"Tofu.RunescapeSpellbook_RuneAstral",2},{"Tofu.RunescapeSpellbook_RuneFire",4}},6, 100,13,3,Color.Purple,"CrumbleUndead",SpellEffects.DealDemonbaneDamage),
        
        new CombatSpell(21,"Combat_Blood","Blood Barrage","Fires a strong vampiric blood missile",10,
            new() { {"Tofu.RunescapeSpellbook_RuneDeath", 4},{"Tofu.RunescapeSpellbook_RuneCosmic",3}}, 10,100,15,1,Color.Crimson, "BloodBarrage",SpellEffects.DealVampiricDamage),
        
        new InventorySpell(22,"Menu_Plank","Plank Make","Turns wood into hardwood and vice versa and uncrafts wooden items into wood",3,
            new() { {"Tofu.RunescapeSpellbook_RuneAstral", 1},{"Tofu.RunescapeSpellbook_RuneCosmic",1}},10,
            (i => i is Item item && (item.itemId.Value == "388" || item.itemId.Value == "709" || 
                                     (CraftingRecipe.craftingRecipes.ContainsKey(item.Name) 
                                      && CraftingRecipe.craftingRecipes[item.Name].Split(' ').ToList() is List<string> recipes 
                                      && ((recipes.IndexOf("388") != -1 && (recipes.IndexOf("388") + 1) % 2 != 0) || (recipes.IndexOf("709") != -1 && recipes.IndexOf("709") + 1 % 2 != 0) ) )))
            ,SpellEffects.PlankMake,
            "Breaks down wooden items into wood, and converts 15 wood into 1 hardwood and vice versa. For recipes that require more than wood, it will only return the wood.",3,"Degrime"),
        
        new InventorySpell(23,"Menu_LowAlch","Low Level Alchemy","Converts an item into 1.1x its sell price",5,
            new() { {"Tofu.RunescapeSpellbook_RuneNature", 1},{"Tofu.RunescapeSpellbook_RuneFire",3}},10,(i=>i is Item item && item.canBeShipped() && item.salePrice(false) > 0 && item.Category != -429 && item.Category != -430 && item.Category != -431),
            SpellEffects.LowAlchemy,"Turn any sellable item into money. Provides 110% of the items value. Put an appropriate item in the slot and press the spell icon to cast.",0,"HighAlch"),

    };
    
    public static readonly List<PerkData> perks = new()
    {
        new PerkData(0,"Sapphire","Sapphire","All teleportation spells are free","Teleportation spells no longer grant experience"),
        new PerkData(1,"Emerald","Emerald","All spells no longer require air runes"),
        new PerkData(2,"Ruby","Ruby","20% chance of non-combat spells taking no runes"),
        new PerkData(3,"Dragonstone","Dragonstone","20% chance of combat spells firing extra projectiles","Does not stack with charge, charge takes precedent")
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
            new LoadableMail("Tofu.RunescapeSpellbook_SpellMail","Dear @,^^I had forgotten one last thing about runic magic. Combat spells require a focus. In layman's terms, a battlestaff." +
              "^I've included one with this letter, and warned the mailcarrier of the consequences if you do not receive it in one piece. " +
              "^^   -M. Rasmodius, Wizard[letterbg 2]" +
              "%item object Tofu.RunescapeSpellbook_StaffMagic 1 %%" +
              "[#]Wizard's Battlestaff Gift")
        },
        {
            new LoadableMail(15,Season.Summer,1,"@,^Have you come across some strange packages in the mines lately? They seem to be full of those weird painted rocks that Emily likes." +
                "^^They're pretty hard to open, but my geode hammer seems to do the trick. If you find any, swing by and I'll help you open it" +
                "^^   -Clint^^P.S I've included some samples with this letter" +
                "%item object Tofu.RunescapeSpellbook_TreasureAirPack 3 %%" +
                "[#]Clint's Pack Opening Service")
        },
        {
            new LoadableMail(3,Season.Spring,2,"Ahoy @,^This was floating around in the ocean so I fished it up, some people have no respect for the seas." +
            "^^It seems like something ya might get some use out of, it'd make some fine firewood!" +
            "^^   -Willy" +
            "%item object Tofu.RunescapeSpellbook_EasyCasket 1 %%" +
            "[#]Willy's Casket")
        },
        {
            new LoadableMail(1,Season.Summer,3,"@,^I sent some of these to Emily as an anonymous gift but came in yesterday and sold them to my shop.^^She said the design made her uncomfortable." +
             "^^Maybe you'll get something out of them." +
             "^^   -Clint" +
             "%item object Tofu.RunescapeSpellbook_RuneDeath 60 %%" +
             "[#]Clint's Terrible Gift")
        },
        {
            new LoadableMail(9,Season.Spring,2,"@,^An old friend gave me some of these, but I don't have enough space to keep all of them." +
             "^^I hope you'll think of the great outdoors when you use them." +
             "^^   -Linus" +
             "%item object Tofu.RunescapeSpellbook_RuneNature 40 %%" +
             "[#]Linus' Nature Stones")
        },
        {
            new LoadableMail(27,Season.Fall,2,"Coco,^^Beef Soup" +
             "^^   -Tofu" +
             "%item object Tofu.RunescapeSpellbook_HardCasket 1 %%" +
             "[#]Letter For Someone Else")
        },
        {
            new Gobcast(21,Season.Spring,1, new()
            {
                "This question from Mudknuckles in Goblin Village. He say 'Generals and also Grubfoot, what fish is best?' " +
                "This bad question. Best fish is whatever fishingman have on them when you hit them with large rock. " +
                "This season many man-fishers carry around Karambwanji. Delicious tropical fish that come out in sunny daytime at beach during spring and summer.",
                "Goblin legend say once, ancient Goblin tribe collect pretty rocks from Karambwanji. Then Big High War God " +
                "kill them with hammers for weakness.",
                "This all for today. Your question terrible. Do not send in question again."
            })
        },
        {  
            new Gobcast(7,Season.Fall,1,new()
            {
                "This question from Goutbones at wherever. He say 'Generals and also Grubfoot, what scariest creature?'. This is a bad question. Goblin should not fear " +
                "because fear is weakness. Me never feel fear. Me once find big-tooth fish on shore at beach. It rainy autumn or winter or spring me forget.",
                "Me start shouting 'Me strong! You weak, big-tooth!'. Nearby fishingman tell me it dead and called Monkfish. He say old humans collect them " +
                "for strong magic rocks. Me then kill him with hammers. ",
                "This all for today. Your question terrible. Do not send in question again."
            })
        },
        {  
            new Gobcast(4,Season.Summer,2,new()
            {
                "This question from Clothears at big beach. He say 'Generals and also Grubfoot! Me catch long fish, big as two Goblin! What me do?'. First, stop fishing. " +
                "Fishing terrible hobby for coward Goblins. Big High War God Give Goblin hammers to crush creatures. Not flimsy stick with worm. " +
                "Second, creature you find very rare fish from summer midday at beach fishingman call 'Manta Ray' but me call it 'What-that-thing'.",
                "Manta Ray is strange fish that think it is bird, but it not bird. This make Manta Ray sad so it start collecting stupid rock thing. " +
                "One day, Me find Manta Ray with pretty purple star on it. Me look at purple star and feel peaceful. This bad, so me run into water and kill Manta Ray. " +
                "This all for today. Your question terrible. Do not send in question again."
            })
        },
        {  
            new Gobcast(24,Season.Fall,1,new()
            {
                "This question from Wormbrain in human prison. He say 'Generals and also Grubfoot, Help! Who is best to steal from?'. Me recommend everyone, as long as you kill them. " +
                " Stealing without murder is like beetle pie without beetle. Me would say, avoid desert traders. They surprisingly strong and fast. One time me manage to get " +
                "lootbag but trader run away and bag full of stupid rocks with air pattern. Why bother carry rock through desert?",
                "This all for today. Your question terrible. Do not send in question again."
            })
        },
        {  
            new Gobcast(2,Season.Winter,1,new()
            {
                "This question from Mistag in stupid cave. He say 'Greetings Generals and Grubfoot! I recently came across this strange battlestaff. I managed to sell it at an adventurer's guild for 100 gold, " +
                "but then I saw them selling it the next day for 1000 gold! Should I have asked for more money?'. First of all, me don't care who buy or sell stupid stick. Not enough " +
                "sharp edges or smashy bits. If you had big hammer, adventurers guild would give you as much money as you ask for. Then you kill them anyway.",
                "This all for today. Your question terrible. Do not send in question again."
            })
        },
        {  
            new Gobcast(3,Season.Spring,2,new()
            {
                "This question from Goblin Champion. Me hate this guy. He say 'Generals and also Grubfoot! Other champions at guild make fun of me. They say Goblin too stupid for magic! " +
                "How me prove them wrong?' Magic for coward Goblins who too weak to carry hammer. My reccomendation is learn to use hammer instead.",
                "If champion insist on using silly rocks, bandage man in desert place carry around lot of rock with scary pictures. Me recommend learning to throw " +
                "rock hard instead of using prissy magic stick.",
                "This all for today. Your question terrible. Do not send in question again."
            })
        },
        {  
            new Gobcast(11,Season.Winter,1,new()
            {
                "This question from Zanik. Grubfoot must stop allowing cave cowards to send in questions. She say something about finding treasure. Question was too long, me not read. ",
                "Goblin is not greedy human. 'Treasure' only good if heavy enough to bludgeon. Once, me find some heavy boxes in sandy cave. Snake that fly carry around big box with green lock. Not too heavy. Maybe good for crush weak creature, like you cave cowards",
                "Bandage man carry some box with purple lock, much better for smashing. Sergeant Mossfists drop on leg and need to go to medicine Goblin." +
                "Very funny. Once, elder goblin tell me about red lock chest. He stupid and senile though. Red colour for proud goblin, not stupid box thing.",
                "This all for today. Your question terrible. Do not send in question again."
            })
        },
        {  
            new Gobcast(2,Season.Summer,3,new()
            {
                "This question from Goblin Guard 1. He say 'Generals and also Grubfoot! Idiot Guard 2 get fancy sword. He say that sword better than spear. How me tell him spear is best?' " +
                "First, you wrong and stupid. Hammer is best. Sword is least best. Maybe even worse than Axe. ",
                "Me once see idiot fish shaped like sword on island vacation. Me relaxing in water at night-time, planning how me crush enemies. " +
                "Then stupid fish come up and poke me in leg. If me not strong as ten Goblins, maybe would have hurt! Tell Guard 2 if he want to use fish weapon, maybe he belong better " +
                "in ocean than in glorious goblin army.",
                "This all for today. Your question terrible. Do not send in question again."
            })
        },
        {  
            new Gobcast(22,Season.Winter,3,new()
            {
                "Ah, right. No General Wartface or Grubfoot. General Wartface figured out me unplug his microphone entire time and say he not want to do show anymore. " +
                "He say 'Show making not real Goblin job anyway, should be crushing the weak instead.'. He wrong and stupid but me start to think, show making actually really boring. ",
                "This last episode, network say they do thing called 'rerun'. Me not understand how they get Goblin that look and talk like me but me not really care. Maybe one day me find " +
                "imposter and crush him. Have not decided yet.",
                "This all for today. You all terrible. Do not send in questions."
            })
        }
        
        
    };

    public static Dictionary<string, Dictionary<string,string>> loadableEvents = new()
    {
        {
            "Data/Events/Farm", new()
            {
                {
                    "Tofu.RunescapeSpellbook_Event0/f Wizard 1000/t 600 1200",
                    "continue/64 15/farmer 64 16 2 Wizard 64 18 0" +
                    "/pause 1500/speak Wizard \"Greetings, @. I hope I am not interrupting your work on the farm.\"" +
                    "/speak Wizard \"I've made great progress with my research as of late, thanks to your generous gifts.\"" +
                    "/speak Wizard \"As thanks, I wanted to give you this old tome of runic magic from my personal library, I have no use for it anymore.\"" +
                    "/stopMusic /itemAboveHead Tofu.RunescapeSpellbook_RuneSpellbook /pause 1500 /glow 24 107 97 /playsound RunescapeSpellbook.MagicLevel /pause 2000 /mail Tofu.RunescapeSpellbook_SpellMail" +
                    "/speak Wizard \"This form of magic should be suitable for a novice. You need only some runestones, I'm sure you've come across some in the mines already.\"/pause 600" +
                    "/speak Wizard \"Well, that was all. I'll be on my way now.\"" +
                    "/pause 300/end"
                }
            }
        },
        {
            "Data/Events/ArchaeologyHouse", new()
            {
                {
                    "Tofu.RunescapeSpellbook_Event1/n Tofu.RunescapeSpellbook_RunesFound",
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

    public static List<CustomBuff> loadableBuffs = new()
    {
        new CustomBuff("Tofu.RunescapeSpellbook_BuffCharge", "Charge", "Combat spells will produce extra projectiles", 60_000, 0),
        new CustomBuff("Tofu.RunescapeSpellbook_BuffDark", "Dark lure", "Spawns more monsters and makes them aggressive towards you", 180_000, 1),
        new CustomBuff("Tofu.RunescapeSpellbook_BuffHunters", "Hunter's Call", "Second bar while fishing which provides 1.5x progress", 300_000, 2),
        new CustomBuff("Tofu.RunescapeSpellbook_BuffBattlemage", "Battlemage's Spark", "Combat spells have a 10% chance of not costing any runes", 540_000, 3),
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
        
        localFarmerData = new PlayerLocalData();

        infiniteRuneReferences = new();
        //Generate the lookup dictionary for determining what weapons give infinite values for each rune
        foreach (StaffWeaponData weapon in staffWeapons.Where(x=>x.providesRune != null))
        {
            if (!infiniteRuneReferences.ContainsKey(weapon.providesRune))
            {
                infiniteRuneReferences.Add(weapon.providesRune,new(){weapon.id.ToString()});
            }
            else
            {
                infiniteRuneReferences[weapon.providesRune].Add(weapon.id.ToString());
            }
        }
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
    
    public static int GetFarmerExperience(Farmer farmer)
    {
        int experience = -1;
        int.TryParse(TryGetModVariable(farmer,"Tofu.RunescapeSpellbook_MagicExp"),out experience);
        return experience;
    }

    public static void IncrementMagicExperience(Farmer farmer, int gainedExperience)
    {
        int experience = GetFarmerExperience(farmer);
        
        if (experience != -1 && experience <= Farmer.getBaseExperienceForLevel(10)) //If our exp should still be tracked then increment it
        {
            int newTotalExperience = (experience + gainedExperience);
            TrySetModVariable(farmer,"Tofu.RunescapeSpellbook_MagicExp",newTotalExperience.ToString());
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
}