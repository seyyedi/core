using System;
using System.Threading.Tasks;

namespace Seyyedi.Api
{
	public static class ApiEvents
	{
		public static async Task Publish(ApiEvent apiEvent)
		{

		}

		public static void Subscribe(string pattern, Action<ApiEvent> listener)
		{

		}
	}
}
