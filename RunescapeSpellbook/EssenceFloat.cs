using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Monsters;

namespace RunescapeSpellbook;

public class EssenceSparkle
{
    private Vector2 offset;
    private float alpha = 1f;
    private float alphaFade;
    private int rotation;
    private Color associatedColour;
    private bool aboveBehind;

    public EssenceSparkle(Color associatedColour)
    {
        this.offset = new Vector2(Game1.random.Next(0,30),Game1.random.Next(0,30));
        this.alphaFade = 0.5f + (float)(Game1.random.NextDouble() * 2);
        this.rotation = Game1.random.Next(360);
        this.aboveBehind = Game1.random.NextBool();
        this.associatedColour = associatedColour;
    }

    public bool Update(GameTime time)
    {
        float elapsedTime = (float)time.ElapsedGameTime.TotalSeconds;
        this.alpha -= elapsedTime * this.alphaFade;
        this.offset.Y -= elapsedTime * 32.0f;

        return this.alpha <= 0;
    }

    public void Draw(SpriteBatch b, Vector2 globalPosition, int standingY)
    {
        b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, globalPosition + offset), new Rectangle(346, 392, 8, 8), new Color(associatedColour,this.alpha), this.rotation, Vector2.Zero, 2f, SpriteEffects.None, standingY / 10000f + (this.aboveBehind ? -0.001f : 0.002f));
    }
}
public class EssenceFloat : BigSlime
{
    private float heldObjectBobTimer;
    private List<EssenceSparkle> sparkles = new List<EssenceSparkle>();
    private Color associatedColour;
    public EssenceFloat(Item essenceItem, Vector2 position, Color associatedColour) : base(position,0)
    {
        this.heldItem.Value = essenceItem;
        base.c.A = 0;
        base.Scale = 0;
        base.Health = 1;
        this.associatedColour = associatedColour;
        startGlowing(associatedColour,false,0.01f);
        
        GenerateSparkle(3);
    }

    public override bool isInvincible() => true;

    protected override void updateAnimation(GameTime time)
    {
        this.heldObjectBobTimer += (float)time.ElapsedGameTime.TotalMilliseconds * 0.007853982f;

        int removedAmount = sparkles.RemoveWhere((EssenceSparkle sparkle) => sparkle.Update(time));
        if (removedAmount > 0)
        {
            GenerateSparkle(removedAmount);
        }
    }
    
    public override Rectangle GetBoundingBox()
    {
        Vector2 pos = this.Position;
        return new Rectangle((int)pos.X + 38, (int)pos.Y + 8, 46, 46);
    }

    private void GenerateSparkle(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            sparkles.Add(new EssenceSparkle(associatedColour));
        }
    }

    public override void draw(SpriteBatch b)
    {
        int standingY = base.StandingPixel.Y;
        Rectangle box = this.GetBoundingBox();
        //b.Draw(Game1.fadeToBlackRect,  Game1.GlobalToLocal(Game1.viewport,box), Color.Purple);
        this.heldItem.Value?.drawInMenu(b, base.getLocalPosition(Game1.viewport) + new Vector2(28f, -16f + (float)Math.Sin(this.heldObjectBobTimer + 1f) * 4f), 1f, 1f, (float)(standingY - 1) / 10000f, StackDrawType.Hide, Color.White, drawShadow: true);
        this.heldItem.Value?.drawInMenu(b, base.getLocalPosition(Game1.viewport) + new Vector2(28f, -16f + (float)Math.Sin(this.heldObjectBobTimer + 1f) * 4f), 1f, 1f, (float)(standingY - 1) / 10000f + 0.001f, StackDrawType.Hide, glowingColor * glowingTransparency, drawShadow: false);
        
        foreach(EssenceSparkle spark in sparkles)
        {
            spark.Draw(b,new Vector2(box.X,box.Y),standingY);
        }
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
            sparkles.Clear();
            //TODO maybe add pickup animation? radial debris doesn't work cuz it expects to grab multiple icons in a row
            Health = 0;
        }
        return false;
    }

    //TODO check interactions with specials like parry + explosions + fire/earth ammo
    
    //TODO check audio
}