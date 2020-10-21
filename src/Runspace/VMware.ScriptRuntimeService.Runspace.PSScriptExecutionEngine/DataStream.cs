// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using VMware.ScriptRuntimeService.Runspace.Types;

namespace VMware.ScriptRuntimeService.Runspace.PSScriptExecutionEngine {
   /// <summary>
   /// Stores PowerShell stream messages. Designed to append stream data during script execution. Once
   /// Script execution is complete it is assumed no more data will be appended to the DataStream. Close method has to be
   /// called in order to finalize data appending and keep the data only for reading.
   /// </summary>
   internal class DataStream {
      private bool _closed;
      private ReaderWriterLockSlim _streamLock = new ReaderWriterLockSlim();
      private List<IStreamRecord> _streamValueBuilder = new List<IStreamRecord>();
      private IStreamRecord[] _streamValue = null;
      private int _maxNumberOfMessages;

      public DataStream() : this (-1) {}

      public DataStream(int numberOfMessageLimit) {
         if (numberOfMessageLimit > 0) {
            _maxNumberOfMessages = numberOfMessageLimit;
         } else {
            _maxNumberOfMessages = Int32.MaxValue;
         }
      }

      public IStreamRecord[] Get() {
         if (_streamValue != null) {
            return _streamValue;
         }

         _streamLock.EnterReadLock();
         try {
            return _streamValueBuilder.ToArray();
         } finally {
            _streamLock.ExitReadLock();
         }
      }

      public void Append(string data) {
         if (!string.IsNullOrEmpty(data)) {
            _streamLock.EnterWriteLock();
            try {
               _streamValueBuilder.Add(new StreamRecord {
                  Time = DateTime.Now,
                  Message = data
               });
               if (_streamValueBuilder.Count > _maxNumberOfMessages) {
                  _streamValueBuilder.RemoveAt(0);
               }
            } finally {
               _streamLock.ExitWriteLock();
            }
         }
      }

      public void Close() {
         if (!_closed) {
            _streamValue = Get();
            _streamLock.Dispose();
            _closed = true;
         }
      }
   }
}
