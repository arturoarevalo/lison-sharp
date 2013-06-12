using System.Threading.Tasks;

namespace Lison
{
    using System.IO;
    using System.Linq;
    using System.Reflection;
	using System.Reflection.Emit;

    internal class Program
    {
        public static System.Random randomizer = new System.Random ((int) System.DateTime.Now.Ticks);
        public static int GetRandomInteger (int maxValue)
        {
            return randomizer.Next (1, maxValue);
        }

        public static string GetRandomString (int length)
        {
            string[] array = new string[54]
                             {
                                 "0", "2", "3", "4", "5", "6", "8", "9",
                                 "a", "b", "c", "d", "e", "f", "g", "h", "j", "k", "m", "n", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z",
                                 "A", "B", "C", "D", "E", "F", "G", "H", "J", "K", "L", "M", "N", "P", "R", "S", "T", "U", "V", "W", "X", "Y", "Z"
                             };
            System.Text.StringBuilder sb = new System.Text.StringBuilder ();
            for (int i = 0; i < length; i++) sb.Append (array [GetRandomInteger (53)]);
            return sb.ToString ();
        }

        class Card
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Address { get; set; }
            public Additional[] AdditionalData { get; set; }
        }

        class Additional
        {
            public string Field1 { get; set; }
            public string Field2 { get; set; }
            public string Field3 { get; set; }
        }

		private static void Main (string [] args)
		{

		    var test = Enumerable
		        .Range (0, 500)
		        .Select (x => new Card
		                      {
                                  Id = GetRandomInteger (5000),
                                  Name = GetRandomString (30),
                                  Address = GetRandomString (50),
                                  AdditionalData = Enumerable.Range (0, 5).Select (y => new Additional
                                                                                        {
                                                                                            Field1 = GetRandomString (10),
                                                                                            Field2 = GetRandomString (10),
                                                                                            Field3 = GetRandomString (10)
                                                                                        }).ToArray ()
		                      })
		        .ToArray ();

			var writer = new StringWriter ();

            Serializer.Serialize (writer, test);

//			System.Console.WriteLine (writer.ToString ());
            System.IO.File.WriteAllText ("c:\\tmp\\test.lison", writer.ToString ());
			System.Console.ReadKey ();


		}
	}
}
