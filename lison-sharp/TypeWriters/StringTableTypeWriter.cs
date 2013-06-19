namespace Lison.TypeWriters
{
    using System;
    using System.Globalization;

    public class StringTableTypeWriter : AbstractTypeWriter
    {
        public StringTableTypeWriter (InitializationContext context)
        {
            Definition = context.AsMember ? "#t" : "#T";
        }

        public override void Write (SerializationContext context, object obj)
        {
            if (obj == null)
            {
                context.WritePrefixed ("0");
            }
            else
            {
                var str = (string) obj;

                context.WriteSeparator ();

                if (str != String.Empty)
                {
                    var index = context.StringTable.AddEntry (str) + 1;
                    context.Write (index.ToString (CultureInfo.InvariantCulture));
                }
            }
        }
    }
}