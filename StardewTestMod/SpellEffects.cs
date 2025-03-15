using System.Linq.Expressions;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;

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
    public static void PlayAnimationTeleport(Action onAnimationComplete)
    {
        Game1.player.playNearbySoundAll("wand", null);
    
        // Callback function for when we finish the animation
        FarmerSprite.endOfAnimationBehavior endBehavior = farmer => 
        {
            onAnimationComplete?.Invoke();
        };
        
        PlayCastAnim(endBehavior,2000);
    }

    public static void PlayAnimationHumidify(Action onAnimationComplete)
    {
        Game1.player.playNearbySoundAll("wand", null);
    
        // Callback function for when we finish the animation
        FarmerSprite.endOfAnimationBehavior endBehavior = farmer => 
        {
            onAnimationComplete?.Invoke();
        };
        
        PlayCastAnim(endBehavior,1000);
    }
    public static KeyValuePair<bool,string> Teleport(string Location, int x, int y, int dir)
    {

        //Disable teleporting if there is an active festival or event
        if (Utility.isFestivalDay(Game1.dayOfMonth,Game1.season) || Utility.IsPassiveFestivalDay(Game1.dayOfMonth,Game1.season,null))
        {
            return new KeyValuePair<bool, string>(false,"It's dangerous to teleport on special days");
        }

        //Warp once the animation ends
        PlayAnimationTeleport(() =>
        {
            Game1.warpFarmer(Location, x, y, dir);
            Game1.player.temporarilyInvincible = false;
            Game1.player.temporaryInvincibilityTimer = 0;
            Game1.player.freezePause = 0;
        });
        
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
}

public class SpellEffects : BaseSpellEffects
{
    public static KeyValuePair<bool,string> TeleportToFarm()
    {
        return Teleport("BusStop", 19, 23,2);
    }
    
    public static KeyValuePair<bool,string> TeleportToPierre()
    {
        return Teleport("Town", 43, 60,0);
    }
    
    public static KeyValuePair<bool,string> WaterTiles()
    {
        Farmer player = Game1.player;
        GameLocation currentLoc = player.currentLocation;
        List<HoeDirt> tilesToCastOn = GetTiles(Game1.player.Tile, currentLoc, 10,
            (tile => tile is HoeDirt hoeLand && 
                     (hoeLand.crop == null || !hoeLand.crop.forageCrop.Value || hoeLand.crop.whichForageCrop.Value != "2") && hoeLand.state.Value != 1))
            .OfType<HoeDirt>().ToList();
        
        
        if (tilesToCastOn.Count == 0)
        {
            return new KeyValuePair<bool, string>(false,$"There isn't anything to water here");
        }
        else
        {
            PlayAnimationHumidify(() =>
            {
                int j = 0;
                player.playNearbySoundAll("slosh", null);
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
            });
            
            return new KeyValuePair<bool, string>(true,"");
        }
    }
    
}