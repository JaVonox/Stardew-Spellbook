using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Extensions;

namespace StardewTestMod;

public class BaseSpellEffects
{
    public static void PlayAnimationTeleport(Action onAnimationComplete)
    {
        Game1.player.playNearbySoundAll("wand", null);
    
        // Callback function for when we finish the animation
        FarmerSprite.endOfAnimationBehavior endBehavior = farmer => 
        {
            onAnimationComplete?.Invoke();
        };
        
        Game1.player.faceDirection(2);
        Game1.player.temporarilyInvincible = true;
        Game1.player.temporaryInvincibilityTimer = -2000;
        Game1.player.freezePause = 2000;
        Game1.displayFarmer = true;
        
        Game1.player.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[2]
        {
            new FarmerSprite.AnimationFrame(57, 2000, secondaryArm: false, flip: false),
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
    
}