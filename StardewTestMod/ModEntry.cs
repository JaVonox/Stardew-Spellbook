using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.GameData.Objects;
using StardewValley.GameData.Weapons;

using StardewValley.Menus;
using StardewValley.Tools;

namespace StardewTestMod
{
    internal sealed class ModEntry : Mod
    {
        public static ModEntry Instance;
        public static IMonitor ModMonitor { get; private set; }
        
        private const string CustomTextureKey = "Mods.StardewTestMod.Assets.modsprites";
        private static string MESSAGE_SYNC_PLAYER_LEVEL = "SYNC_PLAYER_LEVEL";
        public override void Entry(IModHelper helper)
        {
            Instance = this;
            
            ModMonitor = this.Monitor;
            var harmony = new Harmony(this.ModManifest.UniqueID);
            harmony.PatchAll();

            ModAssets.Load(helper);
            
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.Content.AssetRequested += this.OnAssetRequested;
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            
            Monitor.Log("Successfully registered StaffWeapon type for XML serialization", LogLevel.Info);
        }
            
        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            
            if (e.NameWithoutLocale.IsEquivalentTo(CustomTextureKey))
            {
                Monitor.Log("LoadedCustom", LogLevel.Warn);
                e.LoadFromModFile<Texture2D>("Assets/itemsprites", AssetLoadPriority.Medium);
            }

            if (e.NameWithoutLocale.IsEquivalentTo("Data/Objects"))
            {
                e.Edit(asset =>
                    {
                        var objectDict = asset.AsDictionary<string, ObjectData>().Data;

                        foreach (ModLoadObjects newObject in ModAssets.modItems)
                        {
                            newObject.AppendObject(CustomTextureKey, objectDict);
                        }
                        
                        /*
                        StaffWeapon tmp = new StaffWeapon();
                        tmp.Name = "battlestaff";
                        tmp.description = "a battlestaff";
                        tmp.type =
                        */

                        /*
                         ObjectData newItem = new ObjectData();
                        newItem.Name = this.name;
                        newItem.DisplayName = this.displayName;
                        newItem.Description = this.description;
                        newItem.Type = this.type;
                        newItem.Texture = CustomTextureKey;
                        newItem.SpriteIndex = this.spriteIndex;
                        newItem.Category = this.category;
                        ObjectsSet[$"{id}"] = newItem;
                         */

                    }
                );
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

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady)
            {
                return;
            }

            if (e.Button == SButton.F5)
            {
                for (int i = 4290; i < 4301; i++)
                {
                    StardewValley.Object item = ItemRegistry.Create<StardewValley.Object>($"{i}");
                    item.stack.Value = 20;
                    Game1.player.addItemToInventory(item);
                }
            }

            if (e.Button == SButton.F6)
            {
                foreach (StaffWeaponData newWeapon in ModAssets.staffWeapons)
                {
                    MeleeWeapon item = ItemRegistry.Create<MeleeWeapon>(newWeapon.id);
                    Game1.player.addItemToInventory(item);
                }
            }
            
            
            if (e.Button == SButton.F7)
            {
                Monitor.Log($"Loca x {Game1.player.Tile.X}, y {Game1.player.Tile.Y}" , LogLevel.Warn);
                Monitor.Log($"Current shared player level: {ModAssets.farmerSharedData.playerLevel.Value}", LogLevel.Info);
                if (Context.IsMainPlayer)
                {
                    // Host can update the value
                    int newLevel = Game1.random.Next(1, 1500);
                    ModAssets.farmerSharedData.playerLevel.Value = newLevel;
            
                    // Broadcast the change
                    Helper.Multiplayer.SendMessage(
                        newLevel,
                        MESSAGE_SYNC_PLAYER_LEVEL,
                        modIDs: new[] { ModManifest.UniqueID },
                        playerIDs: null // Send to all players
                    );
            
                    Monitor.Log($"Changed player level to: {newLevel}", LogLevel.Info);
                    Game1.addHUDMessage(new HUDMessage($"Set Level: {newLevel}", HUDMessage.newQuest_type));
                }
                else
                {
                    // Non-host players just show the current value
                    Game1.addHUDMessage(new HUDMessage($"Current Level: {ModAssets.farmerSharedData.playerLevel.Value}", HUDMessage.newQuest_type));
                }
            }

