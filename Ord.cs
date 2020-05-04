using DSharpPlus.Entities;
using System.Collections.Generic;
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
        public IReadOnlyList<string> Exemplar { get { return _exemplar; } }

        private List<string> _exemplar { get; set; } = new List<string>();

        public OrdDefinition(string definition, List<string> exemplar)
        {
            Definition = definition;
            _exemplar = exemplar;
        }
    }

    class DiscordEmbedBuilderFromOrdEntry
    {
        public DiscordEmbedBuilder EmbedBuilder { get; private set; } = new DiscordEmbedBuilder();

        public DiscordEmbedBuilderFromOrdEntry(OrdEntry ordEntry)
        {
            EmbedBuilder.AddField("Grundform", ordEntry.Grundform);

            if (ordEntry.Böjningar != "")
                EmbedBuilder.AddField("Böjningar", ordEntry.Böjningar);

            for (int i = 0; i < ordEntry.Definitioner.Count(); i++)
            {
                var definitionEntry = ordEntry.Definitioner[i];

                EmbedBuilder.AddField($"Definition { (i + 1).ToString() }", definitionEntry.Definition);
                EmbedBuilder.AddField("Exemplar", string.Join("\n", definitionEntry.Exemplar));
            }
        }
    }
}
