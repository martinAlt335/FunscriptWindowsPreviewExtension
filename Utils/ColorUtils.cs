using System;
using System.Drawing;
using FunscriptPreviewHandler.Models;

namespace FunscriptPreviewHandler.Utils
{
    public static class ColorUtils
    {
        private static readonly Color[] HeatmapColors = new[]
        {
            Color.FromArgb(0, 0, 0),         // Black
            Color.FromArgb(30, 144, 255),    // DodgerBlue
            Color.FromArgb(34, 139, 34),     // ForestGreen
            Color.FromArgb(255, 215, 0),     // Gold
            Color.FromArgb(220, 20, 60),     // Crimson
            Color.FromArgb(147, 112, 219),   // MediumPurple
            Color.FromArgb(37, 22, 122)      // Deep blue-purple
        };
        
        /// <summary>
        /// Converts intensity/speed value into a heatmap color
        /// </summary>
        public static Color GetHeatmapColor(float intensity)
        {
            const float stepSize = 120f;
    
            if (intensity <= 0) return HeatmapColors[0];
            if (intensity > 5 * stepSize) return HeatmapColors[6];
    
            intensity += stepSize / 2.0f;
            int colorIndex = (int)Math.Floor(intensity / stepSize);
    
            // Prevent index out of bounds
            colorIndex = Math.Min(colorIndex, HeatmapColors.Length - 2);
    
            float t = Math.Min(1.0f, Math.Max(0.0f, (intensity - colorIndex * stepSize) / stepSize));
    
            return LerpColor(HeatmapColors[colorIndex], HeatmapColors[colorIndex + 1], t);
        }        
        /// <summary>
        /// Linear interpolation between two colors
        /// </summary>
        private static Color LerpColor(Color colorA, Color colorB, float t)
        {
            int r = (int)(colorA.R + (colorB.R - colorA.R) * t);
            int g = (int)(colorA.G + (colorB.G - colorA.G) * t);
            int b = (int)(colorA.B + (colorB.B - colorA.B) * t);
            
            return Color.FromArgb(r, g, b);
        }
        
        /// <summary>
        /// Calculates speed between two actions
        /// </summary>
        public static float GetSpeed(FunscriptAction firstAction, FunscriptAction secondAction)
        {
            if (firstAction.at == secondAction.at) return 0;
            
            // Ensure first action is before second
            FunscriptAction first = firstAction;
            FunscriptAction second = secondAction;
            
            if (second.at < first.at)
            {
                first = secondAction;
                second = firstAction;
            }
            
            float posDiff = (float)Math.Abs(second.pos - first.pos);
            float timeDiff = (float)Math.Abs(second.at - first.at);
            
            return 1000 * (posDiff / timeDiff);
        }
    }
}