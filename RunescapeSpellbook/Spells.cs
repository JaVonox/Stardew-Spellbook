using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace RunescapeSpellbook;
public delegate SpellResponse TilesMethod(List<TerrainFeature> tiles, int animOffset);
public delegate SpellResponse InventoryMethod(ref Item? itemArgs);
public delegate SpellResponse BuffMethod(int animOffset);
public delegate void CombatExtraMethod(Farmer caster, NPC target, ref int damage, ref bool isBomb);

///<summary> Base class for all spells - effectively abstract. Attempting to cast this will always report an error </summary>
public class Spell : ITranslatable
{
    public int id;
    public string name;
    public string displayName;
    public string description;
    public string translationKey;
    public int magicLevelRequirement;
    public Dictionary<string,int> requiredItems; //Set of IDs for the required runes
    public double expReward;
    public string audioID;
    
    /// <summary>
    /// the offset from 16 y in the spellanimations.xnb to use for the inventory spell
    /// </summary>
    public int spellAnimOffset;
    public Spell(int id, string name, string translationKey, int magicLevelRequirement, Dictionary<string,int> requiredItems, double expReward, int spellAnimOffset, string audioID = "HighAlch")
    {
        this.id = id;
        this.name = name;
        this.displayName = translationKey;
        this.description = translationKey;
        this.translationKey = translationKey;
        this.magicLevelRequirement = magicLevelRequirement;
        this.requiredItems = requiredItems;
        this.expReward = expReward;
        this.audioID = audioID;
        this.spellAnimOffset = spellAnimOffset;
    }
    
    public virtual void ApplyTranslations()
    {
        this.displayName = KeyTranslator.GetTranslation($"spell.{this.translationKey}.display-name");
        this.description = KeyTranslator.GetTranslation($"spell.{this.translationKey}.description");
    }

    protected bool HasMagicLevel()
    {
        return ModAssets.GetFarmerMagicLevel(Game1.player) >= magicLevelRequirement;
    }
    public virtual SpellResponse CanCastSpell()
    {
        if (!HasMagicLevel())
        {
            return new SpellResponse(false, "spell-error.LowMagicLevel.text");;
        }

        foreach (string runeID in requiredItems.Keys)
        {
            if (!HasRuneCost(runeID))
            {
                return new SpellResponse(false, "spell-error.NoRunes.text");
            }
        }
        return new SpellResponse(true);;
    }

    public virtual bool HasRuneCost(string runeID)
    {
        return (Game1.player.Items.CountId($"{runeID}") >= requiredItems[runeID] || 
                (runeID == "Tofu.RunescapeSpellbook_RuneAir" && ModAssets.CheckHasPerkByName(Game1.player, "Emerald")));
    }

        
    /// <summary>
    /// The response when clicking the spell in the menu -
    /// for most spells this will instantly cast the spell, but for some (inventory + combat spells) it has different effects
    /// </summary>
    /// <returns>Bool for if the cast was successful, string for the error message</returns>
    public virtual SpellResponse SelectSpell()
    {
        return new SpellResponse(false,"spell-error.SpellUnimplemented.text");
    }
    
