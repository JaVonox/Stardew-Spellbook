using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Monsters;
using StardewValley.Projectiles;
using StardewValley.TokenizableStrings;

namespace StardewTestMod;

public class MagicProjectile : BasicProjectile
{
    private NetInt projectileSpriteID = new NetInt();
    private NetFloat projectileRotation = new NetFloat();
    private NetColor projectileColor = new NetColor();

    public MagicProjectile()
    {
        
    }
    public MagicProjectile(int damageToFarmer, int spriteIndex, int bouncesTillDestruct, int tailLength, float rotationVelocity, float xVelocity, float yVelocity, Vector2 startingPosition, float projectileRotation, Color projectileColor, string collisionSound = null, string bounceSound = null, string firingSound = null, bool explode = false, bool damagesMonsters = false, GameLocation location = null, Character firer = null, onCollisionBehavior collisionBehavior = null, string shotItemId = null)
        : base(damageToFarmer, spriteIndex, bouncesTillDestruct, tailLength, rotationVelocity, xVelocity, yVelocity, startingPosition, collisionSound, bounceSound , firingSound, explode , damagesMonsters , location , firer , collisionBehavior, shotItemId )
    {
        this.projectileSpriteID.Value = spriteIndex;
        this.projectileRotation.Value = projectileRotation;
        this.projectileColor.Value = projectileColor;
    }

    protected override void InitNetFields()
    {
        base.InitNetFields();
        base.NetFields.AddField(projectileSpriteID,"spriteID")
            .AddField(projectileRotation,"projectileRotation")
            .AddField(projectileColor,"projectileColor");
    }

    public override void behaviorOnCollisionWithMonster(NPC n, GameLocation location)
    {
        if (!damagesMonsters.Value)
        {
            return;
        }
        Farmer player = GetPlayerWhoFiredMe(location);
        explosionAnimation(location);
        if (n is Monster)
        {
            location.damageMonster(n.GetBoundingBox(), damageToFarmer.Value, damageToFarmer.Value + 1, isBomb: false, player, isProjectile: true);
            if (!(n as Monster).IsInvisible)
            {
                piercesLeft.Value--;
            }
        }
        else
        {
            n.getHitByPlayer(player, location);
            string projectileTokenizedName = TokenStringBuilder.ItemName(itemId.Value);
            //TODO This is bugged
            ModAssets.GlobalChatMessage("Slingshot_Hit", player.Name, n.GetTokenizedDisplayName(), Lexicon.prependTokenizedArticle(projectileTokenizedName));
            piercesLeft.Value--;
        }
    }
    
    public override void draw(SpriteBatch b)
    {
        float current_scale = 4f * localScale;
        Rectangle sourceRect = new Rectangle(projectileSpriteID.Value * 16, 0,16, 16);
        
        float newRotation = rotationVelocity.Value == 0 ? projectileRotation.Value : rotation;
        
        Vector2 pixelPosition = position.Value;
        b.Draw(ModAssets.animTextures, Game1.GlobalToLocal(Game1.viewport, pixelPosition + new Vector2(0f, 0f - height.Value) + new Vector2(32f, 32f)), sourceRect, projectileColor.Value * alpha.Value, newRotation, new Vector2(8f, 8f), current_scale, SpriteEffects.None, (pixelPosition.Y + 96f) / 10000f);
        if (height.Value > 0f)
        {
            b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, pixelPosition + new Vector2(32f, 32f)), Game1.shadowTexture.Bounds, Color.White * alpha.Value * 0.75f, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 2f, SpriteEffects.None, (pixelPosition.Y - 1f) / 10000f);
        }
        float tailAlpha = alpha.Value;
        for (int i = tail.Count - 1; i >= 0; i--)
        {
            b.Draw(ModAssets.animTextures, Game1.GlobalToLocal(Game1.viewport, Vector2.Lerp((i == tail.Count - 1) ? pixelPosition : tail.ElementAt(i + 1), tail.ElementAt(i), (float)tailCounter / 50f) + new Vector2(0f, 0f - height.Value) + new Vector2(32f, 32f)), sourceRect, color.Value * tailAlpha, newRotation, new Vector2(8f, 8f), current_scale, SpriteEffects.None, (pixelPosition.Y - (float)(tail.Count - i) + 96f) / 10000f);
            tailAlpha -= 1f / (float)tail.Count;
            current_scale = 0.8f * (float)(4 - 4 / (i + 4));
        }
    }
}