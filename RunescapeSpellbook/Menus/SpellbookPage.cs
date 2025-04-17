using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using static RunescapeSpellbook.ModLoadObjects;

namespace RunescapeSpellbook;

public class SpellbookPage : IClickableMenu
{
    private List<ClickableComponent> spellIcons = new List<ClickableComponent>();
    
    private int hoverSpellID = -1;
    private int hoverPerkID = -1;
    private int magicLevel;
    private bool hasPerkPoints;
    
    private Texture2D runesTextures;

    private const int spellsPerRow = 6;

    private ClickableTextureComponent magicIcon;
    List<ClickableTextureComponent> perkIcons = new List<ClickableTextureComponent>();
    private List<int> perksAssigned = new List<int>();
    
    private bool hasMagic = false;
    public SpellbookPage(int x, int y, int width, int height)
        : base(x, y, width, height)
    {
        runesTextures = ItemRegistry.GetData($"(O)4290").GetTexture();
        
        hasMagic = ModAssets.HasMagic(Game1.player);
        if (hasMagic)
        {
            List<Spell> orderedSpells = ModAssets.modSpells.OrderBy(x => x.magicLevelRequirement).ToList();

            int spellsPlaced = 0;
            foreach (Spell sp in orderedSpells)
            {
                spellIcons.Add(
                    new ClickableComponent(new Rectangle(xPositionOnScreen + 70 + ((spellsPlaced % spellsPerRow) * 90),
                        yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 4 - 12 +
                        ((ModAssets.spellsSize + 20) * (spellsPlaced / spellsPerRow)),
                        ModAssets.spellsSize, ModAssets.spellsSize), name: sp.name)
                    {
                        myID = sp.id,
                        fullyImmutable = false
                    }
                );
                spellsPlaced++;
            }

            magicIcon = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 70 + ((spellsPerRow) * 90) + 30,
                    yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 14, 80, 80),
                ModAssets.extraTextures, new Rectangle(160, 105, 80, 80), 1f, true);

            magicLevel = ModAssets.GetFarmerMagicLevel(Game1.player);

