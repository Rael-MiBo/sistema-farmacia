using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace SistemaFarmacia
{
    public class FormProdutos : Form
    {
        private string connString = "Server=127.0.0.1;Database=FarmaciaERP;Uid=root;Pwd=root;";
        private DataGridView gridProdutos;
        private TextBox txtNome, txtPreco, txtCodigo;

        public FormProdutos()
        {
            this.Text = "Cadastro de Produtos - FarmaciaERP";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            ConfigurarLayout();
            AtualizarGrid();
        }

        private void ConfigurarLayout()
        {
            Panel pnlCadastro = new Panel { Dock = DockStyle.Top, Height = 150, Padding = new Padding(10), BackColor = Color.WhiteSmoke };
            
            txtCodigo = CriarCampo(pnlCadastro, "Código de Barras:", 10, 20);
            txtNome = CriarCampo(pnlCadastro, "Nome do Produto:", 10, 70);
            txtPreco = CriarCampo(pnlCadastro, "Preço (R$):", 300, 70);

            Button btnSalvar = new Button { 
                Text = "SALVAR PRODUTO", 
                Left = 550, Top = 85, Width = 150, Height = 40, 
                BackColor = Color.SeaGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat 
            };
            btnSalvar.Click += (s, e) => SalvarProduto();
            pnlCadastro.Controls.Add(btnSalvar);

            gridProdutos = new DataGridView { 
                Dock = DockStyle.Fill, 
                BackgroundColor = Color.White, 
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true
            };

            this.Controls.Add(gridProdutos);
            this.Controls.Add(pnlCadastro);
        }

        private TextBox CriarCampo(Panel p, string label, int x, int y)
        {
            Label lbl = new Label { Text = label, Left = x, Top = y, AutoSize = true };
            TextBox txt = new TextBox { Left = x, Top = y + 20, Width = 250, Font = new Font("Segoe UI", 10) };
            p.Controls.Add(lbl);
            p.Controls.Add(txt);
            return txt;
        }

        private void SalvarProduto()
        {
            if (string.IsNullOrWhiteSpace(txtNome.Text) || string.IsNullOrWhiteSpace(txtCodigo.Text)) return;

            using (var conn = new MySqlConnection(connString))
            {
                try {
                    conn.Open();
                    string sql = "INSERT INTO Produtos (codigo_barras, nome, preco) VALUES (@c, @n, @p) ON DUPLICATE KEY UPDATE nome=@n, preco=@p";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@c", txtCodigo.Text);
                        cmd.Parameters.AddWithValue("@n", txtNome.Text);
                        cmd.Parameters.AddWithValue("@p", decimal.Parse(txtPreco.Text));
                        cmd.ExecuteNonQuery();
                    }
                    MessageBox.Show("Produto salvo com sucesso!");
                    LimparCampos();
                    AtualizarGrid();
                } catch (Exception ex) { MessageBox.Show("Erro: " + ex.Message); }
            }
        }

        private void AtualizarGrid()
        {
            using (var conn = new MySqlConnection(connString))
            {
                conn.Open();
                string sql = "SELECT codigo_barras AS 'Código', nome AS 'Nome', preco AS 'Preço (R$)' FROM Produtos";
                MySqlDataAdapter adapter = new MySqlDataAdapter(sql, conn);
                System.Data.DataTable dt = new System.Data.DataTable();
                adapter.Fill(dt);
                gridProdutos.DataSource = dt;
            }
        }

        private void LimparCampos()
        {
            txtNome.Clear(); txtPreco.Clear(); txtCodigo.Clear(); txtCodigo.Focus();
        }
    }
}