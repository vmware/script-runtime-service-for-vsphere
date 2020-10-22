/* 
 * Script Runtime Service for vSphere
 *
 * # Script Runtime Service API    Script Runtime Service for vSphere (SRS) allows running PowerShell and PowerCLI scripts. SRS is a VC add-on that is deployed separately from VCSA. SRS can be accessed via REST API that allows you to create PowerShell instances and run PowerShell and PowerCLI scripts within. No Connect-VIServer is required to run PowerCLI against VC(s) SRS is registered to.    ## Authetication    SRS uses VC SSO as Identity and Authentication Server. Two types of authentication are supported. SIGN and Basic. SIGN authentication is purposed for Service-To-Service access to SRS resources. For convenience of the end-users SRS supports basic authentication passing username and password which are used to acquire SAML HoK token for SRS solution. When basic is used SRS exchanges the username and password for SAML HoK token from the SSO server. SRS uses the SAML token to Connect PowerCLI to VC services in further operations.   On successful authentication SRS returns API Key which is required to authorize further SRS API calls.
 *
 * OpenAPI spec version: 1.0-oas3
 * 
 * Generated by: https://github.com/swagger-api/swagger-codegen.git
 */
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using RestSharp;
using NUnit.Framework;

using IO.Swagger.Client;
using IO.Swagger.Api;
using IO.Swagger.Model;

namespace IO.Swagger.Test
{
    /// <summary>
    ///  Class for testing AuthenticationApi
    /// </summary>
    /// <remarks>
    /// This file is automatically generated by Swagger Codegen.
    /// Please update the test case below to test the API endpoint.
    /// </remarks>
    [TestFixture]
    public class AuthenticationApiTests
    {
        private AuthenticationApi instance;

        /// <summary>
        /// Setup before each unit test
        /// </summary>
        [SetUp]
        public void Init()
        {
            instance = new AuthenticationApi();
        }

        /// <summary>
        /// Clean up after each unit test
        /// </summary>
        [TearDown]
        public void Cleanup()
        {

        }

        /// <summary>
        /// Test an instance of AuthenticationApi
        /// </summary>
        [Test]
        public void InstanceTest()
        {
            // TODO uncomment below to test 'IsInstanceOfType' AuthenticationApi
            //Assert.IsInstanceOfType(typeof(AuthenticationApi), instance, "instance is a AuthenticationApi");
        }

        /// <summary>
        /// Test Login
        /// </summary>
        [Test]
        public void LoginTest()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //instance.Login();
            
        }
        /// <summary>
        /// Test Logout
        /// </summary>
        [Test]
        public void LogoutTest()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //instance.Logout();
            
        }
    }

}
