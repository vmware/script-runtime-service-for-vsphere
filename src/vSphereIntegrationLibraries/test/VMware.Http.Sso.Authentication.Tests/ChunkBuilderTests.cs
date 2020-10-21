// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using VMware.Http.Sso.Authentication.Impl;
using VMware.Http.Sso.Authentication.Tests.Properties;

namespace VMware.Http.Sso.Authentication.Tests {
   public class ChunkBuilderTests {
      [Test]
      public void BuildSingleChunk() {
         // Arrange
         var maxChunkSize = 100;
         var tokenValue = "<xml>small<xml>";
         var paramToken = new ParamValue(ParamValue.Param.token, tokenValue);
         var chunkBuilder = new ChunkBuilder(maxChunkSize);
         // Act
         var remaining = chunkBuilder.Append(paramToken);
         var actual = chunkBuilder.GetValue();

         // Assert
         Assert.IsNull(remaining);
         Assert.AreEqual($"{ParamValue.Param.token}=\"{tokenValue}\"", actual);
      }

      [Test]
      public void ExceedChunkSize() {
         // Arrange
         var tokenValue = "123456";
         var maxChunkSize = 10;
         var paramToken = new ParamValue(ParamValue.Param.token, tokenValue);
         var chunkBuilder = new ChunkBuilder(maxChunkSize);

         // Act
         var remaining = chunkBuilder.Append(paramToken);
         var actual = chunkBuilder.GetValue();

         // Assert
         Assert.AreEqual($"{ParamValue.Param.token}=\"12\"", actual);
         Assert.NotNull(remaining);
         Assert.AreEqual(ParamValue.Param.token, remaining.Key);
         Assert.AreEqual("3456", remaining.Value);
      }
   }
}
