using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
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
            this.Text = "Gerenciar Produtos - FarmaciaERP";
            this.Size = new Size(850, 650);
            this.StartPosition = FormStartPosition.CenterParent;
            ConfigurarLayout();
            AtualizarGrid();
        }

        private void ConfigurarLayout()
        {
            Panel pnlCadastro = new Panel { Dock = DockStyle.Top, Height = 180, Padding = new Padding(15), BackColor = Color.WhiteSmoke };
            
            txtCodigo = CriarCampo(pnlCadastro, "Código de Barras:", 15, 15);
            txtNome = CriarCampo(pnlCadastro, "Nome do Produto:", 15, 75);
            txtPreco = CriarCampo(pnlCadastro, "Preço (R$):", 300, 75);

            // Botão Salvar/Atualizar
            Button btnSalvar = new Button { 
                Text = "SALVAR / ATUALIZAR", 
                Left = 580, Top = 85, Width = 180, Height = 40, 
                BackColor = Color.SeaGreen, ForeColor = Color.White, 
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnSalvar.Click += (s, e) => SalvarProduto();

            // Botão Excluir
            Button btnExcluir = new Button { 
                Text = "EXCLUIR PRODUTO", 
                Left = 580, Top = 130, Width = 180, Height = 35, 
                BackColor = Color.Firebrick, ForeColor = Color.White, 
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnExcluir.Click += (s, e) => ExcluirProduto();

            // Botão Limpar
            Button btnLimpar = new Button { 
                Text = "LIMPAR CAMPOS", 
                Left = 580, Top = 40, Width = 180, Height = 30, 
                BackColor = Color.Gray, ForeColor = Color.White, FlatStyle = FlatStyle.Flat 
            };
            btnLimpar.Click += (s, e) => LimparCampos();

            pnlCadastro.Controls.Add(btnSalvar);
            pnlCadastro.Controls.Add(btnExcluir);
            pnlCadastro.Controls.Add(btnLimpar);

            gridProdutos = new DataGridView { 
                Dock = DockStyle.Fill, 
                BackgroundColor = Color.White, 
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            gridProdutos.CellClick += GridProdutos_CellClick;

            this.Controls.Add(gridProdutos);
            this.Controls.Add(pnlCadastro);
        }

        private TextBox CriarCampo(Panel p, string label, int x, int y)
        {
            Label lbl = new Label { Text = label, Left = x, Top = y, AutoSize = true, Font = new Font("Segoe UI", 9) };
            TextBox txt = new TextBox { Left = x, Top = y + 20, Width = 250, Font = new Font("Segoe UI", 11) };
            p.Controls.Add(lbl);
            p.Controls.Add(txt);
            return txt;
        }

        private void GridProdutos_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = gridProdutos.Rows[e.RowIndex];
                txtCodigo.Text = row.Cells["Código"].Value.ToString();
                txtNome.Text = row.Cells["Nome"].Value.ToString();
                txtPreco.Text = row.Cells["Preço (R$)"].Value.ToString();
                
                txtCodigo.ReadOnly = true; 
                txtCodigo.BackColor = Color.LightGray;
            }
        }

        private void SalvarProduto()
        {
            if (string.IsNullOrWhiteSpace(txtNome.Text) || string.IsNullOrWhiteSpace(txtCodigo.Text)) return;

            using (var conn = new MySqlConnection(connString))
            {
                try {
                    conn.Open();
                    string sql = @"INSERT INTO Produtos (codigo_barras, nome, preco) 
                                   VALUES (@c, @n, @p) 
                                   ON DUPLICATE KEY UPDATE nome=@n, preco=@p";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@c", txtCodigo.Text);
                        cmd.Parameters.AddWithValue("@n", txtNome.Text);
                        cmd.Parameters.AddWithValue("@p", decimal.Parse(txtPreco.Text));
                        cmd.ExecuteNonQuery();
                    }
                    MessageBox.Show("Operação realizada!");
                    LimparCampos();
                    AtualizarGrid();
                } catch (Exception ex) { MessageBox.Show("Erro: " + ex.Message); }
            }
        }

        private void ExcluirProduto()
        {
            if (string.IsNullOrWhiteSpace(txtCodigo.Text))
            {
                MessageBox.Show("Selecione um produto na lista para excluir!");
                return;
            }

            var confirmacao = MessageBox.Show($"Tem certeza que deseja excluir o produto {txtNome.Text}?", 
                                            "Confirmar Exclusão", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirmacao == DialogResult.Yes)
            {
                using (var conn = new MySqlConnection(connString))
                {
                    try {
                        conn.Open();
                        string sql = "DELETE FROM Produtos WHERE codigo_barras = @c";
                        using (var cmd = new MySqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@c", txtCodigo.Text);
                            cmd.ExecuteNonQuery();
                        }
                        MessageBox.Show("Produto removido com sucesso!");
                        LimparCampos();
                        AtualizarGrid();
                    } catch (Exception ex) { MessageBox.Show("Erro ao excluir: " + ex.Message); }
                }
            }
        }

        private void AtualizarGrid()
        {
            using (var conn = new MySqlConnection(connString))
            {
                try {
                    conn.Open();
                    string sql = "SELECT codigo_barras AS 'Código', nome AS 'Nome', preco AS 'Preço (R$)' FROM Produtos ORDER BY nome";
                    MySqlDataAdapter adapter = new MySqlDataAdapter(sql, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    gridProdutos.DataSource = dt;
                } catch (Exception ex) { MessageBox.Show("Erro ao carregar lista: " + ex.Message); }
            }
        }

        private void LimparCampos()
        {
            txtNome.Clear(); txtPreco.Clear(); txtCodigo.Clear(); 
            txtCodigo.ReadOnly = false; txtCodigo.BackColor = Color.White;
            txtCodigo.Focus();
        }
    }
}