﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace BrutalCompanyMinus.Minus.Events
{
    internal class SmallDeilvery : MEvent
    {
        public override string Name() => nameof(SmallDeilvery);

        public override void Initalize()
        {
            Weight = 2;
            Description = "The company has decided to give you a present.";
            ColorHex = "#008000";
            Type = EventType.Good;

            ScaleList.Add(ScaleType.MinItemAmount, new Scale(2.0f, 0.05f));
            ScaleList.Add(ScaleType.MaxItemAmount, new Scale(3.0f, 0.08f));
            ScaleList.Add(ScaleType.MaxValue, new Scale(25.0f, 5.0f));
        }

        public override void Execute() => Manager.DeliverRandomItems(UnityEngine.Random.Range(Get(ScaleType.MinItemAmount), Get(ScaleType.MaxItemAmount) + 1), Get(ScaleType.MaxValue));
    }
}
