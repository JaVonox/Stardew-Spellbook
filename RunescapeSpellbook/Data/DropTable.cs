namespace RunescapeSpellbook;

public class DropTable
{
        public static readonly Dictionary<string, List<ItemDrop>> monsterDrops = new()
    {
        //Caves (Basic)
        { "Big Slime", new(){ 
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureElemental",1,0.02f),
        } },
        { "Green Slime", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureElemental",1,0.02f),
        } },
        { "Rock Crab", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureElemental",1,0.3f),
        } },
        { "Bug", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureElemental",1,0.2f),
        } },
        { "Stone Golem", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureCatalytic",1,0.1f),
            new ItemDrop("Tofu.RunescapeSpellbook_EasyCasket",1,0.04f),
        } },
        { "Ghost", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureCatalytic",2,0.35f),
        } },
        { "Frost Jelly", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureElemental",1,0.2f),
        } },
        { "Skeleton", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureCatalytic",1,0.05f),
            new ItemDrop("Tofu.RunescapeSpellbook_EasyCasket",1,0.08f),
        } },
        { "Lava Crab", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureElemental",2,0.3f),
        } },
        { "Shadow Shaman", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureCatalytic",2,0.2f),
            new ItemDrop("Tofu.RunescapeSpellbook_EasyCasket",1,0.1f),
        } },
        { "Metal Head", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_EasyCasket",1,0.1f),
        } },
        { "Shadow Brute", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureCatalytic",1,0.1f),
        } },
        { "Squid Kid", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureElemental",2,0.2f),
        } }, //Skull Cavern 
        { "Sludge", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureElemental",1,0.2f),
        } },
        { "Serpent", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_EasyCasket",1,0.05f),
        } },
        { "Carbon Ghost", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureCatalytic",2,0.35f),
        } },
        { "Iridium Crab", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureElemental",6,0.6f),
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureCatalytic",3,0.4f),
        } },
        { "Mummy", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_HardCasket",1,0.07f),
        } },
        { "Iridium Bat", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_HardCasket",1,0.15f),
        } },
        { "Haunted Skull", new(){ //Quarry Mine
            new ItemDrop("Tofu.RunescapeSpellbook_EasyCasket",1,0.05f),
            new ItemDrop("Tofu.RunescapeSpellbook_HardCasket",1,0.02f),
        } },
        { "Hot Head", new(){ //Ginger Island/Volcano
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureCatalytic",2,0.2f),
        } },
        { "Dwarvish Sentry", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_BarrowsCasket",1,0.07f),
        } },
        { "Magma Duggy", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_TreasureElemental",5,0.3f),
            new ItemDrop("Tofu.RunescapeSpellbook_BarrowsCasket",1,0.07f),
        } },
    };
    
    
    public static readonly Dictionary<string, List<ItemDrop>> monsterEssence = new()
    {
        //Caves (Basic)
        { "Big Slime", new(){ 
            new ItemDrop("Tofu.RunescapeSpellbook_EssLaw",2,0.08f),
            new ItemDrop("Tofu.RunescapeSpellbook_EssNature",2,0.1f),
        } },
        { "Prismatic Slime", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_EssLaw",4,0.9f),
            new ItemDrop("Tofu.RunescapeSpellbook_EssAstral",5,0.9f),
        } },
        { "Green Slime", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_EssLaw",2,0.08f),
        } },
        { "Fly", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_EssAir",1,0.2f),
            new ItemDrop("Tofu.RunescapeSpellbook_EssChaos",1,0.05f),
        } },
        { "Grub", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_EssNature",2,0.1f),
        } },
        { "Bug", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_EssChaos",1,0.08f),
        } },
        { "Bat", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_EssAir",1,0.15f),
            new ItemDrop("Tofu.RunescapeSpellbook_EssChaos",1,0.25f),
        } },
        { "Stone Golem", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_EssEarth",1,0.2f),
        } },
        { "Dust Spirit", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_EssAir",1,0.04f),
            new ItemDrop("Tofu.RunescapeSpellbook_EssNature",1,0.02f),
        } },
        { "Frost Bat", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_EssAir",1,0.05f),
            new ItemDrop("Tofu.RunescapeSpellbook_EssWater",1,0.15f),
            new ItemDrop("Tofu.RunescapeSpellbook_EssChaos",1,0.3f),
        } },
        { "Frost Jelly", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_EssWater",1,0.1f),
        } },
        { "Skeleton", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_EssChaos",1,0.3f),
            new ItemDrop("Tofu.RunescapeSpellbook_EssDeath",1,0.02f),
        } },
        { "Lava Bat", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_EssAir",2,0.15f),
            new ItemDrop("Tofu.RunescapeSpellbook_EssFire",2,0.15f),
        } },
        { "Lava Crab", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_EssWater",1,0.15f),
            new ItemDrop("Tofu.RunescapeSpellbook_EssFire",1,0.3f),
        } },
        { "Shadow Shaman", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_EssNature",3,0.2f),
            new ItemDrop("Tofu.RunescapeSpellbook_EssAstral",2,0.2f),
        } },
        { "Metal Head", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_EssEarth",2,0.3f),
        } },
        { "Shadow Brute", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_EssAir",2,0.1f),
            new ItemDrop("Tofu.RunescapeSpellbook_EssChaos",2,0.3f),
        } },
        { "Squid Kid", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_EssAir",3,0.2f),
            new ItemDrop("Tofu.RunescapeSpellbook_EssCosmic",2,0.2f),
        } }, //Skull Cavern 
        { "Sludge", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_EssLaw",2,0.2f),
        } },
        { "Serpent", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_EssAir",2,0.25f),
            new ItemDrop("Tofu.RunescapeSpellbook_EssChaos",2,0.1f),
        } },
        { "Carbon Ghost", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_EssDeath",2,0.1f)
        } },
        { "Iridium Crab", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_EssWater",5,0.6f),
        } },
        { "Pepper Rex", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_EssFire",3,1f),
            new ItemDrop("Tofu.RunescapeSpellbook_EssNature",3,0.5f),
        } },
        { "Mummy", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_EssEarth",2,0.2f),
            new ItemDrop("Tofu.RunescapeSpellbook_EssChaos",3,0.3f),
            new ItemDrop("Tofu.RunescapeSpellbook_EssDeath",1,0.2f),
        } },
        { "Iridium Bat", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_EssAir",3,0.5f),
            new ItemDrop("Tofu.RunescapeSpellbook_EssDeath",2,0.2f),
        } },
        { "Haunted Skull", new(){ //Quarry Mine
            new ItemDrop("Tofu.RunescapeSpellbook_EssCosmic",3,0.4f),
            new ItemDrop("Tofu.RunescapeSpellbook_EssAstral",3,0.3f),
        } },
        { "Hot Head", new(){ //Ginger Island/Volcano
            new ItemDrop("Tofu.RunescapeSpellbook_EssFire",2,0.3f),
            new ItemDrop("Tofu.RunescapeSpellbook_EssDeath",3,0.2f),
        } },
        { "Tiger Slime", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_EssWater",2,0.1f),
            new ItemDrop("Tofu.RunescapeSpellbook_EssEarth",1,0.2f),
            new ItemDrop("Tofu.RunescapeSpellbook_EssNature",5,0.3f),
            new ItemDrop("Tofu.RunescapeSpellbook_EssChaos",3,0.3f),
            new ItemDrop("Tofu.RunescapeSpellbook_EssDeath",2,0.1f),
        } },
        { "Magma Sprite", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_EssAir",3,0.2f),
            new ItemDrop("Tofu.RunescapeSpellbook_EssFire",3,0.3f),
        } },
        { "Dwarvish Sentry", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_EssLaw",4,0.24f),
            new ItemDrop("Tofu.RunescapeSpellbook_EssCosmic",10,0.2f),
            new ItemDrop("Tofu.RunescapeSpellbook_EssDeath",3,0.3f),
        } },
        { "Magma Duggy", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_EssFire",2,0.3f),
        } },
        { "Magma Sparker", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_EssFire",3,0.3f),
        } },
        { "False Magma Cap", new(){
            new ItemDrop("Tofu.RunescapeSpellbook_EssEarth",1,0.2f),
            new ItemDrop("Tofu.RunescapeSpellbook_EssCosmic",10,0.2f),
        } },
    };
}