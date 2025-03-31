using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
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
                            newObject.AppendObject(CustomTextureKey, objectDict);
                        }
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
                Game1.player.modData["TofuMagicLevel"] = "0";
                Game1.player.modData["TofuMagicExperience"] = "0";
                Game1.player.modData["TofuMagicProfession1"] = "-1";
                Game1.player.modData["TofuMagicProfession2"] = "-1";
                Game1.player.modData["HasUnlockedMagic"] = "1";
                Monitor.Log("ResetLevel", LogLevel.Info);
            }
            
            if (e.Button == SButton.F8)
            {
                ModAssets.IncrementMagicExperience(Game1.player,5000);
                Monitor.Log("Set Extra EXP", LogLevel.Info);
            }
            
            if (e.Button == SButton.F9)
            {
                foreach (Farmer farmerRoot in ModAssets.GetFarmers())
                {
                    ModMonitor.Log($"Farmer: {farmerRoot.Name}",LogLevel.Warn);
                    ModMonitor.Log($"Exp: {farmerRoot.modData["TofuMagicExperience"]}",LogLevel.Warn);
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
        
        //TODO remove forge + enchant ability from staves
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

                        List<MagicProjectile> generatedProjectiles;
                        StaffWeaponData staffWeaponData = (StaffWeaponData)Traverse.Create(__instance).Field("cachedData").GetValue();
                        KeyValuePair<bool, string> castReturn = spell.CreateCombatProjectile(who, staffWeaponData, mouseX, mouseY, out generatedProjectiles);

                        if (castReturn.Key && generatedProjectiles.Count > 0)
                        {
                            foreach (MagicProjectile projectile in generatedProjectiles)
                            {
                                who.currentLocation.projectiles.Add(projectile);
                            }
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
                    
                    if (!Game1.player.modData.ContainsKey("TofuMagicLevel"))
                    {
                        Game1.player.modData.Add("TofuMagicLevel","0");
                    }
                    
                    if (!Game1.player.modData.ContainsKey("TofuMagicExperience"))
                    {
                        Game1.player.modData.Add("TofuMagicExperience","0");
                    }
                    
                    if (!Game1.player.modData.ContainsKey("TofuMagicProfession1"))
                    {
                        Game1.player.modData.Add("TofuMagicProfession1","-1");
                    }
                    
                    if (!Game1.player.modData.ContainsKey("TofuMagicProfession2"))
                    {
                        Game1.player.modData.Add("TofuMagicProfession2","-1");
                    }
                    
                    if (!Game1.player.modData.ContainsKey("HasUnlockedMagic"))
                    {
                        Game1.player.modData.Add("HasUnlockedMagic","1"); //TODO update when we make magic unlockable
                    }
                }
            }
        }
    }
}