using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData.Machines;
using StardewValley.Inventories;
using StardewValley.Monsters;
using StardewValley.TerrainFeatures;

namespace RunescapeSpellbook;

public class BaseSpellEffects
{
    public static void PlayCastAnim(FarmerSprite.endOfAnimationBehavior endBehavior, int duration, int spellAnimOffset)
    {
        Game1.player.faceDirection(2);
        Game1.player.temporarilyInvincible = true;
        Game1.player.temporaryInvincibilityTimer = -duration;
        Game1.player.freezePause = duration;
        Game1.displayFarmer = true;
        
        TemporaryAnimatedSprite castAnim = new TemporaryAnimatedSprite("Mods.RunescapeSpellbook.Assets.spellanimations",
            new Rectangle(0, (spellAnimOffset * 34) + 16, 16, 34), ((float)duration / 4),
            ModAssets.animFrames, 0,
            new Vector2(Game1.player.position.X, Game1.player.position.Y-80), false, false)
        {
            scale=4f,
            layerDepth = 1.1f
        };
        
        ModAssets.BroadcastSprite(Game1.player.currentLocation,castAnim);
        
        
        Game1.player.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[2]
        {
            new FarmerSprite.AnimationFrame(57, duration, secondaryArm: false, flip: false),
            //
            new FarmerSprite.AnimationFrame(
                (short)Game1.player.FarmerSprite.CurrentFrame, 
                0, 
                false, 
                false, 
                endBehavior, 
                true
            )
        });
    }
    public static void PlayAnimation(Action onAnimationComplete, string sound, int duration, int spellAnimOffset)
    {
        Game1.player.playNearbySoundAll(sound, null);
    
        // Callback function for when we finish the animation
        FarmerSprite.endOfAnimationBehavior endBehavior = farmer => 
        {
            onAnimationComplete?.Invoke();
        };
        
        PlayCastAnim(endBehavior,duration, spellAnimOffset);
    }
    
}

/// <summary>
/// The class full of functions for individual spells to run when cast
/// </summary>
public class SpellEffects : BaseSpellEffects
{
    public static KeyValuePair<bool, string> Humidify(List<TerrainFeature> tiles, int animOffset)
    {
        Farmer player = Game1.player;
        GameLocation currentLoc = player.currentLocation;
        List<HoeDirt> tilesToCastOn = tiles.OfType<HoeDirt>().ToList();

        PlayAnimation(() =>
        {
            int j = 0;
            foreach (HoeDirt appTile in tilesToCastOn)
            {
                if (j % 5 == 0)
                {
                    player.playNearbySoundAll("slosh", null);
                }

                appTile.state.Value = 1;
                ModAssets.BroadcastSprite(currentLoc, new TemporaryAnimatedSprite(13,
                    new Vector2(appTile.Tile.X * 64f, appTile.Tile.Y * 64f), Color.White, 10, Game1.random.NextBool(),
                    70f, 0, 64, (appTile.Tile.Y * 64f + 32f) / 10000f - 0.01f)
                {
                    delayBeforeAnimationStart = 200 + j * 10
                });
                j++;
            }
        }, "RunescapeSpellbook.Humidify", 800,animOffset);

        return new KeyValuePair<bool, string>(true, "");

    }

    public static KeyValuePair<bool, string> CurePlant(List<TerrainFeature> tiles, int animOffset)
    {
        Farmer player = Game1.player;
        GameLocation currentLoc = player.currentLocation;
        List<HoeDirt> tilesToCastOn = tiles.OfType<HoeDirt>().ToList();

        PlayAnimation(() =>
        {

            foreach (HoeDirt appTile in tilesToCastOn)
            {
                //Get a random seed from the current seasonal set and place it here
                string randomSeed =
                    Crop.ResolveSeedId("770", currentLoc); //This gets a randoms seed from the mixed seeds set
                appTile.destroyCrop(true);
                appTile.plant(randomSeed, player, false);
            }
        }, "RunescapeSpellbook.Cure", 800,animOffset);

        return new KeyValuePair<bool, string>(true, "");
    }
    public static KeyValuePair<bool, string> HighAlchemy(ref Item? itemArgs)
    {
        int postCastStackSize = itemArgs.Stack - 1;
        Game1.player.Money += itemArgs.sellToStorePrice(-1L);
        Game1.player.playNearbySoundAll("purchaseRepeat", null);
        itemArgs.ConsumeStack(1);
        if (postCastStackSize == 0)
        {
            itemArgs = null;
        }

        return new KeyValuePair<bool, string>(true, "");
    }
    
