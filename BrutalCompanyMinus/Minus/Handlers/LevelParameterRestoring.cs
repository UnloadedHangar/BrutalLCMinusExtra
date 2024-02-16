﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using JetBrains.Annotations;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

namespace BrutalCompanyMinus.Minus.Handlers
{
    [HarmonyPatch]
    internal class LevelParameterRestoring
    {
        internal static List<SpawnableItemWithRarity> levelScrap = new List<SpawnableItemWithRarity>();
        internal static int MinScrap = 0;
        internal static int MaxScrap = 0;

        internal static List<SpawnableEnemyWithRarity> insideEnemies = new List<SpawnableEnemyWithRarity>();
        internal static List<SpawnableEnemyWithRarity> outsideEnemies = new List<SpawnableEnemyWithRarity>();
        internal static List<SpawnableEnemyWithRarity> daytimeEnemies = new List<SpawnableEnemyWithRarity>();

        public static void StoreUnmodifiedParamaters(SelectableLevel currentLevel)
        {
            currentLevel.spawnableScrap.Clear(); currentLevel.spawnableScrap.AddRange(Assets.levelScrapList[Manager.GetLevelIndex()]);

            Log.LogInfo(string.Format("Storing un-modified level paramaters on level:{0}", currentLevel.name));
            // Store parameters before any changes made
            levelScrap.Clear(); levelScrap.AddRange(currentLevel.spawnableScrap);
            MinScrap = currentLevel.minScrap;
            MaxScrap = currentLevel.maxScrap;

            insideEnemies.Clear(); insideEnemies.AddRange(currentLevel.Enemies);
            outsideEnemies.Clear(); outsideEnemies.AddRange(currentLevel.OutsideEnemies);
            daytimeEnemies.Clear(); daytimeEnemies.AddRange(currentLevel.DaytimeEnemies);
        }


