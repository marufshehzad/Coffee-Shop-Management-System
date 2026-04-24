using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;

namespace CoffeeShopManagement.Forms
{
    public class MenuBrowseForm : Form
    {
        private int userId;
        private FlowLayoutPanel menuPanel;
        private ComboBox cmbCategory;
        private TextBox txtSearch;
        private Dictionary<int, int> cart = new Dictionary<int, int>(); // productId -> quantity
        private Label lblCartCount;

        private readonly Color primaryColor = Color.FromArgb(101, 67, 33);
        private readonly Color bgColor = Color.FromArgb(245, 235, 220);
        private readonly Color accentColor = Color.FromArgb(255, 183, 77);

        public MenuBrowseForm(int userId)
        {
            this.userId = userId;
            InitializeComponent();
            LoadMenu();
        }

        private void InitializeComponent()
        {
            this.Text = "Browse Menu";
            this.BackColor = bgColor;
            this.Font = new Font("Segoe UI", 10);
            this.Size = new Size(760, 600);

            // Title
            Label lblTitle = new Label
            {
                Text = "📋 Coffee Menu",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = primaryColor,
                Location = new Point(20, 10),
                AutoSize = true
            };

            // Search and filter bar
            Label lblSearch = new Label
            {
                Text = "🔍",
                Font = new Font("Segoe UI", 14),
                Location = new Point(20, 55),
                AutoSize = true
            };

            txtSearch = new TextBox
            {
                Location = new Point(50, 55),
                Size = new Size(250, 30),
                Font = new Font("Segoe UI", 10),
                PlaceholderText = "Search products..."
            };
            txtSearch.TextChanged += (s, e) => LoadMenu();

            Label lblCategory = new Label
            {
                Text = "Category:",
                Font = new Font("Segoe UI", 10),
                Location = new Point(320, 58),
                AutoSize = true
            };

            cmbCategory = new ComboBox
            {
                Location = new Point(400, 55),
                Size = new Size(180, 30),
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbCategory.Items.Add("All Categories");
            using (var conn = DatabaseHelper.GetConnection())
            {
                var cmd = new SqliteCommand("SELECT CategoryName FROM Category", conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    cmbCategory.Items.Add(reader.GetString(0));
            }
            cmbCategory.SelectedIndex = 0;
            cmbCategory.SelectedIndexChanged += (s, e) => LoadMenu();

            // Cart button
            Button btnCart = new Button
            {
                Text = "🛒 View Cart",
                Location = new Point(600, 52),
                Size = new Size(130, 35),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCart.FlatAppearance.BorderSize = 0;
            btnCart.Click += BtnViewCart_Click;

            lblCartCount = new Label
            {
                Text = "0 items",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(76, 175, 80),
                Location = new Point(635, 88),
                AutoSize = true
            };

            // Menu items panel (scrollable)
            menuPanel = new FlowLayoutPanel
            {
                Location = new Point(20, 105),
                Size = new Size(720, 450),
                AutoScroll = true,
                WrapContents = true,
                BackColor = bgColor
            };

            this.Controls.AddRange(new Control[] {
                lblTitle, lblSearch, txtSearch, lblCategory, cmbCategory,
                btnCart, lblCartCount, menuPanel
            });
        }

        private void LoadMenu()
        {
            menuPanel.Controls.Clear();

            using var conn = DatabaseHelper.GetConnection();
            string query = @"
                SELECT p.ProductId, p.ProductName, p.Description, p.Price, p.Stock, c.CategoryName
                FROM Product p
                JOIN Category c ON p.CategoryId = c.CategoryId
                WHERE p.Stock > 0";

            if (!string.IsNullOrEmpty(txtSearch.Text))
                query += " AND p.ProductName LIKE @search";

            if (cmbCategory.SelectedIndex > 0)
                query += " AND c.CategoryName = @cat";

            query += " ORDER BY c.CategoryName, p.ProductName";

            var cmd = new SqliteCommand(query, conn);
            if (!string.IsNullOrEmpty(txtSearch.Text))
                cmd.Parameters.AddWithValue("@search", $"%{txtSearch.Text}%");
            if (cmbCategory.SelectedIndex > 0)
                cmd.Parameters.AddWithValue("@cat", cmbCategory.SelectedItem!.ToString());

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                int prodId = reader.GetInt32(0);
                string name = reader.GetString(1);
                string desc = reader.IsDBNull(2) ? "" : reader.GetString(2);
                decimal price = reader.GetDecimal(3);
                int stock = reader.GetInt32(4);
                string category = reader.GetString(5);

                Panel card = CreateMenuCard(prodId, name, desc, price, stock, category);
                menuPanel.Controls.Add(card);
            }
        }

        private Panel CreateMenuCard(int productId, string name, string desc, decimal price, int stock, string category)
        {
            Panel card = new Panel
            {
                Size = new Size(340, 150),
                Margin = new Padding(8),
                BackColor = Color.White,
                Cursor = Cursors.Hand
            };
            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var pen = new Pen(Color.FromArgb(220, 210, 195), 1);
                e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };

            // Coffee icon
            string emoji = category switch
            {
                "Hot Coffee" => "☕",
                "Cold Coffee" => "🧊",
                "Tea" => "🍵",
                "Pastries" => "🥐",
                "Snacks" => "🥪",
                _ => "☕"
            };

            Label lblEmoji = new Label
            {
                Text = emoji,
                Font = new Font("Segoe UI", 28),
                Location = new Point(10, 15),
                Size = new Size(60, 60),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label lblName = new Label
            {
                Text = name,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = primaryColor,
                Location = new Point(75, 10),
                Size = new Size(200, 25)
            };

            Label lblDesc = new Label
            {
                Text = desc,
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.Gray,
                Location = new Point(75, 35),
                Size = new Size(200, 30)
            };

            Label lblCategory = new Label
            {
                Text = category,
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                ForeColor = Color.FromArgb(150, 130, 100),
                Location = new Point(75, 62),
                AutoSize = true
            };

            Label lblPrice = new Label
            {
                Text = $"৳{price:N2}",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(76, 175, 80),
                Location = new Point(15, 95),
                AutoSize = true
            };

            Label lblStock = new Label
            {
                Text = $"In stock: {stock}",
                Font = new Font("Segoe UI", 8),
                ForeColor = stock > 10 ? Color.Green : Color.OrangeRed,
                Location = new Point(15, 125),
                AutoSize = true
            };

            // Quantity controls
            NumericUpDown numQty = new NumericUpDown
            {
                Minimum = 1,
                Maximum = Math.Min(stock, 20),
                Value = 1,
                Location = new Point(180, 100),
                Size = new Size(60, 28),
                Font = new Font("Segoe UI", 9)
            };

            Button btnAdd = new Button
            {
                Text = "➕ Add",
                Location = new Point(250, 97),
                Size = new Size(80, 32),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = accentColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += (s, ev) =>
            {
                int qty = (int)numQty.Value;
                if (cart.ContainsKey(productId))
                    cart[productId] += qty;
                else
                    cart[productId] = qty;

                UpdateCartCount();
                MessageBox.Show($"Added {qty}x {name} to cart! 🛒", "Added to Cart",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            card.Controls.AddRange(new Control[] {
                lblEmoji, lblName, lblDesc, lblCategory, lblPrice, lblStock, numQty, btnAdd
            });

            return card;
        }

        private void UpdateCartCount()
        {
            int count = 0;
            foreach (var qty in cart.Values) count += qty;
            lblCartCount.Text = $"{count} item{(count != 1 ? "s" : "")}";
        }

        private void BtnViewCart_Click(object? sender, EventArgs e)
        {
            if (cart.Count == 0)
            {
                MessageBox.Show("Your cart is empty. Add some items first! ☕",
                    "Empty Cart", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Show cart summary
            Form cartForm = new Form
            {
                Text = "🛒 Your Cart",
                Size = new Size(500, 500),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                BackColor = bgColor,
                Font = new Font("Segoe UI", 10)
            };

            Label lblCartTitle = new Label
            {
                Text = "🛒 Cart Summary",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = primaryColor,
                Location = new Point(20, 15),
                AutoSize = true
            };

            DataGridView dgv = new DataGridView
            {
                Location = new Point(20, 55),
                Size = new Size(440, 250),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 9)
            };
            dgv.ColumnHeadersDefaultCellStyle.BackColor = primaryColor;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.EnableHeadersVisualStyles = false;

            DataTable dt = new DataTable();
            dt.Columns.Add("Product", typeof(string));
            dt.Columns.Add("Qty", typeof(int));
            dt.Columns.Add("Price", typeof(decimal));
            dt.Columns.Add("Subtotal", typeof(decimal));

            decimal total = 0;
            using (var conn = DatabaseHelper.GetConnection())
            {
                foreach (var item in cart)
                {
                    var cmd = new SqliteCommand("SELECT ProductName, Price FROM Product WHERE ProductId=@pid", conn);
                    cmd.Parameters.AddWithValue("@pid", item.Key);
                    using var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        string pName = reader.GetString(0);
                        decimal pPrice = reader.GetDecimal(1);
                        decimal subtotal = pPrice * item.Value;
                        total += subtotal;
                        dt.Rows.Add(pName, item.Value, pPrice, subtotal);
                    }
                }
            }

            dgv.DataSource = dt;

            Label lblTotal = new Label
            {
                Text = $"Total: ৳{total:N2}",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(76, 175, 80),
                Location = new Point(20, 320),
                AutoSize = true
            };

            Button btnPlaceOrder = new Button
            {
                Text = "✅ Place Order",
                Location = new Point(20, 360),
                Size = new Size(210, 42),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnPlaceOrder.FlatAppearance.BorderSize = 0;
            btnPlaceOrder.Click += (s, ev) =>
            {
                PlaceOrder(total);
                cartForm.Close();
            };

            Button btnClear = new Button
            {
                Text = "🗑 Clear Cart",
                Location = new Point(250, 360),
                Size = new Size(210, 42),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                BackColor = Color.FromArgb(244, 67, 54),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnClear.FlatAppearance.BorderSize = 0;
            btnClear.Click += (s, ev) =>
            {
                cart.Clear();
                UpdateCartCount();
                cartForm.Close();
                MessageBox.Show("Cart cleared.", "Cart", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            cartForm.Controls.AddRange(new Control[] { lblCartTitle, dgv, lblTotal, btnPlaceOrder, btnClear });
            cartForm.ShowDialog();
        }

        private void PlaceOrder(decimal total)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                using var transaction = conn.BeginTransaction();

                // Create order
                var orderCmd = new SqliteCommand(@"
                    INSERT INTO Orders (UserId, TotalAmount, Status) VALUES (@uid, @total, 'Pending');
                    SELECT last_insert_rowid();", conn, transaction);
                orderCmd.Parameters.AddWithValue("@uid", userId);
                orderCmd.Parameters.AddWithValue("@total", total);
                int orderId = Convert.ToInt32(orderCmd.ExecuteScalar());

                // Insert order details and update stock
                foreach (var item in cart)
                {
                    var priceCmd = new SqliteCommand("SELECT Price FROM Product WHERE ProductId=@pid", conn, transaction);
                    priceCmd.Parameters.AddWithValue("@pid", item.Key);
                    decimal price = Convert.ToDecimal(priceCmd.ExecuteScalar());

                    var detailCmd = new SqliteCommand(@"
                        INSERT INTO OrderDetails (OrderId, ProductId, Quantity, UnitPrice, Subtotal)
                        VALUES (@oid, @pid, @qty, @price, @sub)", conn, transaction);
                    detailCmd.Parameters.AddWithValue("@oid", orderId);
                    detailCmd.Parameters.AddWithValue("@pid", item.Key);
                    detailCmd.Parameters.AddWithValue("@qty", item.Value);
                    detailCmd.Parameters.AddWithValue("@price", price);
                    detailCmd.Parameters.AddWithValue("@sub", price * item.Value);
                    detailCmd.ExecuteNonQuery();

                    // Update stock
                    var stockCmd = new SqliteCommand(
                        "UPDATE Product SET Stock = Stock - @qty WHERE ProductId=@pid", conn, transaction);
                    stockCmd.Parameters.AddWithValue("@qty", item.Value);
                    stockCmd.Parameters.AddWithValue("@pid", item.Key);
                    stockCmd.ExecuteNonQuery();
                }

                transaction.Commit();

                cart.Clear();
                UpdateCartCount();
                LoadMenu();

                MessageBox.Show($"Order #{orderId} placed successfully! ☕\nTotal: ৳{total:N2}",
                    "Order Placed", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error placing order: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
