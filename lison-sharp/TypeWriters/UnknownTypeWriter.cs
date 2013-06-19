namespace Lison.TypeWriters
{
    public class UnknownTypeWriter : AbstractTypeWriter
    {
        public UnknownTypeWriter (InitializationContext context)
        {
            Definition = context.AsMember ? "#s" : "#S";
        }

        public override void Write (SerializationContext context, object obj)
        {
            context.WriteSeparator ();

            if (obj != null)
            {
                context.Write (obj.ToString ());
            }
        }
    }
}