﻿using BepInEx;
using BepInEx.Configuration;
using BrutalCompanyMinus.Minus;
using BrutalCompanyMinus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Events;
using static BrutalCompanyMinus.Minus.MEvent;
using System.Reflection.Emit;
using static UnityEngine.EventSystems.EventTrigger;
using MonoMod.Utils;
using System.Diagnostics;
using HarmonyLib;
using System.Numerics;
using UnityEngine;
using System.Collections.Concurrent;
using System.Globalization;
using BrutalCompanyMinus.Minus.Events;
using System.Reflection;

namespace BrutalCompanyMinus
{
    [HarmonyPatch]
    internal class Configuration
    {

        public static ConfigFile uiConfig, eventConfig, weatherConfig, customAssetsConfig, difficultyConfig, moddedEventConfig, customEventConfig;

        public static List<ConfigFile> levelConfigs = new List<ConfigFile>();

        public static List<ConfigEntry<int>> eventWeights = new List<ConfigEntry<int>>();
        public static List<List<string>> eventDescriptions = new List<List<string>>();
        public static List<ConfigEntry<string>> eventColorHexes = new List<ConfigEntry<string>>();
        public static List<ConfigEntry<MEvent.EventType>> eventTypes = new List<ConfigEntry<MEvent.EventType>>();
        public static List<Dictionary<ScaleType, Scale>> eventScales = new List<Dictionary<ScaleType, Scale>>();
        public static List<ConfigEntry<bool>> eventEnables = new List<ConfigEntry<bool>>();
        public static List<List<string>>
            eventsToRemove = new List<List<string>>(),
            eventsToSpawnWith = new List<List<string>>();

        public static ConfigEntry<bool> useCustomWeights, showEventsInChat;
        public static Scale eventsToSpawn;
        public static ConfigEntry<float> goodEventIncrementMultiplier, badEventIncrementMultiplier;
        public static float[] weightsForExtraEvents;

        public static ConfigEntry<bool> useWeatherMultipliers, randomizeWeatherMultipliers, enableTerminalText;

        public static Scale[] eventTypeScales = new Scale[6];
        public static ConfigEntry<float> weatherRandomRandomMinInclusive, weatherRandomRandomMaxInclusive;

        public static Weather noneMultiplier, dustCloudMultiplier, rainyMultiplier, stormyMultiplier, foggyMultiplier, floodedMultiplier, eclipsedMultiplier;

        public static ConfigEntry<string> UIKey;
        public static ConfigEntry<bool> NormaliseScrapValueDisplay, EnableUI, ShowUILetterBox, ShowExtraProperties, PopUpUI, DisplayUIAfterShipLeaves;
        public static ConfigEntry<float> UITime;

        public static ConfigEntry<bool> enableQuotaChanges;
        public static ConfigEntry<int> deadLineDaysAmount, startingCredits, startingQuota, baseIncrease, increaseSteepness;
        public static Scale
            spawnChanceMultiplierScaling = new Scale(), insideEnemyMaxPowerCountScaling = new Scale(), outsideEnemyPowerCountScaling = new Scale(), enemyBonusHpScaling = new Scale(), spawnCapMultiplier = new Scale(),
            scrapAmountMultiplier = new Scale(), scrapValueMultiplier = new Scale(), insideSpawnChanceAdditive = new Scale(), outsideSpawnChanceAdditive = new Scale();
        public static ConfigEntry<bool> ignoreScaling;

        public static Dictionary<string, Dictionary<string, int>>  // Level name => Enemy/Scrap name => Rarity
            insideEnemyRarityList = new Dictionary<string, Dictionary<string, int>>(), 
            outsideEnemyRarityList = new Dictionary<string, Dictionary<string, int>>(),
            daytimeEnemyRarityList = new Dictionary<string, Dictionary<string, int>>(),
            scrapRarityList = new Dictionary<string, Dictionary<string, int>>();

        // Custom assets
        public static ConfigEntry<int> nutSlayerLives, nutSlayerHp;
        public static ConfigEntry<float> nutSlayerMovementSpeed;
        public static ConfigEntry<bool> nutSlayerImmortal;

        public static ConfigEntry<int>
            slayerShotgunMinValue, slayerShotgunMaxValue;

