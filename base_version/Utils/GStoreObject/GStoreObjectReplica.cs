using System;

namespace Utils
{
    public class GStoreObjectReplica : IEquatable<GStoreObjectReplica>
    {
        public GStoreObject Object { get; }

        public bool IsMaster;

        public GStoreObjectReplica(GStoreObject gStoreObject, bool isMaster)
        {
            Object = gStoreObject ?? throw new ArgumentNullException("GStoreObject parameter can't be null.");
            IsMaster = isMaster;
        }

        public override string ToString()
        {
            return $"{Object.Identifier.PartitionId}, {Object.Identifier.ObjectId}, {Object.Value}, {IsMaster}";
        }

        public bool Equals(GStoreObjectReplica other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Object.Identifier.PartitionId.Equals(other.Object.Identifier.PartitionId) && Object.Identifier.ObjectId.Equals(other.Object.Identifier.ObjectId);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Object.Identifier.PartitionId, Object.Identifier.ObjectId);
        }
    }
}
