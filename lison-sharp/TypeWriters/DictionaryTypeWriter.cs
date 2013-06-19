namespace Lison.TypeWriters
{
    using System.Collections;
    using System.Globalization;

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
                context.WritePrefixed ("0");
            }
            else
            {
                var dict = (IDictionary) obj;

                context.WritePrefixed (dict.Count.ToString (CultureInfo.InvariantCulture));

                foreach (DictionaryEntry k in dict)
                {
                    KeyTypeWriter.Write (context, k.Key);
                    ValueTypeWriter.Write (context, k.Value);
                }
            }
        }
    }
}