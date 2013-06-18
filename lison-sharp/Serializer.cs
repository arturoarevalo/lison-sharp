namespace Lison
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    public class StringTable
    {
        private Dictionary <string, int> table = new Dictionary <string, int> ();
        private List <string> data = new List <string> ();

        public int AddEntry (string str)
        {
            int index;
            if (!table.TryGetValue (str, out index))
            {
                index = data.Count;
                data.Add (str);
                table.Add (str, index);
            }

            return index;
        }

        public void Dump (TextWriter writer, char separator)
        {
            foreach (var str in data)
            {
                writer.Write (separator);
                writer.Write (str);
            }

            writer.Write (separator);
            writer.Write (data.Count.ToString(CultureInfo.InvariantCulture));
        }
    }

    public static class Serializer
    {
        public const string Version = "LiSON1";
        public const char Separator = '|';

        public static string Serialize <T> (T obj)
        {
            var writer = new StringWriter ();
            Serialize (writer, obj);

            return writer.ToString ();
        }

        public static void Serialize <T> (TextWriter writer, T obj)
        {
            StringTable table = new StringTable ();

            // Write version number.
            writer.Write (Version);

            // Write type definition.
            writer.Write (Separator);
            writer.Write (TypeDefinition.ForType (typeof (T)).Definition);

            // Write object data.
            WriteObject (writer, obj, table);

            // Write string table
            table.Dump (writer, Separator);
        }

        private static void WriteObject (TextWriter writer, object obj, StringTable table)
        {
            if (IsPrimitiveType (obj))
            {
                writer.Write (Separator);

                if (obj is String)
                {
                    if (!String.IsNullOrEmpty ((string) obj))
                    {
                        writer.Write (PrimitiveToString (table.AddEntry (obj as String) + 1));    
                    }
                }
                else
                {
                    writer.Write (PrimitiveToString (obj));
                }

                return;
            }

            if (obj is IDictionary)
            {
                var dict = obj as IDictionary;

                writer.Write (Separator);
                writer.Write (dict.Count.ToString ());

                foreach (DictionaryEntry k in dict)
                {
                    WriteObject (writer, k.Key, table);
                    WriteObject (writer, k.Value, table);
                }

                return;
            }

            if (obj is IEnumerable)
            {
                ICollection collection = (obj is ICollection) 
                    ? (ICollection) obj
                    : ((IEnumerable) obj).Cast<Object> ().ToArray ();
                
                writer.Write (Separator);
                writer.Write (collection.Count.ToString (CultureInfo.InvariantCulture));

                foreach (var item in collection)
                {
                    WriteObject (writer, item, table);
                }

                return;
            }


            if (obj == null)
            {
                writer.Write (Separator);
                writer.Write ('0');
            }
            else
            {
                writer.Write (Separator);

                foreach (var getter in Helpers.GetterCollection.FromType (obj.GetType ()))
                {
                    WriteObject (writer, getter.Method (obj), table);
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
            {
                return "";
            }

            if (obj is string)
            {
                return (string) obj;
            }

            /*
            if (obj is string)
            {
                return SentenceCompressor.Compress ((string) obj);
            }
            */

            if (obj is Guid)
            {
                return ((Guid) obj).ToString ();
            }

            if (obj is bool)
            {
                return ((bool) obj) ? "" : "0";
            }

            if (obj is int || obj is long || obj is double ||
                obj is decimal || obj is float ||
                obj is byte || obj is short ||
                obj is sbyte || obj is ushort ||
                obj is uint || obj is ulong || obj is char
                )
            {
                return ((IConvertible) obj).ToString (NumberFormatInfo.InvariantInfo);
            }

            if (obj is DateTime)
            {
                return String.Format ("{0}", obj);
            }

            if (obj is byte[])
            {
                return Convert.ToBase64String ((byte[]) obj, 0, ((byte[]) obj).Length, Base64FormattingOptions.None);
            }

            if (obj is Enum)
            {
                return ((Enum) obj).ToString ();
            }

            return String.Empty;
        }
    }
}