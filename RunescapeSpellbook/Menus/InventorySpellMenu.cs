using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Menus;

namespace RunescapeSpellbook;
public class InventorySpellMenu : MenuWithInventory
{
    private ClickableTextureComponent inputSpot;
    private ClickableTextureComponent spellIcon;
    
    private Texture2D runesTextures;
    private Farmer caster;
    private InventorySpell targetSpell;
    public Predicate<object>? selectablePredicate;
    
    private int centreY;
    private int casterX;
    
    private int currentFrame;
    private bool isAnimatingCast;
    
    private TemporaryAnimatedSpriteList fluffSprites = new TemporaryAnimatedSpriteList();
    private TemporaryAnimatedSprite castAnim;

    private string spellDescriptionText;
    public InventorySpellMenu(InventorySpell targetSpell, Predicate<object>? selectablePredicate) : base(null, okButton: true, trashCan: true, 12, 132)
    {
        runesTextures = ItemRegistry.GetData($"(O)4290").GetTexture();
        this.targetSpell = targetSpell;
        this.selectablePredicate = selectablePredicate;
        currentFrame = Game1.random.Next(16);
        if (yPositionOnScreen == IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder)
        {
            movePosition(0, -IClickableMenu.spaceToClearTopBorder);
        }

        centreY = yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 128;
        casterX = xPositionOnScreen + 288;
        
        inventory.highlightMethod = highlightTargets;
        
        inputSpot = new ClickableTextureComponent(new Rectangle((casterX - 128), 
            yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + ((308/2)-36),96,96), ModAssets.extraTextures, new Rectangle(160, 0, 24, 24), 4f);
        
        caster = Game1.player;

        spellIcon = new ClickableTextureComponent(new Rectangle(casterX + 80,
                (centreY - 15) - (ModAssets.spellsSize / 2),
                ModAssets.spellsSize, ModAssets.spellsSize),ModAssets.extraTextures, new Rectangle(0,ModAssets.spellsY + (targetSpell.id * ModAssets.spellsSize),ModAssets.spellsSize,ModAssets.spellsSize), 1f);
        
        caster.faceDirection(2); //Point caster down

        isAnimatingCast = false;
        
        spellDescriptionText = targetSpell.longDescription;

    }

    public bool highlightTargets(Item i)
    {
        return this.selectablePredicate(i);
    }

    private bool cannotCast;

    private void reevaluateCannotCast()
    {
        cannotCast = inputSpot.item == null || !targetSpell.CanCastSpell().Key;
    }

