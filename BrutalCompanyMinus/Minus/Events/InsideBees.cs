﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace BrutalCompanyMinus.Minus.Events
{
    internal class InsideBees : MEvent
    {
        public override string Name() => nameof(InsideBees);

        public static InsideBees Instance;

        public override void Initalize()
        {
            Instance = this;

            Weight = 1;
            Descriptions = new List<string>() { "BEES!! wait...", "The facility is abuzz!", "Bee careful", "The inside is sweet", "Why was the bee fired from the barbershop? He only knew how to give a buzz cut." };
            ColorHex = "#800000";
            Type = EventType.VeryBad;

            EventsToSpawnWith = new List<string>() { nameof(Roomba) };

            ScaleList.Add(ScaleType.MinInsideEnemy, new Scale(2.0f, 0.05f, 2.0f, 5.0f));
            ScaleList.Add(ScaleType.MaxInsideEnemy, new Scale(3.0f, 0.084f, 3.0f, 8.0f));
        }

        public override void Execute() => Manager.Spawn.InsideEnemies(Assets.GetEnemy(Assets.EnemyName.CircuitBee), UnityEngine.Random.Range(Get(ScaleType.MinInsideEnemy), Get(ScaleType.MaxInsideEnemy) + 1), 10.0f);
    }
}