            RefreshPerkData();
        }
    }

    public void RefreshPerkData()
    {
        perkIcons.Clear();
        perksAssigned = ModAssets.PerksAssigned(Game1.player);

        int newPerkPoints = (magicLevel / 5) - perksAssigned.Count;

        hasPerkPoints = newPerkPoints > 0;
        int perksPlaced = 0;

        for (int i = 0; i < 4; i++)
        {
            int yValue = perksAssigned.Contains(i)
                ? 182 + ((i + 1) * 160)
                : (newPerkPoints == 0 ? 182 : 262 + (i * 160));

            perkIcons.Add(
                new ClickableTextureComponent(
                    new Rectangle(magicIcon.bounds.X, magicIcon.bounds.Y + 140 + (perksPlaced * 90), 80, 80),
                    ModAssets.extraTextures,
                    new Rectangle(160, yValue, 80, 80),
                    1f, true)
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
        base.performHoverAction(x,y);
        if(!hasMagic){return;}
        foreach (ClickableComponent c in spellIcons)
        {
            if (c.containsPoint(x, y))
            {
                hoverSpellID = c.myID;
                hoverPerkID = -1;
                return;
            }

            hoverSpellID = -1;
        }

        foreach (ClickableTextureComponent perk in perkIcons)
        {
            if (perk.containsPoint(x, y))
            {
                hoverPerkID = perk.myID;
                return;
            }
            hoverPerkID = -1;
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
    private void GenerateHoverBoxSpell(SpriteBatch b, KeyValuePair<bool,string> canCast)
    {
        if(!hasMagic){return;}
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

    private void GenerateHoverBoxPerk(SpriteBatch b)
    {
        if(!hasMagic){return;}
        int x = Game1.getOldMouseX() + 32;
        int y = Game1.getOldMouseY() + 32;
        PerkData hoveredPerk = ModAssets.perks[hoverPerkID];
        bool isHoveredPerkAssigned = ModAssets.HasPerk(Game1.player, hoveredPerk.perkID);

        string hoveredPerkText = isHoveredPerkAssigned
            ? "I have this perk"
            : (!hasPerkPoints ? "I don't have any perk points to spend" : "Click to buy this perk");

        bool perkHasLine2 = hoveredPerk.perkDescriptionLine2 != "";
        Vector2 perkTitleSizes = Game1.dialogueFont.MeasureString(hoveredPerk.perkDisplayName);
        Vector2 perkDescription1Sizes = Game1.smallFont.MeasureString(hoveredPerk.perkDescription);
        Vector2 perkDescription2Sizes = perkHasLine2 ? Game1.smallFont.MeasureString(hoveredPerk.perkDescriptionLine2) : new Vector2(0,0);
        Vector2 perkInteractionSizes = Game1.smallFont.MeasureString(hoveredPerkText);
        
        int requiredWidth = (int)Math.Floor(Math.Max(perkTitleSizes.X,Math.Max(perkDescription1Sizes.X,
            Math.Max(perkInteractionSizes.X,perkDescription2Sizes.X))) + 16);
        requiredWidth = requiredWidth < 100 ? 132 : 32 + requiredWidth;
        
        int requiredHeight = (int)Math.Ceiling(4 + perkTitleSizes.Y + 4 + perkDescription1Sizes.Y + 4 + perkInteractionSizes.Y + 4 + perkDescription2Sizes.Y + 4 + 16);
        requiredHeight = requiredHeight < 50 ? 66 : 16 + requiredHeight;
        
        RepositionHoverBox(requiredWidth, requiredHeight, ref x, ref y);
        
        drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), x, y, requiredWidth, requiredHeight, Color.White, 1f);
        //Title
        
        int nextYOffset = 16;
        b.DrawString(Game1.dialogueFont, hoveredPerk.perkDisplayName, new Vector2(x + 16, y + nextYOffset + 4) + new Vector2(2f, 2f), Game1.textColor);
        nextYOffset += (int)Math.Floor(perkTitleSizes.Y);
        b.DrawString(Game1.smallFont, hoveredPerk.perkDescription, new Vector2(x + 16, y + nextYOffset + 4) + new Vector2(2f, 2f), Game1.textColor);
        nextYOffset += (int)Math.Floor(perkDescription1Sizes.Y);
        if (perkHasLine2)
        {
            b.DrawString(Game1.smallFont, hoveredPerk.perkDescriptionLine2, new Vector2(x + 16, y + nextYOffset + 4) + new Vector2(2f, 2f), Game1.textColor);
            nextYOffset += (int)Math.Floor(perkDescription2Sizes.Y);
        }
        
        b.DrawString(Game1.smallFont, hoveredPerkText, new Vector2(x + 16, y + nextYOffset + 4) + new Vector2(2f, 2f), 
            (isHoveredPerkAssigned || hasPerkPoints ? Color.DarkGreen : Color.DarkRed));
        
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
        else if (hoverPerkID != -1)
        {
            if (!hasPerkPoints)
            {
                //TODO add different message if you already have the perk
                Game1.showRedMessage("I don't have enough perk points to do this", true);
                return;
            }
            
            bool couldAssignPerk = ModAssets.GrantPerk(Game1.player,ModAssets.perks[hoverPerkID].perkID);

            if (couldAssignPerk)
            {
                RefreshPerkData();
            }
            else
            {
                Game1.showRedMessage("Couldn't assign perk", true);
            }
        }
    }
    
    public override void draw(SpriteBatch b)
    {
        b.End();
        b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);

        if (!hasMagic)
        {
            const string messageLine1 = "I don't know any runic spells yet";
            const string messageLine2 = "I might be able to learn some if I reach level 5 friendship with someone with experience in magic";
            
            //TODO format this correctly
            b.DrawString(Game1.dialogueFont,messageLine1, new Vector2((width / 2),height / 2),Game1.textColor);
            b.DrawString(Game1.smallFont,messageLine2, new Vector2((width / 2),64 + (height / 2)),Game1.textColor);
            b.End();
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
            return;
        }
        foreach (ClickableComponent c in spellIcons)
        {
            bool canCast = ModAssets.modSpells[c.myID].CanCastSpell().Key;

            b.Draw(ModAssets.extraTextures, c.bounds,
                new Rectangle(canCast ? 0 : ModAssets.spellsSize,
                    ModAssets.spellsY + (c.myID * ModAssets.spellsSize), ModAssets.spellsSize,
                    ModAssets.spellsSize), Color.White);
            b.Draw(ModAssets.extraTextures, c.bounds,
                new Rectangle(canCast ? 0 : ModAssets.spellsSize,
                    ModAssets.spellsY + (c.myID * ModAssets.spellsSize), ModAssets.spellsSize,
                    ModAssets.spellsSize), Color.White);

            if (c.myID == ModAssets.localFarmerData.selectedSpellID) //If this is the selected spell
            {
                //Draw a box behind the selected spell
                b.Draw(ModAssets.extraTextures, c.bounds,
                    new Rectangle(160, 25, ModAssets.spellsSize, ModAssets.spellsSize), Color.White);
            }
        }

        //Magic Level
        magicIcon.draw(b);
        string levelText = $"Level {magicLevel}";
        int spacing = (int)(magicIcon.bounds.Width - Game1.dialogueFont.MeasureString(levelText).X) / 2;
        b.DrawString(Game1.dialogueFont, $"Level {magicLevel}",
            new Vector2(magicIcon.bounds.X + spacing, magicIcon.bounds.Y + 90), Game1.textColor);

        foreach (ClickableTextureComponent perk in perkIcons)
        {
            perk.draw(b);
        }

        //Needs to be at end to prevent overlap
        if (hoverSpellID != -1)
        {
            GenerateHoverBoxSpell(b, ModAssets.modSpells[hoverSpellID].CanCastSpell());
        }
        else if (hoverPerkID != -1)
        {
            GenerateHoverBoxPerk(b);
        }

        b.End();
        b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
    }
}