    public static Dictionary<string,int> blueGemsEnchants = new Dictionary<string,int>()
    {
        {"62",10}, //Aquamarine
        {"72",30}, //Diamond
        {"74",50}, //Prismatic Shard
        {"550",15}, //Kyanite
        {"541",15}, //Aerinite
        {"564",15}, //Opal
        {"561",15}, //Ghost Crystal
        {"536",5}, //Frozen Geode
        {"749",10}, //Omni Geode
    };
    public static KeyValuePair<bool, string> EnchantSapphireBolt(ref Item? itemArgs)
    {
        if (blueGemsEnchants.ContainsKey(itemArgs.ItemId))
        {
            int postCastStackSize = itemArgs.Stack - 1;
            StardewValley.Object returnItem = ItemRegistry.Create<StardewValley.Object>($"4301");
            returnItem.Stack = blueGemsEnchants[itemArgs.ItemId];
            Utility.CollectOrDrop(returnItem);

            itemArgs.ConsumeStack(1);
            if (postCastStackSize == 0)
            {
                itemArgs = null;
            }
        }
        else
        {
            return new KeyValuePair<bool, string>(false, "I can't convert this into ammo!");
        }

        return new KeyValuePair<bool, string>(true, "");
    }
    
    public static Dictionary<string,int> greenGemsEnchants = new Dictionary<string,int>()
    {
        {"60",10}, //Emerald
        {"70",20}, //Jade
        {"74",30}, //Prismatic Shard
        {"548",10}, //Jamborite
        {"552",10}, //Malachite
        {"557",10}, //Petrified Slime
        {"560",10}, //Ocean Stone
        {"909",5}, //Radioactive Ore
        {"910",25}, //Radioactive Bar
        {"749",5}, //Omni Geode
    };
    public static KeyValuePair<bool, string> EnchantEmeraldBolt(ref Item? itemArgs)
    {
        if (greenGemsEnchants.ContainsKey(itemArgs.ItemId))
        {
            int postCastStackSize = itemArgs.Stack - 1;
            StardewValley.Object returnItem = ItemRegistry.Create<StardewValley.Object>($"4302");
            returnItem.Stack = greenGemsEnchants[itemArgs.ItemId];
            Utility.CollectOrDrop(returnItem);

            itemArgs.ConsumeStack(1);
            if (postCastStackSize == 0)
            {
                itemArgs = null;
            }
        }
        else
        {
            return new KeyValuePair<bool, string>(false, "I can't convert this into ammo!");
        }

        return new KeyValuePair<bool, string>(true, "");
    }
    public static KeyValuePair<bool, string> PlankMake(ref Item? itemArgs)
    {
        int postCastStackSize;
        
        if (itemArgs.ItemId == "388") //Wood to hardwood
        {
            if (itemArgs.Stack < 15)
            {
                return new KeyValuePair<bool, string>(false, "I need atleast 15 wood to make hardwood");
            }
            postCastStackSize = itemArgs.Stack - 15;
            
            StardewValley.Object returnItem = ItemRegistry.Create<StardewValley.Object>($"709");
            Utility.CollectOrDrop(returnItem);
            itemArgs.ConsumeStack(15);
        }
        else if (itemArgs.ItemId == "709") //hardwood to wood
        {
            postCastStackSize = itemArgs.Stack - 1;
            
            StardewValley.Object returnItem = ItemRegistry.Create<StardewValley.Object>($"388");
            returnItem.Stack = 15;
            Utility.CollectOrDrop(returnItem);
            itemArgs.ConsumeStack(1);
        }
        else //Any other recipes
        {
            try
            {
                postCastStackSize = itemArgs.Stack - 1;
                List<string> delimCraftRecipe = CraftingRecipe.craftingRecipes[itemArgs.Name].Split(' ').ToList(); //Splits recipe into fields. Item ID is always followed by amount
                delimCraftRecipe[delimCraftRecipe.Count-1] = delimCraftRecipe[delimCraftRecipe.Count-1].Split('/')[0]; //We need to split the final item again because it will have a / in it to delimit other information
                string[] woodIDs = {"709","388"};
                bool generatedItem = false;
                foreach (string woodID in woodIDs) //Find the instances of wood + hardwood in the crafting recipe. the next number will be 
                {
                    int itemIndex = delimCraftRecipe.IndexOf(woodID);
                    if (itemIndex == -1 || (itemIndex+1) % 2 == 0) //If the item was not found in the recipe or it was an amount value
                    {
                        continue;
                    }

                    int amount = -1;
                    if (!int.TryParse(delimCraftRecipe[itemIndex + 1], out amount))
                    {
                        continue;
                    }
    
                    StardewValley.Object returnItem = ItemRegistry.Create<StardewValley.Object>($"{woodID}");
                    returnItem.Stack = amount;
                    Utility.CollectOrDrop(returnItem);
                    generatedItem = true;
                }

                if (generatedItem)
                {
                    itemArgs.ConsumeStack(1);
                }
            }
            catch (Exception e)
            {
                return new KeyValuePair<bool, string>(false, $"An unexpected error occured {e.Message}");
            }
        }

        if (postCastStackSize == 0)
        {
            itemArgs = null;
        }
        
        return new KeyValuePair<bool, string>(true, "");
    }


