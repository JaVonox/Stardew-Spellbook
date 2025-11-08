using SpaceShared.APIs;
using StardewValley;

namespace RunescapeSpellbook;

public class VirtualCurrencyHandler
{
    private static ISpaceCoreApi SpaceCoreApi;
    private string myID;
    private string itemAssociated;
    public VirtualCurrencyHandler(string myID, string myAssociated)
    {
        this.myID = myID;
        itemAssociated = myAssociated;
    }
    public static void Load(ISpaceCoreApi api)
    {
        SpaceCoreApi = api;
    }

    public void TruncateToDailyCap(Farmer who)
    {
        int amount = GetCurrencyValue(who);
        int difference = amount - GetDailyCap(who);

        if (difference > 0)
        {
            AddCurrency(who,difference * -1);
        }
    }
    public int GetDailyCap(Farmer who)
    {
        return (LevelsHandler.HasMagic(who) ? 20 : 0) + (LevelsHandler.GetFarmerMagicLevel(who) * 20);
    }
    
    public int GetCurrencyValue(Farmer who)
    {
        return SpaceCoreApi.GetVirtualCurrencyAmount(who, myID);
    }
        
    public void AddCurrency(Farmer who, int amount)
    {
        SpaceCoreApi.AddToVirtualCurrency(who,myID,amount);
    }
    
}