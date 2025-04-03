using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using Newtonsoft.Json;
using SharpShell.SharpPreviewHandler;
using FunscriptPreviewHandler.Models;
using FunscriptPreviewHandler.Rendering;
using FunscriptPreviewHandler.Utils;

namespace FunscriptPreviewHandler
{
    public class FunscriptPreviewControl : PreviewHandlerControl
    {
        private readonly TableLayoutPanel _mainLayout;
        private readonly PictureBox _visualizationBox;
        private readonly Label _metadataLabel;
        private readonly Panel _visualizationPanel;
        private readonly Panel _metadataPanel;

        public FunscriptPreviewControl()
        {
            _mainLayout = new TableLayoutPanel { 
                Dock = DockStyle.Fill,
                Padding = new Padding(15),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None,
                BackColor = Color.FromArgb(22, 22, 22)
            };
        
            // Create panel to host visualization with border
            _visualizationPanel = new Panel {
                Dock = DockStyle.Fill,
                Padding = new Padding(1),
                Margin = new Padding(0, 0, 0, 10),
                BackColor = Color.FromArgb(50, 50, 50),
                MaximumSize = new Size(0, 150)
            };
        
            _visualizationBox = new PictureBox { 
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.FromArgb(28, 28, 28)
            };
        
            _visualizationPanel.Controls.Add(_visualizationBox);
        
            // Create panel to host metadata with border
            _metadataPanel = new Panel {
                Dock = DockStyle.Fill,
                Padding = new Padding(1),
                BackColor = Color.FromArgb(50, 50, 50),
                MaximumSize = new Size(0, 300)
            };
        
            _metadataLabel = new Label { 
                Dock = DockStyle.Fill,
                AutoSize = false,
                BackColor = Color.FromArgb(28, 28, 28),
                ForeColor = Color.FromArgb(220, 220, 220),
                Font = new Font("Segoe UI", 10.5f),
                Padding = new Padding(15)
            };
            _metadataPanel.Controls.Add(_metadataLabel);
        
            // Use fixed heights instead of percentages
            _mainLayout.RowCount = 2;
            _mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Auto for visualization
            _mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Auto for metadata
            _mainLayout.Controls.Add(_visualizationPanel, 0, 0);
            _mainLayout.Controls.Add(_metadataPanel, 0, 1);
        
            Controls.Add(_mainLayout);
            BackColor = Color.FromArgb(22, 22, 22);
        }

        public void HandlePreview(string filePath)
        {
            try 
            {
                // Read the funscript JSON from file
                string json = File.ReadAllText(filePath);
                var funscript = JsonConvert.DeserializeObject<Funscript>(json);
                
                if (funscript == null || funscript.actions == null || funscript.actions.Count == 0)
                {
                    _metadataLabel.Text = "Invalid or empty funscript file";
                    return;
                }
                
                // Calculate derived metadata if needed
                int duration = funscript.actions[funscript.actions.Count - 1].at;
                double avgSpeed = MetadataFormatter.CalculateAverageSpeed(funscript.actions);
                
                // Render visualization
                _visualizationBox.Image = RenderPreview(funscript, 
                    Math.Max(400, _visualizationBox.Width), 
                    Math.Max(300, _visualizationBox.Height));
                
                // Display metadata
                _metadataLabel.Text = FormatMetadata(funscript, duration, avgSpeed);
            }
            catch (Exception ex)
            {
                _metadataLabel.Text = $"Error previewing funscript: {ex.Message}";
            }
        }
        
        private Bitmap RenderPreview(Funscript script, int width, int height)
        {
            // Constrain height to 150px maximum
            int maxHeatmapHeight = Math.Min(150, height);

            Bitmap bitmap = new Bitmap(width, maxHeatmapHeight);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                // Fill entire background
                using (var bgBrush = new SolidBrush(Color.FromArgb(28, 28, 28)))
                {
                    g.FillRectangle(bgBrush, 0, 0, width, maxHeatmapHeight);
                }

                // Render heatmap
                HeatmapRenderer.RenderHeatmap(g, script, width, maxHeatmapHeight);
            }

            return bitmap;
        }
        
        private string FormatMetadata(Funscript script, int duration, double avgSpeed)
        {
            var sb = new StringBuilder();

            if (script.metadata != null && !string.IsNullOrEmpty(script.metadata.title))
            {
                sb.AppendLine($"Title: {script.metadata.title}");
                sb.AppendLine();
            }

            sb.AppendLine($"Duration: {MetadataFormatter.FormatTime(duration)} | Actions: {script.actions.Count} | Avg Speed: {avgSpeed:F1} movements/sec");

            if (script.metadata != null)
            {
                // Metadata: Creator and Type
                if (!string.IsNullOrEmpty(script.metadata.creator) || !string.IsNullOrEmpty(script.metadata.type))
                {
                    var creatorTypeInfo = new List<string>();
                    if (!string.IsNullOrEmpty(script.metadata.creator))
                        creatorTypeInfo.Add($"Creator: {script.metadata.creator}");
                    if (!string.IsNullOrEmpty(script.metadata.type))
                        creatorTypeInfo.Add($"Type: {script.metadata.type}");
                    sb.AppendLine(string.Join(" | ", creatorTypeInfo));
                }

                // Metadata: Performers and License
                if ((script.metadata.performers != null && script.metadata.performers.Length > 0) || 
                    !string.IsNullOrEmpty(script.metadata.license))
                {
                    var performersLicenseInfo = new List<string>();
                    if (script.metadata.performers != null && script.metadata.performers.Length > 0)
                        performersLicenseInfo.Add($"Performers: {string.Join(", ", script.metadata.performers)}");
                    if (!string.IsNullOrEmpty(script.metadata.license))
                        performersLicenseInfo.Add($"License: {script.metadata.license}");
                    
                    sb.AppendLine(string.Join(" | ", performersLicenseInfo));
                }

                // Metadata: Tags
                if (script.metadata.tags != null && script.metadata.tags.Length > 0)
                {
                    sb.AppendLine($"Tags: {string.Join(", ", script.metadata.tags)}");
                }

                // Metadata: Description
                if (!string.IsNullOrEmpty(script.metadata.description))
                    sb.AppendLine($"Description: {script.metadata.description}");

                // Metadata: Notes
                if (!string.IsNullOrEmpty(script.metadata.notes))
                    sb.AppendLine($"Notes: {script.metadata.notes}");

                // Metadata: URLs
                if (!string.IsNullOrEmpty(script.metadata.script_url))
                    sb.AppendLine($"Script URL: {script.metadata.script_url}");

                if (!string.IsNullOrEmpty(script.metadata.video_url))
                    sb.AppendLine($"Video URL: {script.metadata.video_url}");

                // Metadata: Chapters
                if (script.metadata.chapters != null && script.metadata.chapters.Length > 0)
                {
                    sb.AppendLine("Chapters:");
                    foreach (var chapter in script.metadata.chapters)
                    {
                        sb.AppendLine($"- {chapter?.name} ({chapter?.startTime} - {chapter?.endTime})");
                    }
                }
            }

            return sb.ToString();
        }
        
        // Override methods from PreviewHandlerControl
        protected override void SetVisualsBackgroundColor(Color color)
        {
            BackColor = color;
            _mainLayout.BackColor = color;
        }
        
        protected override void SetVisualsTextColor(Color color)
        {
            _metadataLabel.ForeColor = color;
        }
        
        protected override void SetVisualsFont(Font font)
        {
            _metadataLabel.Font = font;
        }
    }
}