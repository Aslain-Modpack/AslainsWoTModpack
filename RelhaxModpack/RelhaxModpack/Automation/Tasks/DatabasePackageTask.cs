﻿using RelhaxModpack.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    public abstract class DatabasePackageTask : AutomationTask
    {
        public DatabasePackage DatabasePackage { get { return AutomationSequence.Package; } }

        public List<DatabasePackage> DatabasePackages { get { return AutomationSequence.DatabasePackages; } }

        #region Task execution
        public override void ValidateCommands()
        {
            if (ValidateCommandTrue(DatabasePackage == null, string.Format("DatabasePackage is null (This is an internal application error)")))
                return;

            if (ValidateCommandTrue(DatabasePackages == null, string.Format("DatabasePackages is null (This is an internal application error)")))
                return;
        }
        #endregion
    }
}
