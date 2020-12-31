using System.Xml.Serialization;
using System.Collections.Generic;
using System;
using svenskabot;
using System.IO;
using System.Net;

namespace FolketsOrdbokResource
{
    [XmlRoot(ElementName = "dictionary")]
	public class FolketsOrdbokSource
	{
		[XmlElement(ElementName = "word")]
		public List<Word> Words { get; set; }

		[XmlAttribute(AttributeName = "comment")]
		public string Comment { get; set; }

		[XmlAttribute(AttributeName = "created")]
		public string Created { get; set; }

		[XmlAttribute(AttributeName = "last-changed")]
		public string Lastchanged { get; set; }

		[XmlAttribute(AttributeName = "name")]
		public string Name { get; set; }

		[XmlAttribute(AttributeName = "source-language")]
		public string Sourcelanguage { get; set; }

		[XmlAttribute(AttributeName = "target-language")]
		public string Targetlanguage { get; set; }

		[XmlAttribute(AttributeName = "version")]
		public string Version { get; set; }

		[XmlAttribute(AttributeName = "license")]
		public string License { get; set; }

		[XmlAttribute(AttributeName = "licenseComment")]
		public string LicenseComment { get; set; }

		[XmlAttribute(AttributeName = "originURL")]
		public string OriginURL { get; set; }
	}

	[XmlRoot(ElementName = "translation")]
	public class Translation
	{
		[XmlAttribute(AttributeName = "comment")]
		public string Comment { get; set; }

		[XmlAttribute(AttributeName = "value")]
		public string Value { get; set; }
	}

	[XmlRoot(ElementName = "phonetic")]
	public class Phonetic
	{
		[XmlAttribute(AttributeName = "soundFile")]
		public string SoundFile { get; set; }

		[XmlAttribute(AttributeName = "value")]
		public string Value { get; set; }
	}

	[XmlRoot(ElementName = "see")]
	public class See
	{
		[XmlAttribute(AttributeName = "type")]
		public string Type { get; set; }

		[XmlAttribute(AttributeName = "value")]
		public string Value { get; set; }
	}

	[XmlRoot(ElementName = "example")]
	public class Example
	{
		[XmlElement(ElementName = "translation")]
		public Translation Translation { get; set; }

		[XmlAttribute(AttributeName = "value")]
		public string Value { get; set; }
	}

	[XmlRoot(ElementName = "definition")]
	public class Definition
	{
		[XmlElement(ElementName = "translation")]
		public Translation Translation { get; set; }

		[XmlAttribute(AttributeName = "value")]
		public string Value { get; set; }
	}

	[XmlRoot(ElementName = "word")]
	public class Word
	{
		[XmlElement(ElementName = "translation")]
		public Translation Translation { get; set; }

		[XmlElement(ElementName = "phonetic")]
		public Phonetic Phonetic { get; set; }

		[XmlElement(ElementName = "see")]
		public See See { get; set; }

		[XmlElement(ElementName = "example")]
		public Example Example { get; set; }

		[XmlElement(ElementName = "definition")]
		public Definition Definition { get; set; }

		[XmlAttribute(AttributeName = "class")]
		public string Class { get; set; }

		[XmlAttribute(AttributeName = "comment")]
		public string Comment { get; set; }

		[XmlAttribute(AttributeName = "lang")]
		public string Lang { get; set; }

		[XmlAttribute(AttributeName = "value")]
		public string Value { get; set; }
	}

	public class FolketsOrdbok
    {
		public List<OrdEntry> Words { get; private set; } = new List<OrdEntry>();

		private static string FolketsOrdbokFileName { get; } = "folkets";

		public FolketsOrdbok()
        {
			var path = AppDomain.CurrentDomain.BaseDirectory + FolketsOrdbokFileName;

			Console.WriteLine("Downloading lexicon from Folkets Ordbok...");

			if (File.Exists(path))
			{
				Console.WriteLine("Lexicon from Folkets Ordbok already exists. Skipping download...");
			}
			else
			{
				using (var client = new WebClient())
				{
					// TODO: Check if URL is valid...
					client.DownloadFile(Resources.Config.Sources.FolketsOrdbokLexicon, FolketsOrdbokFileName);
				}

				Console.WriteLine("Finished downloading lexicon from Folkets Ordbok.");
			}

			FolketsOrdbokSource folketsOrdbokSource = null;

			if (File.Exists(path))
			{
				using (var file = File.OpenText(path))
				{
					var serializer = new XmlSerializer(typeof(FolketsOrdbokSource));

					folketsOrdbokSource = (FolketsOrdbokSource)(serializer.Deserialize(file));
				}
			}
			else
            {
				Console.WriteLine("Failed to load Folkets Ordbok.");
            }

			if (folketsOrdbokSource == null)
				return;

			foreach (var word in folketsOrdbokSource.Words)
            {
				var definitionBuilder = new OrdDefinitionBuilder();

				if (word.Definition != null)
					definitionBuilder.Definition = word.Definition.Value;

				if (word.Example != null)
					definitionBuilder.Exempel.Add(word.Example.Value);

				var ordBuilder = new OrdEntryBuilder();
				ordBuilder.Definitioner.Add(definitionBuilder.AsNew());
				ordBuilder.Grundform = word.Value;
				ordBuilder.Ordklass = word.Class;

				Words.Add(ordBuilder.AsNew());
            }
        }
    }
}