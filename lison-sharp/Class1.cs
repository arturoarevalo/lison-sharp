using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lison
{
	using System.Collections;
	using System.Globalization;
	using System.IO;
	using System.Reflection;
	using System.Reflection.Emit;


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

	public static class Serializer
	{
		public const string Version = "LiSON1";
		public const char Separator = '|';

		public static void Serialize <T> (TextWriter writer, T obj)
		{
			// Write version number.
			writer.Write (Version);

			// Write type definition.
			writer.Write (Separator);
			writer.Write (TypeDefinition.ForType (typeof (T)).Definition);

			// Write object data.
			WriteObject (writer, obj);
		}

		private static void WriteObject (TextWriter writer, object obj )
		{
			if (IsPrimitiveType (obj))
			{
				writer.Write (Separator);
				writer.Write (PrimitiveToString (obj));
				return;
			}

			if (obj is IDictionary)
			{
				var dict = obj as IDictionary;

				writer.Write (Separator);
				writer.Write (dict.Count.ToString ());

				foreach (DictionaryEntry k in dict)
				{
					WriteObject (writer, k.Key);
					WriteObject (writer, k.Value);
				}

				return;
			}

			if (obj is IEnumerable)
			{
				var enu = obj as IEnumerable;
				int count = 0;

				if (enu is ICollection)
				{
					count = (enu as ICollection).Count;
				}
				else
				{
					foreach (var item in enu)
					{
						count++;
					}
				}

				writer.Write (Separator);
				writer.Write (count.ToString ());

				foreach (var item in enu)
				{
					WriteObject (writer, item);
				}

				return;
			}


			if (obj == null)
			{
				writer.Write (Separator);
				writer.Write ('0');
				writer.Write (Separator);
			}
			else
			{
				writer.Write (Separator);
				writer.Write (Separator);

				foreach (var getter in Helpers.GetterCollection.FromType (obj.GetType ()))
				{
					WriteObject (writer, getter.Method (obj));
				}
			}
		}

		private static bool IsPrimitiveType (object obj)
		{
			return obj is DBNull ||
			       obj is string || obj is char ||
			       obj is Guid || obj is Enum ||
			       obj is bool || obj is DateTime ||
			       obj is int || obj is long || obj is double ||
			       obj is decimal || obj is float ||
			       obj is byte || obj is short ||
			       obj is sbyte || obj is ushort ||
			       obj is uint || obj is ulong ||
			       obj is byte [];
		}


		private static string PrimitiveToString (object obj)
		{
			if (obj == null || obj is DBNull)
				return "";

			else if (obj is string)
				return (string) obj;

			else if (obj is Guid)
				return ((Guid) obj).ToString ();

			else if (obj is bool)
				return ((bool) obj) ? "true" : "false";

			else if (
				obj is int || obj is long || obj is double ||
				obj is decimal || obj is float ||
				obj is byte || obj is short ||
				obj is sbyte || obj is ushort ||
				obj is uint || obj is ulong || obj is char
				)
				return ((IConvertible) obj).ToString (NumberFormatInfo.InvariantInfo);

			else if (obj is DateTime)
				return String.Format ("{0}", obj);

			else if (obj is byte [])
				return Convert.ToBase64String ((byte []) obj, 0, ((byte []) obj).Length, Base64FormattingOptions.None);

			else if (obj is Enum)
				return ((Enum) obj).ToString ();

			else
				return String.Empty;
		}
	}

	internal class Program
	{
		private static void Main (string [] args)
		{

			var test = new []
			{
				new
				{
					v1 = "Hola",
					v2 = "Mundo",
					v3 = new
					{
						v31 = "v3 Hola",
						v32 = "v3 Mundo"
					},
					v4 = new []
					{
						"item 1", "item 2", "item 3", "item 4", "item 5"
					}
				},
				new
				{
					v1 = "Hola",
					v2 = "Mundo",
					v3 = new
					{
						v31 = "v3 Hola",
						v32 = "v3 Mundo"
					},
					v4 = new []
					{
						"item 1", "item 2", "item 3", "item 4", "item 5"
					}
				},
				new
				{
					v1 = "Hola",
					v2 = "Mundo",
					v3 = new
					{
						v31 = "v3 Hola",
						v32 = "v3 Mundo"
					},
					v4 = new []
					{
						"item 1", "item 2", "item 3", "item 4", "item 5"
					}
				},
				new
				{
					v1 = "Hola",
					v2 = "Mundo",
					v3 = new
					{
						v31 = "v3 Hola",
						v32 = "v3 Mundo"
					},
					v4 = new []
					{
						"item 1", "item 2", "item 3", "item 4", "item 5"
					}
				},
				new
				{
					v1 = "Hola",
					v2 = "Mundo",
					v3 = new
					{
						v31 = "v3 Hola",
						v32 = "v3 Mundo"
					},
					v4 = new []
					{
						"item 1", "item 2", "item 3", "item 4", "item 5"
					}
				}
			};



			var writer = new StringWriter ();
			Serializer.WriteType (test.GetType (), writer);
			writer.Write ((char) 0x000C);
			Serializer.WriteObject (test, writer);



			System.Console.WriteLine (writer.ToString ());
			System.Console.ReadKey ();


		}
	}
}
