using System.Text;

namespace Seyyedi
{
	public static class StringExtensions
	{
		public static string ToCamelCase(this string value)
		{
			var sb = new StringBuilder(value.Length);
			var i = 0;

			while (char.IsUpper(value, i))
			{
				sb.Append(char.ToLower(value[i]));
				i++;

				if (i == value.Length)
				{
					break;
				}
			}

			if (i < value.Length)
			{
				sb.Append(value.Substring(i));
			}

			return sb.ToString();
		}

		public static string ToLowerCase(this string text, char filler, bool noFillerAtStart = true)
		{
			var sb = new StringBuilder(text.Length * 2);

			for (int i = 0; i < text.Length; i++)
			{
				var c = text[i];

				if (char.IsUpper(c))
				{
					if (!noFillerAtStart || i > 0)
					{
						sb.Append(filler);
					}

					sb.Append(char.ToLower(c));
				}
				else
				{
					sb.Append(c);
				}
			}

			return sb.ToString();
		}
	}
}