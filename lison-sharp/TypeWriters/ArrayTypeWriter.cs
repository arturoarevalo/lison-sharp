namespace Lison.TypeWriters
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Linq;

    public class ArrayTypeWriter : AbstractTypeWriter
    {
        public AbstractTypeWriter ValueTypeWriter
        {
            get;
            protected set;
        }

        public ArrayTypeWriter (InitializationContext context)
        {
            Type elementType = context.Type.IsArray ? context.Type.GetElementType () : context.Type.GetGenericArguments () [0];

            ValueTypeWriter = TypeWriterFactory.CreateInstance (new InitializationContext (context, elementType)
                                                                {
                                                                    AsMember = false
                                                                });

            Definition = (context.AsMember ? "<a" : "<A")
                         + ValueTypeWriter.Definition
                         + (context.AsMember ? "/a" : "/A");
        }

        public override void Write (SerializationContext context, object obj)
        {
            if (obj == null)
            {
                context.WritePrefixed ("0");
            }
            else
            {
                ICollection collection = (obj is ICollection)
                                             ? (ICollection) obj
                                             : ((IEnumerable) obj).Cast <Object> ().ToArray ();

                context.WritePrefixed (collection.Count.ToString (CultureInfo.InvariantCulture));

                foreach (var item in collection)
                {
                    ValueTypeWriter.Write (context, item);
                }
            }
        }
    }
}