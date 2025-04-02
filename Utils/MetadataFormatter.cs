using System;
using System.Text;
using System.Collections.Generic;
using FunscriptPreviewHandler.Models;

namespace FunscriptPreviewHandler.Utils
{
    public static class MetadataFormatter
    {
        /// <summary>
        /// Formats milliseconds into a human-readable time string
        /// </summary>
        public static string FormatTime(int milliseconds)
        {
            TimeSpan time = TimeSpan.FromMilliseconds(milliseconds);
            
            if (time.Hours > 0)
                return $"{time.Hours}:{time.Minutes:D2}:{time.Seconds:D2}";
            else
                return $"{time.Minutes}:{time.Seconds:D2}";
        }
        
        /// <summary>
        /// Calculates average speed across all actions in a script
        /// </summary>
        public static double CalculateAverageSpeed(List<FunscriptAction> actions)
        {
            if (actions.Count <= 1)
                return 0;
                
            double totalSpeed = 0;
            int samples = 0;
            
            for (int i = 1; i < actions.Count; i++)
            {
                totalSpeed += ColorUtils.GetSpeed(actions[i-1], actions[i]);
                samples++;
            }
            
            return samples > 0 ? totalSpeed / samples : 0;
        }
    }
}