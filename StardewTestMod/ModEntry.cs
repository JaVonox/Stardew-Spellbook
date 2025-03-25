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
                            newObject.AppendObject("Mods.StardewTestMod.Assets.modsprites", objectDict);
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
                    
                    WeaponData newWeapon = new WeaponData();
                    newWeapon.Name = "staff_battlestaff";
                    newWeapon.DisplayName = "battlestaff";
                    newWeapon.Description = "test battlestaff";
                    newWeapon.Type = 429;
                    newWeapon.Texture = CustomTextureKey;
                    newWeapon.SpriteIndex = 11;
                    weaponDict[$"4290"] = newWeapon;
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
                    item.stack.Value = 10;
                    Game1.player.addItemToInventory(item);
                }
                
                MeleeWeapon weapon = ItemRegistry.Create<MeleeWeapon>($"(W)4290");
                Game1.player.addItemToInventory(weapon);
                
            }

            if (e.Button == SButton.F6)
            {
                Monitor.Log($"Loca x {Game1.player.Tile.X}, y {Game1.player.Tile.Y}" , LogLevel.Warn);
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
                        KeyValuePair<bool, string> castReturn = spell.CreateCombatProjectile(who, mouseX, mouseY, out generatedProjectile);

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
        public class FirstSaveLoadPatcher
        {
            public static void Postfix(Game1 __instance, GameTime gameTime)
            {
                if (Game1.gameMode == 3 && Game1.gameModeTicks == 1)
                {
                    ModAssets.localFarmerData.FirstGameTick();
                }
            }
        }
        
        //TODO add selected spell to farmer, and then use that projectile in the FireProjectile method
        //TODO like how inventory works, we should on page cast we set the farmer variable, then on fire projectile we do the second cast which will decrement the runes
        //TODO also use mouse pos for the angle of the spell?
    }
}