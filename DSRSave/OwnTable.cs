
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DSRSave
{
    public class TableCollection
    {
        [JsonPropertyName("table")]
        public List<Table> Tables { get; set; }
    }

    public class Table
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("bits")]
        public List<TableBitEntry> Bits { get; set; }
    }

    public class TableBitEntry
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("bit")]
        public int BitPosition { get; set; }

        [JsonPropertyName("reverse")]
        public bool Reverse { get; set; } = false;
    }

    public enum BitType
    {
        Absolute,
        Pattern1
    }

    public class OwnTable
    {
        private const string FilePath = "json/table_data.json";
        private readonly Character _character;

        public TableCollection TableData { get; private set; }

        public OwnTable(Character character)
        {
            _character = character;
            TableData = LoadOrCreateTableCollection();
        }

        private TableCollection LoadOrCreateTableCollection()
        {
            if (!File.Exists(FilePath))
            {
                var emptyCollection = new TableCollection { Tables = new List<Table>() };
                SaveTableCollection(emptyCollection);
                return emptyCollection;
            }

            string json = File.ReadAllText(FilePath);
            var tableData = JsonSerializer.Deserialize<TableCollection>(json);

            return tableData ?? new TableCollection { Tables = new List<Table>() };
        }

        public void Save()
        {
            SaveTableCollection(TableData);
        }

        private void SaveTableCollection(TableCollection collection)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(collection, options);
            File.WriteAllText(FilePath, json);
        }
    }
}
