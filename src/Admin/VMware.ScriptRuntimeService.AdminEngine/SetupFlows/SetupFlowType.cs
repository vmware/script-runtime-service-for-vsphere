// **************************************************************************
//  Copyright 2020-2022 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

namespace VMware.ScriptRuntimeService.AdminEngine.SetupFlows {
   public enum SetupFlowType {
      InitialSetup,
      RegisterWithVC,
      UnregisterFromVC,
      UpdateTlsCertificate,
      UpdateTrustedCACertificates,
      CleanupVCRegistration
   }
}
