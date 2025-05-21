using System.Net.NetworkInformation;

namespace RedBank
{
    public partial class Login : Form
    {
        private Bank banco;
        private bool viewPassword = false;

        public Login()
        {
            InitializeComponent();
            banco = new Bank(); // usado apenas no primeiro carregamento
        }

        // Construtor adicional (opcional) se quiser passar o banco ao voltar do Home
        public Login(Bank bancoExistente)
        {
            InitializeComponent();
            banco = bancoExistente; // reutiliza o dicionário de contas
        }

        public bool IsOnline()
        {
            try
            {
                using (Ping ping = new Ping())
                {
                    PingReply reply = ping.Send("8.8.8.8", 1000); // Google DNS
                    return reply.Status == IPStatus.Success;
                }
            }
            catch
            {
                return false;
            }
        }

        private void buttonViewPass_Click(object sender, EventArgs e)
        {
            viewPassword = !viewPassword;
            inputSenha.PasswordChar = viewPassword ? '\0' : '●';
        }

        private async void buttonLogin_Click(object sender, EventArgs e)
        {
            string id = inputCPF.Text.Trim();
            var conta = banco.Login(id);

            if (conta != null)
            {
                // Salvar login se marcado
                if (checkboxManterLogin.Checked)
                {
                    Properties.Settings.Default.CPF = inputCPF.Text;
                    Properties.Settings.Default.Password = inputSenha.Text;
                    Properties.Settings.Default.RememberMe = true;
                }
                else
                {
                    Properties.Settings.Default.CPF = "";
                    Properties.Settings.Default.Password = "";
                    Properties.Settings.Default.RememberMe = false;
                }
                Properties.Settings.Default.Save();

                this.Hide();
                var homeForm = new Home(conta, id, banco);
                homeForm.FormClosed += (s, args) =>
                {
                    this.Show(); // reexibe o login ao fechar o Home
                };
                homeForm.Show();
            }
            else
            {
                labelInvalid.Visible = true;
                buttonLogin.Enabled = false;

                await Task.Delay(2000);

                labelInvalid.Visible = false;
                buttonLogin.Enabled = true;
            }
        }

        private void Login_Load(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.RememberMe)
            {
                inputCPF.Text = Properties.Settings.Default.CPF;
                inputSenha.Text = Properties.Settings.Default.Password;
                checkboxManterLogin.Checked = true;
            }

            if (!IsOnline())
            {
                labelFooter.ForeColor = Color.Maroon;
            }
        }
    }
}