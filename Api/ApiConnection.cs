using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace Seyyedi.Api
{
	public class ApiConnection : IDisposable
	{
		static readonly ILog Log = Logs.Get();
		static JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
		{
			ContractResolver = new CamelCasePropertyNamesContractResolver()
		};

		IConnection _connection;
		IModel _channel;

		public void Open()
		{
			if (_connection != null)
			{
				Log.Warn("Connection to rabbitmq already opened");
				return;
			}

			Log.Info("Opening connection to rabbitmq");

			try
			{
				_connection = new ConnectionFactory
				{
					HostName = RabbitMqEnvironment.Current.Host,
					VirtualHost = "/",
					UserName = RabbitMqEnvironment.Current.User,
					Password = RabbitMqEnvironment.Current.Password,
					AutomaticRecoveryEnabled = true,
					TopologyRecoveryEnabled = true
				}.CreateConnection();

				_channel = _connection.CreateModel();

				_channel.ExchangeDeclare(RabbitMqEnvironment.Current.EventsExchange, "topic", true);
				_channel.ExchangeDeclare(RabbitMqEnvironment.Current.ResourcesExchange, "direct", true);
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
				throw;
			}
		}

		public void Close()
		{
			Log.Info("Closing connection to rabbitmq");

			if (_channel != null)
			{
				_channel.Close();
				_channel.Dispose();
				_channel = null;
			}

			if (_connection != null)
			{
				_connection.Close();
				_connection.Dispose();
				_connection = null;
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposing)
			{
				return;
			}

			Close();
		}

		public void Dispose()
		{
			Dispose(true);
		}

		public ApiConnection Subscribe<T>(string pattern, Action<ApiEvent<T>> handler)
		{
			Log.Debug($"Subscribing to events with pattern {pattern}");

			var id = Guid.NewGuid();
			var queue = $"api.events.{id:N}";

			_channel.QueueDeclare(queue, false, false, true, null);
			_channel.QueueBind(queue, RabbitMqEnvironment.Current.EventsExchange, pattern);

			var consumer = new EventingBasicConsumer(_channel);
			consumer.Received += (model, eventArgs) =>
			{
				Log.Debug($"Incoming message with routing key {eventArgs.RoutingKey}");

				try
				{
					var e = JsonConvert.DeserializeObject<ApiEvent<T>>(
						Encoding.UTF8.GetString(eventArgs.Body),
						_jsonSettings
					);

					handler(e);

					_channel.BasicAck(eventArgs.DeliveryTag, false);
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
					_channel.BasicNack(eventArgs.DeliveryTag, false, false);
				}
			};

			_channel.BasicConsume(queue, false, consumer);

			return this;
		}

		public ApiConnection Publish<T>(ApiEvent<T> apiEvent)
		{
			var body = Encoding.UTF8.GetBytes(
				JsonConvert.SerializeObject(apiEvent, _jsonSettings)
			);

			_channel.BasicPublish(RabbitMqEnvironment.Current.EventsExchange, apiEvent.Id, null, body);

			return this;
		}
	}
}
