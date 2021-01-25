using System;

namespace Utils
{
    public class GStoreObject : IEquatable<GStoreObject>
    {
        public GStoreObjectIdentifier Identifier { get; }
        public string Value { get; set; }

        public GStoreObject(GStoreObjectIdentifier identifier, string value)
        {
            ValidateParameters(identifier, value);

            Identifier = identifier;
            Value = value;
        }

        private void ValidateParameters(GStoreObjectIdentifier identifier, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("value parameter can't be null or empty.");
            }

            if (identifier == null)
            {
                throw new ArgumentException("identifier parameter can't be null");
            }
        }

        public override string ToString()
        {
            return $"{Identifier.PartitionId}, {Identifier.ObjectId}, {Value}";
        }

        public bool Equals(GStoreObject other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Identifier.PartitionId.Equals(other.Identifier.PartitionId) && Identifier.ObjectId.Equals(other.Identifier.ObjectId);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Identifier.PartitionId, Identifier.ObjectId);
        }

    }
}
