using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;

namespace CoffeeShopManagement.Forms
{
    public class LoginForm : Form
    {
        private TabControl tabControl;
        private TextBox txtCustomerUser, txtCustomerPass;
        private TextBox txtStaffUser, txtStaffPass;
        private TextBox txtAdminUser, txtAdminPass;

        // Color scheme
        private readonly Color primaryColor = Color.FromArgb(101, 67, 33);      // Coffee brown
        private readonly Color secondaryColor = Color.FromArgb(193, 154, 107);   // Light brown
        private readonly Color accentColor = Color.FromArgb(255, 183, 77);       // Gold accent
        private readonly Color bgColor = Color.FromArgb(245, 235, 220);          // Cream
        private readonly Color darkBg = Color.FromArgb(62, 39, 18);              // Dark brown
        private readonly Color textLight = Color.White;
        private readonly Color textDark = Color.FromArgb(51, 33, 17);

        public LoginForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "☕ Coffee Shop Management System";
            this.Size = new Size(520, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = bgColor;
            this.Font = new Font("Segoe UI", 10);

            // Header Panel
            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 120,
                BackColor = darkBg
            };
            headerPanel.Paint += (s, e) =>
            {
                using var brush = new LinearGradientBrush(
                    headerPanel.ClientRectangle, darkBg, primaryColor, 45f);
                e.Graphics.FillRectangle(brush, headerPanel.ClientRectangle);
            };

            Label lblTitle = new Label
            {
                Text = "☕ COFFEE SHOP",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = accentColor,
                AutoSize = false,
                Size = new Size(500, 45),
                Location = new Point(10, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            Label lblSubtitle = new Label
            {
                Text = "Management System",
                Font = new Font("Segoe UI", 14, FontStyle.Italic),
                ForeColor = secondaryColor,
                AutoSize = false,
                Size = new Size(500, 30),
                Location = new Point(10, 65),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            headerPanel.Controls.AddRange(new Control[] { lblTitle, lblSubtitle });

            // Tab Control
            tabControl = new TabControl
            {
                Location = new Point(30, 140),
                Size = new Size(440, 350),
                Font = new Font("Segoe UI", 11),
                Padding = new Point(20, 8)
            };

            // Customer Tab
            TabPage customerTab = CreateLoginTab("Customer Login",
                out txtCustomerUser, out txtCustomerPass,
                "Login", "Sign Up",
                BtnCustomerLogin_Click, BtnCustomerSignUp_Click);
            customerTab.Text = "☕ Customer";

            // Staff Tab
            TabPage staffTab = CreateLoginTab("Staff Login",
                out txtStaffUser, out txtStaffPass,
                "Login", null,
                BtnStaffLogin_Click, null);
            staffTab.Text = "👨‍🍳 Staff";

            // Admin Tab
            TabPage adminTab = CreateLoginTab("Admin Login",
                out txtAdminUser, out txtAdminPass,
                "Login", null,
                BtnAdminLogin_Click, null);
            adminTab.Text = "🔒 Admin";

            tabControl.TabPages.AddRange(new[] { customerTab, staffTab, adminTab });

            // Footer
            Label lblFooter = new Label
            {
                Text = "© 2024 Coffee Shop Management System | OOP-2 Project",
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                ForeColor = Color.FromArgb(150, 120, 90),
                AutoSize = false,
                Size = new Size(440, 30),
                Location = new Point(30, 510),
                TextAlign = ContentAlignment.MiddleCenter
            };

            this.Controls.AddRange(new Control[] { headerPanel, tabControl, lblFooter });
        }

        private TabPage CreateLoginTab(string title,
            out TextBox txtUser, out TextBox txtPass,
            string loginBtnText, string? signUpBtnText,
            EventHandler loginHandler, EventHandler? signUpHandler)
        {
            TabPage tab = new TabPage
            {
                BackColor = bgColor,
                Padding = new Padding(20)
            };

            Label lblHeader = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = primaryColor,
                Location = new Point(100, 20),
                AutoSize = true
            };

            Label lblUser = new Label
            {
                Text = "👤 Username:",
                Font = new Font("Segoe UI", 11),
                ForeColor = textDark,
                Location = new Point(50, 75),
                AutoSize = true
            };

            txtUser = new TextBox
            {
                Size = new Size(300, 35),
                Location = new Point(50, 100),
                Font = new Font("Segoe UI", 12),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };

            Label lblPass = new Label
            {
                Text = "🔑 Password:",
                Font = new Font("Segoe UI", 11),
                ForeColor = textDark,
                Location = new Point(50, 145),
                AutoSize = true
            };

            txtPass = new TextBox
            {
                Size = new Size(300, 35),
                Location = new Point(50, 170),
                Font = new Font("Segoe UI", 12),
                BorderStyle = BorderStyle.FixedSingle,
                UseSystemPasswordChar = true,
                BackColor = Color.White
            };

            Button btnLogin = CreateStyledButton(loginBtnText, new Point(50, 225), new Size(300, 42), primaryColor);
            btnLogin.Click += loginHandler;

            tab.Controls.AddRange(new Control[] { lblHeader, lblUser, txtUser, lblPass, txtPass, btnLogin });

            if (signUpBtnText != null && signUpHandler != null)
            {
                Label lblNoAccount = new Label
                {
                    Text = "Don't have an account?",
                    Font = new Font("Segoe UI", 9),
                    ForeColor = Color.Gray,
                    Location = new Point(110, 275),
                    AutoSize = true
                };

                Button btnSignUp = CreateStyledButton(signUpBtnText, new Point(120, 295), new Size(160, 35), secondaryColor);
                btnSignUp.FlatAppearance.BorderColor = primaryColor;
                btnSignUp.Click += signUpHandler;

                tab.Controls.AddRange(new Control[] { lblNoAccount, btnSignUp });
            }

            return tab;
        }

        private Button CreateStyledButton(string text, Point location, Size size, Color bgColor)
        {
            Button btn = new Button
            {
                Text = text,
                Location = location,
                Size = size,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                BackColor = bgColor,
                ForeColor = textLight,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, size.Width, size.Height, 10, 10));

            btn.MouseEnter += (s, e) => btn.BackColor = ControlPaint.Light(bgColor, 0.2f);
            btn.MouseLeave += (s, e) => btn.BackColor = bgColor;

            return btn;
        }

