using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace RunescapeSpellbook;
/*
//TODO should probably dummy this out before building. It might set off some virus scanners or something.
public class MassUnitTest
{
    public MassUnitTest(IModHelper helper,IMonitor monitor, ModConfig config)
    {
        this.player = Game1.player;
        this.input = helper.Input;
        this.monitor = monitor;
        this.config = config;
    }

    private Farmer player;
    private IInputHelper input;
    private IMonitor monitor;
    private ModConfig config;

    private SpellbookPage spPage;
    private List<Spell> orderedSpells;
    public async Task OperateUnitTest()
    {
        if (player.mailReceived.Contains("Tofu.RunescapeSpellbook_HasUnlockedMagic"))
        {
            MessageOut("User already has magic. Might invalidate some tests.", LogLevel.Warn);
        }
        
        await InputNumberOfTimes(SButton.ControllerStart, 1);
        await Task.Delay(TimeSpan.FromMilliseconds(500));
        spPage = (SpellbookPage)((GameMenu)Game1.activeClickableMenu).pages[((GameMenu)Game1.activeClickableMenu).getTabNumberFromName("RSspellbook")];
        orderedSpells = ModAssets.modSpells.OrderBy(x => x.magicLevelRequirement).ToList();
        MessageOut("Finished Setup");
        
        await MagicMenuTests();
        await EventsTest();
    }

    private async Task FullReset()
    {
        await Task.Delay(TimeSpan.FromMilliseconds(500));
        
        Game1.warpFarmer("Caldera", 14, 25, 0);
        await Task.Delay(TimeSpan.FromMilliseconds(100));
        player.setInventory(new List<Item>());
        if (player.eventsSeen.Contains("Tofu.RunescapeSpellbook_Event0")) { player.eventsSeen.Remove("Tofu.RunescapeSpellbook_Event0");}
        if (player.eventsSeen.Contains("Tofu.RunescapeSpellbook_Event1")) { player.eventsSeen.Remove("Tofu.RunescapeSpellbook_Event1");}

        if (player.friendshipData.TryGetValue("Wizard", out Friendship wizFriendship))
        {
            wizFriendship.Clear();
            monitor.Log($"Cleared Wizard Friendship",LogLevel.Info);
        }

        if(player.mailReceived.Contains("Tofu.RunescapeSpellbook_HasUnlockedMagic")){player.mailReceived.Remove("Tofu.RunescapeSpellbook_HasUnlockedMagic");}
        if(player.mailReceived.Contains("Tofu.RunescapeSpellbook_RunesFound")){player.mailReceived.Remove("Tofu.RunescapeSpellbook_RunesFound");}
        //Can't really lower exp here so
        Game1.options.gamepadMode = Options.GamepadModes.ForceOn;

        if (Game1.activeClickableMenu != null)
        {
            Game1.activeClickableMenu.exitThisMenu();
        }
        
        await Task.Delay(TimeSpan.FromMilliseconds(3000));
    }

    private void MessageOut(string message, LogLevel level = LogLevel.Info)
    {
        if (level == LogLevel.Error)
        {
            Game1.showRedMessage(message,true);
        }
        else
        {
            Game1.showGlobalMessage(message);
        }
        monitor.Log(message,level);
    }
    
    private async Task PerformUnitTest(string unitCheckName,Func<bool> unitCheck)
    {
        bool result = unitCheck();
        if (result)
        {
            monitor.Log($"{unitCheckName} succeeded",LogLevel.Info);
        }
        else
        {
            MessageOut($"{unitCheckName} failed. Cascading errors may follow", LogLevel.Error);
            await Task.Delay(TimeSpan.FromMilliseconds(500));
        }
        
        await Task.Delay(TimeSpan.FromMilliseconds(100));
    }
    private async Task InputNumberOfTimes(SButton button, int times)
    {
        for (int i = 0; i < times; i++)
        {
            input.Press(button);
            await Task.Delay(TimeSpan.FromMilliseconds(100));
        }

        await Task.Delay(TimeSpan.FromMilliseconds(100));
    }

    private async Task MagicMenuTests()
    {
        await FullReset();
        MessageOut("Starting Magic Menu Tests");
        //Checking menu accessibility + no magic state
        await InputNumberOfTimes(SButton.ControllerStart, 1);
        await InputNumberOfTimes(SButton.LeftThumbstickUp, 1);
        await InputNumberOfTimes(SButton.LeftThumbstickRight, 8);
        await InputNumberOfTimes(SButton.ControllerA, 1);
        await PerformUnitTest("is spellbook page open (menu)",
            () =>
            {
                return Game1.activeClickableMenu is GameMenu men &&
                       men.currentTab == men.getTabNumberFromName("RSspellbook");
            });
        await PerformUnitTest("is spellbook page blank",
            () =>
            {
                return Game1.activeClickableMenu is GameMenu men && men.pages[men.getTabNumberFromName("RSspellbook")] is SpellbookPage spPage && !spPage.spellIcons.Any();
            });
        await InputNumberOfTimes(SButton.ControllerStart, 1);
        await InputNumberOfTimes(config.SpellbookKey.Keybinds[0].Buttons[0], 1);
        await PerformUnitTest("is spellbook page open (keybind)",
            () =>
            {
                return Game1.activeClickableMenu is GameMenu men &&
                       men.currentTab == men.getTabNumberFromName("RSspellbook");
            });
        await InputNumberOfTimes(config.SpellbookKey.Keybinds[0].Buttons[0], 1);
        await PerformUnitTest("is spellbook page closed (keybind)",
            () =>
            {
                return Game1.activeClickableMenu == null;
            });
        await Task.Delay(TimeSpan.FromMilliseconds(1000));

        player.eventsSeen.Add("Tofu.RunescapeSpellbook_Event0");
        player.mailReceived.Add("Tofu.RunescapeSpellbook_HasUnlockedMagic");
        await InputNumberOfTimes(config.SpellbookKey.Keybinds[0].Buttons[0], 1);
        await PerformUnitTest("is spellbook page open and populated",
            () =>
            {
                return Game1.activeClickableMenu is GameMenu men && men.pages[men.getTabNumberFromName("RSspellbook")] is SpellbookPage && spPage.spellIcons.Any();
            });
        
        for (int x = 0; x < orderedSpells.Count(); x++)
        {
            await CrawlMagic(orderedSpells[x].id);
        }
        
        await Task.Delay(TimeSpan.FromSeconds(1));
        MessageOut("Magic Menu Tests Completed");
    }

    private async Task EventsTest()
    {
        await FullReset();
        MessageOut("Starting Events Tests");
        
        NPC wizardNPC = Game1.getCharacterFromName("Wizard");
        player.changeFriendship(1000,wizardNPC);
        
        await Task.Delay(TimeSpan.FromSeconds(1));
        MessageOut("Events Tests Completed");
    }
    private async Task MagicCastTests()
    {
        await FullReset();
        MessageOut("Starting Magic Cast Tests");
        
        await Task.Delay(TimeSpan.FromSeconds(1));
        MessageOut("Magic Cast Tests Completed");
    }

    private async Task CrawlMagic(int spellIDTarget)
    {
        if (spPage.getCurrentlySnappedComponent() == null)
        {
            spPage.snapToDefaultClickableComponent();
        }

        int targetSpellIndex = orderedSpells.FindIndex(x => x.id == spellIDTarget);
        Point targetSpellPoint = new Point(targetSpellIndex % SpellbookPage.spellsPerRow + 1,targetSpellIndex / SpellbookPage.spellsPerRow);

        int curSpellID = int.Parse(spPage.getCurrentlySnappedComponent().name);
        int currentSpellIndex = orderedSpells.FindIndex(x => x.id == curSpellID);
        Point currentSpellPoint = new Point(currentSpellIndex % SpellbookPage.spellsPerRow + 1,currentSpellIndex / SpellbookPage.spellsPerRow);

        //TODO this doesn't work. Probably needs to be polling for changes before next iteration rather than a conditional check for each while. 
        while (currentSpellPoint != targetSpellPoint)
        {
            Point pointChange = targetSpellPoint - currentSpellPoint;

            this.monitor.Log($"cur Pos {currentSpellPoint} target {targetSpellPoint} changeReq {pointChange}",LogLevel.Warn);
            
            if (pointChange.Y < 0)
            {
                await InputNumberOfTimes(SButton.LeftThumbstickUp, 1);
            }
            else if (pointChange.Y > 0)
            {
                await InputNumberOfTimes(SButton.LeftThumbstickDown, 1);
            }
            
            if (pointChange.X < 0)
            {
                await InputNumberOfTimes(SButton.LeftThumbstickLeft, 1);
            }
            else if (pointChange.X > 0)
            {
                await InputNumberOfTimes(SButton.LeftThumbstickRight, 1);
            }
            
            curSpellID = int.Parse(spPage.getCurrentlySnappedComponent().name);
            currentSpellIndex = orderedSpells.FindIndex(x => x.id == curSpellID);
            currentSpellPoint = new Point(currentSpellIndex % SpellbookPage.spellsPerRow + 1,currentSpellIndex / SpellbookPage.spellsPerRow);
        }
    }
    private async Task EventTests()
    {
        Friendship fr = new Friendship();
        if (!player.friendshipData.TryAdd("Wizard", fr))
        {
            fr = player.friendshipData["Wizard"];
        }
        fr.Clear();
        MessageOut("Reset wizard friendship");
        
        Game1.timeOfDay = 1100;
        Game1.warpFarmer("Farm", 0, 0, flip: false);
        while (player.currentLocation.currentEvent != null)
        {
            player.currentLocation.currentEvent.skipEvent();
            await Task.Delay(TimeSpan.FromSeconds(3));
        }
        
        MessageOut("Event Tests Completed");
    }
}
*/