﻿using RelhaxModpack.Settings;
using RelhaxModpack.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace RelhaxModpack.Windows
{
    /// <summary>
    /// A class definition to specify a window that is a separate sub-function of the application:
    /// It has its own log and settings file, and requires custom loading code
    /// </summary>
    public class RelhaxCustomFeatureWindow : RelhaxWindow
    {
        /// <summary>
        /// Indicates if this editor instance was launched from the MainWindow or from command line
        /// </summary>
        /// <remarks>This changes the behavior of the logging for the editor</remarks>
        public bool LaunchedFromMainWindow { get; set; }

        /// <summary>
        /// The settings definitions class for this window
        /// </summary>
        public ISettingsFile Settings { get; protected set; }

        /// <summary>
        /// Controls is the application will, on startup, run a check to see if it's the latest version
        /// </summary>
        public bool RunStandAloneUpdateCheck { get; set; }

        /// <summary>
        /// Creates an instance of the RelhaxCustomFeatureWindow class
        /// </summary>
        public RelhaxCustomFeatureWindow(ModpackSettings modpackSettings) : base(modpackSettings)
        {
            //subscribe to the loaded event to load custom settings code
            Closed += OnWindowClosed;
        }

        private void OnWindowClosed(object sender, EventArgs e)
        {
            SettingsParser parser = new SettingsParser();
            parser.SaveSettings(Settings);
        }

        protected override void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            SettingsParser parser = new SettingsParser();
            //if it's not launched from the main window, then the ModpackSettings object has not been parsed yet. do that now.
            if (!LaunchedFromMainWindow)
            {
                //TODO: use hook for functions that normally happen in the MainWindow?
            }

            //also load the feature window's custom settings file
            parser.LoadSettings(Settings);

            //load base classes method
            base.OnWindowLoaded(sender, e);

            //if not skipping update, and not launched from the main window, then run a stand-alone update check
            if (!LaunchedFromMainWindow && !CommandLineSettings.SkipUpdate && RunStandAloneUpdateCheck)
            {
                Task.Run(async () =>
                {
                    if (!await CommonUtils.IsManagerUptoDate(CommonUtils.GetApplicationVersion(), ModpackSettings.ApplicationDistroVersion))
                    {
                        MessageBox.Show("Your application is out of date. Please launch the application normally to update");
                    }
                });
            }
        }

        protected override void ApplyFontToWindow()
        {
            if (DefaultFontFamily == null)
            {
                DefaultFontFamily = this.FontFamily;
                SelectedFontFamily = DefaultFontFamily;
                FontList.Clear();
                FontList.AddRange(Fonts.GetFontFamilies(Environment.GetFolderPath(Environment.SpecialFolder.Fonts)).ToList());
            }

            base.ApplyFontToWindow();
        }
    }
}
