// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using k8s.KubeConfigModels;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("VMware.ScriptRuntimeService.APIGateway.Tests")]
namespace VMware.ScriptRuntimeService.APIGateway.Authentication {
   internal class UserToIdentifiableData<T> {
      #region Private fields
      private Dictionary<string, List<string>> _userId2DataIds =
        new Dictionary<string, List<string>>();
      private Dictionary<string, T> _dataId2Data =
         new Dictionary<string, T>();
      #endregion

      #region Private implementation
      private string CalculateDataKey(string userId, string dataId) {
         return $"{userId}_{dataId}";
      }      

      private void SetDataItem(string userId, string dataId, T data) {         
         var dataKey = CalculateDataKey(userId, dataId);
         _dataId2Data[dataKey] = data;         
      }

      private void RemoveDataItem(string userId, string dataId) {         
         var dataKey = CalculateDataKey(userId, dataId);
         if (_dataId2Data.ContainsKey(dataKey)) {
            _dataId2Data.Remove(dataKey);
         }         
      }
      #endregion

      #region Public Interface
      public void Add(string userId, string dataId, T data) {
         lock (this) {
            if (!_userId2DataIds.ContainsKey(userId)) {
               _userId2DataIds[userId] = new List<string>();
            }
            _userId2DataIds[userId].Add(dataId);
            SetDataItem(userId, dataId, data);
         }
      }

      public T[] List(string userId) {
         T[] result = null;

         lock (this) {
            if (_userId2DataIds.ContainsKey(userId)) {
               var resultList = new List<T>();
               foreach (var dataId in _userId2DataIds[userId]) {
                  resultList.Add(GetData(userId, dataId));
               }
               result = resultList.ToArray();
            }
         }

         return result;
      }

      public string[] ListUsers() {
         string[] result = null;

         lock (this) {
            result = _userId2DataIds.Keys.ToArray();
         }

         return result;
      }

      public bool Contains(string userId) {
         lock (this) {
            return _userId2DataIds.ContainsKey(userId);
         }
      }

      public bool Contains(string userId, string dataId) {
         lock (this) {
            return _userId2DataIds.ContainsKey(userId)
                && _userId2DataIds[userId].Contains(dataId);
         }
      }

      public string GetUser(string dataId) {
         string result = string.Empty;

         if (!string.IsNullOrEmpty(dataId)) {
            lock (this) {
               foreach (var userId in _userId2DataIds.Keys) {
                  if (_userId2DataIds[userId].Contains(dataId)) {
                     result = userId;
                     break;
                  }
               }
            }
         }

         return result;
      }

      public T GetData(string userId, string dataId) {
         T result = default;
         var dataKey = CalculateDataKey(userId, dataId);
         lock (this) {
            if (_dataId2Data.ContainsKey(dataKey)) {
               result = _dataId2Data[dataKey];
            }
         }
         return result;
      }

      public void RemoveUser(string userId) {
         lock (this) {
            if (_userId2DataIds.ContainsKey(userId)) {
               foreach (var dataId in _userId2DataIds[userId]) {
                  RemoveDataItem(userId, dataId);
               }
               _userId2DataIds.Remove(userId);
            }
         }
      }

      public void RemoveData(string userId, string dataId) {
         lock (this) {
            if (_userId2DataIds.ContainsKey(userId) &&
                _userId2DataIds[userId].Contains(dataId)) {
               _userId2DataIds[userId].Remove(dataId);
               RemoveDataItem(userId, dataId);
            }
         }
      } 
      #endregion
   }
}
