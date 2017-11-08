using ProductFormApp.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProductFormApp
{
    public partial class AddOrder : Form
    {
        class SelectedProduct
        {
            public string name { get; set; }
            public int id { get; set; }
            public SelectedProduct(string name, int id)
            {
                this.name = name;
                this.id = id;
            }
        }

        private ProdContext db;
        BindingSource selectedProductsSource;
        public AddOrder()
        {
            this.db = new ProdContext();
            InitializeComponent();
        }

        private void AddOrder_Load(object sender, EventArgs e)
        {
            db.Customers.Load();
            db.Categories.Load();
            db.Products.Load();
            db.Orders.Load();
            customerBindingSource.DataSource = db.Customers.Local.ToBindingList();
            categoryBindingSource.DataSource = db.Categories.Local.ToBindingList();

            var bindingList = new BindingList<SelectedProduct>();
            selectedProductsSource = new BindingSource(bindingList, null);
            dataGridView1.DataSource = selectedProductsSource;

        }

        private void categoryDataGridView_SelectionChanged(object sender, EventArgs e)
        {
            showProducts();
        }

        private void showProducts()
        {
            List<int> selectedCategories = new List<int>();
            foreach (DataGridViewCell c in categoryDataGridView.SelectedCells)
            {
                int cat;
                if (Int32.TryParse(categoryDataGridView.Rows[c.RowIndex].Cells[0].FormattedValue.ToString(), out cat))
                {
                    selectedCategories.Add(cat);
                }
            }
            productBindingSource.DataSource = db.Products.Local.ToBindingList().Where(p => selectedCategories.Contains(p.CategoryId));
            if (db.Products.Local.ToBindingList().Where(p => selectedCategories.Contains(p.CategoryId)).ToList().Count == 0)
            {
                productBindingSource.DataSource = new BindingList<Product>();
            }
            
        }

        private string selectedCustomer = null;
        private void customerDataGridView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if(customerDataGridView.SelectedCells.Count > 0)
            {
                selectedCustomer = customerDataGridView.Rows[customerDataGridView.SelectedCells[0].RowIndex].Cells[0].FormattedValue.ToString();
                SelectedCustomer.Text = selectedCustomer;
            }
        }
        

        private void productDataGridView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (productDataGridView.SelectedCells.Count > 0)
            {
                string selectedProductName = productDataGridView.Rows[productDataGridView.SelectedCells[0].RowIndex].Cells[1].FormattedValue.ToString();
                int selectedProductId = Int32.Parse(productDataGridView.Rows[productDataGridView.SelectedCells[0].RowIndex].Cells[0].FormattedValue.ToString());
                selectedProductsSource.Add(new SelectedProduct(selectedProductName, selectedProductId));
            }
        }

        private void zapiszToolStripButton_Click(object sender, EventArgs e)
        {
            if(selectedCustomer == null)
            {
                string msg = "Klient nie został wybrany.";
                MessageBox.Show(msg, "Input Error", MessageBoxButtons.OK);
                return;
            }
            if(selectedProductsSource.Count <= 0)
            {
                string msg = "Nie wybrano produktów.";
                MessageBox.Show(msg, "Input Error", MessageBoxButtons.OK);
                return;
            }


            var Customer = (from customer in db.Customers.Local
                            where customer.CompanyName == selectedCustomer
                            select customer).First();

            foreach (SelectedProduct p in selectedProductsSource)
            {
                var o = new Order();
                o.Customer = Customer;
                var Product = from product in db.Products.Local
                              where product.ProductId == p.id
                              select product;
                o.Product = Product.First();
                
                db.Orders.Local.Add(o);
            }

            db.SaveChanges();
            this.Close();
        }

        private void dataGridView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if(dataGridView1.SelectedCells.Count > 0)
            {
                selectedProductsSource.RemoveAt(dataGridView1.SelectedCells[0].RowIndex);
            }
        }
    }
}