        [System.Runtime.InteropServices.DllImport("Gdi32.dll")]
        private static extern IntPtr CreateRoundRectRgn(int x1, int y1, int x2, int y2, int cx, int cy);

        // ===== EVENT HANDLERS =====

        private void BtnCustomerLogin_Click(object? sender, EventArgs e)
        {
            string username = txtCustomerUser.Text.Trim();
            string password = txtCustomerPass.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both username and password.",
                    "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var conn = DatabaseHelper.GetConnection();
            var cmd = new SqliteCommand(
                "SELECT UserId, FirstName, LastName FROM UserDetails WHERE Username=@u AND Password=@p", conn);
            cmd.Parameters.AddWithValue("@u", username);
            cmd.Parameters.AddWithValue("@p", password);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                int userId = reader.GetInt32(0);
                string name = $"{reader.GetString(1)} {reader.GetString(2)}";
                MessageBox.Show($"Welcome, {name}! ☕", "Login Successful",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.Hide();
                var dashboard = new CustomerDashboard(userId, name);
                dashboard.FormClosed += (s, args) => this.Close();
                dashboard.Show();
            }
            else
            {
                MessageBox.Show("Invalid username or password.",
                    "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnStaffLogin_Click(object? sender, EventArgs e)
        {
            string username = txtStaffUser.Text.Trim();
            string password = txtStaffPass.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both username and password.",
                    "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var conn = DatabaseHelper.GetConnection();
            var cmd = new SqliteCommand(
                "SELECT StaffId, FirstName, LastName, Role FROM Staff WHERE Username=@u AND Password=@p", conn);
            cmd.Parameters.AddWithValue("@u", username);
            cmd.Parameters.AddWithValue("@p", password);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                int staffId = reader.GetInt32(0);
                string name = $"{reader.GetString(1)} {reader.GetString(2)}";
                string role = reader.GetString(3);
                MessageBox.Show($"Welcome, {name}! ({role}) 👨‍🍳", "Login Successful",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.Hide();
                var dashboard = new StaffDashboard(staffId, name, role);
                dashboard.FormClosed += (s, args) => this.Close();
                dashboard.Show();
            }
            else
            {
                MessageBox.Show("Invalid username or password.",
                    "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAdminLogin_Click(object? sender, EventArgs e)
        {
            string username = txtAdminUser.Text.Trim();
            string password = txtAdminPass.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both username and password.",
                    "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var conn = DatabaseHelper.GetConnection();
            var cmd = new SqliteCommand(
                "SELECT AdminId FROM Admin WHERE Username=@u AND Password=@p", conn);
            cmd.Parameters.AddWithValue("@u", username);
            cmd.Parameters.AddWithValue("@p", password);

            var result = cmd.ExecuteScalar();
            if (result != null)
            {
                MessageBox.Show("Welcome, Admin! 🔒", "Login Successful",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.Hide();
                var dashboard = new AdminDashboard();
                dashboard.FormClosed += (s, args) => this.Close();
                dashboard.Show();
            }
            else
            {
                MessageBox.Show("Invalid admin credentials.",
                    "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCustomerSignUp_Click(object? sender, EventArgs e)
        {
            var signUpForm = new CustomerSignUpForm();
            signUpForm.ShowDialog();
        }
    }
}
