﻿using RelhaxModpack.Common;
using RelhaxModpack.Database;
using RelhaxModpack.Utilities.Enums;
using RelhaxModpack.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation
{
    public abstract class AutomationTask : IXmlSerializable, IComponentWithID
    {
        #region Xml serialization
        public virtual string[] PropertiesForSerializationAttributes()
        {
            return new string[] { nameof(ID) };
        }

        public virtual string[] PropertiesForSerializationElements()
        {
            return new string[] { };
        }

        public static Dictionary<string, Type> TaskTypeMapper { get; } = new Dictionary<string, Type>()
        {
            { DownloadStaticTask.TaskCommandName, typeof(DownloadStaticTask) }
        };

        public const string AttributeNameForMapping = "Command";
        #endregion //Xml serialization

        protected Stopwatch ExecutionTimeStopwatch = new Stopwatch();

        public AutomationSequence AutomationSequence { get; set; }

        public DatabaseAutomationRunner DatabaseAutomationRunner { get { return AutomationSequence.DatabaseAutomationRunner; } }

        public List<AutomationMacro> Macros { get { return AutomationSequence.MacrosListForTask; } }

        public string ErrorMessage { get; protected set; } = string.Empty;

        public int ExitCode { get; protected set; } = 0;

        public abstract string Command { get; }

        public string ID { get; set; } = string.Empty;

        public long ExecutionTimeValidateCommandsMs { get; protected set; } = 0;

        public long ExecutionTimeRunTaskMs { get; protected set; } = 0;

        public long ExecutionTimeProcessTaskResultsMs { get; protected set; } = 0;

        public long ExecutionTimeMs
        {
            get
            {
                return ExecutionTimeValidateCommandsMs + ExecutionTimeRunTaskMs + ExecutionTimeProcessTaskResultsMs;
            }
        }

        public string ComponentInternalName { get { return ID; } }

        public virtual void PreProcessingHook()
        {
            //stub
        }

        public abstract void ValidateCommands();

        public abstract Task RunTask();

        public abstract void ProcessTaskResults();

        public virtual bool EvaluateResults(string state)
        {
            if (ExitCode != 0)
            {
                Logging.Error(Logfiles.AutomationRunner, LogOptions.MethodName, "Error in task {0} execution! Exit code {1}, ErrorMessage: {2}", Command, ExitCode, string.IsNullOrEmpty(ErrorMessage) ? "(empty)" : ErrorMessage);
                return false;
            }
            else if (ExitCode == -1)
            {
                Logging.AutomationRunner("BadMemeException: ExitCode result is -1. This could indicate an error with the task API. Please report this error to the developer.", LogLevel.Exception);
                Logging.GetLogfile(Logfiles.AutomationRunner).Write(ErrorMessage);
                return false;
            }
            else
            {
                Logging.Info(Logfiles.AutomationRunner, LogOptions.MethodName, "Task: {0} State: {1}, ExitCode: {2}", Command, state, ExitCode);
                return true;
            }
        }

        public virtual async Task Execute()
        {
            Logging.Info(Logfiles.AutomationRunner, LogOptions.MethodName, "Running task {0}: Task start");
            Logging.Debug(Logfiles.AutomationRunner, LogOptions.MethodName, "Running task {0}: ValidateCommands() start", Command);
            ExecutionTimeStopwatch.Restart();
            try
            {
                ValidateCommands();
            }
            catch (Exception ex)
            {
                ExitCode = -1;
                ErrorMessage = ex.ToString() + Environment.NewLine + ex.StackTrace;
            }
            ExecutionTimeValidateCommandsMs = ExecutionTimeStopwatch.ElapsedMilliseconds;
            Logging.Debug(Logfiles.AutomationRunner, LogOptions.MethodName, "Running task {0}: ValidateCommands() finish, ExecutionTime: {1}", Command, ExecutionTimeValidateCommandsMs.ToString());
            if (!EvaluateResults("ValidateCommands"))
                return;

            Logging.Debug(Logfiles.AutomationRunner, LogOptions.MethodName, "Running task {0}: RunTask() start", Command);
            ExecutionTimeStopwatch.Restart();
            try
            {
                await RunTask();
            }
            catch (Exception ex)
            {
                ExitCode = -1;
                ErrorMessage = ex.ToString() + Environment.NewLine + ex.StackTrace;
            }
            ExecutionTimeRunTaskMs = ExecutionTimeStopwatch.ElapsedMilliseconds;
            Logging.Debug(Logfiles.AutomationRunner, LogOptions.MethodName, "Running task {0}: RunTask() finish, ExecutionTime: {1}", Command, ExecutionTimeRunTaskMs.ToString());
            if (!EvaluateResults("RunTask"))
                return;

            Logging.Debug(Logfiles.AutomationRunner, LogOptions.MethodName, "Running task {0}: ProcessTaskResults() start", Command);
            ExecutionTimeStopwatch.Restart();
            try
            {
                ProcessTaskResults();
            }
            catch (Exception ex)
            {
                ExitCode = -1;
                ErrorMessage = ex.ToString() + Environment.NewLine + ex.StackTrace;
            }
            ExecutionTimeProcessTaskResultsMs = ExecutionTimeStopwatch.ElapsedMilliseconds;
            Logging.Debug(Logfiles.AutomationRunner, LogOptions.MethodName, "Running task {0}: ProcessTaskResults() finish, ExecutionTime: {1}", Command, ExecutionTimeProcessTaskResultsMs.ToString());
            ExecutionTimeStopwatch.Stop();
            if (!EvaluateResults("RunTask"))
                return;
            Logging.Info(Logfiles.AutomationRunner, LogOptions.MethodName, "Finished task {0}: Task end, ExecutionTimeMs: {1}", Command, ExecutionTimeMs.ToString());
        }
    }
}