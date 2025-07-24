using DSRSave;
using System.IO;

namespace DSRSaveSlotDuplicator
{
    public partial class Form1 : Form
    {
        private string _fromSave;
        private string _toSave;
        public Form1()
        {
            InitializeComponent();
        }


        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                _fromSave = ofd.FileName;
                fillDataGridView(ofd.FileName, dataGridView1);
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            var from = dataGridView1.CurrentRow?.Index ?? -1;
            var to = dataGridView2.CurrentRow?.Index ?? -1;

            if (from >= 10 || to>= 10)
            {
                return;
            }

            if (dataGridView1.CurrentRow?.Cells[1].Value?.ToString() == "EMPTY")
            {
                MessageBox.Show("Slot empty!");
                return;
            }

            if (dataGridView2.CurrentRow?.Cells[1].Value?.ToString() != "EMPTY")
            {
                if(MessageBox.Show(
                    "Slot isn't empty!\nThe character on the right will be deleted forever and the character on the left will be in his place.", 
                    "Overwrite the character on the right?", 
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }
            }

            var charactersFrom = loadSave(_fromSave);
            var charactersTo = loadSave(_toSave);

            charactersTo[to] = new Character(charactersFrom[from].GetRawData(), to);

            var characterTo = charactersTo[10];
            var characterFrom = charactersFrom[10];

            //This is what you see when you click load game (name, lvl...). It loads automatically, but only after you enter the save.
            var patterTo = 0xC0 + 400 * to;
            var patterFrom = 0xC0 + 400 * from;

            for (int i = 0; i < 400 && (patterTo + i) < characterTo.GetRawData().Length; i++)
            {
                characterTo[patterTo + i] = characterFrom[patterFrom + i];
            }

            //This flag shows the save slot empty or not. 0 - empty 1 - not
            characterTo[0xC4 + to] = 1;

            //charactersTo[to].General.Name1 = $"Number {to}";
            //charactersTo[to].General.Name2 = $"Number {to}";

            charactersTo.WriteSave(_toSave);

            fillDataGridView(_fromSave, dataGridView1);
            fillDataGridView(_toSave, dataGridView2);

            dataGridView1.ClearSelection();
            dataGridView1.Rows[from].Selected = true;
            dataGridView1.CurrentCell = dataGridView1.Rows[from].Cells[0];

            dataGridView2.ClearSelection();
            dataGridView2.Rows[to].Selected = true;
            dataGridView2.CurrentCell = dataGridView2.Rows[to].Cells[0];

        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                _toSave = ofd.FileName;
                fillDataGridView(ofd.FileName, dataGridView2);
            }
        }
        
        private IList<Character> loadSave(string path) =>
            DSRSaveEditor.ReadSave(path).ToList();

        private void fillDataGridView(string path, DataGridView grid)
        {
            var characters = loadSave(path);

            grid.Columns.Clear();
            grid.Rows.Clear();

            var colSlot = new DataGridViewTextBoxColumn
            {
                Name = "Slot",
                HeaderText = "Slot",
                Width = 50
            };
            var colName = new DataGridViewTextBoxColumn
            {
                Name = "Name",
                HeaderText = "Name",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            };

            grid.Columns.AddRange(colSlot, colName);

            foreach (var c in characters.SkipLast(1))
            {
                string displayName = c.IsEmpty ? "EMPTY" : c.General.Name1;
                int rowIndex = grid.Rows.Add(c.SlotNumber.ToString(), displayName);

                if (c.IsEmpty)
                {
                    var row = grid.Rows[rowIndex];
                    row.Cells[1].Style.ForeColor = Color.Gray;
                    row.ReadOnly = true;
                }
            }
        }
    }
}
