﻿using RelhaxModpack.Utilities;
using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation
{
    public class PackageDownloadTask : PackageTransferTask
    {
        public const string TaskCommandName = "package_download";

        public override string Command { get { return TaskCommandName; } }

        public override void ValidateCommands()
        {
            base.ValidateCommands();

            if (ValidateCommand(string.IsNullOrEmpty(DatabasePackage.ZipFile), string.Format("ExitCode {0}: Package {1} does not have a zip file to download", ExitCode, DatabasePackage.PackageName)))
                return;

            string directoryPath = Path.GetDirectoryName(FilePath);
            //the directory path could be null if the user wants to load/save the database right here and is using a relative path
            if (!string.IsNullOrWhiteSpace(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                Logging.Info("The directory path {0} was not found to exist and was created", directoryPath);
            }
        }

        #region Task execution
        public override async Task RunTask()
        {
            using (WebClient = new WebClient { Credentials = new NetworkCredential(AutomationSettings.BigmodsUsername, AutomationSettings.BigmodsPassword) })
            {
                string serverPath = string.Format("{0}{1}/{2}", PrivateStuff.BigmodsFTPUsersRoot, WoTOnlineFolderVersion, DatabasePackage.ZipFile);

                Logging.Info(Logfiles.AutomationRunner, "Downloading package");
                Logging.Debug(Logfiles.AutomationRunner, "Download zip url = {0}, file = {1}", serverPath, FilePath);
                //https://stackoverflow.com/questions/2953403/c-sharp-passing-method-as-the-argument-in-a-method
                if (DatabaseAutomationRunner != null)
                {
                    WebClient.DownloadProgressChanged += DatabaseAutomationRunner.DownloadProgressChanged;
                    WebClient.DownloadFileCompleted += DatabaseAutomationRunner.DownloadFileCompleted;
                }
                try
                {
                    if(DatabaseAutomationRunner != null)
                    {
                        if (DatabaseAutomationRunner.DownloadProgressChanged != null)
                        {
                            long filesize = await FtpUtils.FtpGetFilesizeAsync(serverPath, WebClient.Credentials);
                            DatabaseAutomationRunner.DownloadFileCompleted.Invoke(null, new System.ComponentModel.AsyncCompletedEventArgs(null, false, filesize));
                        }
                    }
                    await WebClient.DownloadFileTaskAsync(serverPath, FilePath);
                    TransferSuccess = true;
                }
                catch (Exception ex)
                {
                    Logging.Exception(ex.ToString());
                }
                finally
                {
                    if (DatabaseAutomationRunner != null)
                    {
                        WebClient.DownloadProgressChanged -= DatabaseAutomationRunner.DownloadProgressChanged;
                        WebClient.DownloadFileCompleted -= DatabaseAutomationRunner.DownloadFileCompleted;
                    }
                }
            }
        }

        public override void ProcessTaskResults()
        {
            base.ProcessTaskResults();

            if (ProcessTaskResult(!File.Exists(FilePath), string.Format("The download FilePath {0} does not exist", FilePath)))
                return;
        }
        #endregion
    }
}