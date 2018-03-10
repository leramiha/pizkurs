using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace pizza
{
    class Pizza
    {
        public string title { get; set; }
        public double price { get; set; }
        public string desc { get; set; }
        public long key { get; set; }
        public int qty { get; set; }
        public Pizza(string a, double b, string c, long d)
        {
            title = a;
            price = b;
            desc = c;
            key = d;
            qty = 0;
        }
    }

    class ViewModel : INotifyPropertyChanged
    {
        private Pizza selectpizza { get; set; }
        public Pizza SelectPizza
        {
            get { return selectpizza; }
            set
            {
                selectpizza = value;
                try
                {
                    BitmapImage img = new BitmapImage();
                    img.BeginInit();
                    img.CacheOption = BitmapCacheOption.OnLoad;
                    img.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                    img.UriSource = new Uri($"Images/{selectpizza.title}.jpg", UriKind.Relative);
                    img.EndInit();
                    Photo = img;
                    Description = selectpizza.desc;
                }
                catch { MessageBox.Show("No image for this pizza"); }
                if (selectpizza.qty == 0)
                {
                    CanRemove = false;
                    CanAdd = true;
                }
            }
        }
        private string description { get; set; }
        public string Description
        {
            get { return description; }
            set
            {
                description = value;
                OnProperyChange("Description");
            }
        }
        private bool canadd { get; set; }
        public bool CanAdd
        {
            get { return canadd; }
            set
            {
                canadd = value;
                OnProperyChange("CanAdd");
            }
        }
        private bool canremove { get; set; }
        public bool CanRemove
        {
            get { return canremove; }
            set
            {
                canremove = value;
                OnProperyChange("CanRemove");
            }
        }
        private List<Pizza> pizzas { get; set; }
        public List<Pizza> Pizzas
        {
            get { return pizzas; }
            set
            {
                pizzas = value;
                OnProperyChange("Pizzas");
                double sum = 0;
                foreach (Pizza p in Pizzas)
                    sum += p.qty * p.price;
                Total = $"Total: {sum}";
            }
        }
        private ImageSource photo { get; set; }
        public ImageSource Photo
        {
            get { return photo; }
            set
            {
                photo = value;
                OnProperyChange("Photo");
            }
        }
        private string total { get; set; }
        public string Total
        {
            get { return total; }
            set
            {
                total = value;
                OnProperyChange("Total");
            }
        }
        private string name { get; set; }
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                OnProperyChange("Name");
            }
        }
        private string address { get; set; }
        public string Address
        {
            get { return address; }
            set
            {
                address = value;
                OnProperyChange("Address");
            }
        }
        private string phone { get; set; }
        public string Phone
        {
            get { return phone; }
            set
            {
                phone = value;
                OnProperyChange("Phone");
            }
        }
        public MyCommand AddPizza { get; set; } = new MyCommand();
        public MyCommand RemovePizza { get; set; } = new MyCommand();
        public MyCommand Order { get; set; } = new MyCommand();
        public ViewModel()
        {
            AddPizza.Function = DoAddPizza;
            RemovePizza.Function = DoRemovePizza;
            Order.Function = DoOrder;
            List<Pizza> p = new List<Pizza>();
            using (SqlConnection db = new SqlConnection(Properties.Settings.Default.Connection))
            {
                SqlCommand query = new SqlCommand("select title, price, [desc], [key] from menu", db);
                SqlDataReader reader;
                try
                {
                    db.Open();
                    reader = query.ExecuteReader();
                    SqlDataAdapter adapter = new SqlDataAdapter(query);
                    DataTable table = new DataTable();
                    db.Close();
                    adapter.Fill(table);
                    foreach (DataRow row in table.Rows)
                        p.Add(new Pizza((string)row.ItemArray[0], (double)row.ItemArray[1], (string)row.ItemArray[2], (long)row.ItemArray[3]));
                }
                catch
                {
                    MessageBox.Show("Local database is inaccessible");
                }
            }
            Pizzas = p;
            CanAdd = false;
            CanRemove = false;
        }

        private void DoOrder(object obj)
        {
            bool empty = true;
            foreach (Pizza p in Pizzas)
            {
                if (p.qty > 0) { empty = false; break; }
            }
            if (empty)
            {
                MessageBox.Show("Empty orders are not allowed");
                return;
            }
            if (string.IsNullOrEmpty(Name))
            {
                MessageBox.Show("Name is required");
                return;
            }
            if (Name.Length > 150)
            {
                MessageBox.Show("Your name is too long");
                return;
            }
            if (string.IsNullOrEmpty(Address))
            {
                MessageBox.Show("Address is required");
                return;
            }
            if (Address.Length > 150)
            {
                MessageBox.Show("Your address is too long");
                return;
            }
            if (string.IsNullOrEmpty(Phone))
            {
                MessageBox.Show("Phone is required");
                return;
            }
            if (Phone.Length > 12)
            {
                MessageBox.Show("Your phone number is too long");
                return;
            }
            string order = string.Empty;
            foreach (Pizza p in Pizzas)
                if (p.qty > 0)
                    order += $"{p.title} x {p.qty};";
            using (SqlConnection db = new SqlConnection(Properties.Settings.Default.Connection))
            {
                SqlCommand query = new SqlCommand($"insert into orders(name, address, phone, selection) values(\'{Name}\', \'{Address}\', \'{Phone}\', \'{order}\')", db);
                try
                {
                    db.Open();
                    query.ExecuteNonQuery();
                    db.Close();
                }
                catch
                {
                    MessageBox.Show("Local database is inaccessible");
                    return;
                }
            }
            MessageBox.Show("Order complete!");
            Name = string.Empty;
            Address = string.Empty;
            Phone = string.Empty;
            foreach (Pizza p in Pizzas)
                p.qty = 0;
            Pizzas = new List<Pizza>(Pizzas);
        }

        private void DoAddPizza(object obj)
        {
            CanRemove = true;
            SelectPizza.qty += 1;
            Pizzas = new List<Pizza>(Pizzas);
            if (SelectPizza.qty == 10) CanAdd = false;
        }

        private void DoRemovePizza(object obj)
        {
            CanAdd = true;
            SelectPizza.qty -= 1;
            Pizzas = new List<Pizza>(Pizzas);
            if (SelectPizza.qty == 0) CanRemove = false;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnProperyChange(string prop)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }

    class MyCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public Action<object> Function { get; set; }

        public void Execute(object parameter)
        {
            Function(parameter);
        }
    }
}