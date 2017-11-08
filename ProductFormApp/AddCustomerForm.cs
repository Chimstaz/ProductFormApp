using ProductFormApp.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProductFormApp
{
    public partial class AddCustomerForm : Form
    {
        private ProdContext db;
      
        public AddCustomerForm()
        {
            this.db = new ProdContext();
            InitializeComponent();
        }

        private void customerBindingNavigatorSaveItem_Click(object sender, EventArgs e)
        {
            bool inputError = false;
            foreach(DbEntityValidationResult err in db.GetValidationErrors())
            {
                inputError = true;
                foreach(DbValidationError errMsg in err.ValidationErrors)
                    MessageBox.Show(errMsg.ErrorMessage, "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            if (inputError)
                return;
            customerBindingNavigatorSaveItem.Enabled = false;
            this.db.SaveChanges();
            customerDataGridView.Update();
            customerDataGridView.Refresh();
            orderDataGridView.Update();
            orderDataGridView.Refresh();
        }

        private void AddCustomerForm_Load(object sender, EventArgs e)
        {
            db.Customers.Load();
            db.Orders.Load();
            db.Products.Load();
            customerBindingSource.DataSource = db.Customers.Local.ToBindingList();
            orderBindingSource.ListChanged += OrderBindingSource_ListChanged;
        }

        private List<Order> prevList;
        private void OrderBindingSource_ListChanged(object sender, ListChangedEventArgs e)
        {
            if(e.ListChangedType == ListChangedType.ItemDeleted && prevList.Count > e.NewIndex)
            {
                customerBindingNavigatorSaveItem.Enabled = true;
                db.Orders.Local.Remove(prevList[e.NewIndex]);
            }
            prevList = new List<Order>();
            foreach(var o in orderBindingSource.List)
            {
                if(o is Order)
                    prevList.Add(o as Order);
            }
        }

        private void AddCustomerForm_Shown(object sender, EventArgs e)
        {
            customerBindingNavigatorSaveItem.Enabled = false;
        }

        private void customerDataGridView_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            customerBindingNavigatorSaveItem.Enabled = true;
        }

        private void customerDataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            customerBindingNavigatorSaveItem.Enabled = true;
        }

        private void orderDataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            customerBindingNavigatorSaveItem.Enabled = true;
        }

        private void orderDataGridView_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            customerBindingNavigatorSaveItem.Enabled = true;
        }

        private void customerDataGridView_SelectionChanged(object sender, EventArgs e)
        {
            bool changesBefore = customerBindingNavigatorSaveItem.Enabled;
            string selectedCustomer = null;
            foreach (DataGridViewCell c in customerDataGridView.SelectedCells)
            {
                if(selectedCustomer != null && customerDataGridView.Rows[c.RowIndex].Cells[0].FormattedValue.ToString() != selectedCustomer)
                {
                    orderBindingSource.DataSource = new BindingList<Product>();
                    customerBindingNavigatorSaveItem.Enabled = changesBefore;
                    return;
                }
                else
                {
                    selectedCustomer = customerDataGridView.Rows[c.RowIndex].Cells[0].FormattedValue.ToString();
                }
            }
            if (selectedCustomer == null)
            {
                orderBindingSource.DataSource = new BindingList<Product>();
            }
            else
            {
                orderBindingSource.DataSource = from o in db.Orders.Local.ToBindingList()
                                                join p in db.Products.Local on o.Product.ProductId equals p.ProductId
                                                where o.Customer.CompanyName == selectedCustomer
                                                select o;
            }
            
            customerBindingNavigatorSaveItem.Enabled = changesBefore;
        }

        private void AddCustomerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (customerBindingNavigatorSaveItem.Enabled)
            {
                var result = MessageBox.Show("Zmiany nie zostały zapisane. Zapisać je przed zamknięciem?", "Unsaved Changes",
                             MessageBoxButtons.YesNoCancel,
                             MessageBoxIcon.Question);

                if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                if (result == DialogResult.Yes)
                {
                    customerBindingNavigatorSaveItem_Click(sender, e);
                }
            }
            customerBindingSource.CancelEdit();
            orderBindingSource.CancelEdit();
        }

        private void orderDataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if(e.ColumnIndex == 1)
            {
                if (orderBindingSource.List.Count > e.RowIndex && orderBindingSource.List[e.RowIndex] is Order)
                {
                    e.FormattingApplied = true;
                    e.Value = (orderBindingSource.List[e.RowIndex] as Order).Product.Name.ToString();
                }
            }
        }

        private void customerDataGridView_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.ColumnIndex == 0)
            {
                foreach(DataGridViewRow r in customerDataGridView.Rows)
                {
                    if (r.Index != e.RowIndex && r.Cells[0].Value?.ToString() == e.FormattedValue.ToString())
                    {
                        customerDataGridView.Rows[e.RowIndex].ErrorText = "Nazwa musi być unikatowa.";
                        e.Cancel = true;
                        return;
                    }
                }
            }
        }

        private void customerDataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            customerDataGridView.Rows[e.RowIndex].ErrorText = String.Empty;
        }
    }
}
