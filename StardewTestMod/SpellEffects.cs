﻿using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData.Machines;
using StardewValley.Inventories;
using StardewValley.Monsters;
using StardewValley.TerrainFeatures;

namespace StardewTestMod;

public class BaseSpellEffects
{
    public static void PlayCastAnim(FarmerSprite.endOfAnimationBehavior endBehavior, int duration)
    {
        Game1.player.faceDirection(2);
        Game1.player.temporarilyInvincible = true;
        Game1.player.temporaryInvincibilityTimer = -duration;
        Game1.player.freezePause = duration;
        Game1.displayFarmer = true;
        
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
    public static void PlayAnimation(Action onAnimationComplete, string sound, int duration)
    {
        Game1.player.playNearbySoundAll(sound, null);
    
        // Callback function for when we finish the animation
        FarmerSprite.endOfAnimationBehavior endBehavior = farmer => 
        {
            onAnimationComplete?.Invoke();
        };
        
        PlayCastAnim(endBehavior,duration);
    }
    
}

public class PlankMakeConversions
{
    public string inItemID;
    public string outItemID;
}
/// <summary>
/// The class full of functions for individual spells to run when cast
/// </summary>
public class SpellEffects : BaseSpellEffects
{
    public static KeyValuePair<bool, string> Humidify(List<TerrainFeature> tiles)
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
        }, "wand", 1000);

        return new KeyValuePair<bool, string>(true, "");

    }

    public static KeyValuePair<bool, string> CurePlant(List<TerrainFeature> tiles)
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
        }, "wand", 1000);

        return new KeyValuePair<bool, string>(true, "");
    }

    public static KeyValuePair<bool, string> HighAlchemy(ref Item? itemArgs)
    {
        int postCastStackSize = itemArgs.Stack - 1;
        Game1.player.Money += itemArgs.salePrice(false);
        Game1.player.playNearbySoundAll("purchaseRepeat", null);
        itemArgs.ConsumeStack(1);
        if (postCastStackSize == 0)
        {
            itemArgs = null;
        }

        return new KeyValuePair<bool, string>(true, "");
    }
    
    /// <summary>
    /// Conversion on the plankmake spell from wooden items to hardwood. Doesn't include wood itself since that has its own conversion
    /// </summary>
    public static readonly Dictionary<string,int> plankMakeConversions =
    new Dictionary<string, int>(){
        {"(O)709",15},{"(O)169",10},{"(O)298",15},{"(O)322",2},{"(O)328",1},{"(O)405",1},{"(O)734",15},{"(O)325",10},{"(BC)37",25}
    };
    public static KeyValuePair<bool, string> PlankMake(ref Item? itemArgs)
    {
        int postCastStackSize;
        
        
        //TODO convert this to use recipes rather than hardcoded dictionary
        
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
        else //Any other recipes
        {
            postCastStackSize = itemArgs.Stack - 1;
            StardewValley.Object returnItem = ItemRegistry.Create<StardewValley.Object>($"388");
            returnItem.Stack = plankMakeConversions[itemArgs.QualifiedItemId];
            Utility.CollectOrDrop(returnItem);
            itemArgs.ConsumeStack(1);
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

    public static KeyValuePair<bool, string> VileVigour()
    {
        Farmer caster = Game1.player;

        PlayAnimation(() =>
        {
            caster.health -= caster.maxHealth / 3;
            caster.stamina = caster.MaxStamina;
        }, "yoba", 500);

        return new KeyValuePair<bool, string>(true, "");
    }

    public static KeyValuePair<bool, string> BakePie()
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

        }, "wand", 500);

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