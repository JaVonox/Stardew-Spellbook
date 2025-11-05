using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace RunescapeSpellbook;

public class SpellbookPage : IClickableMenu
{ 
    public List<ClickableComponent> spellIcons = new();
    
    private int hoverSpellID = -1;
    private int magicLevel;
    
    private Texture2D runesTextures;

    private const int spellsPerRow = 6;

    private ClickableTextureComponent magicIcon;
    
    private bool hasMagic = false;
    public SpellbookPage(int x, int y, int width, int height)
        : base(x, y, width, height)
    {
        runesTextures = ItemRegistry.GetData($"(O)Tofu.RunescapeSpellbook_RuneSpellbook").GetTexture();
        
        hasMagic = LevelsHandler.HasMagic(Game1.player);
        if (hasMagic)
        {
            List<Spell> orderedSpells = ModAssets.modSpells.OrderBy(x => x.magicLevelRequirement).ToList();

            int spellsPlaced = 0;
            int spellsCount = orderedSpells.Count;
            foreach (Spell sp in orderedSpells)
            {
                spellIcons.Add(
                    new ClickableComponent(new Rectangle(xPositionOnScreen + 70 + ((spellsPlaced % spellsPerRow) * 90),
                        yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 4 - 12 +
                        ((ModAssets.spellsSize + 20) * (spellsPlaced / spellsPerRow)),
                        ModAssets.spellsSize, ModAssets.spellsSize), name: sp.id.ToString())
                    {
                        myID = spellsPlaced,
                        leftNeighborID = spellsPlaced % spellsPerRow == 0 ? -7777 : (spellsPlaced - 1),
                        rightNeighborID = (spellsPlaced + 1) % spellsPerRow == 0 
                            ? (spellsPlaced / spellsPerRow < 4 ? 429 : -7777) 
                            : (spellsPlaced + 1),
                        upNeighborID =  spellsPlaced <= spellsPerRow ? 12348 : (spellsPlaced - spellsPerRow) % spellsCount,
                        downNeighborID = spellsPlaced + spellsPerRow <= spellsCount - 1 ? spellsPlaced + spellsPerRow : -7777, 
                        fullyImmutable = false,
                        name = sp.id.ToString(),
                    }
                );
                spellsPlaced++;
                
            }

            magicIcon = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 70 + ((spellsPerRow) * 90) + 30,
                    yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 14, 80, 80),
                ModAssets.extraTextures, new Rectangle(160, 105, 80, 80), 1f, true);
            magicIcon.myID = 4290;

            magicLevel = LevelsHandler.GetFarmerMagicLevel(Game1.player);
            
