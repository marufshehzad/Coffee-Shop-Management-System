using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace CoffeeShopManagement.Forms
{
    public class ProductManageForm : Form
    {
        private int productId, shopId;
        private TextBox txtName, txtDescription, txtPrice, txtStock;
        private ComboBox cmbCategory;
        private readonly Color primaryColor = Color.FromArgb(101, 67, 33);
        private readonly Color bgColor = Color.FromArgb(250, 243, 232);
        private readonly Color accentColor = Color.FromArgb(255, 183, 77);
        private readonly Color darkBg = Color.FromArgb(62, 39, 18);

        public ProductManageForm(int productId, int shopId)
        {
            this.productId = productId; this.shopId = shopId;
            InitializeComponent(); if (productId > 0) LoadProduct();
        }

        private void InitializeComponent()
        {
            this.Text = productId > 0 ? "✏️ Edit Product" : "➕ Add New Product";
            this.Size = new Size(450, 480); this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog; this.MaximizeBox = false;
            this.BackColor = bgColor; this.Font = new Font("Segoe UI", 10);

            Panel header = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = darkBg };
            header.Paint += (s, e) => { using var b = new LinearGradientBrush(header.ClientRectangle, darkBg, primaryColor, 45f); e.Graphics.FillRectangle(b, header.ClientRectangle); };
            header.Controls.Add(new Label { Text = productId > 0 ? "✏️ Edit Product" : "➕ Add New Product", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = accentColor, Size = new Size(430, 40), Location = new Point(10, 10), TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent });

            int y = 80;
            Label lc = new Label { Text = "Category:", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(80, 50, 20), Location = new Point(30, y), AutoSize = true };
            cmbCategory = new ComboBox { Location = new Point(30, y + 20), Size = new Size(370, 30), Font = new Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList };
            using (var conn = DatabaseHelper.GetConnection()) { using var r = new SqlCommand("SELECT CategoryId, CategoryName FROM Category", conn).ExecuteReader(); while (r.Read()) cmbCategory.Items.Add($"{r.GetInt32(0)}|{r.GetString(1)}"); }
            if (cmbCategory.Items.Count > 0) cmbCategory.SelectedIndex = 0;
            y += 55;

            Label ln = new Label { Text = "Product Name:", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(80, 50, 20), Location = new Point(30, y), AutoSize = true };
            txtName = new TextBox { Location = new Point(30, y + 20), Size = new Size(370, 28), Font = new Font("Segoe UI", 10) }; y += 55;
            Label ld = new Label { Text = "Description:", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(80, 50, 20), Location = new Point(30, y), AutoSize = true };
            txtDescription = new TextBox { Location = new Point(30, y + 20), Size = new Size(370, 28), Font = new Font("Segoe UI", 10) }; y += 55;
            Label lp = new Label { Text = "Price (৳):", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(80, 50, 20), Location = new Point(30, y), AutoSize = true };
            txtPrice = new TextBox { Location = new Point(30, y + 20), Size = new Size(170, 28), Font = new Font("Segoe UI", 10) };
            Label ls = new Label { Text = "Stock:", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(80, 50, 20), Location = new Point(220, y), AutoSize = true };
            txtStock = new TextBox { Location = new Point(220, y + 20), Size = new Size(180, 28), Font = new Font("Segoe UI", 10) }; y += 60;

            Button btnSave = new Button { Text = "💾 Save Product", Location = new Point(30, y + 10), Size = new Size(370, 42), Font = new Font("Segoe UI", 12, FontStyle.Bold), BackColor = primaryColor, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnSave.FlatAppearance.BorderSize = 0; btnSave.Click += BtnSave_Click;

            this.Controls.AddRange(new Control[] { header, lc, cmbCategory, ln, txtName, ld, txtDescription, lp, txtPrice, ls, txtStock, btnSave });
        }

        private void LoadProduct()
        {
            using var conn = DatabaseHelper.GetConnection();
            var cmd = new SqlCommand("SELECT CategoryId, ProductName, Description, Price, Stock FROM Product WHERE ProductId=@id", conn);
            cmd.Parameters.AddWithValue("@id", productId);
            using var r = cmd.ExecuteReader();
            if (r.Read()) {
                int catId = r.GetInt32(0);
                for (int i = 0; i < cmbCategory.Items.Count; i++) { if (cmbCategory.Items[i]!.ToString()!.StartsWith($"{catId}|")) { cmbCategory.SelectedIndex = i; break; } }
                txtName.Text = r.GetString(1); txtDescription.Text = r.IsDBNull(2) ? "" : r.GetString(2);
                txtPrice.Text = r.GetDecimal(3).ToString(); txtStock.Text = r.GetInt32(4).ToString();
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text)) { MessageBox.Show("Name required."); return; }
            if (!decimal.TryParse(txtPrice.Text, out decimal price) || price <= 0) { MessageBox.Show("Valid price required."); return; }
            if (!int.TryParse(txtStock.Text, out int stock) || stock < 0) { MessageBox.Show("Valid stock required."); return; }
            int catId = int.Parse(cmbCategory.SelectedItem!.ToString()!.Split('|')[0]);

            using var conn = DatabaseHelper.GetConnection();
            if (productId > 0) {
                var cmd = new SqlCommand("UPDATE Product SET CategoryId=@cid,ProductName=@n,Description=@d,Price=@p,Stock=@s WHERE ProductId=@pid", conn);
                cmd.Parameters.AddWithValue("@cid", catId); cmd.Parameters.AddWithValue("@n", txtName.Text.Trim());
                cmd.Parameters.AddWithValue("@d", txtDescription.Text.Trim()); cmd.Parameters.AddWithValue("@p", price);
                cmd.Parameters.AddWithValue("@s", stock); cmd.Parameters.AddWithValue("@pid", productId); cmd.ExecuteNonQuery();
            } else {
                var cmd = new SqlCommand("INSERT INTO Product (ShopId,CategoryId,ProductName,Description,Price,Stock) VALUES (@sid,@cid,@n,@d,@p,@s)", conn);
                cmd.Parameters.AddWithValue("@sid", shopId); cmd.Parameters.AddWithValue("@cid", catId);
                cmd.Parameters.AddWithValue("@n", txtName.Text.Trim()); cmd.Parameters.AddWithValue("@d", txtDescription.Text.Trim());
                cmd.Parameters.AddWithValue("@p", price); cmd.Parameters.AddWithValue("@s", stock); cmd.ExecuteNonQuery();
            }
            MessageBox.Show("Product saved! ✅"); this.Close();
        }
    }
}
