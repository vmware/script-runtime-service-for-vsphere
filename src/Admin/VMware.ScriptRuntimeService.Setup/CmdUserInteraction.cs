// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;

namespace VMware.ScriptRuntimeService.Setup
{
   public class CmdUserInteraction : IUserInteraction {
      public void DisplayMessage(string message) {
         Console.WriteLine(message);
      }

      public void DisplayError(string message) {
         var foregroundColor = Console.ForegroundColor;
         Console.ForegroundColor = ConsoleColor.Red;
         Console.WriteLine(message);
         Console.ForegroundColor = foregroundColor;
      }

      public string AskQuestion(string question, string[] validAnswers, string defaultAnswer) {
         string answer;

         if (validAnswers != null && !string.IsNullOrEmpty(defaultAnswer)) {
            // Ask Only Once in default answer is provided
            Console.Write(question);
            answer = Console.ReadLine();
            if (!validAnswers.Contains(answer)) {
               answer = defaultAnswer;
            }
         } else {
            // Ask until valid answer is given
            do
            {
               Console.Write(question);
               answer = Console.ReadLine();
            } while (validAnswers == null || validAnswers.Contains(answer));
         }

         return answer;
      }

      public SecureString SecurePrompt(string message) {
         var secret = new List<char>();

         Console.Write(message);

         do {
            ConsoleKeyInfo key = Console.ReadKey(true);
            // Backspace Should Not Work
            if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter) {
               secret.Add(key.KeyChar);
               Console.Write(@"*");
            }
            else {
               if (key.Key == ConsoleKey.Backspace && secret.Count > 0) {
                  secret.RemoveAt(secret.Count - 1);
                  Console.Write("\b \b");
               }
               else if (key.Key == ConsoleKey.Enter) {
                  break;
               }
            }
         } while (true);
         
         var result = new SecureString();
         foreach (var c in secret) {
            result.AppendChar(c);
         }

         return result;
      }
   }
}
