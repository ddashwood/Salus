namespace SalusExampleParent;

public partial class AddForm : Form
{
    public bool Save { get; private set; }

    public string Data1 => Data1Textbox.Text;
    public string Data2 => Data2TextBox.Text;

    public AddForm()
    {
        InitializeComponent();
    }

    private void SaveButton_Click(object sender, EventArgs e)
    {
        Save = true;
        this.Close();
    }

    private void CancelButton_Click(object sender, EventArgs e)
    {
        this.Close();
    }
}
