using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;

namespace CoffeeShopManagement.Forms
{
    public class CustomerSignUpForm : Form
    {
        private TextBox txtFirstName, txtLastName, txtEmail, txtPhone;
        private TextBox txtAddress, txtUsername, txtPassword, txtConfirmPassword;

        private readonly Color primaryColor = Color.FromArgb(101, 67, 33);
        private readonly Color bgColor = Color.FromArgb(245, 235, 220);
        private readonly Color darkBg = Color.FromArgb(62, 39, 18);
        private readonly Color accentColor = Color.FromArgb(255, 183, 77);
        private readonly Color secondaryColor = Color.FromArgb(193, 154, 107);

        public CustomerSignUpForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "☕ Customer Sign Up";
            this.Size = new Size(500, 650);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = bgColor;
            this.Font = new Font("Segoe UI", 10);

            // Header
            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
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
                Text = "☕ Create Your Account",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = accentColor,
                AutoSize = false,
                Size = new Size(480, 50),
                Location = new Point(10, 10),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            headerPanel.Controls.Add(lblTitle);

            int y = 90;
            int labelX = 40, fieldX = 40;
            int fieldWidth = 400;

            txtFirstName = AddField("First Name:", ref y, labelX, fieldX, fieldWidth);
            txtLastName = AddField("Last Name:", ref y, labelX, fieldX, fieldWidth);
            txtEmail = AddField("Email:", ref y, labelX, fieldX, fieldWidth);
            txtPhone = AddField("Phone:", ref y, labelX, fieldX, fieldWidth);
            txtAddress = AddField("Address:", ref y, labelX, fieldX, fieldWidth);
            txtUsername = AddField("Username:", ref y, labelX, fieldX, fieldWidth);
            txtPassword = AddField("Password:", ref y, labelX, fieldX, fieldWidth, true);
            txtConfirmPassword = AddField("Confirm Password:", ref y, labelX, fieldX, fieldWidth, true);

            Button btnSignUp = new Button
            {
                Text = "✅ Sign Up",
                Location = new Point(40, y + 5),
                Size = new Size(400, 42),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                BackColor = primaryColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSignUp.FlatAppearance.BorderSize = 0;
            btnSignUp.Click += BtnSignUp_Click;
            btnSignUp.MouseEnter += (s, e) => btnSignUp.BackColor = secondaryColor;
            btnSignUp.MouseLeave += (s, e) => btnSignUp.BackColor = primaryColor;

            this.Controls.AddRange(new Control[] { headerPanel, btnSignUp });
        }

        private TextBox AddField(string labelText, ref int y, int labelX, int fieldX, int fieldWidth, bool isPassword = false)
        {
            Label lbl = new Label
            {
                Text = labelText,
                Location = new Point(labelX, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(80, 50, 20)
            };

            TextBox txt = new TextBox
            {
                Location = new Point(fieldX, y + 20),
                Size = new Size(fieldWidth, 28),
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle,
                UseSystemPasswordChar = isPassword
            };

            y += 55;
            this.Controls.AddRange(new Control[] { lbl, txt });
            return txt;
        }

        private void BtnSignUp_Click(object? sender, EventArgs e)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(txtFirstName.Text) ||
                string.IsNullOrWhiteSpace(txtLastName.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text) ||
                string.IsNullOrWhiteSpace(txtUsername.Text) ||
                string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Please fill in all required fields (First Name, Last Name, Email, Username, Password).",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (txtPassword.Text != txtConfirmPassword.Text)
            {
                MessageBox.Show("Passwords do not match.",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!txtEmail.Text.Contains("@"))
            {
                MessageBox.Show("Please enter a valid email address.",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using var conn = DatabaseHelper.GetConnection();
                var cmd = new SqliteCommand(@"
                    INSERT INTO UserDetails (FirstName, LastName, Email, Phone, Address, Username, Password)
                    VALUES (@fn, @ln, @em, @ph, @ad, @un, @pw)", conn);

                cmd.Parameters.AddWithValue("@fn", txtFirstName.Text.Trim());
                cmd.Parameters.AddWithValue("@ln", txtLastName.Text.Trim());
                cmd.Parameters.AddWithValue("@em", txtEmail.Text.Trim());
                cmd.Parameters.AddWithValue("@ph", txtPhone.Text.Trim());
                cmd.Parameters.AddWithValue("@ad", txtAddress.Text.Trim());
                cmd.Parameters.AddWithValue("@un", txtUsername.Text.Trim());
                cmd.Parameters.AddWithValue("@pw", txtPassword.Text);

                cmd.ExecuteNonQuery();

                MessageBox.Show("Account created successfully! You can now log in. ☕",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19) // UNIQUE constraint
            {
                MessageBox.Show("Username already exists. Please choose a different one.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating account: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
