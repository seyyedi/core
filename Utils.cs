using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Seyyedi
{
	public class Utils
	{
		internal static string[] _dimensions = { "Bytes", "KB", "MB", "GB", "TB", "PB", "EB" };

		public static string FormatStorageBytes(string filename)
			=> FormatStorageBytes(
				new FileInfo(filename).Length
			);

		public static string FormatStorageBytes(long bytes, string format = "0.##")
		{
			double currentSize = bytes;
			int currentDimension = 0;

			while (currentSize > 1024)
			{
				if (currentDimension == _dimensions.Length - 1)
				{
					break;
				}

				currentSize /= 1024.0;
				currentDimension++;
			}

			return string.Format("{0} {1}", currentSize.ToString(format), _dimensions[currentDimension]);
		}

		public static string BytesToHex(byte[] data)
		{
			var sb = new StringBuilder(data.Length * 2);

			for (int i = 0; i < data.Length; i++)
			{
				sb.Append(
					data[i].ToString("X2")
				);
			}

			return sb.ToString();
		}

		public static byte[] HexToBytes(string text)
		{
			var data = new byte[text.Length / 2];

			for (int i = 0; i < data.Length; i++)
			{
				data[i] = byte.Parse(
					text.Substring(i * 2, 2),
					NumberStyles.HexNumber,
					CultureInfo.InvariantCulture
				);
			}

			return data;
		}

		public static bool CompareBytes(byte[] a, byte[] b)
		{
			if (a.Length != b.Length)
			{
				return false;
			}

			for (int i = 0; i < a.Length; i++)
			{
				if (a[i] != b[i])
				{
					return false;
				}
			}

			return true;
		}

		public static string IncludeLeadingZero(int value)
		{
			string strValue = value.ToString();

			if (value < 10)
			{
				strValue = "0" + strValue;
			}

			return strValue;
		}

		public static string ComputeHash(string filename)
		{
			using (var stream = File.OpenRead(filename))
			{
				return ComputeHash(stream);
			}
		}

		public static string ComputeHash(Stream stream)
		{
			using (var sha1 = new SHA1Managed())
			{
				var hash = sha1.ComputeHash(stream);

				return BytesToHex(hash);
			}
		}

		static DateTime UnixUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		public static DateTime ConvertFromUtc(long utc)
			=> UnixUtc.AddSeconds(utc);

		public static long ConvertToUtc(DateTime date)
			=> (long)date.Subtract(UnixUtc).TotalSeconds;

		public static byte[] Compress(byte[] data)
			=> Compress(gzip =>
				gzip.WriteBytes(data)
			);

		public static Task<byte[]> Compress(Stream stream)
			=> Compress(gzip =>
				stream.CopyToAsync(gzip)
			);

		public static byte[] Compress(Action<GZipStream> compress)
		{
			using (var outputStream = new MemoryStream())
			{
				using (var compressStream = new GZipStream(outputStream, CompressionLevel.Optimal, true))
				{
					compress(compressStream);
				}

				return outputStream.ToArray();
			}
		}

		public static async Task<byte[]> Compress(Func<GZipStream, Task> compress)
		{
			using (var outputStream = new MemoryStream())
			{
				using (var compressStream = new GZipStream(outputStream, CompressionLevel.Optimal, true))
				{
					await compress(compressStream);
				}

				return outputStream.ToArray();
			}
		}

	}
}
