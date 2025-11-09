using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Monsters;

namespace RunescapeSpellbook;

public class EssenceFloat : BigSlime
{
    private float heldObjectBobTimer;
    private TemporaryAnimatedSpriteList sparkles = new TemporaryAnimatedSpriteList();
    private Color associatedColour;
    public EssenceFloat(Item essenceItem, Vector2 position, Color associatedColour) : base(position,0)
    {
        this.heldItem.Value = essenceItem;
        base.c.A = 0;
        base.Scale = 0;
        base.Health = 1;
        this.associatedColour = associatedColour;
        startGlowing(associatedColour,false,0.01f);
        
        GenerateSparkle(2);
    }

    public override bool isInvincible() => true;

    protected override void updateAnimation(GameTime time)
    {
        this.heldObjectBobTimer += (float)time.ElapsedGameTime.TotalMilliseconds * 0.007853982f;

        int removedAmount = sparkles.RemoveWhere((TemporaryAnimatedSprite sprite) => sprite.update(time));
        if (removedAmount > 0)
        {
            GenerateSparkle(removedAmount);
        }
    }

    private void GenerateSparkle(int amount)
    {
        Rectangle box = this.GetBoundingBox();
        int standingY = base.StandingPixel.Y;

        Vector2 centerPos = new Vector2((base.Tile.X * 64f), (base.Tile.Y * 64f));
        for (int i = 0; i < amount; i++)
        {
            sparkles.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(346, 392, 8, 8),
                new Vector2(centerPos.X + Game1.random.Next(-30,30), centerPos.Y + Game1.random.Next(-30,30)),
                flipped: false, (float)(Game1.random.Next(1,6))/100.0f,
                this.associatedColour)
            {
                motion = new Vector2(0, -1),
                interval = 99999f,
                layerDepth = standingY / 10000f + (Game1.random.NextBool() ? -0.001f : 0.002f),
                scale = 2f,
                scaleChange = 0.01f,
                rotation = Game1.random.Next(360),
                local = false,
                delayBeforeAnimationStart = 0
            });
        }
    }

    public override void draw(SpriteBatch b)
    {
        int standingY = base.StandingPixel.Y;
        this.heldItem.Value?.drawInMenu(b, base.getLocalPosition(Game1.viewport) + new Vector2(28f, -16f + (float)Math.Sin(this.heldObjectBobTimer + 1f) * 4f), 1f, 1f, (float)(standingY - 1) / 10000f, StackDrawType.Hide, Color.White, drawShadow: true);
        this.heldItem.Value?.drawInMenu(b, base.getLocalPosition(Game1.viewport) + new Vector2(28f, -16f + (float)Math.Sin(this.heldObjectBobTimer + 1f) * 4f), 1f, 1f, (float)(standingY - 1) / 10000f + 0.001f, StackDrawType.Hide, glowingColor * glowingTransparency, drawShadow: false);
        
        foreach(TemporaryAnimatedSprite spark in sparkles)
        {
            spark.draw(b, localPosition: true);
        }
    }
    
    public override void updateMovement(GameLocation location, GameTime time)
    {
    }

    
    //TODO being perfectly between two things and walking down still blocks - seems like there's some weird discrepancy here
    
    public override int getTimeFarmerMustPushBeforePassingThrough() => 0;

    //When we check overlap, try to add item - dying if it is successful. either way return false.
    public override bool OverlapsFarmerForDamage(Farmer who)
    {
        if (this.GetBoundingBox().Intersects(who.GetBoundingBox()) && Health > 0 && who.addItemToInventoryBool(this.heldItem.Value))
        {
            sparkles.Clear();
            //TODO maybe add pickup animation? radial debris doesn't work cuz it expects to grab multiple icons in a row
            Health = 0;
        }
        return false;
    }

    //TODO check interactions with specials like parry + explosions + fire/earth ammo
    
    //TODO check audio
}