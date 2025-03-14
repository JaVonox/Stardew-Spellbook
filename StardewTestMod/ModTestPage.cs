using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using static StardewTestMod.ModLoadObjects;

namespace StardewTestMod;

public class ModTestPage : IClickableMenu
{
    
    public List<ClickableComponent> spellIcons = new List<ClickableComponent>();
    
    public int hoverSpellID = -1;

    public ModTestPage(int x, int y, int width, int height)
        : base(x, y, width, height)
    {
        int spellsMaxIndex = ModAssets.modSpells.Length - 1;
        foreach(Spell sp in ModAssets.modSpells)
        {
            spellIcons.Add(
                new ClickableComponent(new Rectangle(xPositionOnScreen + (70 + sp.id * 90),yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 4 - 12,ModAssets.spellsSize,ModAssets.spellsSize), name:sp.name)
                {
                    myID = sp.id,
                    leftNeighborID = sp.id - 1 < 0 ? 0 : sp.id - 1,
                    rightNeighborID = sp.id + 1 > spellsMaxIndex ? -1 : sp.id + 1,
                    fullyImmutable = false
                }
                );
        }
    }

    public override void performHoverAction(int x, int y)
    {
        foreach (ClickableComponent c in spellIcons)
        {
            if (c.containsPoint(x, y))
            {
                hoverSpellID = c.myID;
                break;
            }

            hoverSpellID = -1;
        }
    }

    //Creates the tooltip for the hovered spell
    public const int runesOffset = 15;
    private void GenerateHoverBox(SpriteBatch b, KeyValuePair<bool,string> canCast)
    {
        int x = Game1.getOldMouseX() + 32;
        int y = Game1.getOldMouseY() + 32;
        Spell hoveredSpell = ModAssets.modSpells[hoverSpellID];

        //Find the required size of the tooltip box
        int requiredWidth = (int)Math.Ceiling(
            Math.Max(Game1.smallFont.MeasureString(canCast.Value).X + 16,
                Math.Max(
                    Math.Max(Game1.dialogueFont.MeasureString(hoveredSpell.displayName).X,(float)(hoveredSpell.requiredItems.Count * ((16 * 4) + runesOffset))),
                    Game1.smallFont.MeasureString(hoveredSpell.description).X + 16)
            ));
        requiredWidth = requiredWidth < 100 ? 132 : 32 + requiredWidth;

        int titleHeight = (int)Math.Ceiling(Game1.dialogueFont.MeasureString(hoveredSpell.displayName).Y);
        int descHeight = (int)Math.Ceiling(Game1.smallFont.MeasureString(hoveredSpell.description).Y);
        int errorHeight = (int)Math.Ceiling(Game1.smallFont.MeasureString(canCast.Value).Y);
        
        int requiredHeight = 4 + titleHeight + 4 + descHeight + 36 + (16 * 4) + (!canCast.Key ? errorHeight : 0); //Adjust to add error message;
        requiredHeight = requiredHeight < 50 ? 66 : 16 + requiredHeight;
        
        //Begin drawing
        drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), x, y, requiredWidth, requiredHeight, Color.White, 1f);
        //Title
        int nextYOffset = 16;
        b.DrawString(Game1.dialogueFont, hoveredSpell.displayName, new Vector2(x + 16, y + nextYOffset + 4) + new Vector2(2f, 2f), Game1.textColor);
        nextYOffset += titleHeight;
            b.DrawString(Game1.smallFont, hoveredSpell.description, new Vector2(x + 16, y + nextYOffset + 4) + new Vector2(2f, 2f), Game1.textColor);
        nextYOffset += descHeight;

        if (!canCast.Key)
        {
            b.DrawString(Game1.smallFont, canCast.Value, new Vector2(x + 16, y + nextYOffset + 4) + new Vector2(2f, 2f), Color.Red);
            nextYOffset += errorHeight;
        }
        nextYOffset += 16;
        
        int nextXOffset = 16;
        Texture2D runesTextures = ItemRegistry.GetData($"(O)4290").GetTexture(); //TODO move this somewhere else - it may be loading item textures each frame
        
        foreach (KeyValuePair<int,int> runePair in hoveredSpell.requiredItems) //Key is ID, value is amount
        {
            Vector2 runePosition = new Vector2(x + nextXOffset, y + nextYOffset + 4);
            b.Draw(runesTextures, runePosition, new Rectangle(((runePair.Key - 4290) * 16), 0, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0001f);
            
            Color runeCountColour = hoveredSpell.HasRuneCost(runePair.Key) ? Color.LawnGreen : Color.Red;
            b.DrawString(Game1.dialogueFont, runePair.Value.ToString(), new Vector2(runePosition.X + (16*2) + 10,runePosition.Y + (16*2)), runeCountColour);
            nextXOffset += (16 * 4) + runesOffset;
        }
    }
    public override void draw(SpriteBatch b)
    {
        b.End();
        b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
        foreach (ClickableComponent c in spellIcons)
        {
            bool canCast = ModAssets.modSpells[c.myID].CanCastSpell().Key;
            b.Draw(ModAssets.extraTextures, c.bounds, new Rectangle(canCast ? 0 : ModAssets.spellsSize,ModAssets.spellsY + (c.myID * ModAssets.spellsSize),ModAssets.spellsSize,ModAssets.spellsSize), Color.White);
        }

        //b.DrawString(Game1.dialogueFont, hoverSpellID.ToString(), new Vector2(xPositionOnScreen + IClickableMenu.borderWidth * 3 / 2 + 192 - 20 + 96 - (int)(Game1.dialogueFont.MeasureString(hoverSpellID.ToString()).X), yPositionOnScreen + 500), Game1.textColor);
        //Needs to be at end to prevent overlap
        if (hoverSpellID != -1)
        {
            GenerateHoverBox(b,ModAssets.modSpells[hoverSpellID].CanCastSpell());
        }
        
        b.End();
        b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
    }
    
    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        if (hoverSpellID != -1)
        {
            KeyValuePair<bool,string> castReturn = ModAssets.modSpells[hoverSpellID].CastSpell();
            if (castReturn.Key)
            {
                exitThisMenu();
            }
            else
            {
                Game1.showRedMessage(castReturn.Value, true);
            }
        }
    }
}