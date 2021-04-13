﻿using Data.Models;

namespace IW4MAdmin.Plugins.Stats.Cheat
{
    public class DetectionPenaltyResult
    {
        public Detection.DetectionType Type { get; set; }
        public EFPenalty.PenaltyType ClientPenalty { get; set; }
        public double Value { get; set; }
        public IW4Info.HitLocation Location { get; set; }
        public int HitCount { get; set; }
    }
}
