namespace Lison
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class TypeDefinition
    {
        public string Definition { get; protected set; }

        public TypeDefinition (Type type)
        {
            StringBuilder builder = new StringBuilder();
            WriteType (type, builder, false);

            Definition = builder.ToString ();
        }

        private static void WriteType (Type type, StringBuilder builder, bool member)
        {
            // Primitive types.
            if (IsPrimitiveType (type)) {
                builder.Append (member ? "#s" : "#S");
                return;
            }

            // Dictionaries.
            if (IsGenericDictionary (type)) {
                builder.Append (member ? "<d" : "<D");

                builder.Append ("k");
                WriteType (type.GetGenericArguments () [0], builder, false);

                builder.Append ("v");
                WriteType (type.GetGenericArguments () [1], builder, false);

                builder.Append (member ? "/d" : "/D");

                return;
            }

            // Enumerations.
            if (type.IsArray || IsGenericEnumerable (type)) {
                Type elementType = type.IsArray ? type.GetElementType () : type.GetGenericArguments () [0];

                builder.Append (member ? "<a" : "<A");
                WriteType (elementType, builder, false);
                builder.Append (member ? "/a" : "/A");

                return;
            }

            // Objects.
            builder.Append (member ? "<o" : "<O");
            foreach (var getter in Helpers.GetterCollection.FromType (type)) {
                builder.Append (getter.Name);
                WriteType (getter.PropertyType, builder, true);
            }
            builder.Append (member ? "/o" : "/O");
        }

        public static bool IsGenericDictionary (Type type)
        {
            return type
                .GetInterfaces ()
                .Any (iType => iType.IsGenericType && iType.GetGenericTypeDefinition () == typeof (IDictionary<,>));
        }

        public static bool IsGenericEnumerable (Type type)
        {
            return type
                .GetInterfaces ()
                .Any (iType => iType.IsGenericType && iType.GetGenericTypeDefinition () == typeof (IEnumerable<>));
        }

        private static bool IsPrimitiveType (Type obj)
        {
            return obj == typeof (DBNull) ||
                   obj == typeof (string) || obj == typeof (char) ||
                   obj == typeof (Guid) || obj == typeof (Enum) ||
                   obj == typeof (bool) || obj == typeof (DateTime) ||
                   obj == typeof (int) || obj == typeof (long) || obj == typeof (double) ||
                   obj == typeof (decimal) || obj == typeof (float) ||
                   obj == typeof (byte) || obj == typeof (short) ||
                   obj == typeof (sbyte) || obj == typeof (ushort) ||
                   obj == typeof (uint) || obj == typeof (ulong) ||
                   obj == typeof (byte []);
        }

        private static readonly Dictionary <Type, TypeDefinition> cache = new Dictionary <Type, TypeDefinition> ();

        public static TypeDefinition ForType (Type type)
        {
            lock (cache)
            {
                TypeDefinition value;

                if (!cache.TryGetValue (type, out value))
                {
                    value = new TypeDefinition (type);
                    cache.Add (type, value);
                }

                return value;
            }
        }
    }
}