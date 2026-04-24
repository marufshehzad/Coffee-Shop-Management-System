using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace CoffeeShopManagement.Forms
{
    public class AccountForm : Form
    {
        private int userId; private string userType;
        private TextBox txtFirstName, txtLastName, txtEmail, txtPhone, txtAddress, txtUsername, txtOldPassword, txtNewPassword;
        private readonly Color primaryColor = Color.FromArgb(101, 67, 33);
        private readonly Color bgColor = Color.FromArgb(250, 243, 232);
        private readonly Color accentColor = Color.FromArgb(255, 183, 77);

        public AccountForm(int userId, string userType) { this.userId = userId; this.userType = userType; InitializeComponent(); LoadUserData(); }

        private void InitializeComponent()
        {
            this.BackColor = bgColor; this.Font = new Font("Segoe UI", 10); this.Size = new Size(760, 600);
            Label lblTitle = new Label { Text = "👤 My Account", Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = primaryColor, Location = new Point(20, 10), AutoSize = true };

            Panel profilePanel = new Panel { Location = new Point(20, 55), Size = new Size(700, 420), BackColor = Color.White };
            profilePanel.Paint += (s, e) => { using var pen = new Pen(Color.FromArgb(220, 210, 195)); e.Graphics.DrawRectangle(pen, 0, 0, profilePanel.Width - 1, profilePanel.Height - 1); };

            Label lp = new Label { Text = "📝 Profile Information", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = primaryColor, Location = new Point(20, 15), AutoSize = true };
            int y = 50;
            txtFirstName = AddField(profilePanel, "First Name:", ref y);
            txtLastName = AddField(profilePanel, "Last Name:", ref y);
            txtEmail = AddField(profilePanel, "Email:", ref y);
            txtPhone = AddField(profilePanel, "Phone:", ref y);
            txtAddress = AddField(profilePanel, "Address:", ref y);
            txtUsername = AddField(profilePanel, "Username:", ref y);
            txtUsername.ReadOnly = true; txtUsername.BackColor = Color.FromArgb(240, 240, 240);

            Button btnUpdate = new Button { Text = "💾 Update Profile", Location = new Point(20, y + 5), Size = new Size(200, 40), Font = new Font("Segoe UI", 11, FontStyle.Bold), BackColor = primaryColor, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnUpdate.FlatAppearance.BorderSize = 0; btnUpdate.Click += BtnUpdate_Click;

            Label lpc = new Label { Text = "🔑 Change Password", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = primaryColor, Location = new Point(380, 50), AutoSize = true };
            Label lo = new Label { Text = "Current Password:", Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(80, 50, 20), Location = new Point(380, 80), AutoSize = true };
            txtOldPassword = new TextBox { Location = new Point(380, 100), Size = new Size(280, 28), UseSystemPasswordChar = true };
            Label ln = new Label { Text = "New Password:", Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(80, 50, 20), Location = new Point(380, 135), AutoSize = true };
            txtNewPassword = new TextBox { Location = new Point(380, 155), Size = new Size(280, 28), UseSystemPasswordChar = true };
            Button btnPass = new Button { Text = "🔑 Change Password", Location = new Point(380, 195), Size = new Size(200, 35), Font = new Font("Segoe UI", 10, FontStyle.Bold), BackColor = accentColor, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnPass.FlatAppearance.BorderSize = 0; btnPass.Click += BtnChangePassword_Click;

            profilePanel.Controls.AddRange(new Control[] { lp, btnUpdate, lpc, lo, txtOldPassword, ln, txtNewPassword, btnPass });
            this.Controls.AddRange(new Control[] { lblTitle, profilePanel });
        }

        private TextBox AddField(Panel p, string label, ref int y)
        {
            p.Controls.Add(new Label { Text = label, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(80, 50, 20), Location = new Point(20, y), AutoSize = true });
            TextBox t = new TextBox { Location = new Point(20, y + 18), Size = new Size(320, 28), Font = new Font("Segoe UI", 10) };
            p.Controls.Add(t); y += 50; return t;
        }

        private void LoadUserData()
        {
            using var conn = DatabaseHelper.GetConnection();
            string q = userType == "Customer"
                ? "SELECT FirstName,LastName,Email,Phone,Address,Username FROM UserDetails WHERE UserId=@id"
                : "SELECT FirstName,LastName,Email,Phone,'',Username FROM Staff WHERE StaffId=@id";
            var cmd = new SqlCommand(q, conn); cmd.Parameters.AddWithValue("@id", userId);
            using var r = cmd.ExecuteReader();
            if (r.Read()) { txtFirstName.Text = r.IsDBNull(0) ? "" : r.GetString(0); txtLastName.Text = r.IsDBNull(1) ? "" : r.GetString(1); txtEmail.Text = r.IsDBNull(2) ? "" : r.GetString(2); txtPhone.Text = r.IsDBNull(3) ? "" : r.GetString(3); txtAddress.Text = r.IsDBNull(4) ? "" : r.GetString(4); txtUsername.Text = r.IsDBNull(5) ? "" : r.GetString(5); }
        }

        private void BtnUpdate_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFirstName.Text) || string.IsNullOrWhiteSpace(txtEmail.Text)) { MessageBox.Show("First Name and Email are required."); return; }
            using var conn = DatabaseHelper.GetConnection();
            string q = userType == "Customer"
                ? "UPDATE UserDetails SET FirstName=@fn,LastName=@ln,Email=@em,Phone=@ph,Address=@ad WHERE UserId=@id"
                : "UPDATE Staff SET FirstName=@fn,LastName=@ln,Email=@em,Phone=@ph WHERE StaffId=@id";
            var cmd = new SqlCommand(q, conn);
            cmd.Parameters.AddWithValue("@fn", txtFirstName.Text.Trim()); cmd.Parameters.AddWithValue("@ln", txtLastName.Text.Trim());
            cmd.Parameters.AddWithValue("@em", txtEmail.Text.Trim()); cmd.Parameters.AddWithValue("@ph", txtPhone.Text.Trim());
            if (userType == "Customer") cmd.Parameters.AddWithValue("@ad", txtAddress.Text.Trim());
            cmd.Parameters.AddWithValue("@id", userId); cmd.ExecuteNonQuery();
            MessageBox.Show("Profile updated! ✅", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnChangePassword_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtOldPassword.Text) || string.IsNullOrWhiteSpace(txtNewPassword.Text)) { MessageBox.Show("Enter both passwords."); return; }
            using var conn = DatabaseHelper.GetConnection();
            string tbl = userType == "Customer" ? "UserDetails" : "Staff";
            string idCol = userType == "Customer" ? "UserId" : "StaffId";
            var chk = new SqlCommand($"SELECT COUNT(*) FROM {tbl} WHERE {idCol}=@id AND Password=@old", conn);
            chk.Parameters.AddWithValue("@id", userId); chk.Parameters.AddWithValue("@old", txtOldPassword.Text);
            if ((int)chk.ExecuteScalar()! == 0) { MessageBox.Show("Current password is incorrect."); return; }
            var upd = new SqlCommand($"UPDATE {tbl} SET Password=@new WHERE {idCol}=@id", conn);
            upd.Parameters.AddWithValue("@new", txtNewPassword.Text); upd.Parameters.AddWithValue("@id", userId); upd.ExecuteNonQuery();
            txtOldPassword.Clear(); txtNewPassword.Clear();
            MessageBox.Show("Password changed! 🔑", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
