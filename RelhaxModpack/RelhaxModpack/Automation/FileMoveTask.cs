﻿using RelhaxModpack.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation
{
    public class FileMoveTask : FileDestinationTask
    {
        public const string TaskCommandName = "file_move";

        public override string Command { get { return TaskCommandName; } }

        protected bool fileMoveResult;

        #region Task execution
        public override Task RunTask()
        {
            base.RunTask();

            if (!destinationDeleteResult)
            {
                fileMoveResult = false;
                return null;
            }

            Logging.Info("Moving file from location {0} to location {1}", SourceFilePath, DestinationFilePath);

            if (fileMoveResult)
            fileMoveResult = FileUtils.FileMove(SourceFilePath, DestinationFilePath);

            return null;
        }

        public override void ProcessTaskResults()
        {
            if (!ProcessTaskResultFalse(fileMoveResult, "The file move operation failed"))
                return;
        }
        #endregion
    }
}