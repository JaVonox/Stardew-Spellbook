using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace RunescapeSpellbook;

//TODO should probably dummy this out before building. It might set off some virus scanners or something.
public class MassUnitTest
{
    public MassUnitTest(IModHelper helper,IMonitor monitor)
    {
        this.player = Game1.player;
        this.input = helper.Input;
        this.monitor = monitor;
        helper.Events.GameLoop.UpdateTicking += this.PreTick;
    }

    private Farmer player;
    private IInputHelper input;
    private IMonitor monitor;
    public async Task OperateUnitTest()
    {
        //Setup
        Game1.warpFarmer("Caldera", 14, 25, 0);
        await WaitForTicks(60,"Waited for Caldera");
        player.setInventory(new List<Item>());
        if (player.eventsSeen.Contains("Tofu.RunescapeSpellbook_Event0")) { player.eventsSeen.Remove("Tofu.RunescapeSpellbook_Event0");}
        if (player.eventsSeen.Contains("Tofu.RunescapeSpellbook_Event1")) { player.eventsSeen.Remove("Tofu.RunescapeSpellbook_Event1");}
        await WaitForTicks(120,"Setup Complete");
        
        //Basic Magic Menu checks
        await PressForTicks(SButton.ControllerStart, 60);
        InputNumberOfTimes(SButton.LeftThumbstickRight,8);
    }
    private void PreTick(object? sender, UpdateTickingEventArgs updateTickingEventArgs)
    {
        if(holdTcs == null){return;}
        
        remainingTicks--;
        
        if (remainingTicks < 0)
        {
            if (endMessage != null)
            {
                Game1.showGlobalMessage(endMessage);
                monitor.Log(endMessage);
            }
            heldButton = null;
            var tcs = holdTcs;
            holdTcs = null;
            tcs.SetResult();
        }
        else if(heldButton != null)
        {
            input.Press(heldButton.Value);
        }
    }

    private SButton? heldButton = null;
    private int remainingTicks = 0;
    private TaskCompletionSource? holdTcs = null;
    private string endMessage = null;
    public Task PressForTicks(SButton button, int ticks, string endMessage = null)
    {
        this.endMessage = endMessage;
        holdTcs = new TaskCompletionSource();
        remainingTicks = ticks;
        heldButton = button;        
        return holdTcs.Task;        
    }

    public Task WaitForTicks(int ticks,string endMessage = null)
    {
        this.endMessage = endMessage;
        holdTcs = new TaskCompletionSource();
        remainingTicks = ticks;
        heldButton = null;        
        return holdTcs.Task;  
    }

    public async void InputNumberOfTimes(SButton button, int times)
    {
        for (int i = 0; i < times; i++)
        {
            await PressForTicks(button,5);
        }
    }
}