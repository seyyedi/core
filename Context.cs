using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;

namespace Seyyedi
{
	public class Context
	{
		static string _callContextId = typeof(Context).AssemblyQualifiedName;
		static ConcurrentDictionary<string, Context> _instances = new ConcurrentDictionary<string, Context>();

		public static Context Current
		{
			get
			{
				Context instance;
				var id = GetCurrentId();

				if (!string.IsNullOrEmpty(id) && _instances.TryGetValue(id, out instance))
				{
					return instance;
				}

				throw new InvalidOperationException("Context is not open");
			}
		}

		static string NewId() => Guid.NewGuid().ToString("n");
		static void ClearCurrentId() => CallContext.FreeNamedDataSlot(_callContextId);
		static string GetCurrentId() => CallContext.LogicalGetData(_callContextId) as string;
		static void SetCurrentId(string id) => CallContext.LogicalSetData(_callContextId, id);

		public static void Open() => Open(
			GetCurrentId() ?? NewId()
		);

		public static void Open(string id)
		{
			var instance = new Context(id);

			if (!_instances.TryAdd(id, instance))
			{
				throw new InvalidOperationException($"Unable to open context with id {id}");
			}

			SetCurrentId(id);

			instance.InvokeServices<IContextObserver>(o =>
				o.OnContextOpen()
			);
		}

		public static void Close()
		{
			Context instance;

			if (!_instances.TryRemove(GetCurrentId(), out instance))
			{
				throw new InvalidOperationException("Context is not opened");
			}

			instance.InvokeServices<IContextObserver>(o =>
				o.OnContextClose()
			);

			instance.InvokeServices<IDisposable>(d =>
				d.Dispose()
			);

			ClearCurrentId();
		}

		public static void Run(Action method)
		{
			Open();

			try
			{
				method();
			}
			finally
			{
				Close();
			}
		}

		public static async Task Run(Func<Task> method)
		{
			Open();

			try
			{
				await method();
			}
			finally
			{
				Close();
			}
		}

		public static IEnumerable<T> InitializeServices<T>()
			where T : class
			=> Kernel
				.GetTypes()
				.DerivedFrom<T>()
				.Select(t => GetService(t) as T);

		public static IEnumerable<object> InitializeServices(Predicate<Type> predicate)
			=> Kernel
				.GetTypes()
				.Where(t => predicate(t))
				.Select(t => GetService(t));

		public static T GetService<T>()
			where T : class
			=> GetService(typeof(T)) as T;

		public static object GetService(Type type)
		{
			var id = type.AssemblyQualifiedName;

			var services = Current._services;
			object service;

			if (!services.TryGetValue(id, out service))
			{
				var isServiceRegistered = Kernel
					.GetTypes()
					.Any(t => t == type);

				if (!isServiceRegistered)
				{
					throw new InvalidOperationException($"Type {type.FullName} is not registered in kernel");
				}

				service = Activator.CreateInstance(type);
				services.Add(id, service);
			}

			return service;
		}

		public string Id { get; private set; }
		Dictionary<string, object> _services = new Dictionary<string, object>();

		public Context(string id)
		{
			Id = id;
		}

		void InvokeServices<T>(Action<T> invoke)
			where T : class
			=> _services.Values
				.Select(v => v as T)
				.Where(s => s != null)
				.ForEach(invoke);
	}
}
