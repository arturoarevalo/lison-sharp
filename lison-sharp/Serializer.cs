namespace Lison
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    public static class Serializer
    {
        public const string Version = "LiSON1";
        public const char Separator = '|';

        public static bool InlineStrings { get; set; }

        public static string Serialize <T> (T obj)
        {
            var writer = new StringWriter ();
            Serialize (writer, obj);
            return writer.ToString ();
        }

        public static void Serialize <T> (TextWriter writer, T obj)
        {
            var typeWriter = TypeWriterFactory.CreateInstance (new InitializationContext (typeof (T))
                                                               {
                                                                   InlineStrings = InlineStrings
                                                               });

            var context = new SerializationContext
                          {
                              Writer = writer,
                              Separator = Separator,
                              StringTable = new StringTable ()
                          };

            // Write version number.
            context.Write (Version);

            // Write type definition.
            context.WritePrefixed (typeWriter.Definition);

            // Write object data.
            typeWriter.Write (context, obj);

            // Write string table
            context.StringTable.Dump (writer, Separator);
        }
    }
}