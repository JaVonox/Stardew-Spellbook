using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using GenericModConfigMenu;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using HarmonyLib;
using Leclair.Stardew.BetterGameMenu;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Buffs;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.BigCraftables;
using StardewValley.GameData.Buffs;
using StardewValley.GameData.Crops;
using StardewValley.GameData.FishPonds;
using StardewValley.GameData.Locations;
using StardewValley.GameData.Machines;
using StardewValley.GameData.Objects;
using StardewValley.GameData.Shops;
using StardewValley.GameData.Weapons;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.Projectiles;
using StardewValley.Tools;
using Object = StardewValley.Object;

namespace RunescapeSpellbook
{
    internal sealed class ModEntry : Mod
    {
        public static ModEntry Instance;
        private ModConfig Config;
        internal IBetterGameMenuApi? BetterGameMenuApi;
        public override void Entry(IModHelper helper)
        {
            var harmony = new Harmony(this.ModManifest.UniqueID);
            harmony.PatchAll();

            Instance = this;

            ModAssets.Load(helper);
            Config = helper.ReadConfig<ModConfig>();
            ModAssets.GetSpellBaseExpMultiplier = () => Config.SpellBaseExpMultiplier;
            
            helper.Events.Content.AssetRequested += this.OnAssetRequested;
            helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            
            helper.ConsoleCommands.Add("rs_grantmagic", KeyTranslator.GetTranslation("console.grantmagic.text"), this.GrantMagic);
            helper.ConsoleCommands.Add("rs_setlevel", KeyTranslator.GetTranslation("console.setlevel.text"), this.SetLevel);
            helper.ConsoleCommands.Add("rs_setexp", KeyTranslator.GetTranslation("console.setexp.text"), this.SetExp);
            helper.ConsoleCommands.Add("rs_addexp", KeyTranslator.GetTranslation("console.addexp.text"), this.AddExp);
            helper.ConsoleCommands.Add("rs_clearperks", KeyTranslator.GetTranslation("console.clearperks.text"), this.ResetPerks);
            helper.ConsoleCommands.Add("rs_info", KeyTranslator.GetTranslation("console.info.text"), this.PlayerInfo);
            helper.ConsoleCommands.Add("rs_addrunes", KeyTranslator.GetTranslation("console.addrunes.text"), this.GrantRunes);
            helper.ConsoleCommands.Add("rs_addweps", KeyTranslator.GetTranslation("console.addweps.text"), this.GrantStaffs);
            helper.ConsoleCommands.Add("rs_addammo", KeyTranslator.GetTranslation("console.addammo.text"), this.GrantAmmo);
            helper.ConsoleCommands.Add("rs_addtreasures", KeyTranslator.GetTranslation("console.addtreasures.text"), this.GrantTreasures);
            helper.ConsoleCommands.Add("rs_addpacks", KeyTranslator.GetTranslation("console.addpacks.text"), this.GrantPacks);
            helper.ConsoleCommands.Add("rs_addfish", KeyTranslator.GetTranslation("console.addfish.text"), this.GrantFish);
            helper.ConsoleCommands.Add("rs_addseeds", KeyTranslator.GetTranslation("console.addseeds.text"), this.GrantSeeds);
            helper.ConsoleCommands.Add("rs_addcrops", KeyTranslator.GetTranslation("console.addcrops.text"), this.GrantCrops);
            helper.ConsoleCommands.Add("rs_addpots", KeyTranslator.GetTranslation("console.addpots.text"), this.GrantPotions);
            helper.ConsoleCommands.Add("rs_debug_misc", KeyTranslator.GetTranslation("console.debugmisc.text"), this.DebugCommand);
            helper.ConsoleCommands.Add("rs_debug_position", KeyTranslator.GetTranslation("console.debugpos.text"), this.DebugPosition);
        }

        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            try
            {
            var configMenuAPI =
                this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenuAPI is not null) //Configmenu setup
            {
                configMenuAPI.Register(this.ModManifest, () => this.Config = new ModConfig(),
                    () => this.Helper.WriteConfig(this.Config));
                
                configMenuAPI.AddSectionTitle(this.ModManifest,()=>KeyTranslator.GetTranslation("settings.TitleGeneral.text"));

                configMenuAPI.AddKeybindList(
                    this.ModManifest, () => this.Config.SpellbookKey, value => this.Config.SpellbookKey = value,
                    () => KeyTranslator.GetTranslation("settings.SpellbookKeybind.text"),
                    () => KeyTranslator.GetTranslation("settings.SpellbookKeybind.tooltip"));
                
                /*
                configMenuAPI.AddParagraph(this.ModManifest, ()=>
                    "Certain mods may add new tabs in a way that conflicts with the spellbook tab. By default, if mods are detected that are known to do this," +
                    " the Spellbook will set itself to 'Only Keybind' mode on startup. you can reset this behaviour here. If Lock Spellbook Style is set, behaviour will not longer be " +
                    " automatically changed on startup");

                configMenuAPI.AddBoolOption(
                    this.ModManifest, () => this.Config.LockSpellbookStyle, value => this.Config.LockSpellbookStyle = value,
                    () => "Lock Spellbook Style",
                    () => "Selecting this as true will prevent any automatic toggling of the spellbook tab style if certain mods are detected");
                */
                
                configMenuAPI.AddTextOption(this.ModManifest, () => this.Config.SpellbookTabStyle,
                    value => this.Config.SpellbookTabStyle = value,
                    () => KeyTranslator.GetTranslation("settings.SpellbookTabStyle.text"), ()=> KeyTranslator.GetTranslation("settings.SpellbookTabStyle.tooltip"),
                    new string[] { "Tab and Keybind", "Only Keybind" });
                
                configMenuAPI.AddParagraph(this.ModManifest, () => KeyTranslator.GetTranslation("settings.RelaunchText.text"));
                
                configMenuAPI.AddSectionTitle(this.ModManifest,()=>KeyTranslator.GetTranslation("settings.TitleModifiers.text"));

                configMenuAPI.AddParagraph(this.ModManifest, ()=> KeyTranslator.GetTranslation("settings.MultiplierParagraph.intro") +
                    (Context.IsWorldReady ? KeyTranslator.GetTranslation("settings.MultiplierParagraph.in-game-text", new {PlayerName = Game1.player.Name}) 
                        : KeyTranslator.GetTranslation("settings.MultiplierParagraph.out-game-text")));
                
                configMenuAPI.AddNumberOption(this.ModManifest,
                    () => !Context.IsWorldReady ? this.Config.SpellBaseExpMultiplier : 
                        (int.Parse(ModAssets.TryGetModVariable(Game1.player,"Tofu.RunescapeSpellbook_Setting-MagicExpMultiplier"))) , 
                    value => 
                    {
                        if (!Context.IsWorldReady)
                        {
                            this.Config.SpellBaseExpMultiplier = value;
                        }
                        else
                        {
                            ModAssets.TrySetModVariable(Game1.player,"Tofu.RunescapeSpellbook_Setting-MagicExpMultiplier",$"{value}");
                        }
                    },
                    () => !Context.IsWorldReady ? KeyTranslator.GetTranslation("settings.Multiplier.out-game-text") : KeyTranslator.GetTranslation("settings.Multiplier.in-game-text"),
                    () => !Context.IsWorldReady ? KeyTranslator.GetTranslation("settings.Multiplier.out-game-tooltip") : KeyTranslator.GetTranslation("settings.Multiplier.in-game-tooltip"), 20,
                    200, 5,
                    (num) => $"{num}%");
                
            }
            }
            catch (Exception exception)
            {
                Instance.Monitor.Log(KeyTranslator.GetTranslation("log.ModConfigError.text",new{Exception = exception.Message}),LogLevel.Error);
            }

            try
            {
                BetterGameMenuApi = this.Helper.ModRegistry.GetApi<IBetterGameMenuApi>("leclair.bettergamemenu");
                if (BetterGameMenuApi is not null) //BetterGameMenu setup
                {
                    //TODO betterGameMenu doesn't seem to properly handle a spellbook tab style being keybind only with my current setup. Should fix later.
                    BetterGameMenuApi.RegisterTab("RSspellbook", 159, () => KeyTranslator.GetTranslation("ui.TabName.text"),
                        () => (BetterGameMenuApi.CreateDraw(ModAssets.extraTextures, new Rectangle(0, 0, 16, 16), 4),
                            false), 0,
                        menu => new SpellbookPage(menu.xPositionOnScreen, menu.yPositionOnScreen, menu.width - 64 - 16,
                            menu.height),
                        onResize: input => new SpellbookPage(input.Menu.xPositionOnScreen, input.Menu.yPositionOnScreen,
                            input.Menu.width - 64 - 16, input.Menu.height));

                }
            }
            catch (Exception exception)
            {
                Instance.Monitor.Log(KeyTranslator.GetTranslation("log.GameMenuAPIError.text",new{Exception = exception.Message}),LogLevel.Error);
            }

