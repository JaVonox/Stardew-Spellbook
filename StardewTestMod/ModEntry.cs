using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.GameData.Machines;
using StardewValley.GameData.Objects;
using StardewValley.Menus;

namespace StardewTestMod
{
    internal sealed class ModEntry : Mod
    {
        public static ModEntry Instance;
        public static IMonitor ModMonitor { get; private set; }
        
        private const string CustomTextureKey = "Mods.StardewTestMod.Assets.ModSprites";
        
        public override void Entry(IModHelper helper)
        {
            Instance = this;
            ModMonitor = this.Monitor;
            var harmony = new Harmony(this.ModManifest.UniqueID);
            harmony.PatchAll();

            ModAssets.Load(helper);
            
            var customStrings = helper.Translation.Get("GameMenu_ModTest");
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.Content.AssetRequested += this.OnAssetRequested;
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
                            newObject.AppendObject(CustomTextureKey,objectDict);
                        }
                    }
                );
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
                for (int i = 4290; i < 4299; i++)
                {
                    StardewValley.Object item = ItemRegistry.Create<StardewValley.Object>($"{i}");
                    Game1.player.addItemToInventory(item);
                    
                    StardewValley.Object item2 = ItemRegistry.Create<StardewValley.Object>($"378");
                    Game1.player.addItemToInventory(item2);
                }
                Monitor.Log($"Added custom item to inventory", LogLevel.Info);
            }

            if (e.Button == SButton.F6)
            {
                Monitor.Log($"Location name {Game1.player.currentLocation.name} tile x {Game1.player.Tile.X} y {Game1.player.Tile.Y}", LogLevel.Warn);
                MachineData tmp = DataLoader.Machines(Game1.content).GetValueOrDefault("(BC)13");
                Monitor.Log($"out {tmp.OutputRules[0].Id}", LogLevel.Warn);
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
        
        
        /*
        [HarmonyPatch(typeof(ShopMenu), MethodType.Constructor)]
        [HarmonyPatch(new Type[] { typeof(string),typeof(ShopData),typeof(ShopOwnerData),typeof(NPC),typeof(ShopMenu.OnPurchaseDelegate),typeof(Func<ISalable, bool>),typeof(bool) })]
        public class ShopMenuConstructorPatch
        {
            public static void Prefix(GameMenu __instance, string shopId, ShopData shopData, ShopOwnerData ownerData, NPC owner = null, ShopMenu.OnPurchaseDelegate onPurchase = null, Func<ISalable, bool> onSell = null, bool playOpenSound = true)
            {
                foreach (string salableItemTag in shopData.SalableItemTags)
                {
                    ModMonitor.Log($"Tag name {salableItemTag}", LogLevel.Warn);
                }
            }
        }
        */
    }
}