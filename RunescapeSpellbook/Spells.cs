using System.Security.AccessControl;
using Force.DeepCloner;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData.Weapons;
using StardewValley.TerrainFeatures;

namespace RunescapeSpellbook;
public delegate KeyValuePair<bool, string> TilesMethod(List<TerrainFeature> tiles);
public delegate KeyValuePair<bool, string> InventoryMethod(ref Item? itemArgs);
public delegate KeyValuePair<bool, string> BuffMethod();
public delegate void CombatExtraMethod(Farmer caster, NPC target, ref int damage, ref bool isBomb);

///<summary> Base class for all spells - effectively abstract. Attempting to cast this will always report an error </summary>
public class Spell
{
    public int id;
    public string name;
    public string displayName;
    public string description;
    public int magicLevelRequirement;
    public Dictionary<int,int> requiredItems; //Set of IDs for the required runes
    public int expReward;
    public string audioID;
    public Spell(int id, string name, string displayName, string description, int magicLevelRequirement, Dictionary<int,int> requiredItems, int expReward, string audioID = "HighAlch")
    {
        this.id = id;
        this.name = name;
        this.displayName = displayName;
        this.description = description;
        this.magicLevelRequirement = magicLevelRequirement;
        this.requiredItems = requiredItems;
        this.expReward = expReward;
        this.audioID = audioID;
    }
    
    protected bool HasMagicLevel()
    {
        return ModAssets.GetFarmerMagicLevel(Game1.player) >= magicLevelRequirement;
    }
    public virtual KeyValuePair<bool,string> CanCastSpell()
    {
        if (!HasMagicLevel())
        {
            return new KeyValuePair<bool,string>(false, "My magic level is not high enough to perform this spell");;
        }

        foreach (int runeID in requiredItems.Keys)
        {
            if (!HasRuneCost(runeID))
            {
                return new KeyValuePair<bool,string>(false, "I do not have enough runes to perform this spell");
            }
        }
        return new KeyValuePair<bool,string>(true, "");;
    }

    public virtual bool HasRuneCost(int runeID)
    {
        return (Game1.player.Items.CountId($"{runeID}") >= requiredItems[runeID] || 
                (runeID == 4291 && ModAssets.CheckHasPerkByName(Game1.player, "Emerald")));
    }

        
    /// <summary>
    /// The response when clicking the spell in the menu -
    /// for most spells this will instantly cast the spell, but for some (inventory + combat spells) it has different effects
    /// </summary>
    /// <returns>Bool for if the cast was successful, string for the error message</returns>
    public virtual KeyValuePair<bool,string> SelectSpell()
    {
        return new KeyValuePair<bool, string>(false,"Spell not yet implemented");
    }
    
    /// <summary>
    /// Removes the runes that are required for the cast spell 
    /// </summary>
    /// <param name="ignoreRune">Any runes that should not be decremented</param>
    protected void RemoveRunes(int ignoreRune = -1)
    {
        //Remove all required runes - any granted by staffs (ignoreRune) and air runes if we have the emerald perk
        bool hasInfiniteAir = ModAssets.CheckHasPerkByName(Game1.player, "Emerald");
        List<int> skippedItems = requiredItems.Where(x=>x.Key == ignoreRune || (x.Key == 4291 && hasInfiniteAir)).Select(x=>x.Key).ToList();

        //If we have the ruby perk we have a 20% chance of skipping the cost entirely
        if (GetType() != typeof(CombatSpell) && ModAssets.CheckHasPerkByName(Game1.player, "Ruby") && Game1.random.NextDouble() <= 0.2)
        {
            return;
        }
        
        foreach (KeyValuePair<int, int> runeCost in requiredItems) //Remove runes if we have successfully cast the spell
        {
            if(skippedItems.Contains(runeCost.Key)) {continue;}
            Game1.player.Items.ReduceId($"{runeCost.Key}", runeCost.Value);
        }
    }

    protected virtual void AddExperience()
    {
        ModAssets.IncrementMagicExperience(Game1.player,expReward);
    }

}

///<summary>Subclass of spell which permits teleporting to a set location on the map </summary>
public class TeleportSpell : Spell
{
    private string location;
    private int xPos;
    private int yPos;
    private int dir;
    
