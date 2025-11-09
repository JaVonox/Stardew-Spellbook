using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Monsters;

namespace RunescapeSpellbook;

public class EssenceFloat : BigSlime
{
    
    private float heldObjectBobTimer;
    public EssenceFloat(Item essenceItem, Vector2 position) : base(position,0)
    {
        this.heldItem.Value = essenceItem;
        base.c.A = 0;
        base.Scale = 0;
        base.Health = 1;
    }

    public override bool isInvincible() => true;

    protected override void updateAnimation(GameTime time)
    {
        this.heldObjectBobTimer += (float)time.ElapsedGameTime.TotalMilliseconds * 0.007853982f;
    }

    public override void draw(SpriteBatch b)
    {
        int standingY = base.StandingPixel.Y;
        this.heldItem.Value?.drawInMenu(b, base.getLocalPosition(Game1.viewport) + new Vector2(28f, -16f + (float)Math.Sin(this.heldObjectBobTimer + 1f) * 4f), 1f, 1f, (float)(standingY - 1) / 10000f, StackDrawType.Hide, Color.White, drawShadow: false);
    }
    
    public override void updateMovement(GameLocation location, GameTime time)
    {
    }

    public override int getTimeFarmerMustPushBeforePassingThrough() => 0;

    //When we check overlap, try to add item - dying if it is successful. either way return false.
    public override bool OverlapsFarmerForDamage(Farmer who)
    {
        if (this.GetBoundingBox().Intersects(who.GetBoundingBox()) && Health > 0 && who.addItemToInventoryBool(this.heldItem.Value))
        {
            Health = 0;
        }
        return false;
    }
    
    protected override void sharedDeathAnimation() //Add particles for pickup
    {
        Game1.showRedMessage("ABC");
        //this.shedChunks(Game1.random.Next(4, 9), 0.75f);
    }

    //TODO check interactions with specials like parry + explosions
    
    //TODO check audio
}