    private const int MAX_FLUFF_SPRITES = 50;
    public override void update(GameTime time)
    {
        currentFrame = currentFrame % 1000 == 0 ? 1 : currentFrame + 1;

        bool frameNeedsEvaluation = currentFrame % 30 == 1;
        
        if (frameNeedsEvaluation)
        {
            reevaluateCannotCast();
        }

        spellIcon.bounds.X = casterX + 80 + (!cannotCast && !isAnimatingCast ? Game1.random.Next(-1, 1) : 0);
        spellIcon.bounds.Y = (centreY - 15) - (ModAssets.spellsSize / 2) + (!cannotCast && !isAnimatingCast ? Game1.random.Next(-1, 1) : 0);

        spellIcon.sourceRect = new Rectangle((cannotCast || isAnimatingCast ? ModAssets.spellsSize : 0), 
            ModAssets.spellsY + (targetSpell.id * ModAssets.spellsSize),ModAssets.spellsSize,ModAssets.spellsSize);

        if (frameNeedsEvaluation)
        {
            
            if (!cannotCast && !isAnimatingCast && fluffSprites.Count < MAX_FLUFF_SPRITES)
            {
                fluffSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(346, 392, 8, 8),
                    new Vector2(spellIcon.bounds.X + Game1.random.Next(ModAssets.spellsSize),
                        spellIcon.bounds.Y + Game1.random.Next(ModAssets.spellsSize)), flipped: false, 0.002f,
                    new Color(255, 222, 198))
                {
                    alphaFade = 0.02f,
                    motion = new Vector2(0, -1),
                    interval = 99999f,
                    layerDepth = 0.9f,
                    scale = 2f,
                    scaleChange = 0.01f,
                    rotation = Game1.random.Next(360),
                    delayBeforeAnimationStart = 0
                });
            }
        }
        
        fluffSprites.RemoveWhere((TemporaryAnimatedSprite sprite) => sprite.update(time));
        
        //Update cast time, and when we finish set the animations to new setup
        if (isAnimatingCast && castAnim.update(time))
        {
            castAnim = null;
            isAnimatingCast = false;
            
            if (!cannotCast)
            {
                caster.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[1]
                {
                    new FarmerSprite.AnimationFrame(68, 1800000, secondaryArm: false, flip: false)
                });
            }
            else
            {
                caster.FarmerSprite.PauseForSingleAnimation = false;
                caster.FarmerSprite.StopAnimation();
            }
        }
 
    }
    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        if(isAnimatingCast) {return;}
        
        base.receiveLeftClick(x, y, playSound: true);
        if (inputSpot.containsPoint(x, y))
        {
            (base.heldItem, inputSpot.item) = (inputSpot.item, base.heldItem); //Swap items
            reevaluateCannotCast();
            if (!cannotCast)
            {
                caster.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[1]
                {
                    new FarmerSprite.AnimationFrame(68, 1800000, secondaryArm: false, flip: false)
                });
            }
            else
            {
                caster.FarmerSprite.PauseForSingleAnimation = false;
                caster.FarmerSprite.StopAnimation();
            }
        }
        else if (spellIcon.containsPoint(x, y))
        {
            reevaluateCannotCast();
            if (!cannotCast)
            {
                KeyValuePair<bool, string> castReturn = targetSpell.CastSpell(ref inputSpot.item); //Cast the specified spell

                if (castReturn.Key)
                {
                    isAnimatingCast = true;

                    //play cast animation. we use the wrong texture name temporarily.
                    castAnim = new TemporaryAnimatedSprite("Mods.RunescapeSpellbook.Assets.spellanimations", new Rectangle(0, (targetSpell.spellAnimOffset * 34) + 16, 16, 34), 200f,
                        ModAssets.animFrames, 2,
                        new Vector2(casterX - 8, centreY - 48), false, false) { scale = 4f };
                    
                    Game1.player.playNearbySoundLocal($"RunescapeSpellbook.{targetSpell.audioID}");
                }
                else
                {
                    Game1.showRedMessage(castReturn.Value, true);
                }
            }
        }
    }

    public override void receiveRightClick(int x, int y, bool playSound = true)
    {
        base.receiveRightClick(x, y, playSound: true);
    }
    
    protected override void cleanupBeforeExit()
    {
        _OnCloseMenu();
    }

    public override void emergencyShutDown()
    {
        _OnCloseMenu();
        base.emergencyShutDown();
    }
    
    private void _OnCloseMenu()
    {
        //TODO theres maybe a bug where leaving this menu, even without doing anything gives you Iframes. could be abused for infinite invincibility
        
        fluffSprites.Clear();
        castAnim = null;
        
        if (base.heldItem != null)
        {
            Utility.CollectOrDrop(base.heldItem);
        }

        if (inputSpot.item != null)
        {
            Utility.CollectOrDrop(inputSpot.item);
        }
        
        base.heldItem = null;
        inputSpot.item = null;
        caster.FarmerSprite.PauseForSingleAnimation = false;
        caster.FarmerSprite.StopAnimation();
    }
    public override void draw(SpriteBatch b)
    {
        base.draw(b);
        Game1.dayTimeMoneyBox.drawMoneyBox(b);
        
        inputSpot.draw(b, Color.White, 0.96f);
        inputSpot.drawItem(b, 16, 16,isAnimatingCast ? 0.25f : 1f);
        
        b.Draw(Game1.mouseCursors, new Rectangle(casterX + 80, centreY - (ModAssets.spellsSize / 2),ModAssets.spellsSize,ModAssets.spellsSize),
            new Rectangle(325, 318, 25, 18), Color.White); //Back icon for spell cast
        spellIcon.draw(b,Color.White,0.96f);
        
        caster.FarmerRenderer.draw(b, Game1.player.FarmerSprite.CurrentAnimationFrame, Game1.player.FarmerSprite.CurrentFrame,
            Game1.player.FarmerSprite.SourceRect,
            new Vector2(casterX - 8, centreY - 48), Vector2.Zero, 0.8f, 2, Color.White, 0f, 1f, caster);
        
        foreach (TemporaryAnimatedSprite fluffSprite in fluffSprites)
        {
            fluffSprite.draw(b, localPosition: true);
        }

        if (castAnim != null)
        {
            castAnim.draw(b, localPosition: true);
        }
        
        int totalItems = targetSpell.requiredItems.Count;
        float itemWidth = 16 * 4; // Each item is 16 pixels wide and scaled by 4
        float spacing = 16; // 16 pixels spacing between items
        float totalWidth = (totalItems * itemWidth) + ((totalItems - 1) * spacing);
        float startX = (casterX + 16) - (totalWidth / 2);
        
        int itemIndex = 0;
        foreach (KeyValuePair<int,int> runePair in targetSpell.requiredItems) //Key is ID, value is amount
        {
            float runeX = startX + (itemIndex * (itemWidth + spacing));
            Vector2 runePos = new Vector2(runeX, centreY - (16 * 4 + 80));

            b.Draw(runesTextures, new Vector2(runePos.X,runePos.Y), new Rectangle(((runePair.Key - 4290) * 16), 0, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0001f);
            
            Color runeCountColour = targetSpell.HasRuneCost(runePair.Key) ? Color.LawnGreen : Color.Red;
            b.DrawString(Game1.dialogueFont, runePair.Value.ToString(), new Vector2(runePos.X + (16*2) + 10,runePos.Y + (16*2)), runeCountColour);

            itemIndex++;
        }
        
        base.heldItem?.drawInMenu(b, new Vector2(Game1.getOldMouseX() + 8, Game1.getOldMouseY() + 8), 1f);
        if (!Game1.options.hardwareCursor)
        {
            drawMouse(b);
        }
        
        //Add in description text
        int xPosition = xPositionOnScreen + 576 + 42 + ((wiggleWordsTimer > 0) ? Game1.random.Next(-2, 3) : 0);
        int yPosition = yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 32 + ((wiggleWordsTimer > 0) ? Game1.random.Next(-2, 3) : 0);
        int max_height = 320;
        float scale = 0f;
        string parsed_text;
        do
        {
            scale = ((scale != 0f) ? (scale - 0.1f) : 1f);
            parsed_text = Game1.parseText(spellDescriptionText, Game1.smallFont, (int)(224f / scale));
        }
        while (Game1.smallFont.MeasureString(parsed_text).Y > (float)max_height / scale && scale > 0.5f);
        Utility.drawTextWithColoredShadow(b, parsed_text, Game1.smallFont, new Vector2(xPosition, yPosition), Game1.textColor * 0.75f, Color.Black * 0.2f, scale);
    }
}