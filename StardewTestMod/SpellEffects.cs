using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Extensions;

namespace StardewTestMod;

public class BaseSpellEffects
{
    public static void StartTeleportAnim()
    {
        Game1.player.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[2]
        {
            new FarmerSprite.AnimationFrame(57, 2000, secondaryArm: false, flip: false),
            new FarmerSprite.AnimationFrame((short)Game1.player.FarmerSprite.CurrentFrame, 0, secondaryArm: false, flip: false, null, behaviorAtEndOfFrame: true)
        });
    }

    public static void MidTeleportAnim()
    {
        Game1.player.playNearbySoundAll("wand", null);
        Game1.displayFarmer = false;
        Game1.player.temporarilyInvincible = true;
        Game1.player.temporaryInvincibilityTimer = -2000;
        Game1.player.freezePause = 1000;
        Game1.flashAlpha = 1f;
    }

    public static void EndTeleportAnim()
    {
        Game1.fadeToBlackAlpha = 0.99f;
        Game1.screenGlow = false;
        Game1.player.temporarilyInvincible = false;
        Game1.player.temporaryInvincibilityTimer = 0;
        Game1.displayFarmer = true;
    }

    public static KeyValuePair<bool,string> Teleport(string Location, int x, int y, int dir)
    {

        //Disable teleporting if there is an active festival or event
        if (Utility.isFestivalDay(Game1.dayOfMonth,Game1.season) || Utility.IsPassiveFestivalDay(Game1.dayOfMonth,Game1.season,null))
        {
            return new KeyValuePair<bool, string>(false,"It's dangerous to teleport on special days");
        }
        /*
        Game1.player.faceDirection(2);
        Game1.player.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[2]
        {
            new FarmerSprite.AnimationFrame(57, 2000, secondaryArm: false, flip: false),
            new FarmerSprite.AnimationFrame((short)Game1.player.FarmerSprite.CurrentFrame, 0, secondaryArm: false, flip: false, null, behaviorAtEndOfFrame: true)
        });
        */
        StartTeleportAnim();
        Game1.warpFarmer(Location, x, y, dir);
        /*
        EndTeleportAnim();
        */
        return new KeyValuePair<bool, string>(true,"");;
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