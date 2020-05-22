using DSharpPlus.Entities;
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

        public DiscordEmbedBuilder AsEmbed()
        {
            var outBuilder = new DiscordEmbedBuilder();

            outBuilder.AddField("Grundform", Grundform);

            if (Böjningar != "")
                outBuilder.AddField("Böjningar", Böjningar);

            for (int i = 0; i < Definitioner.Count(); i++)
            {
                var definitionEntry = Definitioner[i];

                var definitionString = definitionEntry.Definition;
                if (definitionEntry.DefinitionT != string.Empty)
                    definitionString += $" ({ definitionEntry.DefinitionT })";

                IEnumerable<string> examples = null;

                const int maxExamples = 5;

                examples = definitionEntry.Exempel.Take(maxExamples);

                // Make one string with line breaks since it takes less room than a header for each example.
                if (examples != null && examples.Count() != 0)
                {
                    var newLine = "\n- ";
                    definitionString += newLine;
                    definitionString += string.Join(newLine, examples);
                }

                outBuilder.AddField($"Definition { (i + 1).ToString() }", definitionString);
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
