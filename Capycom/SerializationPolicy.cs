using Serilog.Events;
using Serilog.Core;

namespace Capycom
{
	class SerializationPolicy : IDestructuringPolicy
	{
		public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, out LogEventPropertyValue result)
		{
			var type = value.GetType();
			if (type.Namespace.Contains("Capycom"))
			{
				var properties = type.GetProperties()
					.Where(p => !IsFromCapycomNamespace(p.PropertyType) && !IsBinary(p.PropertyType))
					.ToDictionary(p => p.Name, p => p.GetValue(value));




				foreach (var property in type.GetProperties().Where(p => p.PropertyType == typeof(IFormFile)))
				{
					var formFile = property.GetValue(value) as IFormFile;
					if (formFile != null)
					{
						properties.Add(property.Name, new { formFile.FileName, formFile.ContentType, formFile.Length });
					}
				}

				foreach (var property in type.GetProperties().Where(p => p.PropertyType == typeof(IFormFileCollection)))
				{
					var formFileCollection = property.GetValue(value) as IFormFileCollection;
					if (formFileCollection != null)
					{
						properties.Add(property.Name, formFileCollection.Select(f => new { f.FileName, f.ContentType, f.Length }).ToList());
					}
				}


				foreach (var property in type.GetProperties().Where(p => p.PropertyType == typeof(List<IFormFile>)))
				{
					var formFileList = property.GetValue(value) as List<IFormFile>;
					if (formFileList != null)
					{
						properties.Add(property.Name, formFileList.Select(f => new { f.FileName, f.ContentType, f.Length }).ToList());
					}
				}



				result = propertyValueFactory.CreatePropertyValue(properties, false);
				return true;
			}
			else if (!IsBinary(type)) //условие для исключения бинарных файлов вне пространства имен Capycom
			{
				// Для всех остальных типов объектов использовать стандартную сериализацию
				result = propertyValueFactory.CreatePropertyValue(value, true);
				return true;
			}
			else if (value is IFormFile formFile)
			{
				// Для IFormFile сериализуем только тип данных, имя файла и размер файла
				var fileProperties = new Dictionary<string, object>
				{
					{ "FileName", formFile.FileName },
					{ "ContentType", formFile.ContentType },
					{ "Length", formFile.Length } // Добавлен размер файла
				};

				result = propertyValueFactory.CreatePropertyValue(fileProperties, true);
				return true;
			}

			else if (value is IFormFileCollection formFileCollection)
			{
				// Для IFormFileCollection сериализуем только тип данных, имя файла и размер файла для каждого файла
				var fileCollectionProperties = formFileCollection
					.Select(f => new { f.FileName, f.ContentType, f.Length }) // Добавлен размер файла
					.ToList();

				result = propertyValueFactory.CreatePropertyValue(fileCollectionProperties, true);
				return true;
			}

			else if (value is List<IFormFile> formFileList)
			{
				// Для List<IFormFile> сериализуем только тип данных, имя файла и размер файла для каждого файла
				var fileListProperties = formFileList
					.Select(f => new { f.FileName, f.ContentType, f.Length }) // Добавлен размер файла
					.ToList();

				result = propertyValueFactory.CreatePropertyValue(fileListProperties, true);
				return true;
			}

			// Для всех остальных типов объектов запретить сериализацию
			result = null;
			return false;
		}
		private bool IsFromCapycomNamespace(Type type)
		{
			while (type != null)
			{
				if (type.Namespace != null && type.Namespace.Contains("Capycom"))
				{
					return true;
				}

				if (type.IsGenericType)
				{
					foreach (var argument in type.GetGenericArguments())
					{
						if (IsFromCapycomNamespace(argument))
						{
							return true;
						}
					}
				}

				type = type.BaseType;
			}

			return false;
		}
		private bool IsBinary(Type type)
		{
			return type == typeof(byte[]) || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)
				&& type.GetGenericArguments()[0] == typeof(byte));
		}
	}
}
