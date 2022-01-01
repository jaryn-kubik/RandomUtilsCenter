﻿using System;
using System.IO;
using System.Text.Json;

namespace Asdf
{
	public sealed class JsonHelper<T> where T : new()
	{
		public JsonHelper(string path, T instance)
		{
			FilePath = path;
			Instance = instance;
		}

		public string FilePath { get; }
		public T Instance { get; }

		public static JsonHelper<T> Load(string fileName)
		{
			var dir = Path.GetDirectoryName(typeof(Startup).Assembly.Location);
			var path = Path.Combine(dir, $".{fileName}.json");
			try
			{
				var data = File.ReadAllBytes(path);
				var instance = JsonSerializer.Deserialize<T>(data);
				return new JsonHelper<T>(path, instance);
			}
			catch
			{
				var instance = new T();
				var result = new JsonHelper<T>(path, instance);
				result.Save();
				return result;
			}
		}

		public static T Load(string fileName, Action<JsonHelper<T>> callback)
		{
			var json = Load(fileName);
			callback(json);
			return json.Instance;
		}

		public void Save()
		{
			var data = JsonSerializer.SerializeToUtf8Bytes(Instance, new JsonSerializerOptions { WriteIndented = true });
			File.WriteAllBytes(FilePath, data);
		}
	}
}