using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace CoffeeShopManagement.Forms
{
    public class LoginForm : Form
    {
        private TabControl tabControl;
        private TextBox txtCustomerUser, txtCustomerPass;
        private TextBox txtStaffUser, txtStaffPass;
        private TextBox txtAdminUser, txtAdminPass;

        // ── Color Palette ──
        private readonly Color primaryColor = Color.FromArgb(101, 67, 33);
        private readonly Color primaryHover = Color.FromArgb(139, 94, 50);
        private readonly Color secondaryColor = Color.FromArgb(193, 154, 107);
        private readonly Color secondaryHover = Color.FromArgb(210, 178, 140);
        private readonly Color accentColor = Color.FromArgb(255, 183, 77);
        private readonly Color bgColor = Color.FromArgb(245, 235, 220);
        private readonly Color darkBg = Color.FromArgb(62, 39, 18);
        private readonly Color cardBg = Color.White;
        private readonly Color textDark = Color.FromArgb(51, 33, 17);
        private readonly Color fieldBg = Color.FromArgb(255, 252, 247);
        private readonly Color borderColor = Color.FromArgb(210, 195, 172);

        public LoginForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // ═══════════════════════════════════════════════════
            // FORM PROPERTIES — FORCED LARGE SIZE
            // ═══════════════════════════════════════════════════
            this.Text = "☕ Coffee Shop Marketplace — Login";
            this.Size = new Size(1100, 750);
            this.MinimumSize = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.BackColor = bgColor;
            this.Font = new Font("Segoe UI", 10);

            // ═══════════════════════════════════════════════════
            // LEFT BRANDING PANEL (Dock.Left, 400px)
            // ═══════════════════════════════════════════════════
            Panel brandPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 400,
                BackColor = darkBg
            };
            brandPanel.Paint += (s, e) =>
            {
                using var brush = new LinearGradientBrush(
                    brandPanel.ClientRectangle, darkBg,
                    Color.FromArgb(85, 55, 25), 135f);
                e.Graphics.FillRectangle(brush, brandPanel.ClientRectangle);
            };

            Label lblCup = new Label
            {
                Text = "☕",
                Font = new Font("Segoe UI", 72),
                ForeColor = accentColor,
                BackColor = Color.Transparent,
                AutoSize = false,
                Size = new Size(380, 110),
                Location = new Point(10, 100),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label lblBrand = new Label
            {
                Text = "COFFEE\nMARKETPLACE",
                Font = new Font("Segoe UI", 30, FontStyle.Bold),
                ForeColor = accentColor,
                BackColor = Color.Transparent,
                AutoSize = false,
                Size = new Size(380, 100),
                Location = new Point(10, 220),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label lblTagline = new Label
            {
                Text = "Multi-Vendor Management System",
                Font = new Font("Segoe UI", 13, FontStyle.Italic),
                ForeColor = secondaryColor,
                BackColor = Color.Transparent,
                AutoSize = false,
                Size = new Size(380, 30),
                Location = new Point(10, 330),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Decorative separator
            Panel sep = new Panel
            {
                BackColor = accentColor,
                Size = new Size(200, 3),
                Location = new Point(100, 375)
            };

            Label lblInfo = new Label
            {
                Text = "🏪  5 Shops  •  40+ Items  •  One Platform\n\n" +
                       "📦  Browse, Order & Pay\n" +
                       "⭐  Rate & Review Items\n" +
                       "📝  Submit Complaints\n" +
                       "💳  bKash • Nagad • Rocket",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(190, 170, 145),
                BackColor = Color.Transparent,
                AutoSize = false,
                Size = new Size(340, 150),
                Location = new Point(30, 400),
                TextAlign = ContentAlignment.TopCenter
            };

            Label lblCopyright = new Label
            {
                Text = "© 2024 OOP-2 Project | SQL Server Edition",
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                ForeColor = Color.FromArgb(130, 110, 85),
                BackColor = Color.Transparent,
                AutoSize = false,
                Size = new Size(380, 25),
                Location = new Point(10, 590),
                TextAlign = ContentAlignment.MiddleCenter
            };

            brandPanel.Controls.AddRange(new Control[] { lblCup, lblBrand, lblTagline, sep, lblInfo, lblCopyright });

            // ═══════════════════════════════════════════════════
            // RIGHT SIDE — TAB CONTROL (Dock.Fill)
            // ═══════════════════════════════════════════════════
            Panel rightPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = bgColor,
                Padding = new Padding(40, 30, 40, 30)
            };

            tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 12),
                Padding = new Point(25, 12),
                ItemSize = new Size(200, 50)
            };

            // ── Customer Tab ──
            TabPage customerTab = CreateLoginTab(
                "☕ Customer Login",
                "Welcome back! Login to browse & order.",
                out txtCustomerUser, out txtCustomerPass,
                "🔓 Login", BtnCustomerLogin_Click,
                "Don't have an account?", "📝 Sign Up", BtnCustomerSignUp_Click,
                null, null
            );
            customerTab.Text = "☕ Customer";

            // ── Staff Tab ──
            TabPage staffTab = CreateLoginTab(
                "👨‍🍳 Staff Login",
                "Staff portal — manage your shop's orders & products.",
                out txtStaffUser, out txtStaffPass,
                "🔓 Login", BtnStaffLogin_Click,
                null, null, null,
                "Forgot Password?", "Contact your Shop Manager or Admin to reset."
            );
            staffTab.Text = "👨‍🍳 Staff";

            // ── Admin Tab ──
            TabPage adminTab = CreateLoginTab(
                "🔒 Admin Login",
                "System admin — full control of the marketplace.",
                out txtAdminUser, out txtAdminPass,
                "🔓 Login", BtnAdminLogin_Click,
                null, null, null,
                "Forgot Password?", "Recovery Key: ADMIN@RESET2024\nContact System Administrator for password reset."
            );
            adminTab.Text = "🔒 Admin";

            tabControl.TabPages.AddRange(new[] { customerTab, staffTab, adminTab });
            rightPanel.Controls.Add(tabControl);

            // ═══════════════════════════════════════════════════
            // ASSEMBLE — brand left, tabs right (Fill)
            // ═══════════════════════════════════════════════════
            this.Controls.Add(rightPanel);
            this.Controls.Add(brandPanel);
        }

        // ═══════════════════════════════════════════════════════════
        // HELPER: Create a complete login tab with 3D buttons
        // ═══════════════════════════════════════════════════════════
        private TabPage CreateLoginTab(
            string title, string subtitle,
            out TextBox txtUser, out TextBox txtPass,
            string loginBtnText, EventHandler loginHandler,
            string? signUpPrompt, string? signUpBtnText, EventHandler? signUpHandler,
            string? forgotText, string? forgotMessage)
        {
            TabPage tab = new TabPage { BackColor = bgColor, Padding = new Padding(30) };

            // ── Card container ──
            Panel card = new Panel
            {
                Size = new Size(500, 450),
                Location = new Point(40, 25),
                BackColor = cardBg,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            card.Paint += (s, e) =>
            {
                using var pen = new Pen(borderColor, 1);
                e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                // Top accent bar
                using var accentBrush = new LinearGradientBrush(
                    new Rectangle(0, 0, card.Width, 5), accentColor, primaryColor, 0f);
                e.Graphics.FillRectangle(accentBrush, 0, 0, card.Width, 5);
            };

            Label lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = primaryColor,
                Location = new Point(30, 25),
                AutoSize = true
            };

            Label lblSubtitle = new Label
            {
                Text = subtitle,
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = Color.Gray,
                Location = new Point(32, 60),
                AutoSize = true
            };

            // Username field
            Label lblUser = new Label
            {
                Text = "👤  Username",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = textDark,
                Location = new Point(30, 105),
                AutoSize = true
            };
            txtUser = new TextBox
            {
                Size = new Size(430, 38),
                Location = new Point(30, 130),
                Font = new Font("Segoe UI", 13),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = fieldBg
            };

            // Password field
            Label lblPass = new Label
            {
                Text = "🔑  Password",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = textDark,
                Location = new Point(30, 180),
                AutoSize = true
            };
            txtPass = new TextBox
            {
                Size = new Size(430, 38),
                Location = new Point(30, 205),
                Font = new Font("Segoe UI", 13),
                BorderStyle = BorderStyle.FixedSingle,
                UseSystemPasswordChar = true,
                BackColor = fieldBg
            };

            // ── 3D LOGIN BUTTON ──
            Button btnLogin = Create3DButton(loginBtnText, new Point(30, 270), new Size(430, 50),
                primaryColor, primaryHover, Color.White);
            btnLogin.Click += loginHandler;

            card.Controls.AddRange(new Control[] { lblTitle, lblSubtitle, lblUser, txtUser, lblPass, txtPass, btnLogin });

            int nextY = 335;

            // ── FORGOT PASSWORD LINK ──
            if (forgotText != null && forgotMessage != null)
            {
                LinkLabel lnkForgot = new LinkLabel
                {
                    Text = $"🔗 {forgotText}",
                    Font = new Font("Segoe UI", 10, FontStyle.Italic),
                    LinkColor = Color.FromArgb(180, 120, 50),
                    ActiveLinkColor = Color.FromArgb(255, 140, 0),
                    VisitedLinkColor = Color.FromArgb(150, 100, 40),
                    Location = new Point(30, nextY),
                    AutoSize = true,
                    Cursor = Cursors.Hand
                };
                string msg = forgotMessage;
                lnkForgot.LinkClicked += (s, ev) =>
                {
                    MessageBox.Show(
                        msg,
                        "🔑 Password Recovery",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                };
                card.Controls.Add(lnkForgot);
                nextY += 35;
            }

            // ── SIGN UP SECTION ──
            if (signUpPrompt != null && signUpBtnText != null && signUpHandler != null)
            {
                Label lblNoAccount = new Label
                {
                    Text = signUpPrompt,
                    Font = new Font("Segoe UI", 9.5f),
                    ForeColor = Color.Gray,
                    Location = new Point(120, nextY + 5),
                    AutoSize = true
                };

                Button btnSignUp = Create3DButton(signUpBtnText, new Point(100, nextY + 30),
                    new Size(280, 42), secondaryColor, secondaryHover, Color.White);
                btnSignUp.Click += signUpHandler;

                card.Controls.AddRange(new Control[] { lblNoAccount, btnSignUp });
            }

            tab.Controls.Add(card);
            return tab;
        }

        // ═══════════════════════════════════════════════════════════
        // HELPER: Create a 3D-style button with hover effect
        // ═══════════════════════════════════════════════════════════
        private Button Create3DButton(string text, Point location, Size size,
            Color bgNormal, Color bgHover, Color fgColor)
        {
            Button btn = new Button
            {
                Text = text,
                Location = location,
                Size = size,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                BackColor = bgNormal,
                ForeColor = fgColor,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(bgNormal, 0.15f);

            // 3D hover effect
            btn.MouseEnter += (s, e) => btn.BackColor = bgHover;
            btn.MouseLeave += (s, e) => btn.BackColor = bgNormal;

            // Paint a subtle bottom shadow for 3D depth
            btn.Paint += (s, e) =>
            {
                // Bottom shadow line
                using var shadowPen = new Pen(ControlPaint.Dark(bgNormal, 0.25f), 2);
                e.Graphics.DrawLine(shadowPen, 0, btn.Height - 1, btn.Width, btn.Height - 1);
                // Right shadow line
                e.Graphics.DrawLine(shadowPen, btn.Width - 1, 0, btn.Width - 1, btn.Height);
                // Top highlight line
                using var highlightPen = new Pen(ControlPaint.Light(bgNormal, 0.3f), 1);
                e.Graphics.DrawLine(highlightPen, 0, 0, btn.Width - 1, 0);
                // Left highlight line
                e.Graphics.DrawLine(highlightPen, 0, 0, 0, btn.Height - 1);
            };

            return btn;
        }

        // ═══════════════════════════════════════════════════════════
        // EVENT HANDLERS
        // ═══════════════════════════════════════════════════════════

        private void BtnCustomerLogin_Click(object? sender, EventArgs e)
        {
            string username = txtCustomerUser.Text.Trim();
            string password = txtCustomerPass.Text.Trim();
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both username and password.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var conn = DatabaseHelper.GetConnection();
            var cmd = new SqlCommand(
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
                MessageBox.Show("Invalid username or password.", "Login Failed",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnStaffLogin_Click(object? sender, EventArgs e)
        {
            string username = txtStaffUser.Text.Trim();
            string password = txtStaffPass.Text.Trim();
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both username and password.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var conn = DatabaseHelper.GetConnection();
            var cmd = new SqlCommand(
                "SELECT StaffId, FirstName, LastName, Role, ShopId FROM Staff WHERE Username=@u AND Password=@p", conn);
            cmd.Parameters.AddWithValue("@u", username);
            cmd.Parameters.AddWithValue("@p", password);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                int staffId = reader.GetInt32(0);
                string name = $"{reader.GetString(1)} {reader.GetString(2)}";
                string role = reader.GetString(3);
                int shopId = reader.GetInt32(4);
                MessageBox.Show($"Welcome, {name}! ({role}) 👨‍🍳", "Login Successful",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Hide();
                var dashboard = new StaffDashboard(staffId, name, role, shopId);
                dashboard.FormClosed += (s, args) => this.Close();
                dashboard.Show();
            }
            else
            {
                MessageBox.Show("Invalid username or password.", "Login Failed",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAdminLogin_Click(object? sender, EventArgs e)
        {
            string username = txtAdminUser.Text.Trim();
            string password = txtAdminPass.Text.Trim();
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both username and password.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var conn = DatabaseHelper.GetConnection();
            var cmd = new SqlCommand(
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
                MessageBox.Show("Invalid admin credentials.", "Login Failed",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCustomerSignUp_Click(object? sender, EventArgs e)
        {
            var signUpForm = new CustomerSignUpForm();
            signUpForm.ShowDialog();
        }
    }
}
