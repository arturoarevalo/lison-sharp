namespace Lison.TypeWriters
{
    using System;

    public class StringTypeWriter : AbstractTypeWriter
    {
        public StringTypeWriter (InitializationContext context)
        {
            Definition = context.AsMember ? "#s" : "#S";
        }

        public override void Write (SerializationContext context, object obj)
        {
            context.WriteSeparator ();

            if (!String.IsNullOrEmpty ((string) obj))
            {
                context.Write ((string) obj);
            }
        }
    }
}