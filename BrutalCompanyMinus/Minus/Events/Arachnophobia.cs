﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace BrutalCompanyMinus.Minus.Events
{
    internal class Arachnophobia : MEvent
    {
        public override string Name() => nameof(Arachnophobia);

        public static Arachnophobia Instance;

        public override void Initalize()
        {
            Instance = this;

            Weight = 1;
            Descriptions = new List<string>() { "Nightmare facility", "I recommend bringing a vacuum cleaner.", "You are going to want to burn this facility down.", "I wish Zeeker's added a flamethrower." };
            ColorHex = "#800000";
            Type = EventType.VeryBad;

            EventsToRemove = new List<string>() { nameof(Trees), nameof(LeaflessBrownTrees), nameof(Spiders) };
            EventsToSpawnWith = new List<string>() { nameof(LeaflessTrees) };

            ScaleList.Add(ScaleType.InsideEnemyRarity, new Scale(50.0f, 0.84f, 50.0f, 100.0f));
            ScaleList.Add(ScaleType.OutsideEnemyRarity, new Scale(10.0f, 0.34f, 10.0f, 30.0f));
            ScaleList.Add(ScaleType.MinInsideEnemy, new Scale(4.0f, 0.084f, 3.0f, 9.0f));
            ScaleList.Add(ScaleType.MaxInsideEnemy, new Scale(6.0f, 0.134f, 4.0f, 14.0f));
            ScaleList.Add(ScaleType.MinOutsideEnemy, new Scale(2.0f, 0.034f, 1.0f, 4.0f));
            ScaleList.Add(ScaleType.MaxOutsideEnemy, new Scale(3.0f, 0.05f, 3.0f, 6.0f));
        }

        public override void Execute()
        {
            EnemyType BunkerSpider = Assets.GetEnemy(Assets.EnemyName.BunkerSpider);

            Manager.AddEnemyToPoolWithRarity(ref RoundManager.Instance.currentLevel.Enemies, BunkerSpider, Get(ScaleType.InsideEnemyRarity));
            Manager.AddEnemyToPoolWithRarity(ref RoundManager.Instance.currentLevel.OutsideEnemies, BunkerSpider, Get(ScaleType.OutsideEnemyRarity));

            Manager.Spawn.OutsideEnemies(BunkerSpider, UnityEngine.Random.Range(Get(ScaleType.MinOutsideEnemy), Get(ScaleType.MaxOutsideEnemy) + 1));
            Manager.Spawn.InsideEnemies(BunkerSpider, UnityEngine.Random.Range(Get(ScaleType.MinInsideEnemy), Get(ScaleType.MaxInsideEnemy) + 1));
        }
    }
}
