namespace Lison
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;

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
            }
            else
            {
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
            {
                return "";
            }

            if (obj is string)
            {
                return (string) obj;
            }

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