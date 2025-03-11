using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace StardewTestMod;

public class ModTestPage : IClickableMenu
{
    public ModTestPage(int x, int y, int width, int height)
        : base(x, y, width, height)
    {
    }
    
    public override void populateClickableComponentList()
    {
        base.populateClickableComponentList();
    }
    
    private const string text = "lmao looser";

    public override void draw(SpriteBatch b)
    {
        b.End();
        b.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, Utility.ScissorEnabled, null, null);
        
        b.DrawString(Game1.dialogueFont, text, new Vector2(xPositionOnScreen + IClickableMenu.borderWidth * 3 / 2 + 192 - 20 + 96 - (int)(Game1.dialogueFont.MeasureString(text).X), yPositionOnScreen + 200), Game1.textColor);

        b.End();
        b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
    }
}