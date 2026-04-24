using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace CoffeeShopManagement.Forms
{
    public class CheckoutForm : Form
    {
        private int orderId, userId;
        private RadioButton rbCash, rbCredit, rbDebit, rbMobile;
        private RadioButton rbBkash, rbNagad, rbRocket;
        private Panel mobilePanel;

        private readonly Color primaryColor = Color.FromArgb(101, 67, 33);
        private readonly Color bgColor = Color.FromArgb(250, 243, 232);
        private readonly Color accentColor = Color.FromArgb(255, 183, 77);
        private readonly Color darkBg = Color.FromArgb(62, 39, 18);

        public CheckoutForm(int orderId, int userId)
        {
            this.orderId = orderId; this.userId = userId;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "💳 Checkout & Payment"; this.Size = new Size(520, 580);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog; this.MaximizeBox = false;
            this.BackColor = bgColor; this.Font = new Font("Segoe UI", 10);

            // Header
            Panel header = new Panel { Dock = DockStyle.Top, Height = 65, BackColor = darkBg };
            header.Paint += (s, e) => { using var b = new LinearGradientBrush(header.ClientRectangle, darkBg, primaryColor, 45f); e.Graphics.FillRectangle(b, header.ClientRectangle); };
            Label lblTitle = new Label { Text = "💳 Payment Checkout", Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = accentColor, Size = new Size(500, 45), Location = new Point(10, 10), TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent };
            header.Controls.Add(lblTitle);

            // Order info
            decimal amount = 0; string shopName = "";
            using (var conn = DatabaseHelper.GetConnection())
            {
                var cmd = new SqlCommand("SELECT o.TotalAmount, s.ShopName FROM Orders o JOIN CoffeeShop s ON o.ShopId=s.ShopId WHERE o.OrderId=@oid", conn);
                cmd.Parameters.AddWithValue("@oid", orderId);
                using var r = cmd.ExecuteReader();
                if (r.Read()) { amount = r.GetDecimal(0); shopName = r.GetString(1); }
            }

            Label lblOrder = new Label { Text = $"Order #{orderId}  •  {shopName}", Font = new Font("Segoe UI", 12), ForeColor = primaryColor, Location = new Point(30, 80), AutoSize = true };
            Label lblAmount = new Label { Text = $"Amount Due: ৳{amount:N2}", Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = Color.FromArgb(76, 175, 80), Location = new Point(30, 110), AutoSize = true };

            // Payment method panel
            Panel methodPanel = new Panel { Location = new Point(30, 155), Size = new Size(440, 300), BackColor = Color.White };
            methodPanel.Paint += (s, e) => { using var pen = new Pen(Color.FromArgb(220, 210, 195)); e.Graphics.DrawRectangle(pen, 0, 0, methodPanel.Width - 1, methodPanel.Height - 1); };

            Label lblMethod = new Label { Text = "Select Payment Method:", Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = primaryColor, Location = new Point(15, 12), AutoSize = true };

            rbCash = new RadioButton { Text = "💵  Cash", Font = new Font("Segoe UI", 12), Location = new Point(20, 50), AutoSize = true, Checked = true };
            rbCredit = new RadioButton { Text = "💳  Credit Card", Font = new Font("Segoe UI", 12), Location = new Point(20, 85), AutoSize = true };
            rbDebit = new RadioButton { Text = "💳  Debit Card", Font = new Font("Segoe UI", 12), Location = new Point(20, 120), AutoSize = true };
            rbMobile = new RadioButton { Text = "📱  Mobile Banking", Font = new Font("Segoe UI", 12), Location = new Point(20, 155), AutoSize = true };

            // Mobile Banking sub-options
            mobilePanel = new Panel { Location = new Point(50, 190), Size = new Size(370, 95), BackColor = Color.FromArgb(255, 248, 235), Visible = false };
            mobilePanel.Paint += (s, e) => { using var pen = new Pen(accentColor); e.Graphics.DrawRectangle(pen, 0, 0, mobilePanel.Width - 1, mobilePanel.Height - 1); };

            Label lblProvider = new Label { Text = "Select Provider:", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = primaryColor, Location = new Point(10, 8), AutoSize = true };
            rbBkash = new RadioButton { Text = "bKash", Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(227, 24, 99), Location = new Point(15, 35), AutoSize = true, Checked = true };
            rbNagad = new RadioButton { Text = "Nagad", Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(255, 103, 31), Location = new Point(120, 35), AutoSize = true };
            rbRocket = new RadioButton { Text = "Rocket", Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(139, 69, 186), Location = new Point(225, 35), AutoSize = true };

            Label lblMobileNote = new Label { Text = "📱 Payment will be processed via your mobile wallet", Font = new Font("Segoe UI", 8, FontStyle.Italic), ForeColor = Color.Gray, Location = new Point(10, 68), AutoSize = true };
            mobilePanel.Controls.AddRange(new Control[] { lblProvider, rbBkash, rbNagad, rbRocket, lblMobileNote });

            // Toggle mobile panel
            rbCash.CheckedChanged += (s, e) => mobilePanel.Visible = false;
            rbCredit.CheckedChanged += (s, e) => mobilePanel.Visible = false;
            rbDebit.CheckedChanged += (s, e) => mobilePanel.Visible = false;
            rbMobile.CheckedChanged += (s, e) => mobilePanel.Visible = rbMobile.Checked;

            methodPanel.Controls.AddRange(new Control[] { lblMethod, rbCash, rbCredit, rbDebit, rbMobile, mobilePanel });

            // Pay button
            Button btnPay = new Button
            {
                Text = $"✅ Pay ৳{amount:N2}", Location = new Point(30, 475), Size = new Size(440, 50),
                Font = new Font("Segoe UI", 14, FontStyle.Bold), BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand
            };
            btnPay.FlatAppearance.BorderSize = 0;
            btnPay.Click += (s, e) => ProcessPayment(amount);

            this.Controls.AddRange(new Control[] { header, lblOrder, lblAmount, methodPanel, btnPay });
        }

        private void ProcessPayment(decimal amount)
        {
            string method = rbCash.Checked ? "Cash" : rbCredit.Checked ? "Credit Card" : rbDebit.Checked ? "Debit Card" : "Mobile Banking";
            string? provider = null;
            if (rbMobile.Checked) provider = rbBkash.Checked ? "bKash" : rbNagad.Checked ? "Nagad" : "Rocket";

            try
            {
                using var conn = DatabaseHelper.GetConnection();
                var cmd = new SqlCommand("INSERT INTO Payment (OrderId,Amount,PaymentMethod,PaymentProvider) VALUES (@oid,@amt,@m,@p)", conn);
                cmd.Parameters.AddWithValue("@oid", orderId); cmd.Parameters.AddWithValue("@amt", amount);
                cmd.Parameters.AddWithValue("@m", method);
                cmd.Parameters.AddWithValue("@p", provider != null ? provider : DBNull.Value);
                cmd.ExecuteNonQuery();

                new SqlCommand($"UPDATE Orders SET Status='Paid' WHERE OrderId={orderId}", conn).ExecuteNonQuery();

                string msg = $"Payment of ৳{amount:N2} successful! ✅\nMethod: {method}";
                if (provider != null) msg += $" ({provider})";
                MessageBox.Show(msg, "Payment Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex) { MessageBox.Show($"Payment error: {ex.Message}"); }
        }
    }
}
