// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections;
using System.Management.Automation;
using System.Text;
using System.Threading;
using VMware.ScriptRuntimeService.Runspace.Types;

namespace VMware.ScriptRuntimeService.Runspace.PSScriptExecutionEngine {
   public class DataStreams : IDataStreams {
      private bool _closed;
      private const int MaxNumberOfMessage = 50;
      private DataStream _information = new DataStream(MaxNumberOfMessage);
      private DataStream _error = new DataStream(MaxNumberOfMessage);
      private DataStream _debug = new DataStream(MaxNumberOfMessage);
      private DataStream _warning = new DataStream(MaxNumberOfMessage);
      private DataStream _verbose = new DataStream(MaxNumberOfMessage);

      public IStreamRecord[] Information => _information.Get();
      public IStreamRecord[] Error => _error.Get();
      public IStreamRecord[] Warning => _warning.Get();
      public IStreamRecord[] Debug => _debug.Get();
      public IStreamRecord[] Verbose => _verbose.Get();

      public void AddInformation(object informationRecord) {
         _information.Append(informationRecord?.ToString());
      }

      public void AddError(object errorRecord) {
         _error.Append(errorRecord?.ToString());
      }

      public void AddWarning(object warnRecord) {
         _warning.Append(warnRecord?.ToString());
      }

      public void AddDebug(object debugRecord) {
         _debug.Append(debugRecord?.ToString());
      }

      public void AddVerbose(object verboseRecord) {
         _verbose.Append(verboseRecord?.ToString());
      }

      public void Close() {
         if (!_closed) {
            _information.Close();
            _error.Close();
            _debug.Close();
            _warning.Close();
            _verbose.Close();
            _closed = true;
         }
      }
   }
}
