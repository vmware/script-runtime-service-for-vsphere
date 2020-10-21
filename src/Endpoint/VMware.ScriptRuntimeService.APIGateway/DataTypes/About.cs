// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using VMware.ScriptRuntimeService.APIGateway.Properties;

namespace VMware.ScriptRuntimeService.APIGateway.DataTypes {
   /// <summary>
   /// About object allows you to read information for the product.
   /// </summary>
   [DataContract(Name = "about")]
   public class About {
      /// <summary>
      /// Full name of the product.
      /// </summary>
      [DataMember(Name = "name")]
      public string Name => APIGatewayResources.ProductName;

      /// <summary>
      /// Vendor name.
      /// </summary>
      [DataMember(Name = "vendor")]
      public string Vendor => APIGatewayResources.Vendor;

      /// <summary>
      /// Version of the product.
      /// </summary>
      [DataMember(Name = "version")]
      public string Version {
         get {
            var assembly = typeof(About).Assembly;
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fileVersionInfo.ProductVersion;
         }
      }

      /// <summary>
      /// Version of the API.
      /// </summary>
      [DataMember(Name = "api_version")]
      public string ApiVersion => APIGatewayResources.ApiVersion;
   }
}
