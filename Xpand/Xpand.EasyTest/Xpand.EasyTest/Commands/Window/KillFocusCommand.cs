﻿using DevExpress.EasyTest.Framework;
using Xpand.EasyTest.Automation;

namespace Xpand.EasyTest.Commands.Window{
    public class KillFocusCommand:Command{
        public const string Name = "KillFocus";
        protected override void InternalExecute(ICommandAdapter adapter){
            WindowAutomation.KillFocus();
        }
    }
}