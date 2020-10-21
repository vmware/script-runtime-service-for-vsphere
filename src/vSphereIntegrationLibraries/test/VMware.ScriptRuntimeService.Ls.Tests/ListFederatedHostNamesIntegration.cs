// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using NUnit.Framework;

namespace VMware.ScriptRuntimeService.Ls.Tests
{
   /// <summary>
   /// Contains integrations tests interacting with vSphere Lookup Service
   /// To run those configure test vSphere on VC_ADDRESS constant
   /// and uncomment [Test] attributes
   /// </summary>
   public class ListFederatedHostNamesIntegration {
      private const string VC_ADDRESS = "10.23.81.78";
      private LookupServiceClient _lsClient;

      [SetUp]
      public void Setup() {
         _lsClient = new LookupServiceClient(VC_ADDRESS, new AcceptAllX509CertificateValidator());
      }

      //[Test]
      public void ListFederatedVCs() {
         var endpoints = new List<VCEndpoint>(_lsClient.GetFederatedVCHostNames());         

         Assert.NotNull(endpoints);
         Assert.AreEqual(2, endpoints.Count);

         var hostNames = new List<string>(endpoints.Select<VCEndpoint, string>(a => a.Hostname));

         Assert.Contains(VC_ADDRESS, hostNames);
         Assert.Contains(VC_ADDRESS, hostNames);

         Assert.NotNull(endpoints[0].SslTrustCertificates);
         Assert.NotNull(endpoints[1].SslTrustCertificates);

         Assert.GreaterOrEqual(1, endpoints[0].SslTrustCertificates.Length);
         Assert.GreaterOrEqual(1, endpoints[1].SslTrustCertificates.Length);
      }
   }
}
