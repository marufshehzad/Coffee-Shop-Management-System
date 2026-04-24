using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;

namespace CoffeeShopManagement.Forms
{
    public class AccountForm : Form
    {
        private int userId;
        private string userType; // "Customer" or "Staff"
        private TextBox txtFirstName, txtLastName, txtEmail, txtPhone, txtAddress;
        private TextBox txtUsername, txtOldPassword, txtNewPassword;

        private readonly Color primaryColor = Color.FromArgb(101, 67, 33);
        private readonly Color bgColor = Color.FromArgb(245, 235, 220);
        private readonly Color accentColor = Color.FromArgb(255, 183, 77);

        public AccountForm(int userId, string userType)
        {
            this.userId = userId;
            this.userType = userType;
            InitializeComponent();
            LoadUserData();
        }

        private void InitializeComponent()
        {
            this.Text = "My Account";
            this.BackColor = bgColor;
            this.Font = new Font("Segoe UI", 10);
            this.Size = new Size(760, 600);

            Label lblTitle = new Label
            {
                Text = "👤 My Account",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = primaryColor,
                Location = new Point(20, 10),
                AutoSize = true
            };

            // Profile section
            Panel profilePanel = new Panel
            {
                Location = new Point(20, 55),
                Size = new Size(700, 400),
                BackColor = Color.White
            };
            profilePanel.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(220, 210, 195), 1);
                e.Graphics.DrawRectangle(pen, 0, 0, profilePanel.Width - 1, profilePanel.Height - 1);
            };

            Label lblProfileTitle = new Label
            {
                Text = "📝 Profile Information",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = primaryColor,
                Location = new Point(20, 15),
                AutoSize = true
            };

            int y = 50;
            txtFirstName = AddProfileField(profilePanel, "First Name:", ref y);
            txtLastName = AddProfileField(profilePanel, "Last Name:", ref y);
            txtEmail = AddProfileField(profilePanel, "Email:", ref y);
            txtPhone = AddProfileField(profilePanel, "Phone:", ref y);
            txtAddress = AddProfileField(profilePanel, "Address:", ref y);
            txtUsername = AddProfileField(profilePanel, "Username:", ref y);
            txtUsername.ReadOnly = true;
            txtUsername.BackColor = Color.FromArgb(240, 240, 240);

            Button btnUpdate = new Button
            {
                Text = "💾 Update Profile",
                Location = new Point(20, y + 10),
                Size = new Size(200, 40),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                BackColor = primaryColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnUpdate.FlatAppearance.BorderSize = 0;
            btnUpdate.Click += BtnUpdate_Click;

            // Password change section
            Label lblPassTitle = new Label
            {
                Text = "🔑 Change Password",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = primaryColor,
                Location = new Point(380, 50),
                AutoSize = true
            };

            Label lblOldPass = new Label
            {
                Text = "Current Password:",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(80, 50, 20),
                Location = new Point(380, 80),
                AutoSize = true
            };
            txtOldPassword = new TextBox
            {
                Location = new Point(380, 100),
                Size = new Size(280, 28),
                UseSystemPasswordChar = true
            };

            Label lblNewPass = new Label
            {
                Text = "New Password:",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(80, 50, 20),
                Location = new Point(380, 135),
                AutoSize = true
            };
            txtNewPassword = new TextBox
            {
                Location = new Point(380, 155),
                Size = new Size(280, 28),
                UseSystemPasswordChar = true
            };

            Button btnChangePass = new Button
            {
                Text = "🔑 Change Password",
                Location = new Point(380, 195),
                Size = new Size(200, 35),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = accentColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnChangePass.FlatAppearance.BorderSize = 0;
            btnChangePass.Click += BtnChangePassword_Click;

            profilePanel.Controls.AddRange(new Control[] {
                lblProfileTitle, btnUpdate, lblPassTitle, lblOldPass, txtOldPassword,
                lblNewPass, txtNewPassword, btnChangePass
            });

            this.Controls.AddRange(new Control[] { lblTitle, profilePanel });
        }

        private TextBox AddProfileField(Panel parent, string label, ref int y)
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
                Location = new Point(20, y + 18),
                Size = new Size(320, 28),
                Font = new Font("Segoe UI", 10)
            };

