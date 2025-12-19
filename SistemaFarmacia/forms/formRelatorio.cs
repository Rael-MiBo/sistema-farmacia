using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using MySql.Data.MySqlClient;


namespace SistemaFarmacia
{
    public class FormRelatorio : Form
    {
        private string connString = "Server=127.0.0.1;Database=FarmaciaERP;Uid=root;Pwd=root;";
        private Label lblTotalSoma;
        private DateTimePicker pickerData;
        private DataGridView gridDetalhado;

        public FormRelatorio()
        {
            this.Text = "Relatório Detalhado de Itens Vendidos";
            this.Size = new Size(900, 650);
            this.StartPosition = FormStartPosition.CenterParent;
            ConfigurarLayout();
            BuscarVendasFiltro("DATE(v.data_hora) = CURDATE()");
        }

        private void ConfigurarLayout()
        {
            Panel pnlFiltros = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = Color.WhiteSmoke, Padding = new Padding(10) };

            Label lblData = new Label { Text = "Filtrar Data:", Left = 10, Top = 15, AutoSize = true };
            pickerData = new DateTimePicker { Left = 10, Top = 35, Width = 120, Format = DateTimePickerFormat.Short };
            pickerData.ValueChanged += (s, e) => BuscarVendasFiltro($"DATE(v.data_hora) = '{pickerData.Value:yyyy-MM-dd}'");

            Button btnHoje = CriarBotao(pnlFiltros, "HOJE", Color.DodgerBlue, 150, 30);
            btnHoje.Click += (s, e) => BuscarVendasFiltro("DATE(v.data_hora) = CURDATE()");

            Button btnMes = CriarBotao(pnlFiltros, "ESTE MÊS", Color.SeaGreen, 300, 30);
            btnMes.Click += (s, e) => BuscarVendasFiltro("MONTH(v.data_hora) = MONTH(CURDATE()) AND YEAR(v.data_hora) = YEAR(CURDATE())");

            Button btnAno = CriarBotao(pnlFiltros, "ESTE ANO", Color.DarkOrange, 450, 30);
            btnAno.Click += (s, e) => BuscarVendasFiltro("YEAR(v.data_hora) = YEAR(CURDATE())");

            gridDetalhado = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.AliceBlue }
            };

            lblTotalSoma = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                Text = "TOTAL NO PERÍODO: R$ 0,00",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White
            };

            pnlFiltros.Controls.Add(lblData);
            pnlFiltros.Controls.Add(pickerData);
            this.Controls.Add(gridDetalhado);
            this.Controls.Add(lblTotalSoma);
            this.Controls.Add(pnlFiltros);
        }

        private Button CriarBotao(Panel p, string texto, Color cor, int x, int y)
        {
            Button btn = new Button
            {
                Text = texto,
                Left = x,
                Top = y,
                Width = 130,
                Height = 40,
                BackColor = cor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            p.Controls.Add(btn);
            return btn;
        }

        private void BuscarVendasFiltro(string filtroSql)
        {
            using (var conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();

                    string sql = $@"SELECT 
                                        v.id AS 'Venda',
                                        iv.produto_nome AS 'Produto',
                                        iv.preco_unitario AS 'Valor Item',
                                        v.metodo_pagamento AS 'Pagamento',
                                        DATE_FORMAT(v.data_hora, '%d/%m/%Y %H:%i') AS 'Data/Hora'
                                    FROM Vendas v
                                    INNER JOIN Itens_Venda iv ON v.id = iv.venda_id
                                    WHERE {filtroSql}
                                    ORDER BY v.data_hora DESC";

                    MySqlDataAdapter adapter = new MySqlDataAdapter(sql, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    gridDetalhado.DataSource = dt;

                    string sqlSoma = $"SELECT SUM(total) FROM Vendas v WHERE {filtroSql}";
                    using (var cmd = new MySqlCommand(sqlSoma, conn))
                    {
                        var result = cmd.ExecuteScalar();
                        decimal total = (result != DBNull.Value && result != null) ? Convert.ToDecimal(result) : 0m;

                        lblTotalSoma.Text = $"TOTAL ACUMULADO NO PERÍODO: R$ {total:N2}";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro ao gerar relatório: " + ex.Message);
                }
            }
        }
    }
}