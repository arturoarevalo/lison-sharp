namespace Lison
{
    using System.IO;

    public class SerializationContext
    {
        public TextWriter Writer { get; set; }
        public char Separator { get; set; }
		public StringTable StringTable { get; set; }


        public void Write (string data)
        {
            Writer.Write (data);
        }

        public void WritePrefixed (string data)
        {
            Writer.Write (Separator);
            Writer.Write (data);
        }

        public void WritePostfixed (string data)
        {
            Writer.Write (data);
            Writer.Write (Separator);
        }

        public void WriteSeparator ()
        {
            Writer.Write (Separator);
        }
    }
}