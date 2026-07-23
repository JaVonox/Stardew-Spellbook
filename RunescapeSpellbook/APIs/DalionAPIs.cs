namespace RunescapeSpellbook.APIs;

public interface IProfessionsApi
{
    IProfessionsConfig GetConfig();
}

public interface IProfessionsConfig
{
    public ISkillsConfig Skills { get; }
}

public interface ISkillsConfig
{
    public Dictionary<string, float> BaseMultipliers { get; }
}

public interface ICombatApi
{
    ICombatConfig GetConfig();
}

public interface ICombatConfig
{
    public Dictionary<string, int> ComboHitsPerWeaponType { get; }
}