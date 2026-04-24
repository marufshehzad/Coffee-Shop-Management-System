using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;

namespace CoffeeShopManagement.Forms
{
    public class ProductManageForm : Form
    {
        private int productId;
        private TextBox txtName, txtDescription, txtPrice, txtStock;
        private ComboBox cmbCategory;

        private readonly Color primaryColor = Color.FromArgb(101, 67, 33);
        private readonly Color bgColor = Color.FromArgb(245, 235, 220);
        private readonly Color accentColor = Color.FromArgb(255, 183, 77);
        private readonly Color darkBg = Color.FromArgb(62, 39, 18);

        public ProductManageForm(int productId)
        {
            this.productId = productId;
            InitializeComponent();
            if (productId > 0) LoadProduct();
        }

        private void InitializeComponent()
        {
            this.Text = productId > 0 ? "✏️ Edit Product" : "➕ Add New Product";
            this.Size = new Size(450, 480);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = bgColor;
            this.Font = new Font("Segoe UI", 10);

            // Header
            Panel header = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = darkBg };
            header.Paint += (s, e) =>
            {
                using var brush = new LinearGradientBrush(header.ClientRectangle, darkBg, primaryColor, 45f);
                e.Graphics.FillRectangle(brush, header.ClientRectangle);
            };
            Label lblTitle = new Label
            {
                Text = productId > 0 ? "✏️ Edit Product" : "➕ Add New Product",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = accentColor,
                Size = new Size(430, 40),
                Location = new Point(10, 10),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            header.Controls.Add(lblTitle);

            int y = 80;

            // Category
            Label lblCat = new Label { Text = "Category:", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(80, 50, 20), Location = new Point(30, y), AutoSize = true };
            cmbCategory = new ComboBox
            {
                Location = new Point(30, y + 20),
                Size = new Size(370, 30),
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            using (var conn = DatabaseHelper.GetConnection())
            {
                var cmd = new SqliteCommand("SELECT CategoryId, CategoryName FROM Category", conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    cmbCategory.Items.Add($"{reader.GetInt32(0)}|{reader.GetString(1)}");
            }
            if (cmbCategory.Items.Count > 0) cmbCategory.SelectedIndex = 0;

            y += 55;

            // Name
            Label lblName = new Label { Text = "Product Name:", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(80, 50, 20), Location = new Point(30, y), AutoSize = true };
            txtName = new TextBox { Location = new Point(30, y + 20), Size = new Size(370, 28), Font = new Font("Segoe UI", 10) };
            y += 55;

            // Description
            Label lblDesc = new Label { Text = "Description:", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(80, 50, 20), Location = new Point(30, y), AutoSize = true };
            txtDescription = new TextBox { Location = new Point(30, y + 20), Size = new Size(370, 28), Font = new Font("Segoe UI", 10) };
            y += 55;

            // Price and Stock side by side
            Label lblPrice = new Label { Text = "Price (৳):", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(80, 50, 20), Location = new Point(30, y), AutoSize = true };
            txtPrice = new TextBox { Location = new Point(30, y + 20), Size = new Size(170, 28), Font = new Font("Segoe UI", 10) };

            Label lblStock = new Label { Text = "Stock:", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(80, 50, 20), Location = new Point(220, y), AutoSize = true };
            txtStock = new TextBox { Location = new Point(220, y + 20), Size = new Size(180, 28), Font = new Font("Segoe UI", 10) };
            y += 60;

            // Save button
            Button btnSave = new Button
            {
                Text = "💾 Save Product",
                Location = new Point(30, y + 10),
                Size = new Size(370, 42),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                BackColor = primaryColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;

            this.Controls.AddRange(new Control[] {
                header, lblCat, cmbCategory, lblName, txtName,
                lblDesc, txtDescription, lblPrice, txtPrice,
                lblStock, txtStock, btnSave
            });
        }

        private void LoadProduct()
        {
            using var conn = DatabaseHelper.GetConnection();
            var cmd = new SqliteCommand("SELECT CategoryId, ProductName, Description, Price, Stock FROM Product WHERE ProductId=@id", conn);
            cmd.Parameters.AddWithValue("@id", productId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                int catId = reader.GetInt32(0);
                for (int i = 0; i < cmbCategory.Items.Count; i++)
                {
                    if (cmbCategory.Items[i]!.ToString()!.StartsWith($"{catId}|"))
                    {
                        cmbCategory.SelectedIndex = i;
                        break;
                    }
                }

                txtName.Text = reader.GetString(1);
                txtDescription.Text = reader.IsDBNull(2) ? "" : reader.GetString(2);
                txtPrice.Text = reader.GetDecimal(3).ToString();
                txtStock.Text = reader.GetInt32(4).ToString();
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Product name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(txtPrice.Text, out decimal price) || price <= 0)
            {
                MessageBox.Show("Please enter a valid price.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtStock.Text, out int stock) || stock < 0)
            {
                MessageBox.Show("Please enter a valid stock quantity.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int categoryId = int.Parse(cmbCategory.SelectedItem!.ToString()!.Split('|')[0]);

            try
            {
                using var conn = DatabaseHelper.GetConnection();

                if (productId > 0)
                {
                    var cmd = new SqliteCommand(@"
                        UPDATE Product SET CategoryId=@cid, ProductName=@name, Description=@desc, Price=@price, Stock=@stock
                        WHERE ProductId=@pid", conn);
                    cmd.Parameters.AddWithValue("@cid", categoryId);
                    cmd.Parameters.AddWithValue("@name", txtName.Text.Trim());
                    cmd.Parameters.AddWithValue("@desc", txtDescription.Text.Trim());
                    cmd.Parameters.AddWithValue("@price", price);
                    cmd.Parameters.AddWithValue("@stock", stock);
                    cmd.Parameters.AddWithValue("@pid", productId);
                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Product updated successfully! ✅", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    var cmd = new SqliteCommand(@"
                        INSERT INTO Product (CategoryId, ProductName, Description, Price, Stock)
                        VALUES (@cid, @name, @desc, @price, @stock)", conn);
                    cmd.Parameters.AddWithValue("@cid", categoryId);
                    cmd.Parameters.AddWithValue("@name", txtName.Text.Trim());
                    cmd.Parameters.AddWithValue("@desc", txtDescription.Text.Trim());
                    cmd.Parameters.AddWithValue("@price", price);
                    cmd.Parameters.AddWithValue("@stock", stock);
                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Product added successfully! ✅", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving product: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
