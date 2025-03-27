using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Objects;
using StardewValley.GameData.Weapons;
using StardewValley.Network;
using StardewValley.TerrainFeatures;

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

public static class ModAssets
{
    public static Texture2D extraTextures; //Includes spells + basic icons
    public static Texture2D animTextures;

    public static PlayerModData localFarmerData;
    
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
        new TeleportSpell(0,"Teleport_Valley","Valley Teleport","Teleports you to Pierre's Store in Pelican Town",1,
            new Dictionary<int, int>() { {4295, 1},{4291,3},{4293,1} },"Town", 43, 60,0),
        
        new TeleportSpell(1,"Teleport_Home","Farm Teleport","Teleports you outside your Farm",4,
            new Dictionary<int, int>() { {4295, 1},{4291,1},{4294,1} }, "BusStop", 19, 23,2),
        
        new InventorySpell(2,"Menu_Superheat","Superheat Item","Smelts ore without a furnace or coal",2,
            new Dictionary<int, int>() { {4296, 1},{4293,4}},
            (i=>i is Item item && DataLoader.Machines(Game1.content).GetValueOrDefault("(BC)13").OutputRules.Any(x=>x.Triggers.Any(y=>y.RequiredItemId == item.QualifiedItemId))),
            SpellEffects.SuperheatItem,"Smelt any ores into bars instantly without any coal cost. Put an appropriate item in the slot and press the spell icon to cast."),
        
        new InventorySpell(3,"Menu_HighAlch","High Level Alchemy","Converts an item into gold",5,
            new Dictionary<int, int>() { {4296, 1},{4293,5}},(i=>i is Item item && item.canBeShipped() && item.salePrice(false) > 0),
            SpellEffects.HighAlchemy,"Turn any sellable item into money. Provides 100% of the items shipping bin value. Put an appropriate item in the slot and press the spell icon to cast."),
        
        new TilesSpell(4,"Area_Humidify","Humidify","Waters the ground around you",3,
            new Dictionary<int, int>() { {4298, 1},{4293,1},{4292,3}}, SpellEffects.Humidify, 10,
            (tile => tile is HoeDirt hoeLand && (hoeLand.crop == null || !hoeLand.crop.forageCrop.Value || hoeLand.crop.whichForageCrop.Value != "2") && hoeLand.state.Value != 1)),
        
        new TilesSpell(5,"Area_Cure","Cure Plant","Replants dead crops",6,
            new Dictionary<int, int>() { {4298, 1},{4294,8}}, SpellEffects.CurePlant, 10,
            (tile => tile is HoeDirt hoeLand && hoeLand.crop != null && hoeLand.crop.dead.Value)),
        
        new BuffSpell(6,"Buff_VileVigour","Vile Vigour","Sacrifices a third of your max health to fill your energy",3,
            new Dictionary<int, int>() { {4297, 1},{4291,3}}, (f=> f is Farmer farmer && farmer.stamina < farmer.MaxStamina), SpellEffects.VileVigour,
                "My energy is already full"),
        
        new BuffSpell(7,"Buff_PieMake","Bake Pie","Cooks a random recipe that you know using your held ingredients",3,
            new Dictionary<int, int>() { {4298, 1},{4293,1},{4292,1}}, 
            (f=> f is Farmer farmer && farmer.cookingRecipes.Length > 0), SpellEffects.BakePie, "I don't know enough recipes"),
        
        new TeleportSpell(8,"Teleport_Desert","Desert Teleport","Teleports you to the desert, if you have access to it",5,
            new Dictionary<int, int>() { {4295, 2},{4294,1},{4293,1}}, "Desert", 19, 34,2,
            ((farmer => Game1.MasterPlayer.mailReceived.Contains("ccVault")))),
        
