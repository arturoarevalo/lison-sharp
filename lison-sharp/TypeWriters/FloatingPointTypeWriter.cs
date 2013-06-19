namespace Lison.TypeWriters
{
    using System;
    using System.Globalization;

    public class FloatingPointTypeWriter : AbstractTypeWriter
    {
        public FloatingPointTypeWriter (InitializationContext context)
        {
            Definition = context.AsMember ? "#f" : "#F";
        }

        public override void Write (SerializationContext context, object obj)
        {
            context.WritePrefixed (((IConvertible) obj).ToString (NumberFormatInfo.InvariantInfo));
        }
    }
}