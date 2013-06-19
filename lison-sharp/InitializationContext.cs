namespace Lison
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class InitializationContext
    {
        public Type Type { get; set; }
        public bool InlineStrings { get; set; }
        public bool AsMember { get; set; }

        public InitializationContext (Type type)
        {
            Type = type;
            InlineStrings = true;
            AsMember = false;
        }

        public InitializationContext (InitializationContext parent)
        {
            Type = parent.Type;
            InlineStrings = parent.InlineStrings;
            AsMember = parent.AsMember;
        }

        public InitializationContext (InitializationContext parent, Type type)
        {
            Type = type;
            InlineStrings = parent.InlineStrings;
            AsMember = parent.AsMember;
        }

        public bool IsGenericDictionary ()
        {
            return Type
                .GetInterfaces ()
                .Any (iType => iType.IsGenericType && iType.GetGenericTypeDefinition () == typeof (IDictionary<,>));
        }

        public  bool IsGenericEnumerable ()
        {
            return Type
                .GetInterfaces ()
                .Any (iType => iType.IsGenericType && iType.GetGenericTypeDefinition () == typeof (IEnumerable<>));
        }

        public bool IsPrimitiveType ()
        {
            return IsBooleanPrimitiveType () ||
                   IsStringPrimitiveType () ||
                   IsIntegerPrimitiveType () ||
                   IsFloatingPointPrimitiveType () ||
                   Type == typeof (DBNull) ||
                   Type == typeof (char) ||
                   Type == typeof (Guid) || Type == typeof (Enum) ||
                   Type == typeof (DateTime) ||
                   Type == typeof (byte[]);
        }

        public bool IsBooleanPrimitiveType ()
        {
            return Type == typeof (bool);
        }
        public bool IsStringPrimitiveType ()
        {
            return Type == typeof (string);
        }

        public bool IsIntegerPrimitiveType ()
        {
            return
                Type == typeof (byte) ||
                Type == typeof (short) ||
                Type == typeof (int) ||
                Type == typeof (long) ||
                Type == typeof (ushort) ||
                Type == typeof (uint) ||
                Type == typeof (ulong);
        }

        public bool IsFloatingPointPrimitiveType ()
        {
            return Type == typeof (float) ||
                   Type == typeof (double) ||
                   Type == typeof (decimal);
        }

    }
}