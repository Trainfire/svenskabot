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

        public DiscordEmbedBuilder AddDataToEmbedBuilder(DiscordEmbedBuilder discordEmbedBuilder)
        {
            int maxDefinitions = Resources.ConstantData.Ord.MaxDefinitions;

            var infoStringBuilder = new StringBuilderEx()
                .AddField("Grundform", Grundform)
                .AddField("Ordklass", Ordklass)
                .AddField("Böjningar", Böjningar);

            discordEmbedBuilder.AddField(":memo:", infoStringBuilder.ToString());

            int definitionCount = Math.Min(maxDefinitions, Definitioner.Count);

            for (int i = 0; i < definitionCount; i++)
            {
                var definitionEntry = Definitioner[i];

                var definitionStringBuilder = new StringBuilderEx();

                // Definition
                if (!string.IsNullOrEmpty(definitionEntry.Definition))
                {
                    definitionStringBuilder
                        .AddField("Definition", definitionEntry.Definition)
                        .AppendWithCondition($" ({ definitionEntry.DefinitionT })", !string.IsNullOrEmpty(definitionEntry.DefinitionT));
                }

                // Konstruction
                definitionStringBuilder.AddField("Konstruktion", definitionEntry.Konstruktion);

                // Exempel
                var exempel = definitionEntry.Exempel.Take(Resources.ConstantData.Ord.MaxExamplesPerDefinition).ToList();
                definitionStringBuilder.AddField("Exempel", exempel);

                // Add to embed
                discordEmbedBuilder.AddField($"{ (i + 1).ToString() }", definitionStringBuilder.ToString());
            }

            if (Definitioner.Count > maxDefinitions)
            {
                var additionalDefinitions = Definitioner.Count - maxDefinitions;
                discordEmbedBuilder.AddField("Obs!", $"Det finns { additionalDefinitions.ToString().ToBold() } definitioner till som kan hittas online.");
            }

            discordEmbedBuilder.AddFieldSafe("Källa", SourceUrl);

            return discordEmbedBuilder;
        }

        public DiscordEmbedBuilder AsEmbedBuilder()
        {
            var outBuilder = new DiscordEmbedBuilder();
            AddDataToEmbedBuilder(outBuilder);
            return outBuilder;
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
