using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace CoffeeShopManagement.Forms
{
    public class ShopMenuForm : Form
    {
        private int userId, shopId;
        private string shopName;
        private FlowLayoutPanel menuPanel;
        private ComboBox cmbCategory;
        private TextBox txtSearch;
        private Dictionary<int, int> cart = new();
        private Label lblCartCount;
        public event EventHandler? BackButton;

        private readonly Color primaryColor = Color.FromArgb(101, 67, 33);
        private readonly Color bgColor = Color.FromArgb(250, 243, 232);
        private readonly Color accentColor = Color.FromArgb(255, 183, 77);

        public ShopMenuForm(int userId, int shopId, string shopName)
        {
            this.userId = userId; this.shopId = shopId; this.shopName = shopName;
            InitializeComponent();
            LoadMenu();
        }

        private void InitializeComponent()
        {
            this.BackColor = bgColor;
            this.Font = new Font("Segoe UI", 10);
            this.WindowState = FormWindowState.Maximized;

            // ─── TOP TOOLBAR (fixed height, docked to top) ───
            Panel toolbarPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 95,
                BackColor = bgColor,
                Padding = new Padding(10, 8, 10, 5)
            };

            Button btnBack = new Button
            {
                Text = "← Back to Shops",
                Location = new Point(10, 8),
                Size = new Size(150, 32),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = Color.FromArgb(120, 90, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.Click += (s, e) => BackButton?.Invoke(this, EventArgs.Empty);

            Label lblTitle = new Label
            {
                Text = $"☕ {shopName}",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = primaryColor,
                Location = new Point(170, 3),
                AutoSize = true
            };

            // Search bar
            txtSearch = new TextBox
            {
                Location = new Point(10, 50),
                Size = new Size(250, 28),
                Font = new Font("Segoe UI", 10),
                PlaceholderText = "🔍 Search products..."
            };
            txtSearch.TextChanged += (s, e) => LoadMenu();

            // Category filter
            cmbCategory = new ComboBox
            {
                Location = new Point(270, 50),
                Size = new Size(180, 28),
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbCategory.Items.Add("All Categories");
            using (var conn = DatabaseHelper.GetConnection())
            {
                using var r = new SqlCommand("SELECT CategoryName FROM Category", conn).ExecuteReader();
                while (r.Read()) cmbCategory.Items.Add(r.GetString(0));
            }
            cmbCategory.SelectedIndex = 0;
            cmbCategory.SelectedIndexChanged += (s, e) => LoadMenu();

            // Cart button
            Button btnCart = new Button
            {
                Text = "🛒 View Cart",
                Location = new Point(470, 47),
                Size = new Size(140, 34),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            btnCart.FlatAppearance.BorderSize = 0;
            btnCart.Click += ShowCart;

            lblCartCount = new Label
            {
                Text = "0 items",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(76, 175, 80),
                Location = new Point(620, 55),
                AutoSize = true
            };

            toolbarPanel.Controls.AddRange(new Control[] { btnBack, lblTitle, txtSearch, cmbCategory, btnCart, lblCartCount });

            // ─── MENU PANEL (fills remaining space, scrollable, wraps items) ───
            menuPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true,
                BackColor = Color.Transparent,
                Padding = new Padding(8, 8, 8, 8)
            };

            // Add controls: menuPanel first (Fill), then toolbar (Top)
            // Dock order matters: Fill must be added BEFORE Top docked panels
            this.Controls.Add(menuPanel);
            this.Controls.Add(toolbarPanel);
        }

        private void LoadMenu()
        {
            menuPanel.SuspendLayout();
            menuPanel.Controls.Clear();

            using var conn = DatabaseHelper.GetConnection();
            string q = @"SELECT p.ProductId, p.ProductName, p.Description, p.Price, p.Stock, c.CategoryName
                         FROM Product p JOIN Category c ON p.CategoryId=c.CategoryId
                         WHERE p.ShopId=@sid AND p.Stock>0";
            if (!string.IsNullOrEmpty(txtSearch.Text)) q += " AND p.ProductName LIKE @s";
            if (cmbCategory.SelectedIndex > 0) q += " AND c.CategoryName=@cat";
            q += " ORDER BY c.CategoryName, p.ProductName";

            var cmd = new SqlCommand(q, conn);
            cmd.Parameters.AddWithValue("@sid", shopId);
            if (!string.IsNullOrEmpty(txtSearch.Text)) cmd.Parameters.AddWithValue("@s", $"%{txtSearch.Text}%");
            if (cmbCategory.SelectedIndex > 0) cmd.Parameters.AddWithValue("@cat", cmbCategory.SelectedItem!.ToString());

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                int pid = reader.GetInt32(0);
                string name = reader.GetString(1);
                string desc = reader.IsDBNull(2) ? "" : reader.GetString(2);
                decimal price = reader.GetDecimal(3);
                int stock = reader.GetInt32(4);
                string cat = reader.GetString(5);

                string emoji = cat switch
                {
                    "Hot Coffee" => "☕",
                    "Cold Coffee" => "🧊",
                    "Tea" => "🍵",
                    "Pastries" => "🥐",
                    "Snacks" => "🥪",
                    _ => "☕"
                };

                // ── Product Card ──
                Panel card = new Panel
                {
                    Size = new Size(360, 150),
                    Margin = new Padding(15),
                    BackColor = Color.White,
                    Cursor = Cursors.Hand
                };
                card.Paint += (s, e) =>
                {
                    using var pen = new Pen(Color.FromArgb(225, 215, 200), 1);
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                };

                // Accent strip on left
                Panel strip = new Panel
                {
                    Size = new Size(4, 150),
                    Location = new Point(0, 0),
                    BackColor = accentColor
                };

                Label le = new Label { Text = emoji, Font = new Font("Segoe UI", 28), Location = new Point(14, 10), Size = new Size(52, 52) };
                Label ln = new Label { Text = name, Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = primaryColor, Location = new Point(70, 10), Size = new Size(220, 24) };
                Label ld = new Label { Text = desc, Font = new Font("Segoe UI", 8.5f), ForeColor = Color.Gray, Location = new Point(70, 34), Size = new Size(270, 30) };
                Label lc = new Label { Text = cat, Font = new Font("Segoe UI", 8, FontStyle.Italic), ForeColor = Color.FromArgb(150, 130, 100), Location = new Point(70, 62), AutoSize = true };

                Label lp = new Label
                {
                    Text = $"৳{price:N0}",
                    Font = new Font("Segoe UI", 14, FontStyle.Bold),
                    ForeColor = Color.FromArgb(76, 175, 80),
                    Location = new Point(14, 90),
                    AutoSize = true
                };
                Label ls = new Label
                {
                    Text = $"Stock: {stock}",
                    Font = new Font("Segoe UI", 8),
                    ForeColor = stock > 10 ? Color.Green : Color.OrangeRed,
                    Location = new Point(14, 118),
                    AutoSize = true
                };

                NumericUpDown num = new NumericUpDown
                {
                    Minimum = 1,
                    Maximum = Math.Min(stock, 20),
                    Value = 1,
                    Location = new Point(190, 98),
                    Size = new Size(60, 28),
                    Font = new Font("Segoe UI", 9)
                };

                Button btnAdd = new Button
                {
                    Text = "➕ Add",
                    Location = new Point(260, 95),
                    Size = new Size(88, 34),
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    BackColor = accentColor,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand
                };
                btnAdd.FlatAppearance.BorderSize = 0;
                btnAdd.MouseEnter += (s, ev) => btnAdd.BackColor = ControlPaint.Light(accentColor, 0.15f);
                btnAdd.MouseLeave += (s, ev) => btnAdd.BackColor = accentColor;

                int id = pid;
                string n = name;
                btnAdd.Click += (s, ev) =>
                {
                    cart[id] = cart.GetValueOrDefault(id) + (int)num.Value;
                    UpdateCart();
                    MessageBox.Show($"Added {(int)num.Value}x {n} to cart! 🛒");
                };

                card.Controls.AddRange(new Control[] { strip, le, ln, ld, lc, lp, ls, num, btnAdd });
                menuPanel.Controls.Add(card);
            }
            menuPanel.ResumeLayout();
        }

        private void UpdateCart()
        {
            int c = 0;
            foreach (var v in cart.Values) c += v;
            lblCartCount.Text = $"{c} item{(c != 1 ? "s" : "")}";
        }

        private void ShowCart(object? sender, EventArgs e)
        {
            if (cart.Count == 0) { MessageBox.Show("Your cart is empty. ☕"); return; }

            Form f = new Form
            {
                Text = "🛒 Your Cart",
                Size = new Size(580, 530),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                BackColor = bgColor,
                Font = new Font("Segoe UI", 10)
            };

            Label lt = new Label { Text = $"🛒 Cart — {shopName}", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = primaryColor, Location = new Point(20, 15), AutoSize = true };

            DataGridView dgv = new DataGridView
            {
                Location = new Point(20, 55),
                Size = new Size(520, 260),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false
            };
            dgv.ColumnHeadersDefaultCellStyle.BackColor = primaryColor;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgv.EnableHeadersVisualStyles = false;

            var dt = new System.Data.DataTable();
            dt.Columns.Add("Product"); dt.Columns.Add("Qty", typeof(int));
            dt.Columns.Add("Price", typeof(decimal)); dt.Columns.Add("Subtotal", typeof(decimal));
            decimal total = 0;
            using (var conn = DatabaseHelper.GetConnection())
            {
                foreach (var item in cart)
                {
                    var cmd = new SqlCommand("SELECT ProductName, Price FROM Product WHERE ProductId=@pid", conn);
                    cmd.Parameters.AddWithValue("@pid", item.Key);
                    using var r = cmd.ExecuteReader();
                    if (r.Read())
                    {
                        decimal p = r.GetDecimal(1);
                        decimal sub = p * item.Value;
                        total += sub;
                        dt.Rows.Add(r.GetString(0), item.Value, p, sub);
                    }
                }
            }
            dgv.DataSource = dt;

            Label lTotal = new Label { Text = $"Total: ৳{total:N2}", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Color.FromArgb(76, 175, 80), Location = new Point(20, 325), AutoSize = true };

            Button btnPlace = new Button
            {
                Text = "✅ Place Order & Pay",
                Location = new Point(20, 370),
                Size = new Size(250, 45),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnPlace.FlatAppearance.BorderSize = 0;
            decimal t = total;
            btnPlace.Click += (s, ev) =>
            {
                int oid = PlaceOrder(t);
                f.Close();
                if (oid > 0) { var pf = new CheckoutForm(oid, userId); pf.ShowDialog(); }
            };

            Button btnClear = new Button
            {
                Text = "🗑 Clear Cart",
                Location = new Point(290, 370),
                Size = new Size(250, 45),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                BackColor = Color.FromArgb(244, 67, 54),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnClear.FlatAppearance.BorderSize = 0;
            btnClear.Click += (s, ev) => { cart.Clear(); UpdateCart(); f.Close(); };

            f.Controls.AddRange(new Control[] { lt, dgv, lTotal, btnPlace, btnClear });
            f.ShowDialog();
        }

        private int PlaceOrder(decimal total)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                using var tx = conn.BeginTransaction();
                var oc = new SqlCommand(
                    "INSERT INTO Orders (UserId,ShopId,TotalAmount,Status) OUTPUT INSERTED.OrderId VALUES (@uid,@sid,@t,'Pending')",
                    conn, tx);
                oc.Parameters.AddWithValue("@uid", userId);
                oc.Parameters.AddWithValue("@sid", shopId);
                oc.Parameters.AddWithValue("@t", total);
                int orderId = (int)oc.ExecuteScalar()!;

                foreach (var item in cart)
                {
                    var pc = new SqlCommand("SELECT Price FROM Product WHERE ProductId=@pid", conn, tx);
                    pc.Parameters.AddWithValue("@pid", item.Key);
                    decimal price = (decimal)pc.ExecuteScalar()!;

                    var dc = new SqlCommand(
                        "INSERT INTO OrderDetails (OrderId,ProductId,Quantity,UnitPrice,Subtotal) VALUES (@oid,@pid,@q,@p,@s)",
                        conn, tx);
                    dc.Parameters.AddWithValue("@oid", orderId);
                    dc.Parameters.AddWithValue("@pid", item.Key);
                    dc.Parameters.AddWithValue("@q", item.Value);
                    dc.Parameters.AddWithValue("@p", price);
                    dc.Parameters.AddWithValue("@s", price * item.Value);
                    dc.ExecuteNonQuery();

                    var sc = new SqlCommand("UPDATE Product SET Stock=Stock-@q WHERE ProductId=@pid", conn, tx);
                    sc.Parameters.AddWithValue("@q", item.Value);
                    sc.Parameters.AddWithValue("@pid", item.Key);
                    sc.ExecuteNonQuery();
                }
                tx.Commit();
                cart.Clear();
                UpdateCart();
                LoadMenu();
                MessageBox.Show($"Order #{orderId} placed! ☕\nTotal: ৳{total:N2}", "Order Placed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return orderId;
            }
            catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}"); return 0; }
        }
    }
}
