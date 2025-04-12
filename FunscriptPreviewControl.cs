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
        private Funscript _currentScript = null;
        
        // Theme toggle properties
        private readonly Button _themeToggleButton;
        private readonly FlowLayoutPanel _buttonPanel;
        
        // Metadata panel resizing properties
        private bool _isResizing = false;
        private readonly Panel _resizeHandle;
        private int _startHeight;
        private Point _startMousePos;

        public FunscriptPreviewControl()
        {
            InitializeSettings();
            
            _mainLayout = CreateMainLayout();
            _visualizationPanel = CreateVisualizationPanel(out _visualizationBox);
            _metadataPanel = CreateMetadataPanel(out _metadataLabel, out _resizeHandle);
            _buttonPanel = CreateButtonPanel(out _themeToggleButton);
            
            ArrangeLayout();
        }
        
        private void InitializeSettings()
        {
            SettingsManager.Initialize();
        }
        
        private TableLayoutPanel CreateMainLayout()
        {
            var themeColors = SettingsManager.GetCurrentThemeColors();
            
            return new TableLayoutPanel { 
                Dock = DockStyle.Fill,
                Padding = new Padding(15),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None,
                BackColor = themeColors.MainBackground
            };
        }
        
        private Panel CreateVisualizationPanel(out PictureBox visualizationBox)
        {
            var themeColors = SettingsManager.GetCurrentThemeColors();
            
            var panel = new Panel {
                Dock = DockStyle.Fill,
                Padding = new Padding(1),
                BackColor = themeColors.PanelBorder,
                MaximumSize = new Size(0, 150)
            };
            
            visualizationBox = new PictureBox { 
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = themeColors.PanelBackground
            };
            
            panel.Controls.Add(visualizationBox);
            return panel;
        }
        
        private Panel CreateMetadataPanel(out Label metadataLabel, out Panel resizeHandle)
        {
            var themeColors = SettingsManager.GetCurrentThemeColors();
            
            var panel = new Panel {
                Dock = DockStyle.Fill,
                Padding = new Padding(1),
                BackColor = themeColors.PanelBorder,
                MinimumSize = new Size(0, 250),
                Height = SettingsManager.MetadataPanelHeight
            };
            
            metadataLabel = new Label { 
                Dock = DockStyle.Fill,
                AutoSize = false,
                BackColor = themeColors.PanelBackground,
                ForeColor = themeColors.TextColor,
                Font = new Font("Segoe UI", 10.5f),
                Padding = new Padding(15)
            };
            
            resizeHandle = CreateResizeHandle();
            
            panel.Controls.Add(resizeHandle);
            panel.Controls.Add(metadataLabel);
            
            return panel;
        }
        
        private Panel CreateResizeHandle()
        {
            var themeColors = SettingsManager.GetCurrentThemeColors();
            
            var handle = new Panel {
                Dock = DockStyle.Bottom,
                Height = 8,
                Cursor = Cursors.SizeNS,
                BackColor = themeColors.PanelBorder
            };
            
            handle.MouseDown += ResizeHandle_MouseDown;
            handle.MouseUp += ResizeHandle_MouseUp;
            handle.MouseMove += ResizeHandle_MouseMove;
            
            return handle;
        }
        
        private FlowLayoutPanel CreateButtonPanel(out Button themeToggleButton)
        {
            var themeColors = SettingsManager.GetCurrentThemeColors();
            
            themeToggleButton = new Button {
                Text = SettingsManager.CurrentTheme == ThemeType.Light ? "Dark Mode" : "Light Mode",
                ForeColor = themeColors.TextColor,
                BackColor = themeColors.PanelBackground,
                FlatStyle = FlatStyle.Flat,
                Width = 120,
                Height = 30,
                Margin = new Padding(0, 0, 0, 0)
            };
            
            themeToggleButton.FlatAppearance.BorderColor = themeColors.PanelBorder;
            themeToggleButton.FlatAppearance.BorderSize = 1;
            themeToggleButton.Click += ThemeToggleButton_Click;
            
            var panel = new FlowLayoutPanel {
                Dock = DockStyle.Bottom,
                Height = 40,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = themeColors.MainBackground,
                Padding = new Padding(15, 0, 15, 10)
            };
            
            panel.Controls.Add(themeToggleButton);
            
            return panel;
        }
        
        private void ArrangeLayout()
        {
            _mainLayout.RowCount = 3;
            _mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, SettingsManager.MetadataPanelHeight));
            _mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _mainLayout.Controls.Add(_visualizationPanel, 0, 0);
            _mainLayout.Controls.Add(_metadataPanel, 0, 1);
            _mainLayout.Controls.Add(_buttonPanel, 0, 2);
        
            Controls.Add(_mainLayout);
            BackColor = SettingsManager.GetCurrentThemeColors().MainBackground;
        }
        
        private void ResizeHandle_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isResizing = true;
                _startMousePos = PointToScreen(e.Location);
                _startHeight = _metadataPanel.Height;
                _resizeHandle.Capture = true;
            }
        }

        private void ResizeHandle_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && _isResizing)
            {
                _isResizing = false;
                _resizeHandle.Capture = false;
        
                Point currentPos = PointToScreen(e.Location);
                int diff = currentPos.Y - _startMousePos.Y;
                int newHeight = _startHeight + diff;
        
                newHeight = Math.Max(newHeight, _metadataPanel.MinimumSize.Height);
                newHeight = Math.Min(newHeight, 800);
        
                _metadataPanel.Height = newHeight;
                _mainLayout.RowStyles[1] = new RowStyle(SizeType.Absolute, newHeight);
                _mainLayout.PerformLayout();
        
                SettingsManager.SetMetadataPanelHeight(newHeight);
            }
        }

        private void ResizeHandle_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isResizing)
            {
                Cursor.Current = Cursors.SizeNS;
            }
        }

        private void ThemeToggleButton_Click(object sender, EventArgs e)
        {
            SettingsManager.ToggleTheme();
            ApplyTheme();
            
            if (_currentScript != null) // Re-render funscript if loaded
            {
                _visualizationBox.Image = RenderPreview(_currentScript, 
                    Math.Max(400, _visualizationBox.Width), 
                    Math.Max(300, _visualizationBox.Height));
            }
        }
        
        private void ApplyTheme()
        {
            var themeColors = SettingsManager.GetCurrentThemeColors();
            
            _mainLayout.BackColor = themeColors.MainBackground;
            BackColor = themeColors.MainBackground;
            
            _visualizationPanel.BackColor = themeColors.PanelBorder;
            _visualizationBox.BackColor = themeColors.PanelBackground;
            
            _metadataPanel.BackColor = themeColors.PanelBorder;
            _metadataLabel.BackColor = themeColors.PanelBackground;
            _metadataLabel.ForeColor = themeColors.TextColor;
            
            _resizeHandle.BackColor = themeColors.PanelBorder;
            
            _themeToggleButton.Text = SettingsManager.CurrentTheme == ThemeType.Light ? "Dark Mode" : "Light Mode";
            _themeToggleButton.ForeColor = themeColors.TextColor;
            _themeToggleButton.BackColor = themeColors.PanelBackground;
            _themeToggleButton.FlatAppearance.BorderColor = themeColors.PanelBorder;
            
            _buttonPanel.BackColor = themeColors.MainBackground;
        }

        public void HandlePreview(string filePath)
        {
            try 
            {
                // Read funscript JSON from file
                string json = File.ReadAllText(filePath);
                _currentScript = JsonConvert.DeserializeObject<Funscript>(json);
                
                if (_currentScript == null || _currentScript.actions == null || _currentScript.actions.Count == 0)
                {
                    _metadataLabel.Text = "Invalid or empty funscript file";
                    return;
                }
                
                // Calculate derived metadata if needed
                int duration = _currentScript.actions[_currentScript.actions.Count - 1].at;
                double avgSpeed = MetadataFormatter.CalculateAverageSpeed(_currentScript.actions);
                
                // Render visualization
                _visualizationBox.Image = RenderPreview(_currentScript, 
                    Math.Max(400, _visualizationBox.Width), 
                    Math.Max(300, _visualizationBox.Height));
                
                // Display metadata
                _metadataLabel.Text = FormatMetadata(_currentScript, duration, avgSpeed);
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
                var themeColors = SettingsManager.GetCurrentThemeColors();
                
                // Fill entire background
                using (var bgBrush = new SolidBrush(themeColors.PanelBackground))
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
        
        protected override void SetVisualsBackgroundColor(Color color)
        {
            ApplyTheme();
        }
        
        protected override void SetVisualsTextColor(Color color)
        {
            var themeColors = SettingsManager.GetCurrentThemeColors();
            _metadataLabel.ForeColor = themeColors.TextColor;
        }
        
        protected override void SetVisualsFont(Font font)
        {
            _metadataLabel.Font = font;
        }
    }
}