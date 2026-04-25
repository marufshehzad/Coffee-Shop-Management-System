using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

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
        private readonly Color bgColor = Color.FromArgb(250, 243, 232);
        private readonly Color darkBg = Color.FromArgb(62, 39, 18);
        private readonly Color cardBg = Color.White;

        public CustomerDashboard(int userId, string name)
        {
            this.userId = userId;
            this.customerName = name;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = $"☕ Coffee Marketplace - {customerName}";
            this.Size = new Size(1100, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = bgColor;
            this.Font = new Font("Segoe UI", 10);
            this.MinimumSize = new Size(900, 600);

            // Sidebar
            Panel sidebar = new Panel { Dock = DockStyle.Left, Width = 230, BackColor = darkBg };
            sidebar.Paint += (s, e) => {
                using var b = new LinearGradientBrush(sidebar.ClientRectangle, darkBg, Color.FromArgb(78, 52, 28), 90f);
                e.Graphics.FillRectangle(b, sidebar.ClientRectangle);
            };

            Label lblLogo = new Label { Text = "☕", Font = new Font("Segoe UI", 40), ForeColor = accentColor, Size = new Size(230, 65), Location = new Point(0, 12), TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent };
            Label lblBrand = new Label { Text = "COFFEE\nMARKETPLACE", Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = accentColor, Size = new Size(210, 45), Location = new Point(10, 78), TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent };
            Label lblWelcome = new Label { Text = $"Hi, {customerName}!", Font = new Font("Segoe UI", 10), ForeColor = Color.FromArgb(200, 180, 150), Size = new Size(210, 22), Location = new Point(10, 130), TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent };
            Panel divider = new Panel { BackColor = accentColor, Size = new Size(190, 2), Location = new Point(20, 160) };
            sidebar.Controls.AddRange(new Control[] { lblLogo, lblBrand, lblWelcome, divider });

            string[] menuItems = { "🏪 Browse Shops", "🛒 My Orders", "⭐ My Reviews", "📝 Complaints", "👤 My Account", "🚪 Logout" };
            EventHandler[] handlers = { ShowShops, ShowOrders, ShowReviews, ShowComplaints, ShowAccount, DoLogout };
            for (int i = 0; i < menuItems.Length; i++)
            {
                Button btn = new Button
                {
                    Text = menuItems[i], Size = new Size(210, 44), Location = new Point(10, 175 + (i * 52)),
                    Font = new Font("Segoe UI", 11), ForeColor = Color.White, BackColor = Color.Transparent,
                    FlatStyle = FlatStyle.Flat, TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(15, 0, 0, 0), Cursor = Cursors.Hand
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(50, 255, 255, 255);
                btn.Click += handlers[i];
                sidebar.Controls.Add(btn);
            }

            contentPanel = new Panel { Dock = DockStyle.Fill, BackColor = bgColor, Padding = new Padding(25), AutoScroll = true };
            this.Controls.Add(contentPanel);
            this.Controls.Add(sidebar);
            ShowShops(null, EventArgs.Empty);
        }

        private void ClearContent() { contentPanel.Controls.Clear(); contentPanel.AutoScrollPosition = new Point(0, 0); }

        // ===== BROWSE SHOPS (FoodPanda-style grid) =====
        private void ShowShops(object? sender, EventArgs e)
        {
            ClearContent();

            // Header panel docked to top
            Panel headerPanel = new Panel { Dock = DockStyle.Top, Height = 65, BackColor = Color.Transparent };
            Label title = new Label { Text = "🏪 Explore Coffee Shops", Font = new Font("Segoe UI", 22, FontStyle.Bold), ForeColor = primaryColor, AutoSize = true, Location = new Point(10, 5) };
            Label subtitle = new Label { Text = "Choose a shop to browse their menu and place an order", Font = new Font("Segoe UI", 10), ForeColor = Color.Gray, AutoSize = true, Location = new Point(12, 42) };
            headerPanel.Controls.AddRange(new Control[] { title, subtitle });

            // Shop grid docked to fill remaining space
            FlowLayoutPanel shopGrid = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true,
                BackColor = Color.Transparent,
                Padding = new Padding(5)
            };

            using var conn = DatabaseHelper.GetConnection();
            var cmd = new SqlCommand(@"SELECT s.ShopId, s.ShopName, s.Location, s.Description, s.AverageRating,
                (SELECT COUNT(*) FROM Product WHERE ShopId=s.ShopId) AS ProductCount
                FROM CoffeeShop s WHERE s.IsActive=1 ORDER BY s.AverageRating DESC", conn);
            using var reader = cmd.ExecuteReader();

            string[] shopEmojis = { "🏠", "☕", "🍵", "🫘", "🌅" };
            int idx = 0;
            while (reader.Read())
            {
                int shopId = reader.GetInt32(0);
                string shopName = reader.GetString(1);
                string location = reader.GetString(2);
                string desc = reader.IsDBNull(3) ? "" : reader.GetString(3);
                decimal rating = reader.GetDecimal(4);
                int prodCount = reader.GetInt32(5);
                string emoji = shopEmojis[idx % shopEmojis.Length];

                Panel card = new Panel { Size = new Size(380, 190), Margin = new Padding(10), BackColor = cardBg, Cursor = Cursors.Hand };
                card.Paint += (s2, pe) => {
                    pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    using var pen = new Pen(Color.FromArgb(230, 220, 200), 1);
                    pe.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                };

                // Color strip on top
                Panel strip = new Panel { Size = new Size(380, 5), Location = new Point(0, 0), BackColor = accentColor };
                Label lblEmoji = new Label { Text = emoji, Font = new Font("Segoe UI", 32), Location = new Point(15, 15), Size = new Size(60, 55) };
                Label lblName = new Label { Text = shopName, Font = new Font("Segoe UI", 15, FontStyle.Bold), ForeColor = primaryColor, Location = new Point(80, 15), AutoSize = true };
                Label lblLoc = new Label { Text = $"📍 {location}", Font = new Font("Segoe UI", 9), ForeColor = Color.Gray, Location = new Point(80, 42), AutoSize = true };
                Label lblDesc = new Label { Text = desc, Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(120, 100, 80), Location = new Point(15, 75), Size = new Size(350, 35) };

                string stars = new string('★', (int)Math.Round((double)rating)) + new string('☆', 5 - (int)Math.Round((double)rating));
                Label lblRating = new Label { Text = $"{stars}  {rating:F1}", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.FromArgb(255, 152, 0), Location = new Point(15, 115), AutoSize = true };
                Label lblCount = new Label { Text = $"📦 {prodCount} items available", Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(100, 80, 60), Location = new Point(15, 145), AutoSize = true };

                Button btnVisit = new Button
                {
                    Text = "View Menu →", Location = new Point(240, 140), Size = new Size(125, 35),
                    Font = new Font("Segoe UI", 10, FontStyle.Bold), BackColor = primaryColor,
                    ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand
                };
                btnVisit.FlatAppearance.BorderSize = 0;
                int sid = shopId; string sname = shopName;
                btnVisit.Click += (s2, ev) => OpenShopMenu(sid, sname);
                card.Click += (s2, ev) => OpenShopMenu(sid, sname);

                card.Controls.AddRange(new Control[] { strip, lblEmoji, lblName, lblLoc, lblDesc, lblRating, lblCount, btnVisit });
                shopGrid.Controls.Add(card);
                idx++;
            }

            // Add Fill first, then Top (Dock ordering rule)
            contentPanel.Controls.Add(shopGrid);
            contentPanel.Controls.Add(headerPanel);
        }

        private void OpenShopMenu(int shopId, string shopName)
        {
            ClearContent();
            var menuForm = new ShopMenuForm(userId, shopId, shopName);
            menuForm.TopLevel = false;
            menuForm.FormBorderStyle = FormBorderStyle.None;
            menuForm.Dock = DockStyle.Fill;
            menuForm.BackButton += (s, e) => ShowShops(null, EventArgs.Empty);
            contentPanel.Controls.Add(menuForm);
            menuForm.Show();
        }

        // ===== MY ORDERS =====
        private void ShowOrders(object? sender, EventArgs e)
        {
            ClearContent();
            contentPanel.Controls.Add(new Label { Text = "🛒 My Orders", Font = new Font("Segoe UI", 22, FontStyle.Bold), ForeColor = primaryColor, AutoSize = true, Location = new Point(10, 5) });

            DataGridView dgv = CreateDGV(new Point(10, 50), new Size(contentPanel.Width - 70, 320));
            dgv.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            using (var conn = DatabaseHelper.GetConnection())
            {
                var cmd = new SqlCommand(@"SELECT o.OrderId AS [Order #], s.ShopName AS [Shop], o.OrderDate AS [Date],
                    o.TotalAmount AS [Total ৳], o.Status,
                    (SELECT STRING_AGG(p.ProductName + ' x' + CAST(od.Quantity AS VARCHAR), ', ')
                     FROM OrderDetails od JOIN Product p ON od.ProductId=p.ProductId WHERE od.OrderId=o.OrderId) AS [Items]
                    FROM Orders o JOIN CoffeeShop s ON o.ShopId=s.ShopId
                    WHERE o.UserId=@uid ORDER BY o.OrderId DESC", conn);
                cmd.Parameters.AddWithValue("@uid", userId);
                var dt = new DataTable(); using var r = cmd.ExecuteReader(); dt.Load(r);
                dgv.DataSource = dt;
            }
            contentPanel.Controls.Add(dgv);

            // Pay pending orders
            Label lblPay = new Label { Text = "💳 Pay for Pending Orders", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = primaryColor, Location = new Point(10, 385), AutoSize = true };
            ComboBox cmbOrders = new ComboBox { Location = new Point(10, 420), Size = new Size(400, 30), Font = new Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList };

            using (var conn = DatabaseHelper.GetConnection())
            {
                var cmd = new SqlCommand(@"SELECT o.OrderId, o.TotalAmount, s.ShopName FROM Orders o
                    JOIN CoffeeShop s ON o.ShopId=s.ShopId
                    LEFT JOIN Payment p ON o.OrderId=p.OrderId
                    WHERE o.UserId=@uid AND p.PaymentId IS NULL AND o.Status <> 'Cancelled' ORDER BY o.OrderId DESC", conn);
                cmd.Parameters.AddWithValue("@uid", userId);
                using var r = cmd.ExecuteReader();
                while (r.Read()) cmbOrders.Items.Add($"Order #{r.GetInt32(0)} - {r.GetString(2)} - ৳{r.GetDecimal(1):N2}");
            }
            if (cmbOrders.Items.Count > 0) cmbOrders.SelectedIndex = 0;

            Button btnPay = new Button
            {
                Text = "💳 Proceed to Payment", Location = new Point(430, 417), Size = new Size(200, 36),
                Font = new Font("Segoe UI", 10, FontStyle.Bold), BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand
            };
            btnPay.FlatAppearance.BorderSize = 0;
            btnPay.Click += (s, ev) =>
            {
                if (cmbOrders.SelectedItem == null) { MessageBox.Show("No pending orders."); return; }
                int orderId = int.Parse(cmbOrders.SelectedItem.ToString()!.Split('#')[1].Split('-')[0].Trim());
                var payForm = new CheckoutForm(orderId, userId);
                payForm.ShowDialog();
                ShowOrders(null, EventArgs.Empty);
            };

            contentPanel.Controls.AddRange(new Control[] { lblPay, cmbOrders, btnPay });
        }

        // ===== MY REVIEWS =====
        private void ShowReviews(object? sender, EventArgs e)
        {
            ClearContent();
            contentPanel.Controls.Add(new Label { Text = "⭐ My Reviews", Font = new Font("Segoe UI", 22, FontStyle.Bold), ForeColor = primaryColor, AutoSize = true, Location = new Point(10, 5) });

            DataGridView dgv = CreateDGV(new Point(10, 50), new Size(contentPanel.Width - 70, 280));
            dgv.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            using (var conn = DatabaseHelper.GetConnection())
            {
                var cmd = new SqlCommand(@"SELECT r.ReviewId AS [#], p.ProductName AS [Item], s.ShopName AS [Shop],
                    r.Rating AS [Stars], r.ReviewText AS [Review], r.ReviewDate AS [Date]
                    FROM ItemReview r JOIN Product p ON r.ProductId=p.ProductId JOIN CoffeeShop s ON p.ShopId=s.ShopId
                    WHERE r.UserId=@uid ORDER BY r.ReviewDate DESC", conn);
                cmd.Parameters.AddWithValue("@uid", userId);
                var dt = new DataTable(); using var rd = cmd.ExecuteReader(); dt.Load(rd); dgv.DataSource = dt;
            }
            contentPanel.Controls.Add(dgv);

            // Write review section
            Label lblWrite = new Label { Text = "✍️ Write a Review (select a completed order item)", Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = primaryColor, Location = new Point(10, 345), AutoSize = true };
            ComboBox cmbItems = new ComboBox { Location = new Point(10, 380), Size = new Size(500, 30), Font = new Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList };
            using (var conn = DatabaseHelper.GetConnection())
            {
                var cmd = new SqlCommand(@"SELECT od.OrderDetailId, p.ProductId, p.ProductName, o.OrderId
                    FROM OrderDetails od JOIN Product p ON od.ProductId=p.ProductId
                    JOIN Orders o ON od.OrderId=o.OrderId
                    WHERE o.UserId=@uid AND o.Status IN ('Completed','Paid')
                    AND NOT EXISTS (SELECT 1 FROM ItemReview ir WHERE ir.UserId=@uid AND ir.ProductId=p.ProductId AND ir.OrderId=o.OrderId)", conn);
                cmd.Parameters.AddWithValue("@uid", userId);
                using var r = cmd.ExecuteReader();
                while (r.Read()) cmbItems.Items.Add($"{r.GetInt32(2)}|{r.GetInt32(1)}|Order#{r.GetInt32(3)}|{r.GetString(2)}");
            }
            if (cmbItems.Items.Count > 0) cmbItems.SelectedIndex = 0;

            ComboBox cmbRating = new ComboBox { Location = new Point(520, 380), Size = new Size(80, 30), Font = new Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbRating.Items.AddRange(new object[] { "⭐ 1", "⭐ 2", "⭐ 3", "⭐ 4", "⭐ 5" }); cmbRating.SelectedIndex = 4;

            TextBox txtReview = new TextBox { Location = new Point(10, 418), Size = new Size(590, 70), Font = new Font("Segoe UI", 10), Multiline = true, PlaceholderText = "Write your review here..." };

            Button btnSubmit = new Button { Text = "⭐ Submit Review", Location = new Point(10, 496), Size = new Size(200, 38), Font = new Font("Segoe UI", 10, FontStyle.Bold), BackColor = accentColor, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnSubmit.FlatAppearance.BorderSize = 0;
            btnSubmit.Click += (s2, ev) =>
            {
                if (cmbItems.SelectedItem == null) { MessageBox.Show("No items to review."); return; }
                string[] parts = cmbItems.SelectedItem.ToString()!.Split('|');
                int productId = int.Parse(parts[1]);
                int orderId = int.Parse(parts[2].Replace("Order#", ""));
                int rating = cmbRating.SelectedIndex + 1;

                using var conn = DatabaseHelper.GetConnection();
                var cmd = new SqlCommand("INSERT INTO ItemReview (UserId,ProductId,OrderId,Rating,ReviewText) VALUES (@uid,@pid,@oid,@r,@t)", conn);
                cmd.Parameters.AddWithValue("@uid", userId); cmd.Parameters.AddWithValue("@pid", productId);
                cmd.Parameters.AddWithValue("@oid", orderId); cmd.Parameters.AddWithValue("@r", rating);
                cmd.Parameters.AddWithValue("@t", txtReview.Text.Trim());
                cmd.ExecuteNonQuery();

                // Update shop average rating
                var updateCmd = new SqlCommand(@"UPDATE CoffeeShop SET AverageRating = (
                    SELECT AVG(CAST(ir.Rating AS DECIMAL(3,2))) FROM ItemReview ir
                    JOIN Product p ON ir.ProductId=p.ProductId WHERE p.ShopId=CoffeeShop.ShopId)
                    WHERE ShopId IN (SELECT ShopId FROM Product WHERE ProductId=@pid)", conn);
                updateCmd.Parameters.AddWithValue("@pid", productId);
                updateCmd.ExecuteNonQuery();

                MessageBox.Show("Review submitted! ⭐", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ShowReviews(null, EventArgs.Empty);
            };
            contentPanel.Controls.AddRange(new Control[] { lblWrite, cmbItems, cmbRating, txtReview, btnSubmit });
        }

        // ===== COMPLAINTS =====
        private void ShowComplaints(object? sender, EventArgs e)
        {
            ClearContent();
            contentPanel.Controls.Add(new Label { Text = "📝 My Complaints", Font = new Font("Segoe UI", 22, FontStyle.Bold), ForeColor = primaryColor, AutoSize = true, Location = new Point(10, 5) });

            DataGridView dgv = CreateDGV(new Point(10, 50), new Size(contentPanel.Width - 70, 230));
            dgv.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            using (var conn = DatabaseHelper.GetConnection())
            {
                var cmd = new SqlCommand(@"SELECT c.ComplaintId AS [#], s.ShopName AS [Shop], c.Subject,
                    c.Description, c.Status, c.CreatedDate AS [Date], c.ResolvedDate AS [Resolved]
                    FROM Complaint c JOIN CoffeeShop s ON c.ShopId=s.ShopId WHERE c.UserId=@uid ORDER BY c.CreatedDate DESC", conn);
                cmd.Parameters.AddWithValue("@uid", userId);
                var dt = new DataTable(); using var r = cmd.ExecuteReader(); dt.Load(r); dgv.DataSource = dt;
            }
            contentPanel.Controls.Add(dgv);

            // Submit complaint form
            Label lblNew = new Label { Text = "📝 Submit a New Complaint", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = primaryColor, Location = new Point(10, 295), AutoSize = true };
            ComboBox cmbShop = new ComboBox { Location = new Point(10, 330), Size = new Size(300, 30), Font = new Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList };
            using (var conn = DatabaseHelper.GetConnection())
            {
                using var r = new SqlCommand("SELECT ShopId, ShopName FROM CoffeeShop WHERE IsActive=1", conn).ExecuteReader();
                while (r.Read()) cmbShop.Items.Add($"{r.GetInt32(0)}|{r.GetString(1)}");
            }
            if (cmbShop.Items.Count > 0) cmbShop.SelectedIndex = 0;

            ComboBox cmbOrder = new ComboBox { Location = new Point(320, 330), Size = new Size(200, 30), Font = new Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbOrder.Items.Add("No specific order");
            cmbShop.SelectedIndexChanged += (s2, ev) =>
            {
                cmbOrder.Items.Clear(); cmbOrder.Items.Add("No specific order");
                if (cmbShop.SelectedItem == null) return;
                int sid = int.Parse(cmbShop.SelectedItem.ToString()!.Split('|')[0]);
                using var conn = DatabaseHelper.GetConnection();
                using var r = new SqlCommand($"SELECT OrderId FROM Orders WHERE UserId={userId} AND ShopId={sid} ORDER BY OrderId DESC", conn).ExecuteReader();
                while (r.Read()) cmbOrder.Items.Add($"Order #{r.GetInt32(0)}");
                cmbOrder.SelectedIndex = 0;
            };
            cmbOrder.SelectedIndex = 0;

            TextBox txtSubject = new TextBox { Location = new Point(10, 370), Size = new Size(510, 28), Font = new Font("Segoe UI", 10), PlaceholderText = "Subject..." };
            TextBox txtDesc = new TextBox { Location = new Point(10, 405), Size = new Size(510, 70), Font = new Font("Segoe UI", 10), Multiline = true, PlaceholderText = "Describe your complaint..." };

            Button btnSubmit = new Button { Text = "📝 Submit Complaint", Location = new Point(10, 483), Size = new Size(220, 38), Font = new Font("Segoe UI", 10, FontStyle.Bold), BackColor = Color.FromArgb(244, 67, 54), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnSubmit.FlatAppearance.BorderSize = 0;
            btnSubmit.Click += (s2, ev) =>
            {
                if (string.IsNullOrWhiteSpace(txtSubject.Text) || string.IsNullOrWhiteSpace(txtDesc.Text))
                { MessageBox.Show("Subject and description are required."); return; }
                int shopId = int.Parse(cmbShop.SelectedItem!.ToString()!.Split('|')[0]);
                int? orderId = null;
                if (cmbOrder.SelectedIndex > 0) orderId = int.Parse(cmbOrder.SelectedItem!.ToString()!.Replace("Order #", ""));

                using var conn = DatabaseHelper.GetConnection();
                var cmd = new SqlCommand("INSERT INTO Complaint (UserId,ShopId,OrderId,Subject,Description) VALUES (@uid,@sid,@oid,@sub,@desc)", conn);
                cmd.Parameters.AddWithValue("@uid", userId); cmd.Parameters.AddWithValue("@sid", shopId);
                cmd.Parameters.AddWithValue("@oid", orderId.HasValue ? orderId.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@sub", txtSubject.Text.Trim()); cmd.Parameters.AddWithValue("@desc", txtDesc.Text.Trim());
                cmd.ExecuteNonQuery();
                MessageBox.Show("Complaint submitted! 📝", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ShowComplaints(null, EventArgs.Empty);
            };
            contentPanel.Controls.AddRange(new Control[] { lblNew, cmbShop, cmbOrder, txtSubject, txtDesc, btnSubmit });
        }

        // ===== MY ACCOUNT =====
        private void ShowAccount(object? sender, EventArgs e)
        {
            ClearContent();
            var form = new AccountForm(userId, "Customer");
            form.TopLevel = false; form.FormBorderStyle = FormBorderStyle.None; form.Dock = DockStyle.Fill;
            contentPanel.Controls.Add(form); form.Show();
        }

        private void DoLogout(object? sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to logout?", "Logout", MessageBoxButtons.YesNo) == DialogResult.Yes)
            { this.Hide(); var login = new LoginForm(); login.FormClosed += (s, a) => this.Close(); login.Show(); }
        }

        private DataGridView CreateDGV(Point loc, Size size)
        {
            var dgv = new DataGridView
            {
                Location = loc, Size = size, BackgroundColor = Color.White, BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true,
                AllowUserToAddRows = false, RowHeadersVisible = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
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
    }
}