        new TeleportSpell(9,"Teleport_Ginger","Ginger Island Teleport","Teleports you to ginger island, if you have access to it",7,
            new Dictionary<int, int>() { {4295, 2},{4292,2},{4293,2}}, "IslandSouth",21,37,0,
            ((farmer => Game1.MasterPlayer.hasOrWillReceiveMail("willyBoatFixed")))),
        
        new TeleportSpell(10,"Teleport_Caves","Caves Teleport","Teleports you to the pelican town mines",2,
            new Dictionary<int, int>() { {4295, 1},{4291,5}}, "Mountain",54,7,0, 
            ((farmer => Game1.MasterPlayer.hasOrWillReceiveMail("landslideDone")))),
        
        new Spell(11,"Menu_WaterOrb","Charge Water Orb","NA Turns aquamarine into strong slingshot ammo",4,
            new Dictionary<int, int>() { {4297, 3},{4292,5}}),
        
        new Spell(12,"Menu_EarthOrb","Charge Earth Orb","NA Turns emeralds into stronger slingshot ammo",7,
            new Dictionary<int, int>() { {4297, 3},{4294,5}}),
        
        new Spell(13,"Buff_DarkLure","Dark Lure","NA Lures more enemies to you",6,
            new Dictionary<int, int>() { {4296, 2},{4297,2}}),
        
        new CombatSpell(14,"Combat_Wind","Wind Strike","A basic air missile",1,
            new Dictionary<int, int>() { {4299, 1},{4291,1}}, 25,15,0,Color.White),
       
        new CombatSpell(15,"Combat_Water","Water Bolt","A low level water missile",2,
            new Dictionary<int, int>() { {4299, 2},{4291,2},{4292,2}}, 35,16,1,Color.DarkCyan),
        
        new CombatSpell(16,"Combat_Undead","Crumble Undead","Hits undead monsters for extra damage",4,
            new Dictionary<int, int>() { {4299, 2},{4291,2},{4294,2}}, 30,13,3,Color.Yellow),
        
        new CombatSpell(17,"Combat_Earth","Earth Blast","A medium level earth missile",6,
            new Dictionary<int, int>() { {4300, 1},{4291,3},{4294,4}}, 60,16,1,Color.DarkGreen),
        
        new CombatSpell(18,"Combat_Fire","Fire Wave","A high level fire missile",8,
            new Dictionary<int, int>() { {4300, 2},{4291,5},{4293,5}}, 95,15,2,Color.OrangeRed),
        
        new Spell(19,"Buff_Charge","Charge","NA Increases the power of combat spells while active",7,
            new Dictionary<int, int>() { {4300, 3},{4291,3},{4293,3}}),
        
        new CombatSpell(20,"Combat_Demonbane","Demonbane","Hits undead monsters for a lot of extra damage",9,
            new Dictionary<int, int>() { {4300, 2},{4297,2},{4293,8}}, 65,13,3,Color.Purple),
        
        new CombatSpell(21,"Combat_Blood","Blood Barrage","Fires a strong vampiric blood missile",10,
            new Dictionary<int, int>() { {4300, 8},{4297,5}}, 80,15,1,Color.Crimson),
        
        new Spell(22,"Menu_Plank","Plank Make","NA Turns hardwood into wood and vice versa",3,
            new Dictionary<int, int>() { {4300, 2},{4298, 2},{4297,5}}), //"Converts 1 hardwood into 9 wood, or 15 wood into 1 hardwood"
        
        new Spell(23,"Buff_Heal","Heal","NA Restores your health in exchange for energy",8,
            new Dictionary<int, int>() { {4300, 3},{4291,3},{4293,3}}),
    };
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
                    expTilNextLevel = Farmer.getBaseExperienceForLevel(currentLevel);
                    
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
    
    //Perks:
    //Sapphire - Teleports are free but grant no xp
    //Emerald - Infinite Air Runes
    //Ruby - 15% chance of non-combat spells taking no runes
    //Dragonstone - Combat Spells give + 2 projectiles

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