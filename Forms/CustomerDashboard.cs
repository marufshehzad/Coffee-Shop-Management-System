using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;

namespace CoffeeShopManagement.Forms
{
    public class CustomerDashboard : Form
    {
        private int userId;
        private string customerName;
        private Panel contentPanel;

        private readonly Color primaryColor = Color.FromArgb(101, 67, 33);
        private readonly Color secondaryColor = Color.FromArgb(193, 154, 107);
        private readonly Color accentColor = Color.FromArgb(255, 183, 77);
        private readonly Color bgColor = Color.FromArgb(245, 235, 220);
        private readonly Color darkBg = Color.FromArgb(62, 39, 18);
        private readonly Color sidebarColor = Color.FromArgb(78, 52, 28);

        public CustomerDashboard(int userId, string name)
        {
            this.userId = userId;
            this.customerName = name;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = $"☕ Coffee Shop - Welcome {customerName}";
            this.Size = new Size(1000, 700);
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

            // Logo in sidebar
            Label lblLogo = new Label
            {
                Text = "☕",
                Font = new Font("Segoe UI", 36),
                ForeColor = accentColor,
                Size = new Size(220, 60),
                Location = new Point(0, 15),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            Label lblWelcome = new Label
            {
                Text = $"Welcome,\n{customerName}",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.White,
                Size = new Size(200, 50),
                Location = new Point(10, 80),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            Panel divider = new Panel
            {
                BackColor = accentColor,
                Size = new Size(180, 2),
                Location = new Point(20, 140)
            };

            sidebar.Controls.AddRange(new Control[] { lblLogo, lblWelcome, divider });

            // Sidebar buttons
            string[] menuItems = { "🏠 Dashboard", "📋 Browse Menu", "🛒 My Orders", "💳 Payment", "👤 My Account", "🚪 Logout" };
            EventHandler[] handlers = { ShowDashboard, ShowMenu, ShowOrders, ShowPayment, ShowAccount, DoLogout };

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

            // Content Panel
            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = bgColor,
                Padding = new Padding(20)
            };

            this.Controls.Add(contentPanel);
            this.Controls.Add(sidebar);

            ShowDashboard(null, EventArgs.Empty);
        }

        private void ClearContent()
        {
            contentPanel.Controls.Clear();
        }

        private Label CreateSectionTitle(string text)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = primaryColor,
                AutoSize = true,
                Location = new Point(20, 15)
            };
        }

        // ===== DASHBOARD VIEW =====
        private void ShowDashboard(object? sender, EventArgs e)
        {
            ClearContent();
            contentPanel.Controls.Add(CreateSectionTitle("🏠 Dashboard"));

            // Stats cards
            int totalOrders = 0, pendingOrders = 0;
            decimal totalSpent = 0;

            using (var conn = DatabaseHelper.GetConnection())
            {
                var cmd1 = new SqliteCommand("SELECT COUNT(*) FROM Orders WHERE UserId=@uid", conn);
                cmd1.Parameters.AddWithValue("@uid", userId);
                totalOrders = Convert.ToInt32(cmd1.ExecuteScalar());

                var cmd2 = new SqliteCommand("SELECT COUNT(*) FROM Orders WHERE UserId=@uid AND Status='Pending'", conn);
                cmd2.Parameters.AddWithValue("@uid", userId);
                pendingOrders = Convert.ToInt32(cmd2.ExecuteScalar());

                var cmd3 = new SqliteCommand("SELECT COALESCE(SUM(TotalAmount),0) FROM Orders WHERE UserId=@uid", conn);
                cmd3.Parameters.AddWithValue("@uid", userId);
                totalSpent = Convert.ToDecimal(cmd3.ExecuteScalar());
            }

            var cards = new (string title, string value, Color color)[]
            {
                ("Total Orders", totalOrders.ToString(), Color.FromArgb(76, 175, 80)),
                ("Pending Orders", pendingOrders.ToString(), Color.FromArgb(255, 152, 0)),
                ("Total Spent", $"৳{totalSpent:N2}", Color.FromArgb(33, 150, 243))
            };

            for (int i = 0; i < cards.Length; i++)
            {
                Panel card = new Panel
                {
                    Size = new Size(220, 120),
                    Location = new Point(20 + (i * 240), 60),
                    BackColor = Color.White
                };
                card.Paint += (s, pe) =>
                {
                    var p = (Panel)s!;
                    pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    using var pen = new Pen(Color.FromArgb(230, 220, 200), 1);
                    pe.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);
                };

                Panel colorBar = new Panel
                {
                    Size = new Size(5, 120),
                    Location = new Point(0, 0),
                    BackColor = cards[i].color
                };

                Label lblCardTitle = new Label
                {
                    Text = cards[i].title,
                    Font = new Font("Segoe UI", 10),
                    ForeColor = Color.Gray,
                    Location = new Point(20, 15),
                    AutoSize = true
                };

                Label lblCardValue = new Label
                {
                    Text = cards[i].value,
                    Font = new Font("Segoe UI", 24, FontStyle.Bold),
                    ForeColor = cards[i].color,
                    Location = new Point(20, 45),
                    AutoSize = true
                };

                card.Controls.AddRange(new Control[] { colorBar, lblCardTitle, lblCardValue });
                contentPanel.Controls.Add(card);
            }

