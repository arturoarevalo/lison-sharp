namespace Lison
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;

    public class SerializationContext
    {
        public TextWriter Writer { get; set; }
        public char Separator { get; set; }
		public StringTable StringTable { get; set; }
    }

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

    public abstract class AbstractTypeWriter
    {
        public string Definition { get; protected set; }

        public abstract void Write (SerializationContext context, object obj);
    }

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
                    return new StringTableTypeWriter (context);
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

    public class BooleanTypeWriter : AbstractTypeWriter
    {
        public BooleanTypeWriter (InitializationContext context)
        {
            Definition = context.AsMember ? "#b" : "#B";
        }

        public override void Write (SerializationContext context, object obj)
        {
            throw new NotImplementedException ();
        }
    }

    public class IntegerTypeWriter : AbstractTypeWriter
    {
        public IntegerTypeWriter (InitializationContext context)
        {
            Definition = context.AsMember ? "#i" : "#I";
        }

		public override void Write (SerializationContext context, object obj)
        {
            throw new NotImplementedException ();
        }
    }

    public class FloatingPointTypeWriter : AbstractTypeWriter
    {
        public FloatingPointTypeWriter (InitializationContext context)
        {
            Definition = context.AsMember ? "#f" : "#F";
        }

		public override void Write (SerializationContext context, object obj)
        {
            throw new NotImplementedException ();
        }
    }


    public class StringTableTypeWriter : AbstractTypeWriter
    {
        public StringTableTypeWriter (InitializationContext context)
        {
            Definition = context.AsMember ? "#t" : "#T";
        }

		public override void Write (SerializationContext context, object obj)
        {
            throw new NotImplementedException ();
        }
    }

    public class StringTypeWriter : AbstractTypeWriter
    {
        public StringTypeWriter (InitializationContext context)
        {
            Definition = context.AsMember ? "#s" : "#S";
        }

		public override void Write (SerializationContext context, object obj)
        {
            throw new NotImplementedException ();
        }
    }

	public class UnknownTypeWriter : AbstractTypeWriter
	{
		public UnknownTypeWriter (InitializationContext context)
		{
			Definition = context.AsMember ? "#s" : "#S";
		}

		public override void Write (SerializationContext context, object obj)
		{
			throw new NotImplementedException ();
		}
	}

    public class DictionaryTypeWriter : AbstractTypeWriter
    {
        public AbstractTypeWriter KeyTypeWriter { get; protected set; }
        public AbstractTypeWriter ValueTypeWriter { get; protected set; }

        public DictionaryTypeWriter (InitializationContext context)
        {
            KeyTypeWriter = TypeWriterFactory.CreateInstance (new InitializationContext (context, context.Type.GetGenericArguments () [0]) { AsMember = true });
            ValueTypeWriter = TypeWriterFactory.CreateInstance (new InitializationContext (context, context.Type.GetGenericArguments ()[1]) { AsMember = true });

            Definition = (context.AsMember ? "<d" : "<D")
                         + "k" + KeyTypeWriter.Definition
                         + "v" + ValueTypeWriter.Definition
                         + (context.AsMember ? "/d" : "/D");
        }

		public override void Write (SerializationContext context, object obj)
        {
			if (obj == null)
			{
				context.Writer.Write (context.Separator);
				context.Writer.Write ("0");
			}
			else
			{
				var dict = (IDictionary) obj;

				context.Writer.Write (context.Separator);
				context.Writer.Write (dict.Count.ToString (CultureInfo.InvariantCulture));

				foreach (DictionaryEntry k in dict)
				{

					WriteObject (writer, k.Key, table);
					WriteObject (writer, k.Value, table);
				}
			}
        }
    }

	public class ArrayTypeWriter : AbstractTypeWriter
	{
		public AbstractTypeWriter ValueTypeWriter
		{
			get;
			protected set;
		}

		public ArrayTypeWriter (InitializationContext context)
		{
			Type elementType = context.Type.IsArray ? context.Type.GetElementType () : context.Type.GetGenericArguments () [0];

			ValueTypeWriter = TypeWriterFactory.CreateInstance (new InitializationContext (context, elementType)
			{
				AsMember = false
			});

			Definition = (context.AsMember ? "<a" : "<A")
						 + ValueTypeWriter.Definition
						 + (context.AsMember ? "/a" : "/A");
		}

		public override void Write (SerializationContext context, object obj)
		{
			throw new NotImplementedException ();
		}
	}

	public class ObjectTypeWriter : AbstractTypeWriter
	{
		public AbstractTypeWriter [] Writers { get; protected set; }

		public ObjectTypeWriter (InitializationContext context)
		{
			var builder = new StringBuilder ();
			var writers = new List <AbstractTypeWriter> ();

			builder.Append (context.AsMember ? "<o" : "<O");
			foreach (var getter in Helpers.GetterCollection.FromType (context.Type))
			{
				var writer = TypeWriterFactory.CreateInstance (new InitializationContext (context, getter.PropertyType)
				{
					AsMember = true
				});

				builder.Append (getter.Name);
				builder.Append (writer.Definition);
				writers.Add (writer);
			}
			builder.Append (context.AsMember ? "/o" : "/O");

			Definition = builder.ToString ();
			Writers = writers.ToArray ();
		}

		public override void Write (SerializationContext context, object obj)
		{
			throw new NotImplementedException ();
		}
	}

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

                if (IsBooleanPrimitiveType (type))
                {
                    builder.Append (member ? "#b" : "#B");
                    return;
                }

                if (IsIntegerPrimitiveType (type))
                {
                    builder.Append (member ? "#i" : "#I");
                    return;
                }

                if (IsFloatingPointPrimitiveType (type))
                {
                    builder.Append (member ? "#f" : "#F");
                    return;
                }

                if (IsStringPrimitiveType (type))
                {
                    builder.Append (member ? "#s" : "#S");
                    return;
                }

                builder.Append (member ? "#u" : "#U");
                return;
            }

            // Dictionaries.
            if (IsGenericDictionary (type)) {
                builder.Append (member ? "<d" : "<D");

                builder.Append ("k");
                WriteType (type.GetGenericArguments () [0], builder, true);

                builder.Append ("v");
                WriteType (type.GetGenericArguments () [1], builder, true);

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

        private static bool IsPrimitiveType (Type type)
        {
            return IsBooleanPrimitiveType (type) ||
                   IsStringPrimitiveType (type) ||
                   IsIntegerPrimitiveType (type) ||
                   IsFloatingPointPrimitiveType (type) ||
                   type == typeof (DBNull) ||
                   type == typeof (char) ||
                   type == typeof (Guid) || type == typeof (Enum) ||
                   type == typeof (DateTime) ||
                   type == typeof (byte[]);
        }

        private static bool IsBooleanPrimitiveType (Type type)
        {
            return type == typeof (bool);
        }
        private static bool IsStringPrimitiveType (Type type)
        {
            return type == typeof (string);
        }

        private static bool IsIntegerPrimitiveType (Type type)
        {
            return
                type == typeof (byte) ||
                type == typeof (short) ||
                type == typeof (int) ||
                type == typeof (long) ||
                type == typeof (ushort) ||
                type == typeof (uint) ||
                type == typeof (ulong);
        }

        private static bool IsFloatingPointPrimitiveType (Type type)
        {
            return type == typeof (float) ||
                   type == typeof (double) ||
                   type == typeof (decimal);
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