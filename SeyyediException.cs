using System;

namespace Seyyedi
{
	public class SeyyediException : Exception
	{
		public SeyyediException()
			: this(null)
		{

		}

		public SeyyediException(string format, params string[] args)
			: this(null, format, args)
		{

		}

		public SeyyediException(Exception inner, string format, params string[] args)
			: base(format == null ? null : string.Format(format, args), inner)
		{

		}
	}
}