        public static CultureInfo en = new CultureInfo("en-US"); // This is important, no touchy

        public static void Initalize()
        {
            uiConfig = new ConfigFile(Paths.ConfigPath + "\\BrutalCompanyMinus\\UI_Settings.cfg", true);
            difficultyConfig = new ConfigFile(Paths.ConfigPath + "\\BrutalCompanyMinus\\Difficulty_Settings.cfg", true);
            eventConfig = new ConfigFile(Paths.ConfigPath + "\\BrutalCompanyMinus\\Events.cfg", true);
            weatherConfig = new ConfigFile(Paths.ConfigPath + "\\BrutalCompanyMinus\\Weather_Settings.cfg", true);
            customAssetsConfig = new ConfigFile(Paths.ConfigPath + "\\BrutalCompanyMinus\\Enemy_Scrap_Weights_Settings.cfg", true);
            moddedEventConfig = new ConfigFile(Paths.ConfigPath + "\\BrutalCompanyMinus\\ModdedEvents.cfg", true);
            customEventConfig = new ConfigFile(Paths.ConfigPath + "\\BrutalCompanyMinus\\CustomEvents.cfg", true);

            // Event settings
            useCustomWeights = difficultyConfig.Bind("_Event Settings", "Use custom weights?", false, "'false'= Use eventType weights to set all the weights     'true'= Use custom set weights");
            eventsToSpawn = getScale(difficultyConfig.Bind("_Event Settings", "Event amount scale", "2, 0.034, 2.0, 4.0", "The base amount of events   Format: BaseScale, IncrementScale, MinCap, MaxCap,   Forumla: BaseScale + (IncrementScale * DaysPassed)").Value);
            weightsForExtraEvents = ParseValuesFromString(difficultyConfig.Bind("_Event Settings", "Weights for extra events.", "40, 40, 15, 5", "Weights for extra events, can be expanded. (40, 40, 15, 5) is equivalent to (+0, +1, +2, +3) events").Value);
            showEventsInChat = difficultyConfig.Bind("_Event Settings", "Will Minus display events in chat?", false);

            // eventType weights
            eventTypeScales = new Scale[6]
            {
                getScale(difficultyConfig.Bind("_EventType Weights", "VeryBad event weight scale", "10, 0.34, 10, 30", "Format: BaseScale, IncrementScale, MinCap, MaxCap,   Forumla: BaseScale + (IncrementScale * DaysPassed)").Value),
                getScale(difficultyConfig.Bind("_EventType Weights", "Bad event weight scale", "40, -0.34, 20, 40", "Format: BaseScale, IncrementScale, MinCap, MaxCap,   Forumla: BaseScale + (IncrementScale * DaysPassed)").Value),
                getScale(difficultyConfig.Bind("_EventType Weights", "Neutral event weight scale", "15, 0, 15, 15", "Format: BaseScale, IncrementScale, MinCap, MaxCap,   Forumla: BaseScale + (IncrementScale * DaysPassed)").Value),
                getScale(difficultyConfig.Bind("_EventType Weights", "Good event weight scale", "18, 0, 18, 18", "Format: BaseScale, IncrementScale, MinCap, MaxCap,   Forumla: BaseScale + (IncrementScale * DaysPassed)").Value),
                getScale(difficultyConfig.Bind("_EventType Weights", "VeryGood event weight scale", "5, 0, 5, 5", "Format: BaseScale, IncrementScale, MinCap, MaxCap,   Forumla: BaseScale + (IncrementScale * DaysPassed)").Value),
                getScale(difficultyConfig.Bind("_EventType Weights", "Remove event weight scale", "12, 0, 12, 12", "These events remove something   Format: BaseScale, IncrementScale, MinCap, MaxCap,   Forumla: BaseScale + (IncrementScale * DaysPassed)").Value)
            };

            // Difficulty scaling
            ignoreScaling = difficultyConfig.Bind("Difficulty Scaling", "Ignore scaling?", false, "Mod wont scale anymore if true.");
            spawnChanceMultiplierScaling = getScale(difficultyConfig.Bind("Difficulty Scaling", "Spawn chance multiplier scale", "1.0, 0.017, 1.0, 2.0", "This will multiply the spawn chance by this,   Format: BaseScale, IncrementScale, MinCap, MaxCap,   Forumla: BaseScale + (IncrementScale * DaysPassed)").Value);
            insideSpawnChanceAdditive = getScale(difficultyConfig.Bind("Difficulty Scaling", "Inside spawn chance additive", "0.0, 0.0, 0.0, 0.0", "This will add to all keyframes for insideSpawns on the animationCurve,   Format: BaseScale, IncrementScale, MinCap, MaxCap,   Forumla: BaseScale + (IncrementScale * DaysPassed)").Value);
            outsideSpawnChanceAdditive = getScale(difficultyConfig.Bind("Difficulty Scaling", "Outside spawn chance additive", "0.0, 0.0, 0.0, 0.0", "This will add to all keyframes for outsideSpawns on the animationCurve,   Format: BaseScale, IncrementScale, MinCap, MaxCap,   Forumla: BaseScale + (IncrementScale * DaysPassed)").Value);
            spawnCapMultiplier = getScale(difficultyConfig.Bind("Difficulty Scaling", "Spawn cap multipler scale", "1.0, 0.017, 1.0, 2.0", "This will multiply outside and inside power counts by this,   Format: BaseScale, IncrementScale, MinCap, MaxCap,   Forumla: BaseScale + (IncrementScale * DaysPassed)").Value);
            insideEnemyMaxPowerCountScaling = getScale(difficultyConfig.Bind("Difficulty Scaling", "Additional Inside Max Enemy Power Count", "0, 0, 0, 0", "Added max enemy power count for inside enemies.,   Format: BaseScale, IncrementScale, MinCap, MaxCap,   Forumla: BaseScale + (IncrementScale * DaysPassed)").Value);
            outsideEnemyPowerCountScaling = getScale(difficultyConfig.Bind("Difficulty Scaling", "Additional Outside Max Enemy Power Count", "0, 0, 0, 0", "Added max enemy power count for outside enemies.,   Format: BaseScale, IncrementScale, MinCap, MaxCap,   Forumla: BaseScale + (IncrementScale * DaysPassed)").Value);
            enemyBonusHpScaling = getScale(difficultyConfig.Bind("Difficulty Scaling", "Additional hp", "0, 0.084, 0, 5", "Added hp to all enemies,   Format: BaseScale, IncrementScale, MinCap, MaxCap,   Forumla: BaseScale + (IncrementScale * DaysPassed)").Value);
            scrapValueMultiplier = getScale(difficultyConfig.Bind("Difficulty Scaling", "Global scrap value multiplier scale", "1.0, 0.0, 1.0, 1.0", "Mutliplies scrap value,   Format: BaseScale, IncrementScale, MinCap, MaxCap,   Forumla: BaseScale + (IncrementScale * DaysPassed)").Value);
            scrapAmountMultiplier = getScale(difficultyConfig.Bind("Difficulty Scaling", "Global scrap amount multiplier scale", "1.0, 0.0, 1.0, 1.0", "Mutliplies scrap value,   Format: BaseScale, IncrementScale, MinCap, MaxCap,   Forumla: BaseScale + (IncrementScale * DaysPassed)").Value);
            goodEventIncrementMultiplier = difficultyConfig.Bind("Difficulty Scaling", "Global multiplier for increment value on good and veryGood eventTypes.", 1.0f);
            badEventIncrementMultiplier = difficultyConfig.Bind("Difficulty Scaling", "Global multiplier for increment value on bad and veryBad eventTypes.", 1.0f);


            // Custom scrap settings
            nutSlayerLives = customAssetsConfig.Bind("NutSlayer", "Lives", 5, "If hp reaches zero or below, decrement lives and reset hp until 0 lives.");
            nutSlayerHp = customAssetsConfig.Bind("NutSlayer", "Hp", 6);
            nutSlayerMovementSpeed = customAssetsConfig.Bind("NutSlayer", "Speed?", 9.5f);
            nutSlayerImmortal = customAssetsConfig.Bind("NutSlayer", "Immortal?", true);
            Assets.grabbableTurret.minValue = customAssetsConfig.Bind("Grabbable Landmine", "Min value", 50).Value;
            Assets.grabbableTurret.maxValue = customAssetsConfig.Bind("Grabbable Landmine", "Max value", 75).Value;
            Assets.grabbableLandmine.minValue = customAssetsConfig.Bind("Grabbable Turret", "Min value", 100).Value;
            Assets.grabbableLandmine.maxValue = customAssetsConfig.Bind("Grabbable Turret", "Max value", 150).Value;
            slayerShotgunMinValue = customAssetsConfig.Bind("Slayer Shotgun", "Min value", 200);
            slayerShotgunMaxValue = customAssetsConfig.Bind("Slayer Shotgun", "Max value", 300);

            // Weather settings
            useWeatherMultipliers = weatherConfig.Bind("_Weather Settings", "Enable weather multipliers?", true, "'false'= Disable all weather multipliers     'true'= Enable weather multipliers");
            randomizeWeatherMultipliers = weatherConfig.Bind("_Weather Settings", "Weather multiplier randomness?", false, "'false'= disable     'true'= enable");
            enableTerminalText = weatherConfig.Bind("_Weather Settings", "Enable terminal text?", true);

            // Weather Random settings
            weatherRandomRandomMinInclusive = weatherConfig.Bind("_Weather Random Multipliers", "Min Inclusive", 0.9f, "Lower bound of random value");
            weatherRandomRandomMaxInclusive = weatherConfig.Bind("_Weather Random Multipliers", "Max Inclusive", 1.2f, "Upper bound of random value");

            // Weather multipliers settings
            Weather createWeatherSettings(Weather weather)
            {
                string configHeader = "_(" + weather.weatherType.ToString() + ") Weather multipliers";

                float valueMultiplierSetting = weatherConfig.Bind(configHeader, "Value Multiplier", weather.scrapValueMultiplier, "Multiply Scrap value for " + weather.weatherType.ToString()).Value;
                float amountMultiplierSetting = weatherConfig.Bind(configHeader, "Amount Multiplier", weather.scrapAmountMultiplier, "Multiply Scrap amount for " + weather.weatherType.ToString()).Value;
                float sizeMultiplerSetting = weatherConfig.Bind(configHeader, "Factory Size Multiplier", weather.factorySizeMultiplier, "Multiply Factory size for " + weather.weatherType.ToString()).Value;

                return new Weather(weather.weatherType, valueMultiplierSetting, amountMultiplierSetting, sizeMultiplerSetting);
            }

            noneMultiplier = createWeatherSettings(new Weather(LevelWeatherType.None, 1.00f, 1.00f, 1.00f));
            dustCloudMultiplier = createWeatherSettings(new Weather(LevelWeatherType.DustClouds, 1.10f, 1.05f, 1.00f));
            rainyMultiplier = createWeatherSettings(new Weather(LevelWeatherType.Rainy, 1.10f, 1.05f, 1.00f));
            stormyMultiplier = createWeatherSettings(new Weather(LevelWeatherType.Stormy, 1.4f, 1.2f, 1.00f));
            foggyMultiplier = createWeatherSettings(new Weather(LevelWeatherType.Foggy, 1.2f, 1.10f, 1.00f));
            floodedMultiplier = createWeatherSettings(new Weather(LevelWeatherType.Flooded, 1.3f, 1.15f, 1.00f));
            eclipsedMultiplier = createWeatherSettings(new Weather(LevelWeatherType.Eclipsed, 1.5f, 1.25f, 1.00f));

            // UI Settings
            UIKey = uiConfig.Bind("UI Options", "Toggle UI Key", "K");
            NormaliseScrapValueDisplay = uiConfig.Bind("UI Options", "Normlize scrap value display number?", true, "In game default value is 0.4, having this set to true will multiply the 'displayed value' by 2.5 so it looks normal.");
            EnableUI = uiConfig.Bind("UI Options", "Enable UI?", true);
            ShowUILetterBox = uiConfig.Bind("UI Options", "Display UI Letter Box?", true);
            ShowExtraProperties = uiConfig.Bind("UI Options", "Display extra properties", true, "Display extra properties on UI such as scrap value and amount multipliers.");
            PopUpUI = uiConfig.Bind("UI Options", "PopUp UI?", true, "Will the UI popup whenever you start the day?");
            UITime = uiConfig.Bind("UI Options", "PopUp UI time.", 45.0f, "UI popup time");
            DisplayUIAfterShipLeaves = uiConfig.Bind("UI Options", "Display UI after ship leaves?", false, "Will only display event's after ship has left.");

            // Events
            void RegisterEvents(ConfigFile toConfig, List<MEvent> events)
            {
                // Event settings
                foreach (MEvent e in events)
                {
                    eventWeights.Add(toConfig.Bind(e.Name(), "Custom Weight", e.Weight, "If you want to use custom weights change 'Use custom weights'? setting in '__Event Settings' to true."));
                    eventDescriptions.Add(ListToDescriptions(toConfig.Bind(e.Name(), "Descriptions", StringsToList(e.Descriptions, "|"), "Seperated by |").Value));
                    eventColorHexes.Add(toConfig.Bind(e.Name(), "Color Hex", e.ColorHex));
                    eventTypes.Add(toConfig.Bind(e.Name(), "Event Type", e.Type));
                    eventEnables.Add(toConfig.Bind(e.Name(), "Event Enabled?", e.Enabled, "Setting this to false will stop the event from occuring.")); // Normal event

                    // Make scale list
                    Dictionary<ScaleType, Scale> scales = new Dictionary<ScaleType, Scale>();
                    foreach (KeyValuePair<ScaleType, Scale> obj in e.ScaleList)
                    {
                        scales.Add(obj.Key, getScale(toConfig.Bind(e.Name(), obj.Key.ToString(), $"{obj.Value.Base.ToString(en)}, {obj.Value.Increment.ToString(en)}, {obj.Value.MinCap.ToString(en)}, {obj.Value.MaxCap.ToString(en)}", ScaleInfoList[obj.Key] + "   Format: BaseScale, IncrementScale, MinCap, MaxCap,   Forumla: BaseScale + (IncrementScale * DaysPassed)").Value));
                    }
                    eventScales.Add(scales);

                    // EventsToRemove and EventsToSpawnWith
                    eventsToRemove.Add(ListToStrings(toConfig.Bind(e.Name(), "Events To Remove", StringsToList(e.EventsToRemove, ", "), "Will prevent said event(s) from occuring.").Value));
                    eventsToSpawnWith.Add(ListToStrings(toConfig.Bind(e.Name(), "Events To Spawn With", StringsToList(e.EventsToSpawnWith, ", "), "Will spawn said events(s).").Value));
                }
            }

            // Custom enemy events
            int customEventCount = customEventConfig.Bind("Temp Custom Monster Event Count", "How many events to generate in config?", 3, "This is temporary for the time being.").Value;
            for (int i = 0; i < customEventCount; i++)
            {
                MEvent e = new Minus.CustomEvents.CustomMonsterEvent();
                e.Enabled = false;
                EventManager.customEvents.Add(e);
            }

            RegisterEvents(eventConfig, EventManager.vanillaEvents);
            RegisterEvents(moddedEventConfig, EventManager.moddedEvents);
            RegisterEvents(customEventConfig, EventManager.customEvents);

            EventManager.events.AddRange(EventManager.vanillaEvents);
            EventManager.events.AddRange(EventManager.moddedEvents);
            EventManager.events.AddRange(EventManager.customEvents);

            // Specific event settings
            Minus.Handlers.FacilityGhost.actionTimeCooldown = eventConfig.Bind(nameof(FacilityGhost), "Normal Action Time Interval", 15.0f, "How often does it take for the ghost to make a decision?").Value;
            Minus.Handlers.FacilityGhost.ghostCrazyActionInterval = eventConfig.Bind(nameof(FacilityGhost), "Crazy Action Time Interval", 0.1f, "How often does it take for the ghost to make a decision while going crazy?").Value;
            Minus.Handlers.FacilityGhost.ghostCrazyPeriod = eventConfig.Bind(nameof(FacilityGhost), "Crazy Period", 5.0f, "How long will the ghost go crazy for?").Value;
            Minus.Handlers.FacilityGhost.crazyGhostChance = eventConfig.Bind(nameof(FacilityGhost), "Crazy Chance", 0.1f, "Whenever the ghost makes a decision, what is the chance that it will go crazy?").Value;
            Minus.Handlers.FacilityGhost.DoNothingWeight = eventConfig.Bind(nameof(FacilityGhost), "Do Nothing Weight", 50, "Whenever the ghost makes a decision, what is the weight to do nothing?").Value;
            Minus.Handlers.FacilityGhost.OpenCloseBigDoorsWeight = eventConfig.Bind(nameof(FacilityGhost), "Open and close big doors weight", 20, "Whenever the ghost makes a decision, what is the weight for ghost to open and close big doors?").Value;
            Minus.Handlers.FacilityGhost.MessWithLightsWeight = eventConfig.Bind(nameof(FacilityGhost), "Mess with lights weight", 16, "Whenever the ghost makes a decision, what is the weight to mess with lights?").Value;
            Minus.Handlers.FacilityGhost.MessWithBreakerWeight = eventConfig.Bind(nameof(FacilityGhost), "Mess with breaker weight", 4, "Whenever the ghost makes a decision, what is the weight to mess with the breaker?").Value;
            Minus.Handlers.FacilityGhost.OpenCloseDoorsWeight = eventConfig.Bind(nameof(FacilityGhost), "Open and close normal doors weight", 9, "Whenever the ghost makes a decision, what is the weight to attempt to open and close normal doors.").Value;
            Minus.Handlers.FacilityGhost.lockUnlockDoorsWeight = eventConfig.Bind(nameof(FacilityGhost), "Lock and unlock normal doors weight", 3, "Whenever the ghost makes a decision, what is the weight to attempt to lock and unlock normal doors.").Value;
            Minus.Handlers.FacilityGhost.chanceToOpenCloseDoor = eventConfig.Bind(nameof(FacilityGhost), "Chance to open and close normal doors", 0.3f, "Whenever the ghosts decides to open and close doors, what is the chance for each individual door that it will do that.").Value;
            Minus.Handlers.FacilityGhost.chanceToLockUnlockDoor = eventConfig.Bind(nameof(FacilityGhost), "Chance to lock and unlock normal doors", 0.1f, "Whenever the ghosts decides to lock and unlock doors, what is the chance for each individual door that it will do that.").Value;

            Minus.Handlers.RealityShift.normalScrapWeight = eventConfig.Bind(nameof(RealityShift), "Normal shift weight", 85, "Weight for transforming scrap into some other scrap?").Value;
            Minus.Handlers.RealityShift.grabbableLandmineWeight = eventConfig.Bind(nameof(RealityShift), "Grabbable landmine shift weight", 15, "Weight for transforming scrap into a grabbable landmine?").Value;
            Minus.Handlers.RealityShift.grabbableTurretWeight = eventConfig.Bind(nameof(RealityShift), "Grabbable turret shift weight", 0, "Weight for transforming scrap into a grabbable turret?").Value;

            Minus.Handlers.DDay.bombardmentInterval = eventConfig.Bind(nameof(Warzone), "Bombardment interval", 100, "The time it takes before each bombardment event.").Value;
            Minus.Handlers.DDay.bombardmentTime = eventConfig.Bind(nameof(Warzone), "Bombardment time", 15, "When a bombardment event occurs, how long will it last?").Value;
            Minus.Handlers.DDay.fireInterval = eventConfig.Bind(nameof(Warzone), "Fire interval", 1, "During a bombardment event how often will it fire?").Value;
            Minus.Handlers.DDay.fireAmount = eventConfig.Bind(nameof(Warzone), "Fire amount", 8, "For every fire interval, how many shot's will it take? This will get scaled higher on bigger maps.").Value;
            Minus.Handlers.DDay.displayWarning = eventConfig.Bind(nameof(Warzone), "Display warning?", true, "Display warning message before bombardment?").Value;
            Minus.Handlers.DDay.volume = eventConfig.Bind(nameof(Warzone), "Siren Volume?", 0.3f, "Volume of the siren? between 0.0 and 1.0").Value;
            Minus.Handlers.ArtilleryShell.speed = eventConfig.Bind(nameof(Warzone), "Artillery shell speed", 100.0f, "How fast does the artillery shell travel?").Value;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(TimeOfDay), "Awake")]
        private static void OnTimeDayStart(ref TimeOfDay __instance)
        {
            enableQuotaChanges = difficultyConfig.Bind("Quota Settings", "_Enable Quota Changes", false);
            if(enableQuotaChanges.Value)
            {
                __instance.quotaVariables.deadlineDaysAmount = difficultyConfig.Bind("Quota Settings", "Deadline Days Amount", __instance.quotaVariables.deadlineDaysAmount).Value;
                __instance.quotaVariables.startingCredits = difficultyConfig.Bind("Quota Settings", "Starting Credits", __instance.quotaVariables.startingCredits).Value;
                __instance.quotaVariables.startingQuota = difficultyConfig.Bind("Quota Settings", "Starting Quota", __instance.quotaVariables.startingQuota).Value;
                __instance.quotaVariables.baseIncrease = difficultyConfig.Bind("Quota Settings", "Base Increase", __instance.quotaVariables.baseIncrease).Value;
                __instance.quotaVariables.increaseSteepness = difficultyConfig.Bind("Quota Settings", "Increase Steepness", __instance.quotaVariables.increaseSteepness).Value;
            }
        }

