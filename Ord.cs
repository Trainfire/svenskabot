using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace svenskabot
{
    /// <summary>
    /// Generic classes since we might want to use them for other dictionary searches.
    /// </summary>
    class OrdEntry
    {
        public string Grundform { get; private set; }
        public string Ordklass { get; private set; }
        public string Böjningar { get; private set; }
        public IReadOnlyList<OrdDefinition> Definitioner { get { return _definitioner; } }

        private List<OrdDefinition> _definitioner { get; set; } = new List<OrdDefinition>();

        public OrdEntry(string grundform, string ordklass, List<OrdDefinition> definitioner, string böjningar)
        {
            Grundform = grundform;
            Ordklass = ordklass;
            Böjningar = böjningar;
            _definitioner = definitioner;
        }

        public DiscordEmbedBuilder AsEmbed(int maxDefinitions = 5, int maxExamplesPerDefinition = 3)
        {
            var outBuilder = new DiscordEmbedBuilder();

            outBuilder.AddField("Grundform", Grundform);

            if (Böjningar != "")
                outBuilder.AddField("Böjningar", Böjningar);

            int definitionCount = Math.Min(maxDefinitions, Definitioner.Count);

            for (int i = 0; i < definitionCount; i++)
            {
                var definitionEntry = Definitioner[i];

                var definitionString = definitionEntry.Definition;
                if (definitionEntry.DefinitionT != string.Empty)
                    definitionString += $" ({ definitionEntry.DefinitionT })";

                var examples = definitionEntry.Exempel?
                    .Take(maxExamplesPerDefinition)
                    .ToList();

                // Make one string with line breaks since it takes less room than a header for each example.
                if (examples != null && examples.Count() != 0)
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

    class OrdDefinition
    {
        public string Definition { get; private set; }
        public string DefinitionT { get; private set; }
        public IReadOnlyList<string> Exempel { get { return _exempel; } }

        private List<string> _exempel { get; set; } = new List<string>();

        public OrdDefinition(string definition, string definitionT, List<string> exempel)
        {
            Definition = definition;
            DefinitionT = definitionT;
            _exempel = exempel;
        }
    }
}
