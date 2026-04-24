using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace CoffeeShopManagement.Forms
{
    public class StaffDashboard : Form
    {
        private int staffId, shopId;
        private string staffName, staffRole;
        private Panel contentPanel;
        private readonly Color primaryColor = Color.FromArgb(101, 67, 33);
        private readonly Color accentColor = Color.FromArgb(255, 183, 77);
        private readonly Color bgColor = Color.FromArgb(250, 243, 232);
        private readonly Color darkBg = Color.FromArgb(62, 39, 18);

        public StaffDashboard(int staffId, string name, string role, int shopId)
        {
            this.staffId = staffId; this.staffName = name; this.staffRole = role; this.shopId = shopId;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            string shopName = "";
            using (var c = DatabaseHelper.GetConnection()) { var cmd = new SqlCommand("SELECT ShopName FROM CoffeeShop WHERE ShopId=@s", c); cmd.Parameters.AddWithValue("@s", shopId); shopName = cmd.ExecuteScalar()?.ToString() ?? ""; }

            this.Text = $"👨‍🍳 {shopName} — {staffName} ({staffRole})";
            this.Size = new Size(1050, 700); this.StartPosition = FormStartPosition.CenterScreen; this.BackColor = bgColor; this.Font = new Font("Segoe UI", 10);

            Panel sidebar = new Panel { Dock = DockStyle.Left, Width = 220, BackColor = darkBg };
            sidebar.Paint += (s, e) => { using var b = new LinearGradientBrush(sidebar.ClientRectangle, darkBg, Color.FromArgb(78, 52, 28), 90f); e.Graphics.FillRectangle(b, sidebar.ClientRectangle); };
            Label ll = new Label { Text = "👨‍🍳", Font = new Font("Segoe UI", 36), ForeColor = accentColor, Size = new Size(220, 60), Location = new Point(0, 15), TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent };
            Label lw = new Label { Text = $"{staffName}\n{shopName}", Font = new Font("Segoe UI", 10), ForeColor = Color.White, Size = new Size(200, 45), Location = new Point(10, 80), TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent };
            Panel div = new Panel { BackColor = accentColor, Size = new Size(180, 2), Location = new Point(20, 135) };
            sidebar.Controls.AddRange(new Control[] { ll, lw, div });

            string[] items = { "🏠 Dashboard", "📦 Products", "📋 Orders", "💰 Payments", "👤 Account", "🚪 Logout" };
            EventHandler[] handlers = { ShowDash, ShowProducts, ShowOrders, ShowPayments, ShowAccount, DoLogout };
            for (int i = 0; i < items.Length; i++)
            {
                Button b = new Button { Text = items[i], Size = new Size(200, 44), Location = new Point(10, 150 + (i * 52)), Font = new Font("Segoe UI", 11), ForeColor = Color.White, BackColor = Color.Transparent, FlatStyle = FlatStyle.Flat, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(15, 0, 0, 0), Cursor = Cursors.Hand };
                b.FlatAppearance.BorderSize = 0; b.FlatAppearance.MouseOverBackColor = Color.FromArgb(50, 255, 255, 255);
                b.Click += handlers[i]; sidebar.Controls.Add(b);
            }
            contentPanel = new Panel { Dock = DockStyle.Fill, BackColor = bgColor, Padding = new Padding(20) };
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

        private void ShowDash(object? s, EventArgs e)
        {
            ClearContent(); contentPanel.Controls.Add(Title("🏠 Staff Dashboard"));
            int prods = 0, orders = 0, pending = 0, completed = 0; decimal rev = 0;
            using (var c = DatabaseHelper.GetConnection()) {
                prods = (int)new SqlCommand($"SELECT COUNT(*) FROM Product WHERE ShopId={shopId}", c).ExecuteScalar()!;
                orders = (int)new SqlCommand($"SELECT COUNT(*) FROM Orders WHERE ShopId={shopId}", c).ExecuteScalar()!;
                pending = (int)new SqlCommand($"SELECT COUNT(*) FROM Orders WHERE ShopId={shopId} AND Status='Pending'", c).ExecuteScalar()!;
                completed = (int)new SqlCommand($"SELECT COUNT(*) FROM Orders WHERE ShopId={shopId} AND Status='Completed'", c).ExecuteScalar()!;
                rev = (decimal)new SqlCommand($"SELECT ISNULL(SUM(p.Amount),0) FROM Payment p JOIN Orders o ON p.OrderId=o.OrderId WHERE o.ShopId={shopId}", c).ExecuteScalar()!;
            }
            var cards = new (string t, string v, Color c)[] { ("Products", prods.ToString(), Color.FromArgb(33, 150, 243)), ("Orders", orders.ToString(), Color.FromArgb(156, 39, 176)), ("Pending", pending.ToString(), Color.FromArgb(255, 152, 0)), ("Completed", completed.ToString(), Color.FromArgb(76, 175, 80)), ("Revenue", $"৳{rev:N0}", Color.FromArgb(0, 150, 136)) };
            for (int i = 0; i < cards.Length; i++) {
                Panel cd = new Panel { Size = new Size(145, 100), Location = new Point(20 + (i * 155), 60), BackColor = Color.White };
                cd.Paint += (s2, pe) => { using var pen = new Pen(Color.FromArgb(230, 220, 200)); pe.Graphics.DrawRectangle(pen, 0, 0, ((Panel)s2!).Width - 1, ((Panel)s2!).Height - 1); };
                cd.Controls.Add(new Panel { Size = new Size(145, 4), BackColor = cards[i].c });
                cd.Controls.Add(new Label { Text = cards[i].t, Font = new Font("Segoe UI", 9), ForeColor = Color.Gray, Location = new Point(10, 15), AutoSize = true });
                cd.Controls.Add(new Label { Text = cards[i].v, Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = cards[i].c, Location = new Point(10, 40), AutoSize = true });
                contentPanel.Controls.Add(cd);
            }
        }

        private void ShowProducts(object? s, EventArgs e)
        {
            ClearContent(); contentPanel.Controls.Add(Title("📦 Shop Products"));
            Button btnAdd = new Button { Text = "➕ Add Product", Location = new Point(20, 55), Size = new Size(180, 36), Font = new Font("Segoe UI", 10, FontStyle.Bold), BackColor = Color.FromArgb(76, 175, 80), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnAdd.FlatAppearance.BorderSize = 0; btnAdd.Click += (s2, ev) => { new ProductManageForm(0, shopId).ShowDialog(); ShowProducts(null, EventArgs.Empty); };

            DataGridView dgv = MakeDGV(new Point(20, 100), new Size(760, 380));
            using (var c = DatabaseHelper.GetConnection()) {
                var cmd = new SqlCommand("SELECT p.ProductId AS ID, p.ProductName AS Product, c.CategoryName AS Category, p.Price AS [Price ৳], p.Stock, p.Description FROM Product p JOIN Category c ON p.CategoryId=c.CategoryId WHERE p.ShopId=@sid ORDER BY c.CategoryName", c);
                cmd.Parameters.AddWithValue("@sid", shopId); var dt = new DataTable(); using var r = cmd.ExecuteReader(); dt.Load(r); dgv.DataSource = dt;
            }

            Button btnEdit = new Button { Text = "✏️ Edit", Location = new Point(210, 55), Size = new Size(90, 36), Font = new Font("Segoe UI", 10, FontStyle.Bold), BackColor = accentColor, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnEdit.FlatAppearance.BorderSize = 0;
            btnEdit.Click += (s2, ev) => { if (dgv.SelectedRows.Count > 0) { new ProductManageForm(Convert.ToInt32(dgv.SelectedRows[0].Cells["ID"].Value), shopId).ShowDialog(); ShowProducts(null, EventArgs.Empty); } };

            Button btnDel = new Button { Text = "🗑️ Delete", Location = new Point(310, 55), Size = new Size(100, 36), Font = new Font("Segoe UI", 10, FontStyle.Bold), BackColor = Color.FromArgb(244, 67, 54), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnDel.FlatAppearance.BorderSize = 0;
            btnDel.Click += (s2, ev) => { if (dgv.SelectedRows.Count > 0 && MessageBox.Show("Delete?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes) { using var c = DatabaseHelper.GetConnection(); new SqlCommand($"DELETE FROM Product WHERE ProductId={dgv.SelectedRows[0].Cells["ID"].Value}", c).ExecuteNonQuery(); ShowProducts(null, EventArgs.Empty); } };

            contentPanel.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDel, dgv });
        }

        private void ShowOrders(object? s, EventArgs e)
        {
            ClearContent(); contentPanel.Controls.Add(Title("📋 Shop Orders"));
            ComboBox cmb = new ComboBox { Location = new Point(20, 55), Size = new Size(180, 30), Font = new Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList };
            cmb.Items.AddRange(new[] { "All", "Pending", "Preparing", "Ready", "Completed", "Paid", "Cancelled" }); cmb.SelectedIndex = 0;

            DataGridView dgv = MakeDGV(new Point(20, 95), new Size(760, 320));
            Action load = () => {
                using var c = DatabaseHelper.GetConnection();
                string q = "SELECT o.OrderId AS [Order #], u.FirstName+' '+u.LastName AS Customer, o.OrderDate AS Date, o.TotalAmount AS [Total ৳], o.Status FROM Orders o LEFT JOIN UserDetails u ON o.UserId=u.UserId WHERE o.ShopId=@sid";
                if (cmb.SelectedIndex > 0) q += $" AND o.Status='{cmb.SelectedItem}'";
                q += " ORDER BY o.OrderId DESC";
                var cmd = new SqlCommand(q, c); cmd.Parameters.AddWithValue("@sid", shopId);
                var dt = new DataTable(); using var r = cmd.ExecuteReader(); dt.Load(r); dgv.DataSource = dt;
            };
            cmb.SelectedIndexChanged += (s2, ev) => load(); load();

            string[] sts = { "Preparing", "Ready", "Completed", "Cancelled" };
            Color[] cls = { Color.FromArgb(33, 150, 243), Color.FromArgb(255, 152, 0), Color.FromArgb(76, 175, 80), Color.FromArgb(244, 67, 54) };
            for (int i = 0; i < sts.Length; i++) {
                string st = sts[i];
                Button b = new Button { Text = $"→ {st}", Location = new Point(20 + (i * 190), 425), Size = new Size(180, 36), Font = new Font("Segoe UI", 10, FontStyle.Bold), BackColor = cls[i], ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
                b.FlatAppearance.BorderSize = 0;
                b.Click += (s2, ev) => { if (dgv.SelectedRows.Count > 0) { using var c = DatabaseHelper.GetConnection(); var cmd = new SqlCommand("UPDATE Orders SET Status=@st WHERE OrderId=@id", c); cmd.Parameters.AddWithValue("@st", st); cmd.Parameters.AddWithValue("@id", dgv.SelectedRows[0].Cells["Order #"].Value); cmd.ExecuteNonQuery(); load(); } };
                contentPanel.Controls.Add(b);
            }
            contentPanel.Controls.AddRange(new Control[] { cmb, dgv });
        }

        private void ShowPayments(object? s, EventArgs e)
        {
            ClearContent(); contentPanel.Controls.Add(Title("💰 Payments"));
            DataGridView dgv = MakeDGV(new Point(20, 60), new Size(760, 420));
            using (var c = DatabaseHelper.GetConnection()) {
                var cmd = new SqlCommand("SELECT p.PaymentId AS [#], p.OrderId AS [Order], u.FirstName+' '+u.LastName AS Customer, p.PaymentDate AS Date, p.Amount AS [Amount ৳], p.PaymentMethod AS Method, ISNULL(p.PaymentProvider,'') AS Provider FROM Payment p JOIN Orders o ON p.OrderId=o.OrderId LEFT JOIN UserDetails u ON o.UserId=u.UserId WHERE o.ShopId=@sid ORDER BY p.PaymentId DESC", c);
                cmd.Parameters.AddWithValue("@sid", shopId); var dt = new DataTable(); using var r = cmd.ExecuteReader(); dt.Load(r); dgv.DataSource = dt;
            }
            contentPanel.Controls.Add(dgv);
        }

        private void ShowAccount(object? s, EventArgs e)
        {
            ClearContent(); var f = new AccountForm(staffId, "Staff"); f.TopLevel = false; f.FormBorderStyle = FormBorderStyle.None; f.Dock = DockStyle.Fill; contentPanel.Controls.Add(f); f.Show();
        }

        private void DoLogout(object? s, EventArgs e)
        {
            if (MessageBox.Show("Logout?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes) { this.Hide(); var l = new LoginForm(); l.FormClosed += (s2, a) => this.Close(); l.Show(); }
        }
    }
}
