using Microsoft.Xna.Framework.Graphics;
using SpaceCore;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewValley;

namespace RunescapeSpellbook;

public static class LevelsHandler
{
    public static ISpaceCoreApi SpaceCoreApi;
    public static bool hasCheckedForMigration = false;
    public static Dictionary<string, Texture2D> skillTextures;
    public static void Load(ISpaceCoreApi api)
    {
        SpaceCoreApi = api;
    }

    public static void LoadSkillTextures(Dictionary<string, Texture2D> newSkillTextures)
    {
        skillTextures = newSkillTextures;
    }
    private static void MigrateToSpacecore(Farmer farmerInstance)
    {
        if (farmerInstance.modData.ContainsKey("Tofu.RunescapeSpellbook_MagicLevel"))
        {
            farmerInstance.modData.Remove("Tofu.RunescapeSpellbook_MagicLevel");
        }

        if (farmerInstance.modData.ContainsKey("Tofu.RunescapeSpellbook_MagicExp"))
        {
            IncrementMagicExperience(farmerInstance,100 * double.Parse(ModAssets.TryGetModVariable(farmerInstance, "Tofu.RunescapeSpellbook_MagicExp")));
            farmerInstance.modData.Remove("Tofu.RunescapeSpellbook_MagicExp");
        }
        
        if (farmerInstance.modData.ContainsKey("Tofu.RunescapeSpellbook_MagicProf1"))
        {
            farmerInstance.modData.Remove("Tofu.RunescapeSpellbook_MagicProf1");
        }
        
        if (farmerInstance.modData.ContainsKey("Tofu.RunescapeSpellbook_MagicProf2"))
        {
            farmerInstance.modData.Remove("Tofu.RunescapeSpellbook_MagicProf2");
        }
    }

    private static void CheckMigrations(Farmer farmer)
    {
        if (!hasCheckedForMigration)
        { 
           hasCheckedForMigration = true;
           MigrateToSpacecore(farmer);
        }
    }
    public static bool HasMagic(Farmer farmer)
    {
        CheckMigrations(farmer);
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
        CheckMigrations(farmer);
        return SpaceCoreApi.GetLevelForCustomSkill(farmer, "Tofu.RunescapeSpellbook.MagicSkill");
    }

    public static void IncrementMagicExperience(Farmer farmer, double gainedExperience, bool shouldUseMultiplier = true)
    {
        if (GetFarmerMagicLevel(farmer) >= 10)
        {
            return;
        }
        
        int expMultiplier = 100;
        int.TryParse(ModAssets.TryGetModVariable(Game1.player, "Tofu.RunescapeSpellbook_Setting-MagicExpMultiplier"),
            out expMultiplier);
        double multiplier = shouldUseMultiplier ? expMultiplier / 100.0 : 1.0;
        int newAddedExperience = (int)(Math.Floor((gainedExperience * multiplier)));

        SpaceCoreApi.AddExperienceForCustomSkill(farmer,"Tofu.RunescapeSpellbook.MagicSkill",newAddedExperience);
    }

    public static bool HasPerk(Farmer farmer, string perkID)
    {
        return farmer.HasCustomProfession(MagicSkill.professionsSet[perkID]);
    }
    
    public class MagicSkill : Skills.Skill
    {
        public static Dictionary<string, MagicProfession> professionsSet;
        public MagicSkill() : base("Tofu.RunescapeSpellbook.MagicSkill")
        {
            this.Icon = LevelsHandler.skillTextures["BigIcon"];
            this.SkillsPageIcon = LevelsHandler.skillTextures["SkillIcon"];
            
            this.ExperienceCurve = new[] { 10000, 38000, 77000, 130000, 215000, 330000, 400000, 690000, 1000000, 1500000 };
            this.ExperienceBarColor = new Microsoft.Xna.Framework.Color(27, 6, 146);

            professionsSet = new Dictionary<string, MagicProfession>()
            {
                {"magicSapphire",new MagicProfession(this, "magicSapphire", "Sapphire")},
                {"magicRuby",new MagicProfession(this, "magicRuby", "Ruby")},
                {"magicEmerald",new MagicProfession(this, "magicEmerald", "Emerald")},
                {"magicDragon",new MagicProfession(this, "magicDragon", "Dragonstone")}
            };

            foreach (Profession prof in professionsSet.Values)
            {
                this.Professions.Add(prof);
            }
            
            this.ProfessionsForLevels.Add(new ProfessionPair(5,professionsSet["magicEmerald"],professionsSet["magicRuby"]));
            this.ProfessionsForLevels.Add(new ProfessionPair(10,professionsSet["magicSapphire"],professionsSet["magicDragon"]));
        }

        public override List<string> GetExtraLevelUpInfo(int level)
        {
            return new List<string>() { "Congratulations, you just advanced a Magic level.", "You've unlocked access to new spells!" }; //TODO translation
        }
        
        //TODO add level up effects (sound etc.)
    
        public override string GetName() => "Runic Magic"; //TODO translation
    }

    public class MagicProfession : Skills.Skill.Profession
    {
        public override string GetName() => KeyTranslator.GetTranslation($"perk.{this.translationKey}.display-name");
        public override string GetDescription() => KeyTranslator.GetTranslation($"perk.{this.translationKey}.description-1");

        private string translationKey;
        public MagicProfession(Skills.Skill skill, string id, string translationKey) : base(skill,id)
        {
            this.translationKey = translationKey;
            base.Icon = LevelsHandler.skillTextures[translationKey];
        }
    }
}
