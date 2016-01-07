﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;

namespace Seyyedi
{
	public static class DynamicExtensions
	{
		public static dynamic ToDynamic(this object value)
		{
			IDictionary<string, object> expando = new ExpandoObject();

			var properties = TypeDescriptor
				.GetProperties(value.GetType());

			foreach (PropertyDescriptor property in properties)
			{
				expando.Add(property.Name, property.GetValue(value));
			}

			return expando as ExpandoObject;
		}
	}
}