            //TODO spellbooks need checking on controllers now perks are gone
        }
    }
    
    public override void snapToDefaultClickableComponent()
    {
        base.snapToDefaultClickableComponent();
        currentlySnappedComponent = getComponentWithID(0);
        snapCursorToCurrentSnappedComponent();
    }

    public override void performHoverAction(int x, int y)
    {
        base.performHoverAction(x,y);
        if(!hasMagic){return;}
        foreach (ClickableComponent c in spellIcons)
        {
            if (c.containsPoint(x, y))
            {
                hoverSpellID = int.Parse(c.name);
                return;
            }

            hoverSpellID = -1;
        }
    }

    private void RepositionHoverBox(int requiredWidth, int requiredHeight, ref int x, ref int y)
    {
        if (x + requiredWidth > Utility.getSafeArea().Right)
        {
            x = Utility.getSafeArea().Right - requiredWidth;
            y += 16;
        }
        if (y + requiredHeight > Utility.getSafeArea().Bottom)
        {
            x += 16;
            if (x + requiredWidth > Utility.getSafeArea().Right)
            {
                x = Utility.getSafeArea().Right - requiredWidth;
            }
            y = Utility.getSafeArea().Bottom - requiredHeight;
        }
    }

    //Creates the tooltip for the hovered spell
    public const int runesOffset = 15;
    private void GenerateHoverBoxSpell(SpriteBatch b, SpellResponse canCast)
    {
        if(!hasMagic){return;}
        int x = Game1.getOldMouseX() + 32;
        int y = Game1.getOldMouseY() + 32;
        Spell hoveredSpell = ModAssets.modSpells[hoverSpellID];

        string levelReqText = KeyTranslator.GetTranslation("ui.RequiresLevel.text",new {MagicLevel = hoveredSpell.magicLevelRequirement});
        string damageText = "";
        if (hoveredSpell.GetType() == typeof(CombatSpell))
        {
            damageText = KeyTranslator.GetTranslation("ui.SpellDamage.text",new {LowDamage = Math.Floor((float)((CombatSpell)hoveredSpell).damage * 0.8f), HighDamage = Math.Floor((float)((CombatSpell)hoveredSpell).damage * 1.2f)});
        }
        
        //Find the required size of the tooltip box
        int requiredWidth = (int)Math.Ceiling(
            Math.Max((damageText != "" ? Game1.smallFont.MeasureString(damageText).X + 52 + 16 : 0),
            Math.Max(Game1.smallFont.MeasureString(levelReqText).X + 16,
            Math.Max(Game1.smallFont.MeasureString(canCast.translatedResponse).X + 16,
                Math.Max(
                    Math.Max(Game1.dialogueFont.MeasureString(hoveredSpell.displayName).X,(float)(hoveredSpell.requiredItems.Count * ((16 * 4) + runesOffset))),
                    Game1.smallFont.MeasureString(hoveredSpell.description).X + 16)))
            ));
        requiredWidth = requiredWidth < 100 ? 132 : 32 + requiredWidth;

        int titleHeight = (int)Math.Ceiling(Game1.dialogueFont.MeasureString(hoveredSpell.displayName).Y);
        int descHeight = (int)Math.Ceiling(Game1.smallFont.MeasureString(hoveredSpell.description).Y);
        int levelHeight = (int)Math.Ceiling(Game1.smallFont.MeasureString(levelReqText).Y);
        int damageHeight = damageText != "" ? (int)Math.Ceiling(Math.Max(10,Game1.smallFont.MeasureString(damageText).Y)) + 4 : 0;
        int errorHeight = (int)Math.Ceiling(Game1.smallFont.MeasureString(canCast.translatedResponse).Y);
        
        int requiredHeight = 4 + titleHeight + 4 + descHeight + levelHeight + damageHeight + 4 + 36 + (16 * 4) + (!canCast.wasSpellSuccessful ? errorHeight : 0); //Adjust to add error message;
        requiredHeight = requiredHeight < 50 ? 66 : 16 + requiredHeight;

        RepositionHoverBox(requiredWidth, requiredHeight, ref x, ref y);
        
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
            Utility.drawWithShadow(b, ModAssets.extraTextures, new Vector2(x + 16, y + nextYOffset + 4), new Rectangle(16, 0, 10, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
            Utility.drawTextWithShadow(b, damageText, Game1.smallFont, new Vector2(x + 16 + 52, y + nextYOffset + 4) + new Vector2(2f, 2f), Game1.textColor);
            nextYOffset += damageHeight;
        }

        if (!canCast.wasSpellSuccessful)
        {
            b.DrawString(Game1.smallFont, canCast.translatedResponse, new Vector2(x + 16, y + nextYOffset + 4) + new Vector2(2f, 2f), Color.Red);
            nextYOffset += errorHeight;
        }
        
        nextYOffset += 16;
        
        int nextXOffset = 16;
        foreach (KeyValuePair<string,int> runePair in hoveredSpell.requiredItems) //Key is ID, value is amount
        {
            Vector2 runePosition = new Vector2(x + nextXOffset, y + nextYOffset + 4);
            b.Draw(runesTextures, runePosition, new Rectangle(((ModAssets.modItems[runePair.Key].SpriteIndex) * 16), 0, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0001f);
            
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
            SpellResponse castReturn = ModAssets.modSpells[hoverSpellID].SelectSpell(); //This may either cast the spell or run alternate effects like opening a new menu
            if (castReturn.wasSpellSuccessful)
            {
                exitThisMenu();
            }
            else
            {
                Game1.showRedMessage(castReturn.translatedResponse);
            }
        }
    }
    
    public override void draw(SpriteBatch b)
    {
        b.End();
        b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);

        if (!hasMagic)
        {
            string messageLine1 = KeyTranslator.GetTranslation("ui.NotMagic.text-1");
            string messageLine2 = KeyTranslator.GetTranslation("ui.NotMagic.text-2");
            string messageLine3 = KeyTranslator.GetTranslation("ui.NotMagic.text-3");
            
            Vector2 message1Size = Game1.dialogueFont.MeasureString(messageLine1);
            Vector2 message2Size = Game1.smallFont.MeasureString(messageLine2);
            Vector2 message3Size = Game1.smallFont.MeasureString(messageLine3);
            
            int drawableWidth = width - (IClickableMenu.borderWidth * 2);
            int drawableStartX = xPositionOnScreen + IClickableMenu.borderWidth;
            
            b.DrawString(Game1.dialogueFont,messageLine1, new Vector2(drawableStartX + (drawableWidth / 2) - (message1Size.X / 2),yPositionOnScreen + (height / 2) - 64),Game1.textColor);
            b.DrawString(Game1.smallFont,messageLine2, new Vector2(drawableStartX + (drawableWidth / 2) - (message2Size.X / 2),message1Size.Y + 4 + yPositionOnScreen + (height / 2) - 64),Game1.textColor);
            b.DrawString(Game1.smallFont,messageLine3, new Vector2(drawableStartX + (drawableWidth / 2) - (message3Size.X / 2),message1Size.Y + 8 + message2Size.Y + yPositionOnScreen + (height / 2) - 64),Game1.textColor);
            
            b.End();
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
            return;
        }
        
        int selectedSpellID = -1;
        int.TryParse(ModAssets.TryGetModVariable(Game1.player, "Tofu.RunescapeSpellbook_SelectedSpellID"), out selectedSpellID);
        
        foreach (ClickableComponent c in spellIcons)
        {
            int perkSpellID = int.Parse(c.name);
            bool canCast = ModAssets.modSpells[perkSpellID].CanCastSpell().wasSpellSuccessful;

            b.Draw(ModAssets.extraTextures, c.bounds,
                new Rectangle(canCast ? 0 : ModAssets.spellsSize,
                    ModAssets.spellsY + (perkSpellID * ModAssets.spellsSize), ModAssets.spellsSize,
                    ModAssets.spellsSize), Color.White);
            b.Draw(ModAssets.extraTextures, c.bounds,
                new Rectangle(canCast ? 0 : ModAssets.spellsSize,
                    ModAssets.spellsY + (perkSpellID * ModAssets.spellsSize), ModAssets.spellsSize,
                    ModAssets.spellsSize), Color.White);

            if (perkSpellID == selectedSpellID) //If this is the selected spell
            {
                //Draw a box behind the selected spell
                b.Draw(ModAssets.extraTextures, c.bounds,
                    new Rectangle(160, 25, ModAssets.spellsSize, ModAssets.spellsSize), Color.White);
            }
        }

        //Magic Level
        magicIcon.draw(b);
        string levelText = KeyTranslator.GetTranslation("ui.LevelDisplay.text",new {MagicLevel = magicLevel});
        int spacing = (int)(magicIcon.bounds.Width - Game1.dialogueFont.MeasureString(levelText).X) / 2;
        b.DrawString(Game1.dialogueFont, levelText,
            new Vector2(magicIcon.bounds.X + spacing, magicIcon.bounds.Y + 90), Game1.textColor);

        //Needs to be at end to prevent overlap
        if (hoverSpellID != -1)
        {
            GenerateHoverBoxSpell(b, ModAssets.modSpells[hoverSpellID].CanCastSpell());
        }

        b.End();
        b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
    }
}