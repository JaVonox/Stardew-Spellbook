using System.Reflection;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.GameData.Objects;
using StardewValley.GameData.Weapons;

using StardewValley.Menus;
using StardewValley.Projectiles;
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
                    newWeapon.Type = 4;
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
                    Game1.player.addItemToInventory(item);
                }
                
                MeleeWeapon weapon = ItemRegistry.Create<MeleeWeapon>($"(W)4290");
                Game1.player.addItemToInventory(weapon);
                
                Monitor.Log($"Added custom items to inventory", LogLevel.Info);
            }

            if (e.Button == SButton.F6)
            {
                Monitor.Log($"Location name {Game1.player.currentLocation.name} tile x {Game1.player.Tile.X} y {Game1.player.Tile.Y}", LogLevel.Warn);
                
                /*
                ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem("(W)1");
                ParsedItemData itemData2 = ItemRegistry.GetDataOrErrorItem("(W)21");
                */
                string[] IDs = { "1","21","4290"};
                int iter = 0;
                for (int i = 0; i < IDs.Length; i++)
                {
                    //ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(IDs[i]);
                    Monitor.Log($"ID {IDs[i]}", LogLevel.Info);
                    if (MeleeWeapon.TryGetData(IDs[i], out var data))
                    {
                        Monitor.Log($"in {IDs[i]}", LogLevel.Info);
                        Monitor.Log($"item" +
                                    $"Name {data.Name}" +
                                    $"Min Damage {data.MinDamage}"+
                                    $"Max Damage {data.MaxDamage}"+
                                    $"KnockBack {data.Knockback}"+
                                    $"speed {data.Speed}"+
                                    $"addedPrec {data.Precision}"+
                                    $"addedDef {data.Defense}"+
                                    $"type {data.Type}"+
                                    $"addedArea {data.AreaOfEffect}"+
                                    $"critChance {data.CritChance}"+
                                    $"critMult {data.CritMultiplier}"
                            , LogLevel.Warn);
                    }
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
                if (type == 4)
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
                if(__instance.type.Value == 4)
                {
                    ModMonitor.Log($"Staff Projectile", LogLevel.Warn);
                }
                
            }
        }
        
    }
}