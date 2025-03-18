using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData.Machines;
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

    public static KeyValuePair<bool,string> Teleport(string Location, int x, int y, int dir)
    {

        //Disable teleporting if there is an active festival or event
        if (Utility.isFestivalDay(Game1.dayOfMonth,Game1.season) || Utility.IsPassiveFestivalDay(Game1.dayOfMonth,Game1.season,null))
        {
            return new KeyValuePair<bool, string>(false,"It's dangerous to teleport on special days");
        }

        //Warp once the animation ends
        PlayAnimation(() =>
        {
            Game1.warpFarmer(Location, x, y, dir);
            Game1.player.temporarilyInvincible = false;
            Game1.player.temporaryInvincibilityTimer = 0;
            Game1.player.freezePause = 0;
        },"wand",2000);
        
        return new KeyValuePair<bool, string>(true,"");
    }
    
    //This method takes in a predicate that allows us to set custom conditions for which terrain feature we return
    public static List<TerrainFeature> GetTiles(Vector2 playerTile, GameLocation currentLoc, int size, Predicate<TerrainFeature> predicate)
    {
        //Gets all tiles in a size area around the player and then outputs the set to have effects applied to
        List<TerrainFeature> tileLocations = new List<TerrainFeature>();

        for (int y = -(size/2); y < (size/2); y++)
        {
            for (int x = -(size/2); x < (size/2); x++)
            {
                Vector2 accessedLocation = playerTile + new Vector2(x, y);
                if (currentLoc.terrainFeatures.TryGetValue(accessedLocation, out var terrainFeature) && predicate(terrainFeature))
                {
                    tileLocations.Add(terrainFeature);
                }
            }
        }

        return tileLocations;
    }

    public static KeyValuePair<bool, string> IsItemValidForOperation(ref Item? itemArgs, Predicate<Item?> predicate)
    {
        if (itemArgs == null || !predicate(itemArgs))
        {
            return new KeyValuePair<bool, string>(false,$"Invalid Item");
        }
        
        return new KeyValuePair<bool, string>(true,$"");

    }
}

public class SpellEffects : BaseSpellEffects
{
    public static KeyValuePair<bool,string> TeleportToFarm(ref Item? itemArgs, Predicate<object>? castPredicate = null)
    {
        return Teleport("BusStop", 19, 23,2);
    }
    
    public static KeyValuePair<bool,string> TeleportToPierre(ref Item? itemArgs, Predicate<object>? castPredicate = null)
    {
        return Teleport("Town", 43, 60,0);
    }
    
    public static KeyValuePair<bool,string> Humidify(ref Item? itemArgs, Predicate<object>? castPredicate = null)
    {
        Farmer player = Game1.player;
        GameLocation currentLoc = player.currentLocation;
        List<HoeDirt> tilesToCastOn = GetTiles(Game1.player.Tile, currentLoc, 10,castPredicate).OfType<HoeDirt>().ToList();
        
        
        if (tilesToCastOn.Count == 0)
        {
            return new KeyValuePair<bool, string>(false,$"There isn't anything to water here");
        }
        else
        {
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
                    ModAssets.BroadcastSprite(currentLoc, new TemporaryAnimatedSprite(13, new Vector2(appTile.Tile.X * 64f, appTile.Tile.Y * 64f), Color.White, 10, Game1.random.NextBool(), 70f, 0, 64, (appTile.Tile.Y * 64f + 32f) / 10000f - 0.01f)
                    {
                        delayBeforeAnimationStart = 200 + j * 10
                    });
                    j++;
                }
                Game1.player.temporarilyInvincible = false;
                Game1.player.temporaryInvincibilityTimer = 0;
                Game1.player.freezePause = 0;
            },"wand",1000);
            
            return new KeyValuePair<bool, string>(true,"");
        }
    }
    
    public static KeyValuePair<bool,string> CurePlant(ref Item? itemArgs, Predicate<object>? castPredicate = null)
    {
        Farmer player = Game1.player;
        GameLocation currentLoc = player.currentLocation;
        List<HoeDirt> tilesToCastOn = GetTiles(Game1.player.Tile, currentLoc, 10, castPredicate).OfType<HoeDirt>().ToList();
        
        
        if (tilesToCastOn.Count == 0)
        {
            return new KeyValuePair<bool, string>(false,$"I can't see any dead plants here");
        }
        else
        {
            PlayAnimation(() =>
            {
                foreach (HoeDirt appTile in tilesToCastOn)
                {

                    //Get a random seed from the current seasonal set and place it here
                    string randomSeed = Crop.ResolveSeedId("770",currentLoc); //This gets a randoms seed from the mixed seeds set
                    appTile.destroyCrop(true);
                    appTile.plant(randomSeed,player,false);

                }
                Game1.player.temporarilyInvincible = false;
                Game1.player.temporaryInvincibilityTimer = 0;
                Game1.player.freezePause = 0;
            },"wand",1000);
            
            return new KeyValuePair<bool, string>(true,"");
        }
    }
    public static KeyValuePair<bool, string> HighAlchemy(ref Item? itemArgs, Predicate<object>? castPredicate = null)
    {
        KeyValuePair<bool, string> operationReturn = IsItemValidForOperation(ref itemArgs,castPredicate);

        if (operationReturn.Key)
        {
            int postCastStackSize = itemArgs.Stack - 1;
            Game1.player.Money += itemArgs.salePrice(false);
            Game1.player.playNearbySoundAll("purchaseRepeat", null);
            itemArgs.ConsumeStack(1);
            if (postCastStackSize == 0)
            {
                itemArgs = null;
            }
        }
        else
        {
            return new KeyValuePair<bool, string>(false,"Couldn't convert item to money");
        }
        
        return new KeyValuePair<bool, string>(true,"");
    }
    public static KeyValuePair<bool, string> SuperheatItem(ref Item? itemArgs, Predicate<object>? castPredicate = null)
    {
        KeyValuePair<bool, string> operationReturn = IsItemValidForOperation(ref itemArgs,castPredicate);

        if (!operationReturn.Key)
        {
            return new KeyValuePair<bool, string>(false,"This item can't be smelted");
        }

        string itemId = itemArgs.QualifiedItemId;
        MachineOutputRule? furnaceRule = DataLoader.Machines(Game1.content).GetValueOrDefault("(BC)13").OutputRules
            .FirstOrDefault(r => r.Triggers.Any(x => x.RequiredItemId == itemId));

        if (furnaceRule == null)
        {
            return new KeyValuePair<bool, string>(false,"This item can't be smelted");
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
            return new KeyValuePair<bool, string>(false,$"I need atleast {stackSizeRequired} {itemArgs.DisplayName}");
        }
        
        return new KeyValuePair<bool, string>(true,"");
    }
}