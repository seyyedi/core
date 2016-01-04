using System;
using System.Diagnostics;
using log4net;

namespace Seyyedi
{
	public static class ObjectExtensions
	{
		public static string ToJson(this object obj)
			=> Newtonsoft.Json.JsonConvert.SerializeObject(obj);
	}
}