    /// <summary>
    /// Removes the runes that are required for the cast spell 
    /// </summary>
    /// <param name="ignoreRune">Any runes that should not be decremented</param>
    protected void RemoveRunes(string? ignoreRune = null)
    {
        //Remove all required runes - any granted by staffs (ignoreRune) and air runes if we have the emerald perk
        bool hasInfiniteAir = ModAssets.CheckHasPerkByName(Game1.player, "Emerald");
        List<string> skippedItems = requiredItems.Where(x=>x.Key == ignoreRune || (x.Key == "Tofu.RunescapeSpellbook_RuneAir" && hasInfiniteAir)).Select(x=>x.Key).ToList();

        //If we have the ruby perk we have a 20% chance of skipping the cost entirely
        if (GetType() != typeof(CombatSpell) && ModAssets.CheckHasPerkByName(Game1.player, "Ruby") && Game1.random.NextDouble() <= 0.2)
        {
            return;
        }
        
        foreach (KeyValuePair<string, int> runeCost in requiredItems) //Remove runes if we have successfully cast the spell
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
    public TeleportSpell(int id, string name, string translationKey, int magicLevelRequirement, Dictionary<string, int> requiredItems, double expReward,
        string location, int xPos = 0, int yPos = 0, int dir = 2, Predicate<Farmer>? extraTeleportReqs = null):
        base(id, name, translationKey, magicLevelRequirement, requiredItems,expReward,4,"Teleport")
    {
        this.location = location;
        this.xPos = xPos;
        this.yPos = yPos;
        this.dir = dir;
        this.extraTeleportReqs = extraTeleportReqs;
    }
    
    public SpellResponse Teleport()
    {
        //Prevent teleporting out of temporary places - event locations
        if (GameLocation.IsTemporaryName(Game1.player.currentLocation.Name))
        {
            return new SpellResponse(false,"spell-error.TeleportFestivalInside.text");
        }
        
        //Disable teleporting if there is an active festival or event
        if (Utility.isFestivalDay(Game1.dayOfMonth,Game1.season))
        {
            bool loadData = Event.tryToLoadFestivalData($"{Utility.getSeasonKey(Game1.season)}{Game1.dayOfMonth}", out var _, out var _, out var eventLocationName, out var _, out var _);
            if (loadData && eventLocationName == location)
            {
                return new SpellResponse(false,"spell-error.TeleportFestivalOutside.text");
            }
        }

        if (extraTeleportReqs != null && !extraTeleportReqs(Game1.player)) //Check the extra conditions (such as teleport being unlocked)
        {
            return new SpellResponse(false,"spell-error.TeleportUnknown.text");
        }

        string newLoc = location;
        int newXpos = xPos;
        int newYpos = yPos;
        int newDir = dir;
        
        if (location == "FarmHouse")
        {
            Farm mainFarm = Game1.getFarm();
            newLoc = mainFarm.Name;
            Point doorSpot = mainFarm.GetMainFarmHouseEntry();
            newXpos = doorSpot.X;
            newYpos = doorSpot.Y;
            newDir = 2;
        }

        //Warp once the animation ends
        BaseSpellEffects.PlayAnimation(() =>
        {
            Game1.warpFarmer(newLoc, newXpos, newYpos, newDir);
            Game1.player.temporarilyInvincible = false;
            Game1.player.temporaryInvincibilityTimer = 0;
            Game1.player.freezePause = 0;
        },"RunescapeSpellbook.Teleport",800,spellAnimOffset);
        
        return new SpellResponse(true);
    }
    public override SpellResponse SelectSpell()
    {
        SpellResponse actionResult = CanCastSpell();

        if (actionResult.wasSpellSuccessful) //First pass of action result checks if we can actually cast the selected spell - either due to level or rune cost etc.
        {
            actionResult = Teleport();
            
            if(actionResult.wasSpellSuccessful) //Second pass checks if there are any spell specific issues - like how teleporting is forbidden on festival days
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
    
    public override bool HasRuneCost(string runeID)
    {
        return (Game1.player.Items.CountId($"{runeID}") >= requiredItems[runeID] ||
                (runeID == "Tofu.RunescapeSpellbook_RuneAir" && ModAssets.CheckHasPerkByName(Game1.player, "Emerald")) 
                || ModAssets.CheckHasPerkByName(Game1.player, "Sapphire"));
    }
}

///<summary>Subclass of spell which permits a player to affect tiles in an area around them </summary>
public class TilesSpell : Spell
{
    private int baseSize;
    private double perTileExp;
    
    ///<summary>Requirements for selecting the specific terrain that the spell will impact - such as if the spot is unwatered for humidity</summary>
    ///<returns>True if the requirements for the teleport are met</returns>
    private Predicate<TerrainFeature>? terrainReqs;
    
    ///<summary>Specifies a function to be ran with the set of tiles collected via the terrainReqs predicate</summary>
    private TilesMethod doAction; 
    public TilesSpell(int id, string name, string translationKey, int magicLevelRequirement, Dictionary<string, int> requiredItems, double perTileExp, TilesMethod doAction, int baseSize, int spellAnimOffset, string AudioID = "HighAlch",
        Predicate<TerrainFeature>? terrainReqs = null):
        base(id, name, translationKey, magicLevelRequirement, requiredItems, 0,spellAnimOffset,AudioID)
    {
        this.terrainReqs = terrainReqs;
        this.doAction = doAction;
        this.baseSize = baseSize;
        this.perTileExp = perTileExp;
    }
    public List<TerrainFeature> GetTiles(Vector2 playerTile, GameLocation currentLoc, int size)
    {
        //Gets all tiles in a size area around the player and then outputs the set to have effects applied to
        List<TerrainFeature> tileLocations = new();

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
    
    public override SpellResponse SelectSpell()
    {
        SpellResponse actionResult = CanCastSpell();

        if (actionResult.wasSpellSuccessful)
        {
            List<TerrainFeature> tilesToCastOn = GetTiles(Game1.player.Tile, Game1.player.currentLocation, baseSize).ToList();
            
            if (tilesToCastOn.Count == 0)
            {
                return new SpellResponse(false,"spell-error.TilesNoTilesDefault.text");
            }

            SpellResponse result = doAction(tilesToCastOn,spellAnimOffset);

            if (!result.wasSpellSuccessful) //Check if we have any additional issues
            {
                return result;
            }
            
            RemoveRunes();
            AddExperiencePerTile(tilesToCastOn.Count);
            
            return new SpellResponse(true);
            
        }
        
        return actionResult;
    }
    
    protected void AddExperiencePerTile(int tileCount)
    {
        ModAssets.IncrementMagicExperience(Game1.player,perTileExp * ((double)tileCount));
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
    
    public InventorySpell(int id, string name, string translationKey, int magicLevelRequirement, Dictionary<string, int> requiredItems, double expReward, Predicate<object>? highlightPredicate, InventoryMethod doAction, int spellAnimOffset, string AudioID = "HighAlch"):
        base(id, name, translationKey, magicLevelRequirement, requiredItems,expReward,spellAnimOffset,AudioID)
    {
        this.highlightPredicate = highlightPredicate;
        this.doAction = doAction;
        this.longDescription = translationKey;
    }
    
    public override void ApplyTranslations()
    {
        base.ApplyTranslations();
        this.longDescription = KeyTranslator.GetTranslation($"spell.{this.translationKey}.long-description");
    }
    
    public SpellResponse IsItemValidForOperation(ref Item? itemArgs)
    {
        if (itemArgs == null || !highlightPredicate(itemArgs))
        {
            return new SpellResponse(false,"spell-error.MenuInvalidItem.text");
        }
        
        return new SpellResponse(true);

    }
    
    ///<summary>This override for the select spell function does not actually cast the spell, instead it opens the menu for the actual spell casting</summary>
    /// <remarks>See CastSpell for the actual spell cast</remarks>
    /// <returns>Bool for if the cast was successful, string for the error message</returns>
    public override SpellResponse SelectSpell()
    {
        SpellResponse actionResult = CanCastSpell();

        if (actionResult.wasSpellSuccessful)
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
    public SpellResponse CastSpell(ref Item? itemArgs)
    {
        SpellResponse operationReturn = IsItemValidForOperation(ref itemArgs);
        SpellResponse actionResult = CanCastSpell();
        
        if (operationReturn.wasSpellSuccessful && actionResult.wasSpellSuccessful)
        {
            operationReturn = doAction(ref itemArgs);

            if (operationReturn.wasSpellSuccessful)
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
    private string buffInvalidTranslationKey;
    public BuffSpell(int id, string name, string translationKey, int magicLevelRequirement, Dictionary<string, int> requiredItems, double expReward, Predicate<Farmer> farmerConditions, BuffMethod doAction, int spellAnimOffset,string AudioID = "HighAlch",string buffInvalidTranslationKey = "spell-error.GeneralFail.text"):
        base(id, name, translationKey, magicLevelRequirement, requiredItems,expReward,spellAnimOffset,AudioID)
    {
        this.farmerConditions = farmerConditions;
        this.buffInvalidTranslationKey = buffInvalidTranslationKey;
        this.doAction = doAction;
    }
    
    public SpellResponse IsBuffValid(Farmer farmer)
    {
        if (!farmerConditions(farmer))
        {
            return new SpellResponse(false,"spell-error.BuffInvalidBuffDefault.text");
        }
        
        return new SpellResponse(true,$"");

    }
    
    public override SpellResponse SelectSpell()
    {
        SpellResponse actionResult = CanCastSpell();

        if (actionResult.wasSpellSuccessful)
        {
            actionResult = IsBuffValid(Game1.player);

            if (!actionResult.wasSpellSuccessful)
            {
                return new SpellResponse(false, this.buffInvalidTranslationKey);
            }
            else
            {
                actionResult = doAction(spellAnimOffset);
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
    private Color projectileColor;
    
    /// <summary>
    /// This is an extra effect that is applied using the monster 
    /// </summary>
    public CombatExtraMethod? combatEffect;

    //Sprite rotation offset is the amount of rotation we need to have to make it point upwards in the projectile (in degrees)
    public CombatSpell(int id, string name, string translationKey,
        int magicLevelRequirement, Dictionary<string, int> requiredItems, double expReward,
        int damage,float velocity,int projectileSpriteID, Color projectileColor,string firingSound = "HighAlch", CombatExtraMethod? combatEffect = null)
        : base(id, name, translationKey, magicLevelRequirement, requiredItems,expReward,projectileSpriteID,firingSound)
    {
        this.damage = damage;
        this.projectileSpriteID = projectileSpriteID;
        this.velocity = velocity;
        this.firingSound = "RunescapeSpellbook." + firingSound;
        this.projectileColor = projectileColor;
        this.combatEffect = combatEffect;
    }
    
    public override bool HasRuneCost(string runeID)
    {
        if (Game1.player.Items.CountId($"{runeID}") >= requiredItems[runeID] || (runeID == "Tofu.RunescapeSpellbook_RuneAir" && ModAssets.CheckHasPerkByName(Game1.player, "Emerald")))
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
    private SpellResponse CanCastSpell(StaffWeaponData castingWeapon)
    {
        SpellResponse canCast = base.CanCastSpell(); //First we check if we can cast using the base (magic level + has runes or any staff weapons in inventory)
        if (!canCast.wasSpellSuccessful)
        {
            return canCast;
        }
        
        foreach (string runeID in requiredItems.Keys) //If we can cast it, then we have to check if the weapon is the reason we can get the rune
        {
            //we now check every rune that the weapon doesnt provide, and if the base HasRuneCost says the rune cost is not sufficientt
            //then the spell cannot be cast (this catches the case where a player has a staff in their inventory that provides infinite runes but is not using it)
            if (castingWeapon.providesRune != runeID && !base.HasRuneCost(runeID))
            {
                return new SpellResponse(false,"spell-error.CombatWrongStaff.text");
            }
        }
        
        return new SpellResponse(true,$"");
    }

    
    ///<summary> The effects when a player clicks the spell in the spellbook menu - this should set it so that the spell is selected for combat casts </summary>
    /// <returns>Bool for if the cast was successful, string for the error message</returns>
    public override SpellResponse SelectSpell()
    {
        SpellResponse actionResult = base.CanCastSpell(); //we use the base can cast spell to check if it is selectable - which is using the custom HasRuneCost
        
        ModAssets.TrySetModVariable(Game1.player,"Tofu.RunescapeSpellbook_SelectedSpellID",actionResult.wasSpellSuccessful ? this.id.ToString() : "-1");
        
        return actionResult;
    }
    
    protected override void AddExperience()
    {
        ModAssets.IncrementMagicExperience(Game1.player,expReward);
    }
    
    const float extraProjectileOffsets = (float)(10 * (Math.PI/180));
    
    ///<summary> Generates the projectile specified by the spell to be spawned elsewhere </summary>
    public SpellResponse CreateCombatProjectile(Farmer caster, StaffWeaponData castingWeapon, int x, int y, out List<MagicProjectile> projectile)
    {
        projectile = null;
        SpellResponse actionResult = CanCastSpell(castingWeapon);
        if (actionResult.wasSpellSuccessful)
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
            
            int projectileCount = caster.hasBuff("Tofu.RunescapeSpellbook_BuffCharge") || (ModAssets.CheckHasPerkByName(Game1.player, "Dragonstone") && Game1.random.NextDouble() <= 0.2) ? 3 : 1;
            
            List<MagicProjectile> generatedProjectiles = new();
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
                    firingSound: projectileCount > 1 ? "RunescapeSpellbook.MultiHit" :  firingSound,
                    collisionSound: "",
                    bounceSound: "",
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

            if (generatedProjectiles.Count > 0 && !(caster.hasBuff("Tofu.RunescapeSpellbook_BuffBattlemage") && Game1.random.NextDouble() <= 0.1f))
            {
                RemoveRunes(castingWeapon.providesRune);
                AddExperience();
            }

            projectile = generatedProjectiles;
        }

        return actionResult;
    }
}