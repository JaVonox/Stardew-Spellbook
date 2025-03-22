using Microsoft.Xna.Framework;
using StardewTestMod;
using StardewValley;

namespace StardewTestMod;

public delegate KeyValuePair<bool, string> SpellMethod(ref Item? i, Predicate<object>? p);
public class Spell
{
    public int id;
    public string name;
    public string displayName;
    public string description;
    public SpellType spellType;
    public int magicLevelRequirement;
    public int spriteIndex;
    public string longDescription;
    
    public SpellMethod DoAction; //The function to use. returns a bool for if it was successful, string as any output args, and may take an item as an Input
    public Dictionary<int,int> requiredItems; //Set of IDs for the required runes - add duplicates to designate more than 1 item required
    public Predicate<object>? castPredicate; //Miscellanious predicate to determine if there is any extra conditions that must be met - spell effects dictate where to use this
    public Spell(int id, string name, string displayName, string description, SpellType spellType, int magicLevelRequirement, int spriteIndex, Dictionary<int,int> requiredItems, SpellMethod DoAction, Predicate<object>? castPredicate = null, string longDescription = null)
    {
        this.id = id;
        this.name = name;
        this.displayName = displayName;
        this.description = description;
        this.spellType = spellType;
        this.magicLevelRequirement = magicLevelRequirement;
        this.spriteIndex = spriteIndex;
        this.requiredItems = requiredItems;
        this.DoAction = DoAction;
        this.castPredicate = castPredicate;
        this.longDescription = longDescription;
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

    public virtual KeyValuePair<bool,string> CastSpell(bool isInventorySpellMenu, ref Item? itemArgs)
    {
        KeyValuePair<bool,string> actionResult = CanCastSpell();
        if (actionResult.Key) //First pass of action result checks if we can actually cast the selected spell - either due to level or rune cost etc.
        {
            switch (spellType)
            {
                //If the spell is teleport or map utility, we can just cast the spell immediately
                case SpellType.InventoryUtility: //If we are using an inventory utility spell, we need to open the inventory spell menu first
                    if (!isInventorySpellMenu) //If we're not in the inventory menu,
                    {
                        Game1.activeClickableMenu = new InventorySpellMenu(this,castPredicate);
                        break;
                    }
                    goto default; //Fallthrough if we are already in the inventory spell menu
                case SpellType.Combat: //Combat spells have their own unique class
                    break;
                case SpellType.Teleport:
                case SpellType.MapUtility:
                case SpellType.Buff:
                default:
                    actionResult = DoAction(ref itemArgs,castPredicate);
            
                    if(actionResult.Key) //Second pass checks if there are any spell specific issues - like how teleporting is forbidden on festival days
                    {
                        foreach (KeyValuePair<int, int> runeCost in requiredItems) //Remove runes if we have successfully cast the spell
                        {
                            Game1.player.Items.ReduceId($"{runeCost.Key}", runeCost.Value); 
                        }
                    }

                    break;
            }
        }

        return actionResult;
    }

    public void RemoveRunes()
    {
        foreach (KeyValuePair<int, int> runeCost in requiredItems) //Remove runes if we have successfully cast the spell
        {
            Game1.player.Items.ReduceId($"{runeCost.Key}", runeCost.Value); 
        }
    }

    public virtual MagicProjectile? CreateCombatProjectile(Farmer caster)
    {
        return null;
    }
}

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
        int magicLevelRequirement, int spriteIndex, Dictionary<int, int> requiredItems
        , int damage,float velocity,int projectileSpriteID, Color projectileColor, bool explode = false, string firingSound = "wand", string collisionSound = "wand")
        : base(id, name, displayName, description, SpellType.Combat, magicLevelRequirement, spriteIndex, requiredItems,
            null)
    {
        this.damage = damage;
        this.projectileSpriteID = projectileSpriteID;
        this.velocity = velocity;
        this.explode = explode;
        this.firingSound = firingSound;
        this.collisionSound = collisionSound;
        this.projectileColor = projectileColor;
        
        
    }

    public virtual KeyValuePair<bool, string> CastSpell(bool isInventorySpellMenu, ref Item? itemArgs)
    {
        KeyValuePair<bool,string> actionResult = CanCastSpell();
        if (actionResult.Key) //First pass of action result checks if we can actually cast the selected spell - either due to level or rune cost etc.
        {
            
        }
        
        return actionResult;
    }
    
    public override MagicProjectile? CreateCombatProjectile(Farmer caster)
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
        
        base.RemoveRunes();
        
        return generatedProjectile;
    }
}