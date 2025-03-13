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

    public override void draw(SpriteBatch b)
    {
        b.End();
        b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
        foreach (ClickableComponent c in spellIcons)
        {
            b.Draw(ModAssets.extraTextures, c.bounds, new Rectangle(0,ModAssets.spellsY + (c.myID * ModAssets.spellsSize),ModAssets.spellsSize,ModAssets.spellsSize), Color.White);
        }

        b.DrawString(Game1.dialogueFont, hoverSpellID.ToString(), new Vector2(xPositionOnScreen + IClickableMenu.borderWidth * 3 / 2 + 192 - 20 + 96 - (int)(Game1.dialogueFont.MeasureString(hoverSpellID.ToString()).X), yPositionOnScreen + 500), Game1.textColor);
        b.End();
        b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
    }
}