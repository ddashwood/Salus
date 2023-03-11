namespace SalusExampleParent;

public partial class AddEditForm : Form
{
    public bool Save { get; private set; }

    public string Data1
    {
        get => Data1TextBox.Text;
        set => Data1TextBox.Text = value;
    }

    public string Data2
    {
        get => Data2TextBox.Text;
        set => Data2TextBox.Text = value;
    }

    public AddEditForm()
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
