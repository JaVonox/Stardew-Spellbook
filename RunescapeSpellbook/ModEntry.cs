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
using StardewValley.GameData.Weapons;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.Tools;

namespace RunescapeSpellbook
{
    internal sealed class ModEntry : Mod
    {
        public static ModEntry Instance;
        public override void Entry(IModHelper helper)
        {
            Instance = this;
            
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
            helper.ConsoleCommands.Add("rs_grantrunes", "Gives the player some runes.\n\nUsage: rs_grantrunes <value>\n\n value: default, rune name, elemental, catalytic, teleport, utility, combat, combat2", this.GrantRunes);
            helper.ConsoleCommands.Add("rs_grantstaff", "Gives the player staves.\n\nUsage: rs_grantstaff", this.GrantStaffs);
            helper.ConsoleCommands.Add("rs_grantammo", "Gives the player ammo.\n\nUsage: rs_grantammo", this.GrantAmmo);
            helper.ConsoleCommands.Add("rs_granttreasure", "Gives the player treasures.\n\nUsage: rs_granttreasure", this.GrantTreasures);
            helper.ConsoleCommands.Add("rs_grantpacks", "Gives the player packs.\n\nUsage: rs_grantpacks", this.GrantPacks);
            helper.ConsoleCommands.Add("rs_miscDebug", "Runs a command left in for testing. Do not use. \n\nUsage: rs_miscDebug", this.DebugCommand);
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
            
            //Add wizard event for unlocking magic
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Events/Farm"))
            {
                e.Edit(asset =>
                    {
                        var eventDict = asset.AsDictionary<string, string>().Data;
                        
                        //Wizard gives magic book event
                        eventDict.Add("RS.0/f Wizard 1000/t 600 1200",
                            "continue/64 15/farmer 64 16 2 Wizard 64 18 0" +
                            "/pause 1500/speak Wizard \"Greetings, @. I hope I am not interrupting your work on the farm.\"" +
                            "/speak Wizard \"I've made great progress with my research as of late, thanks to your generous gifts.\"" +
                            "/speak Wizard \"As thanks, I wanted to give you this old tome of runic magic from my personal library, I have no use for it anymore.\"" +
                            "/stopMusic /itemAboveHead 4290 /pause 1500 /glow 24 107 97 /playsound RunescapeSpellbook.MagicLevel /pause 2000 /mail RSSpellMailGet" +
                            "/speak Wizard \"This form of magic should be suitable for a novice. You need only some runestones, I'm sure you've come across some in the mines already.\"/pause 600" +
                            "/speak Wizard \"Well, that was all. I'll be on my way now.\"" +
                            "/pause 300/end");
                    }
                );
            }
            
            //Gunther + Marlon event
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Events/ArchaeologyHouse"))
            {
                e.Edit(asset =>
                    {
                        var eventDict = asset.AsDictionary<string, string>().Data;
                        //Wizard gives magic book event
                        eventDict.Add("RS.1/n RSRunesFound",
                            "continue/11 9/farmer 50 50 0 Gunther 11 9 0 Marlon 12 9 3" +
                            "/skippable /pause 1000/speak Gunther \"Marlon, you know I can't accept a sword as payment for your late return fees...\"" +
                            "/speak Marlon \"This is an antique! I've been using this blade for decades now!\"" +
                            "/warp farmer 3 14 /playSound doorClose /pause 1000" +
                            "/move farmer 0 -1 1 /move farmer 3 0 2 /move farmer 0 1 1 /move farmer 5 0 0 /move Marlon 0 0 2 /move farmer 0 -3 0" +
                            "/move Gunther 0 0 2 /speak Gunther \"Ah! Welcome! Just let me finish putting these books away and I'll be right with you!\"" +
                            "/move Gunther 0 0 0 /pause 500 /jump Gunther 8 /pause 500 /textAboveHead Gunther \"*huff* *puff*\" /pause 2000 /move Gunther 0 0 2"+
                            "/speak Gunther \"Perhaps the books can wait. What do you need today, @?\"" +
                            "/question null \"#I found this underground#Can you tell me about this?\"" +
                            "/speak Gunther \"Let me have a look...\" /pause 1000"+
                            "/speak Gunther \"Hmm... I'm not quite sure what that is... \""+
                            "/speak Gunther \"The runes aren't any I recognise either...\""+
                            "/move Marlon 0 0 3 /speak Marlon \"Ah, well isn't that nostalgic.\""+
                            "/move Gunther 0 0 1 /speak Gunther \"You're familiar with these?\""+
                            "/speak Marlon \"Not myself, but an old friend of mine used to be obsessed with them.\""+
                            "/move Marlon 0 0 2 /speak Marlon \"Could you bring it over here, @? I'd like to have a closer look.\""+
                            "/move farmer 1 0 0 /move Marlon 0 0 2 /move farmer 0 -1 0 /pause 1000 /move farmer 0 1 0 /pause 1000" +
                            "/speak Marlon \"As I suspected, these are definitely guthixian runestones. Or rather, they contain guthixian runestones.\"" +
                            "/speak Marlon \"Ol' Ras used to spend hours trying to crack these things open, until I showed up. Turns out a strike with the trusty hammer does the job in seconds.\""+
                            "/move Marlon 0 1 2 /move Gunther 0 0 2" +
                            "/speak Marlon \"I'd take these down to the blacksmith, If he's worth his prices, he'll be able to open them.\""+
                            "/speak Marlon \"If you want to actually use the things, you'll have to pry it out of Rasmodius. He's a secretive old man, but get on his good side and he'll talk your ear off.\""+
                            "/pause 500 /end");
                    }
                );
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
                                    if (!mailDict.ContainsKey(mailKey))
                                    {
                                        mailDict.Add(mailKey, mailVal);
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
                                catch (Exception e)
                                {
                                    Monitor.Log(e.Message, LogLevel.Error); //reports mail error if the mail delim method failed. this should be rare, and only should occur with non-dated mail
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
                            //I.e Fire rune packs will have the same gift value as a fire rune
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
                                
                                characterPrefsStrings[idToModify] = characterPrefsStrings[idToModify] + $" {itemPref.Key}";
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
                __instance.tabs[__instance.tabs.Count - 1] = new ClickableComponent(
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
                    ClickableComponent c = __instance.tabs.Where(x => x.name == "RSspellbook").First();
                    b.Draw(ModAssets.extraTextures, new Vector2(c.bounds.X, c.bounds.Y + ((__instance.currentTab == __instance.getTabNumberFromName(c.name)) ? 8 : 0)), new Rectangle(0, 0, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0001f);
                    
                    if (!__instance.hoverText.Equals(""))
                    {
                        IClickableMenu.drawHoverText(b, __instance.hoverText, Game1.smallFont, 0, 0, -1, null, -1, null, null, 0, null, -1, -1, -1, 1f, null, null, null, null, null, null);
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
                    if (outID != 4359 && outID != 4360 && modItem is TreasureObjects && modItem is not PackObject)
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
                        f.playNearbySoundLocal("clubswipe", null);
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

                        List<MagicProjectile> generatedProjectiles;
                        
                        var cachedDataField = Traverse.Create(__instance).Field("cachedData");
                        var cachedData = cachedDataField.GetValue();

                        if (cachedData is not StaffWeaponData)
                        {
                            Instance.Monitor.Log("Invalid cast to staff weapon", LogLevel.Error);
                            return;
                        }
                        KeyValuePair<bool, string> castReturn = spell.CreateCombatProjectile(who, (StaffWeaponData)cachedData, mouseX, mouseY, out generatedProjectiles);

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
                        if (ModAssets.HasMagic(Game1.player))
                        {
                            Game1.showRedMessage("No Selected Spell", true);
                        }
                        else
                        {
                            Game1.showRedMessage("I don't know how to use this", true);
                        }
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
                    ;
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
                if (ModAssets.monsterDrops.ContainsKey(name))
                {
                    List<ItemDrop> monsterDrops = ModAssets.monsterDrops[name];

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
                if (id == "24" && !__result) //If we are searching for 24 - the monster musk bonus - and we dont find it, also check for 420 - the dark lure buff
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
            this.Monitor.Log("Added magic");
            int reqLevel;
            if (args.Length > 0 && int.TryParse(args[0], out reqLevel))
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
            
            int reqLevel;
            if (int.TryParse(args[0], out reqLevel))
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
            
            int reqExp;
            if (int.TryParse(args[0], out reqExp))
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
            
            int reqAddExp;
            if (int.TryParse(args[0], out reqAddExp))
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
                Instance.Monitor.Log($"Farmer: {farmerRoot.Name}",LogLevel.Info);
                Instance.Monitor.Log($"HasMagic: {ModAssets.HasMagic(farmerRoot)}",LogLevel.Info);
                Instance.Monitor.Log($"Level: {ModAssets.GetFarmerMagicLevel(farmerRoot)}",LogLevel.Info);
                Instance.Monitor.Log($"Exp: {farmerRoot.modData["TofuMagicExperience"]}",LogLevel.Info);
                
                List<int> perkIDs = ModAssets.PerksAssigned(farmerRoot);
                int perkIndex = 1;
                foreach (int id in perkIDs)
                {
                    string perkName = id == -1 ? "Unassigned" : ModAssets.perks.Where(x=>x.perkID==id).Select(x=>x.perkName).First();
                    Instance.Monitor.Log($"Perk Slot {perkIndex}: {perkName}",LogLevel.Info);
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

            string runeReq;
            if (args.Length == 0)
            {
                runeReq = "default";
            }
            else
            {
                runeReq = args[0].ToLower();
            }
            
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
            
            foreach (ModLoadObjects foundItem in ModAssets.modItems.Where(x=>x.Value is TreasureObjects && x.Value is not PackObject).Select(y=>y.Value))
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
            Game1.warpFarmer("Caldera",24,30,2);
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