using System;
using System.Drawing;
using Microsoft.Win32;

namespace FunscriptPreviewHandler.Utils
{
    public enum ThemeType
    {
        Dark,
        Light
    }
    
    public class ThemeColors
    {
        public Color MainBackground { get; set; }
        public Color PanelBorder { get; set; }
        public Color PanelBackground { get; set; }
        public Color TextColor { get; set; }
    }
    
    public static class SettingsManager
    {
        private const string RegistryKey = @"Software\FunscriptPreviewHandler";
        private const string ThemeValueName = "Theme";
        private const string MetadataPanelHeightValueName = "MetadataPanelHeight";
        
        public static ThemeType CurrentTheme { get; private set; } = ThemeType.Dark;
        public static int MetadataPanelHeight { get; private set; } = 250; // Default height
        
        public static ThemeColors DarkTheme = new ThemeColors
        {
            MainBackground = Color.FromArgb(22, 22, 22),
            PanelBorder = Color.FromArgb(50, 50, 50),
            PanelBackground = Color.FromArgb(28, 28, 28),
            TextColor = Color.FromArgb(220, 220, 220)
        };
        
        public static ThemeColors LightTheme = new ThemeColors
        {
            MainBackground = Color.FromArgb(240, 240, 240),
            PanelBorder = Color.FromArgb(180, 180, 180),
            PanelBackground = Color.FromArgb(255, 255, 255),
            TextColor = Color.FromArgb(30, 30, 30)
        };
        
        public static ThemeColors GetCurrentThemeColors()
        {
            return CurrentTheme == ThemeType.Dark ? DarkTheme : LightTheme;
        }
        
        public static void Initialize()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(RegistryKey))
                {
                    if (key != null)
                    {
                        // Load theme
                        var themeValue = key.GetValue(ThemeValueName);
                        if (themeValue != null && themeValue.ToString() == ThemeType.Light.ToString())
                        {
                            CurrentTheme = ThemeType.Light;
                        }
                        
                        // Load metadata panel height setting
                        var heightValue = key.GetValue(MetadataPanelHeightValueName);
                        if (heightValue != null)
                        {
                            try
                            {
                                int height = Convert.ToInt32(heightValue);
                                // Set reasonable limit
                                if (height >= 100 && height <= 800)
                                {
                                    MetadataPanelHeight = height;
                                }
                            }
                            catch
                            {
                                // If conversion fails, use default height
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // If registry access fails, fall back to defaults
                CurrentTheme = ThemeType.Dark;
                MetadataPanelHeight = 250;
            }
        }
        
        public static void ToggleTheme()
        {
            CurrentTheme = CurrentTheme == ThemeType.Dark ? ThemeType.Light : ThemeType.Dark;
            SaveThemeSetting();
        }
        
        public static void SetMetadataPanelHeight(int height)
        {
            // Validate height is within reasonable bounds
            if (height >= 100 && height <= 800)
            {
                MetadataPanelHeight = height;
                SaveMetadataPanelHeightSetting();
            }
        }
        
        private static void SaveThemeSetting()
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(RegistryKey))
                {
                    if (key != null)
                    {
                        key.SetValue(ThemeValueName, CurrentTheme.ToString());
                    }
                }
            }
            catch (Exception)
            {
                // Silently fail if registry write fails
            }
        }
        
        private static void SaveMetadataPanelHeightSetting()
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(RegistryKey))
                {
                    if (key != null)
                    {
                        key.SetValue(MetadataPanelHeightValueName, MetadataPanelHeight);
                    }
                }
            }
            catch (Exception)
            {
                // Silently fail if registry write fails
            }
        }
    }
}