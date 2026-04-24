using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;

namespace CoffeeShopManagement.Forms
{
    public class AdminDashboard : Form
    {
        private Panel contentPanel;

        private readonly Color primaryColor = Color.FromArgb(101, 67, 33);
        private readonly Color secondaryColor = Color.FromArgb(193, 154, 107);
        private readonly Color accentColor = Color.FromArgb(255, 183, 77);
        private readonly Color bgColor = Color.FromArgb(245, 235, 220);
        private readonly Color darkBg = Color.FromArgb(62, 39, 18);
        private readonly Color sidebarColor = Color.FromArgb(78, 52, 28);

        public AdminDashboard()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "☕ Admin Dashboard - Coffee Shop Management";
            this.Size = new Size(1100, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = bgColor;
            this.Font = new Font("Segoe UI", 10);

            // Sidebar
            Panel sidebar = new Panel { Dock = DockStyle.Left, Width = 230, BackColor = darkBg };
            sidebar.Paint += (s, e) =>
            {
                using var brush = new LinearGradientBrush(sidebar.ClientRectangle, darkBg, sidebarColor, 90f);
                e.Graphics.FillRectangle(brush, sidebar.ClientRectangle);
            };

            Label lblLogo = new Label
            {
                Text = "🔒",
                Font = new Font("Segoe UI", 36),
                ForeColor = accentColor,
                Size = new Size(230, 60),
                Location = new Point(0, 15),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            Label lblAdmin = new Label
            {
                Text = "ADMIN PANEL",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = accentColor,
                Size = new Size(210, 30),
                Location = new Point(10, 80),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            Label lblSubtitle = new Label
            {
                Text = "Full System Control",
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = secondaryColor,
                Size = new Size(210, 20),
                Location = new Point(10, 110),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            Panel divider = new Panel { BackColor = accentColor, Size = new Size(190, 2), Location = new Point(20, 140) };
            sidebar.Controls.AddRange(new Control[] { lblLogo, lblAdmin, lblSubtitle, divider });

            string[] menuItems = {
                "📊 Dashboard", "👥 All Users", "👨‍🍳 All Staff",
                "📦 All Products", "📋 All Orders", "💰 All Payments",
                "➕ Add Staff", "🚪 Logout"
            };
            EventHandler[] handlers = {
                ShowDashboard, ShowUsers, ShowStaff,
                ShowProducts, ShowOrders, ShowPayments,
                ShowAddStaff, DoLogout
            };

            for (int i = 0; i < menuItems.Length; i++)
            {
                Button btn = new Button
                {
                    Text = menuItems[i],
                    Size = new Size(210, 42),
                    Location = new Point(10, 155 + (i * 50)),
                    Font = new Font("Segoe UI", 10),
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
            contentPanel.Controls.Add(CreateTitle("📊 Admin Dashboard"));

            int users = 0, staff = 0, products = 0, orders = 0, pending = 0;
            decimal revenue = 0;

            using (var conn = DatabaseHelper.GetConnection())
            {
                users = Convert.ToInt32(new SqliteCommand("SELECT COUNT(*) FROM UserDetails", conn).ExecuteScalar());
                staff = Convert.ToInt32(new SqliteCommand("SELECT COUNT(*) FROM Staff", conn).ExecuteScalar());
                products = Convert.ToInt32(new SqliteCommand("SELECT COUNT(*) FROM Product", conn).ExecuteScalar());
                orders = Convert.ToInt32(new SqliteCommand("SELECT COUNT(*) FROM Orders", conn).ExecuteScalar());
                pending = Convert.ToInt32(new SqliteCommand("SELECT COUNT(*) FROM Orders WHERE Status='Pending'", conn).ExecuteScalar());
                revenue = Convert.ToDecimal(new SqliteCommand("SELECT COALESCE(SUM(Amount),0) FROM Payment", conn).ExecuteScalar());
            }

            var cards = new (string title, string value, string icon, Color color)[]
            {
                ("Total Users", users.ToString(), "👥", Color.FromArgb(33, 150, 243)),
                ("Total Staff", staff.ToString(), "👨‍🍳", Color.FromArgb(156, 39, 176)),
                ("Products", products.ToString(), "📦", Color.FromArgb(0, 150, 136)),
                ("Total Orders", orders.ToString(), "📋", Color.FromArgb(255, 152, 0)),
                ("Pending Orders", pending.ToString(), "⏳", Color.FromArgb(244, 67, 54)),
                ("Total Revenue", $"৳{revenue:N0}", "💰", Color.FromArgb(76, 175, 80))
            };

            for (int i = 0; i < cards.Length; i++)
            {
                int row = i / 3;
                int col = i % 3;
                Panel card = new Panel
                {
                    Size = new Size(250, 120),
                    Location = new Point(20 + (col * 270), 60 + (row * 140)),
                    BackColor = Color.White
                };
                card.Paint += (s, pe) =>
                {
                    using var pen = new Pen(Color.FromArgb(230, 220, 200), 1);
                    pe.Graphics.DrawRectangle(pen, 0, 0, ((Panel)s!).Width - 1, ((Panel)s!).Height - 1);
                };

                Panel colorBar = new Panel { Size = new Size(5, 120), Location = new Point(0, 0), BackColor = cards[i].color };

                Label icon = new Label
                {
                    Text = cards[i].icon,
                    Font = new Font("Segoe UI", 28),
                    Location = new Point(15, 20),
                    Size = new Size(55, 55)
                };

                Label title = new Label
                {
                    Text = cards[i].title,
                    Font = new Font("Segoe UI", 9),
                    ForeColor = Color.Gray,
                    Location = new Point(75, 15),
                    AutoSize = true
                };

                Label value = new Label
                {
                    Text = cards[i].value,
                    Font = new Font("Segoe UI", 22, FontStyle.Bold),
                    ForeColor = cards[i].color,
                    Location = new Point(75, 45),
                    AutoSize = true
                };

                card.Controls.AddRange(new Control[] { colorBar, icon, title, value });
                contentPanel.Controls.Add(card);
            }

            // Recent activity
            Label lblRecent = new Label
            {
                Text = "📋 Recent Orders",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = primaryColor,
                Location = new Point(20, 350),
                AutoSize = true
            };
            contentPanel.Controls.Add(lblRecent);

            DataGridView dgv = new DataGridView
            {
                Location = new Point(20, 385),
                Size = new Size(810, 230),
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
                    SELECT o.OrderId AS 'Order #',
                           COALESCE(u.FirstName || ' ' || u.LastName, 'Guest') AS 'Customer',
                           o.OrderDate AS 'Date', o.TotalAmount AS 'Total (৳)', o.Status
                    FROM Orders o
                    LEFT JOIN UserDetails u ON o.UserId = u.UserId
                    ORDER BY o.OrderId DESC LIMIT 10", conn);
                var dt = new DataTable();
                using var reader = cmd.ExecuteReader();
                dt.Load(reader);
                dgv.DataSource = dt;
            }

            contentPanel.Controls.Add(dgv);
        }

        // ===== ALL USERS =====
        private void ShowUsers(object? sender, EventArgs e)
        {
            ClearContent();
            contentPanel.Controls.Add(CreateTitle("👥 All Users"));

            DataGridView dgv = CreateDataGrid(new Point(20, 60), new Size(810, 400));

            using (var conn = DatabaseHelper.GetConnection())
            {
                var cmd = new SqliteCommand(@"
                    SELECT UserId AS 'ID', FirstName AS 'First Name', LastName AS 'Last Name',
                           Email, Phone, Address, Username
                    FROM UserDetails ORDER BY UserId", conn);
                var dt = new DataTable();
                using var reader = cmd.ExecuteReader();
                dt.Load(reader);
                dgv.DataSource = dt;
            }

            Button btnDelete = CreateDeleteButton("🗑 Delete User", new Point(20, 470));
            btnDelete.Click += (s, ev) =>
            {
                if (dgv.SelectedRows.Count > 0)
                {
                    int id = Convert.ToInt32(dgv.SelectedRows[0].Cells["ID"].Value);
                    if (MessageBox.Show("Delete this user and all their data?", "Confirm",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        using var conn = DatabaseHelper.GetConnection();
                        new SqliteCommand($"DELETE FROM OrderDetails WHERE OrderId IN (SELECT OrderId FROM Orders WHERE UserId={id})", conn).ExecuteNonQuery();
                        new SqliteCommand($"DELETE FROM Payment WHERE OrderId IN (SELECT OrderId FROM Orders WHERE UserId={id})", conn).ExecuteNonQuery();
                        new SqliteCommand($"DELETE FROM Orders WHERE UserId={id}", conn).ExecuteNonQuery();
                        new SqliteCommand($"DELETE FROM UserDetails WHERE UserId={id}", conn).ExecuteNonQuery();
                        ShowUsers(null, EventArgs.Empty);
                        MessageBox.Show("User deleted. ✅");
                    }
                }
            };

            contentPanel.Controls.AddRange(new Control[] { dgv, btnDelete });
        }

        // ===== ALL STAFF =====
        private void ShowStaff(object? sender, EventArgs e)
        {
            ClearContent();
            contentPanel.Controls.Add(CreateTitle("👨‍🍳 All Staff"));

            DataGridView dgv = CreateDataGrid(new Point(20, 60), new Size(810, 400));

            using (var conn = DatabaseHelper.GetConnection())
            {
                var cmd = new SqliteCommand(@"
                    SELECT StaffId AS 'ID', FirstName AS 'First Name', LastName AS 'Last Name',
                           Email, Phone, Role, Username
                    FROM Staff ORDER BY StaffId", conn);
                var dt = new DataTable();
                using var reader = cmd.ExecuteReader();
                dt.Load(reader);
                dgv.DataSource = dt;
            }

            Button btnDelete = CreateDeleteButton("🗑 Delete Staff", new Point(20, 470));
            btnDelete.Click += (s, ev) =>
            {
                if (dgv.SelectedRows.Count > 0)
                {
                    int id = Convert.ToInt32(dgv.SelectedRows[0].Cells["ID"].Value);
                    if (MessageBox.Show("Delete this staff member?", "Confirm",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        using var conn = DatabaseHelper.GetConnection();
                        new SqliteCommand($"DELETE FROM Staff WHERE StaffId={id}", conn).ExecuteNonQuery();
                        ShowStaff(null, EventArgs.Empty);
                        MessageBox.Show("Staff deleted. ✅");
                    }
                }
            };

            contentPanel.Controls.AddRange(new Control[] { dgv, btnDelete });
        }

        // ===== ALL PRODUCTS =====
        private void ShowProducts(object? sender, EventArgs e)
        {
            ClearContent();
            contentPanel.Controls.Add(CreateTitle("📦 All Products"));

            DataGridView dgv = CreateDataGrid(new Point(20, 60), new Size(810, 400));

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

            Button btnDelete = CreateDeleteButton("🗑 Delete Product", new Point(20, 470));
            btnDelete.Click += (s, ev) =>
            {
                if (dgv.SelectedRows.Count > 0)
                {
                    int id = Convert.ToInt32(dgv.SelectedRows[0].Cells["ID"].Value);
                    if (MessageBox.Show("Delete this product?", "Confirm",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        using var conn = DatabaseHelper.GetConnection();
                        new SqliteCommand($"DELETE FROM Product WHERE ProductId={id}", conn).ExecuteNonQuery();
                        ShowProducts(null, EventArgs.Empty);
                        MessageBox.Show("Product deleted. ✅");
                    }
                }
            };

            contentPanel.Controls.AddRange(new Control[] { dgv, btnDelete });
        }

        // ===== ALL ORDERS =====
        private void ShowOrders(object? sender, EventArgs e)
        {
            ClearContent();
            contentPanel.Controls.Add(CreateTitle("📋 All Orders"));

            DataGridView dgv = CreateDataGrid(new Point(20, 60), new Size(810, 450));

            using (var conn = DatabaseHelper.GetConnection())
            {
                var cmd = new SqliteCommand(@"
                    SELECT o.OrderId AS 'Order #',
                           COALESCE(u.FirstName || ' ' || u.LastName, 'Guest') AS 'Customer',
                           o.OrderDate AS 'Date', o.TotalAmount AS 'Total (৳)', o.Status,
                           (SELECT GROUP_CONCAT(p.ProductName || ' x' || od.Quantity, ', ')
                            FROM OrderDetails od JOIN Product p ON od.ProductId=p.ProductId
                            WHERE od.OrderId=o.OrderId) AS 'Items'
                    FROM Orders o
                    LEFT JOIN UserDetails u ON o.UserId = u.UserId
                    ORDER BY o.OrderId DESC", conn);
                var dt = new DataTable();
                using var reader = cmd.ExecuteReader();
                dt.Load(reader);
                dgv.DataSource = dt;
            }

            contentPanel.Controls.Add(dgv);
        }

        // ===== ALL PAYMENTS =====
        private void ShowPayments(object? sender, EventArgs e)
        {
            ClearContent();
            contentPanel.Controls.Add(CreateTitle("💰 All Payments"));

            DataGridView dgv = CreateDataGrid(new Point(20, 60), new Size(810, 450));

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

            // Revenue summary
            decimal totalRevenue = 0;
            using (var conn = DatabaseHelper.GetConnection())
            {
                totalRevenue = Convert.ToDecimal(new SqliteCommand("SELECT COALESCE(SUM(Amount),0) FROM Payment", conn).ExecuteScalar());
            }

            Label lblTotal = new Label
            {
                Text = $"💰 Total Revenue: ৳{totalRevenue:N2}",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(76, 175, 80),
                Location = new Point(20, 520),
                AutoSize = true
            };

            contentPanel.Controls.AddRange(new Control[] { dgv, lblTotal });
        }

        // ===== ADD STAFF =====
        private void ShowAddStaff(object? sender, EventArgs e)
        {
            ClearContent();
            contentPanel.Controls.Add(CreateTitle("➕ Add New Staff"));

            Panel formPanel = new Panel
            {
                Location = new Point(20, 60),
                Size = new Size(500, 450),
                BackColor = Color.White
            };
            formPanel.Paint += (s, pe) =>
            {
                using var pen = new Pen(Color.FromArgb(220, 210, 195), 1);
                pe.Graphics.DrawRectangle(pen, 0, 0, formPanel.Width - 1, formPanel.Height - 1);
            };

            int y = 20;
            TextBox txtFN = AddFormField(formPanel, "First Name:", ref y);
            TextBox txtLN = AddFormField(formPanel, "Last Name:", ref y);
            TextBox txtEmail = AddFormField(formPanel, "Email:", ref y);
            TextBox txtPhone = AddFormField(formPanel, "Phone:", ref y);

            Label lblRole = new Label
            {
                Text = "Role:",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(80, 50, 20),
                Location = new Point(20, y),
                AutoSize = true
            };
            ComboBox cmbRole = new ComboBox
            {
                Location = new Point(20, y + 20),
                Size = new Size(440, 28),
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbRole.Items.AddRange(new[] { "Barista", "Cashier", "Manager", "Supervisor" });
            cmbRole.SelectedIndex = 0;
            formPanel.Controls.AddRange(new Control[] { lblRole, cmbRole });
            y += 55;

            TextBox txtUsername = AddFormField(formPanel, "Username:", ref y);
            TextBox txtPassword = AddFormField(formPanel, "Password:", ref y, true);

            Button btnSave = new Button
            {
                Text = "💾 Create Staff Account",
                Location = new Point(20, y + 10),
                Size = new Size(440, 42),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                BackColor = primaryColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += (s, ev) =>
            {
                if (string.IsNullOrWhiteSpace(txtFN.Text) || string.IsNullOrWhiteSpace(txtEmail.Text) ||
                    string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
                {
                    MessageBox.Show("Please fill all required fields.", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    using var conn = DatabaseHelper.GetConnection();
                    var cmd = new SqliteCommand(@"
                        INSERT INTO Staff (FirstName, LastName, Email, Phone, Role, Username, Password)
                        VALUES (@fn, @ln, @em, @ph, @role, @un, @pw)", conn);
                    cmd.Parameters.AddWithValue("@fn", txtFN.Text.Trim());
                    cmd.Parameters.AddWithValue("@ln", txtLN.Text.Trim());
                    cmd.Parameters.AddWithValue("@em", txtEmail.Text.Trim());
                    cmd.Parameters.AddWithValue("@ph", txtPhone.Text.Trim());
                    cmd.Parameters.AddWithValue("@role", cmbRole.SelectedItem!.ToString());
                    cmd.Parameters.AddWithValue("@un", txtUsername.Text.Trim());
                    cmd.Parameters.AddWithValue("@pw", txtPassword.Text);
                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Staff account created! ✅", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    txtFN.Clear(); txtLN.Clear(); txtEmail.Clear();
                    txtPhone.Clear(); txtUsername.Clear(); txtPassword.Clear();
                }
                catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
                {
                    MessageBox.Show("Username already exists.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            formPanel.Controls.Add(btnSave);
            contentPanel.Controls.Add(formPanel);
        }

        // ===== HELPERS =====
        private TextBox AddFormField(Panel parent, string label, ref int y, bool isPassword = false)
        {
            Label lbl = new Label
            {
                Text = label,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(80, 50, 20),
                Location = new Point(20, y),
                AutoSize = true
            };
            TextBox txt = new TextBox
            {
                Location = new Point(20, y + 20),
                Size = new Size(440, 28),
                Font = new Font("Segoe UI", 10),
                UseSystemPasswordChar = isPassword
            };
            y += 55;
            parent.Controls.AddRange(new Control[] { lbl, txt });
            return txt;
        }

        private DataGridView CreateDataGrid(Point location, Size size)
        {
            DataGridView dgv = new DataGridView
            {
                Location = location,
                Size = size,
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
            return dgv;
        }

        private Button CreateDeleteButton(string text, Point location)
        {
            Button btn = new Button
            {
                Text = text,
                Location = location,
                Size = new Size(200, 40),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                BackColor = Color.FromArgb(244, 67, 54),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        // ===== LOGOUT =====
        private void DoLogout(object? sender, EventArgs e)
        {
            if (MessageBox.Show("Logout from Admin?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                this.Hide();
                var login = new LoginForm();
                login.FormClosed += (s, args) => this.Close();
                login.Show();
            }
        }
    }
}
