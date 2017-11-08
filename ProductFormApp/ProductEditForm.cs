using ProductFormApp.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProductFormApp
{
    public partial class ProductEditForm : Form
    {
        private ProdContext db;
        public ProductEditForm()
        {
            InitializeComponent();
        }
        

        private void categoryDataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            zapiszToolStripButton.Enabled = true;
        }

        private void categoryDataGridView_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            zapiszToolStripButton.Enabled = true;
        }

        private void productDataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            zapiszToolStripButton.Enabled = true;
        }

        private void productDataGridView_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            zapiszToolStripButton.Enabled = true;
        }

        private void zapiszToolStripButton_Click(object sender, EventArgs e)
        {
            bool inputError = false;
            foreach (DbEntityValidationResult err in db.GetValidationErrors())
            {
                inputError = true;
                foreach (DbValidationError errMsg in err.ValidationErrors)
                    MessageBox.Show(errMsg.ErrorMessage, "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            if (inputError)
                return;

            zapiszToolStripButton.Enabled = false;
            db.SaveChanges();
            categoryDataGridView.Update();
            categoryDataGridView.Refresh();
            productDataGridView.Update();
            productDataGridView.Refresh();
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            db = new ProdContext();
            db.Categories.Load();
            this.categoryBindingSource.DataSource = db.Categories.Local.ToBindingList();
            db.Products.Load();
            this.productBindingSource.DataSource = db.Products.Local.ToBindingList();
            productBindingSource.AddingNew += new AddingNewEventHandler(product_AddingNew);
            productBindingSource.AllowNew = true;
            productDataGridView.RowValidated += new DataGridViewCellEventHandler(product_Validated);
        }

        private void product_Validated(object sender, DataGridViewCellEventArgs e)
        {
            if ((productDataGridView.DataSource as BindingSource).List.Count > e.RowIndex &&
                (productDataGridView.DataSource as BindingSource).List[0] is Product)
            {
                db.Products.Local.Add((productDataGridView.DataSource as BindingSource).List[e.RowIndex] as Product);
            }
        }

        private void MainWindow_Shown(object sender, EventArgs e)
        {
            zapiszToolStripButton.Enabled = false;
        }

        private void categoryDataGridView_SelectionChanged(object sender, EventArgs e)
        {
            showProducts();
        }

        private int defaultCategory = 0;
        private void showProducts()
        {
            bool changesBefore = zapiszToolStripButton.Enabled;
            List<int> selectedCategories = new List<int>();
            foreach (DataGridViewCell c in categoryDataGridView.SelectedCells)
            {
                int cat;
                if (Int32.TryParse(categoryDataGridView.Rows[c.RowIndex].Cells[0].FormattedValue.ToString(), out cat))
                {
                    selectedCategories.Add(cat);
                    defaultCategory = cat;
                }
            }
            productBindingSource.DataSource = db.Products.Local.ToBindingList().Where(p => selectedCategories.Contains(p.CategoryId));
            if(db.Products.Local.ToBindingList().Where(p => selectedCategories.Contains(p.CategoryId)).ToList().Count == 0)
            {
                productBindingSource.DataSource = new BindingList<Product>();
            }

            zapiszToolStripButton.Enabled = changesBefore;
        }
        
        private void product_AddingNew(object sender, AddingNewEventArgs e)
        {
            e.NewObject = new Product();
        }

        private void ProductEditForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (zapiszToolStripButton.Enabled)
            {
                var result = MessageBox.Show("Zmiany nie zostały zapisane. Zapisać je przed zamknięciem?", "Unsaved Changes",
                             MessageBoxButtons.YesNoCancel,
                             MessageBoxIcon.Question);

                if(result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                if(result == DialogResult.Yes)
                {
                    zapiszToolStripButton_Click(sender, e);
                }
            }
            categoryBindingSource.CancelEdit();
            productBindingSource.CancelEdit();
        }

        private void productDataGridView_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.ColumnIndex == 3)
            {
                int catId;
                if (Int32.TryParse(e.FormattedValue.ToString(), out catId)) {
                    if((from c in db.Categories.Local where c.CategoryId == catId select c).Any())
                    {
                        return;
                    }
                    productDataGridView.Rows[e.RowIndex].ErrorText = "Nie znaleziono kategorii o id " + catId;
                    e.Cancel = true;
                    return;
                }
                productDataGridView.Rows[e.RowIndex].ErrorText = "CategoryId musi być liczbą.";
                e.Cancel = true;
                return;
            }
            if(e.ColumnIndex == 1)
            {
                if(db.Products.Local.Where(p => p.Name == e.FormattedValue.ToString() 
                   && p.ProductId != Int32.Parse(productDataGridView.Rows[e.RowIndex].Cells[0].Value.ToString())).Any())
                {
                    productDataGridView.Rows[e.RowIndex].ErrorText = "Nazwa musi być unikatowa.";
                    e.Cancel = true;
                    return;
                }
            }
        }

        private void productDataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            productDataGridView.Rows[e.RowIndex].ErrorText = String.Empty;
        }

        private void productDataGridView_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            productDataGridView.Rows[e.RowIndex].ErrorText = e.Exception.Message;
        }

        private void productDataGridView_DefaultValuesNeeded(object sender, DataGridViewRowEventArgs e)
        {
            e.Row.Cells[3].Value = defaultCategory != 0 ? defaultCategory : db.Categories.Local.First().CategoryId;
        }

        private void categoryDataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            categoryDataGridView.Rows[e.RowIndex].ErrorText = String.Empty;
        }

        private void categoryDataGridView_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.ColumnIndex == 1)
            {
                if (db.Categories.Local.Where(c => c.Name == e.FormattedValue.ToString()
                    && c.CategoryId != Int32.Parse(categoryDataGridView.Rows[e.RowIndex].Cells[0].Value.ToString())).Any())
                {
                    categoryDataGridView.Rows[e.RowIndex].ErrorText = "Nazwa musi być unikatowa.";
                    e.Cancel = true;
                    return;
                }
            }
        }
    }
}
