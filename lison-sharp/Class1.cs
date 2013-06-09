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

	public static class Serializer
	{
		public static void WriteType (Type type, TextWriter writer, bool member = false)
		{
			if (IsPrimitiveType (type))
			{
				writer.Write (member ? "#s" : "%s");
				return;
			}

			if (IsGenericDictionary (type))
			{
				writer.Write (member ? "#D" : "%D");

				writer.Write ("k");
				WriteType (type.GetGenericArguments () [0], writer);

				writer.Write ("v");
				WriteType (type.GetGenericArguments () [1], writer);

				writer.Write (member ? "#d" : "%d");

				return;
			}

			if (type.IsArray || IsGenericEnumerable (type))
			{
				Type elementType = type.IsArray ? type.GetElementType () : type.GetGenericArguments () [0];

				writer.Write (member ? "#A" : "%A");
				WriteType (elementType, writer, false);
				writer.Write (member ? "#a" : "%a");

				return;
			}

			writer.Write (member ? "#O" : "%O");
			foreach (var getter in Helpers.GetterCollection.FromType (type))
			{
				writer.Write (getter.Name);
				WriteType (getter.PropertyType, writer, true);
			}
			writer.Write (member ? "#o" : "%o");
		}

		public static bool IsGenericDictionary (Type type)
		{
			foreach (Type iType in type.GetInterfaces ())
			{
				if (iType.IsGenericType && iType.GetGenericTypeDefinition ()
				    == typeof (IDictionary <,>))
				{
					return true;
				}
			}

			return false;
		}



		public static bool IsGenericEnumerable (Type type)
		{
			foreach (Type iType in type.GetInterfaces ())
			{
				if (iType.IsGenericType && iType.GetGenericTypeDefinition ()
				    == typeof (IEnumerable <>))
				{
					return true;
				}
			}

			return false;
		}


		public static void WriteDefinition (object obj, TextWriter writer)
		{
			if (IsPrimitiveType (obj))
			{
				writer.Write ("#s");
				return;
			}

			if (obj is IDictionary)
			{
				var dict = obj as IDictionary;

				writer.Write (dict.Count.ToString ());
				writer.Write ((char) 0x000C);

				foreach (DictionaryEntry k in dict)
				{
					WriteObject (k.Key, writer);
					WriteObject (k.Value, writer);
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

				writer.Write (count.ToString ());
				writer.Write ((char) 0x000C);

				foreach (var item in enu)
				{
					WriteObject (item, writer);
				}

				return;
			}

			foreach (var getter in Helpers.GetterCollection.FromType (obj.GetType ()))
			{
				WriteObject (getter.Method (obj), writer);
			}
		}


		public static void WriteObject (object obj, TextWriter writer)
		{
			if (IsPrimitiveType (obj))
			{
				writer.Write (PrimitiveToString (obj));
				writer.Write ((char) 0x000C);
				return;
			}

			if (obj is IDictionary)
			{
				var dict = obj as IDictionary;

				writer.Write (dict.Count.ToString ());
				writer.Write ((char) 0x000C);

				foreach (DictionaryEntry k in dict)
				{
					WriteObject (k.Key, writer);
					WriteObject (k.Value, writer);
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

				writer.Write (count.ToString ());
				writer.Write ((char) 0x000C);

				foreach (var item in enu)
				{
					WriteObject (item, writer);
				}

				return;
			}

			foreach (var getter in Helpers.GetterCollection.FromType (obj.GetType ()))
			{
				WriteObject (getter.Method (obj), writer);
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