        private static bool modifiedEnemySpawns = false;
        public static void ModifyEnemyScrapSpawns(StartOfRound instance)
        {
            if (modifiedEnemySpawns) return;

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            Log.LogInfo("Modifying enemy map pool and scrap pool from config.");

            // Multi-thread this shit
            Parallel.For(0, instance.levels.Length, i =>
            {
                Log.LogInfo(string.Format("Modifying enemy map pool and scrap pool for {0} using config settings.", instance.levels[i].name));

                if (!Configuration.insideEnemyRarityList.TryGetValue(instance.levels[i].name, out _))
                {
                    Log.LogError(string.Format("Level {0} dosen't exist in dictionaries, skipping.", instance.levels[i].name));
                    return;
                }

                if (Configuration.customEnemyWeights.Value)
                {
                    instance.levels[i].Enemies.Clear();
                    instance.levels[i].OutsideEnemies.Clear();
                    instance.levels[i].DaytimeEnemies.Clear();
                }
                if (Configuration.customScrapWeights.Value) instance.levels[i].spawnableScrap.Clear();
                
                foreach (KeyValuePair<string, int> insideEnemy in Configuration.insideEnemyRarityList[instance.levels[i].name])
                {
                    EnemyType enemy = Assets.GetEnemy(insideEnemy.Key);
                    int rarity = insideEnemy.Value;
                    if (Configuration.enableAllEnemies.Value && rarity == 0 && !enemy.isOutsideEnemy) rarity = Configuration.allEnemiesDefaultWeight.Value;
                    if (rarity == 0) continue; // Skip Entry
                    instance.levels[i].Enemies.Add(new SpawnableEnemyWithRarity() { enemyType = enemy, rarity = rarity });
                }

                foreach (KeyValuePair<string, int> outsideEnemy in Configuration.outsideEnemyRarityList[instance.levels[i].name])
                {
                    EnemyType enemy = Assets.GetEnemy(outsideEnemy.Key);
                    int rarity = outsideEnemy.Value;
                    if (Configuration.enableAllEnemies.Value && rarity == 0 && enemy.isOutsideEnemy) rarity = Configuration.allEnemiesDefaultWeight.Value;
                    if (outsideEnemy.Value == 0) continue; // Skip Entry
                    instance.levels[i].OutsideEnemies.Add(new SpawnableEnemyWithRarity() { enemyType = enemy, rarity = rarity });
                }

                foreach (KeyValuePair<string, int> daytimeEnemy in Configuration.daytimeEnemyRarityList[instance.levels[i].name])
                {
                    EnemyType enemy = Assets.GetEnemy(daytimeEnemy.Key);
                    int rarity = daytimeEnemy.Value;
                    if (Configuration.enableAllEnemies.Value && rarity == 0 && enemy.isDaytimeEnemy) rarity = Configuration.allEnemiesDefaultWeight.Value;
                    if (daytimeEnemy.Value == 0) continue; // Skip Entry
                    instance.levels[i].DaytimeEnemies.Add(new SpawnableEnemyWithRarity() { enemyType = enemy, rarity = rarity });
                }

                foreach (KeyValuePair<string, int> scrap in Configuration.scrapRarityList[instance.levels[i].name])
                {
                    Item item = Assets.GetItem(scrap.Key);
                    int rarity = scrap.Value;
                    if (Configuration.enableAllScrap.Value && rarity == 0) rarity = Configuration.allScrapDefaultWeight.Value;
                    if (scrap.Value == 0) continue; // Skip Entry
                    instance.levels[i].spawnableScrap.Add(new SpawnableItemWithRarity() { spawnableItem = Assets.GetItem(scrap.Key), rarity = scrap.Value });
                }
                
            });

            stopWatch.Stop();
            Log.LogInfo(string.Format("Took {0}ms", stopWatch.ElapsedMilliseconds));

            modifiedEnemySpawns = true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(StartOfRound), "Start")]
        private static void onStartOfRoundStart()
        {
            modifiedEnemySpawns = false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(RoundManager), "waitForScrapToSpawnToSync")]
        public static void OnwaitForScrapToSpawnToSync(ref RoundManager __instance)
        {
            Log.LogInfo(string.Format("Restoring un-modified level paramaters on level:{0}", __instance.currentLevel.name));
            // Restore paramaters
            __instance.currentLevel.spawnableScrap.Clear(); __instance.currentLevel.spawnableScrap.AddRange(levelScrap); // Unmodified
            __instance.currentLevel.minScrap = MinScrap;
            __instance.currentLevel.maxScrap = MaxScrap;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(StartOfRound), "ShipLeave")]
        public static void OnShipLeave()
        {
            Log.LogInfo("Restoring un-modified level enemy spawns on current level.");
            // Restore parameters
            RoundManager.Instance.currentLevel.Enemies.Clear(); RoundManager.Instance.currentLevel.Enemies.AddRange(insideEnemies);
            RoundManager.Instance.currentLevel.OutsideEnemies.Clear(); RoundManager.Instance.currentLevel.OutsideEnemies.AddRange(outsideEnemies);
            RoundManager.Instance.currentLevel.DaytimeEnemies.Clear(); RoundManager.Instance.currentLevel.DaytimeEnemies.AddRange(daytimeEnemies);
        }
    }

    internal class _LevelParameterRestoring
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.SpawnScrapInLevel))] // This is bad dont do this
        private static IEnumerable<CodeInstruction> OnUpdateIL(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var code = new List<CodeInstruction>(instructions);

            code.Insert(0, new CodeInstruction(OpCodes.Ret));
            code.Insert(0, Transpilers.EmitDelegate(new Action(SpawnScrapInLevelCopy)));

            return code.AsEnumerable();
        }

        private static void SpawnScrapInLevelCopy() // This fix is stupid(no other choice but to do this) but it works
        {
            RoundManager r = RoundManager.Instance;

            r.currentLevel.spawnableScrap.Clear(); r.currentLevel.spawnableScrap.AddRange(Manager.ScrapToSpawn);

            int num = (int)((float)r.AnomalyRandom.Next(r.currentLevel.minScrap, r.currentLevel.maxScrap) * r.scrapAmountMultiplier);
            if (StartOfRound.Instance.isChallengeFile)
            {
                int num2 = r.AnomalyRandom.Next(10, 30);
                num += num2;
                Log.LogInfo($"Anomaly random 0b: {num2}");
            }
            List<Item> ScrapToSpawn = new List<Item>();
            List<int> list = new List<int>();
            int num3 = 0;
            List<int> list2 = new List<int>(r.currentLevel.spawnableScrap.Count);
            for (int j = 0; j < r.currentLevel.spawnableScrap.Count; j++)
            {
                if (j == r.increasedScrapSpawnRateIndex)
                {
                    list2.Add(100);
                }
                else
                {
                    list2.Add(r.currentLevel.spawnableScrap[j].rarity);
                }
            }
            int[] weights = list2.ToArray();
            for (int k = 0; k < num; k++)
            {
                ScrapToSpawn.Add(r.currentLevel.spawnableScrap[r.GetRandomWeightedIndex(weights)].spawnableItem);
            }
            Log.LogInfo($"Number of scrap to spawn: {ScrapToSpawn.Count}. minTotalScrapValue: {r.currentLevel.minTotalScrapValue}. Total value of items: {num3}.");
            RandomScrapSpawn randomScrapSpawn = null;
            RandomScrapSpawn[] source = UnityEngine.Object.FindObjectsOfType<RandomScrapSpawn>();
            List<NetworkObjectReference> list3 = new List<NetworkObjectReference>();
            List<RandomScrapSpawn> usedSpawns = new List<RandomScrapSpawn>();
            int i;
            for (i = 0; i < ScrapToSpawn.Count; i++)
            {
                if (ScrapToSpawn[i] == null)
                {
                    Log.LogInfo("Error!!!!! Found null element in list ScrapToSpawn. Skipping it.");
                    continue;
                }
                List<RandomScrapSpawn> list4 = ((ScrapToSpawn[i].spawnPositionTypes != null && ScrapToSpawn[i].spawnPositionTypes.Count != 0) ? source.Where((RandomScrapSpawn x) => ScrapToSpawn[i].spawnPositionTypes.Contains(x.spawnableItems) && !x.spawnUsed).ToList() : source.ToList());
                if (list4.Count <= 0)
                {
                    Log.LogInfo("No tiles containing a scrap spawn with item type: " + ScrapToSpawn[i].itemName);
                    continue;
                }
                if (usedSpawns.Count > 0 && list4.Contains(randomScrapSpawn))
                {
                    list4.RemoveAll((RandomScrapSpawn x) => usedSpawns.Contains(x));
                    if (list4.Count <= 0)
                    {
                        usedSpawns.Clear();
                        i--;
                        continue;
                    }
                }
                randomScrapSpawn = list4[r.AnomalyRandom.Next(0, list4.Count)];
                usedSpawns.Add(randomScrapSpawn);
                Vector3 position;
                if (randomScrapSpawn.spawnedItemsCopyPosition)
                {
                    randomScrapSpawn.spawnUsed = true;
                    position = randomScrapSpawn.transform.position;
                }
                else
                {
                    position = r.GetRandomNavMeshPositionInBoxPredictable(randomScrapSpawn.transform.position, randomScrapSpawn.itemSpawnRange, r.navHit, r.AnomalyRandom) + Vector3.up * ScrapToSpawn[i].verticalOffset;
                }
                GameObject obj = UnityEngine.Object.Instantiate(ScrapToSpawn[i].spawnPrefab, position, Quaternion.identity, r.spawnedScrapContainer);
                GrabbableObject component = obj.GetComponent<GrabbableObject>();
                component.transform.rotation = Quaternion.Euler(component.itemProperties.restingRotation);
                component.fallTime = 0f;
                list.Add((int)((float)r.AnomalyRandom.Next(ScrapToSpawn[i].minValue, ScrapToSpawn[i].maxValue) * r.scrapValueMultiplier));
                num3 += list[list.Count - 1];
                component.scrapValue = list[list.Count - 1];
                NetworkObject component2 = obj.GetComponent<NetworkObject>();
                component2.Spawn();
                list3.Add(component2);
            }
            r.StartCoroutine(Manager.Spawn.waitForScrapToSpawnToSync(list3.ToArray(), list.ToArray()));
        }

    }
}
