﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace BrutalCompanyMinus.Minus.Events
{
    internal class NoLandmines : MEvent
    {
        public override string Name() => nameof(NoLandmines);

        public override void Initalize()
        {
            Weight = 1;
            Description = "No landmines";
            ColorHex = "#008000";
            Type = EventType.Remove;

            EventsToRemove = new List<string>() { nameof(Landmines), nameof(OutsideLandmines), nameof(Warzone) };
        }

        public override void Execute()
        {
            AnimationCurve curve = new AnimationCurve(new Keyframe(0f, 0f));

            foreach (SpawnableMapObject obj in RoundManager.Instance.currentLevel.spawnableMapObjects)
            {
                if (obj.prefabToSpawn.name == Assets.GetObject(Assets.ObjectName.Landmine).name)
                {
                    obj.numberToSpawn = curve;
                }
            }
        }

    }
}