    public static KeyValuePair<bool, string> SuperheatItem(ref Item? itemArgs)
    {
        string itemId = itemArgs.QualifiedItemId;
        MachineOutputRule? furnaceRule = DataLoader.Machines(Game1.content).GetValueOrDefault("(BC)13").OutputRules
            .FirstOrDefault(r => r.Triggers.Any(x => x.RequiredItemId == itemId));

        if (furnaceRule == null)
        {
            return new KeyValuePair<bool, string>(false, "This item can't be smelted");
        }

        int stackSizeRequired = furnaceRule.Triggers.First(x => x.RequiredItemId == itemId).RequiredCount;
        string itemReturn = furnaceRule.OutputItem[0].ItemId;

        if (itemArgs.Stack >= stackSizeRequired) //If we have enough of the item
        {
            int postCastStackSize = itemArgs.Stack - stackSizeRequired;

            StardewValley.Object furnaceItem = ItemRegistry.Create<StardewValley.Object>($"{itemReturn}");

            Utility.CollectOrDrop(furnaceItem);
            Game1.player.playNearbySoundAll("furnace", null);
            itemArgs.ConsumeStack(stackSizeRequired);

            if (postCastStackSize == 0)
            {
                itemArgs = null;
            }
        }
        else
        {
            return new KeyValuePair<bool, string>(false, $"I need atleast {stackSizeRequired} {itemArgs.DisplayName}");
        }

        return new KeyValuePair<bool, string>(true, "");
    }

    public static KeyValuePair<bool, string> VileVigour(int animOffset)
    {
        Farmer caster = Game1.player;

        PlayAnimation(() =>
        {
            caster.health -= caster.maxHealth / 3;
            caster.stamina = caster.MaxStamina;
        }, "RunescapeSpellbook.Vile", 500,animOffset);

        return new KeyValuePair<bool, string>(true, "");
    }

    public static KeyValuePair<bool, string> BakePie(int animOffset)
    {
        CraftingRecipe? selectedRecipe = Game1.player.cookingRecipes.Keys
            .Select(x => new CraftingRecipe(x, true))
            .Where(x => x.doesFarmerHaveIngredientsInInventory()).OrderBy(x => Game1.random.Next()).FirstOrDefault();

        if (selectedRecipe == null)
        {
            return new KeyValuePair<bool, string>(false, "I can't cook anything with these ingredients");
        }

        PlayAnimation(() =>
        {
            Item crafted = selectedRecipe.createItem();
            selectedRecipe.consumeIngredients(new List<IInventory>() { Game1.player.Items });
            Utility.CollectOrDrop(crafted);

        }, "RunescapeSpellbook.BakePie", 800,animOffset);

        return new KeyValuePair<bool, string>(true, "");
    }

    public static KeyValuePair<bool, string> Charge(int animOffset)
    {
        PlayAnimation(() =>
        {
            Game1.player.applyBuff("429");

        }, "RunescapeSpellbook.Charge", 800,animOffset);
        
        return new KeyValuePair<bool, string>(true, "");
    }
    
    public static KeyValuePair<bool, string> DarkLure(int animOffset)
    {
        PlayAnimation(() =>
        {
            Game1.player.applyBuff("430");

        }, "RunescapeSpellbook.DarkLure", 800,animOffset);
        
        return new KeyValuePair<bool, string>(true, "");
    }

    private static readonly string[] undeadMonsters = {
        "Ghost",
        "Carbon Ghost",
        "Skeleton",
        "Skeleton Mage",
        "Skeleton Warrior",
        "Mummy",
        "Putrid Ghost"
    };

    public static void DealUndeadDamage(Farmer caster, NPC target, ref int damage, ref bool isBomb)
    {
        string targetMonster = ((Monster)target).Name;

        if (undeadMonsters.Contains(targetMonster))
        {
            damage = (int)Math.Floor(damage * 1.5f);

            if (targetMonster == "Mummy")
            {
                isBomb = true;
            }
        }
    }

    public static void DealDemonbaneDamage(Farmer caster, NPC target, ref int damage, ref bool isBomb)
    {
        string targetMonster = ((Monster)target).Name;

        if (undeadMonsters.Contains(targetMonster))
        {
            damage *= 2;
            
            if (targetMonster == "Mummy")
            {
                isBomb = true;
            }
        }
    }
    public static void DealVampiricDamage(Farmer caster, NPC target, ref int damage, ref bool isBomb)
    {
        if (caster.health != caster.maxHealth)
        {
            int healAmount = (int)Math.Ceiling(Math.Min((float)damage * 0.1f, 20));
            healAmount = Math.Min(caster.maxHealth - caster.health, healAmount);
            caster.currentLocation.debris.Add(new Debris(healAmount, caster.getStandingPosition(), Color.Lime, 1f, caster));
            Game1.playSound("healSound", null);
            caster.health += healAmount;
        }
    }
}