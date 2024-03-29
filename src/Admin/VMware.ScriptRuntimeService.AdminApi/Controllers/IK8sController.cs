// **************************************************************************
//  Copyright 2020-2022 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System.IO;
using VMware.ScriptRuntimeService.AdminApi.DataTypes;
using VMware.ScriptRuntimeService.AdminEngine.K8sClient;

namespace VMware.ScriptRuntimeService.AdminApi.Controllers {
   public interface IK8sController {
      void RestartSrsService();
      IK8sController WithUpdateK8sSettings(K8sSettings k8sSettings);
      Stream GetPodLogReader(LogType podType);
   }
}
