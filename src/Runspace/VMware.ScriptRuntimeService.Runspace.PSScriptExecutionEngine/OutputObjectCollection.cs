// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.ServiceModel.Channels;
using System.Text;
using VMware.ScriptRuntimeService.Runspace.Types;

namespace VMware.ScriptRuntimeService.Runspace.PSScriptExecutionEngine {
   public class OutputObjectCollection : IOutputObjectCollection {
      public OutputObjectCollection() { }

      public OutputObjectCollection(
         ICollection<PSObject> outputObjects,
         Func<ICollection<PSObject>, string[]> serializeObjectsFunc,
         Func<ICollection<PSObject>, string> formatObjectsFunc) {

         SerializedObjects = serializeObjectsFunc.Invoke(outputObjects);
         FormattedTextPresentation = formatObjectsFunc.Invoke(outputObjects);

      }

      public string FormattedTextPresentation { get; internal set; }
      public string[] SerializedObjects { get; internal set; }
   }
}
