namespace SalusExampleParent
{
    public partial class MainForm : Form
    {
        private readonly ExampleDbContext _context;

        public MainForm(ExampleDbContext context)
        {
            _context = context;
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            RefreshData();
        }

        private void Grid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var senderGrid = (DataGridView)sender;

            var deleteIndex = senderGrid.Columns["Delete"].Index;
            var editIndex = senderGrid.Columns["Edit"].Index;

            if (senderGrid.Columns[e.ColumnIndex] is DataGridViewButtonColumn && e.RowIndex >= 0)
            {
                if (e.ColumnIndex == deleteIndex)
                {
                    DeleteRow(e.RowIndex, (int)senderGrid.Rows[e.RowIndex].Cells["Id"].Value);
                }
                else if (e.ColumnIndex == editIndex)
                {
                    EditRow(e.RowIndex, (int)senderGrid.Rows[e.RowIndex].Cells["Id"].Value);
                }
            }

        }

        private void DeleteRow(int rowIndex, int id)
        {
            var row = _context.ExampleData.SingleOrDefault(r => r.Id == id);
            if (row != null)
            {
                Grid.Rows.RemoveAt(rowIndex);
                _context.ExampleData.Remove(row);
                _context.SaveChanges();
            }
        }

        private void EditRow(int rowIndex, int id)
        {
            var editForm = new AddEditForm();
            editForm.Data1 = Grid.Rows[rowIndex].Cells["Data1"].Value.ToString() ?? "";
            editForm.Data2 = Grid.Rows[rowIndex].Cells["Data2"].Value.ToString() ?? "";
            editForm.ShowDialog();

            if (editForm.Save)
            {
                var row = _context.ExampleData.SingleOrDefault(r => r.Id == id);
                if (row != null)
                {
                    Grid.Rows[rowIndex].Cells["Data1"].Value = editForm.Data1;
                    Grid.Rows[rowIndex].Cells["Data2"].Value = editForm.Data2;

                    row.Data1 = editForm.Data1;
                    row.Data2 = editForm.Data2;
                    _context.SaveChanges();
                }
            }
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            var addForm = new AddEditForm();
            addForm.ShowDialog();

            if (addForm.Save)
            {
                var row = new ExampleData
                {
                    Data1 = addForm.Data1,
                    Data2 = addForm.Data2
                };

                _context.Add(row);
                _context.SaveChanges();

                Grid.Rows.Add(row.Id, row.Data1, row.Data2, "Edit", "Delete");
            }
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            RefreshData();
        }

        private void RefreshData()
        {
            var data = _context.ExampleData.ToList();

            Grid.Rows.Clear();
            foreach (var row in data)
            {
                Grid.Rows.Add(row.Id, row.Data1, row.Data2, "Edit", "Delete");
            }
        }


    }
}
