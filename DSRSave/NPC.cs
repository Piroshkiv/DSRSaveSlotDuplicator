using System.Text.Json;
using System.Text.Json.Serialization;

namespace DSRSave
{
    public class NpcCollection
    {
        [JsonPropertyName("npcs")]
        public List<Npc> Npcs { get; set; }
    }

    public class Npc
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("positions")]
        public List<NpcPosition> Positions { get; set; }
    }

    public class NpcPosition
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("bits")]
        public List<NpcBitEntry> Bits { get; set; }
    }

    public class NpcBitEntry
    {
        [JsonPropertyName("offset")]
        public string OffsetHex { get; set; }

        [JsonPropertyName("bit")]
        public int BitPosition { get; set; }

        [JsonPropertyName("reverse")]
        public bool Reverse { get; set; } = false;

        [JsonIgnore]
        public int Offset => Convert.ToInt32(OffsetHex, 16);
    }

    public class NPCEditor
    {
        private const string FilePath = "json/npc_data.json";
        private readonly Character _character;
        private readonly NpcCollection _npcData;

        public NPCEditor(Character character)
        {
            _character = character;
            string json = File.ReadAllText(FilePath);
            _npcData = JsonSerializer.Deserialize<NpcCollection>(json)
                       ?? throw new Exception($"Failed to parse {FilePath}.");
        }

        public NpcCollection NPCData { get => _npcData; }

        public void SetNpcAlive(string name, string positionName, bool alive)
        {
            var npc = _npcData.Npcs.FirstOrDefault(n => n.Name == name);
            if (npc == null)
                throw new Exception($"NPC with name '{name}' not found.");

            var position = npc.Positions.FirstOrDefault(p => p.Name == positionName);
            if (position == null)
                throw new Exception($"Position '{positionName}' not found for NPC '{name}'.");

            var patternOffsets = _character.FindPattern1().ToList();
            if (patternOffsets.Count == 0)
                throw new Exception("Pattern not found in character data.");

            int baseOffset = patternOffsets.Last();

            foreach (var bitEntry in position.Bits)
            {
                int absoluteOffset = baseOffset + bitEntry.Offset;
                if (absoluteOffset < 0 || absoluteOffset >= _character.GetRawData().Length)
                    throw new Exception($"Calculated offset {absoluteOffset} is out of bounds.");

                bool bitValue = bitEntry.Reverse ? !alive : alive;

                _character.SetBit(absoluteOffset, bitEntry.BitPosition, bitValue);
            }
        }

    }
}
