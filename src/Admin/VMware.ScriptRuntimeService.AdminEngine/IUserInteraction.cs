// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System.Security;

namespace VMware.ScriptRuntimeService.AdminEngine {
   public interface IUserInteraction {
      void DisplayMessage(string message);
      void DisplayError(string message);
      string AskQuestion(string question, string[] validAnswers, string defaultAnswer);
      SecureString SecurePrompt(string message);
   }
}
