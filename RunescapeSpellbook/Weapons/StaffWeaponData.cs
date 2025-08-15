using StardewValley.GameData.Weapons;

namespace RunescapeSpellbook;

public class StaffWeaponData : WeaponData
{
    public int id;
    public float projectileDamageModifier;
    public int providesRune; //Which rune ID to grant infinite of for the sake of spells
    
    public StaffWeaponData(int id, string name, string displayName, string description, int minSwingDamage, int maxSwingDamage, int spriteIndex, float projectileDamageModifier = 1.0f,
        int providesRune = -1, int speedMod = 0, int precision = 0, int defenseMod = 0, float critChanceMod = 0.02f, float critDamageMod = 3f)
    {
        base.Name = name;
        base.DisplayName = displayName;
        base.Description = description;
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
    }
    
}