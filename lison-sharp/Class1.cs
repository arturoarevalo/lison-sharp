using System.Threading.Tasks;

namespace Lison
{
    using System.Collections.Generic;
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

            public Card ()
            {
                Id = GetRandomInteger (5000);
                Name = "This is a small string";
                Address = "Nothing is more difficult, and therefore more precious, than to be able to decide";
                AdditionalData = Enumerable.Range (0, 5).Select (y => new Additional ()).ToArray ();

            }
        }

        class Additional
        {
            public string Field1 { get; set; }
            public string Field2 { get; set; }
            public string Field3 { get; set; }

            public Additional ()
            {
                Field1 = "this is an example of what works very well with smaz";
                Field2 = "the end";
                Field2 = null;
                Field3 = "Nel mezzo del cammin di nostra vita, mi ritrovai in una selva oscura";
                Field3 = "";
            }
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

		    var test2 = new Dictionary <string, Card>
		                {
		                    { "Key1", new Card () },
		                    { "Key2", new Card () },
		                    { "Key3", new Card () },
		                    { "Key4", new Card () },
		                    { "Key5", new Card () }
		                };

                var writer = new StringWriter ();

            Serializer.Serialize (writer, test2);

//			System.Console.WriteLine (writer.ToString ());
            System.IO.File.WriteAllText ("c:\\tmp\\test.lison", writer.ToString ());
			System.Console.ReadKey ();


		}
	}
}
