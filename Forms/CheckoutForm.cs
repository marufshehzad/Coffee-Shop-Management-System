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
        private decimal originalAmount;
        private RadioButton rbCash, rbCredit, rbDebit, rbMobile;
        private RadioButton rbBkash, rbNagad, rbRocket;
        private Panel mobilePanel;
        private TextBox txtPromo;
        private Label lblDiscount, lblFinal;
        private int discountPct = 0;
        private string? appliedPromo = null;

        private readonly Color primaryColor = Color.FromArgb(101, 67, 33);
        private readonly Color bgColor = Color.FromArgb(250, 243, 232);
        private readonly Color accentColor = Color.FromArgb(255, 183, 77);
        private readonly Color darkBg = Color.FromArgb(62, 39, 18);

        public CheckoutForm(int orderId, int userId) { this.orderId = orderId; this.userId = userId; InitializeComponent(); }

        private void InitializeComponent()
        {
            this.Text = "💳 Checkout & Payment"; this.Size = new Size(560, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog; this.MaximizeBox = false;
            this.BackColor = bgColor; this.Font = new Font("Segoe UI", 10);

            // Header
            Panel header = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = darkBg };
            header.Paint += (s, e) => { using var b = new LinearGradientBrush(header.ClientRectangle, darkBg, primaryColor, 45f); e.Graphics.FillRectangle(b, header.ClientRectangle); };
            header.Controls.Add(new Label { Text = "💳 Payment Checkout", Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = accentColor, Size = new Size(540, 42), Location = new Point(10, 8), TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent });

            // Order info
            string shopName = "";
            using (var conn = DatabaseHelper.GetConnection()) {
                var cmd = new SqlCommand("SELECT o.TotalAmount, s.ShopName FROM Orders o JOIN CoffeeShop s ON o.ShopId=s.ShopId WHERE o.OrderId=@oid", conn);
                cmd.Parameters.AddWithValue("@oid", orderId);
                using var r = cmd.ExecuteReader();
                if (r.Read()) { originalAmount = r.GetDecimal(0); shopName = r.GetString(1); }
            }

            Label lblOrder = new Label { Text = $"Order #{orderId}  •  {shopName}", Font = new Font("Segoe UI", 12), ForeColor = primaryColor, Location = new Point(25, 72), AutoSize = true };
            Label lblAmount = new Label { Text = $"Subtotal: ৳{originalAmount:N2}", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = primaryColor, Location = new Point(25, 98), AutoSize = true };

            // ═══ PROMO CODE SECTION ═══
            Panel promoPanel = new Panel { Location = new Point(25, 135), Size = new Size(490, 75), BackColor = Color.White };
            promoPanel.Paint += (s, e) => { using var pen = new Pen(Color.FromArgb(220, 210, 195)); e.Graphics.DrawRectangle(pen, 0, 0, promoPanel.Width - 1, promoPanel.Height - 1); };
            Label lblPromo = new Label { Text = "🏷️ Promo Code:", Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = primaryColor, Location = new Point(12, 8), AutoSize = true };
            txtPromo = new TextBox { Location = new Point(12, 35), Size = new Size(280, 28), Font = new Font("Segoe UI", 11), PlaceholderText = "Enter promo code (e.g. AIUB20)" };
            Button btnApply = new Button { Text = "Apply", Location = new Point(300, 32), Size = new Size(90, 32), Font = new Font("Segoe UI", 10, FontStyle.Bold), BackColor = Color.FromArgb(0, 150, 136), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnApply.FlatAppearance.BorderSize = 0;
            btnApply.Click += ApplyPromo;
            lblDiscount = new Label { Text = "", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(76, 175, 80), Location = new Point(398, 38), AutoSize = true };
            promoPanel.Controls.AddRange(new Control[] { lblPromo, txtPromo, btnApply, lblDiscount });

            lblFinal = new Label { Text = $"💰 Total: ৳{originalAmount:N2}", Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = Color.FromArgb(76, 175, 80), Location = new Point(25, 218), AutoSize = true };

            // ═══ PAYMENT METHOD ═══
            Panel methodPanel = new Panel { Location = new Point(25, 260), Size = new Size(490, 290), BackColor = Color.White };
            methodPanel.Paint += (s, e) => { using var pen = new Pen(Color.FromArgb(220, 210, 195)); e.Graphics.DrawRectangle(pen, 0, 0, methodPanel.Width - 1, methodPanel.Height - 1); };

            Label lblMethod = new Label { Text = "Select Payment Method:", Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = primaryColor, Location = new Point(15, 12), AutoSize = true };
            rbCash = new RadioButton { Text = "💵  Cash", Font = new Font("Segoe UI", 12), Location = new Point(20, 48), AutoSize = true, Checked = true };
            rbCredit = new RadioButton { Text = "💳  Credit Card", Font = new Font("Segoe UI", 12), Location = new Point(20, 80), AutoSize = true };
            rbDebit = new RadioButton { Text = "💳  Debit Card", Font = new Font("Segoe UI", 12), Location = new Point(20, 112), AutoSize = true };
            rbMobile = new RadioButton { Text = "📱  Mobile Banking", Font = new Font("Segoe UI", 12), Location = new Point(20, 144), AutoSize = true };

            mobilePanel = new Panel { Location = new Point(50, 178), Size = new Size(420, 95), BackColor = Color.FromArgb(255, 248, 235), Visible = false };
            mobilePanel.Paint += (s, e) => { using var pen = new Pen(accentColor); e.Graphics.DrawRectangle(pen, 0, 0, mobilePanel.Width - 1, mobilePanel.Height - 1); };
            Label lblProvider = new Label { Text = "Select Provider:", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = primaryColor, Location = new Point(10, 8), AutoSize = true };
            rbBkash = new RadioButton { Text = "bKash", Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(227, 24, 99), Location = new Point(15, 35), AutoSize = true, Checked = true };
            rbNagad = new RadioButton { Text = "Nagad", Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(255, 103, 31), Location = new Point(130, 35), AutoSize = true };
            rbRocket = new RadioButton { Text = "Rocket", Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(139, 69, 186), Location = new Point(250, 35), AutoSize = true };
            mobilePanel.Controls.AddRange(new Control[] { lblProvider, rbBkash, rbNagad, rbRocket,
                new Label { Text = "📱 Payment via mobile wallet", Font = new Font("Segoe UI", 8, FontStyle.Italic), ForeColor = Color.Gray, Location = new Point(10, 68), AutoSize = true } });

            rbCash.CheckedChanged += (s, e) => mobilePanel.Visible = false;
            rbCredit.CheckedChanged += (s, e) => mobilePanel.Visible = false;
            rbDebit.CheckedChanged += (s, e) => mobilePanel.Visible = false;
            rbMobile.CheckedChanged += (s, e) => mobilePanel.Visible = rbMobile.Checked;

            methodPanel.Controls.AddRange(new Control[] { lblMethod, rbCash, rbCredit, rbDebit, rbMobile, mobilePanel });

            // Pay button
            decimal final_ = originalAmount;
            Button btnPay = new Button
            {
                Text = $"✅ Pay ৳{final_:N2}", Location = new Point(25, 565), Size = new Size(490, 55),
                Font = new Font("Segoe UI", 14, FontStyle.Bold), BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand
            };
            btnPay.FlatAppearance.BorderSize = 0;
            btnPay.Click += (s, e) => ProcessPayment();

            this.Controls.AddRange(new Control[] { header, lblOrder, lblAmount, promoPanel, lblFinal, methodPanel, btnPay });
        }

        private void ApplyPromo(object? sender, EventArgs e)
        {
            string code = txtPromo.Text.Trim().ToUpper();
            if (string.IsNullOrEmpty(code)) { MessageBox.Show("Enter a promo code."); return; }

            using var conn = DatabaseHelper.GetConnection();
            var cmd = new SqlCommand("SELECT DiscountPct FROM PromoCode WHERE Code=@c AND IsActive=1 AND (ExpiryDate IS NULL OR ExpiryDate>GETDATE())", conn);
            cmd.Parameters.AddWithValue("@c", code);
            var result = cmd.ExecuteScalar();

            if (result != null)
            {
                discountPct = (int)result;
                appliedPromo = code;
                decimal disc = originalAmount * discountPct / 100;
                decimal final_ = originalAmount - disc;
                lblDiscount.Text = $"✅ -{discountPct}%";
                lblFinal.Text = $"💰 Total: ৳{final_:N2}  (saved ৳{disc:N2})";
                lblFinal.ForeColor = Color.FromArgb(0, 150, 0);
                // Update pay button
                foreach (Control c in this.Controls) if (c is Button b && b.Text.StartsWith("✅ Pay")) b.Text = $"✅ Pay ৳{final_:N2}";
                MessageBox.Show($"Promo code '{code}' applied! {discountPct}% off 🎉", "Discount Applied", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                discountPct = 0; appliedPromo = null;
                lblDiscount.Text = "❌ Invalid";
                lblDiscount.ForeColor = Color.Red;
                MessageBox.Show("Invalid or expired promo code.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ProcessPayment()
        {
            string method = rbCash.Checked ? "Cash" : rbCredit.Checked ? "Credit Card" : rbDebit.Checked ? "Debit Card" : "Mobile Banking";
            string? provider = rbMobile.Checked ? (rbBkash.Checked ? "bKash" : rbNagad.Checked ? "Nagad" : "Rocket") : null;

            decimal discAmt = originalAmount * discountPct / 100;
            decimal finalAmt = originalAmount - discAmt;

            try
            {
                using var conn = DatabaseHelper.GetConnection();
                var cmd = new SqlCommand("INSERT INTO Payment (OrderId,Amount,DiscountAmount,FinalAmount,PaymentMethod,PaymentProvider,PromoCode) VALUES (@oid,@amt,@disc,@final,@m,@p,@promo)", conn);
                cmd.Parameters.AddWithValue("@oid", orderId);
                cmd.Parameters.AddWithValue("@amt", originalAmount);
                cmd.Parameters.AddWithValue("@disc", discAmt);
                cmd.Parameters.AddWithValue("@final", finalAmt);
                cmd.Parameters.AddWithValue("@m", method);
                cmd.Parameters.AddWithValue("@p", provider != null ? provider : DBNull.Value);
                cmd.Parameters.AddWithValue("@promo", appliedPromo != null ? appliedPromo : DBNull.Value);
                cmd.ExecuteNonQuery();

                // Update order status and discount info
                var upd = new SqlCommand("UPDATE Orders SET Status='Paid', DiscountAmount=@d, PromoCode=@pc WHERE OrderId=@oid", conn);
                upd.Parameters.AddWithValue("@d", discAmt);
                upd.Parameters.AddWithValue("@pc", appliedPromo != null ? appliedPromo : DBNull.Value);
                upd.Parameters.AddWithValue("@oid", orderId);
                upd.ExecuteNonQuery();

                this.Close();

                // Show receipt
                ShowReceipt(method, provider, discAmt, finalAmt);
            }
            catch (Exception ex) { MessageBox.Show($"Payment error: {ex.Message}"); }
        }

        // ═══ RECEIPT / INVOICE ═══
        private void ShowReceipt(string method, string? provider, decimal discount, decimal finalAmt)
        {
            Form receipt = new Form
            {
                Text = "🧾 Payment Receipt", Size = new Size(500, 620),
                StartPosition = FormStartPosition.CenterScreen,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false, BackColor = Color.White,
                Font = new Font("Segoe UI", 10)
            };

            // Load order details
            string shopName = "", customerName = "", orderDate = "";
            var items = new System.Collections.Generic.List<(string name, int qty, decimal price, decimal sub)>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                var cmd = new SqlCommand("SELECT s.ShopName, u.FirstName+' '+u.LastName, o.OrderDate FROM Orders o JOIN CoffeeShop s ON o.ShopId=s.ShopId JOIN UserDetails u ON o.UserId=u.UserId WHERE o.OrderId=@oid", conn);
                cmd.Parameters.AddWithValue("@oid", orderId);
                using var r = cmd.ExecuteReader();
                if (r.Read()) { shopName = r.GetString(0); customerName = r.GetString(1); orderDate = r.GetDateTime(2).ToString("dd MMM yyyy, hh:mm tt"); }

                var ic = new SqlCommand("SELECT p.ProductName, od.Quantity, od.UnitPrice, od.Subtotal FROM OrderDetails od JOIN Product p ON od.ProductId=p.ProductId WHERE od.OrderId=@oid", conn);
                ic.Parameters.AddWithValue("@oid", orderId);
                using var ir = ic.ExecuteReader();
                while (ir.Read()) items.Add((ir.GetString(0), ir.GetInt32(1), ir.GetDecimal(2), ir.GetDecimal(3)));
            }

            Panel receiptPanel = new Panel { Location = new Point(20, 15), Size = new Size(440, 500), BackColor = Color.White };
            receiptPanel.Paint += (s, e) =>
            {
                var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
                g.DrawRectangle(new Pen(Color.FromArgb(200, 190, 170), 2), 0, 0, receiptPanel.Width - 1, receiptPanel.Height - 1);

                int y = 15;
                // Header
                g.DrawString("☕ " + shopName, new Font("Segoe UI", 18, FontStyle.Bold), new SolidBrush(primaryColor), 20, y); y += 35;
                g.DrawString("─────────────────────────────────", new Font("Segoe UI", 8), Brushes.LightGray, 20, y); y += 18;
                g.DrawString($"Receipt #INV-{orderId:D4}", new Font("Segoe UI", 11, FontStyle.Bold), Brushes.Black, 20, y); y += 22;
                g.DrawString($"Date: {orderDate}", new Font("Segoe UI", 9), Brushes.Gray, 20, y); y += 18;
                g.DrawString($"Customer: {customerName}", new Font("Segoe UI", 9), Brushes.Gray, 20, y); y += 18;
                g.DrawString($"Payment: {method}{(provider != null ? $" ({provider})" : "")}", new Font("Segoe UI", 9), Brushes.Gray, 20, y); y += 25;

                // Items header
                g.DrawString("─────────────────────────────────", new Font("Segoe UI", 8), Brushes.LightGray, 20, y); y += 15;
                g.DrawString("Item", new Font("Segoe UI", 10, FontStyle.Bold), Brushes.Black, 20, y);
                g.DrawString("Qty", new Font("Segoe UI", 10, FontStyle.Bold), Brushes.Black, 250, y);
                g.DrawString("Price", new Font("Segoe UI", 10, FontStyle.Bold), Brushes.Black, 300, y);
                g.DrawString("Total", new Font("Segoe UI", 10, FontStyle.Bold), Brushes.Black, 370, y); y += 22;

                foreach (var item in items)
                {
                    g.DrawString(item.name, new Font("Segoe UI", 9), Brushes.Black, 20, y);
                    g.DrawString($"x{item.qty}", new Font("Segoe UI", 9), Brushes.Gray, 255, y);
                    g.DrawString($"৳{item.price:N0}", new Font("Segoe UI", 9), Brushes.Gray, 300, y);
                    g.DrawString($"৳{item.sub:N0}", new Font("Segoe UI", 9, FontStyle.Bold), Brushes.Black, 370, y);
                    y += 20;
                }

                y += 8;
                g.DrawString("─────────────────────────────────", new Font("Segoe UI", 8), Brushes.LightGray, 20, y); y += 18;

                // Totals
                g.DrawString($"Subtotal:", new Font("Segoe UI", 10), Brushes.Black, 20, y);
                g.DrawString($"৳{originalAmount:N2}", new Font("Segoe UI", 10), Brushes.Black, 350, y); y += 22;

                if (discount > 0)
                {
                    g.DrawString($"Discount ({appliedPromo}):", new Font("Segoe UI", 10), new SolidBrush(Color.FromArgb(244, 67, 54)), 20, y);
                    g.DrawString($"-৳{discount:N2}", new Font("Segoe UI", 10, FontStyle.Bold), new SolidBrush(Color.FromArgb(244, 67, 54)), 350, y); y += 22;
                }

                g.DrawString("═══════════════════════", new Font("Segoe UI", 8), new SolidBrush(primaryColor), 20, y); y += 15;
                g.DrawString("TOTAL PAID:", new Font("Segoe UI", 14, FontStyle.Bold), new SolidBrush(primaryColor), 20, y);
                g.DrawString($"৳{finalAmt:N2}", new Font("Segoe UI", 14, FontStyle.Bold), new SolidBrush(Color.FromArgb(76, 175, 80)), 310, y); y += 35;

                g.DrawString("✅ Payment Successful!", new Font("Segoe UI", 11, FontStyle.Bold | FontStyle.Italic), new SolidBrush(Color.FromArgb(76, 175, 80)), 120, y); y += 25;
                g.DrawString("Thank you for your order! ☕", new Font("Segoe UI", 9, FontStyle.Italic), Brushes.Gray, 140, y);
            };

            Button btnClose = new Button { Text = "✅ Close", Location = new Point(20, 525), Size = new Size(210, 42), Font = new Font("Segoe UI", 12, FontStyle.Bold), BackColor = primaryColor, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => receipt.Close();

            Button btnPrint = new Button { Text = "🖨️ Print Receipt", Location = new Point(250, 525), Size = new Size(210, 42), Font = new Font("Segoe UI", 12, FontStyle.Bold), BackColor = Color.FromArgb(33, 150, 243), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnPrint.FlatAppearance.BorderSize = 0;
            btnPrint.Click += (s, e) => MessageBox.Show("Receipt sent to printer! 🖨️", "Print", MessageBoxButtons.OK, MessageBoxIcon.Information);

            receipt.Controls.AddRange(new Control[] { receiptPanel, btnClose, btnPrint });
            receipt.ShowDialog();
        }
    }
}
