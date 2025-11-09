namespace RunescapeSpellbook;

public struct ItemDrop
{
    public string itemID;
    public int amount;
    public double chance;

    public int minAmount;
    public int maxAmount;

    public int quality = 0;
    public ItemDrop(string itemID, int amount, double chance = 1.0)
    {
        this.itemID = itemID;
        this.amount = amount;
        this.chance = chance;
        
        this.minAmount = amount;
        this.maxAmount = amount;
    }
    public ItemDrop(string itemID, int minAmount, int maxAmount, double weight = 1.0, int quality = 0) 
    {
        this.itemID = itemID;
        this.amount = minAmount;
        this.minAmount = minAmount;
        this.maxAmount = maxAmount;
        this.chance = weight;
        this.quality = quality;
    }
    
}
public enum PrefType
{
    Hate,
    Dislike,
    Neutral,
    Like,
    Love
}

public class SpellResponse
{
    public bool wasSpellSuccessful;
    private string? translationString;
    private object? replacements;
    public SpellResponse(bool wasSpellSuccessful, string? translationKey = null, object? replacements = null)
    {
        this.wasSpellSuccessful = wasSpellSuccessful;
        this.translationString = translationKey;
        this.replacements = replacements;
    }

    private string? cachedTranslation;
    public string translatedResponse
    {
        get
        {
            if (cachedTranslation == null)
            {
                cachedTranslation = translationString == null 
                    ? "" 
                    : KeyTranslator.GetTranslation(translationString, replacements);
            }
            return cachedTranslation;
        }
    }
}

public static class KeyTranslator
{
    public static Func<string, object?, string> TranslationFunc;
    public static string GetTranslation(string translationKey, object? replacements = null)
    {
        return TranslationFunc(translationKey, replacements);
    }
}