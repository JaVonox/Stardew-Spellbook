using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Menus;
using StardewValley.Mods;

namespace RunescapeSpellbook;

public class DrawHandler
{
    
    public static void PostHudStepHandler(object? sender, RenderedStepEventArgs e)
    {
        switch (e.Step)
        {
            case(RenderSteps.HUD):
                DrawOverheal(e);
                break;
        }

    }
    
    private static readonly int healthIconSize = 128;
    private static readonly Color healthTextColour = new(50, 221, 31);
    private static void DrawOverheal(RenderedStepEventArgs e)
    {
        if (Game1.showingHealthBar && Game1.player.buffs.IsApplied("Tofu.RunescapeSpellbook_Overheal") && Game1.player.buffs.AppliedBuffs["Tofu.RunescapeSpellbook_Overheal"].customFields.TryGetValue("Tofu.RunescapeSpellbook_OverhealAmount", out string healthVal))
        {
            var viewport = Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea();
            float iconX = viewport.Right - 144 - healthIconSize;
            float iconY = viewport.Bottom - 58 - Game1.player.maxHealth / 2f - healthIconSize / 2f;
                    
            e.SpriteBatch.Draw(
                ModAssets.extraTextures,
                new Rectangle((int)iconX, (int)iconY, healthIconSize, healthIconSize),
                new Rectangle(160,902,80,80),
                Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f
            );

            Vector2 textSize = Game1.dialogueFont.MeasureString(healthVal);
            Vector2 textPos = new Vector2(
                iconX + healthIconSize / 2f - textSize.X / 2f,
                iconY + healthIconSize / 2f - textSize.Y / 2f
            );
            e.SpriteBatch.DrawString(Game1.dialogueFont, healthVal, textPos, healthTextColour);

        }
    }
    
}

