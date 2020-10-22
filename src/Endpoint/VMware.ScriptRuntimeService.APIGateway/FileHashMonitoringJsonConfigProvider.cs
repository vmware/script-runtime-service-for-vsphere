// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace VMware.ScriptRuntimeService.APIGateway
{
   /// <summary>
   /// Configuration source for json file that uses <see cref="FileHashMonitoringJsonConfigProvider"/>
   /// It uses <see cref="JsonConfigurationSource "/> internally.
   /// </summary>
   public class FileHashMonitoringJsonConfigurationSource : IConfigurationSource
   {
      private JsonConfigurationSource _jsonSource;
      public FileHashMonitoringJsonConfigurationSource(string path) {
         _jsonSource = new JsonConfigurationSource();
         _jsonSource.Path = path;
         _jsonSource.ReloadOnChange = true;
         if (File.Exists(path)) {            
            var fullPath = Path.GetFullPath(path);
            _jsonSource.Path = Path.GetFileName(fullPath);
            _jsonSource.FileProvider = new PhysicalFileProvider(Path.GetDirectoryName(fullPath));
         }
      }
      public IConfigurationProvider Build(IConfigurationBuilder builder) {
         return new FileHashMonitoringJsonConfigProvider(_jsonSource);
      }
   }

   /// <summary>
   /// Configuration provider that watches source json file for changes based on content Sha1 hash.
   /// The reason for introducing this one as an extesion to JsonConfigurationProvider which 
   /// watch file system for file update is:
   /// When we run service in k8s and attach config json file from k8s config map
   /// config map update doesn't update file info attributes (like last edit time).
   /// Reloading settings based on content instead of file attributes works for settings attached 
   /// from k8s config map.
   /// </summary>
   public class FileHashMonitoringJsonConfigProvider : JsonConfigurationProvider {
      Timer _sourceFileChangeCheckTimer;
      private SHA1 _sha1 = SHA1.Create();
      private string _lastSourceHash;
      public FileHashMonitoringJsonConfigProvider(JsonConfigurationSource source): base(source) {
         TryComputeSourceHash(out _lastSourceHash);
      }      

      protected override void Dispose(bool disposing) {
         base.Dispose(disposing);
         if (disposing && _sourceFileChangeCheckTimer != null) {            
            _sourceFileChangeCheckTimer.Dispose();
            _sourceFileChangeCheckTimer = null;
         }
      }      

      public override void Load() {
         base.Load();
         if (_sourceFileChangeCheckTimer == null) {
            _sourceFileChangeCheckTimer = new Timer((state) => {               
               if (TryComputeSourceHash(out var currentHash) && 
                  _lastSourceHash != currentHash) {
                  _lastSourceHash = currentHash;
                  Load();
               }
            }, 
            null, 
            1000, 
            60 * 1000);
         }
      }

      private bool TryComputeSourceHash(out string hash) {
         var result = false;
         hash = string.Empty;
         var sourcePath = Source?.Path;
         var sourcePathFileInfo = Source?.FileProvider?.GetFileInfo(Source?.Path);
         if (sourcePathFileInfo != null && sourcePathFileInfo.Exists) {
            sourcePath = sourcePathFileInfo.PhysicalPath;
         }

         if (!string.IsNullOrEmpty(sourcePath) &&
             File.Exists(sourcePath)) {
            try {
               using (FileStream stream = File.OpenRead(sourcePath)) {
                  hash = Convert.ToBase64String(_sha1.ComputeHash(stream));
                  result = true;
               }
            } catch(IOException) { }
         }

         return result;
      }
   }

   /// <summary>
   /// Defins extension method for IConfigurationBuilder
   /// </summary>
   public static class SettingsJsonConfigurationProvider
   {
      public static IConfigurationBuilder AddContentBasedUpdateJsonFileConfiguration(
        this IConfigurationBuilder builder,
        string settingsJsonFilePath) {         
         return builder.Add(new FileHashMonitoringJsonConfigurationSource(settingsJsonFilePath));         
      }      
   }
}
   