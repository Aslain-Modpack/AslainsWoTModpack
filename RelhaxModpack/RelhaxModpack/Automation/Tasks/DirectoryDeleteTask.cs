﻿using RelhaxModpack.Database;
using RelhaxModpack.UI;
using RelhaxModpack.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    public class DirectoryDeleteTask : DirectoryTask, IXmlSerializable, ICancelOperation
    {
        public const string TaskCommandName = "directory_delete";

        public override string Command { get { return TaskCommandName; } }

        protected bool good = false;

        protected bool reportingProgress { get { return DatabaseAutomationRunner != null; } }

        protected RelhaxProgress relhaxProgress;

        protected Progress<RelhaxProgress> progress;

        protected CancellationTokenSource cancellationTokenSource;

        #region Xml Serialization
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes();
        }
        #endregion

        #region Task Execution
        public override void ProcessMacros()
        {
            base.ProcessMacros();
        }

        public override void ValidateCommands()
        {
            base.ValidateCommands();
        }

        public async override Task RunTask()
        {
            cancellationTokenSource = new CancellationTokenSource();

            if (reportingProgress)
            {
                relhaxProgress = new RelhaxProgress()
                {
                    ChildCurrentProgress = "barWithTextChild",
                    ChildCurrent = 0,
                    ChildTotal = 0,
                    ParrentCurrent = 0,
                    ParrentTotal = 0
                };

                progress = new Progress<RelhaxProgress>();

                progress.ProgressChanged += DatabaseAutomationRunner.RelhaxProgressChanged;
            }

            await Task.Run(() =>
            {
                try
                {
                    //get list of all source files
                    string[] filesToDelete = Directory.GetFiles(DirectoryPath, "*", SearchOption.AllDirectories);

                    if (reportingProgress)
                        relhaxProgress.ChildTotal = filesToDelete.Count();

                    foreach (string file in filesToDelete)
                    {
                        if (reportingProgress)
                        {
                            relhaxProgress.ChildCurrent++;
                            (progress as IProgress<RelhaxProgress>).Report(relhaxProgress);
                        }

                        bool result = FileUtils.FileDelete(file);
                        if (!result)
                            return;
                    }

                    if (reportingProgress)
                    {
                        relhaxProgress.ChildTotal = relhaxProgress.ChildCurrent;
                        (progress as IProgress<RelhaxProgress>).Report(relhaxProgress);
                    }

                    good = true;
                }
                catch (OperationCanceledException)
                {
                    good = false;
                    return;
                }
                catch (Exception ex)
                {
                    Logging.Exception(ex.ToString());
                    good = false;
                    return;
                }
                finally
                {
                    cancellationTokenSource.Dispose();
                    if (reportingProgress)
                    {
                        progress.ProgressChanged -= DatabaseAutomationRunner.RelhaxProgressChanged;
                    }
                }
            });
        }

        public override void ProcessTaskResults()
        {
            if (ProcessTaskResultFalse(good, "The delete process failed"))
                return;
        }

        public virtual void Cancel()
        {
            cancellationTokenSource.Cancel();
        }
        #endregion
    }
}
