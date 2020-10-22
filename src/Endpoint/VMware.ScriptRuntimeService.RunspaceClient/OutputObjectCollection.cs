// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using VMware.ScriptRuntimeService.Runspace.Types;

namespace VMware.ScriptRuntimeService.RunspaceClient {
   public class OutputObjectCollection : IOutputObjectCollection {
      public string FormattedTextPresentation { get; set; }
      public string[] SerializedObjects { get; set; }
   }
}
