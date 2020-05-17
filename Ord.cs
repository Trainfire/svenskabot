using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace svenskabot
{
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

    class DiscordEmbedBuilderFromOrdEntry
    {
        public DiscordEmbedBuilder EmbedBuilder { get; private set; } = new DiscordEmbedBuilder();

        public DiscordEmbedBuilderFromOrdEntry(OrdEntry ordEntry, int maxExamples = -1)
        {
            EmbedBuilder.AddField("Grundform", ordEntry.Grundform);

            if (ordEntry.Böjningar != "")
                EmbedBuilder.AddField("Böjningar", ordEntry.Böjningar);

            for (int i = 0; i < ordEntry.Definitioner.Count(); i++)
            {
                var definitionEntry = ordEntry.Definitioner[i];

                var definitionString = definitionEntry.Definition;
                if (definitionEntry.DefinitionT != string.Empty)
                    definitionString += $" ({ definitionEntry.DefinitionT })";

                IEnumerable<string> examples = null;

                if (maxExamples > 0)
                {
                    examples = definitionEntry.Exempel.Take(maxExamples);
                }
                else if (maxExamples == -1)
                {
                    examples = definitionEntry.Exempel;
                }

                // Make one string with line breaks since it takes less room than a header for each example.
                if (examples != null && examples.Count() != 0)
                {
                    var newLine = "\n- ";
                    definitionString += newLine;
                    definitionString += string.Join(newLine, examples);
                }

                EmbedBuilder.AddField($"Definition { (i + 1).ToString() }", definitionString);
            }
        }
    }
}
