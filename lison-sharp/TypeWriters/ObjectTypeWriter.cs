namespace Lison.TypeWriters
{
    using System.Collections.Generic;
    using System.Text;
    using Lison.Helpers;

    public class ObjectTypeWriter : AbstractTypeWriter
    {
        public PropertyDefinition[] Properties { get; protected set; }

        public class PropertyDefinition
        {
            public AbstractTypeWriter TypeWriter { get; set; }
            public GenericGetter Getter { get; set; }
        }

        public ObjectTypeWriter (InitializationContext context)
        {
            var builder = new StringBuilder ();
            var properties = new List<PropertyDefinition> ();

            builder.Append (context.AsMember ? "<o" : "<O");
            foreach (var getter in Helpers.GetterCollection.FromType (context.Type))
            {
                var writer = TypeWriterFactory.CreateInstance (new InitializationContext (context, getter.PropertyType)
                                                               {
                                                                   AsMember = true
                                                               });

                builder.Append (getter.Name);
                builder.Append (writer.Definition);
                properties.Add (new PropertyDefinition
                                {
                                    TypeWriter = writer,
                                    Getter = getter.Method
                                });
            }
            builder.Append (context.AsMember ? "/o" : "/O");

            Definition = builder.ToString ();
            Properties = properties.ToArray ();
        }

        public override void Write (SerializationContext context, object obj)
        {
            if (obj == null)
            {
                context.WritePrefixed ("0");
            }
            else
            {
                context.WriteSeparator ();

                foreach (var property in Properties)
                {
                    property.TypeWriter.Write (context, property.Getter (obj));
                }
            }
        }
    }
}