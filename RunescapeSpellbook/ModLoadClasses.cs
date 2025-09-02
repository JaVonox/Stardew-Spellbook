using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.GameData;
using StardewValley.GameData.BigCraftables;
using StardewValley.GameData.Buffs;
using StardewValley.GameData.Crops;
using StardewValley.GameData.FishPonds;
using StardewValley.GameData.Locations;
using StardewValley.GameData.Machines;
using StardewValley.GameData.Objects;
using StardewValley.GameData.Shops;

namespace RunescapeSpellbook;
public struct ItemDrop
{
    public string itemID;
    public int amount;
    public double chance;

    public int minAmount;
    public int maxAmount;
    public ItemDrop(string itemID, int amount, double chance = 1.0)
    {
        this.itemID = itemID;
        this.amount = amount;
        this.chance = chance;
        
        this.minAmount = amount;
        this.maxAmount = amount;
    }
    public ItemDrop(string itemID, int minAmount, int maxAmount, double weight = 1.0) 
    {
        this.itemID = itemID;
        this.amount = minAmount;
        this.minAmount = minAmount;
        this.maxAmount = maxAmount;
        this.chance = weight;
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
public class ModLoadObjects : ObjectData
{
    public string id;
    public Dictionary<string, PrefType>? characterPreferences;
    public ModLoadObjects(string id, string displayName, string description, int spriteIndex, Dictionary<string, PrefType>? characterPreferences, string type = "Basic", int category = -2)
    {
        this.id = id;
        base.Name = id;
        base.DisplayName = displayName;
        base.Description = description;
        base.Type = type;
        base.Texture = "Mods.RunescapeSpellbook.Assets.itemsprites";
        base.SpriteIndex = spriteIndex;
        base.Category = category;
        base.ExcludeFromRandomSale = true;
        this.characterPreferences = characterPreferences ?? new();
        base.Price = 1;
    }

    public void AppendObject(IDictionary<string,ObjectData> ObjectsSet)
    {
        ObjectsSet[$"{id}"] = this;
    }
}
public class RunesObjects : ModLoadObjects
{
    public RunesObjects(string id,string displayName, string description, int spriteIndex,int category,Dictionary<string, PrefType>? characterPreferences = null) : 
        base(id,displayName,description,spriteIndex,characterPreferences,"Basic",category)
    {
        base.Price = 2;
    }
    
}
public class SlingshotItem : ModLoadObjects
{
    public int extraDamage = 0;
    public int debuffType = 0;
    public bool explodes = false;
    public SlingshotItem(string id, string displayName, string description, int spriteID, int extraDamage, int debuffType, bool explodes = false, Dictionary<string, PrefType>? characterPreferences = null) : 
        base(id,displayName,description,spriteID,characterPreferences,"Basic",-2)
    {
        this.debuffType = debuffType;
        this.explodes = explodes;
        this.extraDamage = extraDamage;
    }
}
public class TreasureObjects : ModLoadObjects
{
    public TreasureObjects(string id,  string displayName, string description, int spriteID,
        List<ItemDrop> itemDrops, int sellprice = 35, Dictionary<string, PrefType>? characterPreferences = null) :
        base(id, displayName, description,spriteID,characterPreferences, "Basic", -28)
    {
        List<ObjectGeodeDropData> objects = new();

        double totalWeight = itemDrops.Sum(itemDrop => itemDrop.chance);

        int dropID = 0;
        foreach (ItemDrop item in itemDrops)
        {
            ObjectGeodeDropData geodeItem = new ObjectGeodeDropData();
            geodeItem.Id = dropID.ToString();
            geodeItem.ItemId = item.itemID.ToString();

            geodeItem.MinStack = item.minAmount;
            geodeItem.MaxStack = item.maxAmount;
            
            geodeItem.Chance = BalanceItemPercentage(itemDrops,dropID,totalWeight);
            geodeItem.Precedence = 0;
            objects.Add(geodeItem);
            dropID++;
        }
        
        base.GeodeDropsDefaultItems = false;
        base.GeodeDrops = objects;
        base.Price = sellprice;
    }


    public TreasureObjects(string id, string displayName, string description, int spriteID, Dictionary<string, PrefType>? characterPreferences) : 
        base(id,displayName,description,spriteID,characterPreferences,"Basic",-28)
    {
        List<ObjectGeodeDropData> objects = new();
        
        ObjectGeodeDropData geodeItem = new ObjectGeodeDropData();
        geodeItem.Id = "0";
        geodeItem.ItemId = "Tofu.RunescapeSpellbook_RuneAir";
        geodeItem.Chance = 1.0;
        geodeItem.MinStack = 10;
        geodeItem.MaxStack = 20;
        geodeItem.Precedence = 0;
        objects.Add(geodeItem);

        base.GeodeDropsDefaultItems = false;
        base.GeodeDrops = objects;
        base.Price = 35;
    }

    /// <summary>
    /// Finds what the new percentage should be for an index in the array to match its expected percentage chance -
    /// assuming that we work sequentially
    /// </summary>
    /// <returns></returns>
    protected static double BalanceItemPercentage(List<ItemDrop> items, int calculatedIndex, double totalWeight)
    {
        double desiredChance = (items[calculatedIndex].chance / totalWeight); //The specified chance of this item drop occuring divided by the total chances
        if (calculatedIndex == 0)
        {
            return desiredChance; //If its the first index we always have the desired chance
        }
        
        if (calculatedIndex == items.Count - 1)
        {
            return 1; //final item will always have 100% chance
        }

        double divisor = 1;

        for(int i = calculatedIndex - 1; i >= 0;i--)
        {
            divisor *= (1 - BalanceItemPercentage(items,i,totalWeight)); 
        }
        
        return desiredChance / divisor;
    }
}

public class PackObject : TreasureObjects
{
    public string packItem;
    public int packBaseIncrease;
    
    public static readonly int PACK_BASE_MULT = 7;
    public static readonly double[] PACK_CHANCES = {1.5,0.5,0.2};
    public static readonly int[] PACK_TIERED_MULTIPLIERS = { 1, 2, 4, 5 };
    public PackObject(string id, string displayName, string description, int spriteID, string packItem, int packBaseIncrease = 0) :
        base(id, displayName, description, spriteID,null)
    {
        this.packItem = packItem;
        this.packBaseIncrease = packBaseIncrease;
        List<ItemDrop> itemDrops = GetItemRanges();

        for (int i = 0; i < PACK_CHANCES.Length;i++)
        {
            itemDrops.Add( new ItemDrop(packItem,(PACK_BASE_MULT * (PACK_TIERED_MULTIPLIERS[i])) + this.packBaseIncrease,(PACK_BASE_MULT * (PACK_TIERED_MULTIPLIERS[i+1])) + this.packBaseIncrease - 1,PACK_CHANCES[i]));
        }
        
        base.SpriteIndex = spriteID;
        List<ObjectGeodeDropData> objects = new();

        double totalWeight = itemDrops.Sum(itemDrop => itemDrop.chance);

        int dropID = 0;
        foreach (ItemDrop item in itemDrops)
        {
            ObjectGeodeDropData geodeItem = new ObjectGeodeDropData();
            geodeItem.Id = dropID.ToString();
            geodeItem.ItemId = item.itemID.ToString();

            geodeItem.MinStack = item.minAmount;
            geodeItem.MaxStack = item.maxAmount;
            
            geodeItem.Chance = BalanceItemPercentage(itemDrops,dropID,totalWeight);
            geodeItem.Precedence = 0;
            objects.Add(geodeItem);
            dropID++;
        }
        
        base.GeodeDropsDefaultItems = false;
        base.GeodeDrops = objects;
    }

    public List<ItemDrop> GetItemRanges()
    {
        List<ItemDrop> itemDrops = new List<ItemDrop>();

        for (int i = 0; i < PACK_CHANCES.Length;i++)
        {
            itemDrops.Add( new ItemDrop(packItem, (PACK_BASE_MULT * (PACK_TIERED_MULTIPLIERS[i])) + this.packBaseIncrease, ((PACK_BASE_MULT * (PACK_TIERED_MULTIPLIERS[i+1])) + this.packBaseIncrease - 1),PACK_CHANCES[i]));
        }
        return itemDrops;
    }
}
public class FishObject : ModLoadObjects
{
    private int dartChance;
    private int minDayTime;
    private int maxDayTime;
    private List<Season> seasons;
    private string weather;
    private List<string> locations;
    private int catchChance;
    private int minFishingLevel;
    private Color fishTypeWaterColour;
    private Dictionary<int, List<string>> populationGates;
    private Dictionary<int, ItemDrop> rewards;
    private int spawnTime;
    private int minLength;
    private int maxLength;
    
    public FishObject(string id,string displayName, string description, int spriteID, int dartChance, int minDayTime, 
        int maxDayTime, List<Season> seasons, string weather, List<string> locations, int catchChance, int minFishingLevel, int price, int edibility, int spawnTime,
        Color waterColour, string roeColour, int minLength, int maxLength, Dictionary<int, List<string>> populationGates, Dictionary<int, ItemDrop> rewards, Dictionary<string,PrefType> characterPrefs = null)
        : base(id, displayName, description,spriteID,characterPrefs,"Basic",-4)
    {
        this.dartChance = dartChance;
        this.minDayTime = minDayTime;
        this.maxDayTime = maxDayTime;
        this.seasons = seasons;
        this.weather = weather;
        this.locations = locations;
        this.catchChance = catchChance;
        this.minFishingLevel = minFishingLevel;
        base.Price = price;
        base.ExcludeFromFishingCollection = true;
        base.Edibility = edibility;
        base.ContextTags = new() {$"item_{id}",roeColour};
        
        this.fishTypeWaterColour = waterColour;
        this.populationGates = populationGates;
        this.rewards = rewards;
        this.spawnTime = spawnTime;
        this.minLength = minLength;
        this.maxLength = maxLength;
    }

    public void AppendFishData(IDictionary<string,string> fishDict)
    {
        string seasonsText = "";
        foreach (Season season in seasons)
        {
            if (seasonsText != "") { seasonsText += " ";}
            seasonsText += season.ToString();
        }

        fishDict.Add($"{this.id}", $"{this.Name}/{this.dartChance}/dart/{this.minLength}/{this.maxLength}/{this.minDayTime} {this.maxDayTime}/{seasonsText}/{this.weather}/690 .4 685 .1/2/.{this.catchChance}/.5/{this.minFishingLevel}/false");
    }

    public void AppendPondData(IList<FishPondData> pondData)
    {
        FishPondData newPondData = new FishPondData();
        newPondData.Id = this.Name.ToString();
        newPondData.RequiredTags = new(){$"item_{base.Name}"};
        newPondData.PopulationGates = this.populationGates;
        newPondData.ProducedItems = new();
        newPondData.SpawnTime = spawnTime;
            
        foreach (KeyValuePair<int,ItemDrop> pondDrop in rewards)
        {
            FishPondReward pondReward = new FishPondReward();
            pondReward.RequiredPopulation = pondDrop.Key;
            pondReward.Chance = (float)pondDrop.Value.chance;
            pondReward.MinStack = pondDrop.Value.minAmount;
            pondReward.MaxStack = pondDrop.Value.maxAmount;
            pondReward.ItemId = $"{pondDrop.Value.itemID}";
            newPondData.ProducedItems.Add(pondReward);
        }
        
        FishPondWaterColor waterColour = new FishPondWaterColor();
        waterColour.Color = $"{fishTypeWaterColour.R} {fishTypeWaterColour.G} {fishTypeWaterColour.B}";
        waterColour.MinPopulation = 0;
        waterColour.MinUnlockedPopulationGate = 0;
        
        newPondData.WaterColor = new() { waterColour };
        pondData.Add(newPondData);
    }
    public void AppendLocationData(IDictionary<string, LocationData> locationSet)
    {
        foreach (string loc in this.locations)
        {
            SpawnFishData fishData = new SpawnFishData();
            fishData.ItemId = $"(O){base.id}";
            fishData.Chance = float.Parse($"0.{this.catchChance}"); 
            fishData.MinFishingLevel = this.minFishingLevel;
            fishData.MinDistanceFromShore = 2;
            fishData.MaxDistanceFromShore = -1;
            
            string seasonsText = "";
            foreach (Season season in seasons)
            {
                if (seasonsText != "") { seasonsText += " ";}
                seasonsText += season.ToString();
            }
            
            fishData.Condition = $"SEASON {seasonsText}";
            
            locationSet[loc].Fish.Add(fishData);
        }
    }
}
public class SeedObject : ModLoadObjects
{
    public SeedObject(string id, string displayName, string description, int spriteIndex, int price, Dictionary<string, PrefType>? characterPreferences = null) : base(id,displayName,description,spriteIndex,characterPreferences,"Basic",-74)
    {
        base.Price = price;
    }
}
public class CropObject : ModLoadObjects
{
    private List<Season> growableSeasons;
    private int phases;
    private int daysPerPhase;
    private string seedID;
    private int growthSpriteRow;
    private float harvestIncPerFarmLevel;
    private int harvestAmount;
    private HarvestMethod harvestMethod;
    public CropObject(string harvestItemId, string displayName, string description, string seedId, List<Season> growableSeasons, int daysPerPhase, int growthSpriteRow,
        int spriteID, int price, int edibility, string colour, int category = -75, int harvestAmount = 1, float harvestIncPerFarmLevel = 0, Dictionary<string,PrefType>? characterPreferences = null, HarvestMethod harvestMethod = HarvestMethod.Grab)
    : base(harvestItemId,displayName,description,spriteID,characterPreferences,"Basic",category)
    {
        this.seedID = seedId;
        this.growableSeasons = growableSeasons;
        this.daysPerPhase = daysPerPhase;
        this.harvestIncPerFarmLevel = harvestIncPerFarmLevel;
        this.harvestMethod = harvestMethod;
        this.growthSpriteRow = growthSpriteRow + (growthSpriteRow % 2);
        
        base.ContextTags = new() {colour};
        this.harvestAmount = harvestAmount;
        base.Price = price;
        base.Edibility = edibility;
    }

    public void AppendCropData(IDictionary<string,CropData> cropDict)
    {
        CropData cropInfo = new CropData();
        cropInfo.Texture = "Mods.RunescapeSpellbook.Assets.modplants";
        cropInfo.SpriteIndex = growthSpriteRow;
        cropInfo.Seasons = growableSeasons;
        cropInfo.DaysInPhase = new() { daysPerPhase, daysPerPhase, daysPerPhase, daysPerPhase};
        cropInfo.HarvestItemId = $"(O){base.id}";
        cropInfo.HarvestMaxIncreasePerFarmingLevel = harvestIncPerFarmLevel;
        cropInfo.HarvestMethod = this.harvestMethod;
        cropInfo.HarvestMinStack = harvestAmount;
        cropDict[this.seedID] = cropInfo;
    }
}

public class PotionObject : ModLoadObjects
{
    public float healPercent = 0;
    public float extraHealthPerQuality = 0;
    public int grantBuffID = 0;
    public int duration = 0;
    
    /// <summary>
    /// 0 is Craftable, 1 is Keg, 2 is Preserve
    /// </summary>
    public int craftType;
    /// <summary>
    /// For keg - item of id that makes this potion in the keg
    /// For cookable - crafting string for making the potion
    /// </summary>
    public string? creationString = null;
    public int creationTime = 180;
        
    //Keg item (Usable)
    public PotionObject(string id, string displayName, string description, int spriteIndex, int price, float healPercent, float extraHealthPerQuality, string kegItemID, int creationTime, Dictionary<string, PrefType>? characterPreferences = null) :
        base(id,displayName,description,spriteIndex,characterPreferences,"Basic",-26)
    {
        this.healPercent = healPercent;
        this.extraHealthPerQuality = extraHealthPerQuality;
        this.craftType = 1;
        this.creationString = kegItemID;
        this.creationTime = creationTime;
        this.Price = price;
        base.Edibility = 0;
        base.IsDrink = true;
    }
    
    //Preservable (Unusable)
    public PotionObject(string id, string displayName, string description, int spriteIndex, int price, string preserveItemID, int creationTime, string colour, Dictionary<string, PrefType>? characterPreferences = null) :
        base(id,displayName,description,spriteIndex,characterPreferences,"Basic",-26)
    {
        this.craftType = 2;
        this.creationString = preserveItemID;
        this.creationTime = creationTime;
        this.Price = price;
        base.Edibility = -300;
        base.ContextTags = new() {colour};
    }
    
    //Cooking (Usable)
    public PotionObject(string id, string displayName, string description, int spriteIndex, int price, string cookingRecipe, List<string> buffs, Dictionary<string, PrefType>? characterPreferences = null) :
        base(id,displayName,description,spriteIndex,characterPreferences,"Basic",-7)
    {
        this.craftType = 0;
        this.creationString = cookingRecipe;
        this.Price = price;
        base.Edibility = 0;
        base.IsDrink = true;

        if (buffs.Count > 0)
        {
            base.Buffs = new List<ObjectBuffData>(); 
            foreach (string buff in buffs)
            {
                ObjectBuffData newBuff = new ObjectBuffData();
                newBuff.BuffId = buff;
                base.Buffs.Add(newBuff);
            }
        }
    }

    public void AddMachineOutput(IDictionary<string,MachineData> machineDict)
    {
        MachineOutputTriggerRule newMachineOutputTrigger = new MachineOutputTriggerRule();
        newMachineOutputTrigger.Id = $"RS_{base.Name}";
        newMachineOutputTrigger.RequiredItemId = $"(O){this.creationString}";

        MachineItemOutput newMachineOutput = new MachineItemOutput();
        newMachineOutput.ItemId =  $"(O){base.id}";
        newMachineOutput.Id = $"RS_{base.Name}";
        newMachineOutput.CopyQuality = true;

        MachineOutputRule newOutputRule = new MachineOutputRule();
        newOutputRule.Id = $"RS_{base.Name}";
        newOutputRule.Triggers = new(){newMachineOutputTrigger};
        newOutputRule.OutputItem= new(){newMachineOutput};
        newOutputRule.MinutesUntilReady = this.creationTime;
        
        machineDict[craftType == 1 ? "(BC)12" : "(BC)15"].OutputRules.Insert(0,newOutputRule);
    }

    public void AddCookingOutput(IDictionary<string, string> cookingDict)
    {
        cookingDict[base.Name] = $"{this.creationString}/1 /{base.id}";
    }
}

public class MachinesObject : BigCraftableData
{
    public string id;
    private Dictionary<string, string> inToOut;
    private int inputAmount;
    private Func<string,List<ItemDrop>> amountReturnMethod;
    private string? additionalItemRequired;
    private string? failMessage;
    private int returnRolls;
    
    public string? creationString;
    public MachinesObject(string id, string displayname, string description, int price, int spriteIndex, Dictionary<string,string> inToOut, int inputAmount, Func<string,List<ItemDrop>> amountReturnMethod, string craftingRecipe, int returnRolls = 1,string? additionalItemRequired = null, string? failMessage = "")
    {
        this.id = id;
        base.Name = id;
        base.DisplayName = displayname;
        base.Description = description;
        base.Price = price;
        base.Texture = "Mods.RunescapeSpellbook.Assets.modmachines";
        base.SpriteIndex = spriteIndex;
        this.inToOut = inToOut;
        this.inputAmount = inputAmount;
        this.amountReturnMethod = amountReturnMethod;
        this.additionalItemRequired = additionalItemRequired;
        this.creationString = craftingRecipe;
        this.failMessage = failMessage;
        this.returnRolls = returnRolls;
    }
    
    public void AddMachineRules(IDictionary<string,MachineData> machineDict)
    {
        MachineData targetMachineData = new MachineData();
        targetMachineData.OutputRules = new List<MachineOutputRule>();
        
        foreach (KeyValuePair<string, string> processables in inToOut)
        {
            List<ItemDrop> itemRanges = (amountReturnMethod.Invoke(processables.Key)).OrderBy(x => x.chance).ToList(); //Get the item ranges ordered by chance lowest to highest
            
            MachineOutputRule nRule = new MachineOutputRule();
            nRule.MinutesUntilReady = 10;
            nRule.Id = $"RS_{this.id}_{processables.Key}";

            MachineOutputTriggerRule nTrigRule = new MachineOutputTriggerRule();
            nTrigRule.Id = $"RS_{this.id}_T_{processables.Key}";
            nTrigRule.RequiredItemId = $"(O){processables.Key}";
            nTrigRule.RequiredCount = inputAmount;

            MachineItemOutput outItem = new MachineItemOutput();
            outItem.Id = $"RS_{this.id}_T_{processables.Key}";
            outItem.ItemId = $"(O){processables.Value}";
            outItem.QualityModifierMode = QuantityModifier.QuantityModifierMode.Stack;
            
            List<List<float>> quantityRanges = new List<List<float>>();
            for (int x = 0; x < itemRanges.Count; x++)
            {
                quantityRanges.Add(new List<float>());
                for (int y = itemRanges[x].minAmount; y <= itemRanges[x].maxAmount;y++)
                {
                    quantityRanges[x].Add((float)y);
                }
            }
            
            List<QuantityModifier> quantModifiers = new List<QuantityModifier>();
            for (int rollAmountIter = 0; rollAmountIter < this.returnRolls; rollAmountIter++)
            {
                double totalWeight = itemRanges.Sum(x => x.chance);
                double cumulativeProb = 0;
    
                for (int i = 0; i < itemRanges.Count; i++)
                {
                    double prevCumulativeProb = cumulativeProb;
                    cumulativeProb += itemRanges[i].chance / totalWeight;
        
                    QuantityModifier newModifier = new QuantityModifier();
                    newModifier.Id = $"RS_{this.id}_T_{processables.Key}_{rollAmountIter}_{i}";
                    newModifier.Modification = QuantityModifier.ModificationType.Add;
                    
                    newModifier.Condition = prevCumulativeProb > 0 
                        ? $"SYNCED_RANDOM tick {rollAmountIter}_roll {cumulativeProb:F6}, !SYNCED_RANDOM tick {rollAmountIter}_roll {prevCumulativeProb:F6}"
                        : $"SYNCED_RANDOM tick {rollAmountIter}_roll {cumulativeProb:F6}";
                    newModifier.RandomAmount = quantityRanges[i];
                    quantModifiers.Add(newModifier);

                }
            }

            outItem.StackModifiers = quantModifiers;

            nRule.Triggers = new List<MachineOutputTriggerRule>() { nTrigRule };
            nRule.OutputItem = new List<MachineItemOutput>() { outItem };
            targetMachineData.OutputRules.Add(nRule);
        }

        if (additionalItemRequired != null)
        {
            MachineItemAdditionalConsumedItems newAddItem = new MachineItemAdditionalConsumedItems();
            newAddItem.ItemId = additionalItemRequired;
            newAddItem.InvalidCountMessage = failMessage;
            targetMachineData.AdditionalConsumedItems = new List<MachineItemAdditionalConsumedItems>(){newAddItem};
        }
        
        machineDict.Add($"(BC){this.id}", targetMachineData);
    }
    
    public void AddCraftingRecipe(IDictionary<string, string> craftingDict)
    {
        craftingDict[base.Name] = $"{this.creationString}/1 /{this.id}";
    }
}
public abstract class LoadableText
{
    public string id;
    public List<string> contents;
    public LoadableText()
    {
    }

    public LoadableText(string id, List<string> contents)
    {
        this.id = id;
        this.contents = contents;
    }
    
    public LoadableText(string id, string contents)
    {
        this.id = id;
        this.contents = new(){contents};
    }
}

public class LoadableMail : LoadableText
{
    public LoadableMail(int day, Season season, int reqYear, string contents) : base(
        $"{season.ToString().ToLower()}_{day}_{reqYear}",contents) { }
    public LoadableMail(string mailID, string contents) : base(mailID, contents) { }
}

public abstract class LoadableTV : LoadableText
{
    protected abstract string introText { get; }
    public abstract string channelName { get; }
    
    public int day;
    public Season season;
    public int firstYear;

    public LoadableTV(int channelID, int day, Season season, int firstYear, List<string> contents)
    {
        base.id = $"{channelID}";
        base.contents = CreateContentsWithIntro(contents);
        this.day = day;
        this.season = season;
        this.firstYear = firstYear;
    }

    protected List<string> CreateContentsWithIntro(List<string> contents)
    {
        List<string> result = new() { introText };
        result.AddRange(contents);
        return result;
    }
}

public class Gobcast : LoadableTV
{
    private static readonly string staticIntroText = "Hello! This My Goblin My Goblin and also Grubfoot. Me General Bentnoze. Me joined by General Wartface and also Grubfoot. "+
                                                     "We solve goblin problems. You send us problems. Grubfoot! Get problem for today."; 
    protected override string introText => staticIntroText;
    
    private static readonly string staticChannelName = "My Goblin My Goblin and also Grubfoot"; 
    public override string channelName => staticChannelName;
    
    private static int channelID = 429;
    public Gobcast(int day, Season season, int firstYear, List<string> contents) :
        base(channelID,day, season, firstYear, contents) { }
}
public class PerkData
{
    public int perkID;
    public string perkName;
    public string perkDisplayName;
    public string perkDescription;
    public string perkDescriptionLine2;
    public PerkData(int perkID, string perkName, string perkDisplayName, string perkDescription, string perkDescriptionLine2 = "")
    {
        this.perkID = perkID;
        this.perkName = perkName;
        this.perkDisplayName = perkDisplayName;
        this.perkDescription = perkDescription;
        this.perkDescriptionLine2 = perkDescriptionLine2;
    }

    public bool HasPerk(Farmer farmer)
    {
        return ModAssets.HasPerk(farmer, this.perkID);
    }
}

public class ShopListings
{
    public readonly ShopItemData itemData;
    public readonly int insertIndex;
    public ShopListings(string tradeID, string qualifiedID, int price,int newInsertIndex = 0, int minStack = -1, int maxstack = -1, string condition = "", bool isRecipe = false,
        int toolUpgradeLevel = -1)
    {
        itemData = new ShopItemData();
        itemData.Id = qualifiedID;
        itemData.ItemId = qualifiedID;
        itemData.Price = price;
        itemData.MinStack = minStack;
        itemData.MaxStack = maxstack;
        itemData.Condition = condition == "" ? null : condition;
        itemData.ToolUpgradeLevel = toolUpgradeLevel;
        itemData.IsRecipe = isRecipe;
        insertIndex = newInsertIndex;
    }

    public ShopListings(string tradeID, string qualifiedID,string tradeItemID, int tradeAmount,int newInsertIndex = 0, int minStack = -1, int maxStack = -1, string condition = "")
    {
        itemData = new ShopItemData();
        itemData.Id = tradeID;
        itemData.ItemId = qualifiedID;
        itemData.Price = -1;
        itemData.MinStack = minStack;
        itemData.MaxStack = maxStack;
        itemData.Condition = condition == "" ? null : condition;
        itemData.ToolUpgradeLevel = -1;
        itemData.TradeItemId = tradeItemID;
        itemData.TradeItemAmount = tradeAmount;
        insertIndex = newInsertIndex;
    }
}

public class CustomBuff
{
    private string buffID;
    private string displayName;
    private string description;
    private int duration;
    private int spriteIndex;

    public CustomBuff(string buffId, string displayName, string description, int duration, int spriteIndex)
    {
        this.buffID = buffId;
        this.displayName = displayName;
        this.description = description;
        this.duration = duration;
        this.spriteIndex = spriteIndex;
    }

    public void AppendBuff(IDictionary<string, BuffData> buffDict)
    {
        BuffData buffInfo = new BuffData();
        buffInfo.DisplayName = displayName;
        buffInfo.Description = description;
        buffInfo.Duration = duration;
        buffInfo.IconTexture = "Mods.RunescapeSpellbook.Assets.buffsprites";
        buffInfo.IconSpriteIndex = spriteIndex;
        buffDict.Add(this.buffID,buffInfo);
    }
}

