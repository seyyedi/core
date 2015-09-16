using System;
using System.IO;
using System.Text;

namespace Seyyedi
{
	public static class StreamExtensions
	{
		public static byte[] ReadBytes(this Stream stream, int count)
		{
			var bytes = new byte[count];

			stream.Read(bytes, 0, count);

			return bytes;
		}

		public static Int16 ReadInt16(this Stream stream)
		{
			return BitConverter.ToInt16(
				ReadBytes(stream, 2),
				0
			);
		}

		public static Int32 ReadInt32(this Stream stream)
		{
			return BitConverter.ToInt32(
				ReadBytes(stream, 4),
				0
			);
		}

		public static Int64 ReadInt64(this Stream stream)
		{
			return BitConverter.ToInt64(
				ReadBytes(stream, 8),
				0
			);
		}

		public static UInt16 ReadUInt16(this Stream stream)
		{
			return BitConverter.ToUInt16(
				ReadBytes(stream, 2),
				0
			);
		}

		public static UInt32 ReadUInt32(this Stream stream)
		{
			return BitConverter.ToUInt32(
				ReadBytes(stream, 4),
				0
			);
		}

		public static UInt64 ReadUInt64(this Stream stream)
		{
			return BitConverter.ToUInt64(
				ReadBytes(stream, 8),
				0
			);
		}

		public static Guid ReadGuid(this Stream stream)
		{
			return new Guid(
				ReadBytes(stream, 16)
			);
		}

		public static string ReadString(this Stream stream, Encoding encoding = null)
		{
			if (encoding == null)
			{
				encoding = Encoding.UTF8;
			}

			var length = ReadInt32(stream);

			return encoding.GetString(
				ReadBytes(stream, length)
			);
		}

		public static T WriteBytes<T>(this T stream, byte[] bytes) where T : Stream
		{
			stream.Write(bytes, 0, bytes.Length);

			return stream;
		}

		public static T WriteInt16<T>(this T stream, Int16 value) where T : Stream
		{
			return WriteBytes<T>(stream,
				BitConverter.GetBytes(value)
			);
		}

		public static T WriteInt32<T>(this T stream, Int32 value) where T : Stream
		{
			return WriteBytes<T>(stream,
				BitConverter.GetBytes(value)
			);
		}

		public static T WriteInt64<T>(this T stream, Int64 value) where T : Stream
		{
			return WriteBytes<T>(stream,
				BitConverter.GetBytes(value)
			);
		}

		public static T WriteUInt16<T>(this T stream, UInt16 value) where T : Stream
		{
			return WriteBytes<T>(stream,
				BitConverter.GetBytes(value)
			);
		}

		public static T WriteUInt32<T>(this T stream, UInt32 value) where T : Stream
		{
			return WriteBytes<T>(stream,
				BitConverter.GetBytes(value)
			);
		}

		public static T WriteUInt64<T>(this T stream, UInt64 value) where T : Stream
		{
			return WriteBytes<T>(stream,
				BitConverter.GetBytes(value)
			);
		}

		public static T WriteGuid<T>(this T stream, Guid value) where T : Stream
		{
			return WriteBytes<T>(stream,
				value.ToByteArray()
			);
		}

		public static T WriteString<T>(this T stream, string value, params object[] args) where T : Stream
		{
			return WriteString<T>(stream, string.Format(value, args));
		}

		public static T WriteString<T>(this T stream, string value, Encoding encoding = null, bool writeLength = false) where T : Stream
		{
			if (encoding == null)
			{
				encoding = Encoding.UTF8;
			}

			var bytes = encoding.GetBytes(value);

			if (writeLength)
			{
				WriteInt32(stream, bytes.Length);
			}

			return WriteBytes<T>(stream,
				bytes
			);
		}
	}
}