            if (e.Button == SButton.F8)
            {
                ModAssets.farmerSharedData.playerLevel.Value += 10;
                
                Instance.Helper.Multiplayer.SendMessage(
                    ModAssets.farmerSharedData.playerLevel.Value,
                    MESSAGE_SYNC_PLAYER_LEVEL,
                    modIDs: new[] { this.ModManifest.UniqueID },
                    playerIDs: null // Send to all players
                );
            }
        }
        
        //Multiplayer
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // We'll use Game1's netWorldState to store our shared data
            Instance.Helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
        }
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            Instance.Helper.Events.Multiplayer.ModMessageReceived += OnModMessageReceived;
    
            // If host, set initial value and broadcast it
            if (Context.IsMainPlayer)
            {
                // Initialize with new random value if not already set
                if (ModAssets.farmerSharedData.playerLevel.Value == 0)
                {
                    ModAssets.farmerSharedData.playerLevel.Value = Game1.random.Next(1, 1500);
                }

                // Broadcast the current value to all players
                Instance.Helper.Multiplayer.SendMessage(
                    ModAssets.farmerSharedData.playerLevel.Value,
                    MESSAGE_SYNC_PLAYER_LEVEL,
                    modIDs: new[] { this.ModManifest.UniqueID },
                    playerIDs: null // Send to all players
                );

                Monitor.Log($"Host initialized player level to: {ModAssets.farmerSharedData.playerLevel.Value}",
                    LogLevel.Info);
            }
        }
    
        private void OnModMessageReceived(object sender, ModMessageReceivedEventArgs e)
        {
            if (e.FromModID == ModManifest.UniqueID)
            {
                if (e.Type == MESSAGE_SYNC_PLAYER_LEVEL)
                {
                    int newLevel = e.ReadAs<int>();
                    
                    ModAssets.farmerSharedData.playerLevel.Value = newLevel;
        
                    Monitor.Log($"Received player level update: {newLevel}", LogLevel.Info);
                }
            }
        }
        
        //Add menu item to getTabNumberFromName
        [HarmonyPatch(typeof(GameMenu), "getTabNumberFromName")]
        public class GameMenuTabNumberPatch
        {
            public static bool Prefix(GameMenu __instance, string name, ref int __result)
            {
                if (name == "modtest")
                {
                    __result = 10;
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
                __instance.pages.Add(new SpellbookPage(__instance.xPositionOnScreen, __instance.yPositionOnScreen, __instance.width - 64 - 16, __instance.height));
                __instance.tabs.Add(new ClickableComponent(new Rectangle(__instance.xPositionOnScreen + 704, __instance.yPositionOnScreen + IClickableMenu.tabYPositionRelativeToMenuY + 64, 64, 64), "modtest", "Spellbook")
                {
                    myID = 12350,
                    downNeighborID = 10,
                    leftNeighborID = 12348,
                    tryDefaultIfNoDownNeighborExists = true,
                    fullyImmutable = true
                });
            }
        }

        [HarmonyPatch(typeof(GameMenu), "draw")]
        [HarmonyPatch(new Type[] { typeof(SpriteBatch) })]
        public class GameMenuDrawPatch
        {
            private const int newMenuIndex = 0;
            public static void Postfix(GameMenu __instance, SpriteBatch b)
            {
                if(!__instance.invisible)
                {
                    ClickableComponent c = __instance.tabs.Where(x => x.name == "modtest").First();
                    b.Draw(ModAssets.extraTextures, new Vector2(c.bounds.X, c.bounds.Y + ((__instance.currentTab == __instance.getTabNumberFromName(c.name)) ? 8 : 0)), new Rectangle(newMenuIndex * 16, 0, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0001f);
                    DuplicateDraws(__instance, b);
                }
            }

            /*
             * Some duplicate methods to prevent the mouse and hover text from appearing under it
             * This'd probably be better to use a transpiler for but it seems very complicated
             */
            private static void DuplicateDraws(GameMenu __instance, SpriteBatch b)
            {
                if (!__instance.hoverText.Equals(""))
                {
                    IClickableMenu.drawHoverText(b, __instance.hoverText, Game1.smallFont, 0, 0, -1, null, -1, null, null, 0, null, -1, -1, -1, 1f, null, null, null, null, null, null);
                }
                __instance.drawMouse(b, ignore_transparency: true);
            }
        }
        
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
                        f.playNearbySoundLocal("clubswipe", null);
                    }
                }
            }
        }
        
        //Add to weapon swipe
        [HarmonyPatch(typeof(MeleeWeapon), "FireProjectile")]
        [HarmonyPatch(new Type[] { typeof(Farmer) })]
        public class FireProjectilePatcher
        {
            public static void Prefix(MeleeWeapon __instance, Farmer who)
            {
                if(__instance.type.Value == 429)
                {
                    if (ModAssets.localFarmerData.selectedSpellID != -1 &&
                        ModAssets.modSpells[ModAssets.localFarmerData.selectedSpellID].GetType() == typeof(CombatSpell))
                    {
                        CombatSpell spell = (CombatSpell)ModAssets.modSpells[ModAssets.localFarmerData.selectedSpellID];
                        Point mousePos = Game1.getMousePosition();
                        int mouseX = mousePos.X + Game1.viewport.X;
                        int mouseY = mousePos.Y + Game1.viewport.Y;

                        MagicProjectile? generatedProjectile;
                        StaffWeaponData staffWeaponData = (StaffWeaponData)Traverse.Create(__instance).Field("cachedData").GetValue();
                        KeyValuePair<bool, string> castReturn = spell.CreateCombatProjectile(who, staffWeaponData, mouseX, mouseY, out generatedProjectile);

                        if (castReturn.Key && generatedProjectile != null)
                        {
                            who.currentLocation.projectiles.Add(generatedProjectile);
                        }
                        else
                        {
                            Game1.showRedMessage(castReturn.Value, true);
                        }
                    }
                    else
                    {
                        Game1.showRedMessage("No Selected Spell", true);
                    }
                }
                
            }
        }
        
        //this patch adds to the first post load - so it maintains between days but if you save and reload it will reset
        [HarmonyPatch(typeof(Game1), "_update")]
        [HarmonyPatch(new Type[] {typeof(GameTime)})]
        public class FirstFrameAfterLoadPatcher
        {
            public static void Postfix(Game1 __instance, GameTime gameTime)
            {
                if (Game1.gameMode == 3 && Game1.gameModeTicks == 1)
                {
                    ModAssets.localFarmerData.FirstGameTick();
            
                    // If host and connected, broadcast the current level
                    if (Context.IsMultiplayer && Context.IsMainPlayer && Context.IsWorldReady)
                    {
                        ModEntry.Instance.Helper.Multiplayer.SendMessage(
                            ModAssets.farmerSharedData.playerLevel.Value,
                            ModEntry.MESSAGE_SYNC_PLAYER_LEVEL,
                            modIDs: new[] { ModEntry.Instance.ModManifest.UniqueID }
                        );
                
                        ModEntry.ModMonitor.Log($"Initial player level sync: {ModAssets.farmerSharedData.playerLevel.Value}", LogLevel.Info);
                    }
                    
                }
            }
        }
        
        
        //TODO farmerID -> new FarmerDataClass dictionary
        //TODO stores magic level on server, exp gained locally. at night we send the exp gained to the host (maybe also with disconnect?)
        //TODO host updates exp + level for all people at night. 
    }
}