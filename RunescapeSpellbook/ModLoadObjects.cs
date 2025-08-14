using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
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
    
    public static Dictionary<int,ModLoadObjects> modItems = new(){
        {4290,new RunesObjects(4290,"Rune_Spellbook","Spellbook","Debug object.",-999)},
        {4291,new RunesObjects(4291,"Rune_Air","Air Rune","One of the 4 basic elemental Runes",-429)},
        {4292,new RunesObjects(4292,"Rune_Water","Water Rune","One of the 4 basic elemental Runes",-429,
            new(){{"Willy",PrefType.Neutral},{"Elliott",PrefType.Neutral}})},
        {4293,new RunesObjects(4293,"Rune_Fire","Fire Rune","One of the 4 basic elemental Runes",-429,
            new(){{"Sam",PrefType.Neutral},{"Vincent",PrefType.Neutral}})},
        {4294,new RunesObjects(4294,"Rune_Earth","Earth Rune","One of the 4 basic elemental Runes",-429,
            new(){{"Dwarf",PrefType.Neutral},{"Demetrius",PrefType.Neutral}})},
        {4295,new RunesObjects(4295,"Rune_Law","Law Rune","Used for teleport spells",-431,
            new(){{"Wizard",PrefType.Like}})},
        {4296,new RunesObjects(4296,"Rune_Nature","Nature Rune","Used for alchemy spells",-431,
            new(){{"Leo",PrefType.Neutral},{"Linus",PrefType.Neutral},{"Wizard",PrefType.Neutral}})},
        {4297,new RunesObjects(4297,"Rune_Cosmic","Cosmic Rune","Used for enchant spells",-431,
            new(){{"Emily",PrefType.Neutral},{"Maru",PrefType.Like},{"Wizard",PrefType.Neutral}})},
        {4298,new RunesObjects(4298,"Rune_Astral","Astral Rune","Used for Lunar spells",-431,
            new(){{"Emily",PrefType.Like},{"Maru",PrefType.Neutral},{"Wizard",PrefType.Neutral}})},
        {4299,new RunesObjects(4299,"Rune_Chaos","Chaos Rune","Used for low level combat spells",-430,
            new(){{"Emily",PrefType.Hate},{"Kent",PrefType.Hate},{"Wizard",PrefType.Neutral}})},
        {4300,new RunesObjects(4300,"Rune_Death","Death Rune","Used for high level combat spells",-430,
            new(){{"Sebastian",PrefType.Like},{"Emily",PrefType.Hate},{"George",PrefType.Hate},{"Evelyn",PrefType.Hate},{"Wizard",PrefType.Neutral}})},

        {4301,new SlingshotItem(4301,"Ammo_Fire","Fire Orb","Enchanted ammo that burns enemies in a radius around a hit enemy. Fire cannot finish off enemies.",30)},
        {4302,new SlingshotItem(4302,"Ammo_Earth","Earth Orb","Enchanted ammo that explodes and poisons enemies in a radius around a hit enemy. Poison cannot finish off enemies.",31)},
        
        {4359,new TreasureObjects(4359,"Treasure_Elemental","Elemental Geode","Contains some elemental Runes. A blacksmith might be able to open it.",19,
            new()
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
            new()
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
            new()
            {
                new ItemDrop(4359,5,10,0.5),
                new ItemDrop(4360,5,10,0.5),
                
                new ItemDrop(4352,1,1,0.3),
                new ItemDrop(4353,1,1,0.3),
                new ItemDrop(4354,1,1,0.3),
                new ItemDrop(4355,1,1,0.3),
                
                new ItemDrop(4362,1,1,0.05),
            },200,new(){{"Abigail",PrefType.Like}})},
        
        {4362,new TreasureObjects(4362,"Treasure_HardCasket","High Level Casket","Contains some valuable magical goodies. A blacksmith might be able to open it.",22,
            new()
            {
                new ItemDrop(4359,10,15,0.5),
                new ItemDrop(4360,10,15,0.5),
                
                new ItemDrop(4352,1,1,0.7),
                new ItemDrop(4353,1,1,0.7),
                new ItemDrop(4354,1,1,0.7),
                new ItemDrop(4355,1,1,0.7),
                new ItemDrop(4356,1,1,0.5),
                new ItemDrop(4363,1,1,0.1),
            },500,new(){{"Abigail",PrefType.Love}})},
        
        {4363,new TreasureObjects(4363,"Treasure_BarrowsCasket","Barrows Casket","Contains some very valuable magical goodies. A blacksmith might be able to open it.",23,
            new()
            {
                new ItemDrop(4356,1,1,1),
                new ItemDrop(4357,1,1,0.5),
                new ItemDrop(4358,1,1,0.5),
            },1000,new(){{"Abigail",PrefType.Love}})},
        
        {4364,new PackObject(4364,"Treasure_AirPack","Air Rune Pack","A pack containing many air Runes. A blacksmith might be able to open it.",24,4291,3)},
        {4365,new PackObject(4365,"Treasure_WaterPack","Water Rune Pack","A pack containing many water Runes. A blacksmith might be able to open it.",25,4292,1)},
        {4366,new PackObject(4366,"Treasure_FirePack","Fire Rune Pack","A pack containing many fire Runes. A blacksmith might be able to open it.",26,4293)},
        {4367,new PackObject(4367,"Treasure_EarthPack","Earth Rune Pack","A pack containing many earth Runes. A blacksmith might be able to open it.",27,4294)},
        {4368,new PackObject(4368,"Treasure_ChaosPack","Chaos Rune Pack","A pack containing many chaos Runes. A blacksmith might be able to open it.",28,4299)},
        {4369,new PackObject(4369,"Treasure_DeathPack","Death Rune Pack","A pack containing many death Runes. A blacksmith might be able to open it.",29,4300)},
        
        {4370,new FishObject(4370,"Fish_Karam","Karambwanji","A small brightly coloured tropical fish. Traditionally associated with elemental magic",32,45,600,1800,new(){Season.Spring,Season.Summer},"sunny",new()
            { "Beach" },6,2,20,5,2,Color.Cyan,"color_sea_green",1,7,
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
                {9,new ItemDrop(4361,1,1,0.05)},
                {1,new ItemDrop(4292,10,16,0.5)},
                {2,new ItemDrop(4291,10,16,0.3)},
                {5,new ItemDrop(4294,10,15,0.3)},
                {6,new ItemDrop(4293,10,15,0.2)},
                {0,new ItemDrop(812,1,1,1.0)}
            }
            )},
        {4371,new FishObject(4371,"Fish_Monk","Monkfish","An anglerfish known for its toothy smile. Traditionally associated with combat magic",33,60,1600,2300,new(){Season.Fall,Season.Winter,Season.Spring},"rainy",new()
            { "Beach" },4,5,60,50,3,Color.NavajoWhite,"color_sand",25,40,
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
                        "213","242","728","787","4361"
                    }
                    
                }
            }, new()
            {
                {9,new ItemDrop(4362,1,1,0.05)},
                {7,new ItemDrop(4300,10,30,0.4)},
                {3,new ItemDrop(4299,10,30,0.5)},
                {0,new ItemDrop(812,1,1,1.0)}
            }
        )},
        {4372,new FishObject(4372,"Fish_Manta","Manta Ray","A large and intelligent fish that feeds on plankton. Traditionally associated with catalytic magic",34,75,1200,1800,new(){Season.Summer},"sunny",new()
                { "Beach" },6,6,100,70,4,Color.RoyalBlue,"color_red",118,216,
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
                        "4359 10","832","4361"
                    }
                    
                }
            }, new()
            {
                {2,new ItemDrop(4295,3,5,0.5)},
                {6,new ItemDrop(4297,3,5,0.3)},
                {5,new ItemDrop(4298,3,5,0.3)},
                {0,new ItemDrop(4296,3,5,1.0)},
            }
        )},
        {4373,new FishObject(4373,"Fish_Sword","Swordfish","A predatory fish with a flat sword-like pointed bill",35,95,2000,2600,new(){Season.Spring,Season.Summer,Season.Fall,Season.Winter},"both",new()
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
                {9,new ItemDrop(74,1,1,0.01)},
                {4,new ItemDrop(4300,10,15,0.6)},
                {2,new ItemDrop(773,1,3,0.5)},
                {0,new ItemDrop(812,1,1,1.0)},
            }
        )},
        {4374, new SeedObject(4374,"Harralander Seed","Harralander Seed","Plant these in the fall. Takes 12 days to mature.",36,50)},
        {4375,new CropObject(4375,"Harralander","Harralander","A herb that naturally grows in rocky crevices, named for its destructive nature.",
            "4374",new(){Season.Fall},3,0,37,110,-50,"color_green",-75,1,0.25f) },
        {4376, new SeedObject(4376,"Lantadyme Seed","Lantadyme Seed","Plant these in the winter. Takes 12 days to mature.",38,50)},
        {4377,new CropObject(4377,"Lantadyme","Lantadyme","A herb that is said to resemble eyes when in bloom",
            "4376",new(){Season.Winter},3,1,39,130,-50,"color_blue",-75,1,0.25f) },
        {4378, new PotionObject(4378,"Guthix Rest","Guthix Rest","A relaxing cup of tea that restores some health. Can heal over your maximum health.",40,350,0.3f,0.15f,"4375",5000)},
        {4379, new PotionObject(4379,"Saradomin Brew","Saradomin Brew","A relaxing cup of tea that restores a lot of health. Can heal over your maximum health.",44,500,0.6f,0.25f,"4377",6000)},
        {4380, new PotionObject(4380,"Harralander Dye","Harralander Dye","A green dye made from harralander. Prized by artists.",41,250,"4375",60,"colour_green")},
        {4381, new PotionObject(4381,"Lantadyme Dye","Lantadyme Dye","A blue dye made from lantadyme. Prized by artists.",45,350,"4377",60,"color_dark_blue")},
        //{4382, new PotionObject(4382,"Compost Potion","Compost Potion","UNUSED",42,80,"4375 1 382 3")},
        {4383, new PotionObject(4383,"Hunter Potion","Hunter Potion","Increases Fishing Prowess",43,80,"4375 1 881 3")},
        {4384, new PotionObject(4384,"Battlemage Potion","Battlemage Potion","Increases Magical Damage",46,160,"4377 1 90 3")},
        {4385, new PotionObject(4385,"Super Restore","Super Restore","Increases Max Energy Significantly",47,160,"4377 1 -5 3")},
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
            new() { {4295, 1},{4291,3},{4292,2} },8,"Town", 43, 60,0),
        
        new TeleportSpell(1,"Teleport_Home","Farm Teleport","Teleports you outside the main house on your farm",4,
            new() { {4295, 1},{4291,3},{4294,3} },10, "FarmHouse"),
        
        new InventorySpell(2,"Menu_Superheat","Superheat Item","Smelts ore without a furnace or coal, or burns wood into coal at a discount",1,
            new() { {4296, 1},{4293,4}},10,
            (i=>i is Item item && (item.QualifiedItemId == "(O)388" || DataLoader.Machines(Game1.content).GetValueOrDefault("(BC)13").OutputRules.Any(x=>x.Triggers.Any(y=>y.RequiredItemId == item.QualifiedItemId)))),
            SpellEffects.SuperheatItem,"Smelt any ores into bars instantly without any coal cost, or smelt wood into coal. Put an appropriate item in the slot and press the spell icon to cast.",1,"Superheat"),
        
        new InventorySpell(3,"Menu_HighAlch","High Level Alchemy","Converts an item into 1.5x its sell price",5,
            new() { {4296, 1},{4293,5}},15,(i=>i is Item item && item.canBeShipped() && item.salePrice(false) > 0 && item.Category != -429 && item.Category != -430 && item.Category != -431),
            SpellEffects.HighAlchemy,"Turn any sellable item into money. Provides 150% of the items value. Put an appropriate item in the slot and press the spell icon to cast.",0,"HighAlch"),
        
        new TilesSpell(4,"Area_Humidify","Humidify","Waters the ground around you",1,
            new() { {4298, 1},{4293,1},{4292,3}}, 0.4f,SpellEffects.Humidify, 10,5,"Humidify",
            (tile => tile is HoeDirt hoeLand && (hoeLand.crop == null || !hoeLand.crop.forageCrop.Value || hoeLand.crop.whichForageCrop.Value != "2") && hoeLand.state.Value != 1)),
        
        new TilesSpell(5,"Area_Cure","Cure Plant","Replants dead crops",6,
            new() { {4298, 1},{4294,8}},0.6f, SpellEffects.CurePlant, 10,6,"Cure",
            (tile => tile is HoeDirt hoeLand && hoeLand.crop != null && hoeLand.crop.dead.Value)),
        
        new BuffSpell(6,"Buff_VileVigour","Vile Vigour","Sacrifices a third of your max health to fill your energy",3,
            new() { {4297, 1},{4291,3}},5, (f=> f is Farmer farmer && farmer.stamina < farmer.MaxStamina), SpellEffects.VileVigour,
            7,"Vile","My energy is already full"),
        
        new BuffSpell(7,"Buff_PieMake","Bake Pie","Cooks a random recipe that you know using your held ingredients",3,
            new() { {4298, 1},{4293,1},{4292,1}}, 15,
            (f=> f is Farmer farmer && farmer.cookingRecipes.Length > 0), SpellEffects.BakePie, 8,"BakePie","I don't know enough recipes"),
        
        new TeleportSpell(8,"Teleport_Desert","Desert Teleport","Teleports you to the desert, if you have access to it",5,
            new() { {4295, 2},{4294,5},{4293,5}}, 15,"Desert", 19, 34,2,
            ((farmer => Game1.MasterPlayer.mailReceived.Contains("ccVault")))),
        
        new TeleportSpell(9,"Teleport_Ginger","Ginger Island Teleport","Teleports you to ginger island, if you have access to it",7,
            new() { {4295, 2},{4292,5},{4293,5}},15, "IslandSouth",21,37,0,
            ((farmer => Game1.MasterPlayer.hasOrWillReceiveMail("willyBoatFixed")))),
        
        new TeleportSpell(10,"Teleport_Caves","Caves Teleport","Teleports you to the pelican town mines",2,
            new() { {4295, 1},{4291,5}},10, "Mountain",54,7,0, 
            ((farmer => Game1.MasterPlayer.hasOrWillReceiveMail("landslideDone")))),
        
        new InventorySpell(11,"Menu_EnchantRuby","Enchant Ruby Bolt","Convert any red or orange stones into fiery ammo",4,
            new() { {4297, 1},{4293,3}},10,(i => i is Item item && SpellEffects.redGemsEnchants.ContainsKey(item.ItemId)),SpellEffects.EnchantRubyBolts,
            "Convert any red gems or rocks into fiery ammo for the slingshot. On hitting an enemy, fire spreads to nearby enemies. Fire cannot finish off enemies.",2,"EnchantBolt"),
        
        new InventorySpell(12,"Menu_EnchantEmerald","Enchant Emerald Bolt","Convert any green stones into explosive poisonous ammo",8,
            new() { {4297, 2},{4294,3}},15,(i => i is Item item && SpellEffects.greenGemsEnchants.ContainsKey(item.ItemId)),SpellEffects.EnchantEmeraldBolt,
            "Convert any green gems into explosive poisonous ammo for the slingshot. On hitting an enemy, poison spreads to nearby enemies. Poison cannot finish off enemies.",2,"EnchantBolt"),
        
        new BuffSpell(13,"Buff_DarkLure","Dark Lure","Summons more enemies, and makes them prioritise you over other farmers for 3 minutes",6,
            new() { {4296, 2},{4297,2}},10,(f=> f is Farmer farmer && !farmer.hasBuff("430")),SpellEffects.DarkLure, 9,"DarkLure","I'm already luring monsters!"),
        
        new CombatSpell(14,"Combat_Wind","Wind Strike","A basic air missile",0,
            new() { {4299, 1},{4291,1}}, 1,40,15,0,Color.White,"WindStrike"),
       
        new CombatSpell(15,"Combat_Water","Water Bolt","A low level water missile",2,
            new() { {4299, 2},{4291,2},{4292,2}},2, 70,16,1,Color.DarkCyan,"WaterBolt"),
        
        new CombatSpell(16,"Combat_Undead","Crumble Undead","Hits undead monsters for extra damage",4,
            new() { {4299, 2},{4291,2},{4294,2}},4, 60,13,3,Color.Yellow,"CrumbleUndead",SpellEffects.DealUndeadDamage),
        
        new CombatSpell(17,"Combat_Earth","Earth Blast","A medium level earth missile",6,
            new() { {4300, 1},{4291,3},{4294,3}},4, 90,16,1,Color.DarkGreen,"EarthBlast"),
        
        new CombatSpell(18,"Combat_Fire","Fire Wave","A high level fire missile",8,
            new() { {4300, 2},{4291,3},{4293,4}},5, 120,15,2,Color.OrangeRed,"FireWave"),
        
        new BuffSpell(19,"Buff_Charge","Charge","Spells cast three projectiles for 60 seconds",7,
            new() { {4300, 3},{4291,3},{4293,3}},10,(f=> f is Farmer farmer && !farmer.hasBuff("429")),SpellEffects.Charge, 10,"Charge","I'm already charged!"),
        
        new CombatSpell(20,"Combat_Demonbane","Demonbane","Hits undead monsters for a lot of extra damage",9,
            new() { {4300, 2},{4298,2},{4293,4}},6, 100,13,3,Color.Purple,"CrumbleUndead",SpellEffects.DealDemonbaneDamage),
        
        new CombatSpell(21,"Combat_Blood","Blood Barrage","Fires a strong vampiric blood missile",10,
            new() { {4300, 4},{4297,3}}, 10,100,15,1,Color.Crimson, "BloodBarrage",SpellEffects.DealVampiricDamage),
        
        new InventorySpell(22,"Menu_Plank","Plank Make","Turns wood into hardwood and vice versa and uncrafts wooden items into wood",3,
            new() { {4298, 1},{4297,1}},10,
            (i => i is Item item && (item.itemId.Value == "388" || item.itemId.Value == "709" || 
                                     (CraftingRecipe.craftingRecipes.ContainsKey(item.Name) 
                                      && CraftingRecipe.craftingRecipes[item.Name].Split(' ').ToList() is List<string> recipes 
                                      && ((recipes.IndexOf("388") != -1 && (recipes.IndexOf("388") + 1) % 2 != 0) || (recipes.IndexOf("709") != -1 && recipes.IndexOf("709") + 1 % 2 != 0) ) )))
            ,SpellEffects.PlankMake,
            "Breaks down wooden items into wood, and converts 15 wood into 1 hardwood and vice versa. For recipes that require more than wood, it will only return the wood.",3,"Degrime"),
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
            new ItemDrop(4295,2,0.08f),
            new ItemDrop(4296,2,0.1f),
            new ItemDrop(4359,1,0.02f),
        } },
        { "Prismatic Slime", new(){
            new ItemDrop(4295,4,0.9f),
            new ItemDrop(4298,5,0.9f),
        } },
        { "Green Slime", new(){
            new ItemDrop(4295,2,0.08f),
            new ItemDrop(4359,1,0.02f),
        } },
        { "Fly", new(){
            new ItemDrop(4364,1,0.2f),
            new ItemDrop(4368,1,0.05f),
        } },
        { "Rock Crab", new(){
            new ItemDrop(4359,1,0.3f),
        } },
        { "Grub", new(){
            new ItemDrop(4296,2,0.1f),
        } },
        { "Bug", new(){
            new ItemDrop(4368,1,0.08f),
            new ItemDrop(4359,1,0.2f),
        } },
        { "Bat", new(){
            new ItemDrop(4364,1,0.15f),
            new ItemDrop(4368,1,0.25f),
        } },
        { "Stone Golem", new(){
            new ItemDrop(4367,1,0.2f),
            new ItemDrop(4360,1,0.1f),
            new ItemDrop(4361,1,0.04f),
        } },
        { "Dust Spirit", new(){
            new ItemDrop(4364,1,0.04f),
            new ItemDrop(4296,1,0.02f),
        } },
        { "Frost Bat", new(){
            new ItemDrop(4364,1,0.05f),
            new ItemDrop(4365,1,0.15f),
            new ItemDrop(4368,1,0.3f),
        } },
        { "Ghost", new(){
            new ItemDrop(4295,3,0.1f),
            new ItemDrop(4297,2,0.05f),
            new ItemDrop(4360,1,0.15f),
        } },
        { "Frost Jelly", new(){
            new ItemDrop(4365,1,0.1f),
            new ItemDrop(4359,1,0.2f),
        } },
        { "Skeleton", new(){
            new ItemDrop(4368,2,0.3f),
            new ItemDrop(4369,1,0.02f),
            new ItemDrop(4360,1,0.05f),
            new ItemDrop(4361,1,0.08f),
        } },
        { "Lava Bat", new(){
            new ItemDrop(4364,2,0.15f),
            new ItemDrop(4366,2,0.15f),
        } },
        { "Lava Crab", new(){
            new ItemDrop(4365,1,0.15f),
            new ItemDrop(4366,1,0.3f),
            new ItemDrop(4359,2,0.3f),
        } },
        { "Shadow Shaman", new(){
            new ItemDrop(4296,3,0.2f),
            new ItemDrop(4298,2,0.2f),
            new ItemDrop(4360,2,0.2f),
        } },
        { "Metal Head", new(){
            new ItemDrop(4367,2,0.3f),
            new ItemDrop(4361,1,0.1f),
        } },
        { "Shadow Brute", new(){
            new ItemDrop(4364,2,0.1f),
            new ItemDrop(4368,2,0.3f),
            new ItemDrop(4360,1,0.1f),
        } },
        { "Squid Kid", new(){
            new ItemDrop(4364,3,0.2f),
            new ItemDrop(4297,2,0.2f),
            new ItemDrop(4359,2,0.2f),
        } }, //Skull Cavern 
        { "Sludge", new(){
            new ItemDrop(4295,2,0.2f),
            new ItemDrop(4359,1,0.2f),
        } },
        { "Serpent", new(){
            new ItemDrop(4364,2,0.25f),
            new ItemDrop(4297,6,0.1f),
            new ItemDrop(4361,1,0.1f),
            new ItemDrop(4368,2,0.1f),
        } },
        { "Carbon Ghost", new(){
            new ItemDrop(4295,4,0.3f),
            new ItemDrop(4298,4,0.2f),
            new ItemDrop(4369,2,0.1f),
        } },
        { "Iridium Crab", new(){
            new ItemDrop(4365,5,0.6f),
            new ItemDrop(4359,6,0.6f),
            new ItemDrop(4360,3,0.4f),
        } },
        { "Pepper Rex", new(){
            new ItemDrop(4366,3,1f),
            new ItemDrop(4296,3,0.5f),
            new ItemDrop(4361,1,0.5f),
        } },
        { "Mummy", new(){
            new ItemDrop(4367,2,0.2f),
            new ItemDrop(4295,3,0.3f),
            new ItemDrop(4368,3,0.3f),
            new ItemDrop(4369,1,0.2f),
            new ItemDrop(4362,1,0.1f),
        } },
        { "Iridium Bat", new(){
            new ItemDrop(4364,3,0.5f),
            new ItemDrop(4369,2,0.2f),
            new ItemDrop(4362,1,0.2f),
        } },
        { "Haunted Skull", new(){ //Quarry Mine
            new ItemDrop(4297,3,0.4f),
            new ItemDrop(4298,3,0.3f),
            new ItemDrop(4361,1,0.05f),
            new ItemDrop(4362,1,0.02f),
        } },
        { "Hot Head", new(){ //Ginger Island/Volcano
            new ItemDrop(4366,2,0.3f),
            new ItemDrop(4369,3,0.2f),
            new ItemDrop(4360,2,0.2f),
        } },
        { "Tiger Slime", new(){
            new ItemDrop(4365,2,0.1f),
            new ItemDrop(4367,1,0.2f),
            new ItemDrop(4296,5,0.3f),
            new ItemDrop(4368,3,0.3f),
            new ItemDrop(4369,2,0.1f),
        } },
        { "Magma Sprite", new(){
            new ItemDrop(4364,3,0.2f),
            new ItemDrop(4366,2,0.3f),
        } },
        { "Dwarvish Sentry", new(){
            new ItemDrop(4295,4,0.24f),
            new ItemDrop(4297,10,0.2f),
            new ItemDrop(4369,3,0.3f),
            new ItemDrop(4363,1,0.05f),
        } },
        { "Magma Duggy", new(){
            new ItemDrop(4366,2,0.3f),
            new ItemDrop(4359,5,0.3f),
            new ItemDrop(4363,1,0.05f),
        } },
        { "Magma Sparker", new(){
            new ItemDrop(4366,2,0.3f),
        } },
        { "False Magma Cap", new(){
            new ItemDrop(4367,1,0.2f),
            new ItemDrop(4297,10,0.2f),
        } },
    };

    //Items to be put in shops
    public static Dictionary<string, List<ShopListings>> loadableShops = new()
    {
        {"AdventureShop", new()
        {
            new ShopListings("Marlon_Battlestaff","(W)4351",2000,2,-1,-1,"PLAYER_HAS_SEEN_EVENT Current RS.0")
        }},
        {"DesertTrade", new()
        {
            new ShopListings("Desert_AirRunes","(O)4291","(O)60",1,4,40,40,"PLAYER_HAS_SEEN_EVENT Current RS.0")
        }},
        {"Sandy", new()
        {
            new ShopListings("Seed_Harralander","(O)4374",100,4,-1,-1,"PLAYER_HAS_SEEN_EVENT Current RS.0, PLAYER_BASE_FARMING_LEVEL Current 3"),
            new ShopListings("Seed_Lantadyme","(O)4376",300,5,-1,-1,"PLAYER_HAS_SEEN_EVENT Current RS.0, PLAYER_BASE_FARMING_LEVEL Current 8"),
            new ShopListings("Recipe_Hunter","(O)4383",8000,6,-1,-1,"PLAYER_HAS_SEEN_EVENT Current RS.0, PLAYER_BASE_FARMING_LEVEL Current 3",true),
            new ShopListings("Recipe_Battlemage","(O)4384",8000,7,-1,-1,"PLAYER_HAS_SEEN_EVENT Current RS.0, PLAYER_BASE_FARMING_LEVEL Current 8",true),
            new ShopListings("Recipe_SuperRestore","(O)4385",6000,8,-1,-1,"PLAYER_HAS_SEEN_EVENT Current RS.0, PLAYER_BASE_FARMING_LEVEL Current 8",true),
        }}
    };
    
    /// <summary>
    /// mail + notes to load into the game
    /// <remarks>bool: true is mail, false is secret note</remarks>
    /// </summary>
    public static List<LoadableText> loadableText = new()
    {
        {
            new LoadableMail("RSSpellMailGet","Dear @,^^I had forgotten one last thing about runic magic. Combat spells require a focus. In layman's terms, a battlestaff." +
              "^I've included one with this letter, and warned the mailcarrier of the consequences if you do not receive it in one piece. " +
              "^^   -M. Rasmodius, Wizard[letterbg 2]" +
              "%item object 4351 1 %%" +
              "[#]Wizard's Battlestaff Gift")
        },
        {
            new LoadableMail(15,Season.Summer,1,"@,^Have you come across some strange packages in the mines lately? They seem to be full of those weird painted rocks that Emily likes." +
                "^^They're pretty hard to open, but my geode hammer seems to do the trick. If you find any, swing by and I'll help you open it" +
                "^^   -Clint^^P.S I've included some samples with this letter" +
                "%item object 4364 3 %%" +
                "[#]Clint's Pack Opening Service")
        },
        {
            new LoadableMail(3,Season.Spring,2,"Ahoy @,^This was floating around in the ocean so I fished it up, some people have no respect for the seas." +
            "^^It seems like something ya might get some use out of, it'd make some fine firewood!" +
            "^^   -Willy" +
            "%item object 4361 1 %%" +
            "[#]Willy's Casket")
        },
        {
            new LoadableMail(1,Season.Summer,3,"@,^I sent some of these to Emily as an anonymous gift but came in yesterday and sold them to my shop.^^She said the design made her uncomfortable." +
             "^^Maybe you'll get something out of them." +
             "^^   -Clint" +
             "%item object 4300 60 %%" +
             "[#]Clint's Terrible Gift")
        },
        {
            new LoadableMail(9,Season.Spring,2,"@,^An old friend gave me some of these, but I don't have enough space to keep all of them." +
             "^^I hope you'll think of the great outdoors when you use them." +
             "^^   -Linus" +
             "%item object 4296 40 %%" +
             "[#]Linus' Nature Stones")
        },
        {
            new LoadableMail(27,Season.Fall,2,"Coco,^^Beef Soup" +
             "^^   -Tofu" +
             "%item object 4362 1 %%" +
             "[#]Letter For Someone Else")
        },
        {
            new LoadableSecret(419,"In a past life, the men of the desert practiced runic magic." +
              "^^Their Flesh inherited their strength." +
              "^^Their Souls inherited their wisdom." +
              "^^Their Visage, sealed within the crypt of death, inherited the light of the stars." +
              "^^Their Shadows, those who escaped the jaws of the ancient beasts, stole away the secrets of the world.")
        },
        {
            new LoadableSecret(429,"Once, the great druid brought balance to the world." +
              "^^As he slept, the world splintered, and the spirits became restless." +
              "^^The spirits sought those with those whom they shared affinity." +
              "^^The great snakes of the desert were granted mastery of the winds." +
              "^^The spiders of the sea spread the ocean inland." +
              "^^The men learned to till the soil, in exchange for their dead."+
              "^^The flame spread to the depths and tropics, creating life where there was none."+
              "^^That what remained found refuge in the primordial slurry, which became the slime.")
        },
        {
            new LoadableSecret(438,"The ancient men, blessed with the power of creation, made tools of war." +
              "^^When the ancient empires fell, their weapons became scattered." +
              "^^The first casket hid the cornucopia of elements." +
              "^^Stone Golem, Skeleton, Metal Head, Serpent, Pepper Rex, Haunted Skull." + 
              "^^The second casket contained the secrets of the elements and the symbol of their lord."+
              "^^Mummy, Iridium Bat, Haunted Skull."+
              "^^The final casket held the forbidden knowledge of the god slayer, granted to his wights."+
              "^^High Level Casket, Dwarvish Sentry, Magma Duggy.")
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
            "Data/Events/ArchaeologyHouse", new()
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

        infiniteRuneReferences = new();
        //Generate the lookup dictionary for determining what weapons give infinite values for each rune
        foreach (StaffWeaponData weapon in staffWeapons.Where(x=>x.providesRune != -1))
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
        List<Farmer> farmers = new();
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
        List<int> perkIDs = new();
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