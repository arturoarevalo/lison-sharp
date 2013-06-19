namespace Lison
{
    using System;
    using System.Collections.Generic;
    using Lison.TypeWriters;

    public static class TypeWriterFactory {

        private static readonly Dictionary<Type, AbstractTypeWriter> cache = new Dictionary<Type, AbstractTypeWriter> ();

        public static AbstractTypeWriter CreateInstance (InitializationContext context)
        {
            lock (cache)
            {
                AbstractTypeWriter value;

                if (!cache.TryGetValue (context.Type, out value))
                {
                    value = InternalCreateInstance (context);
                    cache.Add (context.Type, value);
                }

                return value;
            }
        }

        private static AbstractTypeWriter InternalCreateInstance (InitializationContext context)
        {
            // Primitive types.
            if (context.IsPrimitiveType ())
            {
                if (context.IsBooleanPrimitiveType ())
                {
                    return new BooleanTypeWriter (context);
                }

                if (context.IsIntegerPrimitiveType ())
                {
                    return new IntegerTypeWriter (context);
                }

                if (context.IsFloatingPointPrimitiveType ())
                {
                    return new FloatingPointTypeWriter (context);
                }

                if (context.IsStringPrimitiveType ())
                {
                    if (context.InlineStrings)
                    {
                        return new StringTypeWriter (context);
                    }
                    else
                    {
                        return new StringTableTypeWriter (context);
                    }
                }

                return new UnknownTypeWriter (context);
            }

            // Dictionaries.
            if (context.IsGenericDictionary ())
            {
                return new DictionaryTypeWriter (context);
            }

            // Enumerations.
            if (context.Type.IsArray || context.IsGenericEnumerable ())
            {
                return new ArrayTypeWriter (context);
            }

            // Objects.
            return new ObjectTypeWriter (context);
        }

    }
}