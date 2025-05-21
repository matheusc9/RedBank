using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace RedBank
{
    public partial class Home : Form
    {
        private Bank banco;
        private BankAccount contaAtual;
        private string contaIdAtual;
        private bool hideNumbers = false;
        private string contaId;


        public Home(BankAccount conta, string id, Bank bankRef)
        {
            InitializeComponent();
            banco = bankRef;            // Usa o mesmo banco instanciado no Login
            contaAtual = conta;
            contaId = id;
        }

        private void Home_Load(object sender, EventArgs e)
        {
            if (contaAtual != null)
            {
                labelWelcome.Text = $"Olá, {contaAtual.Nome}";
                labelSaldo.Text = $"R${contaAtual.Saldo}";
                AtualizarHistorico();
            }
            else
            {
                labelWelcome.Text = "ID inválido.";
            }
        }

        private void AtualizarHistorico()
        {
            listBoxHistorico.Items.Clear();
            foreach (var item in contaAtual.Historico)
            {
                listBoxHistorico.Items.Add(item);
            }
            listBoxHistorico.TopIndex = listBoxHistorico.Items.Count - 1;
        }

        // Exemplos de chamada que você pode usar em botões:
        private void Depositar_Click(object sender, EventArgs e)
        {
            string resultado = contaAtual.Depositar(100); // valor de exemplo
            MessageBox.Show(resultado);
        }

        private void Transferir_Click(object sender, EventArgs e)
        {
            string destino = "1"; // exemplo
            double valor = 50;

            string resultado = contaAtual.Transferir(banco.Contas, contaIdAtual, destino, valor);
            MessageBox.Show(resultado);
        }

        private void buttonHide_Click(object sender, EventArgs e)
        {
            hideNumbers = !hideNumbers;

            if (hideNumbers)
            {
                labelSaldo.Text = new string('●', labelSaldo.Text.Length);
                buttonHide.Text = "✖️";
            }
            else
            {
                buttonHide.Text = "👁️";
                labelSaldo.Text = $"R${contaAtual.Saldo}";
            }
        }

        private void circleConta_Click(object sender, EventArgs e)
        {
            this.Hide();
            Login loginForm = new Login(banco); // ✅ Passa o mesmo banco usado
            loginForm.FormClosed += (s, args) => Environment.Exit(0); // mais elegante que Environment.Exit
            loginForm.Show();
        }

        private void buttonTransferir_Click(object sender, EventArgs e)
        {
            panelTransferir.Visible = !panelTransferir.Visible;
        }

        private void btnClosePanelTransferir_Click(object sender, EventArgs e)
        {
            panelTransferir.Visible = false;
        }

        private void btnTransferirValor_Click(object sender, EventArgs e)
        {
            panelTransferir.Visible = false;
            string destinoId = inputDestinoTransferir.Text;
            double valor = double.Parse(inputValorTransferir.Text);
            string msg = contaAtual.Transferir(banco.Contas, contaId, destinoId, valor);
            labelSaldo.Text = $"R${contaAtual.Saldo}";
            //MessageBox.Show(msg);
            AtualizarHistorico();
        }

        private void buttonDepositar_Click(object sender, EventArgs e)
        {
            panelDepositar.Visible = !panelDepositar.Visible;
        }

        private void btnClosePanelDepositar_Click(object sender, EventArgs e)
        {
            panelDepositar.Visible = false;
        }

        private void btnDepositarValor_Click(object sender, EventArgs e)
        {
            panelDepositar.Visible = false;
            double valor = double.Parse(inputValorDepositar.Text);
            string msg = contaAtual.Depositar(valor);
            labelSaldo.Text = $"R${contaAtual.Saldo}";
            //MessageBox.Show(msg);
            AtualizarHistorico();
        }

        private void buttonHistorico_Click(object sender, EventArgs e)
        {
            listBoxHistorico.Visible = !listBoxHistorico.Visible;
        }
    }

    // ==== Backend do banco (convertido do Python) ====

    public class BankAccount
    {
        public string Nome { get; set; }
        public double Saldo { get; set; }
        public List<string> Historico { get; private set; } = new List<string>();

        public BankAccount(string nome, double saldo)
        {
            Nome = nome;
            Saldo = saldo;
        }

        public string Depositar(double valor)
        {
            if (valor < 1)
            {
                return "Valor inválido para depósito.";
            }

            Saldo += valor;
            string msg = $"Depósito de R${valor} realizado.";
            Historico.Add(msg);
            return $"Depósito concluído: R${valor} para {Nome}";
        }

        public string Transferir(Dictionary<string, BankAccount> contas, string myId, string destinoId, double valor)
        {
            if (valor > Saldo)
            {
                return "Você não possui saldo suficiente!";
            }

            if (valor < 1)
            {
                return "Valor inválido!";
            }

            if (!contas.ContainsKey(destinoId))
            {
                return "ID inválido ou inexistente!";
            }

            if (destinoId == myId)
            {
                return "Você não pode transferir para si mesmo!";
            }

            Saldo -= valor;
            contas[destinoId].Saldo += valor;

            string msgEnvio = $"Transferência de R${valor} para {contas[destinoId].Nome} (ID {destinoId})";
            string msgRecebido = $"Recebido R${valor} de {Nome} (ID {myId})";

            Historico.Add(msgEnvio);
            contas[destinoId].Historico.Add(msgRecebido);

            return $"Transferência concluída: R${valor} para {contas[destinoId].Nome}";
        }
    }

    public class Bank
    {
        public Dictionary<string, BankAccount> Contas { get; private set; }

        public Bank()
        {
            Contas = new Dictionary<string, BankAccount>
            {
                { "1", new BankAccount("Alice Oliveira", 150.0) },
                { "2", new BankAccount("Bruno Santos", 200.0) },
                { "3", new BankAccount("Carla Veiga", 300.0) },
                { "4", new BankAccount("Diego Silva", 450.0) },
                { "5", new BankAccount("Elisa Pereira", 500.0) },
            };
        }

        public BankAccount Login(string id)
        {
            if (Contas.ContainsKey(id))
            {
                return Contas[id];
            }
            return null;
        }
    }
}
