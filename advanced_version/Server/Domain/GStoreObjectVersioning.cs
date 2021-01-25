using System;
using System.Collections.Generic;
using System.Text;

namespace GStoreServer.Domain
{
    class GStoreObjectVersioning
    {
        private int newestVersion = -1;
        private int newestRequestId;
        private string newestValue;

        private int currentRequestId = 0;

        private readonly ISet<int> unmatchedOperations = new HashSet<int>();

        // Returns true if it matches an operation (and removes it from the set of unmatched operations)
        // Returns false if it doesn't match an operation (and adds it to the set of unmatched operations)
        public bool MatchOperation(int requestId)
        {
            if (unmatchedOperations.Contains(requestId))
            {
                unmatchedOperations.Remove(requestId);
                return true;
            }
            else
            {
                unmatchedOperations.Add(requestId);
                return false;
            }
        }

        public string GetNewestValue()
        {
            if (newestVersion == -1) throw new Exception("No operation set");
            return newestValue;
        }

        public int GetNewestRequestId()
        {
            if (newestVersion == -1) throw new Exception("No operation set");
            return newestRequestId;
        }

        // Sets newest operation if it is newer than current stored newest
        // Returns true if set
        // Returns false otherwise
        public bool SetNewestOperation(int version, int requestId, string value)
        {
            if (version < 0) throw new Exception("Version must be higher than 0");
            if (version > newestVersion)
            {
                newestVersion = version;
                newestRequestId = requestId;
                newestValue = value;
            }
            return false;
        }

        public void SetCurrentRequestId(int requestId)
        {
            currentRequestId = requestId;
        }

        public int GetCurrentRequestId()
        {
            return currentRequestId;
        }


    }
}
