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

        public OrdEntry AsNew()
        {
            return new OrdEntry(Grundform, Ordklass, Definitioner, Böjningar);
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
        public IReadOnlyList<OrdDefinition> Definitioner { get { return _definitioner; } }

        private List<OrdDefinition> _definitioner { get; set; }

        public OrdEntry(string grundform, string ordklass, List<OrdDefinition> definitioner, string böjningar)
        {
            Grundform = grundform ?? string.Empty;
            Ordklass = ordklass ?? string.Empty;
            Böjningar = böjningar ?? string.Empty;
            _definitioner = definitioner ?? new List<OrdDefinition>();
        }

        public DiscordEmbedBuilder AsEmbed()
        {
            int maxDefinitions = Resources.ConstantData.Ord.MaxDefinitions;

            var outBuilder = new DiscordEmbedBuilder();

            if (!string.IsNullOrEmpty(Grundform))
                outBuilder.AddField("Grundform", Grundform);

            if (!string.IsNullOrEmpty(Ordklass))
                outBuilder.AddField("Ordklass", Ordklass);

            if (!string.IsNullOrEmpty(Böjningar))
                outBuilder.AddField("Böjningar", Böjningar);

            int definitionCount = Math.Min(maxDefinitions, Definitioner.Count);

            for (int i = 0; i < definitionCount; i++)
            {
                var definitionEntry = Definitioner[i];

                var definitionString = definitionEntry.Definition;
                if (!string.IsNullOrEmpty(definitionEntry.DefinitionT))
                    definitionString += $" ({ definitionEntry.DefinitionT })";

                var examples = definitionEntry.Exempel
                    .Take(Resources.ConstantData.Ord.MaxExamplesPerDefinition)
                    .ToList();

                // Make one string with line breaks since it takes less room than a header for each example.
                if (examples.Count() != 0)
                    examples.ForEach(example => definitionString += "\n- " + example.ToItalics());

                outBuilder.AddField($"Definition { (i + 1).ToString() }", definitionString);
            }

            if (Definitioner.Count > maxDefinitions)
            {
                var additionalDefinitions = Definitioner.Count - maxDefinitions;
                outBuilder.AddField("Obs!", $"Det finns { additionalDefinitions.ToString().ToBold() } definitioner till som kan hittas online.");
            }

            return outBuilder;
        }
    }

    class OrdDefinitionBuilder
    {
        public OrdDefinition AsNew()
        {
            return new OrdDefinition(Definition, DefinitionT, Exempel);
        }

        public string Definition { get; set; }
        public string DefinitionT { get; set; }
        public List<string> Exempel { get; set; } = new List<string>();
    }

    public class OrdDefinition
    {
        public string Definition { get; private set; }
        public string DefinitionT { get; private set; }
        public IReadOnlyList<string> Exempel { get { return _exempel; } }

        private List<string> _exempel { get; set; } = new List<string>();

        public OrdDefinition(string definition, string definitionT, List<string> exempel)
        {
            Definition = definition ?? string.Empty;
            DefinitionT = definitionT ?? string.Empty;
            _exempel = exempel ?? new List<string>();
        }

        public bool IsValid()
        {
            return Definition != string.Empty || DefinitionT != string.Empty;
        }
    }
}
