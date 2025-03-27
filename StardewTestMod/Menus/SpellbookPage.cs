using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using static StardewTestMod.ModLoadObjects;

namespace StardewTestMod;

public class SpellbookPage : IClickableMenu
{
    public List<ClickableComponent> spellIcons = new List<ClickableComponent>();
    
    public int hoverSpellID = -1;
    public int tempMagicLevel;

    private Texture2D runesTextures;

    private const int spellsPerRow = 6;

    private ClickableTextureComponent magicIcon;
    List<ClickableTextureComponent> perkIcons = new List<ClickableTextureComponent>();
    private List<int> perksAssigned = new List<int>();
    public SpellbookPage(int x, int y, int width, int height)
        : base(x, y, width, height)
    {
        runesTextures = ItemRegistry.GetData($"(O)4290").GetTexture();
        List<Spell> orderedSpells = ModAssets.modSpells.OrderBy(x => x.magicLevelRequirement).ToList();
        
        int spellsPlaced = 0;
        foreach(Spell sp in orderedSpells)
        {
            spellIcons.Add(
                new ClickableComponent(new Rectangle(xPositionOnScreen + 70 + ((spellsPlaced % spellsPerRow) * 90),yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 4 - 12 + ((ModAssets.spellsSize + 20) * (spellsPlaced / spellsPerRow) ),
                    ModAssets.spellsSize,ModAssets.spellsSize), name:sp.name)
                {
                    myID = sp.id,
                    fullyImmutable = false
                }
                );
            spellsPlaced++;
        }

        magicIcon = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 70 + ((spellsPerRow) * 90) + 30, 
            yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 14,80,80),
            ModAssets.extraTextures,new Rectangle(160,105,80,80),1f,true);
        
        tempMagicLevel = ModAssets.GetFarmerMagicLevel(Game1.player);
        perksAssigned = ModAssets.PerksAssigned(Game1.player);
        
        int newPerkPoints = perksAssigned.Count - (tempMagicLevel / 5);
        
        int perksPlaced = 0;
        for (int i = 0; i < 4; i++)
        {
            int yValue = perksAssigned.Contains(i) ? 182 + ((i+1) * 160) :
                    (newPerkPoints == 0 ? 182 : 262 + (i * 160));
                
            perkIcons.Add(
                new ClickableTextureComponent(
                    new Rectangle(magicIcon.bounds.X, magicIcon.bounds.Y + 140 + (perksPlaced * 90) ,80,80),
                    ModAssets.extraTextures,
                    new Rectangle(160,yValue,80,80),
                    1f,true)
                {
                    myID = i,
                    fullyImmutable = false
                }
            );
            perksPlaced++;
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

        string levelReqText = $"Requires Lvl. {hoveredSpell.magicLevelRequirement}";
        string damageText = "";
        if (hoveredSpell.GetType() == typeof(CombatSpell))
        {
            damageText = $"{Math.Floor((float)((CombatSpell)hoveredSpell).damage * 0.8f)}-{Math.Floor((float)((CombatSpell)hoveredSpell).damage * 1.2f)} Damage";
        }
        
        //Find the required size of the tooltip box
        int requiredWidth = (int)Math.Ceiling(
            Math.Max((damageText != "" ? Game1.smallFont.MeasureString(damageText).X + 52 + 16 : 0),
            Math.Max(Game1.smallFont.MeasureString(levelReqText).X + 16,
            Math.Max(Game1.smallFont.MeasureString(canCast.Value).X + 16,
                Math.Max(
                    Math.Max(Game1.dialogueFont.MeasureString(hoveredSpell.displayName).X,(float)(hoveredSpell.requiredItems.Count * ((16 * 4) + runesOffset))),
                    Game1.smallFont.MeasureString(hoveredSpell.description).X + 16)))
            ));
        requiredWidth = requiredWidth < 100 ? 132 : 32 + requiredWidth;

        int titleHeight = (int)Math.Ceiling(Game1.dialogueFont.MeasureString(hoveredSpell.displayName).Y);
        int descHeight = (int)Math.Ceiling(Game1.smallFont.MeasureString(hoveredSpell.description).Y);
        int levelHeight = (int)Math.Ceiling(Game1.smallFont.MeasureString(levelReqText).Y);
        int damageHeight = damageText != "" ? (int)Math.Ceiling(Math.Max(10,Game1.smallFont.MeasureString(damageText).Y)) + 4 : 0;
        int errorHeight = (int)Math.Ceiling(Game1.smallFont.MeasureString(canCast.Value).Y);
        
        int requiredHeight = 4 + titleHeight + 4 + descHeight + levelHeight + damageHeight + 4 + 36 + (16 * 4) + (!canCast.Key ? errorHeight : 0); //Adjust to add error message;
        requiredHeight = requiredHeight < 50 ? 66 : 16 + requiredHeight;
        
        //Begin drawing
        drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), x, y, requiredWidth, requiredHeight, Color.White, 1f);
        //Title
        int nextYOffset = 16;
        b.DrawString(Game1.dialogueFont, hoveredSpell.displayName, new Vector2(x + 16, y + nextYOffset + 4) + new Vector2(2f, 2f), Game1.textColor);
        nextYOffset += titleHeight;
        b.DrawString(Game1.smallFont, levelReqText, new Vector2(x + 16, y + nextYOffset + 4) + new Vector2(2f, 2f), Game1.textColor);
        nextYOffset += levelHeight;
        b.DrawString(Game1.smallFont, hoveredSpell.description, new Vector2(x + 16, y + nextYOffset + 4) + new Vector2(2f, 2f), Game1.textColor);
        nextYOffset += descHeight;
        
        if (damageText != "")
        {
            Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(x + 16, y + nextYOffset + 4), new Rectangle(120, 428, 10, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
            Utility.drawTextWithShadow(b, damageText, Game1.smallFont, new Vector2(x + 16 + 52, y + nextYOffset + 4) + new Vector2(2f, 2f), Game1.textColor);
            nextYOffset += damageHeight;
        }

        if (!canCast.Key)
        {
            b.DrawString(Game1.smallFont, canCast.Value, new Vector2(x + 16, y + nextYOffset + 4) + new Vector2(2f, 2f), Color.Red);
            nextYOffset += errorHeight;
        }
        
        nextYOffset += 16;
        
        int nextXOffset = 16;
        foreach (KeyValuePair<int,int> runePair in hoveredSpell.requiredItems) //Key is ID, value is amount
        {
            Vector2 runePosition = new Vector2(x + nextXOffset, y + nextYOffset + 4);
            b.Draw(runesTextures, runePosition, new Rectangle(((runePair.Key - 4290) * 16), 0, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0001f);
            
            Color runeCountColour = hoveredSpell.HasRuneCost(runePair.Key) ? Color.LawnGreen : Color.Red;
            b.DrawString(Game1.dialogueFont, runePair.Value.ToString(), new Vector2(runePosition.X + (16*2) + 10,runePosition.Y + (16*2)), runeCountColour);
            nextXOffset += (16 * 4) + runesOffset;
        }
    }
    
    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        if (hoverSpellID != -1)
        {
            Item? nullItem = null;
            KeyValuePair<bool,string> castReturn = ModAssets.modSpells[hoverSpellID].SelectSpell(); //This may either cast the spell or run alternate effects like opening a new menu
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
    
    public override void draw(SpriteBatch b)
    {
        b.End();
        b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
        
        foreach (ClickableComponent c in spellIcons)
        {
            bool canCast = ModAssets.modSpells[c.myID].CanCastSpell().Key;
            
            b.Draw(ModAssets.extraTextures, c.bounds, new Rectangle(canCast ? 0 : ModAssets.spellsSize,ModAssets.spellsY + (c.myID * ModAssets.spellsSize),ModAssets.spellsSize,ModAssets.spellsSize), Color.White);
            b.Draw(ModAssets.extraTextures, c.bounds, new Rectangle(canCast ? 0 : ModAssets.spellsSize,ModAssets.spellsY + (c.myID * ModAssets.spellsSize),ModAssets.spellsSize,ModAssets.spellsSize), Color.White);

            if (c.myID == ModAssets.localFarmerData.selectedSpellID) //If this is the selected spell
            {
                //Draw a box behind the selected spell
                b.Draw(ModAssets.extraTextures, c.bounds, new Rectangle(160,25,ModAssets.spellsSize,ModAssets.spellsSize), Color.White);
            }
        }
        
        //Magic Level
        magicIcon.draw(b);
        string levelText = $"Level {tempMagicLevel}";
        int spacing = (int)(magicIcon.bounds.Width - Game1.dialogueFont.MeasureString(levelText).X) / 2;
        b.DrawString(Game1.dialogueFont,$"Level {tempMagicLevel}",new Vector2(magicIcon.bounds.X + spacing,magicIcon.bounds.Y + 90),Game1.textColor);

        foreach (ClickableTextureComponent perk in perkIcons)
        {
            perk.draw(b);
        }

        //Needs to be at end to prevent overlap
        if (hoverSpellID != -1)
        {
            GenerateHoverBox(b,ModAssets.modSpells[hoverSpellID].CanCastSpell());
        }
        
        b.End();
        b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
    }
}