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
        private readonly Color secondaryColor = Color.FromArgb(193, 154, 107);

        public AdminDashboard() { InitializeComponent(); }

        private void InitializeComponent()
        {
            this.Text = "🔒 Admin — Coffee Marketplace"; this.Size = new Size(1100, 750);
            this.StartPosition = FormStartPosition.CenterScreen; this.BackColor = bgColor; this.Font = new Font("Segoe UI", 10);

            Panel sidebar = new Panel { Dock = DockStyle.Left, Width = 230, BackColor = darkBg };
            sidebar.Paint += (s, e) => { using var b = new LinearGradientBrush(sidebar.ClientRectangle, darkBg, Color.FromArgb(78, 52, 28), 90f); e.Graphics.FillRectangle(b, sidebar.ClientRectangle); };
            sidebar.Controls.Add(new Label { Text = "🔒", Font = new Font("Segoe UI", 36), ForeColor = accentColor, Size = new Size(230, 60), Location = new Point(0, 15), TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent });
            sidebar.Controls.Add(new Label { Text = "ADMIN PANEL", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = accentColor, Size = new Size(210, 30), Location = new Point(10, 80), TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent });
            sidebar.Controls.Add(new Panel { BackColor = accentColor, Size = new Size(190, 2), Location = new Point(20, 120) });

            string[] items = { "📊 Dashboard", "🏪 Shops", "👥 Users", "👨‍🍳 Staff", "📦 Products", "📋 Orders", "💰 Payments", "⭐ Reviews", "📝 Complaints", "🚪 Logout" };
            EventHandler[] handlers = { ShowDash, ShowShops, ShowUsers, ShowStaff, ShowProducts, ShowOrders, ShowPayments, ShowReviews, ShowComplaints, DoLogout };
            for (int i = 0; i < items.Length; i++) {
                Button b = new Button { Text = items[i], Size = new Size(210, 40), Location = new Point(10, 135 + (i * 45)), Font = new Font("Segoe UI", 10), ForeColor = Color.White, BackColor = Color.Transparent, FlatStyle = FlatStyle.Flat, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(15, 0, 0, 0), Cursor = Cursors.Hand };
                b.FlatAppearance.BorderSize = 0; b.FlatAppearance.MouseOverBackColor = Color.FromArgb(50, 255, 255, 255);
                b.Click += handlers[i]; sidebar.Controls.Add(b);
            }
            contentPanel = new Panel { Dock = DockStyle.Fill, BackColor = bgColor, Padding = new Padding(20), AutoScroll = true };
            this.Controls.Add(contentPanel); this.Controls.Add(sidebar);
            ShowDash(null, EventArgs.Empty);
        }

        private void ClearContent() { contentPanel.Controls.Clear(); }
        private Label Title(string t) => new Label { Text = t, Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = primaryColor, AutoSize = true, Location = new Point(20, 15) };
        private DataGridView MakeDGV(Point loc, Size sz) {
            var d = new DataGridView { Location = loc, Size = sz, BackgroundColor = Color.White, BorderStyle = BorderStyle.None, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false, RowHeadersVisible = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, Font = new Font("Segoe UI", 9) };
            d.ColumnHeadersDefaultCellStyle.BackColor = primaryColor; d.ColumnHeadersDefaultCellStyle.ForeColor = Color.White; d.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold); d.EnableHeadersVisualStyles = false;
            d.DefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 235, 200); d.DefaultCellStyle.SelectionForeColor = Color.Black; return d;
        }
        private Button DelBtn(string text, Point loc) { var b = new Button { Text = text, Location = loc, Size = new Size(200, 38), Font = new Font("Segoe UI", 10, FontStyle.Bold), BackColor = Color.FromArgb(244, 67, 54), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand }; b.FlatAppearance.BorderSize = 0; return b; }

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
                pending = (int)new SqlCommand("SELECT COUNT(*) FROM Orders WHERE Status='Pending'", c).ExecuteScalar()!;
                rev = (decimal)new SqlCommand("SELECT ISNULL(SUM(Amount),0) FROM Payment", c).ExecuteScalar()!;
            }
            var cards = new (string t, string v, string ic, Color cl)[] { ("Shops", shops.ToString(), "🏪", Color.FromArgb(0, 150, 136)), ("Users", users.ToString(), "👥", Color.FromArgb(33, 150, 243)), ("Staff", staff.ToString(), "👨‍🍳", Color.FromArgb(156, 39, 176)), ("Products", products.ToString(), "📦", Color.FromArgb(255, 152, 0)), ("Orders", orders.ToString(), "📋", Color.FromArgb(76, 175, 80)), ("Pending", pending.ToString(), "⏳", Color.FromArgb(244, 67, 54)), ("Revenue", $"৳{rev:N0}", "💰", Color.FromArgb(0, 120, 80)) };
            for (int i = 0; i < cards.Length; i++) {
                int r = i / 4, col = i % 4;
                Panel cd = new Panel { Size = new Size(190, 100), Location = new Point(20 + (col * 200), 60 + (r * 115)), BackColor = Color.White };
                cd.Paint += (s2, pe) => { using var pen = new Pen(Color.FromArgb(230, 220, 200)); pe.Graphics.DrawRectangle(pen, 0, 0, ((Panel)s2!).Width - 1, ((Panel)s2!).Height - 1); };
                cd.Controls.Add(new Panel { Size = new Size(5, 100), BackColor = cards[i].cl });
                cd.Controls.Add(new Label { Text = cards[i].ic, Font = new Font("Segoe UI", 22), Location = new Point(15, 15), Size = new Size(45, 45) });
                cd.Controls.Add(new Label { Text = cards[i].t, Font = new Font("Segoe UI", 8), ForeColor = Color.Gray, Location = new Point(65, 12), AutoSize = true });
                cd.Controls.Add(new Label { Text = cards[i].v, Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = cards[i].cl, Location = new Point(65, 35), AutoSize = true });
                contentPanel.Controls.Add(cd);
            }
        }

        private void ShowShops(object? s, EventArgs e) { ShowTable("🏪 Coffee Shops", "SELECT ShopId AS ID, ShopName AS Shop, Location, AverageRating AS Rating, CASE WHEN IsActive=1 THEN 'Active' ELSE 'Inactive' END AS Status FROM CoffeeShop", "CoffeeShop", "ShopId"); }
        private void ShowUsers(object? s, EventArgs e) { ShowTable("👥 All Users", "SELECT UserId AS ID, FirstName+' '+LastName AS Name, Email, Phone, Address, Username FROM UserDetails", "UserDetails", "UserId"); }
        private void ShowStaff(object? s, EventArgs e) { ShowTable("👨‍🍳 All Staff", "SELECT s.StaffId AS ID, s.FirstName+' '+s.LastName AS Name, c.ShopName AS Shop, s.Email, s.Role, s.Username FROM Staff s JOIN CoffeeShop c ON s.ShopId=c.ShopId", "Staff", "StaffId"); }
        private void ShowProducts(object? s, EventArgs e) { ShowTable("📦 All Products", "SELECT p.ProductId AS ID, p.ProductName AS Product, c.ShopName AS Shop, cat.CategoryName AS Category, p.Price AS [Price ৳], p.Stock FROM Product p JOIN CoffeeShop c ON p.ShopId=c.ShopId JOIN Category cat ON p.CategoryId=cat.CategoryId", "Product", "ProductId"); }
        private void ShowOrders(object? s, EventArgs e) { ShowTable("📋 All Orders", "SELECT o.OrderId AS [Order #], u.FirstName+' '+u.LastName AS Customer, c.ShopName AS Shop, o.OrderDate AS Date, o.TotalAmount AS [Total ৳], o.Status FROM Orders o LEFT JOIN UserDetails u ON o.UserId=u.UserId JOIN CoffeeShop c ON o.ShopId=c.ShopId ORDER BY o.OrderId DESC", null, null); }
        private void ShowPayments(object? s, EventArgs e) { ShowTable("💰 All Payments", "SELECT p.PaymentId AS [#], p.OrderId AS [Order], u.FirstName+' '+u.LastName AS Customer, c.ShopName AS Shop, p.Amount AS [Amount ৳], p.PaymentMethod AS Method, ISNULL(p.PaymentProvider,'') AS Provider, p.PaymentDate AS Date FROM Payment p JOIN Orders o ON p.OrderId=o.OrderId LEFT JOIN UserDetails u ON o.UserId=u.UserId JOIN CoffeeShop c ON o.ShopId=c.ShopId ORDER BY p.PaymentId DESC", null, null); }
        private void ShowReviews(object? s, EventArgs e) { ShowTable("⭐ All Reviews", "SELECT r.ReviewId AS [#], u.FirstName+' '+u.LastName AS Customer, p.ProductName AS Item, c.ShopName AS Shop, r.Rating AS Stars, r.ReviewText AS Review, r.ReviewDate AS Date FROM ItemReview r JOIN UserDetails u ON r.UserId=u.UserId JOIN Product p ON r.ProductId=p.ProductId JOIN CoffeeShop c ON p.ShopId=c.ShopId ORDER BY r.ReviewDate DESC", null, null); }

        private void ShowComplaints(object? s, EventArgs e)
        {
            ClearContent(); contentPanel.Controls.Add(Title("📝 All Complaints"));
            DataGridView dgv = MakeDGV(new Point(20, 60), new Size(810, 350));
            using (var c = DatabaseHelper.GetConnection()) {
                var dt = new DataTable(); using var r = new SqlCommand("SELECT c.ComplaintId AS [#], u.FirstName+' '+u.LastName AS Customer, s.ShopName AS Shop, c.Subject, c.Status, c.CreatedDate AS Date, c.ResolvedDate FROM Complaint c JOIN UserDetails u ON c.UserId=u.UserId JOIN CoffeeShop s ON c.ShopId=s.ShopId ORDER BY c.CreatedDate DESC", c).ExecuteReader();
                dt.Load(r); dgv.DataSource = dt;
            }

            string[] sts = { "In Progress", "Resolved", "Closed" };
            Color[] cls = { Color.FromArgb(33, 150, 243), Color.FromArgb(76, 175, 80), Color.Gray };
            for (int i = 0; i < sts.Length; i++) {
                string st = sts[i];
                Button b = new Button { Text = $"→ {st}", Location = new Point(20 + (i * 180), 420), Size = new Size(170, 36), Font = new Font("Segoe UI", 10, FontStyle.Bold), BackColor = cls[i], ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
                b.FlatAppearance.BorderSize = 0;
                b.Click += (s2, ev) => {
                    if (dgv.SelectedRows.Count > 0) {
                        using var c = DatabaseHelper.GetConnection();
                        string resolved = st == "Resolved" || st == "Closed" ? ",ResolvedDate=GETDATE()" : "";
                        new SqlCommand($"UPDATE Complaint SET Status='{st}'{resolved} WHERE ComplaintId={dgv.SelectedRows[0].Cells["#"].Value}", c).ExecuteNonQuery();
                        ShowComplaints(null, EventArgs.Empty);
                    }
                };
                contentPanel.Controls.Add(b);
            }
            contentPanel.Controls.Add(dgv);
        }

        private void ShowTable(string title, string query, string? delTable, string? delIdCol)
        {
            ClearContent(); contentPanel.Controls.Add(Title(title));
            DataGridView dgv = MakeDGV(new Point(20, 60), new Size(810, 420));
            using (var c = DatabaseHelper.GetConnection()) { var dt = new DataTable(); using var r = new SqlCommand(query, c).ExecuteReader(); dt.Load(r); dgv.DataSource = dt; }
            contentPanel.Controls.Add(dgv);

            if (delTable != null && delIdCol != null) {
                Button bd = DelBtn($"🗑 Delete", new Point(20, 490));
                bd.Click += (s, ev) => {
                    if (dgv.SelectedRows.Count > 0 && MessageBox.Show("Delete this record?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes) {
                        using var c = DatabaseHelper.GetConnection();
                        new SqlCommand($"DELETE FROM {delTable} WHERE {delIdCol}={dgv.SelectedRows[0].Cells["ID"].Value}", c).ExecuteNonQuery();
                        ShowTable(title, query, delTable, delIdCol);
                    }
                };
                contentPanel.Controls.Add(bd);
            }
        }

        private void DoLogout(object? s, EventArgs e) { if (MessageBox.Show("Logout?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes) { this.Hide(); var l = new LoginForm(); l.FormClosed += (s2, a) => this.Close(); l.Show(); } }
    }
}
