using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace CoffeeShopManagement.Forms
{
    public class AdminDashboard : Form
    {
        private Panel contentPanel;
        private readonly Color primaryColor = Color.FromArgb(101, 67, 33);
        private readonly Color accentColor = Color.FromArgb(255, 183, 77);
        private readonly Color bgColor = Color.FromArgb(250, 243, 232);
        private readonly Color darkBg = Color.FromArgb(62, 39, 18);

        public AdminDashboard() { InitializeComponent(); }

        private void InitializeComponent()
        {
            this.Text = "🔒 Admin — Coffee Marketplace"; this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen; this.WindowState = FormWindowState.Maximized;
            this.BackColor = bgColor; this.Font = new Font("Segoe UI", 10); this.MinimumSize = new Size(1000, 650);

            Panel sidebar = new Panel { Dock = DockStyle.Left, Width = 230, BackColor = darkBg };
            sidebar.Paint += (s, e) => { using var b = new LinearGradientBrush(sidebar.ClientRectangle, darkBg, Color.FromArgb(78, 52, 28), 90f); e.Graphics.FillRectangle(b, sidebar.ClientRectangle); };
            sidebar.Controls.Add(new Label { Text = "🔒", Font = new Font("Segoe UI", 36), ForeColor = accentColor, Size = new Size(230, 55), Location = new Point(0, 15), TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent });
            sidebar.Controls.Add(new Label { Text = "ADMIN PANEL", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = accentColor, Size = new Size(210, 30), Location = new Point(10, 72), TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent });
            sidebar.Controls.Add(new Panel { BackColor = accentColor, Size = new Size(190, 2), Location = new Point(20, 110) });

            string[] items = { "📊 Dashboard", "✅ Approvals", "📈 Analytics", "🏪 Shops", "👥 Users", "👨‍🍳 Staff", "📦 Products", "📋 Orders", "💰 Payments", "⭐ Reviews", "📝 Complaints", "🚪 Logout" };
            EventHandler[] handlers = { ShowDash, ShowApprovals, ShowAnalytics, ShowShops, ShowUsers, ShowStaff, ShowProducts, ShowOrders, ShowPayments, ShowReviews, ShowComplaints, DoLogout };
            for (int i = 0; i < items.Length; i++) {
                Button b = new Button { Text = items[i], Size = new Size(210, 38), Location = new Point(10, 122 + (i * 42)), Font = new Font("Segoe UI", 10), ForeColor = Color.White, BackColor = Color.Transparent, FlatStyle = FlatStyle.Flat, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(12, 0, 0, 0), Cursor = Cursors.Hand };
                b.FlatAppearance.BorderSize = 0; b.FlatAppearance.MouseOverBackColor = Color.FromArgb(50, 255, 255, 255);
                b.Click += handlers[i]; sidebar.Controls.Add(b);
            }
            contentPanel = new Panel { Dock = DockStyle.Fill, BackColor = bgColor, Padding = new Padding(20), AutoScroll = true };
            this.Controls.Add(contentPanel); this.Controls.Add(sidebar);
            ShowDash(null, EventArgs.Empty);
        }

        private void ClearContent() { contentPanel.Controls.Clear(); contentPanel.AutoScrollPosition = new Point(0, 0); }
        private Label Title(string t) => new Label { Text = t, Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = primaryColor, AutoSize = true, Location = new Point(20, 15) };
        private DataGridView MakeDGV(Point loc, Size sz) {
            var d = new DataGridView { Location = loc, Size = sz, BackgroundColor = Color.White, BorderStyle = BorderStyle.None, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false, RowHeadersVisible = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, Font = new Font("Segoe UI", 9), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            d.ColumnHeadersDefaultCellStyle.BackColor = primaryColor; d.ColumnHeadersDefaultCellStyle.ForeColor = Color.White; d.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold); d.EnableHeadersVisualStyles = false;
            d.DefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 235, 200); d.DefaultCellStyle.SelectionForeColor = Color.Black; return d;
        }

        // ═══ DASHBOARD ═══
        private void ShowDash(object? s, EventArgs e)
        {
            ClearContent(); contentPanel.Controls.Add(Title("📊 Admin Dashboard"));
            int users = 0, staff = 0, shops = 0, products = 0, orders = 0, pending = 0; decimal rev = 0;
            using (var c = DatabaseHelper.GetConnection()) {
                users = (int)new SqlCommand("SELECT COUNT(*) FROM UserDetails", c).ExecuteScalar()!;
                staff = (int)new SqlCommand("SELECT COUNT(*) FROM Staff", c).ExecuteScalar()!;
                shops = (int)new SqlCommand("SELECT COUNT(*) FROM CoffeeShop", c).ExecuteScalar()!;
                products = (int)new SqlCommand("SELECT COUNT(*) FROM Product", c).ExecuteScalar()!;
                orders = (int)new SqlCommand("SELECT COUNT(*) FROM Orders", c).ExecuteScalar()!;
                pending = (int)new SqlCommand("SELECT COUNT(*) FROM Orders WHERE Status='Awaiting Approval'", c).ExecuteScalar()!;
                rev = (decimal)new SqlCommand("SELECT ISNULL(SUM(FinalAmount),0) FROM Payment", c).ExecuteScalar()!;
            }
            var cards = new (string t, string v, string ic, Color cl)[] { ("Shops", shops.ToString(), "🏪", Color.FromArgb(0, 150, 136)), ("Users", users.ToString(), "👥", Color.FromArgb(33, 150, 243)), ("Staff", staff.ToString(), "👨‍🍳", Color.FromArgb(156, 39, 176)), ("Products", products.ToString(), "📦", Color.FromArgb(255, 152, 0)), ("Orders", orders.ToString(), "📋", Color.FromArgb(76, 175, 80)), ("Awaiting", pending.ToString(), "⏳", Color.FromArgb(244, 67, 54)), ("Revenue", $"৳{rev:N0}", "💰", Color.FromArgb(0, 120, 80)) };
            for (int i = 0; i < cards.Length; i++) {
                Panel cd = new Panel { Size = new Size(190, 95), Location = new Point(20 + ((i % 4) * 200), 60 + ((i / 4) * 110)), BackColor = Color.White };
                cd.Paint += (s2, pe) => { using var pen = new Pen(Color.FromArgb(230, 220, 200)); pe.Graphics.DrawRectangle(pen, 0, 0, ((Panel)s2!).Width - 1, ((Panel)s2!).Height - 1); };
                cd.Controls.Add(new Panel { Size = new Size(5, 95), BackColor = cards[i].cl });
                cd.Controls.Add(new Label { Text = cards[i].ic, Font = new Font("Segoe UI", 20), Location = new Point(15, 12), Size = new Size(42, 42) });
                cd.Controls.Add(new Label { Text = cards[i].t, Font = new Font("Segoe UI", 8), ForeColor = Color.Gray, Location = new Point(60, 12), AutoSize = true });
                cd.Controls.Add(new Label { Text = cards[i].v, Font = new Font("Segoe UI", 17, FontStyle.Bold), ForeColor = cards[i].cl, Location = new Point(60, 33), AutoSize = true });
                contentPanel.Controls.Add(cd);
            }
        }

        // ═══ PENDING APPROVALS ═══
        private void ShowApprovals(object? s, EventArgs e)
        {
            ClearContent(); contentPanel.Controls.Add(Title("✅ Pending Order Approvals"));
            contentPanel.Controls.Add(new Label { Text = "Orders awaiting admin confirmation before staff can process them", Font = new Font("Segoe UI", 10), ForeColor = Color.Gray, Location = new Point(22, 45), AutoSize = true });

            DataGridView dgv = MakeDGV(new Point(20, 75), new Size(contentPanel.Width - 80, 350));
            Action loadApprovals = () => {
                using var c = DatabaseHelper.GetConnection();
                var dt = new DataTable();
                using var r = new SqlCommand("SELECT o.OrderId AS [Order #], u.FirstName+' '+u.LastName AS Customer, s.ShopName AS Shop, o.OrderDate AS Date, o.TotalAmount AS [Total ৳], o.Status FROM Orders o LEFT JOIN UserDetails u ON o.UserId=u.UserId JOIN CoffeeShop s ON o.ShopId=s.ShopId WHERE o.Status='Awaiting Approval' ORDER BY o.OrderId DESC", c).ExecuteReader();
                dt.Load(r); dgv.DataSource = dt;
            };
            loadApprovals();

            Button btnConfirm = new Button { Text = "✅ Confirm Selected Order", Location = new Point(20, 440), Size = new Size(280, 45), Font = new Font("Segoe UI", 12, FontStyle.Bold), BackColor = Color.FromArgb(76, 175, 80), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnConfirm.FlatAppearance.BorderSize = 0;
            btnConfirm.Click += (s2, ev) => {
                if (dgv.SelectedRows.Count == 0) return;
                int oid = Convert.ToInt32(dgv.SelectedRows[0].Cells["Order #"].Value);
                using var c = DatabaseHelper.GetConnection();
                new SqlCommand($"UPDATE Orders SET Status='Confirmed' WHERE OrderId={oid}", c).ExecuteNonQuery();
                MessageBox.Show($"Order #{oid} confirmed! ✅ Staff can now process it.", "Approved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                loadApprovals();
            };

            Button btnReject = new Button { Text = "❌ Reject Selected Order", Location = new Point(320, 440), Size = new Size(280, 45), Font = new Font("Segoe UI", 12, FontStyle.Bold), BackColor = Color.FromArgb(244, 67, 54), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnReject.FlatAppearance.BorderSize = 0;
            btnReject.Click += (s2, ev) => {
                if (dgv.SelectedRows.Count == 0) return;
                int oid = Convert.ToInt32(dgv.SelectedRows[0].Cells["Order #"].Value);
                using var c = DatabaseHelper.GetConnection();
                new SqlCommand($"UPDATE Orders SET Status='Rejected' WHERE OrderId={oid}", c).ExecuteNonQuery();
                MessageBox.Show($"Order #{oid} rejected.", "Rejected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                loadApprovals();
            };

            Button btnConfirmAll = new Button { Text = "✅✅ Confirm ALL Pending", Location = new Point(620, 440), Size = new Size(250, 45), Font = new Font("Segoe UI", 12, FontStyle.Bold), BackColor = Color.FromArgb(0, 150, 136), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnConfirmAll.FlatAppearance.BorderSize = 0;
            btnConfirmAll.Click += (s2, ev) => {
                using var c = DatabaseHelper.GetConnection();
                int count = (int)new SqlCommand("SELECT COUNT(*) FROM Orders WHERE Status='Awaiting Approval'", c).ExecuteScalar()!;
                if (count == 0) { MessageBox.Show("No pending orders."); return; }
                new SqlCommand("UPDATE Orders SET Status='Confirmed' WHERE Status='Awaiting Approval'", c).ExecuteNonQuery();
                MessageBox.Show($"All {count} orders confirmed! ✅", "Bulk Approved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                loadApprovals();
            };

            contentPanel.Controls.AddRange(new Control[] { dgv, btnConfirm, btnReject, btnConfirmAll });
        }

        // ═══ ANALYTICS (GDI+ Bar Chart) ═══
        private void ShowAnalytics(object? s, EventArgs e)
        {
            ClearContent(); contentPanel.Controls.Add(Title("📈 Sales Analytics"));

            // Sales per shop data
            var shopSales = new System.Collections.Generic.List<(string name, decimal sales)>();
            using (var c = DatabaseHelper.GetConnection()) {
                using var r = new SqlCommand("SELECT s.ShopName, ISNULL(SUM(p.FinalAmount),0) AS Sales FROM CoffeeShop s LEFT JOIN Orders o ON s.ShopId=o.ShopId LEFT JOIN Payment p ON o.OrderId=p.OrderId GROUP BY s.ShopName ORDER BY Sales DESC", c).ExecuteReader();
                while (r.Read()) shopSales.Add((r.GetString(0), r.GetDecimal(1)));
            }

            // Bar chart panel
            Panel chartPanel = new Panel { Location = new Point(20, 55), Size = new Size(780, 380), BackColor = Color.White, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            chartPanel.Paint += (s2, pe) => {
                var g = pe.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
                g.DrawRectangle(new Pen(Color.FromArgb(220, 210, 195)), 0, 0, chartPanel.Width - 1, chartPanel.Height - 1);
                g.DrawString("💰 Total Sales Per Shop (৳)", new Font("Segoe UI", 14, FontStyle.Bold), new SolidBrush(primaryColor), 20, 15);

                if (shopSales.Count == 0) { g.DrawString("No sales data yet", new Font("Segoe UI", 12), Brushes.Gray, 300, 180); return; }

                decimal max = 1; foreach (var ss in shopSales) if (ss.sales > max) max = ss.sales;
                int barW = Math.Min(120, (chartPanel.Width - 100) / Math.Max(shopSales.Count, 1));
                int chartH = 260, baseY = 340;
                Color[] colors = { Color.FromArgb(0, 150, 136), Color.FromArgb(33, 150, 243), Color.FromArgb(255, 152, 0), Color.FromArgb(156, 39, 176), Color.FromArgb(244, 67, 54) };

                for (int i = 0; i < shopSales.Count; i++) {
                    int barH = (int)((double)shopSales[i].sales / (double)max * chartH);
                    int x = 60 + i * (barW + 20);
                    using var brush = new LinearGradientBrush(new Rectangle(x, baseY - barH, barW, barH), colors[i % colors.Length], ControlPaint.Light(colors[i % colors.Length], 0.3f), 90f);
                    g.FillRectangle(brush, x, baseY - barH, barW, barH);
                    g.DrawRectangle(new Pen(ControlPaint.Dark(colors[i % colors.Length], 0.2f)), x, baseY - barH, barW, barH);
                    // Value on top
                    g.DrawString($"৳{shopSales[i].sales:N0}", new Font("Segoe UI", 8, FontStyle.Bold), new SolidBrush(colors[i % colors.Length]), x, baseY - barH - 18);
                    // Label below
                    var sf = new StringFormat { Alignment = StringAlignment.Center };
                    g.DrawString(shopSales[i].name, new Font("Segoe UI", 8), Brushes.Black, x + barW / 2, baseY + 5, sf);
                }
                // Axis line
                g.DrawLine(new Pen(Color.Gray, 1), 55, baseY, 55 + shopSales.Count * (barW + 20), baseY);
            };

            // Top items
            Panel itemsPanel = new Panel { Location = new Point(20, 450), Size = new Size(780, 200), BackColor = Color.White, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            itemsPanel.Paint += (s2, pe) => {
                pe.Graphics.DrawRectangle(new Pen(Color.FromArgb(220, 210, 195)), 0, 0, itemsPanel.Width - 1, itemsPanel.Height - 1);
                pe.Graphics.DrawString("🏆 Top Selling Items", new Font("Segoe UI", 14, FontStyle.Bold), new SolidBrush(primaryColor), 20, 12);
                using var c = DatabaseHelper.GetConnection();
                using var r = new SqlCommand("SELECT TOP 5 p.ProductName, SUM(od.Quantity) AS Qty, s.ShopName FROM OrderDetails od JOIN Product p ON od.ProductId=p.ProductId JOIN CoffeeShop s ON p.ShopId=s.ShopId GROUP BY p.ProductName, s.ShopName ORDER BY Qty DESC", c).ExecuteReader();
                int y = 48; int rank = 1;
                string[] medals = { "🥇", "🥈", "🥉", "4.", "5." };
                while (r.Read() && rank <= 5) {
                    pe.Graphics.DrawString($"{medals[rank - 1]} {r.GetString(0)} ({r.GetString(2)}) — {r.GetInt32(1)} sold",
                        new Font("Segoe UI", 11), new SolidBrush(rank <= 3 ? primaryColor : Color.Gray), 30, y);
                    y += 28; rank++;
                }
                if (rank == 1) pe.Graphics.DrawString("No sales data yet", new Font("Segoe UI", 11), Brushes.Gray, 30, 50);
            };

            contentPanel.Controls.AddRange(new Control[] { chartPanel, itemsPanel });
        }

        // ═══ STAFF with Shop Filtering ═══
        private void ShowStaff(object? s, EventArgs e)
        {
            ClearContent(); contentPanel.Controls.Add(Title("👨‍🍳 All Staff"));

            Label lblFilter = new Label { Text = "🏪 Filter by Shop:", Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = primaryColor, Location = new Point(22, 52), AutoSize = true };
            ComboBox cmbShop = new ComboBox { Location = new Point(180, 48), Size = new Size(280, 30), Font = new Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbShop.Items.Add("All Shops");
            using (var c = DatabaseHelper.GetConnection()) { using var r = new SqlCommand("SELECT ShopId, ShopName FROM CoffeeShop", c).ExecuteReader(); while (r.Read()) cmbShop.Items.Add($"{r.GetInt32(0)}|{r.GetString(1)}"); }
            cmbShop.SelectedIndex = 0;

            DataGridView dgv = MakeDGV(new Point(20, 90), new Size(contentPanel.Width - 80, 380));
            Action loadStaff = () => {
                using var c = DatabaseHelper.GetConnection();
                string q = "SELECT s.StaffId AS ID, s.FirstName+' '+s.LastName AS Name, c.ShopName AS Shop, s.Email, s.Role, s.Username FROM Staff s JOIN CoffeeShop c ON s.ShopId=c.ShopId";
                if (cmbShop.SelectedIndex > 0) { int sid = int.Parse(cmbShop.SelectedItem!.ToString()!.Split('|')[0]); q += $" WHERE s.ShopId={sid}"; }
                q += " ORDER BY c.ShopName, s.FirstName";
                var dt = new DataTable(); using var r = new SqlCommand(q, c).ExecuteReader(); dt.Load(r); dgv.DataSource = dt;
            };
            cmbShop.SelectedIndexChanged += (s2, ev) => loadStaff(); loadStaff();

            Button btnDel = new Button { Text = "🗑 Delete Staff", Location = new Point(20, 485), Size = new Size(200, 38), Font = new Font("Segoe UI", 10, FontStyle.Bold), BackColor = Color.FromArgb(244, 67, 54), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnDel.FlatAppearance.BorderSize = 0;
            btnDel.Click += (s2, ev) => {
                if (dgv.SelectedRows.Count > 0 && MessageBox.Show("Delete?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes) {
                    using var c = DatabaseHelper.GetConnection(); new SqlCommand($"DELETE FROM Staff WHERE StaffId={dgv.SelectedRows[0].Cells["ID"].Value}", c).ExecuteNonQuery(); loadStaff();
                }
            };
            contentPanel.Controls.AddRange(new Control[] { lblFilter, cmbShop, dgv, btnDel });
        }

        // ═══ Generic table views ═══
        private void ShowShops(object? s, EventArgs e) { ShowTable("🏪 Coffee Shops", "SELECT ShopId AS ID, ShopName AS Shop, Location, AverageRating AS Rating, CASE WHEN IsActive=1 THEN 'Active' ELSE 'Inactive' END AS Status FROM CoffeeShop", "CoffeeShop", "ShopId"); }
        private void ShowUsers(object? s, EventArgs e) { ShowTable("👥 All Users", "SELECT UserId AS ID, FirstName+' '+LastName AS Name, Email, Phone, Address, Username FROM UserDetails", "UserDetails", "UserId"); }
        private void ShowProducts(object? s, EventArgs e) { ShowTable("📦 All Products", "SELECT p.ProductId AS ID, p.ProductName AS Product, c.ShopName AS Shop, cat.CategoryName AS Category, p.Price AS [Price ৳], p.Stock FROM Product p JOIN CoffeeShop c ON p.ShopId=c.ShopId JOIN Category cat ON p.CategoryId=cat.CategoryId", "Product", "ProductId"); }
        private void ShowOrders(object? s, EventArgs e) { ShowTable("📋 All Orders", "SELECT o.OrderId AS [Order #], u.FirstName+' '+u.LastName AS Customer, c.ShopName AS Shop, o.OrderDate AS Date, o.TotalAmount AS [Total ৳], o.Status FROM Orders o LEFT JOIN UserDetails u ON o.UserId=u.UserId JOIN CoffeeShop c ON o.ShopId=c.ShopId ORDER BY o.OrderId DESC", null, null); }
        private void ShowPayments(object? s, EventArgs e) { ShowTable("💰 All Payments", "SELECT p.PaymentId AS [#], p.OrderId AS [Order], u.FirstName+' '+u.LastName AS Customer, c.ShopName AS Shop, p.Amount AS [Original ৳], p.DiscountAmount AS [Discount ৳], p.FinalAmount AS [Paid ৳], p.PaymentMethod AS Method, ISNULL(p.PaymentProvider,'') AS Provider, ISNULL(p.PromoCode,'') AS Promo, p.PaymentDate AS Date FROM Payment p JOIN Orders o ON p.OrderId=o.OrderId LEFT JOIN UserDetails u ON o.UserId=u.UserId JOIN CoffeeShop c ON o.ShopId=c.ShopId ORDER BY p.PaymentId DESC", null, null); }
        private void ShowReviews(object? s, EventArgs e) { ShowTable("⭐ All Reviews", "SELECT r.ReviewId AS [#], u.FirstName+' '+u.LastName AS Customer, p.ProductName AS Item, c.ShopName AS Shop, r.Rating AS Stars, r.ReviewText AS Review, r.ReviewDate AS Date FROM ItemReview r JOIN UserDetails u ON r.UserId=u.UserId JOIN Product p ON r.ProductId=p.ProductId JOIN CoffeeShop c ON p.ShopId=c.ShopId ORDER BY r.ReviewDate DESC", null, null); }

        private void ShowComplaints(object? s, EventArgs e)
        {
            ClearContent(); contentPanel.Controls.Add(Title("📝 All Complaints"));
            DataGridView dgv = MakeDGV(new Point(20, 60), new Size(contentPanel.Width - 80, 350));
            using (var c = DatabaseHelper.GetConnection()) {
                var dt = new DataTable(); using var r = new SqlCommand("SELECT c.ComplaintId AS [#], u.FirstName+' '+u.LastName AS Customer, s.ShopName AS Shop, c.Subject, c.Status, c.CreatedDate AS Date FROM Complaint c JOIN UserDetails u ON c.UserId=u.UserId JOIN CoffeeShop s ON c.ShopId=s.ShopId ORDER BY c.CreatedDate DESC", c).ExecuteReader();
                dt.Load(r); dgv.DataSource = dt;
            }
            string[] sts = { "In Progress", "Resolved", "Closed" }; Color[] cls = { Color.FromArgb(33, 150, 243), Color.FromArgb(76, 175, 80), Color.Gray };
            for (int i = 0; i < sts.Length; i++) {
                string st = sts[i];
                Button b = new Button { Text = $"→ {st}", Location = new Point(20 + (i * 180), 420), Size = new Size(170, 36), Font = new Font("Segoe UI", 10, FontStyle.Bold), BackColor = cls[i], ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
                b.FlatAppearance.BorderSize = 0;
                b.Click += (s2, ev) => {
                    if (dgv.SelectedRows.Count > 0) { using var c = DatabaseHelper.GetConnection(); string resolved = st != "In Progress" ? ",ResolvedDate=GETDATE()" : ""; new SqlCommand($"UPDATE Complaint SET Status='{st}'{resolved} WHERE ComplaintId={dgv.SelectedRows[0].Cells["#"].Value}", c).ExecuteNonQuery(); ShowComplaints(null, EventArgs.Empty); }
                };
                contentPanel.Controls.Add(b);
            }
            contentPanel.Controls.Add(dgv);
        }

        private void ShowTable(string title, string query, string? delTable, string? delIdCol)
        {
            ClearContent(); contentPanel.Controls.Add(Title(title));
            DataGridView dgv = MakeDGV(new Point(20, 60), new Size(contentPanel.Width - 80, 420));
            using (var c = DatabaseHelper.GetConnection()) { var dt = new DataTable(); using var r = new SqlCommand(query, c).ExecuteReader(); dt.Load(r); dgv.DataSource = dt; }
            contentPanel.Controls.Add(dgv);
            if (delTable != null) {
                Button bd = new Button { Text = "🗑 Delete", Location = new Point(20, 490), Size = new Size(200, 38), Font = new Font("Segoe UI", 10, FontStyle.Bold), BackColor = Color.FromArgb(244, 67, 54), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
                bd.FlatAppearance.BorderSize = 0;
                bd.Click += (s, ev) => { if (dgv.SelectedRows.Count > 0 && MessageBox.Show("Delete?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes) { using var c = DatabaseHelper.GetConnection(); new SqlCommand($"DELETE FROM {delTable} WHERE {delIdCol}={dgv.SelectedRows[0].Cells["ID"].Value}", c).ExecuteNonQuery(); ShowTable(title, query, delTable, delIdCol); } };
                contentPanel.Controls.Add(bd);
            }
        }

        private void DoLogout(object? s, EventArgs e) { if (MessageBox.Show("Logout?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes) { this.Hide(); var l = new LoginForm(); l.FormClosed += (s2, a) => this.Close(); l.Show(); } }
    }
}
