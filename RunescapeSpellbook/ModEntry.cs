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
using Object = StardewValley.Object;

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
            
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.Content.AssetRequested += this.OnAssetRequested;
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

                        foreach (ModLoadObjects newObject in ModAssets.modItems)
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
                        eventDict.Add("RS.0/f Wizard 0/t 600 700",
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
                            "/pause 1000/speak Gunther \"Marlon, you know I can't accept a sword as payment for your late return fees...\"" +
                            "/speak Marlon \"This is an antique! I've been using this blade for decades now!\"" +
                            "/warp farmer 3 14 /playSound doorClose /pause 1000" +
                            "/move farmer 0 -1 1" +
                            "/move farmer 3 0 2" +
                            "/move farmer 0 1 1" +
                            "/move farmer 5 0 0" +
                            "/move farmer 0 -3 0" +
                            "/move Gunther 0 0 2" +
                            "/move Marlon 0 0 2" +
                            "/speak Gunther \"Ah! Welcome! Just let me finish putting these books away and I'll be right with you!\"" +
                            "/move Gunther 0 0 0 /pause 500 /jump Gunther 8 /pause 500 /textAboveHead Gunther \"*huff* *puff*\" /pause 2000 /move Gunther 0 0 2"+
                            "/speak Gunther \"Perhaps the books can wait. What do you need today, @?\"" +
                            "/question null \"#I found this underground#Can you tell me about this?\"" +
                            "/speak Gunther \"Let me have a look...\""+
                            "/pause 1000" +
                            "/speak Gunther \"Hmm... I'm not quite sure what that is... \""+
                            "/speak Gunther \"The runes aren't any I recognise either...\""+
                            "/move Marlon 0 0 3" +
                            "/speak Marlon \"Ah, well isn't that nostalgic. It's been decades since I've seen one of those.\""+
                            "/move Gunther 0 0 1" +
                            "/speak Gunther \"You're familiar with these?\""+
                            "/speak Marlon \"Not myself, but an old friend of mine used to be obsessed with them.\""+
                            "/speak Marlon \"Could you bring it over here, @? I'd like to have a closer look.\""+
                            "/move farmer 1 0 0 /move farmer 0 -1 0 /pause 1000 /move farmer 0 1 0 /pause 1000" +
                            "/speak Marlon \"As I suspected, these are definitely guthixian runestones. Or rather, they contain guthixian runestones.\"" +
                            "/speak Marlon \"Ol' Ras used to spend hours trying to crack these things open, until I showed up. Turns out a strike with the trusty hammer does the job in seconds.\""+
                            "/move Marlon 0 1 2" +
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
                            //This removes the mail if it overlaps with another mail - such as if a new mail is added on a date the mod uses
                            if (!mailDict.ContainsKey(mail.Key)) //TODO make this change the date
                            {
                                mailDict.Add(mail.Key, mail.Value);
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
                                                                   " " + ModAssets.modItems.Select(x => x.id).Join(null," ");
                        }
                        else if (!characterName.Contains("Universal_")) //We only accept non universals for this so far
                        {
                            Dictionary<int,PrefType> itemPreferences = 
                                ModAssets.modItems.Where(x=>x.characterPreferences.Keys.Contains(characterName))
                                    .ToDictionary(i=>i.id,j=> j.characterPreferences[characterName]); //get a dictionary of gifts for this character with their preference
                            
                            if(!itemPreferences.Any()) {continue;} //If we have no data to assign, skip this entirely
                            
                            //If all treasures are the same for a treasureitem, then we consider it the same as gifting the item itself
                            //I.e Fire rune packs will have the same gift value as a fire rune
                            Dictionary<int, PrefType> treasureItems =
                                ModAssets.modItems.Where(x =>
                                    x is PackObject pack && ModAssets.modItems.First(z=>z.id == pack.packItem).characterPreferences.ContainsKey(characterName))
                                    .ToDictionary(i=>i.id,j=> ModAssets.modItems.First(k=>k.id == ((PackObject)j).packItem).characterPreferences[characterName]);

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

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady)
            {
                return;
            }

            if (e.Button == SButton.F5)
            {
                for (int i = 4291; i < 4303; i++)
                {
                    StardewValley.Object item = ItemRegistry.Create<StardewValley.Object>($"{i}");
                    item.Stack = 20;
                    Game1.player.addItemToInventory(item);
                }
            }

            if (e.Button == SButton.F6)
            {
                foreach (StaffWeaponData newWeapon in ModAssets.staffWeapons)
                {
                    MeleeWeapon item = ItemRegistry.Create<MeleeWeapon>(newWeapon.id.ToString());
                    Game1.player.addItemToInventory(item);
                }
            }
            
            if (e.Button == SButton.F7)
            {
                Game1.player.modData["TofuMagicLevel"] = "0";
                Game1.player.modData["TofuMagicExperience"] = "0";
                Game1.player.modData["TofuMagicProfession1"] = "-1";
                Game1.player.modData["TofuMagicProfession2"] = "-1";
                Game1.player.modData["HasUnlockedMagic"] = "0";
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
                    Instance.Monitor.Log($"Farmer: {farmerRoot.Name}",LogLevel.Warn);
                    Instance.Monitor.Log($"Exp: {farmerRoot.modData["TofuMagicExperience"]}",LogLevel.Warn);
                }
            }
            
            if (e.Button == SButton.F10)
            {
                Instance.Monitor.Log($"Farmer X:{Game1.player.Tile.X} Y:{Game1.player.Tile.Y}");
                /*
                Game1.player.modData["HasUnlockedMagic"] = "1";
                Game1.player.changeFriendship(10000, Game1.getCharacterFromName("Wizard"));
                for (int i = 4359; i < 4370; i++)
                {
                    StardewValley.Object item = ItemRegistry.Create<StardewValley.Object>($"{i}");
                    item.stack.Value = 20;
                    Game1.player.addItemToInventory(item);
                }
                */
            }
            
            if (e.Button == SButton.F11)
            {
                Game1.player.mailReceived.Add("RSRunesFound");
                Game1.warpFarmer(Game1.getLocationRequest("ArchaeologyHouse"),3,14,2);
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
        
        //TODO check for and remove forge + enchant ability from staves
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
                    int level = staffWeaponData.providesRune != -1 ? 10 : int.Parse(staffWeaponData.id.ToString()) - 4343;
                    
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
                        Utility.drawTextWithShadow(spriteBatch, $"{ModAssets.modItems.First(x=>x.id == staffWeaponData.providesRune).DisplayName}", font,
                            new Vector2(x + 16 + 52, y + 16 + 12), c3 * 0.9f * alpha);
                        y += extraSize;
                    }
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
                if (Game1.gameMode == 3)
                {
                    if (Game1.gameModeTicks == 1)
                    {
                        ModAssets.localFarmerData.FirstGameTick();
                    }
                    
                    if (!Game1.player.modData.ContainsKey("TofuMagicLevel"))
                    {
                        Game1.player.modData.Add("TofuMagicLevel","0");
                        Game1.player.modData.Add("TofuMagicExperience","0");
                        Game1.player.modData.Add("TofuMagicProfession1","-1");
                        Game1.player.modData.Add("TofuMagicProfession2","-1");
                        Game1.player.modData.Add("HasUnlockedMagic","0");
                        ModAssets.localFarmerData.FirstGameTick();
                    }
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
                if (ModAssets.monsterDrops.ContainsKey(name))
                {
                    foreach (ItemDrop item in ModAssets.monsterDrops[name])
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
                if (!__instance.hasOrWillReceiveMail("RSRunesFound") && ModAssets.modItems.Any(x=> (x is PackObject || x.id == 4359 || x.id == 4360) && x.id.ToString() == item.ItemId))
                {
                    __instance.mailReceived.Add("RSRunesFound");
                    __instance.holdUpItemThenMessage(item,countAdded);
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
                if (ModAssets.modItems.Any(x=> (x is PackObject || x.id == 4359 || x.id == 4360) && x.id.ToString() == item.ItemId))
                {
                    Game1.drawObjectDialogue(new List<string> { "Your hands tingle as you pick up the mysterious object. Maybe the archaeologist will know something about this?"});
                    who.completelyStopAnimatingOrDoingAction();
                    return false;
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
    }
}