// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using VMware.Http.Sso.Authentication.Properties;

namespace VMware.Http.Sso.Authentication.Impl {
   public class RequestsVerifier {
      private int _clockToleranceSec;
      private int _maxRequestAgeSec;
      private SortedSet<Nonce> _lastIds = new SortedSet<Nonce>();

      public RequestsVerifier(int clockToleranceSec, int maxRequestAgeSec) {
         _clockToleranceSec = clockToleranceSec;
         _maxRequestAgeSec = maxRequestAgeSec;
      }

      public void VerifyAgeAndRepeatOnNewRequest(Nonce nonce) {
         Nonce notPreviousThan =
            Nonce.FromDate(DateTime.Now.Subtract(new TimeSpan(0, 0, 0, _maxRequestAgeSec + _clockToleranceSec)));

         if (nonce.CompareTo(notPreviousThan) < 0) {
            throw new AuthException(Resources.Request_Is_Quite_Old);
         }

         lock(_lastIds) {
            PurgeOlder(notPreviousThan);
            if (_lastIds.Contains(nonce)) {
               throw new AuthException(Resources.Repeat_Attack_Alert);
            }
            _lastIds.Add(nonce);
         }
      }

      private void PurgeOlder(Nonce notPreviousThan) {
         _lastIds.RemoveWhere(current => current.CompareTo(notPreviousThan) < 0);
      }
   }
}
