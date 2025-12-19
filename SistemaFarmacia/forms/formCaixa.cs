using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using MySql.Data.MySqlClient;


namespace SistemaFarmacia
{
    public partial class FormCaixa : Form
    {
        private string connString = "Server=127.0.0.1;Database=FarmaciaERP;Uid=root;Pwd=root;";
        private DataGridView gridItens;
        private Label lblTotal;
        private TextBox txtBarcode;
        private Button btnFinalizar;
        private decimal totalVenda = 0;

        public FormCaixa()
        {
            ConfigurarJanela();
            CriarComponentes();
        }

        private void ConfigurarJanela()
        {
            this.Text = "Farmácia ERP - Frente de Caixa";
            this.Size = new Size(1024, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(245, 245, 245);
        }

        private void CriarComponentes()
        {
            Panel pnlTopo = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.FromArgb(0, 122, 204) };
            Label lblTitulo = new Label
            {
                Text = "SISTEMA DE VENDAS - FARMÁCIA",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlTopo.Controls.Add(lblTitulo);

            gridItens = new DataGridView
            {
                Dock = DockStyle.Left,
                Width = 600,
                BackgroundColor = Color.White,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false,
                MultiSelect = false,
                Font = new Font("Segoe UI", 10)
            };
            gridItens.Columns.Add("id", "Cód");
            gridItens.Columns.Add("nome", "Produto");
            gridItens.Columns.Add("preco", "Preço (R$)");
            gridItens.KeyDown += GridItens_KeyDown;

            Panel pnlDireito = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            Label lblInstr = new Label { Text = "Bipe o Produto:", Dock = DockStyle.Top, Height = 30, Font = new Font("Segoe UI", 12) };
            txtBarcode = new TextBox { Dock = DockStyle.Top, Font = new Font("Segoe UI", 24), BackColor = Color.LightYellow };
            txtBarcode.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) ProcessarBip(txtBarcode.Text); };

            lblTotal = new Label
            {
                Text = "TOTAL: R$ 0,00",
                Dock = DockStyle.Bottom,
                Height = 100,
                Font = new Font("Segoe UI", 32, FontStyle.Bold),
                ForeColor = Color.DarkGreen,
                TextAlign = ContentAlignment.MiddleRight
            };

            btnFinalizar = new Button
            {
                Text = "FINALIZAR VENDA",
                Dock = DockStyle.Bottom,
                Height = 80,
                BackColor = Color.FromArgb(0, 150, 136),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 16, FontStyle.Bold)
            };
            btnFinalizar.Click += (s, e) => MostrarOpcoesPagamento();

            Button btnRelatorio = new Button { Text = "RELATÓRIOS", Dock = DockStyle.Bottom, Height = 40, BackColor = Color.FromArgb(45, 45, 48), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnRelatorio.Click += (s, e) => { new FormRelatorio().ShowDialog(); };

            Button btnProdutos = new Button { Text = "GERENCIAR PRODUTOS", Dock = DockStyle.Bottom, Height = 40, BackColor = Color.FromArgb(0, 122, 204), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnProdutos.Click += (s, e) => { new FormProdutos().ShowDialog(); };

            pnlDireito.Controls.Add(txtBarcode);
            pnlDireito.Controls.Add(lblInstr);
            pnlDireito.Controls.Add(btnRelatorio);
            pnlDireito.Controls.Add(new Label { Dock = DockStyle.Bottom, Height = 5 });
            pnlDireito.Controls.Add(btnProdutos);
            pnlDireito.Controls.Add(new Label { Dock = DockStyle.Bottom, Height = 5 });
            pnlDireito.Controls.Add(btnFinalizar);
            pnlDireito.Controls.Add(lblTotal);

            this.Controls.Add(pnlDireito);
            this.Controls.Add(gridItens);
            this.Controls.Add(pnlTopo);
        }

        private void ProcessarBip(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo)) return;
            using (var conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT id, nome, preco FROM Produtos WHERE codigo_barras = @c";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@c", codigo);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                decimal preco = reader.GetDecimal("preco");
                                gridItens.Rows.Add(reader["id"], reader["nome"], preco.ToString("N2"));
                                totalVenda += preco;
                                lblTotal.Text = $"TOTAL: R$ {totalVenda:N2}";
                            }
                            else { MessageBox.Show("Produto não cadastrado!"); }
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Erro: " + ex.Message); }
            }
            txtBarcode.Clear();
            txtBarcode.Focus();
        }

        private void GridItens_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                RemoverItemSelecionado();
            }
        }

        private void RemoverItemSelecionado()
        {
            if (gridItens.CurrentRow != null)
            {
                var valorItemStr = gridItens.CurrentRow.Cells[2].Value.ToString();
                if (decimal.TryParse(valorItemStr, out decimal valorItem))
                {
                    totalVenda -= valorItem;
                    if (totalVenda < 0) totalVenda = 0;
                    lblTotal.Text = $"TOTAL: R$ {totalVenda:N2}";
                    gridItens.Rows.Remove(gridItens.CurrentRow);
                }
            }
        }

        private void MostrarOpcoesPagamento()
        {
            if (totalVenda <= 0) return;
            ContextMenuStrip menu = new ContextMenuStrip();
            ToolStripMenuItem menuPix = new ToolStripMenuItem("Pagar com PIX", null, (s, e) => FinalizarVenda("Pix"));
            ToolStripMenuItem menuDinheiro = new ToolStripMenuItem("Pagar com DINHEIRO", null, (s, e) => FinalizarVenda("Dinheiro"));
            menu.Items.Add(menuPix);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(menuDinheiro);
            menu.Show(btnFinalizar, new Point(0, -menu.Height));
        }

        private void FinalizarVenda(string metodo)
        {
            using (var conn = new MySqlConnection(connString))
            {
                conn.Open();
                var trans = conn.BeginTransaction();
                try
                {
                    string sqlVenda = "INSERT INTO Vendas (total, metodo_pagamento, data_hora) VALUES (@t, @m, NOW()); SELECT LAST_INSERT_ID();";
                    MySqlCommand cmdVenda = new MySqlCommand(sqlVenda, conn, trans);
                    cmdVenda.Parameters.AddWithValue("@t", totalVenda);
                    cmdVenda.Parameters.AddWithValue("@m", metodo);
                    int vendaId = Convert.ToInt32(cmdVenda.ExecuteScalar());

                    foreach (DataGridViewRow row in gridItens.Rows)
                    {
                        string sqlItem = "INSERT INTO Itens_Venda (venda_id, produto_nome, preco_unitario) VALUES (@vId, @nome, @preco)";
                        MySqlCommand cmdItem = new MySqlCommand(sqlItem, conn, trans);
                        cmdItem.Parameters.AddWithValue("@vId", vendaId);
                        cmdItem.Parameters.AddWithValue("@nome", row.Cells[1].Value.ToString());
                        cmdItem.Parameters.AddWithValue("@preco", decimal.Parse(row.Cells[2].Value?.ToString() ?? "0"));
                        cmdItem.ExecuteNonQuery();
                    }
                    trans.Commit();
                    MessageBox.Show($"Venda no ({metodo}) finalizada!");
                    LimparCarrinho();
                }
                catch (Exception ex) { trans.Rollback(); MessageBox.Show("Erro ao salvar: " + ex.Message); }
            }
        }

        private void LimparCarrinho()
        {
            gridItens.Rows.Clear();
            totalVenda = 0;
            lblTotal.Text = "TOTAL: R$ 0,00";
            txtBarcode.Focus();
        }
    }
}