    ///<summary>Additional conditions aside from the global holiday ban - such as preventing teleports to ginger island if
    /// the island has not yet been unlocked </summary>
    /// <returns>True if the requirements for the teleport are met</returns>
    private Predicate<Farmer>? extraTeleportReqs;
    public TeleportSpell(int id, string name, string displayName, string description, int magicLevelRequirement, Dictionary<int, int> requiredItems, int expReward,
        string location, int xPos, int yPos, int dir, Predicate<Farmer>? extraTeleportReqs = null):
        base(id, name, displayName, description, magicLevelRequirement, requiredItems,expReward,"Teleport")
    {
        this.location = location;
        this.xPos = xPos;
        this.yPos = yPos;
        this.dir = dir;
        this.extraTeleportReqs = extraTeleportReqs;
    }
    public KeyValuePair<bool,string> Teleport()
    {
        //Disable teleporting if there is an active festival or event
        if (Utility.isFestivalDay(Game1.dayOfMonth,Game1.season) || Utility.IsPassiveFestivalDay(Game1.dayOfMonth,Game1.season,null))
        {
            return new KeyValuePair<bool, string>(false,"It's dangerous to teleport on special days");
        }
        if (extraTeleportReqs != null && !extraTeleportReqs(Game1.player)) //Check the extra conditions (such as teleport being unlocked)
        {
            return new KeyValuePair<bool, string>(false,"I don't know how to teleport there");
        }

        //Warp once the animation ends
        BaseSpellEffects.PlayAnimation(() =>
        {
            Game1.warpFarmer(location, xPos, yPos, dir);
            Game1.player.temporarilyInvincible = false;
            Game1.player.temporaryInvincibilityTimer = 0;
            Game1.player.freezePause = 0;
        },"RunescapeSpellbook.Teleport",2000);
        
        return new KeyValuePair<bool, string>(true,"");
    }
    public override KeyValuePair<bool, string> SelectSpell()
    {
        KeyValuePair<bool, string> actionResult = CanCastSpell();

        if (actionResult.Key) //First pass of action result checks if we can actually cast the selected spell - either due to level or rune cost etc.
        {
            actionResult = Teleport();
            
            if(actionResult.Key) //Second pass checks if there are any spell specific issues - like how teleporting is forbidden on festival days
            {
                if (!ModAssets.CheckHasPerkByName(Game1.player, "Sapphire")) //Having the sapphire perk means no exp for teleport spells
                {
                    RemoveRunes();
                    AddExperience();
                }
            }
            
        }
        
        return actionResult;
    }
    
    public override bool HasRuneCost(int runeID)
    {
        return (Game1.player.Items.CountId($"{runeID}") >= requiredItems[runeID] ||
                (runeID == 4291 && ModAssets.CheckHasPerkByName(Game1.player, "Emerald")) 
                || ModAssets.CheckHasPerkByName(Game1.player, "Sapphire"));
    }
}

///<summary>Subclass of spell which permits a player to affect tiles in an area around them </summary>
public class TilesSpell : Spell
{
    private int baseSize;
    private string noTilesMessage;
    private float perTileExp;
    
    ///<summary>Requirements for selecting the specific terrain that the spell will impact - such as if the spot is unwatered for humidity</summary>
    ///<returns>True if the requirements for the teleport are met</returns>
    private Predicate<TerrainFeature>? terrainReqs;
    
    ///<summary>Specifies a function to be ran with the set of tiles collected via the terrainReqs predicate</summary>
    private TilesMethod doAction; 
    public TilesSpell(int id, string name, string displayName, string description, int magicLevelRequirement, Dictionary<int, int> requiredItems, float perTileExp, TilesMethod doAction, int baseSize, string AudioID = "HighAlch",
        Predicate<TerrainFeature>? terrainReqs = null, string noTilesMessage = "Couldn't find any tiles to cast on"):
        base(id, name, displayName, description, magicLevelRequirement, requiredItems, 0,AudioID)
    {
        this.terrainReqs = terrainReqs;
        this.doAction = doAction;
        this.baseSize = baseSize;
        this.noTilesMessage = noTilesMessage;
        this.perTileExp = perTileExp;
    }
    public List<TerrainFeature> GetTiles(Vector2 playerTile, GameLocation currentLoc, int size)
    {
        //Gets all tiles in a size area around the player and then outputs the set to have effects applied to
        List<TerrainFeature> tileLocations = new List<TerrainFeature>();

        for (int y = -(size/2); y < (size/2); y++)
        {
            for (int x = -(size/2); x < (size/2); x++)
            {
                Vector2 accessedLocation = playerTile + new Vector2(x, y);
                if (currentLoc.terrainFeatures.TryGetValue(accessedLocation, out var terrainFeature) && terrainReqs(terrainFeature))
                {
                    tileLocations.Add(terrainFeature);
                }
            }
        }

        return tileLocations;
    }
    
