namespace Lison
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;

    public class StringTable
    {
        private Dictionary <string, int> table = new Dictionary <string, int> ();
        private List <string> data = new List <string> ();

        public int AddEntry (string str)
        {
            int index;
            if (!table.TryGetValue (str, out index))
            {
                index = data.Count;
                data.Add (str);
                table.Add (str, index);
            }

            return index;
        }

        public void Dump (TextWriter writer, char separator)
        {
            foreach (var str in data)
            {
                writer.Write (separator);
                writer.Write (str);
            }

            writer.Write (separator);
            writer.Write (data.Count.ToString(CultureInfo.InvariantCulture));
        }
    }
}