﻿using RelhaxModpack.Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    public class MacroSubstringFilenameTask : MacroSubstringTask, IXmlSerializable
    {
        /// <summary>
        /// The xml name of this command.
        /// </summary>
        public const string TaskCommandName = "macro_substring_filename";

        public override string Command {  get { return TaskCommandName; } }

        public string FilePath { get; set; }

        #region Xml serialization
        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml attributes.
        /// </summary>
        /// <returns>A list of string property names.</returns>
        /// <remarks>Xml attributes will always be written, xml elements are optional.</remarks>
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(FilePath) }).ToArray();
        }
        #endregion

        #region Task execution
        public override void ProcessMacros()
        {
            base.ProcessMacros();
            FilePath = ProcessMacro(nameof(FilePath), FilePath);
        }

        public override void ValidateCommands()
        {
            base.ValidateCommands();

            if (ValidateCommandStringNullEmptyTrue(nameof(FilePath), FilePath))
                return;

            if (ValidateCommandFalse(File.Exists(FilePath), string.Format("The filepath {0} does not exist", FilePath)))
                return;
        }

        public override async Task RunTask()
        {
            await base.RunTask();
        }

        protected override async Task GetStringValue()
        {
            stringWithValue = Path.GetFileName(FilePath);
        }

        public override void ProcessTaskResults()
        {
            base.ProcessTaskResults();
        }
        #endregion
    }
}