    public override KeyValuePair<bool, string> SelectSpell()
    {
        KeyValuePair<bool, string> actionResult = CanCastSpell();

        if (actionResult.Key)
        {
            List<TerrainFeature> tilesToCastOn = GetTiles(Game1.player.Tile, Game1.player.currentLocation, baseSize).ToList();
            
            if (tilesToCastOn.Count == 0)
            {
                return new KeyValuePair<bool, string>(false,this.noTilesMessage);
            }

            doAction(tilesToCastOn);
            
            RemoveRunes();
            AddExperiencePerTile(tilesToCastOn.Count);
            
            return new KeyValuePair<bool, string>(true,"");
            
        }
        
        return actionResult;
    }
    
    protected void AddExperiencePerTile(int tileCount)
    {
        ModAssets.IncrementMagicExperience(Game1.player,Math.Max(2,(int)Math.Floor(perTileExp * (float)tileCount)));
    }
}
///<summary> Subclass of spell which opens a new menu, allowing a user to specify which item the spell will be applied upon </summary>
public class InventorySpell : Spell
{
    ///<summary>Requirements for an item to be accepted for the spell - as well as highlighted in the inventory menu.</summary>
    /// <remarks>Does not specify the amount of an item that must exist, this needs to be specified in the doAction </remarks>
    ///<returns>True if the item is valid</returns>
    private Predicate<object>? highlightPredicate;
    
    ///<summary>Specifies a function to be ran when the player has put an appropriate item in the item slot</summary>
    private InventoryMethod doAction;
    
    ///<summary>Description placed on the side menu to detail specific mechanics</summary>
    public string longDescription;

    /// <summary>
    /// the offset from 16 y in the spellanimations.xnb to use for the inventory spell
    /// </summary>
    public int spellAnimOffset;
    public InventorySpell(int id, string name, string displayName, string description, int magicLevelRequirement, Dictionary<int, int> requiredItems, int expReward, Predicate<object>? highlightPredicate, InventoryMethod doAction, string longDescription, string AudioID = "HighAlch", int spellAnimOffset = 0):
        base(id, name, displayName, description, magicLevelRequirement, requiredItems,expReward,AudioID)
    {
        this.highlightPredicate = highlightPredicate;
        this.doAction = doAction;
        this.longDescription = longDescription;
        this.spellAnimOffset = spellAnimOffset;
    }
    
    public KeyValuePair<bool, string> IsItemValidForOperation(ref Item? itemArgs)
    {
        if (itemArgs == null || !highlightPredicate(itemArgs))
        {
            return new KeyValuePair<bool, string>(false,$"Invalid Item");
        }
        
        return new KeyValuePair<bool, string>(true,$"");

    }
    
    ///<summary>This override for the select spell function does not actually cast the spell, instead it opens the menu for the actual spell casting</summary>
    /// <remarks>See CastSpell for the actual spell cast</remarks>
    /// <returns>Bool for if the cast was successful, string for the error message</returns>
    public override KeyValuePair<bool, string> SelectSpell()
    {
        KeyValuePair<bool, string> actionResult = CanCastSpell();

        if (actionResult.Key)
        {
            Game1.activeClickableMenu = new InventorySpellMenu(this, highlightPredicate);
        }

        return actionResult;
    }

    /// <summary>
    /// Casts the associated inventory spell with the provided item
    /// </summary>
    /// <param name="itemArgs">The item data for the item put in the input slot</param>
    /// <returns>Bool for if the cast was successful, string for the error message</returns>
    public KeyValuePair<bool, string> CastSpell(ref Item? itemArgs)
    {
        KeyValuePair<bool, string> operationReturn = IsItemValidForOperation(ref itemArgs);
        KeyValuePair<bool, string> actionResult = CanCastSpell();
        
        if (operationReturn.Key && actionResult.Key)
        {
            operationReturn = doAction(ref itemArgs);

            if (operationReturn.Key)
            {
                RemoveRunes();
                AddExperience();
            }
        }
        
        return operationReturn;
    }
}

///<summary> A general type of spell which just does an effect to a farmer. this can be anything from applying an effect or giving an item </summary>
public class BuffSpell : Spell
{
    ///<summary>Requirements for the farmer to be able to cast this spell.</summary>
    ///<returns>True if the cast can go ahead</returns>
    private Predicate<Farmer> farmerConditions;
    
    ///<summary>Specifies a function to be ran when we cast the spell</summary>
    private BuffMethod doAction;
    
