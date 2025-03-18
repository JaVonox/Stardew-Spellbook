using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace StardewTestMod;

public class InventorySpellMenu : MenuWithInventory
{
    private ClickableTextureComponent inputSpot;

    private ClickableTextureComponent spellIcon;
    private Farmer caster;
    private Spell targetSpell;
    public Predicate<object>? selectablePredicate;
    
    private int centreY;
    private int casterX;

    private bool playCast = false;
    public InventorySpellMenu(Spell targetSpell, Predicate<object>? selectablePredicate) : base(null, okButton: true, trashCan: true, 12, 132)
    {
        this.targetSpell = targetSpell;
        this.selectablePredicate = selectablePredicate;
        descriptionText = targetSpell.description;
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
                centreY - (ModAssets.spellsSize / 2),
                ModAssets.spellsSize, ModAssets.spellsSize),ModAssets.extraTextures, new Rectangle(0,ModAssets.spellsY + (targetSpell.id * ModAssets.spellsSize),ModAssets.spellsSize,ModAssets.spellsSize), 1f);
    }

    public bool highlightTargets(Item i)
    {
        return this.selectablePredicate(i);
    }
    
    public override void update(GameTime time)
    {
        descriptionText = targetSpell.longDescription;
    }
    
    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        base.receiveLeftClick(x, y, playSound: true);
        if (inputSpot.containsPoint(x, y))
        {
            (base.heldItem, inputSpot.item) = (inputSpot.item, base.heldItem); //Swap items
            return;
        }
        else if (spellIcon.containsPoint(x, y))
        {
            if (inputSpot.item != null)
            {
                KeyValuePair<bool, string> castReturn = targetSpell.CastSpell(true,ref inputSpot.item); //Cast the specified spell

                if (!castReturn.Key)
                {
                    Game1.showRedMessage(castReturn.Value, true);
                }
                else
                {
                    playCast = true;
                }
            }
        }
        else
        {
            
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
    }

    private void EndCastAnimation()
    {
        playCast = false;
    }
    public override void draw(SpriteBatch b)
    {
        base.draw(b);
        Game1.dayTimeMoneyBox.drawMoneyBox(b);
        
        inputSpot.draw(b, Color.White, 0.96f);
        inputSpot.drawItem(b, 16, 16);
        
        b.Draw(Game1.mouseCursors, spellIcon.bounds,new Rectangle(325, 318, 25, 18), Color.White); //Back icon for spell cast
        spellIcon.draw(b,Color.White,0.96f);
        
        FarmerRenderer.isDrawingForUI = true;
        
        caster.FarmerRenderer.draw(b, new FarmerSprite.AnimationFrame(0, 0, secondaryArm: false, flip: false), 0,
            new Rectangle(0, 0, 16, 32),
            new Vector2(casterX - 8, centreY - 48), Vector2.Zero, 0.8f, 2, Color.White, 0f, 1f, caster);

        FarmerRenderer.isDrawingForUI = false;
        
        base.heldItem?.drawInMenu(b, new Vector2(Game1.getOldMouseX() + 8, Game1.getOldMouseY() + 8), 1f);
        if (!Game1.options.hardwareCursor)
        {
            drawMouse(b);
        }
    }
}