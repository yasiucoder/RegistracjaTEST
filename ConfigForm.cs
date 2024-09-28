using System;
using System.Windows.Forms;

namespace RegistracjaTEST
{
    public class ConfigForm : Form
    {
        private NumericUpDown numAccounts;
        private NumericUpDown numThreads;
        private Button submitButton;
        private ComboBox siteSelector;
        public RegistrationConfig SelectedConfig { get; private set; }

        public int NumberOfAccounts { get; private set; } = 9;
        public int MaxConcurrentThreads { get; private set; } = 3;

        public ConfigForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Konfiguracja";
            this.Size = new System.Drawing.Size(350, 230);

            Label labelAccounts = new Label
            {
                Text = "Liczba kont:",
                Location = new System.Drawing.Point(10, 50),
                AutoSize = true
            };
            this.Controls.Add(labelAccounts);

            numAccounts = new NumericUpDown
            {
                Location = new System.Drawing.Point(200, 50),
                Minimum = 1,
                Maximum = 100,
                Value = NumberOfAccounts,
                Width = 100
            };
            this.Controls.Add(numAccounts);

            Label labelThreads = new Label
            {
                Text = "Maksymalna liczba wątków:",
                Location = new System.Drawing.Point(10, 80),
                AutoSize = true
            };
            this.Controls.Add(labelThreads);

            numThreads = new NumericUpDown
            {
                Location = new System.Drawing.Point(200, 80),
                Minimum = 1,
                Maximum = 10,
                Value = MaxConcurrentThreads,
                Width = 100
            };
            this.Controls.Add(numThreads);

            submitButton = new Button
            {
                Text = "Zatwierdź",
                Location = new System.Drawing.Point(125, 130),
                Width = 100
            };
            submitButton.Click += SubmitButton_Click;
            this.Controls.Add(submitButton);

            siteSelector = new ComboBox
            {
                Location = new System.Drawing.Point(10, 20),
                Width = 300,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            siteSelector.Items.AddRange(new string[] { "Strona 1 (bez PINu)", "Strona 2 (z PINem)" });
            siteSelector.SelectedIndex = 0;
            this.Controls.Add(siteSelector);
        }

        private void SubmitButton_Click(object sender, EventArgs e)
        {
            NumberOfAccounts = (int)numAccounts.Value;
            MaxConcurrentThreads = (int)numThreads.Value;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        public (int numOfAccounts, int maxConcurrentThreads, RegistrationConfig regConfig) GetConfiguration()
        {
            if (this.ShowDialog() == DialogResult.OK)
            {
                return (NumberOfAccounts, MaxConcurrentThreads, SelectedConfig);
            }
            return (9, 3, null); // wartości domyślne
        }
    }
}