    ///<summary>The message to display when a player does not meet the requirements for the spell specified in the farmerConditions predicate</summary>
    private string buffInvalidMessage;
    public BuffSpell(int id, string name, string displayName, string description, int magicLevelRequirement, Dictionary<int, int> requiredItems, int expReward, Predicate<Farmer> farmerConditions, BuffMethod doAction, string AudioID = "HighAlch",string buffInvalidMessage = "Couldn't cast spell"):
        base(id, name, displayName, description, magicLevelRequirement, requiredItems,expReward,AudioID)
    {
        this.farmerConditions = farmerConditions;
        this.buffInvalidMessage = buffInvalidMessage;
        this.doAction = doAction;
    }
    
    public KeyValuePair<bool, string> IsBuffValid(Farmer farmer)
    {
        if (!farmerConditions(farmer))
        {
            return new KeyValuePair<bool, string>(false,$"I can't use this spell right now");
        }
        
        return new KeyValuePair<bool, string>(true,$"");

    }
    
    public override KeyValuePair<bool, string> SelectSpell()
    {
        KeyValuePair<bool, string> actionResult = CanCastSpell();

        if (actionResult.Key)
        {
            actionResult = IsBuffValid(Game1.player);

            if (!actionResult.Key)
            {
                return new KeyValuePair<bool, string>(false, this.buffInvalidMessage);
            }
            else
            {
                actionResult = doAction();
                RemoveRunes();
                AddExperience();
            }
        }
        
        return actionResult;
    }
}

///<summary> A type of spell that specifies the effects of a spell when cast from a battlestaff </summary>
public class CombatSpell : Spell
{
    public int damage;
    private int projectileSpriteID;
    private float velocity;
    private bool explode;
    private string firingSound;
    private string collisionSound;
    private Color projectileColor;
    
    /// <summary>
    /// This is an extra effect that is applied using the monster 
    /// </summary>
    public CombatExtraMethod? combatEffect;

    //Sprite rotation offset is the amount of rotation we need to have to make it point upwards in the projectile (in degrees)
    public CombatSpell(int id, string name, string displayName, string description,
        int magicLevelRequirement, Dictionary<int, int> requiredItems, int expReward,
        int damage,float velocity,int projectileSpriteID, Color projectileColor,string firingSound = "HighAlch", CombatExtraMethod? combatEffect = null)
        : base(id, name, displayName, description, magicLevelRequirement, requiredItems,expReward,firingSound)
    {
        this.damage = damage;
        this.projectileSpriteID = projectileSpriteID;
        this.velocity = velocity;
        this.explode = explode;
        this.firingSound = "RunescapeSpellbook." + firingSound;
        this.collisionSound = "RunescapeSpellbook.Preserve";
        this.projectileColor = projectileColor;
        this.combatEffect = combatEffect;
    }
    
    public override bool HasRuneCost(int runeID)
    {
        if (Game1.player.Items.CountId($"{runeID}") >= requiredItems[runeID] || (runeID == 4291 && ModAssets.CheckHasPerkByName(Game1.player, "Emerald")))
        {
            return true;
        }

        //If we have a weapon in our inventory that allows for this, we can select Rune
        List<string> weaponIDs;
        if (ModAssets.infiniteRuneReferences.TryGetValue(runeID, out weaponIDs))
        {
            return weaponIDs.Any(x => Game1.player.Items.ContainsId(x));
        }
        
        return false;
    }

    ///<summary>This variant of can cast spell checks each step to see if we can cast the spell with our held item, not just from the inventory as
    /// the select spell method does for casting spells</summary>
    private KeyValuePair<bool, string> CanCastSpell(StaffWeaponData castingWeapon)
    {
        KeyValuePair<bool, string> canCast = base.CanCastSpell(); //First we check if we can cast using the base (magic level + has runes or any staff weapons in inventory)
        if (!canCast.Key)
        {
            return canCast;
        }
        
        foreach (int runeID in requiredItems.Keys) //If we can cast it, then we have to check if the weapon is the reason we can get the rune
        {
            //we now check every rune that the weapon doesnt provide, and if the base HasRuneCost says the rune cost is not sufficientt
            //then the spell cannot be cast (this catches the case where a player has a staff in their inventory that provides infinite runes but is not using it)
            if (castingWeapon.providesRune != runeID && !base.HasRuneCost(runeID))
            {
                return new KeyValuePair<bool, string>(false,$"I do not have enough runes to cast this spell with this staff");
            }
        }
        
        return new KeyValuePair<bool, string>(true,$"");
    }

    
    ///<summary> The effects when a player clicks the spell in the spellbook menu - this should set it so that the spell is selected for combat casts </summary>
    /// <returns>Bool for if the cast was successful, string for the error message</returns>
    public override KeyValuePair<bool, string> SelectSpell()
    {
        KeyValuePair<bool,string> actionResult = base.CanCastSpell(); //we use the base can cast spell to check if it is selectable - which is using the custom HasRuneCost
        
        ModAssets.localFarmerData.selectedSpellID = actionResult.Key && ModAssets.localFarmerData.selectedSpellID != this.id ? this.id : -1;
        
        return actionResult;
    }
    
