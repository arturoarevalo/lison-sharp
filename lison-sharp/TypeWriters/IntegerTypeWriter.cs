namespace Lison.TypeWriters
{
    using System;
    using System.Globalization;

    public class IntegerTypeWriter : AbstractTypeWriter
    {
        public IntegerTypeWriter (InitializationContext context)
        {
            Definition = context.AsMember ? "#i" : "#I";
        }

        public override void Write (SerializationContext context, object obj)
        {
            context.WritePrefixed (((IConvertible) obj).ToString (NumberFormatInfo.InvariantInfo));
        }
    }
}