            if (!Config.LockSpellbookStyle && BetterGameMenuApi is null)
            {
                //bool loadedRiskyMod = false;
                
                
                if (this.Helper.ModRegistry.IsLoaded("Annosz.UiInfoSuite2"))
                {
                    Instance.Monitor.Log(KeyTranslator.GetTranslation("log.UsingOverlapMod.line-1"),LogLevel.Warn);
                    Instance.Monitor.Log(KeyTranslator.GetTranslation("log.UsingOverlapMod.line-2"), LogLevel.Warn);
                    Instance.Monitor.Log(KeyTranslator.GetTranslation("log.UsingOverlapMod.line-3"),LogLevel.Warn);
                    string firstKey = Config.SpellbookKey.Keybinds.Length > 0
                        ? Config.SpellbookKey.Keybinds[0].ToString()
                        : "Unbound";
                    
                    Instance.Monitor.Log(KeyTranslator.GetTranslation("log.UsingOverlapMod.line-4", new {Keybind = firstKey}),LogLevel.Warn);
                    
                    /*
                    loadedRiskyMod = true;
                    Config.SpellbookTabStyle = "Only Keybind";
                    */
                }

                /*
                if (loadedRiskyMod)
                {
                    Instance.Monitor.Log("RunescapeSpellbook has discovered one or more mods are enabled that might cause UI overlaps.",LogLevel.Warn);
                    Instance.Monitor.Log("The Runescape Spellbook menu tab has been set to 'Only Keybind' mode to prevent UI problems.", LogLevel.Warn);
                    Instance.Monitor.Log("You can reenable the spellbook tab by setting the Spellbook Tab Style in the config, but relaunching the game with these mods enabled will automatically set you into 'Only Keybind' Mode again unless you set 'Lock Spellbook Style' to true",LogLevel.Warn);
                    Instance.Monitor.Log($"Your current spellbook keybind is set to {Config.SpellbookKey.Keybinds[0].ToString()}",LogLevel.Warn);
                }
                */
            }
            
        }
        private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
        {
            if (!Context.IsWorldReady)
            {
                return;
            }

            //On spellbookKeybind we open the spellbook menu
            if (Config.SpellbookKey.JustPressed())
            {
                if (Game1.activeClickableMenu == null && Game1.CanShowPauseMenu())
                {
                    IClickableMenu menu;
                    Game1.PushUIMode();
                    
                    if (BetterGameMenuApi is not null)
                    {
                        menu = BetterGameMenuApi.CreateMenu("RSspellbook", true);
                    }
                    else
                    {
                        menu = new GameMenu(0);
                        ((GameMenu)menu).changeTab(((GameMenu)menu).getTabNumberFromName("RSspellbook"));
                    }
                    
                    Game1.activeClickableMenu = menu;
                    Game1.PopUIMode();

                }
                else if (Game1.activeClickableMenu != null && ((BetterGameMenuApi is not null && BetterGameMenuApi.IsMenu(Game1.activeClickableMenu) && BetterGameMenuApi.ActiveMenu != null && BetterGameMenuApi.ActiveMenu.CurrentTab == "RSspellbook")
                         || (Game1.activeClickableMenu is GameMenu menu && menu.currentTab == menu.getTabNumberFromName("RSspellbook"))))
                {
                    Game1.activeClickableMenu.exitThisMenu();
                }
                else
                {
                    Game1.showRedMessage(KeyTranslator.GetTranslation("ui.BlockSpellbookOpen.text"));
                }
            }
        }

        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Mods.RunescapeSpellbook.Assets.itemsprites"))
            {
                e.LoadFromModFile<Texture2D>("assets/itemsprites", AssetLoadPriority.Medium);
            }
            
            if (e.NameWithoutLocale.IsEquivalentTo("Mods.RunescapeSpellbook.Assets.spellanimations"))
            {
                e.LoadFromModFile<Texture2D>("assets/spellanimations", AssetLoadPriority.Medium);
            }
            
            if (e.NameWithoutLocale.IsEquivalentTo("Mods.RunescapeSpellbook.Assets.modplants"))
            {
                e.LoadFromModFile<Texture2D>("assets/modplants", AssetLoadPriority.Medium);
            }
            
            if (e.NameWithoutLocale.IsEquivalentTo("Mods.RunescapeSpellbook.Assets.buffsprites"))
            {
                e.LoadFromModFile<Texture2D>("assets/buffsprites", AssetLoadPriority.Medium);
            }
            
            if (e.NameWithoutLocale.IsEquivalentTo("Mods.RunescapeSpellbook.Assets.modmachines"))
            {
                e.LoadFromModFile<Texture2D>("assets/modmachines", AssetLoadPriority.Medium);
            }
            
            if (e.NameWithoutLocale.IsEquivalentTo("Data/AudioChanges"))
            {
                e.Edit(asset =>
                    {
                        List<string> audioTracks = ModAssets.modSpells.Select(x=>x.audioID).Distinct().ToList();
                        audioTracks.Add("Splash"); //Add the sound for hitting
                        audioTracks.Add("MagicLevel"); //Add the sound for levelling up
                        audioTracks.Add("MultiHit"); //Add the sound for when you fire multiple projectiles
                        audioTracks.Add("Liquid"); //Add Poison-y sound
                        audioTracks.Add("FireBurn"); //Add flame sound
                        
                        var data = asset.AsDictionary<string, AudioCueData>().Data;
                        foreach (string audioTrack in audioTracks)
                        {
                            data.Add($"{audioTrack}", new AudioCueData() {
                                Id = $"RunescapeSpellbook.{audioTrack}",
                                Category = "Sound",
                                FilePaths = new() { Path.Combine(Helper.DirectoryPath, "assets/Audio", $"{audioTrack}.ogg") },
                            });
                        }
                    }
                );
            }
            
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Objects"))
            {
                e.Edit(asset =>
                    {
                        var objectDict = asset.AsDictionary<string, ObjectData>().Data;

                        foreach (ModLoadObjects newObject in ModAssets.modItems.Values)
                        {
                            newObject.AppendObject(objectDict);
                        }
                        
                    }
                );
            }
            
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Shops"))
            {
                e.Edit(asset =>
                    {
                        foreach (KeyValuePair<string,List<ShopListings>> addShopData in ModAssets.loadableShops)
                        {
                            ShopData shopData = asset.AsDictionary<string, ShopData>().Data[addShopData.Key];
                            foreach (ShopListings item in addShopData.Value)
                            {
                                shopData.Items.Insert(item.insertIndex,item.itemData);
                            }
                        }
                    }
                );
            }
            
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Crops"))
            {
                e.Edit(asset =>
                    {
                        var cropDict = asset.AsDictionary<string, CropData>().Data;
                        foreach (ModLoadObjects newObject in ModAssets.modItems.Where(x=>x.Value is CropObject).Select(y=>y.Value).ToList())
                        {
                            ((CropObject)newObject).AppendCropData(cropDict);
                        }
                    }
                );
            }
            
            if (e.NameWithoutLocale.IsEquivalentTo("Data/BigCraftables"))
            {
                e.Edit(asset =>
                    {
                        var machineCraftablesDict = asset.AsDictionary<string, BigCraftableData>().Data;
                        
                        foreach (MachinesObject machineItem in ModAssets.machineItems)
                        {
                            machineCraftablesDict.Add(machineItem.id,machineItem);
                        }
                    }
                );
            }
            
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Machines"))
            {
                e.Edit(asset =>
                    {
                        var machineDict = asset.AsDictionary<string, MachineData>().Data;
                        
                        foreach (MachinesObject machineItem in ModAssets.machineItems)
                        {
                            machineItem.AddMachineRules(machineDict);
                        }
                        
                        foreach (PotionObject newKegItem in ModAssets.modItems.Where(x=>x.Value is PotionObject pot && pot.craftType != 0).Select(y=>y.Value).ToList())
                        {
                            newKegItem.AddMachineOutput(machineDict);
                        }
                    }
                );
            }
            
            if (e.NameWithoutLocale.IsEquivalentTo("Data/CookingRecipes"))
            {
                e.Edit(asset =>
                    {
                        var cookingDict = asset.AsDictionary<string, string>().Data;
                        
                        foreach (PotionObject newKegItem in ModAssets.modItems.Where(x=>x.Value is PotionObject pot && pot.craftType == 0).Select(y=>y.Value).ToList())
                        {
                            newKegItem.AddCookingOutput(cookingDict);
                        }
                    }
                );
            }
            
            if (e.NameWithoutLocale.IsEquivalentTo("Data/CraftingRecipes"))
            {
                e.Edit(asset =>
                    {
                        var craftingRecipe = asset.AsDictionary<string, string>().Data;
                        
                        foreach (MachinesObject newKegItem in ModAssets.machineItems.Where(x=>x.creationString != null).ToList())
                        {
                            newKegItem.AddCraftingRecipe(craftingRecipe);
                        }
                    }
                );
            }
            
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Fish"))
            {
                e.Edit(asset =>
                    {
                        var fishDict = asset.AsDictionary<string, string>().Data;
                        foreach (ModLoadObjects newObject in ModAssets.modItems.Where(x=>x.Value is FishObject).Select(y=>y.Value).ToList())
                        {
                            ((FishObject)newObject).AppendFishData(fishDict);
                        }
                    }
                );
            }
            
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Buffs"))
            {
                e.Edit(asset =>
                    {
                        var buffDict = asset.AsDictionary<string, BuffData>().Data;
                        foreach (CustomBuff newBuff in ModAssets.loadableBuffs)
                        {
                            newBuff.AppendBuff(buffDict);
                        }
                    }
                );
            }

            
            if (e.NameWithoutLocale.IsEquivalentTo("Data/FishPondData"))
            {
                e.Edit(asset =>
                    {
                        IList<FishPondData> pondList = (IList<FishPondData>)(asset.GetData<object>());
                        
                        foreach (ModLoadObjects newObject in ModAssets.modItems.Where(x=>x.Value is FishObject).Select(y=>y.Value).ToList())
                        {
                            ((FishObject)newObject).AppendPondData(pondList);
                        }
                    }
                );
            }
            
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Locations"))
            {
                e.Edit(asset =>
                    {
                        var locationDict = asset.AsDictionary<string, LocationData>().Data;
                        
                        foreach (ModLoadObjects newObject in ModAssets.modItems.Where(x=>x.Value is FishObject).Select(y=>y.Value).ToList())
                        {
                            ((FishObject)newObject).AppendLocationData(locationDict);
                        }
                    }
                );
            }

            if (e.NameWithoutLocale.StartsWith("Data/Events") && ModAssets.loadableEvents.ContainsKey(e.NameWithoutLocale.Name))
            {
                e.Edit(asset =>
                {
                    var eventDict = asset.AsDictionary<string, string>().Data;

                    foreach (LoadableEvent eventData in ModAssets.loadableEvents[
                                 e.NameWithoutLocale.Name])
                    {
                        eventDict.Add(eventData.id, eventData.contents[0]);
                    }
                });
            }
            
            //Possible mails
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Mail"))
            {
                e.Edit(asset =>
                    {
                        var mailDict = asset.AsDictionary<string, string>().Data;

                        foreach (LoadableMail mail in ModAssets.loadableText.Where(x=>x is LoadableMail))
                        {
                            string mailKey = mail.id;
                            string mailVal = mail.contents[0];
                            
                            try
                            {
                                while (true) //Loop until we get a valid assignment for mail
                                {
                                    //This removes the mail if it overlaps with another mail - such as if a new mail is added on a date the mod uses or if a future update adds the same key
                                    if (mailDict.TryAdd(mailKey,mailVal))
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        Monitor.Log(KeyTranslator.GetTranslation("log.DuplicateMailKey.text", new {MailKey = mailKey}), LogLevel.Warn);
                                        string[] mailDelim = mailKey.Split('_');
                                        int newYear = int.Parse(mailDelim[2]) + 1;
                                        
                                        if (newYear > 255)
                                        {
                                            throw new Exception(KeyTranslator.GetTranslation("log.TooManyMailKeys.text", new {MailKey = mailKey}));
                                        }
                                        
                                        mailKey = mailDelim[0] + "_" + mailDelim[1] + "_" + newYear; //Adds mail to the same date next year. increments year until we get a valid value
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Monitor.Log(ex.Message, LogLevel.Error); //reports mail error if the mail delim method failed. this should be rare, and only should occur with non-dated mail
                                break;
                            }
                        }
                    }
                );
            }

            if (e.NameWithoutLocale.IsEquivalentTo("Data/NPCGiftTastes"))
            {
                e.Edit(asset =>
                {
                    var preferencesDict = asset.AsDictionary<string, string>().Data;

                    foreach (string characterName in preferencesDict.Keys)
                    {
                        if (characterName == "Universal_Dislike")
                        {
                            //Add all items from the mod to universal dislikes
                            preferencesDict["Universal_Dislike"] = preferencesDict["Universal_Dislike"] +
                                                                   " " + ModAssets.modItems.Keys.Join(null," ");
                        }
                        else if (!characterName.Contains("Universal_")) //We only accept non universals for this so far
                        {
                            Dictionary<string,PrefType> itemPreferences = 
                                ModAssets.modItems.Where(x=>x.Value.characterPreferences != null && x.Value.characterPreferences.Keys.Contains(characterName))
                                    .ToDictionary(i=>i.Value.id,j=> j.Value.characterPreferences[characterName]); //get a dictionary of gifts for this character with their preference
                            
                            if(!itemPreferences.Any()) {continue;} //If we have no data to assign, skip this entirely
                            
                            //If all treasures are the same for a treasureitem, then we consider it the same as gifting the item itself
                            //I.e. Fire rune packs will have the same gift value as a fire rune
                            Dictionary<string, PrefType> treasureItems =
                                ModAssets.modItems.Where(x =>
                                    x.Value is PackObject pack && ModAssets.modItems.ContainsKey(pack.packItem) && ModAssets.modItems[pack.packItem].characterPreferences.ContainsKey(characterName))
                                    .ToDictionary(i=>i.Value.id,j=> ModAssets.modItems[((PackObject)j.Value).packItem].characterPreferences[characterName]);

                            if (treasureItems.Any()) //Add treasures in to item preferences
                            {
                                itemPreferences.TryAddMany(treasureItems);
                            }

                            string[] characterPrefsStrings = preferencesDict[characterName].Split('/'); //character preferences are delimited with / 
                            //The indexes always follow this format: 0 text 1 loveIDs 2 text 3 likeIDs 4 text 5 dislikeIDs 6 text 7 hateIDs 8 text 9 neutralIDs
                            
                            foreach (KeyValuePair<string, PrefType> itemPref in itemPreferences)
                            {
                                int idToModify = itemPref.Value switch
                                {
                                    PrefType.Love => 1,
                                    PrefType.Like => 3,
                                    PrefType.Dislike => 5,
                                    PrefType.Hate => 7,
                                    PrefType.Neutral => 9
                                };
                                
                                characterPrefsStrings[idToModify] += $" {itemPref.Key}";
                            }

                            preferencesDict[characterName] = characterPrefsStrings.Join(null, "/");
                        }
                    }
                });
            }
            
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Weapons"))
            {
                e.Edit(asset =>
                {
                    var weaponDict = asset.AsDictionary<string, WeaponData>().Data;
                    
                    foreach (StaffWeaponData newWeapon in ModAssets.staffWeapons)
                    {
                        weaponDict.Add(newWeapon.id, newWeapon);
                    }
                });

            }
        }
        
        //Add menu item to getTabNumberFromName
        [HarmonyPatch(typeof(GameMenu), "getTabNumberFromName")]
        public class GameMenuTabNumberPatch
        {
            public static bool Prefix(GameMenu __instance, string name, ref int __result)
            {
                if (name == "RSspellbook")
                {
                    __result = __instance.tabs.FindIndex(x => x.name == "RSspellbook");
                    return false; //effectively a break
                }

                return true; //Makes the previous method continue
            }
        }
        
        //Add menu tab 
        [HarmonyPatch(typeof(GameMenu), MethodType.Constructor)]
        [HarmonyPatch(new Type[] { typeof(bool) })]
        public class GameMenuConstructorPatch
        {
            public static void Postfix(GameMenu __instance, bool playOpeningSound = true)
            {
                bool isOnlyKeybind = Instance.Config.SpellbookTabStyle == "Only Keybind";
                //If we are not on only keybind, move the spellbook tab to be left of the game menu
                if (!isOnlyKeybind)
                {
                    //Move the options and exit tags right two to fit in spellbook page
                    //exit tab
                    __instance.tabs[^1].bounds = new Rectangle(__instance.xPositionOnScreen + 704,
                        __instance.yPositionOnScreen + IClickableMenu.tabYPositionRelativeToMenuY + 64, 64, 64);
                    __instance.tabs[^1].myID = 12351;
                    __instance.tabs[^1].leftNeighborID = 12350;
                
                    //options tab
                    __instance.tabs[^2].bounds = new Rectangle(__instance.xPositionOnScreen + 640,
                        __instance.yPositionOnScreen + IClickableMenu.tabYPositionRelativeToMenuY + 64, 64, 64);
                    __instance.tabs[^2].myID = 12350;
                    __instance.tabs[^2].leftNeighborID = 12349;
                    
                }
                
                //spellbook tab
                __instance.tabs.Add(new ClickableComponent(
                    new Rectangle(__instance.xPositionOnScreen + 576,
                        __instance.yPositionOnScreen + IClickableMenu.tabYPositionRelativeToMenuY + 64, 64, 64),
                    "RSspellbook", KeyTranslator.GetTranslation("ui.TabName.text"))
                {
                    myID = 12349,
                    downNeighborID = 9,
                    leftNeighborID = 12348,
                    tryDefaultIfNoDownNeighborExists = true,
                    fullyImmutable = true,
                    visible = !isOnlyKeybind,
                });

                //spellbook page
                __instance.pages.Add(new SpellbookPage(__instance.xPositionOnScreen, __instance.yPositionOnScreen,
                    __instance.width - 64 - 16, __instance.height));
            }
        }

        [HarmonyPatch(typeof(GameMenu), "draw")]
        [HarmonyPatch(new Type[] { typeof(SpriteBatch) })]
        public class GameMenuDrawPatch
        {
            public static void Postfix(GameMenu __instance, SpriteBatch b)
            {
                if(!__instance.invisible && Instance.Config.SpellbookTabStyle != "Only Keybind")
                {
                    //this is used to ensure that we dont overlap any big menus
                    if ((__instance.pages[__instance.currentTab] as CollectionsPage)?.letterviewerSubMenu == null)
                    {
                        ClickableComponent c = __instance.tabs.First(x=>x.name == "RSspellbook");

                        if (c.visible)
                        {
                            b.Draw(ModAssets.extraTextures,
                                new Vector2(c.bounds.X,
                                    c.bounds.Y +
                                    ((__instance.currentTab == __instance.getTabNumberFromName(c.name)) ? 8 : 0)),
                                new Rectangle(0, 0, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None,
                                0.0001f);
                        }
                    }

                    if (!__instance.hoverText.Equals(""))
                    {
                        IClickableMenu.drawHoverText(b, __instance.hoverText, Game1.smallFont);
                    }
                    __instance.drawMouse(b, ignore_transparency: true);
                }
            }
        }

        //Prevent Geode crushing for special items
        [HarmonyPatch(typeof(Utility), "IsGeode")]
        [HarmonyPatch(new Type[] { typeof(Item),typeof(bool)})]
        public class GeodeCrusherPatch
        {
            public static bool Prefix(ref bool __result, Item item, bool disallow_special_geodes)
            {
                //Special geode blocking check now includes TreasureObjects, since those can output weapons. This prevents bugs with using geodes in the geode crusher.
                //ContextTags could be used to do this as well, but it seems to cause errors too.
                if (disallow_special_geodes && ModAssets.modItems.TryGetValue(item.ItemId, out ModLoadObjects modItem))
                {
                    if (item.ItemId != "Tofu.RunescapeSpellbook_TreasureElemental" && item.ItemId != "Tofu.RunescapeSpellbook_TreasureCatalytic" && modItem is TreasureObjects and not PackObject)
                    {
                        __result = false;
                        return false;
                    }
                }

                return true;
            }
        }
        
        [HarmonyPatch(typeof(Farmer), "Update")]
        [HarmonyPatch(new Type[] { typeof(GameTime),typeof(GameLocation)})]
        public class FarmerBonusHealthPatcher
        {
            public static void Prefix(ref Farmer __instance, GameTime time, GameLocation location)
            {
                //Handle Bonus Health
                if (__instance.health > __instance.maxHealth)
                {
                    ModAssets.localFarmerData.bonusHealth += __instance.health - __instance.maxHealth;
                    __instance.health = __instance.maxHealth;
                }
                else if (ModAssets.localFarmerData.bonusHealth > 0)
                {
                    ModAssets.localFarmerData.bonusHealth -= __instance.maxHealth - __instance.health;
                    __instance.health = __instance.maxHealth;
                    
                    if (ModAssets.localFarmerData.bonusHealth < 0)
                    {
                        __instance.health -= Math.Abs(ModAssets.localFarmerData.bonusHealth);
                        ModAssets.localFarmerData.bonusHealth = 0;
                    }
                }
            }
        }
        
        [HarmonyPatch(typeof(Farmer), "doneEating")]
        public class FarmerEatingPatch
        {
            public static bool Prefix(ref Farmer __instance)
            {
                if (__instance.mostRecentlyGrabbedItem != null && __instance.IsLocalPlayer)
                {
                    Object consumed = __instance.itemToEat as Object;
                    string consumedID = consumed.QualifiedItemId;
                    if (ModAssets.modItems.Any(x=> x.Value is PotionObject && consumedID == $"(O){x.Key}"))
                    {
                        PotionObject pot = (PotionObject)ModAssets.modItems[consumed.ItemId];
                        __instance.isEating = false;
                        __instance.tempFoodItemTextureName.Value = null;
                        __instance.completelyStopAnimatingOrDoingAction();
                        __instance.forceCanMove();

                        if (consumedID == "(O)Tofu.RunescapeSpellbook_PotGuthix" || consumedID == "(O)Tofu.RunescapeSpellbook_PotSara")
                        {
                            int addAmount = (int)Math.Floor((float)__instance.maxHealth * (pot.healPercent + ((float)__instance.itemToEat.Quality * pot.extraHealthPerQuality)));
                            int newTotal = __instance.health + addAmount;
                            
                            if (__instance.health + ModAssets.localFarmerData.bonusHealth < newTotal)
                            {
                                ModAssets.localFarmerData.bonusHealth = newTotal <= __instance.maxHealth ? 0 : newTotal - __instance.maxHealth;
                                __instance.health = Math.Min(newTotal, __instance.maxHealth);
                            }
                        }
                        
                        foreach (Buff buff in consumed.GetFoodOrDrinkBuffs())
                        {
                            __instance.applyBuff(buff);
                        }
                        
                        return false;
                    }
                }
                
                return true;
            }
        }
        
        [HarmonyPatch(typeof(Farmer), "dayupdate")]
        [HarmonyPatch(new Type[] { typeof(int)})]
        public class FarmerSleepPatcher
        {
            public static void Prefix(ref Farmer __instance, int timeWentToSleep)
            {
                ModAssets.localFarmerData.bonusHealth = 0;
            }
        }
        
        [HarmonyPatch(typeof(BobberBar), MethodType.Constructor)]
        [HarmonyPatch(new Type[] { typeof(string), typeof(float),typeof(bool),typeof(List<string>),typeof(string),typeof(bool),typeof(string),typeof(bool)})]
        public class BobberConstructPatcher
        {
            public static void Postfix(ref BobberBar __instance, string whichFish, float fishSize, bool treasure, List<string> bobbers, string setFlagOnCatch, bool isBossFish, string baitID = "", bool goldenTreasure = false)
            {
                if (Game1.player.hasBuff("Tofu.RunescapeSpellbook_BuffHunters"))
                {
                    __instance.bobbers.Add("Tofu.RunescapeSpellbook_BuffHunters");
                }
            }
        }

        public static class BobberPatches
        {

            [HarmonyPatch(typeof(BobberBar), "update")]
            [HarmonyPatch(new Type[] { typeof(GameTime) })]
            public class BobberUpdatePatcher
            {
                public static void Postfix(ref BobberBar __instance, GameTime time)
                {
                    bool bobberInMiddleZone = IsBobberInsideMini(ref __instance, out int middleZoneCentre, out int middleZoneStart,
                        out int middleZoneEnd, out int middleZoneHeight);
                    
                    if (__instance.bobberInBar && bobberInMiddleZone && __instance.distanceFromCatching < 1.0f &&
                        __instance.bobbers.Contains("Tofu.RunescapeSpellbook_BuffHunters"))
                    {
                        __instance.distanceFromCatching += 0.001f;
                    }
                }
            }

            [HarmonyPatch(typeof(BobberBar), "draw")]
            [HarmonyPatch(new Type[] { typeof(SpriteBatch) })]
            public class BobberDrawPatcher
            {
                public static void Postfix(ref BobberBar __instance, SpriteBatch b)
                {
                    if (__instance.scale == 1f && __instance.bobbers.Contains("Tofu.RunescapeSpellbook_BuffHunters"))
                    {
                        Game1.StartWorldDrawInUI(b);

                        bool bobberInMiddleZone = IsBobberInsideMini(ref __instance, out int middleZoneCentre, out int middleZoneStart,
                            out int middleZoneEnd, out int middleZoneHeight);
                        
                        Vector2 targetPosition = new Vector2(
                            __instance.xPositionOnScreen + (64 * __instance.scale - 12f),
                            __instance.yPositionOnScreen + 12 + middleZoneStart
                        ) + __instance.barShake + __instance.everythingShake;

                        b.Draw(Game1.mouseCursors,
                            targetPosition,
                            new Rectangle(682, 2078, 9, 2),
                            bobberInMiddleZone ? Color.Purple : Color.Red,
                            -(float)Math.PI / 2f,
                            new Vector2(9, 0),
                            new Vector2(((float)middleZoneHeight) / 9f,6f),
                            SpriteEffects.None,
                            0.1f);
                        Game1.EndWorldDrawInUI(b);
                    }
                }
            }

            private static readonly float zonePercent = 0.3f;
            private static bool IsBobberInsideMini(ref BobberBar __instance, out int middleZoneCentre, out int middleZoneStart, out int middleZoneEnd, out int middleZoneHeight)
            {
                middleZoneCentre =
                    (int)Math.Floor(__instance.bobberBarPos + (GetRandomPosition(ref __instance)));
                middleZoneStart =
                    middleZoneCentre - (int)Math.Floor((float)__instance.bobberBarHeight * (zonePercent / 2.0f));
                middleZoneEnd =
                    middleZoneCentre + (int)Math.Floor((float)__instance.bobberBarHeight * (zonePercent / 2.0f));
                middleZoneHeight = middleZoneEnd - middleZoneStart;

                return (__instance.bobberPosition + 4f) >= (middleZoneStart - 32f) &&
                                          (__instance.bobberPosition - 4f) <= (middleZoneEnd - 32f);
            }

            private static int GetRandomPosition(ref BobberBar __instance)
            {
                float halfZone = __instance.bobberBarHeight * (zonePercent / 2.0f);
                return (int)halfZone + (Game1.dayOfMonth * __instance.maxFishSize * (int)Math.Floor(Game1.player.stamina)) % (int)(__instance.bobberBarHeight - 2 * halfZone);
            }
        }

        [HarmonyPatch(typeof(Game1), "drawHUD")]
        public class FarmerHealthImage
        {
            private static readonly List<Color> healthTiers = new()
            {
                new Color(0,255,0),
                new Color(0,128,255),
                new Color(255,0,255),
                new Color(255,128,0),
                new Color(0,0,255),
                new Color(0,255,127),
                new Color(255,0,128),
                new Color(128,0,255),
            };

            public static void Prefix(ref Game1 __instance)
            {
                if (Game1.hitShakeTimer > 0 && ModAssets.localFarmerData.bonusHealth > 0)
                {
                    Game1.hitShakeTimer = 0;
                }
            }

            public static void Postfix(ref Game1 __instance)
            {
                if (Game1.showingHealthBar && ModAssets.localFarmerData.bonusHealth > 0)
                {
                    //Get how many times we go over the healthbar
                    int healthDepth = (1 + ModAssets.localFarmerData.bonusHealth / Game1.player.maxHealth);
                    
                    Vector2 topOfBar = new Vector2(Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Right - 48 - 8, Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Bottom - 224 - 16 - (int)((float)(Game1.player.MaxStamina - 270) * 0.625f));
                    topOfBar.X -= 56;
                    topOfBar.Y = Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Bottom - 224 - 16 - (Game1.player.maxHealth - 100);
                    int barFullHeight = 168 + (Game1.player.maxHealth - 100);
                    
                    DrawBonusHealthBar(healthDepth - 1,Game1.player.maxHealth,topOfBar,barFullHeight);
                    DrawBonusHealthBar(healthDepth,ModAssets.localFarmerData.bonusHealth % Game1.player.maxHealth,topOfBar,barFullHeight);
                    
                    //Extra health draw code
                    /*
                    string bonusHealth = $"+{ModAssets.localFarmerData.bonusHealth}";
                    Vector2 stringSize = Game1.dialogueFont.MeasureString(bonusHealth);
                    Game1.drawWithBorder(bonusHealth, Color.Black * 0f, healthTiers[1], new Vector2(topOfBar.X - stringSize.X, barFullHeight + 128f + stringSize.Y));
                    */
                }
            }

            private static void DrawBonusHealthBar(int colorIndex, int healthAmount, Vector2 topOfBar, int barFullHeight)
            {
                int height = (int)((float)healthAmount / (float)Game1.player.maxHealth * (float)barFullHeight);
                Rectangle health_bar_rect = new Microsoft.Xna.Framework.Rectangle((int)topOfBar.X + 12, (int)topOfBar.Y + 48 + barFullHeight - height, 24, height);
                Game1.spriteBatch.Draw(Game1.staminaRect, health_bar_rect, Game1.staminaRect.Bounds, healthTiers[colorIndex % healthTiers.Count], 0f, Vector2.Zero, SpriteEffects.None, 0f);
            }
        }

        public static class MeleeWeaponPatches
        {
        //Add to weapon swipe
        [HarmonyPatch(typeof(MeleeWeapon), "doSwipe")]
        [HarmonyPatch(new Type[] { typeof(int), typeof(Vector2),typeof(int),typeof(float),typeof(Farmer) })]
        public class SwipePatcher
        {
            public static void Postfix(MeleeWeapon __instance, int type, Vector2 position, int facingDirection, float swipeSpeed, Farmer f)
            {
                if (type == 429)
                {
                    switch (f.FacingDirection)
                    {
                        case 0:
                            ((FarmerSprite)f.Sprite).animateOnce(248, swipeSpeed, 6);
                            __instance.Update(0, 0, f);
                            break;
                        case 1:
                            ((FarmerSprite)f.Sprite).animateOnce(240, swipeSpeed, 6);
                            __instance.Update(1, 0, f);
                            break;
                        case 2:
                            ((FarmerSprite)f.Sprite).animateOnce(232, swipeSpeed, 6);
                            __instance.Update(2, 0, f);
                            break;
                        case 3:
                            ((FarmerSprite)f.Sprite).animateOnce(256, swipeSpeed, 6);
                            __instance.Update(3, 0, f);
                            break;
                    }
                    if (__instance.PlayUseSounds)
                    {
                        f.playNearbySoundLocal("clubswipe");
                    }
                }
            }
        }
        
        [HarmonyPatch(typeof(MeleeWeapon), "FireProjectile")]
        [HarmonyPatch(new Type[] { typeof(Farmer) })]
        public class FireProjectilePatcher
        {
            public static void Prefix(MeleeWeapon __instance, Farmer who)
            {
                if(__instance.type.Value == 429)
                {
                    if (ModAssets.localFarmerData.selectedSpellID != -1 &&
                        ModAssets.modSpells[ModAssets.localFarmerData.selectedSpellID].GetType() == typeof(CombatSpell) && ModAssets.HasMagic(Game1.player))
                    {
                        CombatSpell spell = (CombatSpell)ModAssets.modSpells[ModAssets.localFarmerData.selectedSpellID];
                        Point mousePos = Game1.getMousePosition();
                        int mouseX = mousePos.X + Game1.viewport.X;
                        int mouseY = mousePos.Y + Game1.viewport.Y;
                        
                        var cachedDataField = Traverse.Create(__instance).Field("cachedData");
                        var cachedData = cachedDataField.GetValue();

                        if (cachedData is not StaffWeaponData staffWepData)
                        {
                            return;
                        }
                        
                        SpellResponse castReturn = spell.CreateCombatProjectile(who, staffWepData, mouseX, mouseY, out List<MagicProjectile> generatedProjectiles);

                        if (castReturn.wasSpellSuccessful && generatedProjectiles.Count > 0)
                        {
                            foreach (MagicProjectile projectile in generatedProjectiles)
                            {
                                who.currentLocation.projectiles.Add(projectile);
                            }
                        }
                        else
                        {
                            Game1.showRedMessage(castReturn.translatedResponse);
                        }
                    }
                    else
                    {
                        Game1.showRedMessage(KeyTranslator.GetTranslation(ModAssets.HasMagic(Game1.player) ? "ui.NoSpellSelected.text" : "ui.NoBattlestaffKnowledge.text"));
                    }
                }
                
            }
        }
        
        [HarmonyPatch(typeof(MeleeWeapon), "getExtraSpaceNeededForTooltipSpecialIcons")]
        [HarmonyPatch(new Type[] { typeof(SpriteFont), typeof(int), typeof(int), typeof(int), typeof(StringBuilder), typeof(string), typeof(int)  })]
        public class SpacePatcher
        {
            public static void Postfix(ref Point __result,MeleeWeapon __instance, SpriteFont font, int minWidth, int horizontalBuffer, int startingHeight, StringBuilder descriptionText, string boldTitleText, int moneyAmountToDisplayAtBottom)
            {
                if (__instance.type.Value == 429) //Staff type
                {
                    StaffWeaponData staffWeaponData = (StaffWeaponData)Traverse.Create(__instance).Field("cachedData").GetValue();
                    bool staffProvidesRune = staffWeaponData.providesRune != "";
                    __result.Y += (staffWeaponData.projectileDamageModifier > 1.0f ? 48 : 0) + (staffProvidesRune ? 48 : 0); //Add to the size of the extra space to allow for extra staff damage symbol
                }
            }
        }
        
        [HarmonyPatch(typeof(MeleeWeapon), "getCategoryName")]
        public class CategoryNamePatcher
        {
            public static void Postfix(ref string __result,MeleeWeapon __instance)
            {
                if (__instance.type.Value == 429) //Staff type
                {
                    StaffWeaponData staffWeaponData = (StaffWeaponData)Traverse.Create(__instance).Field("cachedData").GetValue();

                    __result = KeyTranslator.GetTranslation("ui.BattlestaffDescription.text",
                        new { StaffLevel = staffWeaponData.level });
                }
            }
        }

        [HarmonyPatch]
        public class MeleeTooltipPatcher
        {
            private static MethodBase TargetMethod()
            {
                // Get all methods in MeleeWeapon
                var methods = typeof(MeleeWeapon).GetMethods(BindingFlags.Public | BindingFlags.Instance);

                var method = methods.FirstOrDefault(x => x.Name == "drawTooltip");

                if (method == null)
                {
                    return null;
                }
                
                var parameters = method.GetParameters();
        
                //This is essential to allow us to use a ref type for the method
                if (parameters[0].ParameterType == typeof(SpriteBatch) &&
                    parameters[1].ParameterType == typeof(int).MakeByRefType() && 
                    parameters[2].ParameterType == typeof(int).MakeByRefType() &&
                    parameters[3].ParameterType == typeof(SpriteFont) &&
                    parameters[4].ParameterType == typeof(float) &&
                    parameters[5].ParameterType == typeof(StringBuilder))
                {
                    return method;
                }
                    
                return null;
            }
            public static void Postfix(MeleeWeapon __instance, SpriteBatch spriteBatch, ref int x, ref int y, SpriteFont font, float alpha, StringBuilder overrideText)
            {
                if (__instance.type.Value == 429) //Staff type
                {
                    StaffWeaponData staffWeaponData = (StaffWeaponData)Traverse.Create(__instance).Field("cachedData").GetValue();
                    Color c3 = Game1.textColor;
                    int extraSize = (int)Math.Max(font.MeasureString("TT").Y, 48f);

                    if (staffWeaponData.projectileDamageModifier > 1.0f)
                    {
                        Utility.drawWithShadow(spriteBatch, ModAssets.extraTextures,
                            new Vector2(x + 16 + 4, y + 16 + 4), new Rectangle(16, 0, 10, 10), Color.White, 0f,
                            Vector2.Zero, 4f, flipped: false, 1f);
                        Utility.drawTextWithShadow(spriteBatch, KeyTranslator.GetTranslation("ui.BattlestaffMultiplier.text", new {Multiplier = staffWeaponData.projectileDamageModifier}), font,
                            new Vector2(x + 16 + 52, y + 16 + 12), c3 * 0.9f * alpha);
                        y += extraSize;
                    }
                    
                    if (staffWeaponData.providesRune != "")
                    {
                        Utility.drawWithShadow(spriteBatch, ModAssets.extraTextures,
                            new Vector2(x + 16 + 4, y + 16 + 4), new Rectangle(26, 0, 10, 10), Color.White, 0f,
                            Vector2.Zero, 4f, flipped: false, 1f);
                        Utility.drawTextWithShadow(spriteBatch, KeyTranslator.GetTranslation("ui.BattlestaffRune.text", new {RuneName =  ModAssets.modItems[staffWeaponData.providesRune].DisplayName}), font,
                            new Vector2(x + 16 + 52, y + 16 + 12), c3 * 0.9f * alpha);
                        y += extraSize;
                    }
                }
            }
        }
            
        }
        
        //Setup Variables and farmerData 
        [HarmonyPatch(typeof(Farmer), "farmerInit")]
        public class SetupModVariablesPatcher
        {
            public static void Postfix(Farmer __instance)
            {
                if (!Context.IsMultiplayer || (Context.IsMultiplayer && __instance.IsLocalPlayer))
                {
                    ModAssets.SetupModDataKeys(__instance);
                    ModAssets.localFarmerData.Reset();
                }
            }
        }
        
        //Add monster items
        [HarmonyPatch(typeof(Monster), "parseMonsterInfo")]
        [HarmonyPatch(new Type[] { typeof(string) })]
        public class MonsterDropPatcher
        {
            public static void Postfix(Monster __instance, string name)
            {
                if (ModAssets.monsterDrops.TryGetValue(name, out var monsterDrops))
                {
                    foreach (ItemDrop item in monsterDrops)
                    {
                        if (Game1.random.NextDouble() <= item.chance)
                        {
                            for (int i = 0; i < item.amount; i++)
                            {
                                __instance.objectsToDrop.Add(item.itemID);
                            }
                        }
                    }
                }
            }
        }
        
        [HarmonyPatch(typeof(BuffManager), "IsApplied")]
        [HarmonyPatch(new Type[] { typeof(string) })]
        public class BuffIsAppliedPatcher
        {
            public static void Postfix(ref bool __result, BuffManager __instance, string id)
            {
                if (!__result && id == "24") //If we are searching for 24 - the monster musk bonus - and we do not find it, also check for dark lure buff
                {
                    __result = __instance.AppliedBuffIds.Contains("Tofu.RunescapeSpellbook_BuffDark");
                }
            }
        }
        
        [HarmonyPatch(typeof(Farmer), "OnItemReceived")]
        [HarmonyPatch(new Type[] { typeof(Item), typeof(int), typeof(Item),typeof(bool) })]
        public class FarmerItemRecievedPatcher
        {
            public static void Postfix(Farmer __instance,Item item, int countAdded, Item mergedIntoStack, bool hideHudNotification = false)
            {
                //If any mod items are picked up for the first time we play a special animation
                if (!__instance.hasOrWillReceiveMail("Tofu.RunescapeSpellbook_RunesFound") && ModAssets.modItems.TryGetValue(item.ItemId, out var modItem))
                {
                    if (modItem is PackObject || item.ItemId == "Tofu.RunescapeSpellbook_TreasureElemental" || item.ItemId == "Tofu.RunescapeSpellbook_TreasureCatalytic")
                    {
                        __instance.mailReceived.Add("Tofu.RunescapeSpellbook_RunesFound");
                        __instance.holdUpItemThenMessage(item, countAdded);
                    }
                }
            }
        }
        
        [HarmonyPatch(typeof(Farmer), "showReceiveNewItemMessage")]
        [HarmonyPatch(new Type[] { typeof(Farmer), typeof(Item), typeof(int) })]
        public class FarmerNewItemAnimPatcher
        {
            public static bool Prefix(Farmer __instance,Farmer who, Item item, int countAdded)
            {
                //If any mod items are picked up for the first time we play a special animation
                if (ModAssets.modItems.TryGetValue(item.ItemId, out var modItem))
                {
                    if (modItem is PackObject || item.ItemId == "Tofu.RunescapeSpellbook_TreasureElemental" || item.ItemId == "Tofu.RunescapeSpellbook_TreasureCatalytic")
                    {
                        Game1.drawObjectDialogue(new List<string>
                        {
                            KeyTranslator.GetTranslation("minievent.FindRunePack.text")
                        });
                        who.completelyStopAnimatingOrDoingAction();
                        return false;
                    }
                }

                return true;
            }
        }
        
        [HarmonyPatch(typeof(Monster), "findPlayerPriority")]
        [HarmonyPatch(new Type[] { typeof(Farmer) })]
        public class MonsterAIPatcher
        {
            public static void Postfix(ref double __result, Monster __instance, Farmer f)
            {
                if (f.hasBuff($"Tofu.RunescapeSpellbook_BuffDark")) //If we have dark lure, make the result extremely low for this player, so we can increase their priority
                {
                    __result *= 0.1;
                }
            }
        }
        
        //Certain Bat AI enemies do not assign drops via the usual method. This modification hardcodes some solutions to add in items for these enemies
        [HarmonyPatch(typeof(Bat), "getExtraDropItems")]
        public class HauntedSkullPatcher
        {
            public static void Postfix(ref List<Item> __result,Bat __instance)
            {
                if (__instance.hauntedSkull.Value && __instance.cursedDoll.Value)
                {
                    foreach (ItemDrop item in ModAssets.monsterDrops["Haunted Skull"])
                    {
                        if (Game1.random.NextDouble() <= item.chance)
                        {
                            for (int i = 0; i < item.amount; i++) //For some reason having single stacked items on the ground as monster drops just feels unnatural
                            {
                                StardewValley.Object generatedItem = ItemRegistry.Create<StardewValley.Object>($"{item.itemID}");
                                __result.Add(generatedItem);
                            }
                        }
                    }
                }
            }
        }
        
        [HarmonyPatch(typeof(Slingshot), "canThisBeAttached")]
        [HarmonyPatch(new Type[] { typeof(StardewValley.Object),typeof(int) })]
        public class SlingshotAttachmentPatcher
        {
            public static void Postfix(ref bool __result, StardewValley.Object o, int slot)
            {
                //update result to check if the item is either of the extra ammos
                __result = __result || ModAssets.modItems.Any(x=> o.ItemId == x.Key && x.Value is SlingshotItem);
            }
        }
        
        [HarmonyPatch(typeof(Slingshot), "GetAmmoDamage")]
        [HarmonyPatch(new Type[] { typeof(StardewValley.Object)})]
        public class SlingshotDamagePatcher
        {
            public static void Postfix(ref int __result, StardewValley.Object ammunition)
            {
                if (__result == 1 && ModAssets.modItems.TryGetValue(ammunition.ItemId, out ModLoadObjects objItem) && objItem is SlingshotItem ammo)
                {
                    __result += ammo.extraDamage;
                }
            }
        }
        
        [HarmonyPatch(typeof(Object), "GetCategoryDisplayName")]
        [HarmonyPatch(new Type[] { typeof(int)})]
        public class DisplayNamePatcher
        {
            public static void Postfix(ref string __result, int category)
            {
                switch (category)
                {
                    case -429:
                        __result = KeyTranslator.GetTranslation("ui.CategoryElemental.text");
                        break;
                    case -430:
                        __result = KeyTranslator.GetTranslation("ui.CategoryCombat.text");
                        break;
                    case -431:
                        __result = KeyTranslator.GetTranslation("ui.CategoryCatalytic.text");
                        break;
                } 
            }
        }
        
        [HarmonyPatch(typeof(Object), "GetCategoryColor")]
        [HarmonyPatch(new Type[] { typeof(int)})]
        public class ColourPatcher
        {
            public static void Postfix(ref Color __result, int category)
            {
                switch (category)
                {
                    case -429:
                        __result = new Color(124,149,101);
                        break;
                    case -430:
                        __result = new Color(135,92,17);
                        break;
                    case -431:
                        __result = new Color(114,34,28);
                        break;
                } 
            }
        }
        
        [HarmonyPatch(typeof(Slingshot), "GetAmmoCollisionBehavior")]
        [HarmonyPatch(new Type[] { typeof(Object)})]
        public class SlingshotExplodePatcher
        {
            public static void Postfix(Slingshot __instance, ref BasicProjectile.onCollisionBehavior __result, Object ammunition)
            {
                if (ModAssets.modItems.TryGetValue(ammunition.ItemId, out ModLoadObjects objItem) && objItem is SlingshotItem ammo && ammo.explodes)
                {
                    __result = BasicProjectile.explodeOnImpact;
                }
            }
        }
            
        [HarmonyPatch(typeof(Slingshot), "GetAmmoCollisionSound")]
        [HarmonyPatch(new Type[] { typeof(Object)})]
        public class SlingshotSoundPatcher
        {
            public static void Postfix(Slingshot __instance, ref string __result, Object ammunition)
            {
                if (ModAssets.modItems.TryGetValue(ammunition.ItemId, out ModLoadObjects objItem) && objItem is SlingshotItem ammo && ammo.explodes)
                {
                    __result = "explosion";
                }
            }
        }

        [HarmonyPatch(typeof(BasicProjectile), "behaviorOnCollisionWithMonster")]
        [HarmonyPatch(new Type[] { typeof(NPC), typeof(GameLocation) })]
        public class SlingshotProjectileMonsterHit
        {
            public static bool Prefix(BasicProjectile __instance, NPC n, GameLocation location)
            {
                if (__instance.itemId != null && (__instance.damagesMonsters.Value && n is Monster))
                {
                    Farmer player = __instance.GetPlayerWhoFiredMe(location);
                    
                    //BasicProjectile ItemIDs are Qualified (will always be (O) in this case) so if we skip the first 3 values we should get an unqualified ID 
                    if (ModAssets.modItems.TryGetValue(__instance.itemId.Value.Substring(3), out ModLoadObjects objItem) &&
                        objItem is SlingshotItem ammoItem)
                    {
                        HitMonster(__instance, n as Monster, location, player, ammoItem.debuffType);

                        if (ammoItem.explodes) //Earth Ammo
                        {
                            n.currentLocation.explode(new Vector2(n.Tile.X, n.Tile.Y), 2, player);
                        }

                        return false;
                    }
                }

                return true;
            }

            //These monsters do not allow for glowing effects, so it isn't possible to show the player the fact they're debuffed. Its easier to give them immunity
            private static void HitMonster(BasicProjectile __instance, Monster n, GameLocation location, Farmer player,
                int debuffType)
            {
                location.damageMonster(n.GetBoundingBox(), __instance.damageToFarmer.Value,
                    __instance.damageToFarmer.Value + 1, isBomb: false, player, isProjectile: true);

                foreach (NPC npcEffected in location.characters.Where(x =>
                             x is Monster mon && Vector2.Distance(n.Tile, x.Tile) < 5))
                {
                    Monster monsterEffected = npcEffected as Monster;

                    if (monsterEffected.mineMonster.Value)
                    {
                        monsterEffected.startGlowing(debuffType == 2 ? Color.Red : Color.Green, false, 0.05f);
                        
                        DelayedAction.functionAfterDelay(() => ApplyPoisonEffect(monsterEffected, debuffType,DEBUFFHITS), DEBUFFDELAY);
                    }

                    if (!n.IsInvisible)
                    {
                        __instance.piercesLeft.Value--;
                    }
                }
            }

            private static readonly int DEBUFFHITS = 5;
            private static readonly int DEBUFFDELAY = 3000;
            private static void ApplyPoisonEffect(Monster target, int debuffIndex, int hitsRemaining)
            {
                if (target.Health <= 1 || target.isInvincible())
                {
                    if (target.isGlowing)
                    {
                        target.stopGlowing();
                    }
                    return;
                }
                
                bool isFire = debuffIndex == 2;
                int hitDamage = isFire ? 30 : 50;
                int realDamage = (target.Health - hitDamage <= 0 ? target.Health - 1 : hitDamage) + target.resilience.Value;
                
                target.takeDamage(realDamage, 0, 0, false, 100, isFire ? "RunescapeSpellbook.FireBurn" : "RunescapeSpellbook.Liquid");
                
                Rectangle monsterBox = target.GetBoundingBox();
                target.currentLocation.debris.Add(new Debris(realDamage,
                    new Vector2(monsterBox.Center.X + 16, monsterBox.Center.Y),
                    isFire ? Color.Orange : Color.Purple,
                    1.25f, target));

                if (!target.isGlowing)
                {
                    target.startGlowing(isFire ? Color.Red : Color.Green, false, 0.05f);
                }
                
                if (hitsRemaining > 0 && target.Health > 1 && !target.isInvincible())
                {
                    DelayedAction.functionAfterDelay(() => ApplyPoisonEffect(target, debuffIndex,hitsRemaining - 1), DEBUFFDELAY);
                }
                else
                {
                    if (target.isGlowing)
                    {
                        target.stopGlowing();
                    }
                }
                
                
            }
        }

        [HarmonyPatch(typeof(TV), "checkForAction")]
            public class TVChannelTranspiler
            {
                static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
                {
                    try
                    {
                        var codes = new List<CodeInstruction>(instructions);

                        var toArrayMethod = AccessTools.Method(typeof(List<Response>), "ToArray");
                        var modifyChannelsMethod = AccessTools.Method(typeof(TVChannelTranspiler), "ModifyChannelsList");

                        if (toArrayMethod == null)
                        {
                            return instructions;
                        }

                        for (int i = 0; i < codes.Count; i++)
                        {
                            var current = codes[i];
                            var next = codes[i + 1];

                            if (next.opcode == OpCodes.Callvirt &&
                                next.operand is MethodInfo method &&
                                method == toArrayMethod &&
                                (current.opcode == OpCodes.Ldloc_S ||
                                 current.opcode == OpCodes.Ldloc_0 ||
                                 current.opcode == OpCodes.Ldloc_1 ||
                                 current.opcode == OpCodes.Ldloc_2 ||
                                 current.opcode == OpCodes.Ldloc_3 ||
                                 current.opcode == OpCodes.Ldloc))
                            {
                                var insertPoint = i + 1;
                            
                                var dupInstruction = new CodeInstruction(OpCodes.Dup);
                                var callInstruction = new CodeInstruction(OpCodes.Call, modifyChannelsMethod);
                            
                                if (codes[insertPoint].labels.Count > 0)
                                {
                                    dupInstruction.labels.AddRange(codes[insertPoint].labels);
                                    codes[insertPoint].labels.Clear();
                                }
                            
                                codes.Insert(insertPoint, dupInstruction);
                                codes.Insert(insertPoint + 1, callInstruction);
                                break;
                            }
                        }
                    
                        return codes;

                    }
                    catch (Exception e)
                    {
                        return instructions;
                    }
                }
                public static void ModifyChannelsList(List<Response> channels)
                {
                    foreach (LoadableTV addChannel in ModAssets.loadableText.Where(x =>
                                 x is LoadableTV tvChannel && tvChannel.day == Game1.dayOfMonth && tvChannel.season == Game1.season && Game1.year >= tvChannel.firstYear))
                    {
                        string channelName = addChannel.channelName + (Game1.year == addChannel.firstYear ? "" : KeyTranslator.GetTranslation("ui.Rerun.text"));
                        channels.Insert(channels.Count - 1, new Response($"RS_{addChannel.id}", channelName));
                    }
                }

                [HarmonyPatch(typeof(TV), "selectChannel")]
                [HarmonyPatch(new Type[] { typeof(Farmer), typeof(string)})]
                public class ChannelSelectPatcher
                {
                    public static void Postfix(TV __instance, Farmer who, string answer)
                    {
                        var currentChannelTraverse = Traverse.Create(__instance).Field("currentChannel");
                        var currentChannelValue = currentChannelTraverse.GetValue<int>();
                    
                        if (currentChannelValue == 0)
                        {
                            LoadableText? modChannel = ModAssets.loadableText.Find(x => x is LoadableTV tvChannel && $"RS_{x.id}" == answer && tvChannel.day == Game1.dayOfMonth && tvChannel.season == Game1.season);

                            if (modChannel != null)
                            {
                                currentChannelTraverse.SetValue(int.Parse(modChannel.id));
                            
                                var screenTraverse = Traverse.Create(__instance).Field("screen");
                                screenTraverse.SetValue(new TemporaryAnimatedSprite("Mods.RunescapeSpellbook.Assets.spellanimations", new Rectangle(64, 0, 42, 28), 3000f, 2, 999999, __instance.getScreenPosition(), flicker: false, flipped: false, (float)(__instance.boundingBox.Bottom - 1) / 10000f + 1E-05f, 0f, Color.White, __instance.getScreenSizeModifier(), 0f, 0f, 0f));
                                Game1.drawObjectDialogue(modChannel.contents[0]);
                                Game1.afterDialogues = __instance.proceedToNextScene;
                            }

                        }
                    }
                }

                [HarmonyPatch(typeof(TV), "proceedToNextScene")]
                public class ChannelNextDialoguePatcher
                {
                    private static int lastIndex = 0;
                    public static void Postfix(TV __instance)
                    {
                        var currentChannelTraverse = Traverse.Create(__instance).Field("currentChannel");
                        var currentChannelValue = currentChannelTraverse.GetValue<int>();
                    
                        LoadableText? modChannel = ModAssets.loadableText.Find(x => x is LoadableTV tvChannel && int.Parse(x.id) == currentChannelValue && tvChannel.day == Game1.dayOfMonth && tvChannel.season == Game1.season);
                        if (modChannel != null)
                        {
                            int maxDialogue = modChannel.contents.Count - 1;

                            if (maxDialogue == lastIndex)
                            {
                                __instance.turnOffTV();
                                lastIndex = 0;
                            }
                            else
                            {
                                lastIndex++;
                                Game1.drawObjectDialogue(modChannel.contents[lastIndex]);
                                Game1.afterDialogues = __instance.proceedToNextScene;
                            }
                        }
                    }
                }
            }

            //Console Commands
            private bool HasNoMagic()
            {
                if (!ModAssets.HasMagic(Game1.player))
                {
                    this.Monitor.Log(KeyTranslator.GetTranslation("log.NoMagic.text"),LogLevel.Warn);
                    return true;
                }

                return false;
            }
        
            private bool HasNoWorldContextReady()
            {
                if (!Context.IsWorldReady)
                {
                    this.Monitor.Log(KeyTranslator.GetTranslation("log.NoWorld.text"),LogLevel.Warn);
                    return true;
                }

                return false;
            }

            private void GrantMagic(string command, string[] args)
            {
                if(HasNoWorldContextReady()){return;}
            
                if (ModAssets.HasMagic(Game1.player))
                {
                    this.Monitor.Log(KeyTranslator.GetTranslation("log.AlreadyHaveMagic.text"),LogLevel.Warn);
                    return;
                }
                Game1.player.eventsSeen.Add("Tofu.RunescapeSpellbook_Event0");
                Monitor.Log(KeyTranslator.GetTranslation("log.AddedMagic.text"),LogLevel.Info);
                if (args.Length > 0 && int.TryParse(args[0], out int reqLevel))
                {
                    reqLevel = Math.Clamp(reqLevel, 0, 10);
                    ModAssets.TrySetModVariable(Game1.player,"Tofu.RunescapeSpellbook_MagicLevel",reqLevel.ToString());
                    ModAssets.TrySetModVariable(Game1.player,"Tofu.RunescapeSpellbook_MagicExp",(Farmer.getBaseExperienceForLevel(reqLevel)).ToString());
                    this.Monitor.Log(KeyTranslator.GetTranslation("log.SetMagicLevel.text", new {RequestedLevel = reqLevel}),LogLevel.Info);
                }
            }
    
            private void SetLevel(string command, string[] args)
            {
                if (HasNoWorldContextReady() || HasNoMagic()){return;}

                if (args.Length == 0)
                {
                    this.Monitor.Log(KeyTranslator.GetTranslation("log.SpecifyMagicLevel.text"),LogLevel.Error);
                    return;
                }
            
                if (int.TryParse(args[0], out int reqLevel))
                {
                    reqLevel = Math.Clamp(reqLevel, 0, 10);
                    ModAssets.TrySetModVariable(Game1.player, "Tofu.RunescapeSpellbook_MagicLevel", (reqLevel).ToString());
                    ModAssets.TrySetModVariable(Game1.player, "Tofu.RunescapeSpellbook_MagicExp", (Farmer.getBaseExperienceForLevel(reqLevel)).ToString());
                    this.Monitor.Log(KeyTranslator.GetTranslation("log.SetMagicLevel.text", new {RequestedLevel = reqLevel}),LogLevel.Info);
                }
            }
        
            private void SetExp(string command, string[] args)
            {
                if (HasNoWorldContextReady() || HasNoMagic()){return;}

                if (args.Length == 0)
                {
                    this.Monitor.Log(KeyTranslator.GetTranslation("log.SpecifyExpSet.text"),LogLevel.Error);
                    return;
                }
            
                if (double.TryParse(args[0], out double reqExp))
                {
                    reqExp = Math.Clamp(reqExp, 0, Farmer.getBaseExperienceForLevel(10));
                    ModAssets.TrySetModVariable(Game1.player, "Tofu.RunescapeSpellbook_MagicLevel", "0");
                    ModAssets.TrySetModVariable(Game1.player, "Tofu.RunescapeSpellbook_MagicExp", "0");
                    ModAssets.IncrementMagicExperience(Game1.player, reqExp,false);
                    this.Monitor.Log(KeyTranslator.GetTranslation("log.SetExperienceLevel.text", new {RequestedExp = reqExp}),LogLevel.Info);
                }
            }
        
            private void AddExp(string command, string[] args)
            {
                if (HasNoWorldContextReady() || HasNoMagic()){return;}

                if (args.Length == 0)
                {
                    this.Monitor.Log(KeyTranslator.GetTranslation("log.SpecifyExpAdd.text"),LogLevel.Error);
                    return;
                }
            
                if (double.TryParse(args[0], out double reqAddExp))
                {
                    reqAddExp = Math.Clamp(reqAddExp, 0, Farmer.getBaseExperienceForLevel(10) - ModAssets.GetFarmerExperience(Game1.player));
                    ModAssets.IncrementMagicExperience(Game1.player, reqAddExp,false);
                    this.Monitor.Log(KeyTranslator.GetTranslation("log.AddExperienceLevel.text", new {RequestedExp = reqAddExp}),LogLevel.Info);
                }
            }
        
            private void ResetPerks(string command, string[] args)
            {
                if (HasNoWorldContextReady() || HasNoMagic()){return;}
            
                ModAssets.TrySetModVariable(Game1.player,"Tofu.RunescapeSpellbook_MagicProf1","-1");
                ModAssets.TrySetModVariable(Game1.player,"Tofu.RunescapeSpellbook_MagicProf2","-1");
                this.Monitor.Log(KeyTranslator.GetTranslation("log.RemovePerks.text"),LogLevel.Info);
            }
        
            private void PlayerInfo(string command, string[] args)
            {
                if (HasNoWorldContextReady()){return;}

                foreach (Farmer farmerRoot in ModAssets.GetFarmers())
                {
                    Monitor.Log(KeyTranslator.GetTranslation("log.PlayerInfo.text-name", new {Value = farmerRoot.Name}),LogLevel.Info);
                    Monitor.Log(KeyTranslator.GetTranslation("log.PlayerInfo.text-hasmagic", new {Value = ModAssets.HasMagic(farmerRoot)}),LogLevel.Info);
                    Monitor.Log(KeyTranslator.GetTranslation("log.PlayerInfo.text-level", new {Value = ModAssets.GetFarmerMagicLevel(farmerRoot)}),LogLevel.Info);
                    Monitor.Log(KeyTranslator.GetTranslation("log.PlayerInfo.text-exp", new {Value = ModAssets.TryGetModVariable(farmerRoot,"Tofu.RunescapeSpellbook_MagicExp")}),LogLevel.Info);
                    Monitor.Log(KeyTranslator.GetTranslation("log.PlayerInfo.text-multiplier",new {Value = ModAssets.TryGetModVariable(farmerRoot,"Tofu.RunescapeSpellbook_Setting-MagicExpMultiplier")}),LogLevel.Info);
                    
                    List<int> perkIDs = ModAssets.PerksAssigned(farmerRoot);
                    int perkIndex = 1;
                    foreach (int id in perkIDs)
                    {
                        string perkName = id == -1 ? KeyTranslator.GetTranslation("log.PlayerInfo.perk-default") : ModAssets.perks.Where(x=>x.perkID==id).Select(x=>x.perkName).First();
                        Monitor.Log(KeyTranslator.GetTranslation("log.PlayerInfo.text-perk",new {PerkIndex = perkIndex,PerkName = perkName}),LogLevel.Info);
                    }
                }
            }
            private void GrantRunes(string command, string[] args)
            {
                if(HasNoWorldContextReady()){return;}
            
                string runeReq = args.Length == 0 ? "default" : args[0].ToLower();

                List<int> runeReqs;
            
                if (runeReq == "default")
                {
                    runeReqs = new List<int>() { -429, -431, -430 };
                }
                else if (runeReq == "elemental" || runeReq == "elem")
                {
                    runeReqs = new List<int>() { -429 };
                }
                else if (runeReq == "catalytic" || runeReq == "cat" || runeReq == "cata")
                {
                    runeReqs = new List<int>() { -431, -430 };
                }
                else if (runeReq == "teleport" || runeReq == "tele")
                {
                    runeReqs = new List<int>() { -429};
                
                    StardewValley.Object item = ItemRegistry.Create<StardewValley.Object>($"(O)Tofu.RunescapeSpellbook_RuneLaw");
                    item.Stack = 255;
                    Game1.player.addItemToInventory(item);
                }
                else if (runeReq == "utility" || runeReq == "util")
                {
                    runeReqs = new List<int>() { -429,-431};
                }
                else if (runeReq == "combat" || runeReq == "comb")
                {
                    runeReqs = new List<int>() { -429,-430};
                }
                else if (runeReq == "combat2" || runeReq == "com2" || runeReq == "comb2")
                {
                    runeReqs = new List<int>() { -429,-430};
                
                    StardewValley.Object cosmRune = ItemRegistry.Create<StardewValley.Object>($"(O)Tofu.RunescapeSpellbook_RuneCosmic");
                    cosmRune.Stack = 255;
                    Game1.player.addItemToInventory(cosmRune);
                
                    StardewValley.Object astRune = ItemRegistry.Create<StardewValley.Object>($"(O)Tofu.RunescapeSpellbook_RuneAstral");
                    astRune.Stack = 255;
                    Game1.player.addItemToInventory(astRune);
                }
                else
                {
                    List<ModLoadObjects> matchList = ModAssets.modItems.Where(x=>x.Value is RunesObjects && x.Value.DisplayName.ToLower().Contains(runeReq)).Select(y=>y.Value).ToList();
                    if (matchList.Count == 0)
                    {
                        this.Monitor.Log(KeyTranslator.GetTranslation("log.GrantRune.text-fail", new {RuneReq = runeReq}),LogLevel.Error);
                        return;
                    }
                    else
                    {
                        StardewValley.Object item = ItemRegistry.Create<StardewValley.Object>($"{matchList[0].id}");
                        item.Stack = 255;
                        Game1.player.addItemToInventory(item);
                    }
                    return;
                
                }
                foreach (string id in ModAssets.modItems.Where(x => runeReqs.Contains(x.Value.Category)).Select(y=>y.Key))
                {
                    StardewValley.Object item = ItemRegistry.Create<StardewValley.Object>($"(O){id}");
                    item.Stack = 255;
                    Game1.player.addItemToInventory(item);
                }
            
                this.Monitor.Log(KeyTranslator.GetTranslation("log.GrantRune.text-works", new {RuneReq = runeReq}),LogLevel.Info);
            }
        
            private void GrantAmmo(string command, string[] args)
            {
                if (HasNoWorldContextReady()){return;}
            
                foreach (ModLoadObjects foundItem in ModAssets.modItems.Where(x=>x.Value is SlingshotItem).Select(y=>y.Value))
                {
                    StardewValley.Object item = ItemRegistry.Create<StardewValley.Object>($"{foundItem.id}");
                    item.Stack = 255;
                    Game1.player.addItemToInventory(item);
                }
            
                this.Monitor.Log(KeyTranslator.GetTranslation("log.GrantedAmmo.text"),LogLevel.Info);
            }
            private void GrantStaffs(string command, string[] args)
            {
                if (HasNoWorldContextReady()){return;}

                foreach (StaffWeaponData newWeapon in ModAssets.staffWeapons)
                {
                    MeleeWeapon item = ItemRegistry.Create<MeleeWeapon>(newWeapon.id);
                    Game1.player.addItemToInventory(item);
                }
            
                this.Monitor.Log(KeyTranslator.GetTranslation("log.GrantedWeps.text"),LogLevel.Info);
            }
        
            private void GrantTreasures(string command, string[] args)
            {
                if (HasNoWorldContextReady()){return;}
            
                foreach (ModLoadObjects foundItem in ModAssets.modItems.Where(x=>x.Value is TreasureObjects and not PackObject).Select(y=>y.Value))
                {
                    StardewValley.Object item = ItemRegistry.Create<StardewValley.Object>($"{foundItem.id}");
                    item.Stack = 20;
                    Game1.player.addItemToInventory(item);
                }
            
                this.Monitor.Log(KeyTranslator.GetTranslation("log.GrantedTreasures.text"),LogLevel.Info);
            }
        
            private void DebugCommand(string command, string[] args)
            {
                if (HasNoWorldContextReady()){return;}
            }
        
            private void DebugPosition(string command, string[] args)
            {
                if (HasNoWorldContextReady()){return;}
            
                Farmer player = Game1.player;
                Monitor.Log($"{player.currentLocation.Name}\n x: {player.Tile.X}\n y: {player.Tile.Y}",LogLevel.Info);
            }
        
            private void GrantPacks(string command, string[] args)
            {
                if (HasNoWorldContextReady()){return;}
            
                foreach (ModLoadObjects foundItem in ModAssets.modItems.Where(x=>x.Value is PackObject).Select(y=>y.Value))
                {
                    StardewValley.Object item = ItemRegistry.Create<StardewValley.Object>($"{foundItem.id}");
                    item.Stack = 20;
                    Game1.player.addItemToInventory(item);
                }
            
                this.Monitor.Log(KeyTranslator.GetTranslation("log.GrantPacks.text"),LogLevel.Info);
            }
        
            private void GrantFish(string command, string[] args)
            {
                if (HasNoWorldContextReady()){return;}
            
                foreach (ModLoadObjects foundItem in ModAssets.modItems.Where(x=>x.Value is FishObject).Select(y=>y.Value))
                {
                    StardewValley.Object item = ItemRegistry.Create<StardewValley.Object>($"{foundItem.id}");
                    item.Stack = 20;
                    Game1.player.addItemToInventory(item);
                }
            
                this.Monitor.Log(KeyTranslator.GetTranslation("log.GrantFish.text"),LogLevel.Info);
            }
            
            private void GrantSeeds(string command, string[] args)
            {
                if (HasNoWorldContextReady()){return;}
            
                foreach (ModLoadObjects foundItem in ModAssets.modItems.Where(x=>x.Value is SeedObject).Select(y=>y.Value))
                {
                    StardewValley.Object item = ItemRegistry.Create<StardewValley.Object>($"{foundItem.id}");
                    item.Stack = 20;
                    Game1.player.addItemToInventory(item);
                }
            
                this.Monitor.Log(KeyTranslator.GetTranslation("log.GrantSeeds.text"),LogLevel.Info);
            }
            
            private void GrantCrops(string command, string[] args)
            {
                if (HasNoWorldContextReady()){return;}
            
                foreach (ModLoadObjects foundItem in ModAssets.modItems.Where(x=>x.Value is CropObject).Select(y=>y.Value))
                {
                    StardewValley.Object item = ItemRegistry.Create<StardewValley.Object>($"{foundItem.id}");
                    item.Stack = 20;
                    Game1.player.addItemToInventory(item);
                }
            
                this.Monitor.Log(KeyTranslator.GetTranslation("log.GrantCrops.text"),LogLevel.Info);
            }
            
            private void GrantPotions(string command, string[] args)
            {
                if (HasNoWorldContextReady()){return;}
            
                foreach (ModLoadObjects foundItem in ModAssets.modItems.Where(x=>x.Value is PotionObject).Select(y=>y.Value))
                {
                    StardewValley.Object item = ItemRegistry.Create<StardewValley.Object>($"{foundItem.id}");
                    item.Stack = 20;
                    Game1.player.addItemToInventory(item);
                }
            
                this.Monitor.Log(KeyTranslator.GetTranslation("log.GrantPots.text"),LogLevel.Info);
            }
    }
}