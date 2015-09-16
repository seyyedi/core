using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Seyyedi
{
	public static class JsonFile
	{
		static JsonSerializerSettings _settings = new JsonSerializerSettings
		{
			Formatting = Formatting.Indented,
			TypeNameHandling = TypeNameHandling.Auto
		};

		public static T Load<T>(FileInfo file)
		{
			var json = File.ReadAllText(file.FullName, Encoding.UTF8);

			return JsonConvert.DeserializeObject<T>(json, _settings);
		}

		public static void Save<T>(T obj, FileInfo file, bool writeTypes = false)
		{
			var json = JsonConvert.SerializeObject(obj, _settings);

			file.Directory.Ensure();

			File.WriteAllText(file.FullName, json, Encoding.UTF8);
		}
	}
}
