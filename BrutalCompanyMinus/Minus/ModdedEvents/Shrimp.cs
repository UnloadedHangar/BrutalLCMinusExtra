﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace BrutalCompanyMinus.Minus.Events
{
    internal class Shrimp : MEvent
    {
        public override string Name() => nameof(Shrimp);

        public static Shrimp Instance;

        public override void Initalize()
        {
            Instance = this;

            Weight = 3;
            Descriptions = new List<string>() { "Shrimp", "Actual doggo", "You have to feed it..." };
            ColorHex = "#FF0000";
            Type = EventType.Bad;

            monsterEvents = new List<MonsterEvent>() { new MonsterEvent(
                "ShrimpEnemy",
                new Scale(25.0f, 0.417f, 25.0f, 50.0f),
                new Scale(4.0f, 0.134f, 4.0f, 12.0f),
                new Scale(1.0f, 0.034f, 1.0f, 3.0f),
                new Scale(1.0f, 0.05f, 1.0f, 4.0f),
                new Scale(0.0f, 0.05f, 0.0f, 1.0f),
                new Scale(0.0f, 0.084f, 0.0f, 2.0f))
            };
        }

        public override bool AddEventIfOnly() => Compatibility.lcOfficePresent;

        public override void Execute() => ExecuteAllMonsterEvents();
    }
}