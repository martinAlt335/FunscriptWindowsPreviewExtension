using System;
using System.Drawing;
using System.Collections.Generic;
using FunscriptPreviewHandler.Models;
using FunscriptPreviewHandler.Utils;

namespace FunscriptPreviewHandler.Rendering
{
    public static class HeatmapRenderer
    {

        public static void RenderHeatmap(Graphics g, Funscript script, int width, int height, int yOffset = 0)
        {
            if (script.actions.Count <= 1)
                return;

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int padding = 8;

            // Fill background

            // Calculate time to x-coordinate scaling with padding
            float msToX = (width - (padding * 2)) / (float)script.actions[script.actions.Count - 1].at;

            List<float> intensityList = new List<float>();
            List<int> posList = new List<int>();

            Point? lastPoint = null;

            for (int i = 1; i < script.actions.Count; i++)
            {
                int x = padding + (int)(msToX * script.actions[i].at);
                int y = height - padding - (int)((height - (padding * 2)) * (script.actions[i].pos / 100.0f)) + yOffset;
                Point currentPoint = new Point(x, y);

                // Skip large gaps
                if (script.actions[i].at - script.actions[i - 1].at > 5000)
                {
                    intensityList.Clear();
                    posList.Clear();
                    lastPoint = currentPoint;
                    continue;
                }

                // Calculate intensity/speed
                float intensity = ColorUtils.GetSpeed(script.actions[i - 1], script.actions[i]);
                intensityList.Add(intensity);
                posList.Add(script.actions[i].pos);

                // Keep window sizes in check
                if (intensityList.Count > 20)
                    intensityList.RemoveAt(0);
                if (posList.Count > 10)
                    posList.RemoveAt(0);

                // Calculate average intensity
                float averageIntensity = 0;
                foreach (var val in intensityList)
                    averageIntensity += val;
                averageIntensity /= intensityList.Count;

                // Get color based on intensity
                Color segmentColor = ColorUtils.GetHeatmapColor(averageIntensity);

                // Draw the colored line segment connecting the action points
                if (lastPoint.HasValue)
                {
                    using (var pen = new Pen(segmentColor, 2))
                    {
                        g.DrawLine(pen, lastPoint.Value, currentPoint);
                    }
                }

                lastPoint = currentPoint;
            }
        }
    }
}