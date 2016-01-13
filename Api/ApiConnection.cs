using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

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
				Log.Warn("RabbitMq already online");
				return;
			}

			Log.Info("Opening connection to RabbitMq");

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

				_channel.ExchangeDeclare(RabbitMqEnvironment.Current.EventsExchange, "topic", true, false, null);
				_channel.ExchangeDeclare(RabbitMqEnvironment.Current.RequestsExchange, "direct", true, false, null);
				_channel.ExchangeDeclare(RabbitMqEnvironment.Current.ResponsesExchange, "direct", true, false, new Dictionary<string, object>
				{
					{ "alternate-exchange", RabbitMqEnvironment.Current.RetryResponsesExchange }
				});

				_channel.ExchangeDeclare(RabbitMqEnvironment.Current.RetryResponsesExchange, "topic", true, false, null);
				_channel.QueueDeclare(RabbitMqEnvironment.Current.RetryResponsesQueue, true, false, false, new Dictionary<string, object>
				{
					{ "x-dead-letter-exchange", RabbitMqEnvironment.Current.ResponsesExchange },
					{ "x-message-ttl", 10000 }
				});
				_channel.QueueBind(RabbitMqEnvironment.Current.RetryResponsesQueue, RabbitMqEnvironment.Current.RetryResponsesExchange, "#");

				Log.Info("RabbitMq is online");
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
				throw;
			}
		}

		public void Close()
		{
			Log.Info("Closing connection to RabbitMq");

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

			Log.Info("RabbitMq is offline");
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

		byte[] Serialize(object obj)
		{
			return Encoding.UTF8.GetBytes(
				JsonConvert.SerializeObject(obj, _jsonSettings)
			);
		}

		T Deserialize<T>(byte[] data)
		{
			return JsonConvert.DeserializeObject<T>(
				Encoding.UTF8.GetString(data),
				_jsonSettings
			);
		}

		public ApiConnection Subscribe(string pattern, Action<ApiEvent<object>> handler, bool noAck = false)
		{
			return Subscribe<object>(pattern, handler, noAck);
		}

		public ApiConnection Subscribe(string pattern, Func<ApiEvent<object>, Task> handler, bool noAck = false)
		{
			return Subscribe<object>(pattern, handler, noAck);
		}

		public ApiConnection Subscribe<T>(string pattern, Action<ApiEvent<T>> handler, bool noAck = false)
		{
			return Subscribe<T>(pattern, @event => { handler(@event); return Task.FromResult(true); }, noAck);
		}

		public ApiConnection Subscribe<T>(string pattern, Func<ApiEvent<T>, Task> handler, bool noAck = false)
		{
			Log.Debug($"Subscribing to events with pattern {pattern}");

			var id = Guid.NewGuid();
			var queue = $"{RabbitMqEnvironment.Current.EventsExchange}.{id:N}";

			_channel.QueueDeclare(queue, false, false, true, null);
			_channel.QueueBind(queue, RabbitMqEnvironment.Current.EventsExchange, pattern);

			var consumer = new EventingBasicConsumer(_channel);
			consumer.Received += async (sender, msg) =>
			{
				Log.Debug($"Incoming message with routing key {msg.RoutingKey}");

				try
				{
					var apiEvent = Deserialize<ApiEvent<T>>(msg.Body);

					await handler(apiEvent);

					if (!noAck)
					{
						_channel.BasicAck(msg.DeliveryTag, false);
					}
				}
				catch (Exception ex)
				{
					Log.Exception(ex);

					if (!noAck)
					{
						_channel.BasicNack(msg.DeliveryTag, false, false);
					}
				}
			};

			_channel.BasicConsume(queue, noAck, consumer);

			return this;
		}

		public ApiConnection Publish<T>(ApiEvent<T> apiEvent)
		{
			_channel.BasicPublish(RabbitMqEnvironment.Current.EventsExchange, apiEvent.Id, null,
				Serialize(apiEvent)
			);

			return this;
		}

		public ApiConnection Request(string path, Func<Dictionary<string, object>, object> handler, bool noAck = false)
		{
			return Request(path, req => Task.FromResult(handler(req)), noAck);
		}

		public ApiConnection Request(string path, Func<Dictionary<string, object>, Task<object>> handler, bool noAck = false)
		{
			return Request<Dictionary<string, object>>(path, handler, noAck);
		}

		public ApiConnection Request<T>(string path, Func<T, object> handler, bool noAck = false)
		{
			return Request<T>(path, req => Task.FromResult(handler(req)), noAck);
		}

		public ApiConnection Request<T>(string path, Func<T, Task<object>> handler, bool noAck = false)
		{
			Log.Debug($"Handling requests to {path}");

			var id = Guid.NewGuid();
			var queue = $"{RabbitMqEnvironment.Current.Namespace}/{path}";

			_channel.QueueDeclare(queue, true, false, false, null);
			_channel.QueueBind(queue, RabbitMqEnvironment.Current.RequestsExchange, path);

			var consumer = new EventingBasicConsumer(_channel);
			consumer.Received += async (sender, msg) =>
			{
				Log.Debug($"Incoming message with routing key {msg.RoutingKey}");

				if (!msg.BasicProperties.IsCorrelationIdPresent())
				{
					Log.Warn("No correlation id present");

					if (!noAck)
					{
						_channel.BasicNack(msg.DeliveryTag, false, false);
					}

					return;
				}

				if (!msg.BasicProperties.IsMessageIdPresent())
				{
					Log.Warn("No message id present");

					if (!noAck)
					{
						_channel.BasicNack(msg.DeliveryTag, false, false);
					}

					return;
				}

				try
				{
					var req = JsonConvert.DeserializeObject<T>(
						Encoding.UTF8.GetString(msg.Body),
						_jsonSettings
					);

					var resp = await handler(req);

					var replyProps = _channel.CreateBasicProperties();
					replyProps.CorrelationId = msg.BasicProperties.CorrelationId;
					replyProps.MessageId = msg.BasicProperties.MessageId;

					_channel.BasicPublish(RabbitMqEnvironment.Current.ResponsesExchange, msg.BasicProperties.ReplyTo, replyProps,
						Serialize(resp)
					);

					if (!noAck)
					{
						_channel.BasicAck(msg.DeliveryTag, false);
					}
				}
				catch (Exception ex)
				{
					Log.Exception(ex);

					if (!noAck)
					{
						_channel.BasicNack(msg.DeliveryTag, false, false);
					}
				}
			};

			_channel.BasicConsume(queue, noAck, consumer);

			return this;
		}
	}
}