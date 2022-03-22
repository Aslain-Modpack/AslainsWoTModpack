﻿using RelhaxModpack.Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    public abstract class FileSourceTask : AutomationTask, IXmlSerializable
    {
        public string SourceFilePath { get; set; } = string.Empty;

        #region Xml Serialization
        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml attributes.
        /// </summary>
        /// <returns>A list of string property names.</returns>
        /// <remarks>Xml attributes will always be written, xml elements are optional.</remarks>
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(SourceFilePath) }).ToArray();
        }
        #endregion

        #region Task execution
        public override void ProcessMacros()
        {
            SourceFilePath = ProcessMacro(nameof(SourceFilePath), SourceFilePath);
        }

        public override void ValidateCommands()
        {
            if (ValidateCommandTrue(string.IsNullOrEmpty(SourceFilePath), string.Format("SourceFilePath is empty string")))
                return;

            if (ValidateCommandTrue(!File.Exists(SourceFilePath), string.Format("SourceFilePath of {0} file does not exist", SourceFilePath)))
                return;
        }
        #endregion
    }
}
