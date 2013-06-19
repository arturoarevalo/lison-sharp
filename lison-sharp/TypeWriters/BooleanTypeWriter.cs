namespace Lison.TypeWriters
{
    public class BooleanTypeWriter : AbstractTypeWriter
    {
        public BooleanTypeWriter (InitializationContext context)
        {
            Definition = context.AsMember ? "#b" : "#B";
        }

        public override void Write (SerializationContext context, object obj)
        {
            context.WritePrefixed (((bool) obj) ? "" : "0");
        }
    }
}