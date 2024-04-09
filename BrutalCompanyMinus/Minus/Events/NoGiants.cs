﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace BrutalCompanyMinus.Minus.Events
{
    internal class NoGiants : MEvent
    {
        public override string Name() => nameof(NoGiants);

        public static NoGiants Instance;

        public override void Initalize()
        {
            Instance = this;

            Weight = 1;
            Descriptions = new List<string>() { "No stomping", "Eddie hall isn't allowed here", "No creature's with an IQ of a toddler here, I hope." };
            ColorHex = "#008000";
            Type = EventType.Remove;

            EventsToRemove = new List<string>() { nameof(ForestGiant), nameof(Hell) };
        }

        public override bool AddEventIfOnly() => Manager.SpawnExists(Assets.EnemyName.ForestKeeper) || Manager.SpawnExists("PinkGiantObj");

        public override void Execute()
        {
            Manager.RemoveSpawn(Assets.EnemyName.ForestKeeper);
            Manager.RemoveSpawn("PinkGiantObj");
        }
    }
}
