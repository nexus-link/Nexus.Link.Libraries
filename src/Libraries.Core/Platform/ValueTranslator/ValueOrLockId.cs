using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Nexus.Link.Libraries.Core.Platform.ValueTranslator
{
    /// <summary>
    /// ValueOrLockId
    /// </summary>
    [DataContract]
    public class ValueOrLockId :  IEquatable<ValueOrLockId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValueOrLockId" /> class.
        /// </summary>
        /// <param name="isLock">True if this is a lock and <see cref="LockId"/> is used, else <see cref="Value"/> is used.</param>
        public ValueOrLockId(bool isLock)
        {
            IsLock = isLock;
        }
        
        /// <summary>
        /// Gets or Sets IsLock
        /// </summary>
        [DataMember(Name="IsLock", EmitDefaultValue=false)]
        public bool IsLock { get; set; }
        /// <summary>
        /// Gets or Sets Value
        /// </summary>
        [DataMember(Name="Value", EmitDefaultValue=false)]
        public string Value { get; set; }
        /// <summary>
        /// Gets or Sets LockId
        /// </summary>
        [DataMember(Name="LockId", EmitDefaultValue=false)]
        public string LockId { get; set; }
        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString() => IsLock ? $"Lock: {LockId}" : Value;

        /// <summary>
        /// Returns true if objects are equal
        /// </summary>
        /// <param name="obj">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object obj)
        {
            // credit: http://stackoverflow.com/a/10454552/677735
            return Equals(obj as ValueOrLockId);
        }

        /// <summary>
        /// Returns true if ValueOrLockId instances are equal
        /// </summary>
        /// <param name="other">Instance of ValueOrLockId to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(ValueOrLockId other)
        {
            // credit: http://stackoverflow.com/a/10454552/677735
            if (other == null)
                return false;

            return 
                IsLock == other.IsLock && 
                (
                    Value == other.Value ||
                    Value != null &&
                    Value.Equals(other.Value)
                ) && 
                (
                    LockId == other.LockId ||
                    LockId != null &&
                    LockId.Equals(other.LockId)
                );
        }

        /// <summary>
        /// Gets the hash code
        /// </summary>
        /// <returns>Hash code</returns>
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            // credit: http://stackoverflow.com/a/263416/677735
            unchecked // Overflow is fine, just wrap
            {
                var hash = 41;
                hash = hash * 59 + IsLock.GetHashCode();
                if (Value != null)
                    hash = hash * 59 + Value.GetHashCode();
                if (LockId != null)
                    hash = hash * 59 + LockId.GetHashCode();
                return hash;
            }
        }
    }

}