    protected override void AddExperience()
    {
        ModAssets.IncrementMagicExperience(Game1.player,expReward);
    }
    
    const float extraProjectileOffsets = (float)(10 * (Math.PI/180));
    
    ///<summary> Generates the projectile specified by the spell to be spawned elsewhere </summary>
    public KeyValuePair<bool, string> CreateCombatProjectile(Farmer caster, StaffWeaponData castingWeapon, int x, int y, out List<MagicProjectile> projectile)
    {
        projectile = null;
        KeyValuePair<bool, string> actionResult = CanCastSpell(castingWeapon);
        if (actionResult.Key)
        {
            Vector2 mousePos = new Vector2(x, y);
            Vector2 characterPos = caster.getStandingPosition();

            //TODO maybe add gamepad functionality??
            Vector2 v = Utility.getVelocityTowardPoint(characterPos, mousePos, velocity);
            caster.faceGeneralDirection(mousePos);
            
            //Precompute the chance of crit so that we can give it a 1 or 0 chance for the projectile to sync crits
            float critChance = castingWeapon.CritChance +
                               (caster.hasBuff("statue_of_blessings_5") ? 0.1f : 0f) *
                               (caster.professions.Contains(25) ? 1.5f : 1f);
            
            int projectileCount = caster.hasBuff("429") || (ModAssets.CheckHasPerkByName(Game1.player, "Dragonstone") && Game1.random.NextDouble() <= 0.2) ? 3 : 1;
            
            List<MagicProjectile> generatedProjectiles = new List<MagicProjectile>();
            for(int i = 0; i < projectileCount; i++)
            {
                //The angle to offset projectiles from (first projectile is offset by 0, extras are +/- extraProjectileOffsets)
                Vector2 projectileVelocity;
                if (i == 0)
                {
                    projectileVelocity = v;
                }
                else //add extra projectiles if we have the perk and the random chance hits
                {
                    float instanceOffset = i % 2 == 0 ? extraProjectileOffsets : -extraProjectileOffsets;
                    float cosAngle = (float)Math.Cos(instanceOffset);
                    float sinAngle = (float)Math.Sin(instanceOffset);
                    projectileVelocity.X = v.X * cosAngle - v.Y * sinAngle;
                    projectileVelocity.Y = v.X * sinAngle + v.Y * cosAngle;
                }
                
                float projectileAngle = (float)(Math.Atan2(projectileVelocity.Y, projectileVelocity.X)) + (float)(Math.PI / 2);
            
                //if critdamage does not equal 0 we have not crit. anything else is used as the crit damage
                float critDamage = (Game1.random.NextDouble() < (double)(critChance + (float)caster.LuckLevel * (critChance / 40f))) ?
                    castingWeapon.CritMultiplier : 0;

                //Damage is +/- 20%
                MagicProjectile generatedProjectile = new MagicProjectile(
                    (int)Math.Floor(((float)damage * (castingWeapon.projectileDamageModifier +
                                                      (float)((0.4 * Game1.random.NextDouble()) - 0.2))))
                    ,
                    projectileSpriteID,
                    0,
                    0,
                    0,
                    projectileVelocity.X,
                    projectileVelocity.Y,
                    caster.getStandingPosition() - new Vector2(32f, 32f),
                    projectileAngle,
                    projectileColor,
                    critDamage,
                    this.id,
                    firingSound: firingSound,
                    collisionSound: collisionSound,
                    bounceSound: firingSound,
                    explode: explode,
                    damagesMonsters: true,
                    location: caster.currentLocation,
                    firer: caster);

                generatedProjectile.ignoreTravelGracePeriod.Value = true;
                generatedProjectile.ignoreMeleeAttacks.Value = true;
                generatedProjectile.maxTravelDistance.Value = 12 * 64;
                generatedProjectile.height.Value = 32f;
                
                generatedProjectiles.Add(generatedProjectile);
            }

            if (generatedProjectiles.Count > 0)
            {
                RemoveRunes(castingWeapon.providesRune);
                AddExperience(); //TODO maybe this should only count if you hit an enemy
            }

            projectile = generatedProjectiles;
        }

        return actionResult;
    }
}