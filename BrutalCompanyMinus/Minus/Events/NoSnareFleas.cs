﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace BrutalCompanyMinus.Minus.Events
{
    internal class NoSnareFleas : MEvent
    {
        public override string Name() => nameof(NoSnareFleas);

        public override void Initalize()
        {
            Weight = 1;
            Description = "No Ceiling campers!";
            ColorHex = "#008000";
            Type = EventType.Remove;

            EventsToRemove = new List<string>() { nameof(SnareFleas) };
        }

        public override bool AddEventIfOnly() => Manager.SpawnExists(Assets.EnemyNameList[Assets.EnemyName.SnareFlea]);

        public override void Execute() => Manager.RemoveSpawn(Assets.EnemyNameList[Assets.EnemyName.SnareFlea]);
    }
}