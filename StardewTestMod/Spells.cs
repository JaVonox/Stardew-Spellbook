﻿using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace StardewTestMod;
public delegate KeyValuePair<bool, string> TilesMethod(List<TerrainFeature> tiles);
public delegate KeyValuePair<bool, string> InventoryMethod(ref Item? itemArgs);
public delegate KeyValuePair<bool, string> BuffMethod();

///<summary> Base class for all spells - effectively abstract. Attempting to cast this will always report an error </summary>
public class Spell
{
    public int id;
    public string name;
    public string displayName;
    public string description;
    public int magicLevelRequirement;
    public Dictionary<int,int> requiredItems; //Set of IDs for the required runes
    public Spell(int id, string name, string displayName, string description, int magicLevelRequirement, Dictionary<int,int> requiredItems)
    {
        this.id = id;
        this.name = name;
        this.displayName = displayName;
        this.description = description;
        this.magicLevelRequirement = magicLevelRequirement;
        this.requiredItems = requiredItems;
    }
    
    public KeyValuePair<bool,string> CanCastSpell()
    {
        //TODO add magic level checking
        foreach (int runeID in requiredItems.Keys)
        {
            if (!HasRuneCost(runeID))
            {
                return new KeyValuePair<bool,string>(false, "I do not have enough runes to perform this spell");
            }
        }
        return new KeyValuePair<bool,string>(true, "");;
    }

    public bool HasRuneCost(int runeID)
    {
        return (Game1.player.Items.CountId($"{runeID}") >= requiredItems[runeID]);
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
    
    public void RemoveRunes()
    {
        foreach (KeyValuePair<int, int> runeCost in requiredItems) //Remove runes if we have successfully cast the spell
        {
            Game1.player.Items.ReduceId($"{runeCost.Key}", runeCost.Value); 
        }
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
    public TeleportSpell(int id, string name, string displayName, string description, int magicLevelRequirement, Dictionary<int, int> requiredItems,
        string location, int xPos, int yPos, int dir,Predicate<Farmer>? extraTeleportReqs = null):
        base(id, name, displayName, description, magicLevelRequirement, requiredItems)
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
        },"wand",2000);
        
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
                RemoveRunes();
            }
            
        }
        
        return actionResult;
    }
}

///<summary>Subclass of spell which permits a player to affect tiles in an area around them </summary>
public class TilesSpell : Spell
{
    private int baseSize;
    private string noTilesMessage;
    
    ///<summary>Requirements for selecting the specific terrain that the spell will impact - such as if the spot is unwatered for humidity</summary>
    ///<returns>True if the requirements for the teleport are met</returns>
    private Predicate<TerrainFeature>? terrainReqs;
    
    ///<summary>Specifies a function to be ran with the set of tiles collected via the terrainReqs predicate</summary>
    private TilesMethod doAction; 
    public TilesSpell(int id, string name, string displayName, string description, int magicLevelRequirement, Dictionary<int, int> requiredItems, TilesMethod doAction, int baseSize,
        Predicate<TerrainFeature>? terrainReqs = null, string noTilesMessage = "Couldn't find any tiles to cast on"):
        base(id, name, displayName, description, magicLevelRequirement, requiredItems)
    {
        this.terrainReqs = terrainReqs;
        this.doAction = doAction;
        this.baseSize = baseSize;
        this.noTilesMessage = noTilesMessage;
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
            
            return new KeyValuePair<bool, string>(true,"");
            
        }
        
        return actionResult;
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
    public InventorySpell(int id, string name, string displayName, string description, int magicLevelRequirement, Dictionary<int, int> requiredItems, Predicate<object>? highlightPredicate, InventoryMethod doAction, string longDescription):
        base(id, name, displayName, description, magicLevelRequirement, requiredItems)
    {
        this.highlightPredicate = highlightPredicate;
        this.doAction = doAction;
        this.longDescription = longDescription;
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

        if (operationReturn.Key)
        {
            operationReturn = doAction(ref itemArgs);

            if (operationReturn.Key)
            {
                RemoveRunes();
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
    public BuffSpell(int id, string name, string displayName, string description, int magicLevelRequirement, Dictionary<int, int> requiredItems, Predicate<Farmer> farmerConditions, BuffMethod doAction, string buffInvalidMessage = "Couldn't cast spell"):
        base(id, name, displayName, description, magicLevelRequirement, requiredItems)
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
            }
        }
        
        return actionResult;
    }
}

///<summary> A type of spell that specifies the effects of a spell when cast from a battlestaff </summary>
public class CombatSpell : Spell
{
    private int damage;
    private int projectileSpriteID;
    private float velocity;
    private bool explode;
    private string firingSound;
    private string collisionSound;
    private Color projectileColor;

    //Sprite rotation offset is the amount of rotation we need to have to make it point upwards in the projectile (in degrees)
    public CombatSpell(int id, string name, string displayName, string description,
        int magicLevelRequirement, Dictionary<int, int> requiredItems
        , int damage,float velocity,int projectileSpriteID, Color projectileColor, bool explode = false, string firingSound = "wand", string collisionSound = "wand")
        : base(id, name, displayName, description, magicLevelRequirement, requiredItems)
    {
        this.damage = damage;
        this.projectileSpriteID = projectileSpriteID;
        this.velocity = velocity;
        this.explode = explode;
        this.firingSound = firingSound;
        this.collisionSound = collisionSound;
        this.projectileColor = projectileColor;
    }
    
    ///<summary> The effects when a player clicks the spell in the spellbook menu - this should set it so that the spell is selected for combat casts </summary>
    /// <returns>Bool for if the cast was successful, string for the error message</returns>
    public virtual KeyValuePair<bool, string> SelectSpell()
    {
        KeyValuePair<bool,string> actionResult = CanCastSpell();
        if (actionResult.Key) 
        {
            
        }
        
        return actionResult;
    }
    
    ///<summary> Generates the projectile specified by the spell to be spawned elsewhere </summary>
    public MagicProjectile? CreateCombatProjectile(Farmer caster)
    {
        float shotAngle = 0f;
        float rotationRequired = 0f;
        switch (caster.facingDirection.Value)
        {
            case 0: //Up
                shotAngle = 90f;
                rotationRequired = 0f;
                break;
            case 1: //Right
                shotAngle = 0f;
                rotationRequired = 90f;
                break;
            case 2: //Down
                shotAngle = 270f;
                rotationRequired = 180f;
                break;
            case 3: //Left
                shotAngle = 180f;
                rotationRequired = 270f;
                break;
        }

        rotationRequired *= (float)Math.PI / 180f;
        shotAngle *= (float)Math.PI / 180f;
        
        MagicProjectile generatedProjectile = new MagicProjectile(
            damage,
            projectileSpriteID, 
            0, 
            0, 
            0, 
            (float)velocity * (float)Math.Cos(shotAngle), 
            (float)velocity * (float)(0.0 - Math.Sin(shotAngle)),
            caster.getStandingPosition() - new Vector2(32f, 32f), 
            rotationRequired,
            projectileColor,
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
        
        RemoveRunes();
        
        return generatedProjectile;
    }
}