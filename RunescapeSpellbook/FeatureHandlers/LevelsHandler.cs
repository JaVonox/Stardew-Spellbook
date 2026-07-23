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
        int startLevel = GetFarmerMagicLevel(farmer);
        if (startLevel >= 10)
        {
            return;
        }
        
        int expMultiplier = 0;
        int.TryParse(ModAssets.TryGetModVariable(Game1.player, "Tofu.RunescapeSpellbook_Setting-MagicExpMultiplier"),
            out expMultiplier);
        expMultiplier = expMultiplier == 0 ? 100 : expMultiplier;
        
        double multiplier = shouldUseMultiplier ? expMultiplier / 100.0 : 1.0;
        
        int newAddedExperience = (int)(Math.Floor((gainedExperience * multiplier)));

        SpaceCoreApi.AddExperienceForCustomSkill(farmer,"Tofu.RunescapeSpellbook.MagicSkill",newAddedExperience);
        if (startLevel < GetFarmerMagicLevel(farmer))
        {
            farmer.currentLocation.localSound("RunescapeSpellbook.MagicLevel", null, null);
            Game1.addHUDMessage(new HUDMessage(KeyTranslator.GetTranslation("skill.RunescapeMagic.level-up-1"),2));
        }
    }

    public static bool HasPerk(Farmer farmer, string perkID)
    {
        if (perkID == "magicSapphire")
        {
            return farmer.HasCustomProfession(MagicSkill.professionsSet["magicSapphire"]) || farmer.HasCustomProfession(MagicSkill.professionsSet["magicSapphireB"]);
        }
        if (perkID == "magicDragon")
        {
            return farmer.HasCustomProfession(MagicSkill.professionsSet["magicDragon"]) || farmer.HasCustomProfession(MagicSkill.professionsSet["magicDragonB"]);
        }
        
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

            //THIS NEEDS TO BE IN ORDER (from level 5 perks to level 10 perks) or spacecore will bug and allow multiple selections
            professionsSet = new Dictionary<string, MagicProfession>()
            {
                {"magicRuby",new MagicProfession(this, "magicRuby", "Ruby")},
                {"magicEmerald",new MagicProfession(this, "magicEmerald", "Emerald")},
                {"magicSapphire",new MagicProfession(this, "magicSapphire", "Sapphire")},
                {"magicSapphireB",new MagicProfession(this, "magicSapphireB", "Sapphire")},
                {"magicDragon",new MagicProfession(this, "magicDragon", "Dragonstone")},
                {"magicDragonB",new MagicProfession(this, "magicDragonB", "Dragonstone")}
            };

            foreach (Profession prof in professionsSet.Values)
            {
                this.Professions.Add(prof);
            }
            
            this.ProfessionsForLevels.Add(new ProfessionPair(5, professionsSet["magicEmerald"], professionsSet["magicRuby"]));
            this.ProfessionsForLevels.Add(new ProfessionPair(10,professionsSet["magicSapphire"],professionsSet["magicDragon"],professionsSet["magicEmerald"]));
            this.ProfessionsForLevels.Add(new ProfessionPair(10,professionsSet["magicSapphireB"],professionsSet["magicDragonB"],professionsSet["magicRuby"]));
        }

        public override List<string> GetExtraLevelUpInfo(int level)
        {
            List<string> levelUpInfo = new(){ KeyTranslator.GetTranslation("skill.RunescapeMagic.level-up-1")};

            List<Spell>? newSpells = ModAssets.modSpells.Where(x => x.magicLevelRequirement == level).ToList();
            if (newSpells != null && newSpells.Count > 0)
            {
                levelUpInfo.Add(KeyTranslator.GetTranslation("skill.RunescapeMagic.level-up-2"));
                levelUpInfo.AddRange(newSpells.Select(y=>y.displayName));
            }

            return levelUpInfo;
        }
        public override string GetName() => KeyTranslator.GetTranslation("skill.RunescapeMagic.display-name");
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
