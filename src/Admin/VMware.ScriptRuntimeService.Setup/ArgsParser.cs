// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System.Collections.Generic;
using VMware.ScriptRuntimeService.AdminEngine;

namespace VMware.ScriptRuntimeService.Setup {
   public class ArgsParser {
      private readonly string[] _positionalParameters = new string[] { };// { "psc", "user", "password", "configdir" };

      private class Token {
         public string Parameter { get; set; }
         public string Value { get; set; }
      }

      public UserInput Parse(IEnumerable<string> args) {

         var tokens = Tokenize(args);

         var result = new UserInput();

         foreach (var token in tokens) {
            UpdateUserInput(result, token);
         }

         return result;
      }

      private void UpdateUserInput(UserInput userInput, Token token) {
         var lowerCaseParameterName = token.Parameter.ToLower();
         switch (lowerCaseParameterName)
         {
            case "psc":
               userInput.Psc = token.Value;
               break;
            case "user":
               userInput.User = token.Value;
               break;
            case "password":
               userInput.SetPassword(token.Value);
               break;
            case "configdir":
               userInput.ConfigDir = token.Value;
               break;
            case "k8ssettings":
               userInput.K8sSettings = token.Value;
               break;
            case "force":
               userInput.ForceSpecified = true;
               break;
         }
      }

   private IEnumerable<Token> Tokenize(IEnumerable<string> args) {
         int position = 0;
         Token token = null;
         foreach (var arg in args) {
            if (token == null) {
               token = new Token();
            }
            if (arg.StartsWith("-")) {
               // Parameter name
               token.Parameter = arg.TrimStart('-').Trim();
            } else {
               // Patameter value
               token.Value = arg.Trim();

               // Resolve positional parameter names
               if (string.IsNullOrEmpty(token.Parameter)) {
                  if (position < _positionalParameters.Length) {
                     token.Parameter = _positionalParameters[position];
                  }
               }
               position++;
            }

            if (token.Value != null) {
               yield return token;

               token = null;
            }
            
         }
      }
   }
}
