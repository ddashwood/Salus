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

            if (senderGrid.Columns[e.ColumnIndex] is DataGridViewButtonColumn && e.RowIndex >= 0 && e.ColumnIndex == deleteIndex)
            {
                var id = (int)senderGrid.Rows[e.RowIndex].Cells["Id"].Value;

                var row = _context.ExampleData.SingleOrDefault(r => r.Id == id);
                if (row != null)
                {
                    Grid.Rows.RemoveAt(e.RowIndex);
                    _context.ExampleData.Remove(row);
                    _context.SaveChanges();
                }
            }
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            var addForm = new AddForm();
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

                Grid.Rows.Add(row.Id, row.Data1, row.Data2, "Delete");
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
                Grid.Rows.Add(row.Id, row.Data1, row.Data2, "Delete");
            }
        }


    }
}
