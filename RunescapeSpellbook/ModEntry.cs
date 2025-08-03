using System.Reflection;
using System.Text;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Buffs;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.Objects;
using StardewValley.GameData.Shops;
using StardewValley.GameData.Weapons;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.Tools;
using Object = StardewValley.Object;

namespace RunescapeSpellbook
{
    internal sealed class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            var harmony = new Harmony(this.ModManifest.UniqueID);
            harmony.PatchAll();

            ModAssets.Load(helper);
            
            helper.Events.Content.AssetRequested += this.OnAssetRequested;
            
            helper.ConsoleCommands.Add("rs_grantmagic", "Gives the player magic.\n\nUsage: rs_grantmagic <value>\n\n value: the level to set to", this.GrantMagic);
            helper.ConsoleCommands.Add("rs_setlevel", "Sets the players magic level.\n\nUsage: rs_setlevel <value>\n\n value: the level to set to", this.SetLevel);
            helper.ConsoleCommands.Add("rs_setexp", "Sets the players experience level.\n\nUsage: rs_setexp <value>\n\n value: the experience to set to", this.SetExp);
            helper.ConsoleCommands.Add("rs_addexp", "Adds to the players experience level.\n\nUsage: rs_addexp <value>\n\n value: the experience to add", this.AddExp);
            helper.ConsoleCommands.Add("rs_clearperks", "Clears a players perks.\n\nUsage: rs_clearperks", this.ResetPerks);
            helper.ConsoleCommands.Add("rs_info", "Dumps some info about all players to console.\n\nUsage: rs_info", this.PlayerInfo);
            helper.ConsoleCommands.Add("rs_addrunes", "Gives the player some runes.\n\nUsage: rs_addrunes <value>\n\n value: default, rune name, elemental, catalytic, teleport, utility, combat, combat2", this.GrantRunes);
            helper.ConsoleCommands.Add("rs_addweps", "Gives the player staves.\n\nUsage: rs_addweps", this.GrantStaffs);
            helper.ConsoleCommands.Add("rs_addammo", "Gives the player ammo.\n\nUsage: rs_addammo", this.GrantAmmo);
            helper.ConsoleCommands.Add("rs_addtreasures", "Gives the player treasures.\n\nUsage: rs_addtreasures", this.GrantTreasures);
            helper.ConsoleCommands.Add("rs_addpacks", "Gives the player packs.\n\nUsage: rs_addpacks", this.GrantPacks);
            helper.ConsoleCommands.Add("rs_debug_misc", "Runs a command left in for testing. Do not use. \n\nUsage: rs_debug_misc", this.DebugCommand);
            helper.ConsoleCommands.Add("rs_debug_position", "Reports the position of the local player \n\nUsage: rs_debug_position", this.DebugPosition);
        }
            
        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Mods.RunescapeSpellbook.Assets.modsprites"))
            {
                e.LoadFromModFile<Texture2D>("Assets/itemsprites", AssetLoadPriority.Medium);
            }
            
            if (e.NameWithoutLocale.IsEquivalentTo("Mods.RunescapeSpellbook.Assets.spellanimations"))
            {
                e.LoadFromModFile<Texture2D>("Assets/spellanimations", AssetLoadPriority.Medium);
            }
            
            if (e.NameWithoutLocale.IsEquivalentTo("Data/AudioChanges"))
            {
                e.Edit(asset =>
                    {
                        List<string> audioTracks = ModAssets.modSpells.Select(x=>x.audioID).Distinct().ToList();
                        audioTracks.Add("Preserve"); //Add the sound for hitting
                        audioTracks.Add("MagicLevel"); //Add the sound for levelling up
                        audioTracks.Add("MultiHit"); //Add the sound for when you fire multiple projectiles
                        
                        var data = asset.AsDictionary<string, AudioCueData>().Data;
                        foreach (string audioTrack in audioTracks)
                        {
                            data.Add($"{audioTrack}", new AudioCueData() {
                                Id = $"RunescapeSpellbook.{audioTrack}",
                                Category = "Sound",
                                FilePaths = new List<string>() { Path.Combine(Helper.DirectoryPath, "Assets/Audio", $"{audioTrack}.ogg") },
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
                            newObject.AppendObject("Mods.RunescapeSpellbook.Assets.modsprites", objectDict);
                        }
                    }
                );
            }
            
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Shops"))
            {
                e.Edit(asset =>
                    {
                        foreach (KeyValuePair<string,List<ShopListings>> addShopData in ModAssets.loadedShops)
                        {
                            ShopData shopData = asset.AsDictionary<string, ShopData>().Data[addShopData.Key];
                            foreach (ShopListings ShopListings in addShopData.Value)
                            {
                                shopData.Items.Add(ShopListings.itemData);
                            }
                        }
                    }
                );
            }

            if (e.NameWithoutLocale.StartsWith("Data/Events") && ModAssets.loadableEvents.ContainsKey(e.NameWithoutLocale.Name))
            {
                e.Edit(asset =>
                {
                    var eventDict = asset.AsDictionary<string, string>().Data;

                    foreach (KeyValuePair<string, string> eventData in ModAssets.loadableEvents[
                                 e.NameWithoutLocale.Name])
                    {
                        eventDict.Add(eventData.Key, eventData.Value);
                    }
                });
            }
            
            //Possible mails
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Mail"))
            {
                e.Edit(asset =>
                    {
                        var mailDict = asset.AsDictionary<string, string>().Data;

                        foreach (KeyValuePair<string,string> mail in ModAssets.loadableMail)
                        {
                            string mailKey = mail.Key;
                            string mailVal = mail.Value;
                            
                            while (true) //Loop until we get a valid assignment for mail
                            {
                                try
                                {
                                    //This removes the mail if it overlaps with another mail - such as if a new mail is added on a date the mod uses or if a future update adds the same key
                                    if (mailDict.TryAdd(mailKey,mailVal))
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        Monitor.Log($"Duplicate mail key: {mailKey}. Attempting to access next available year", LogLevel.Warn);
                                        string[] mailDelim = mailKey.Split('_');
                                        int newYear = int.Parse(mailDelim[2]) + 1;
                                        
                                        if (newYear > 255)
                                        {
                                            throw new Exception($"Couldn't find a new year for {mailKey} before 255");
                                        }
                                        
                                        mailKey = mailDelim[0] + "_" + mailDelim[1] + "_" + newYear; //Adds mail to the same date next year. increments year until we get a valid value
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Monitor.Log(ex.Message, LogLevel.Error); //reports mail error if the mail delim method failed. this should be rare, and only should occur with non-dated mail
                                    break;
                                }
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
                            Dictionary<int,PrefType> itemPreferences = 
                                ModAssets.modItems.Where(x=>x.Value.characterPreferences != null && x.Value.characterPreferences.Keys.Contains(characterName))
                                    .ToDictionary(i=>i.Value.id,j=> j.Value.characterPreferences[characterName]); //get a dictionary of gifts for this character with their preference
                            
                            if(!itemPreferences.Any()) {continue;} //If we have no data to assign, skip this entirely
                            
                            //If all treasures are the same for a treasureitem, then we consider it the same as gifting the item itself
                            //I.e. Fire rune packs will have the same gift value as a fire rune
                            Dictionary<int, PrefType> treasureItems =
                                ModAssets.modItems.Where(x =>
                                    x.Value is PackObject pack && ModAssets.modItems.ContainsKey(pack.packItem) && ModAssets.modItems[pack.packItem].characterPreferences.ContainsKey(characterName))
                                    .ToDictionary(i=>i.Value.id,j=> ModAssets.modItems[((PackObject)j.Value).packItem].characterPreferences[characterName]);

                            if (treasureItems.Any()) //Add treasures in to item preferences
                            {
                                itemPreferences.TryAddMany(treasureItems);
                            }

                            string[] characterPrefsStrings = preferencesDict[characterName].Split('/'); //character preferences are delimited with / 
                            //The indexes always follow this format: 0 text 1 loveIDs 2 text 3 likeIDs 4 text 5 dislikeIDs 6 text 7 hateIDs 8 text 9 neutralIDs
                            
                            foreach (KeyValuePair<int, PrefType> itemPref in itemPreferences)
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
                        weaponDict.Add(newWeapon.id.ToString(), newWeapon);
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
                //Replace the exit table position so it is at the end of the list
                //^1 is the same as length-1, apparently. neat
                __instance.tabs[^1] = new ClickableComponent(
                    new Rectangle(__instance.xPositionOnScreen + 704,
                        __instance.yPositionOnScreen + IClickableMenu.tabYPositionRelativeToMenuY + 64, 64, 64), "exit",
                    Game1.content.LoadString("Strings\\UI:GameMenu_Exit"))
                {
                    myID = 12349,
                    downNeighborID = 9,
                    leftNeighborID = 12348,
                    tryDefaultIfNoDownNeighborExists = true,
                    fullyImmutable = true
                };
                
                __instance.pages.Add(new SpellbookPage(__instance.xPositionOnScreen, __instance.yPositionOnScreen, __instance.width - 64 - 16, __instance.height));
                __instance.tabs.Add(new ClickableComponent(new Rectangle(__instance.xPositionOnScreen + 640, __instance.yPositionOnScreen + IClickableMenu.tabYPositionRelativeToMenuY + 64, 64, 64), "RSspellbook", "Spellbook")
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
            public static void Postfix(GameMenu __instance, SpriteBatch b)
            {
                if(!__instance.invisible)
                {
                    ClickableComponent c = __instance.tabs.First(x=>x.name == "RSspellbook");
                    b.Draw(ModAssets.extraTextures, new Vector2(c.bounds.X, c.bounds.Y + ((__instance.currentTab == __instance.getTabNumberFromName(c.name)) ? 8 : 0)), new Rectangle(0, 0, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0001f);
                    
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
                if (disallow_special_geodes && int.TryParse(item.ItemId, out int outID) && ModAssets.modItems.TryGetValue(outID, out ModLoadObjects modItem))
                {
                    if (outID != 4359 && outID != 4360 && modItem is TreasureObjects and not PackObject)
                    {
                        __result = false;
                        return false;
                    }
                }

                return true;
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
        
        //TODO add Custom forge enchants to replace existing forge enchants 
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
                        
                        KeyValuePair<bool, string> castReturn = spell.CreateCombatProjectile(who, staffWepData, mouseX, mouseY, out List<MagicProjectile> generatedProjectiles);

                        if (castReturn.Key && generatedProjectiles.Count > 0)
                        {
                            foreach (MagicProjectile projectile in generatedProjectiles)
                            {
                                who.currentLocation.projectiles.Add(projectile);
                            }
                        }
                        else
                        {
                            Game1.showRedMessage(castReturn.Value);
                        }
                    }
                    else
                    {
                        Game1.showRedMessage(ModAssets.HasMagic(Game1.player) ? "No Selected Spell" : "I don't know how to use this");
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
                    bool staffProvidesRune = staffWeaponData.providesRune != -1;
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
                    int level = staffWeaponData.providesRune != -1 ? 10 : staffWeaponData.id - 4343;
                    
                    __result = $"Level {level} Battlestaff";
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
                        Utility.drawTextWithShadow(spriteBatch, $"{staffWeaponData.projectileDamageModifier}x Spell Damage", font,
                            new Vector2(x + 16 + 52, y + 16 + 12), c3 * 0.9f * alpha);
                        y += extraSize;
                    }
                    
                    if (staffWeaponData.providesRune != -1)
                    {
                        Utility.drawWithShadow(spriteBatch, ModAssets.extraTextures,
                            new Vector2(x + 16 + 4, y + 16 + 4), new Rectangle(26, 0, 10, 10), Color.White, 0f,
                            Vector2.Zero, 4f, flipped: false, 1f);
                        Utility.drawTextWithShadow(spriteBatch, $"{ModAssets.modItems[staffWeaponData.providesRune].DisplayName}", font,
                            new Vector2(x + 16 + 52, y + 16 + 12), c3 * 0.9f * alpha);
                        y += extraSize;
                    }
                }
            }
        }
            
        }
        
        //This patch runs
        [HarmonyPatch(typeof(Farmer), "farmerInit")]
        public class SetupModVariablesPatcher
        {
            public static void Postfix(Farmer __instance)
            {
                if (!__instance.modData.ContainsKey("TofuMagicLevel"))
                {
                    __instance.modData.Add("TofuMagicLevel","0");
                    __instance.modData.Add("TofuMagicExperience","0");
                    __instance.modData.Add("TofuMagicProfession1","-1");
                    __instance.modData.Add("TofuMagicProfession2","-1");
                    __instance.modData.Add("HasUnlockedMagic","0");
                }
                
                ModAssets.localFarmerData.Reset();
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
                                __instance.objectsToDrop.Add(item.itemID.ToString());
                            }
                        }
                    }
                }
            }
        }
        
        [HarmonyPatch(typeof(Buff), MethodType.Constructor)]
        [HarmonyPatch(new Type[] { typeof(string),typeof(string),typeof(string),typeof(int),typeof(Texture2D),typeof(int),typeof(BuffEffects),typeof(bool),typeof(string),typeof(string) })]
        public class BuffPatcher
        {
            public static void Postfix(Buff __instance,string id, string source = null, string displaySource = null, int duration = -1, Texture2D iconTexture = null, int iconSheetIndex = -1, BuffEffects effects = null, bool? isDebuff = null, string displayName = null, string description = null)
            {
                if (id == "429") //Charge buff
                {
                    __instance.displayName = "Charge";
                    __instance.description = "Summons more shots for every combat spell";
                    __instance.millisecondsDuration = 60000;
                    __instance.glow = Color.White;
                    __instance.iconTexture = ModAssets.extraTextures;
                    __instance.iconSheetIndex = 3;
                }
                else if (id == "430") //Dark lure buff
                {
                    __instance.displayName = "Dark Lure";
                    __instance.description = "Spawns more monsters, and makes them prioritise you over other farmers";
                    __instance.millisecondsDuration = 180000;
                    __instance.glow = Color.White;
                    __instance.iconTexture = ModAssets.extraTextures;
                    __instance.iconSheetIndex = 4;
                }
            }
        }


        [HarmonyPatch(typeof(Farmer), "hasBuff")]
        [HarmonyPatch(new Type[] { typeof(string) })]
        public class FarmerhasBuffPatcher
        {
            public static void Postfix(ref bool __result, Farmer __instance, string id)
            {
                if (id == "24" && !__result) //If we are searching for 24 - the monster musk bonus - and we do not find it, also check for 420 - the dark lure buff
                {
                    __result = __instance.buffs.IsApplied("430");
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
                if (!__instance.hasOrWillReceiveMail("RSRunesFound") && int.TryParse(item.ItemId, out int itemId) && ModAssets.modItems.TryGetValue(itemId, out var modItem))
                {
                    if (modItem is PackObject || itemId == 4359 || itemId == 4360)
                    {
                        __instance.mailReceived.Add("RSRunesFound");
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
                if (int.TryParse(item.ItemId, out int itemId) && ModAssets.modItems.TryGetValue(itemId, out var modItem))
                {
                    if (modItem is PackObject || itemId == 4359 || itemId == 4360)
                    {
                        Game1.drawObjectDialogue(new List<string>
                        {
                            "Your hands tingle as you pick up the mysterious object. Maybe the archaeologist will know something about this?"
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
                if (f.hasBuff($"430")) //If we have dark lure, make the result extremely low for this player, so we can increase their priority
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
                __result = __result || o.QualifiedItemId == "(O)4301" || o.QualifiedItemId == "(O)4302";
            }
        }
        
        [HarmonyPatch(typeof(Slingshot), "GetAmmoDamage")]
        [HarmonyPatch(new Type[] { typeof(StardewValley.Object)})]
        public class SlingshotDamagePatcher
        {
            public static void Postfix(ref int __result, StardewValley.Object ammunition)
            {
                if (__result == 1) //Append extra slingshot damages to the orb items
                {
                    switch (ammunition.QualifiedItemId)
                    {
                        case "(O)4301":
                            __result = 15;
                            break;
                        case "(O)4302":
                            __result = 25;
                            break;
                        default:
                            __result = 1;
                            break;
                    }
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
                        __result = "Elemental Rune";
                        break;
                    case -430:
                        __result = "Combat Rune";
                        break;
                    case -431:
                        __result = "Catalytic Rune";
                        break;
                    default:
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
                    default:
                        break;
                } 
            }
        }
        
        /*
        [HarmonyPatch(typeof(BasicProjectile), "behaviorOnCollisionWithMonster")]
        [HarmonyPatch(new Type[] { typeof(NPC),typeof(GameLocation) })]
        public class SlingshotProjectileMonsterHit
        {
            public static bool Prefix(BasicProjectile __instance, NPC n, GameLocation location)
            {
                if (__instance.itemId != null && (__instance.damagesMonsters.Value && n is Monster))
                {
                    Farmer player = __instance.GetPlayerWhoFiredMe(location);
                    Monster monster = n as Monster;
                    if (__instance.itemId.ToString() == "(O)4301") //Water Ammo - significantly cuts monster speed
                    {
                        HitMonster(__instance,monster,location,player,1);
                        return false;
                    }
                    else if (__instance.itemId.ToString() == "(O)4302") //Earth Ammo - significantly cuts monster defence
                    {
                        HitMonster(__instance,monster,location,player,0);
                        return false;
                    }
                }
                return true; 
            }

            private static void HitMonster(BasicProjectile __instance, Monster n, GameLocation location, Farmer player, int debuffType)
            {
                location.damageMonster(n.GetBoundingBox(), __instance.damageToFarmer.Value, __instance.damageToFarmer.Value + 1, isBomb: false, player, isProjectile: true);

                foreach (NPC npcEffected in location.characters)
                {
                    if(!(npcEffected is Monster)) {continue;}
                    
                    Monster monsterEffected = npcEffected as Monster;
                    
                    if (monsterEffected.mineMonster.Value)
                    {
                        //Apply Debuff
                        switch (debuffType)
                        {
                            case 0:
                                monsterEffected.resilience.Value = 1;
                                monsterEffected.doEmote(12);
                                break;
                            case 1:
                                monsterEffected.speed = 1;
                                monsterEffected.doEmote(28);
                                break;
                        }
                    }
                }

                if (!n.IsInvisible)
                {
                    __instance.piercesLeft.Value--;
                }
            }
        }
        */
        
        //Console Commands
        private bool HasNoMagic()
        {
            if (!ModAssets.HasMagic(Game1.player))
            {
                this.Monitor.Log("You don't have access to magic, use rs_grantmagic to give magic access",LogLevel.Warn);
                return true;
            }

            return false;
        }
        
        private bool HasNoWorldContextReady()
        {
            if (!Context.IsWorldReady)
            {
                this.Monitor.Log("World not yet initialised",LogLevel.Warn);
                return true;
            }

            return false;
        }

        private void GrantMagic(string command, string[] args)
        {
            if(HasNoWorldContextReady()){return;}
            
            if (ModAssets.HasMagic(Game1.player))
            {
                this.Monitor.Log("You already have access to magic",LogLevel.Warn);
                return;
            }
            Game1.player.eventsSeen.Add("RS.0");
            Monitor.Log("Added magic");
            if (args.Length > 0 && int.TryParse(args[0], out int reqLevel))
            {
                reqLevel = Math.Clamp(reqLevel, 0, 10);
                Game1.player.modData["TofuMagicLevel"] = (reqLevel).ToString();
                Game1.player.modData["TofuMagicExperience"] = (Farmer.getBaseExperienceForLevel(reqLevel)).ToString();
                this.Monitor.Log($"Set magic level to {reqLevel}",LogLevel.Info);
            }
        }
    
        private void SetLevel(string command, string[] args)
        {
            if (HasNoWorldContextReady() || HasNoMagic()){return;}

            if (args.Length == 0)
            {
                this.Monitor.Log("Specify a magic level to apply",LogLevel.Error);
                return;
            }
            
            if (int.TryParse(args[0], out int reqLevel))
            {
                reqLevel = Math.Clamp(reqLevel, 0, 10);
                Game1.player.modData["TofuMagicLevel"] = (reqLevel).ToString();
                Game1.player.modData["TofuMagicExperience"] = (Farmer.getBaseExperienceForLevel(reqLevel)).ToString();
                this.Monitor.Log($"Set magic level to {reqLevel}",LogLevel.Info);
            }
        }
        
        private void SetExp(string command, string[] args)
        {
            if (HasNoWorldContextReady() || HasNoMagic()){return;}

            if (args.Length == 0)
            {
                this.Monitor.Log("Specify an amount of exp to set to",LogLevel.Error);
                return;
            }
            
            if (int.TryParse(args[0], out int reqExp))
            {
                reqExp = Math.Clamp(reqExp, 0, 15000);
                Game1.player.modData["TofuMagicLevel"] = "0";
                ModAssets.IncrementMagicExperience(Game1.player, reqExp);
                this.Monitor.Log($"Set experience to {reqExp}",LogLevel.Info);
            }
        }
        
        private void AddExp(string command, string[] args)
        {
            if (HasNoWorldContextReady() || HasNoMagic()){return;}

            if (args.Length == 0)
            {
                this.Monitor.Log("Specify an amount of exp to add",LogLevel.Error);
                return;
            }
            
            if (int.TryParse(args[0], out int reqAddExp))
            {
                reqAddExp = Math.Clamp(reqAddExp, 0, Farmer.getBaseExperienceForLevel(10) - ModAssets.GetFarmerExperience(Game1.player));
                ModAssets.IncrementMagicExperience(Game1.player, reqAddExp);
                this.Monitor.Log($"Added {reqAddExp} experience to player",LogLevel.Info);
            }
        }
        
        private void ResetPerks(string command, string[] args)
        {
            if (HasNoWorldContextReady() || HasNoMagic()){return;}
            
            Game1.player.modData["TofuMagicProfession1"] = "-1";
            Game1.player.modData["TofuMagicProfession2"] = "-1";
            
            this.Monitor.Log($"Removed assigned perks",LogLevel.Info);
        }
        
        private void PlayerInfo(string command, string[] args)
        {
            if (HasNoWorldContextReady()){return;}

            foreach (Farmer farmerRoot in ModAssets.GetFarmers())
            {
                Monitor.Log($"Farmer: {farmerRoot.Name}",LogLevel.Info);
                Monitor.Log($"HasMagic: {ModAssets.HasMagic(farmerRoot)}",LogLevel.Info);
                Monitor.Log($"Level: {ModAssets.GetFarmerMagicLevel(farmerRoot)}",LogLevel.Info);
                Monitor.Log($"Exp: {farmerRoot.modData["TofuMagicExperience"]}",LogLevel.Info);
                
                List<int> perkIDs = ModAssets.PerksAssigned(farmerRoot);
                int perkIndex = 1;
                foreach (int id in perkIDs)
                {
                    string perkName = id == -1 ? "Unassigned" : ModAssets.perks.Where(x=>x.perkID==id).Select(x=>x.perkName).First();
                    Monitor.Log($"Perk Slot {perkIndex}: {perkName}",LogLevel.Info);
                }
            }
        }
        private void GrantElemRunes()
        {
            for (int i = 4291; i <= 4294; i++)
            {
                StardewValley.Object item = ItemRegistry.Create<StardewValley.Object>($"{i}");
                item.Stack = 255;
                Game1.player.addItemToInventory(item);
            }
        }
        private void GrantRunes(string command, string[] args)
        {
            if(HasNoWorldContextReady()){return;}
            
            string runeReq = args.Length == 0 ? "default" : args[0].ToLower();
            
            if (runeReq == "default")
            {
                foreach (int id in ModAssets.modItems.Where(x => x.Value is RunesObjects && x.Key != 4290).Select(y=>y.Key))
                {
                    StardewValley.Object item = ItemRegistry.Create<StardewValley.Object>($"{id}");
                    item.Stack = 255;
                    Game1.player.addItemToInventory(item);
                }
            }
            else if (runeReq == "elemental" || runeReq == "elem")
            {
                GrantElemRunes();
            }
            else if (runeReq == "catalytic" || runeReq == "cat" || runeReq == "cata")
            {
                for (int i = 4295; i <= 4300; i++)
                {
                    StardewValley.Object item = ItemRegistry.Create<StardewValley.Object>($"{i}");
                    item.Stack = 255;
                    Game1.player.addItemToInventory(item);
                }
            }
            else if (runeReq == "teleport" || runeReq == "tele")
            {
                GrantElemRunes();
                
                StardewValley.Object item = ItemRegistry.Create<StardewValley.Object>($"4295");
                item.Stack = 255;
                Game1.player.addItemToInventory(item);
            }
            else if (runeReq == "utility" || runeReq == "util")
            {
                GrantElemRunes();
                
                for (int i = 4296; i <= 4298; i++)
                {
                    StardewValley.Object item = ItemRegistry.Create<StardewValley.Object>($"{i}");
                    item.Stack = 255;
                    Game1.player.addItemToInventory(item);
                }
            }
            else if (runeReq == "combat" || runeReq == "comb")
            {
                GrantElemRunes();
                
                for (int i = 4299; i <= 4300; i++)
                {
                    StardewValley.Object item = ItemRegistry.Create<StardewValley.Object>($"{i}");
                    item.Stack = 255;
                    Game1.player.addItemToInventory(item);
                }
            }
            else if (runeReq == "combat2" || runeReq == "com2" || runeReq == "comb2")
            {
                GrantElemRunes();
                
                for (int i = 4297; i <= 4300; i++)
                {
                    StardewValley.Object item = ItemRegistry.Create<StardewValley.Object>($"{i}");
                    item.Stack = 255;
                    Game1.player.addItemToInventory(item);
                }
            }
            else
            {
                List<ModLoadObjects> matchList = ModAssets.modItems.Where(x=>x.Value is RunesObjects && x.Value.Name.ToLower().Contains(runeReq)).Select(y=>y.Value).ToList();
                if (matchList.Count == 0)
                {
                    this.Monitor.Log($"Invalid rune set to grant {runeReq}",LogLevel.Error);
                    return;
                }
                else
                {
                    StardewValley.Object item = ItemRegistry.Create<StardewValley.Object>($"{matchList[0].id}");
                    item.Stack = 255;
                    Game1.player.addItemToInventory(item);
                }
                
            }
            
            this.Monitor.Log($"Granted runes from set {runeReq}",LogLevel.Info);
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
            
            this.Monitor.Log($"Granted all ammos",LogLevel.Info);
        }
        private void GrantStaffs(string command, string[] args)
        {
            if (HasNoWorldContextReady()){return;}

            foreach (StaffWeaponData newWeapon in ModAssets.staffWeapons)
            {
                MeleeWeapon item = ItemRegistry.Create<MeleeWeapon>(newWeapon.id.ToString());
                Game1.player.addItemToInventory(item);
            }
            
            this.Monitor.Log($"Granted all staves",LogLevel.Info);
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
            
            this.Monitor.Log($"Granted all treasures",LogLevel.Info);
        }
        private void DebugCommand(string command, string[] args)
        {
            if (HasNoWorldContextReady()){return;}
            Game1.warpFarmer("AdventureGuild",5,13,2);
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
            
            this.Monitor.Log($"Granted all packs",LogLevel.Info);
        }
    }
}