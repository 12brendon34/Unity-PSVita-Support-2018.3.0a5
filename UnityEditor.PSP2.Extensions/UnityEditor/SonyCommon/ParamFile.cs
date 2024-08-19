using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace UnityEditor.SonyCommon
{
	public class ParamFile
	{
		private Dictionary<string, string> sfxParams = new Dictionary<string, string>();

		public void Clear()
		{
			sfxParams = new Dictionary<string, string>();
		}

		public void Dump()
		{
			Console.WriteLine("### sfxParams...");
			foreach (KeyValuePair<string, string> sfxParam in sfxParams)
			{
				Console.WriteLine("    sfxParams[" + sfxParam.Key + "] = " + sfxParam.Value);
			}
		}

		public void Write(string XmlFileName)
		{
			Console.WriteLine("### Writing package params: " + XmlFileName);
			XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
			xmlWriterSettings.Indent = true;
			xmlWriterSettings.IndentChars = "\t";
			using (XmlWriter xmlWriter = XmlWriter.Create(XmlFileName, xmlWriterSettings))
			{
				xmlWriter.WriteStartDocument();
				xmlWriter.WriteComment("Generated by Unity editor");
				xmlWriter.WriteComment("See Param_File_Editor-Users_Guide_e.pdf");
				xmlWriter.WriteStartElement("paramsfo");
				foreach (KeyValuePair<string, string> sfxParam in sfxParams)
				{
					xmlWriter.WriteStartElement("param");
					xmlWriter.WriteAttributeString("key", sfxParam.Key);
					xmlWriter.WriteValue(sfxParam.Value);
					xmlWriter.WriteEndElement();
				}
				xmlWriter.WriteEndElement();
				xmlWriter.WriteEndDocument();
			}
		}

		public void Read(string XmlFileName)
		{
			Clear();
			using (XmlReader xmlReader = XmlReader.Create(XmlFileName))
			{
				while (xmlReader.Read())
				{
					XmlNodeType nodeType = xmlReader.NodeType;
					if (nodeType == XmlNodeType.Element && xmlReader.Name == "param")
					{
						string attribute = xmlReader.GetAttribute(0);
						xmlReader.Read();
						string value = xmlReader.Value;
						xmlReader.Read();
						Set(attribute, value);
					}
				}
			}
		}

		public void Remove(string key)
		{
			if (sfxParams.ContainsKey(key))
			{
				sfxParams.Remove(key);
			}
		}

		public void Set(string key, string value)
		{
			sfxParams[key] = value;
		}

		public void SetInt(string key, int value)
		{
			sfxParams[key] = value.ToString();
		}

		public int GetInt(string key, int defaultValue)
		{
			string text = Get(key, defaultValue.ToString());
			int result = 0;
			try
			{
				result = Convert.ToInt32(text);
			}
			catch
			{
				Console.WriteLine("Invalid number '" + text + "' read from param file for key " + key);
			}
			return result;
		}

		public string Get(string key, string defaultValue)
		{
			try
			{
				string text = sfxParams[key];
				if (text.Length > 0)
				{
					return text;
				}
				return defaultValue;
			}
			catch (KeyNotFoundException)
			{
				return defaultValue;
			}
		}

		public string GetWithWarning(string key, string defaultValue, string warning)
		{
			try
			{
				string text = sfxParams[key];
				if (text.Length > 0)
				{
					return text;
				}
				Debug.LogWarning(warning);
				return defaultValue;
			}
			catch (KeyNotFoundException)
			{
				Debug.LogWarning(warning);
				return defaultValue;
			}
		}

		public string GetWithWarningAndSetDefault(string key, string defaultValue, string warning, bool enableWarning)
		{
			try
			{
				string text = sfxParams[key];
				if (text.Length > 0)
				{
					return text;
				}
				if (enableWarning)
				{
					Debug.LogWarning(warning);
				}
				sfxParams[key] = defaultValue;
				return defaultValue;
			}
			catch (KeyNotFoundException)
			{
				if (enableWarning)
				{
					Debug.LogWarning(warning);
				}
				sfxParams[key] = defaultValue;
				return defaultValue;
			}
		}

		public string GetWithError(string key, string defaultValue, string error)
		{
			try
			{
				string text = sfxParams[key];
				if (text.Length > 0)
				{
					return text;
				}
				Debug.LogError(error);
				return defaultValue;
			}
			catch (KeyNotFoundException)
			{
				Debug.LogError(error);
				return defaultValue;
			}
		}
	}
}
