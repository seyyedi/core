using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Seyyedi
{
	public static class Kernel
	{
		public static Predicate<Assembly> AssemblyPredicate = a => false;

		static Lazy<IEnumerable<Assembly>> Assemblies = new Lazy<IEnumerable<Assembly>>(() =>
			new[] { typeof(Kernel).Assembly }
				.Concat(AppDomain.CurrentDomain
					.GetAssemblies()
					.Where(a => AssemblyPredicate(a))
				)
				.ToList()
		);

		static Lazy<IEnumerable<Type>> Types = new Lazy<IEnumerable<Type>>(() => Assemblies.Value
			.SelectMany(a => a.GetTypes())
			.Where(t => t.IsClass && !t.IsAbstract && t.IsPublic)
		);

		public static IEnumerable<Type> GetTypes()
			=> Types.Value;

		public static IEnumerable<Type> DerivedFrom<TBase>(this IEnumerable<Type> types)
			where TBase : class
			=> types
				.Where(t => typeof(TBase).IsAssignableFrom(t));

		public static IEnumerable<Type> DerivedFromGeneric(this IEnumerable<Type> types, Type genericBaseType)
			=> types
				.Where(t =>
				{
					var type = t;

					while (type != null && type != typeof(object))
					{
						if (type.IsGenericType && type.GetGenericTypeDefinition() == genericBaseType)
						{
							return true;
						}

						type = type.BaseType;
					}

					return false;
				});

		public static IEnumerable<Type> WithAttribute<TAttribute>(this IEnumerable<Type> types)
			where TAttribute : Attribute
			=> types
				.Select(t => new
				{
					Type = t,
					Attribute = t.GetCustomAttribute<TAttribute>()
				})
				.Where(e => e.Attribute != null)
				.Select(e => e.Type);
	}
}
