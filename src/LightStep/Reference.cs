using System;
using System.Collections.Generic;

namespace LightStep
{
    public sealed class Reference : IEquatable<Reference>
    {
        public SpanContext Context { get; }
        
        public string ReferenceType { get; }

        public Reference(SpanContext context, string referenceType)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            ReferenceType = referenceType ?? throw new ArgumentNullException(nameof(referenceType));
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Reference);
        }

        public bool Equals(Reference other)
        {
            return other != null &&
                   EqualityComparer<SpanContext>.Default.Equals(Context, other.Context)
                   && ReferenceType == other.ReferenceType;
        }

        public override int GetHashCode()
        {
            var hashCode = 2083322454;
            hashCode = hashCode * -1521134295 + EqualityComparer<SpanContext>.Default.GetHashCode(Context);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ReferenceType);
            return hashCode;
        }
    }
}
