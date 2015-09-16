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

		public static IEnumerable<Type> GetTypesWithBaseClass<TBaseClass>()
			where TBaseClass : class
			=> GetTypes()
				.Where(t => typeof(TBaseClass).IsAssignableFrom(t));

		public static IEnumerable<Type> GetTypesWithAttribute<TAttribute>()
			where TAttribute : Attribute
			=> GetTypes()
				.Select(t => new
				{
					Type = t,
					Attribute = t.GetCustomAttribute<TAttribute>()
				})
				.Where(e => e.Attribute != null)
				.Select(e => e.Type);
	}
}
