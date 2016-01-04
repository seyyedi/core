using System;

namespace Seyyedi.Api
{
	public class ApiEnvironment
	{
		public RabbitMqEnvironment RabbitMq { get; set; }

		public static readonly ApiEnvironment Local = new ApiEnvironment
		{
			RabbitMq = RabbitMqEnvironment.Local
		};

		public static ApiEnvironment Current;
	}

	public class RabbitMqEnvironment
	{
		public string Host { get; set; }
		public string User { get; set; }
		public string Password { get; set; }

		public string Namespace { get; set; } = "api";
		public string EventsExchange => $"{Namespace}.events";
		public string ResourcesExchange => $"{Namespace}.resources";

		public static readonly RabbitMqEnvironment Setup = new RabbitMqEnvironment
		{
			Host = "localhost",
			User = "guest",
			Password = "guest"
		};

		public static readonly RabbitMqEnvironment Local = new RabbitMqEnvironment
		{
			Host = "localhost",
			User = "api",
			Password = "9fcc35f24fab4e6fb1d56ada4764bdf2"
		};

		public static RabbitMqEnvironment Current => ApiEnvironment.Current.RabbitMq;
	}
}
