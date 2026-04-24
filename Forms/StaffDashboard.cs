using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;

namespace CoffeeShopManagement.Forms
{
    public class StaffDashboard : Form
    {
        private int staffId;
        private string staffName, staffRole;
        private Panel contentPanel;

        private readonly Color primaryColor = Color.FromArgb(101, 67, 33);
        private readonly Color secondaryColor = Color.FromArgb(193, 154, 107);
        private readonly Color accentColor = Color.FromArgb(255, 183, 77);
        private readonly Color bgColor = Color.FromArgb(245, 235, 220);
        private readonly Color darkBg = Color.FromArgb(62, 39, 18);
        private readonly Color sidebarColor = Color.FromArgb(78, 52, 28);

        public StaffDashboard(int staffId, string name, string role)
        {
            this.staffId = staffId;
            this.staffName = name;
            this.staffRole = role;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = $"☕ Staff Dashboard - {staffName} ({staffRole})";
            this.Size = new Size(1050, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = bgColor;
            this.Font = new Font("Segoe UI", 10);

            // Sidebar
            Panel sidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 220,
                BackColor = darkBg
            };
            sidebar.Paint += (s, e) =>
            {
                using var brush = new LinearGradientBrush(
                    sidebar.ClientRectangle, darkBg, sidebarColor, 90f);
                e.Graphics.FillRectangle(brush, sidebar.ClientRectangle);
            };

            Label lblLogo = new Label
            {
                Text = "👨‍🍳",
                Font = new Font("Segoe UI", 36),
                ForeColor = accentColor,
                Size = new Size(220, 60),
                Location = new Point(0, 15),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            Label lblWelcome = new Label
            {
                Text = $"{staffName}\n({staffRole})",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.White,
                Size = new Size(200, 50),
                Location = new Point(10, 80),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            Panel divider = new Panel { BackColor = accentColor, Size = new Size(180, 2), Location = new Point(20, 140) };
            sidebar.Controls.AddRange(new Control[] { lblLogo, lblWelcome, divider });

            string[] menuItems = { "🏠 Dashboard", "📦 Manage Products", "📋 Track Orders", "💰 Payments", "👤 My Account", "🚪 Logout" };
            EventHandler[] handlers = { ShowDashboard, ShowProducts, ShowOrders, ShowPayments, ShowAccount, DoLogout };

            for (int i = 0; i < menuItems.Length; i++)
            {
                Button btn = new Button
                {
                    Text = menuItems[i],
                    Size = new Size(200, 45),
                    Location = new Point(10, 155 + (i * 55)),
                    Font = new Font("Segoe UI", 11),
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    FlatStyle = FlatStyle.Flat,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(15, 0, 0, 0),
                    Cursor = Cursors.Hand
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(50, 255, 255, 255);
                btn.Click += handlers[i];
                sidebar.Controls.Add(btn);
            }

            contentPanel = new Panel { Dock = DockStyle.Fill, BackColor = bgColor, Padding = new Padding(20) };

            this.Controls.Add(contentPanel);
            this.Controls.Add(sidebar);
            ShowDashboard(null, EventArgs.Empty);
        }

        private void ClearContent() { contentPanel.Controls.Clear(); }

        private Label CreateTitle(string text) =>
            new Label { Text = text, Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = primaryColor, AutoSize = true, Location = new Point(20, 15) };

        // ===== DASHBOARD =====
        private void ShowDashboard(object? sender, EventArgs e)
        {
            ClearContent();
            contentPanel.Controls.Add(CreateTitle("🏠 Staff Dashboard"));

            int totalProducts = 0, totalOrders = 0, pendingOrders = 0, completedOrders = 0;
            decimal totalRevenue = 0;

            using (var conn = DatabaseHelper.GetConnection())
            {
                totalProducts = Convert.ToInt32(new SqliteCommand("SELECT COUNT(*) FROM Product", conn).ExecuteScalar());
                totalOrders = Convert.ToInt32(new SqliteCommand("SELECT COUNT(*) FROM Orders", conn).ExecuteScalar());
                pendingOrders = Convert.ToInt32(new SqliteCommand("SELECT COUNT(*) FROM Orders WHERE Status='Pending'", conn).ExecuteScalar());
                completedOrders = Convert.ToInt32(new SqliteCommand("SELECT COUNT(*) FROM Orders WHERE Status='Completed'", conn).ExecuteScalar());
                totalRevenue = Convert.ToDecimal(new SqliteCommand("SELECT COALESCE(SUM(Amount),0) FROM Payment", conn).ExecuteScalar());
            }

            var cards = new (string title, string value, Color color)[]
            {
                ("Products", totalProducts.ToString(), Color.FromArgb(33, 150, 243)),
                ("Total Orders", totalOrders.ToString(), Color.FromArgb(156, 39, 176)),
                ("Pending", pendingOrders.ToString(), Color.FromArgb(255, 152, 0)),
                ("Completed", completedOrders.ToString(), Color.FromArgb(76, 175, 80)),
                ("Revenue", $"৳{totalRevenue:N0}", Color.FromArgb(0, 150, 136))
            };

            for (int i = 0; i < cards.Length; i++)
            {
                Panel card = new Panel { Size = new Size(145, 110), Location = new Point(20 + (i * 155), 60), BackColor = Color.White };
                card.Paint += (s, pe) =>
                {
                    using var pen = new Pen(Color.FromArgb(230, 220, 200), 1);
                    pe.Graphics.DrawRectangle(pen, 0, 0, ((Panel)s!).Width - 1, ((Panel)s!).Height - 1);
                };
                Panel bar = new Panel { Size = new Size(145, 4), Location = new Point(0, 0), BackColor = cards[i].color };
                Label t = new Label { Text = cards[i].title, Font = new Font("Segoe UI", 9), ForeColor = Color.Gray, Location = new Point(10, 15), AutoSize = true };
                Label v = new Label { Text = cards[i].value, Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = cards[i].color, Location = new Point(10, 45), AutoSize = true };
                card.Controls.AddRange(new Control[] { bar, t, v });
                contentPanel.Controls.Add(card);
            }
        }

        // ===== MANAGE PRODUCTS =====
        private void ShowProducts(object? sender, EventArgs e)
        {
            ClearContent();
            contentPanel.Controls.Add(CreateTitle("📦 Manage Products"));

            // Add Product button
            Button btnAdd = new Button
            {
                Text = "➕ Add New Product",
                Location = new Point(20, 55),
                Size = new Size(200, 38),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += (s, ev) => { ShowProductForm(0); };

            contentPanel.Controls.Add(btnAdd);

            DataGridView dgv = new DataGridView
            {
                Location = new Point(20, 100),
                Size = new Size(760, 380),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                Font = new Font("Segoe UI", 9)
            };
            dgv.ColumnHeadersDefaultCellStyle.BackColor = primaryColor;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgv.EnableHeadersVisualStyles = false;

            using (var conn = DatabaseHelper.GetConnection())
            {
                var cmd = new SqliteCommand(@"
                    SELECT p.ProductId AS 'ID', p.ProductName AS 'Product',
                           c.CategoryName AS 'Category', p.Price AS 'Price (৳)',
                           p.Stock, p.Description
                    FROM Product p
                    JOIN Category c ON p.CategoryId = c.CategoryId
                    ORDER BY c.CategoryName, p.ProductName", conn);

                var dt = new DataTable();
                using var reader = cmd.ExecuteReader();
                dt.Load(reader);
                dgv.DataSource = dt;
            }

            // Edit and Delete buttons
            Button btnEdit = new Button
            {
                Text = "✏️ Edit",
                Location = new Point(240, 55),
                Size = new Size(100, 38),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = accentColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnEdit.FlatAppearance.BorderSize = 0;
            btnEdit.Click += (s, ev) =>
            {
                if (dgv.SelectedRows.Count > 0)
                {
                    int prodId = Convert.ToInt32(dgv.SelectedRows[0].Cells["ID"].Value);
                    ShowProductForm(prodId);
                }
                else
                    MessageBox.Show("Please select a product to edit.");
            };

            Button btnDelete = new Button
            {
                Text = "🗑️ Delete",
                Location = new Point(355, 55),
                Size = new Size(110, 38),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(244, 67, 54),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.Click += (s, ev) =>
            {
                if (dgv.SelectedRows.Count > 0)
                {
                    int prodId = Convert.ToInt32(dgv.SelectedRows[0].Cells["ID"].Value);
                    if (MessageBox.Show("Delete this product?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        using var conn = DatabaseHelper.GetConnection();
                        new SqliteCommand($"DELETE FROM Product WHERE ProductId={prodId}", conn).ExecuteNonQuery();
                        ShowProducts(null, EventArgs.Empty);
                    }
                }
            };

            contentPanel.Controls.AddRange(new Control[] { btnEdit, btnDelete, dgv });
        }

        private void ShowProductForm(int productId)
        {
            var form = new ProductManageForm(productId);
            form.ShowDialog();
            ShowProducts(null, EventArgs.Empty);
        }

        // ===== TRACK ORDERS =====
        private void ShowOrders(object? sender, EventArgs e)
        {
            ClearContent();
            contentPanel.Controls.Add(CreateTitle("📋 Track Orders"));

            // Filter
            ComboBox cmbStatus = new ComboBox
            {
                Location = new Point(20, 55),
                Size = new Size(180, 30),
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbStatus.Items.AddRange(new[] { "All Orders", "Pending", "Preparing", "Ready", "Completed", "Paid", "Cancelled" });
            cmbStatus.SelectedIndex = 0;

            DataGridView dgv = new DataGridView
            {
                Location = new Point(20, 95),
                Size = new Size(760, 340),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                Font = new Font("Segoe UI", 9)
            };
            dgv.ColumnHeadersDefaultCellStyle.BackColor = primaryColor;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgv.EnableHeadersVisualStyles = false;

            Action loadOrders = () =>
            {
                using var conn = DatabaseHelper.GetConnection();
                string query = @"
                    SELECT o.OrderId AS 'Order #',
                           COALESCE(u.FirstName || ' ' || u.LastName, 'Guest') AS 'Customer',
                           o.OrderDate AS 'Date', o.TotalAmount AS 'Total (৳)', o.Status,
                           (SELECT GROUP_CONCAT(p.ProductName || ' x' || od.Quantity, ', ')
                            FROM OrderDetails od JOIN Product p ON od.ProductId=p.ProductId
                            WHERE od.OrderId=o.OrderId) AS 'Items'
                    FROM Orders o
                    LEFT JOIN UserDetails u ON o.UserId = u.UserId";

                if (cmbStatus.SelectedIndex > 0)
                    query += $" WHERE o.Status = '{cmbStatus.SelectedItem}'";

                query += " ORDER BY o.OrderId DESC";

                var dt = new DataTable();
                using var reader = new SqliteCommand(query, conn).ExecuteReader();
                dt.Load(reader);
                dgv.DataSource = dt;
            };

            cmbStatus.SelectedIndexChanged += (s, ev) => loadOrders();
            loadOrders();

            // Status update buttons
            string[] statuses = { "Preparing", "Ready", "Completed", "Cancelled" };
            Color[] colors = { Color.FromArgb(33, 150, 243), Color.FromArgb(255, 152, 0), Color.FromArgb(76, 175, 80), Color.FromArgb(244, 67, 54) };

            for (int i = 0; i < statuses.Length; i++)
            {
                string status = statuses[i];
                Button btn = new Button
                {
                    Text = $"→ {status}",
                    Location = new Point(20 + (i * 190), 445),
                    Size = new Size(180, 38),
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    BackColor = colors[i],
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.Click += (s, ev) =>
                {
                    if (dgv.SelectedRows.Count > 0)
                    {
                        int orderId = Convert.ToInt32(dgv.SelectedRows[0].Cells["Order #"].Value);
                        using var conn = DatabaseHelper.GetConnection();
                        var cmd = new SqliteCommand("UPDATE Orders SET Status=@st WHERE OrderId=@id", conn);
                        cmd.Parameters.AddWithValue("@st", status);
                        cmd.Parameters.AddWithValue("@id", orderId);
                        cmd.ExecuteNonQuery();
                        loadOrders();
                        MessageBox.Show($"Order #{orderId} → {status} ✅");
                    }
                    else
                        MessageBox.Show("Select an order first.");
                };
                contentPanel.Controls.Add(btn);
            }

            contentPanel.Controls.AddRange(new Control[] { cmbStatus, dgv });
        }

        // ===== PAYMENTS =====
        private void ShowPayments(object? sender, EventArgs e)
        {
            ClearContent();
            contentPanel.Controls.Add(CreateTitle("💰 Payment Records"));

            DataGridView dgv = new DataGridView
            {
                Location = new Point(20, 60),
                Size = new Size(760, 420),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 9)
            };
            dgv.ColumnHeadersDefaultCellStyle.BackColor = primaryColor;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgv.EnableHeadersVisualStyles = false;

            using (var conn = DatabaseHelper.GetConnection())
            {
                var cmd = new SqliteCommand(@"
                    SELECT p.PaymentId AS 'Payment #', p.OrderId AS 'Order #',
                           COALESCE(u.FirstName || ' ' || u.LastName, 'Guest') AS 'Customer',
                           p.PaymentDate AS 'Date', p.Amount AS 'Amount (৳)', p.PaymentMethod AS 'Method'
                    FROM Payment p
                    JOIN Orders o ON p.OrderId = o.OrderId
                    LEFT JOIN UserDetails u ON o.UserId = u.UserId
                    ORDER BY p.PaymentId DESC", conn);

                var dt = new DataTable();
                using var reader = cmd.ExecuteReader();
                dt.Load(reader);
                dgv.DataSource = dt;
            }

            contentPanel.Controls.Add(dgv);
        }

        // ===== ACCOUNT =====
        private void ShowAccount(object? sender, EventArgs e)
        {
            ClearContent();
            var accountForm = new AccountForm(staffId, "Staff");
            accountForm.TopLevel = false;
            accountForm.FormBorderStyle = FormBorderStyle.None;
            accountForm.Dock = DockStyle.Fill;
            contentPanel.Controls.Add(accountForm);
            accountForm.Show();
        }

        // ===== LOGOUT =====
        private void DoLogout(object? sender, EventArgs e)
        {
            if (MessageBox.Show("Logout?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                this.Hide();
                var login = new LoginForm();
                login.FormClosed += (s, args) => this.Close();
                login.Show();
            }
        }
    }
}
