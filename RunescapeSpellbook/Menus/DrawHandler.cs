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
            case(RenderSteps.Menu):
                AttemptDrawSpellbookTab(e);
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

    private static void AttemptDrawSpellbookTab(RenderedStepEventArgs e)
    {
        if(Game1.activeClickableMenu is GameMenu gameMenu && ModEntry.Instance.Config.SpellbookTabStyle != "Only Keybind")
        {
            IClickableMenu curTab = gameMenu.pages[gameMenu.currentTab];
            //this is used to ensure that we dont overlap any big menus
            if (Game1.activeClickableMenu.GetChildMenu() == null && (curTab as CollectionsPage)?.letterviewerSubMenu == null && !(curTab is MapPage))
            {
                ClickableComponent c = gameMenu.tabs.First(x=>x.name == "RSspellbook");

                if (c.visible)
                {
                    e.SpriteBatch.Draw(ModAssets.extraTextures,
                        new Vector2(c.bounds.X,
                            c.bounds.Y +
                            ((gameMenu.currentTab == gameMenu.getTabNumberFromName(c.name)) ? 8 : 0)),
                        new Rectangle(0, 0, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None,
                        0.0001f);
                }
                if (!gameMenu.hoverText.Equals(""))
                {
                    IClickableMenu.drawHoverText(e.SpriteBatch, gameMenu.hoverText, Game1.smallFont);
                }
                gameMenu.drawMouse(e.SpriteBatch, ignore_transparency: true);
            }
        }
    }
    
}

