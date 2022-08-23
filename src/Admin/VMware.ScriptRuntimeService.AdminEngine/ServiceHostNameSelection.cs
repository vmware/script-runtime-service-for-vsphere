// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace VMware.ScriptRuntimeService.AdminEngine {
   public class ServiceHostNameSelection {
      private IUserInteraction _userInteraction;

      public ServiceHostNameSelection(IUserInteraction userInteraction) {
         _userInteraction = userInteraction;
      }

      public SetupServiceSettings Resolve() {
         var hostname = "";

         // Ask user to pick up IP address from the list of available IP Addresses
         var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
         var ipAddresses = (
            from netInterface
               in networkInterfaces
            where netInterface.OperationalStatus == OperationalStatus.Up
            from ip in netInterface.GetIPProperties().UnicastAddresses
            where ip.Address.AddressFamily == AddressFamily.InterNetwork
            select ip.Address.ToString()).ToList();
         var ipAddressChoiceSb = new StringBuilder();
         var validOptions = new List<string>();
         for (var i = 1; i <= ipAddresses.Count; i++) {
            ipAddressChoiceSb.AppendLine($"[{i}] {ipAddresses[i - 1]}");
            validOptions.Add(i.ToString());
         }

         if (ipAddresses.Count > 1) {
            var answer = _userInteraction.AskQuestion(
               string.Format(
                  Resources.IpAddressChoice,
                  ipAddressChoiceSb,
                  $"1 - {ipAddresses.Count}"),
               validOptions.ToArray(),
               "1");

            if (!int.TryParse(answer, out var intAnswer)) {
               throw new InvalidDataException("Invalid IP address option was provided.");
            }

            hostname = ipAddresses[intAnswer - 1];
         } else {
            hostname = ipAddresses[0];
         }

         // IP or Hostname has been selected
         return new SetupServiceSettings().SetHostname(hostname);
      }
   }
}
