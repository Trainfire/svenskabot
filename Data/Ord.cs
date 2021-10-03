using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace svenskabot
{
    class OrdEntryBuilder
    {
        public string Grundform { get; set; }
        public string Ordklass { get; set; }
        public string Böjningar { get; set; }
        public List<OrdDefinition> Definitioner = new List<OrdDefinition>();
        public string SourceUrl { get; set; }

        public OrdEntry AsNew()
        {
            return new OrdEntry(Grundform, Ordklass, Definitioner, Böjningar, SourceUrl);
        }
    }

    /// <summary>
    /// Generic classes since we might want to use them for other dictionary searches.
    /// </summary>
    public class OrdEntry
    {
        public string Grundform { get; private set; }
        public string Ordklass { get; private set; }
        public string Böjningar { get; private set; }
        public string SourceUrl { get; private set; }
        public IReadOnlyList<OrdDefinition> Definitioner { get; private set; }

        public OrdEntry(string grundform, string ordklass, List<OrdDefinition> definitioner, string böjningar, string sourceUrl)
        {
            Grundform = grundform ?? string.Empty;
            Ordklass = ordklass ?? string.Empty;
            Böjningar = böjningar ?? string.Empty;
            Definitioner = definitioner ?? new List<OrdDefinition>();
            SourceUrl = sourceUrl ?? string.Empty;
        }

        public void AddToBuilder(DiscordEmbedBuilder discordEmbedBuilder)
        {
            int maxDefinitions = Resources.ConstantData.Ord.MaxDefinitions;

            discordEmbedBuilder.AddFieldSafe("Grundform", Grundform);
            discordEmbedBuilder.AddFieldSafe("Ordklass", Ordklass);
            discordEmbedBuilder.AddFieldSafe("Böjningar", Böjningar);

            int definitionCount = Math.Min(maxDefinitions, Definitioner.Count);

            for (int i = 0; i < definitionCount; i++)
            {
                var definitionEntry = Definitioner[i];

                string definitionString = string.Empty;

                definitionString += AddHeader("Definition");
                definitionString += definitionEntry.Definition;

                if (!string.IsNullOrEmpty(definitionEntry.DefinitionT))
                {
                    definitionString += $" ({ definitionEntry.DefinitionT })";
                }

                definitionString += AddLineBreak();

                if (definitionEntry.Konstruktion.Count() != 0)
                {
                    definitionString += AddHeader("Konstruktion");
                    definitionString += AddList(definitionEntry.Konstruktion.ToArray());
                    definitionString += AddLineBreak();
                }

                var examples = definitionEntry.Exempel
                    .Take(Resources.ConstantData.Ord.MaxExamplesPerDefinition)
                    .ToList();

                if (examples.Count() != 0)
                {
                    definitionString += AddHeader("Exempel");
                    definitionString += AddList(examples.ToArray());
                }

                discordEmbedBuilder.AddField($"{ (i + 1).ToString() }", definitionString);
            }

            if (Definitioner.Count > maxDefinitions)
            {
                var additionalDefinitions = Definitioner.Count - maxDefinitions;
                discordEmbedBuilder.AddField("Obs!", $"Det finns { additionalDefinitions.ToString().ToBold() } definitioner till som kan hittas online.");
            }

            discordEmbedBuilder.AddFieldSafe("Källa", SourceUrl);
        }

        public DiscordEmbedBuilder AsEmbedBuilder()
        {
            var outBuilder = new DiscordEmbedBuilder();
            AddToBuilder(outBuilder);
            return outBuilder;
        }

        string AddHeader(string header)
        {
            return $"{ header }: ".ToBold();
        }

        string AddList(string[] strings)
        {
            return string.Join("; ", strings);
        }

        string AddLineBreak()
        {
            return "\n";
        }
    }

    class OrdDefinitionBuilder
    {
        public OrdDefinition AsNew()
        {
            return new OrdDefinition(Definition, DefinitionT, Exempel, Konstruktion);
        }

        public string Definition { get; set; }
        public string DefinitionT { get; set; }
        public List<string> Exempel { get; set; } = new List<string>();
        public List<string> Konstruktion { get; set; } = new List<string>();
    }

    public class OrdDefinition
    {
        public string Definition { get; private set; }
        public string DefinitionT { get; private set; }
        public IReadOnlyList<string> Exempel { get { return _exempel; } }
        public List<string> Konstruktion { get; private set; }

        private List<string> _exempel { get; set; } = new List<string>();

        public OrdDefinition(string definition, string definitionT, List<string> exempel, List<string> konstruktion)
        {
            Definition = definition ?? string.Empty;
            DefinitionT = definitionT ?? string.Empty;
            _exempel = exempel ?? new List<string>();
            Konstruktion = konstruktion ?? new List<string>();
        }

        public bool IsValid()
        {
            return Definition != string.Empty || DefinitionT != string.Empty;
        }
    }
}