            // Recent orders table
            Label lblRecent = new Label
            {
                Text = "📋 Recent Orders",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = primaryColor,
                Location = new Point(20, 200),
                AutoSize = true
            };
            contentPanel.Controls.Add(lblRecent);

            DataGridView dgv = new DataGridView
            {
                Location = new Point(20, 235),
                Size = new Size(720, 250),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                Font = new Font("Segoe UI", 9)
            };
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 235, 200);
            dgv.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = primaryColor;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgv.EnableHeadersVisualStyles = false;

            using (var conn = DatabaseHelper.GetConnection())
            {
                var cmd = new SqliteCommand(@"
                    SELECT OrderId AS 'Order #', OrderDate AS 'Date', 
                           TotalAmount AS 'Total (৳)', Status 
                    FROM Orders WHERE UserId=@uid 
                    ORDER BY OrderId DESC LIMIT 10", conn);
                cmd.Parameters.AddWithValue("@uid", userId);

                var dt = new DataTable();
                using var reader = cmd.ExecuteReader();
                dt.Load(reader);
                dgv.DataSource = dt;
            }

            contentPanel.Controls.Add(dgv);
        }

        // ===== BROWSE MENU =====
        private void ShowMenu(object? sender, EventArgs e)
        {
            ClearContent();
            var menuForm = new MenuBrowseForm(userId);
            menuForm.TopLevel = false;
            menuForm.FormBorderStyle = FormBorderStyle.None;
            menuForm.Dock = DockStyle.Fill;
            contentPanel.Controls.Add(menuForm);
            menuForm.Show();
        }

        // ===== MY ORDERS =====
        private void ShowOrders(object? sender, EventArgs e)
        {
            ClearContent();
            contentPanel.Controls.Add(CreateSectionTitle("🛒 My Orders"));

            DataGridView dgv = new DataGridView
            {
                Location = new Point(20, 60),
                Size = new Size(720, 400),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 9)
            };
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 235, 200);
            dgv.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = primaryColor;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgv.EnableHeadersVisualStyles = false;

            using (var conn = DatabaseHelper.GetConnection())
            {
                var cmd = new SqliteCommand(@"
                    SELECT o.OrderId AS 'Order #', o.OrderDate AS 'Date',
                           o.TotalAmount AS 'Total (৳)', o.Status,
                           (SELECT GROUP_CONCAT(p.ProductName || ' x' || od.Quantity, ', ')
                            FROM OrderDetails od
                            JOIN Product p ON od.ProductId = p.ProductId
                            WHERE od.OrderId = o.OrderId) AS 'Items'
                    FROM Orders o
                    WHERE o.UserId=@uid
                    ORDER BY o.OrderId DESC", conn);
                cmd.Parameters.AddWithValue("@uid", userId);

                var dt = new DataTable();
                using var reader = cmd.ExecuteReader();
                dt.Load(reader);
                dgv.DataSource = dt;
            }

            contentPanel.Controls.Add(dgv);
        }

        // ===== PAYMENT =====
        private void ShowPayment(object? sender, EventArgs e)
        {
            ClearContent();
            contentPanel.Controls.Add(CreateSectionTitle("💳 Payment History"));

            DataGridView dgv = new DataGridView
            {
                Location = new Point(20, 60),
                Size = new Size(720, 250),
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
                           p.PaymentDate AS 'Date', p.Amount AS 'Amount (৳)',
                           p.PaymentMethod AS 'Method'
                    FROM Payment p
                    JOIN Orders o ON p.OrderId = o.OrderId
                    WHERE o.UserId=@uid
                    ORDER BY p.PaymentId DESC", conn);
                cmd.Parameters.AddWithValue("@uid", userId);

                var dt = new DataTable();
                using var reader = cmd.ExecuteReader();
                dt.Load(reader);
                dgv.DataSource = dt;
            }

            contentPanel.Controls.Add(dgv);

            // Pending payments section
            Label lblPending = new Label
            {
                Text = "⏳ Pending Payments",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = primaryColor,
                Location = new Point(20, 330),
                AutoSize = true
            };
            contentPanel.Controls.Add(lblPending);

            ComboBox cmbOrders = new ComboBox
            {
                Location = new Point(20, 365),
                Size = new Size(300, 30),
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            using (var conn = DatabaseHelper.GetConnection())
            {
                var cmd = new SqliteCommand(@"
                    SELECT o.OrderId, o.TotalAmount 
                    FROM Orders o 
                    LEFT JOIN Payment p ON o.OrderId = p.OrderId
                    WHERE o.UserId=@uid AND p.PaymentId IS NULL AND o.Status != 'Cancelled'
                    ORDER BY o.OrderId DESC", conn);
                cmd.Parameters.AddWithValue("@uid", userId);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    cmbOrders.Items.Add($"Order #{reader.GetInt32(0)} - ৳{reader.GetDecimal(1):N2}");
                }
            }

            if (cmbOrders.Items.Count > 0)
                cmbOrders.SelectedIndex = 0;

            ComboBox cmbMethod = new ComboBox
            {
                Location = new Point(340, 365),
                Size = new Size(200, 30),
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbMethod.Items.AddRange(new[] { "Cash", "Credit Card", "Debit Card", "Mobile Banking", "bKash" });
            cmbMethod.SelectedIndex = 0;

            Button btnPay = new Button
            {
                Text = "💳 Pay Now",
                Location = new Point(560, 362),
                Size = new Size(150, 36),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnPay.FlatAppearance.BorderSize = 0;
            btnPay.Click += (s, ev) =>
            {
                if (cmbOrders.SelectedItem == null)
                {
                    MessageBox.Show("No pending orders to pay.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string selected = cmbOrders.SelectedItem.ToString()!;
                int orderId = int.Parse(selected.Split('#')[1].Split('-')[0].Trim());

                using var conn = DatabaseHelper.GetConnection();
                var cmd = new SqliteCommand("SELECT TotalAmount FROM Orders WHERE OrderId=@oid", conn);
                cmd.Parameters.AddWithValue("@oid", orderId);
                decimal amount = Convert.ToDecimal(cmd.ExecuteScalar());

                var payCmd = new SqliteCommand(@"
                    INSERT INTO Payment (OrderId, Amount, PaymentMethod) VALUES (@oid, @amt, @method)", conn);
                payCmd.Parameters.AddWithValue("@oid", orderId);
                payCmd.Parameters.AddWithValue("@amt", amount);
                payCmd.Parameters.AddWithValue("@method", cmbMethod.SelectedItem!.ToString());
                payCmd.ExecuteNonQuery();

                var updateCmd = new SqliteCommand("UPDATE Orders SET Status='Paid' WHERE OrderId=@oid", conn);
                updateCmd.Parameters.AddWithValue("@oid", orderId);
                updateCmd.ExecuteNonQuery();

                MessageBox.Show($"Payment of ৳{amount:N2} successful! ✅", "Payment Complete",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                ShowPayment(null, EventArgs.Empty);
            };

            contentPanel.Controls.AddRange(new Control[] { cmbOrders, cmbMethod, btnPay });
        }

        // ===== MY ACCOUNT =====
        private void ShowAccount(object? sender, EventArgs e)
        {
            ClearContent();
            var accountForm = new AccountForm(userId, "Customer");
            accountForm.TopLevel = false;
            accountForm.FormBorderStyle = FormBorderStyle.None;
            accountForm.Dock = DockStyle.Fill;
            contentPanel.Controls.Add(accountForm);
            accountForm.Show();
        }

        // ===== LOGOUT =====
        private void DoLogout(object? sender, EventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to logout?", "Logout",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                this.Hide();
                var login = new LoginForm();
                login.FormClosed += (s, args) => this.Close();
                login.Show();
            }
        }
    }
}
