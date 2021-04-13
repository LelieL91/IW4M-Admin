﻿using SharedLibraryCore;
using System;
using Data.Models;

namespace IW4MAdmin.Plugins.Stats.Cheat
{
    class Strain
    {
        private const double StrainDecayBase = 0.9;
        private double CurrentStrain;
        public double LastDistance { get; private set; }
        public Vector3 LastAngle { get; private set; }
        public double LastDeltaTime { get; private set; }

        public double GetStrain(double killDistance, Vector3 newAngle, double deltaTime)
        {
            if (LastAngle == null)
                LastAngle = newAngle;

            LastDeltaTime = deltaTime;

            double decayFactor = GetDecay(deltaTime);
            CurrentStrain *= decayFactor;
            double[] distance = Utilities.AngleStuff(newAngle, LastAngle);
            LastDistance = distance[0] + distance[1];

            // this happens on first kill
            if ((distance[0] == 0 && distance[1] == 0) ||
                deltaTime == 0 ||
                double.IsNaN(CurrentStrain))
            {
                return CurrentStrain;
            }

            double newStrain = Math.Pow(LastDistance, 0.99) / deltaTime;
            newStrain *= killDistance / 1000.0;

            CurrentStrain += newStrain;

            LastAngle = newAngle;
            return CurrentStrain;
        }

        private double GetDecay(double deltaTime) => Math.Pow(StrainDecayBase, Math.Pow(2.0, deltaTime / 250.0) / 1000.0);
    }
}