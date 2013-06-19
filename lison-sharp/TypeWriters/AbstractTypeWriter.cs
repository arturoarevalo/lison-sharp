namespace Lison.TypeWriters
{
    public abstract class AbstractTypeWriter
    {
        public string Definition { get; protected set; }

        public abstract void Write (SerializationContext context, object obj);
    }
}