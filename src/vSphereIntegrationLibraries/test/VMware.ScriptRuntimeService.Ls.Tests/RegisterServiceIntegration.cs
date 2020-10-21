// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System.Security;
using System.Security.Cryptography.X509Certificates;
using NUnit.Framework;

namespace VMware.ScriptRuntimeService.Ls.Tests
{
   /// <summary>
   /// Contains integrations tests interacting with vSphere Lookup Service
   /// To run those configure test vSphere on VC_ADDRESS, VC_USER, and VC_PASSWORD constant
   /// and uncomment [Test] attributes
   /// </summary>
   public class RegisterServiceIntegration
   {
      private const string VC_ADDRESS = "10.23.80.118";
      private const string VC_USER = "administrator@vsphere.local";
      private const string VC_PASSWORD = "Admin!23";
      private LookupServiceClient _lsClient;

      private static SecureString ConvertStringToSecureString(string str)
      {
         // There must be a better way to do it but I'm not wasting more time
         // with it right now
         SecureString secureString = new SecureString();
         foreach (char c in str)
         {
            secureString.AppendChar(c);
         }

         return secureString;
      }

      [SetUp]
      public void Setup()
      {
         _lsClient = new LookupServiceClient(VC_ADDRESS, new AcceptAllX509CertificateValidator());
      }

      //[Test]
      public void RegisterDeleteService()
      {
         var vcUser = VC_USER; // SSO Username
         var vcPassword = ConvertStringToSecureString(VC_PASSWORD); // SSO Username
         var serviceID = "95245223-a43d-48b4-971d-b3dd77bbd5db";
         _lsClient.RegisterService(
            vcUser, // SSO Username
            vcPassword, // SSO Password src/PsExecutionHost.sln
            "11c16647-fdb9-47cd-bcf7-83f09dfe7a20",
            "test",
            "com.vmware.test",
            serviceID,
            "test.ServiceName",
            "1.0",
            "com.vmware.test",
            "test",
            @"http://10.23.93.72:5001/",
            "https",
            "com.vmware.test",
            new X509Certificate2(
               X509Certificate.CreateFromCertFile(
                  @"C:\git-repos\SsoAdminClientLib\TestCertificate\psexecutionhost.cer")));

         _lsClient.DeleteService(
            vcUser,
            vcPassword,
            serviceID); 
      }

      //[Test]
      public void DeleteUnexistentService() {
         var vcUser = VC_USER; // SSO Username
         var vcPassword = ConvertStringToSecureString(VC_PASSWORD); // SSO Username
         var serviceID = "95245223-a43d-48b4-971d-b3dd77bbdsdfb";
         
         _lsClient.DeleteService(
            vcUser,
            vcPassword,
            serviceID); 
      }

      //[Test]
      public void ListRegisteredServices() {
         var services = _lsClient.ListRegisteredServices();
         Assert.NotNull(services);
      }

      //[Test]
      public void GetStsEndpoint() {
         var stsUri = _lsClient.GetStsEndpointUri();
         Assert.NotNull(stsUri);
         Assert.IsTrue(stsUri.ToString().Contains("STSService"));
      }

      //[Test]
      public void GetSsoAdminEndpoint() {
         var ssoAdminUri = _lsClient.GetSsoAdminEndpointUri();
         Assert.NotNull(ssoAdminUri);
         Assert.IsTrue(ssoAdminUri.ToString().Contains("sso-adminserver"));
      }
   }
}