        private static bool Initalized = false;
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Terminal), "Awake")]
        private static void OnTerminalAwake()
        {
            if (Initalized) return;

            // Initalize Events
            foreach (MEvent e in EventManager.vanillaEvents) e.Initalize();
            foreach (MEvent e in EventManager.moddedEvents) e.Initalize();

            Manager.currentTerminal = GameObject.FindObjectOfType<Terminal>();

            // Config
            Initalize();

            // Use config settings
            for (int i = 0; i != EventManager.events.Count; i++)
            {
                EventManager.events[i].Weight = eventWeights[i].Value;
                EventManager.events[i].Descriptions = eventDescriptions[i];
                EventManager.events[i].ColorHex = eventColorHexes[i].Value;
                EventManager.events[i].Type = eventTypes[i].Value;
                EventManager.events[i].ScaleList = eventScales[i];
                EventManager.events[i].Enabled = eventEnables[i].Value;
                EventManager.events[i].EventsToRemove = eventsToRemove[i];
                EventManager.events[i].EventsToSpawnWith = eventsToSpawnWith[i];
            }

            // Create disabled events list and update
            List<MEvent> newEvents = new List<MEvent>();
            foreach (MEvent e in EventManager.events)
            {
                if (!e.Enabled)
                {
                    EventManager.disabledEvents.Add(e);
                }
                else
                {
                    newEvents.Add(e);
                }
            }
            EventManager.events = newEvents;

            EventManager.UpdateEventTypeCounts();
            if (!useCustomWeights.Value) EventManager.UpdateAllEventWeights();

            Log.LogInfo(
                $"\n\nTotal Events:{EventManager.events.Count},   Disabled Events:{EventManager.disabledEvents.Count},   Total Events - Remove Count:{EventManager.events.Count - EventManager.eventTypeCount[5]}\n" +
                $"Very Bad:{EventManager.eventTypeCount[0]}\n" +
                $"Bad:{EventManager.eventTypeCount[1]}\n" +
                $"Neutral:{EventManager.eventTypeCount[2]}\n" +
                $"Good:{EventManager.eventTypeCount[3]}\n" +
                $"Very Good:{EventManager.eventTypeCount[4]}\n" +
                $"Remove:{EventManager.eventTypeCount[5]}\n");

            Initalized = true;
        }

        private static Scale getScale(string from)
        {
            float[] values = ParseValuesFromString(from);
            return new Scale(values[0], values[1], values[2], values[3]);
        }

        private static float[] ParseValuesFromString(string from)
        {
            return from.Split(',').Select(x => float.Parse(x, en)).ToArray();
        }

        private static string StringsToList(List<string> strings, string seperator)
        {
            string text = "";
            foreach (string s in strings)
            {
                text += s;
                text += seperator;
            }
            if (strings.Count > 0) text = text.Substring(0, text.Length - seperator.Length);
            return text;
        }

        private static List<string> ListToStrings(string text)
        {
            if (text.IsNullOrWhiteSpace()) return new List<string>();
            text = text.Replace(" ", "");
            return text.Split(',').ToList();
        }

        private static List<string> ListToDescriptions(string text)
        {
            if (text.IsNullOrWhiteSpace()) return new List<string>() { "" };
            return text.Split("|").ToList();
        }
    }
}
