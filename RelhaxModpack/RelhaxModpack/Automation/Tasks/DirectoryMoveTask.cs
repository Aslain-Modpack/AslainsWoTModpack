﻿using RelhaxModpack.Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    public class DirectoryMoveTask : DirectoryDestinationTask, IXmlSerializable, ICancelOperation
    {
        /// <summary>
        /// The xml name of this command.
        /// </summary>
        public const string TaskCommandName = "directory_move";

        public override string Command { get { return TaskCommandName; } }

        protected bool good = false;

        protected CancellationTokenSource cancellationTokenSource;

        #region Xml Serialization
        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml attributes.
        /// </summary>
        /// <returns>A list of string property names.</returns>
        /// <remarks>Xml attributes will always be written, xml elements are optional.</remarks>
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
            await base.RunTask();
            if (searchResults == null || searchResults.Count() == 0)
                return;

            cancellationTokenSource = new CancellationTokenSource();

            await Task.Run(() =>
            {
                string lastSourceFile = string.Empty, lastDestinationFile = string.Empty;
                try
                {
                    //move each file over
                    foreach (string sourceFile in searchResults)
                    {
                        if (cancellationTokenSource.IsCancellationRequested)
                            throw new OperationCanceledException(cancellationTokenSource.Token);

                        string destinationFile = sourceFile.Replace(DirectoryPath, DestinationPath);
                        string destinationPath = Path.GetDirectoryName(destinationFile);
                        if (!Directory.Exists(destinationPath))
                            Directory.CreateDirectory(destinationPath);

                        lastSourceFile = sourceFile;
                        lastDestinationFile = destinationFile;
                        File.Move(sourceFile, destinationFile);
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
                    Logging.Error("Failed moving file from {0} to {1}", lastSourceFile, lastDestinationFile);
                    good = false;
                    return;
                }
                finally
                {
                    cancellationTokenSource.Dispose();
                }
            });
        }

        public override void ProcessTaskResults()
        {
            if (ProcessTaskResultFalse(good, "The move process failed"))
                return;
        }

        public virtual void Cancel()
        {
            cancellationTokenSource.Cancel();
        }
        #endregion
    }
}
