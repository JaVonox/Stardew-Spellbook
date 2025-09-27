using StardewValley.GameData.Weapons;

namespace RunescapeSpellbook;

public class StaffWeaponData : WeaponData, ITranslatable
{
    public string id;
    public float projectileDamageModifier;
    public string? providesRune; //Which rune ID to grant infinite of for the sake of spells
    public int level;
    protected string translationKey;
    
    public StaffWeaponData(string id, string translationKey, int minSwingDamage, int maxSwingDamage, int spriteIndex, int level = 10, float projectileDamageModifier = 1.0f,
        string providesRune = "", int speedMod = 0, int precision = 0, int defenseMod = 0, float critChanceMod = 0.02f, float critDamageMod = 3f)
    {
        base.Name = id;
        this.translationKey = translationKey;
        base.DisplayName = translationKey;
        base.Description = translationKey;
        base.MinDamage = minSwingDamage;
        base.MaxDamage = maxSwingDamage;
        base.Speed = speedMod;
        base.Precision = precision;
        base.Defense = defenseMod;
        base.CritChance = critChanceMod;
        base.CritMultiplier = critDamageMod;
        base.SpriteIndex = spriteIndex;
        
        base.Type = 429;
        base.Texture = "Mods.RunescapeSpellbook.Assets.itemsprites";
        base.Knockback = 1.5f;
        base.CanBeLostOnDeath = false;
        
        this.id = id;
        this.projectileDamageModifier = projectileDamageModifier;
        this.providesRune = providesRune;
        this.level = level;
    }
    
    public virtual void ApplyTranslations()
    {
        base.DisplayName = KeyTranslator.GetTranslation($"weapon.{this.translationKey}.display-name");
        base.Description = KeyTranslator.GetTranslation($"weapon.{this.translationKey}.description");
    }
    
}