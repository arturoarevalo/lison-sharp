using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lison.Helpers
{
	using System.Reflection;
	using System.Reflection.Emit;

	public class GetterCollection : List <Getter>
	{
		private static readonly Dictionary <Type, GetterCollection> cache = new Dictionary<Type, GetterCollection> ();

		public GetterCollection (Type type)
		{
			Initialize (type);
		}

		public static GetterCollection FromType (Type type)
		{
			lock (cache) {
				GetterCollection getters;
				if (!cache.TryGetValue (type, out getters)) {
					getters = new GetterCollection (type);
					cache.Add (type, getters);
				}

				return getters;
			}
		}

		private void Initialize (Type type)
		{
			PropertyInfo [] properties = type.GetProperties (BindingFlags.Public | BindingFlags.Instance);
			foreach (PropertyInfo property in properties)
			{
				//if (!p.CanWrite) continue;

				object [] attributes = property.GetCustomAttributes (typeof (System.Xml.Serialization.XmlIgnoreAttribute), false);
				if (attributes != null && attributes.Length > 0)
				{
					continue;
				}

				GenericGetter method = CreateGetMethod (property);
				if (method != null)
				{
					Add (new Getter
					{
						Name = property.Name,
						Method = method,
						PropertyType = property.PropertyType
					});
				}
			}
		}

		private static GenericGetter CreateGetMethod (PropertyInfo propertyInfo)
		{
			MethodInfo getMethod = propertyInfo.GetGetMethod (true);
			if (getMethod == null)
			{
				return null;
			}

			Type [] arguments = new Type [1];
			arguments [0] = typeof (object);

			DynamicMethod getter = new DynamicMethod ("_", typeof (object), arguments, true);
			ILGenerator il = getter.GetILGenerator ();
			il.Emit (OpCodes.Ldarg_0);
			il.Emit (OpCodes.Castclass, propertyInfo.DeclaringType);
			il.EmitCall (OpCodes.Callvirt, getMethod, null);

			if (!propertyInfo.PropertyType.IsClass)
			{
				il.Emit (OpCodes.Box, propertyInfo.PropertyType);
			}

			il.Emit (OpCodes.Ret);

			return (GenericGetter)getter.CreateDelegate (typeof (GenericGetter));
		}
	}
}