            y += 50;
            parent.Controls.AddRange(new Control[] { lbl, txt });
            return txt;
        }

        private void LoadUserData()
        {
            using var conn = DatabaseHelper.GetConnection();

            if (userType == "Customer")
            {
                var cmd = new SqliteCommand(
                    "SELECT FirstName, LastName, Email, Phone, Address, Username FROM UserDetails WHERE UserId=@id", conn);
                cmd.Parameters.AddWithValue("@id", userId);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    txtFirstName.Text = reader.IsDBNull(0) ? "" : reader.GetString(0);
                    txtLastName.Text = reader.IsDBNull(1) ? "" : reader.GetString(1);
                    txtEmail.Text = reader.IsDBNull(2) ? "" : reader.GetString(2);
                    txtPhone.Text = reader.IsDBNull(3) ? "" : reader.GetString(3);
                    txtAddress.Text = reader.IsDBNull(4) ? "" : reader.GetString(4);
                    txtUsername.Text = reader.IsDBNull(5) ? "" : reader.GetString(5);
                }
            }
            else // Staff
            {
                var cmd = new SqliteCommand(
                    "SELECT FirstName, LastName, Email, Phone, '', Username FROM Staff WHERE StaffId=@id", conn);
                cmd.Parameters.AddWithValue("@id", userId);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    txtFirstName.Text = reader.IsDBNull(0) ? "" : reader.GetString(0);
                    txtLastName.Text = reader.IsDBNull(1) ? "" : reader.GetString(1);
                    txtEmail.Text = reader.IsDBNull(2) ? "" : reader.GetString(2);
                    txtPhone.Text = reader.IsDBNull(3) ? "" : reader.GetString(3);
                    txtAddress.Text = reader.IsDBNull(4) ? "" : reader.GetString(4);
                    txtUsername.Text = reader.IsDBNull(5) ? "" : reader.GetString(5);
                }
            }
        }

        private void BtnUpdate_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFirstName.Text) || string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("First Name and Email are required.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using var conn = DatabaseHelper.GetConnection();

                if (userType == "Customer")
                {
                    var cmd = new SqliteCommand(@"
                        UPDATE UserDetails SET FirstName=@fn, LastName=@ln, Email=@em, Phone=@ph, Address=@ad
                        WHERE UserId=@id", conn);
                    cmd.Parameters.AddWithValue("@fn", txtFirstName.Text.Trim());
                    cmd.Parameters.AddWithValue("@ln", txtLastName.Text.Trim());
                    cmd.Parameters.AddWithValue("@em", txtEmail.Text.Trim());
                    cmd.Parameters.AddWithValue("@ph", txtPhone.Text.Trim());
                    cmd.Parameters.AddWithValue("@ad", txtAddress.Text.Trim());
                    cmd.Parameters.AddWithValue("@id", userId);
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    var cmd = new SqliteCommand(@"
                        UPDATE Staff SET FirstName=@fn, LastName=@ln, Email=@em, Phone=@ph
                        WHERE StaffId=@id", conn);
                    cmd.Parameters.AddWithValue("@fn", txtFirstName.Text.Trim());
                    cmd.Parameters.AddWithValue("@ln", txtLastName.Text.Trim());
                    cmd.Parameters.AddWithValue("@em", txtEmail.Text.Trim());
                    cmd.Parameters.AddWithValue("@ph", txtPhone.Text.Trim());
                    cmd.Parameters.AddWithValue("@id", userId);
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Profile updated successfully! ✅", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating profile: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnChangePassword_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtOldPassword.Text) || string.IsNullOrWhiteSpace(txtNewPassword.Text))
            {
                MessageBox.Show("Please enter both current and new password.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using var conn = DatabaseHelper.GetConnection();
                string table = userType == "Customer" ? "UserDetails" : "Staff";
                string idCol = userType == "Customer" ? "UserId" : "StaffId";

                var checkCmd = new SqliteCommand(
                    $"SELECT COUNT(*) FROM {table} WHERE {idCol}=@id AND Password=@old", conn);
                checkCmd.Parameters.AddWithValue("@id", userId);
                checkCmd.Parameters.AddWithValue("@old", txtOldPassword.Text);

                if (Convert.ToInt32(checkCmd.ExecuteScalar()) == 0)
                {
                    MessageBox.Show("Current password is incorrect.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var updateCmd = new SqliteCommand(
                    $"UPDATE {table} SET Password=@new WHERE {idCol}=@id", conn);
                updateCmd.Parameters.AddWithValue("@new", txtNewPassword.Text);
                updateCmd.Parameters.AddWithValue("@id", userId);
                updateCmd.ExecuteNonQuery();

                txtOldPassword.Clear();
                txtNewPassword.Clear();

                MessageBox.Show("Password changed successfully! 🔑", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error changing password: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
