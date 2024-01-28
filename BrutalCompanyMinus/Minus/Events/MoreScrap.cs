﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace BrutalCompanyMinus.Minus.Events
{
    internal class MoreScrap : MEvent
    {
        public override string Name() => nameof(MoreScrap);

        public override void Initalize()
        {
            Weight = 2;
            Description = "There is slighly more scrap in the facility!";
            ColorHex = "#008000";
            Type = EventType.Good;

            ScaleList.Add(ScaleType.ScrapAmount, new Scale(1.2f, 0.004f));
        }

        public override void Execute()
        {
            Manager.scrapAmountMultiplier *= Getf(ScaleType.ScrapAmount);
        }
    }
}