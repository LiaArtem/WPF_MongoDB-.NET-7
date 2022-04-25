using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Globalization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Configuration;

namespace WPF_MongoDB
{
    public class UserData
    {        
        public ObjectId Id { get; set; }
        private string? textValue;
        private int? intValue;
        private double? doubleValue;
        private Boolean? boolValue;
        private DateTime? dateValue;        
        public int Version { get; set; }
        public string? TextValue
        {
            get { return textValue; }
            set { textValue = value; OnPropertyChanged("TextValue"); }
        }
        public int? IntValue
        {
            get { return intValue; }
            set { intValue = value; OnPropertyChanged("IntValue"); }
        }
        public double? DoubleValue
        {
            get { return doubleValue; }
            set { doubleValue = value; OnPropertyChanged("DoubleValue"); }
        }
        public Boolean? BoolValue
        {
            get { return boolValue; }
            set { boolValue = value; OnPropertyChanged("BoolValue"); }
        }
        public DateTime? DateValue
        {
            get { return dateValue; }
            set { dateValue = value; OnPropertyChanged("DateValue"); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {        
        readonly bool is_initialize = true;
        bool is_filter = false;
        public ObjectId DataGrig_Id;

        public MainWindow()
        {
            InitializeComponent();

            value2.IsEnabled = false;
            is_initialize = false;            
            UpdateDatagrid();
        }

        // чтение данных базы данных
        private static IMongoCollection<UserData> ReadDatabase()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            string databaseName = ConfigurationManager.ConnectionStrings["DatabaseName"].ConnectionString;
            MongoClient client = new(connectionString);
            IMongoDatabase db = client.GetDatabase(databaseName);
            IMongoCollection<UserData> collection = db.GetCollection<UserData>("UserData");
            return collection;
        }

        public static ObjectId GetInternalId(string id)
        {
            if (!ObjectId.TryParse(id, out ObjectId internalId))
                internalId = ObjectId.Empty;

            return internalId;
        }

        private void UpdateDatagrid()
        {

            if (is_initialize == true) return;

            var collection = ReadDatabase();
            var filter = new BsonDocument();

            if (is_filter == false)
            {                
                DataGrid1.ItemsSource = collection.Find(filter).ToList();                
            }
            else
            {
                String m_value1 = value1.Text.ToString();
                String m_value2 = value2.Text.ToString();                                

                if (value_type.Text == "id")
                {                    
                    var filter2 = Builders<UserData>.Filter.Eq("_id", GetInternalId(m_value1));
                    DataGrid1.ItemsSource = collection.Find(filter2).ToList();
                }
                else if (value_type.Text == "text")
                {                    
                    var filter2 = Builders<UserData>.Filter.Regex("TextValue", new BsonRegularExpression(m_value1));
                    DataGrid1.ItemsSource = collection.Find(filter2).ToList();
                }
                else if (value_type.Text == "int")
                {
                    _ = int.TryParse(m_value1, out int m_value1_int);
                    _ = int.TryParse(m_value2, out int m_value2_int);

                    var builder = Builders<UserData>.Filter;
                    var filter2 = builder.Gte("IntValue", m_value1_int) & builder.Lte("IntValue", m_value2_int);                    
                    DataGrid1.ItemsSource = collection.Find(filter2).ToList();                                        
                }
                else if (value_type.Text == "double")
                {
                    _ = double.TryParse(m_value1, out double m_value1_dbl);
                    _ = double.TryParse(m_value2, out double m_value2_dbl);

                    var builder = Builders<UserData>.Filter;
                    var filter2 = builder.Gte("DoubleValue", m_value1_dbl) & builder.Lte("DoubleValue", m_value2_dbl);
                    DataGrid1.ItemsSource = collection.Find(filter2).ToList();                    
                }
                else if (value_type.Text == "bool")
                {
                    _ = Boolean.TryParse(m_value1, out Boolean m_value1_bool);

                    var filter2 = Builders<UserData>.Filter.Eq("BoolValue", m_value1_bool);
                    DataGrid1.ItemsSource = collection.Find(filter2).ToList();                    
                }
                else if (value_type.Text == "date")
                {
                    _ = DateTime.TryParseExact(m_value1, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime m_value1_dat);
                    _ = DateTime.TryParseExact(m_value2, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime m_value2_dat);
                    m_value2_dat = m_value2_dat.AddDays(1);

                    var builder = Builders<UserData>.Filter;
                    var filter2 = builder.Gte("DateValue", m_value1_dat) & builder.Lte("DateValue", m_value2_dat);
                    DataGrid1.ItemsSource = collection.Find(filter2).ToList();                    
                }
            }
            this.DataContext = DataGrid1.ItemsSource;

            // Выделить сроку с курсором
            Boolean m_is_focus = false;
            if (DataGrid1.Items.Count > 0)
            {
                foreach (UserData drv in DataGrid1.ItemsSource)
                {
                    if (drv.Id == DataGrig_Id)
                    {
                        DataGrid1.SelectedItem = drv;
                        DataGrid1.ScrollIntoView(drv);
                        DataGrid1.Focus();
                        m_is_focus = true;
                        break;
                    }
                }
                
                if (!m_is_focus) 
                {
                    DataGrid1.SelectedItem = 1;
                    DataGrid1.ScrollIntoView(1);
                    DataGrid1.Focus();
                }
            }
        }

        // добавить запись
        private void Button_insertClick(object sender, RoutedEventArgs e)
        {
            AddWindow addWin = new(new UserData());
            if (addWin.ShowDialog() == true)
            {
                UserData ud = addWin.UserDataAdd;

                var collection = ReadDatabase();
                collection.InsertOne(ud);                    
                UpdateDatagrid();
            }
        }

        // изменить запись
        private void Button_updateClick(object sender, RoutedEventArgs e)
        {
            // если ни одного объекта не выделено, выходим
            if (DataGrid1.SelectedItem == null) return;
            // получаем выделенный объект
            UserData? ud = DataGrid1.SelectedItem as UserData;

            AddWindow addWin = new(new UserData());
            if (ud != null) 
            { 
                addWin = new(new UserData
                {
                    Id = ud.Id,
                    TextValue = ud.TextValue,
                    IntValue = ud.IntValue,
                    DoubleValue = ud.DoubleValue,
                    BoolValue = ud.BoolValue,
                    DateValue = ud.DateValue
                });
            }

            if (addWin.ShowDialog() == true)
            {
                // получаем измененный объект                
                if (ud != null)
                {
                    var collection = ReadDatabase();
                    var filter = Builders<UserData>.Filter.Eq("_id", ud.Id);
                    
                    // защита от неконтролируемого обновления
                    var data = collection.Find(filter).ToList();
                    if (data != null)
                    {
                        foreach (var item in data)
                        {
                            if (item.Version != ud.Version)
                            {
                                MessageBox("Данные в базе данных изменились обновите данные в гриде", System.Windows.MessageBoxImage.Warning);
                                return;
                            }

                        }
                    }
                    // обновление
                    var update = Builders<UserData>.Update
                                        .Inc("Version", 1)
                                        .Set("TextValue", addWin.UserDataAdd.TextValue)
                                        .Set("IntValue", addWin.UserDataAdd.IntValue)
                                        .Set("DoubleValue", addWin.UserDataAdd.DoubleValue)
                                        .Set("BoolValue", addWin.UserDataAdd.BoolValue)
                                        .Set("DateValue", addWin.UserDataAdd.DateValue)                                        
                                        ;
                    try
                    {
                        collection.UpdateOne(filter, update);                                                                                                        

                        UpdateDatagrid();
                        MessageBox("Запись обновлена");
                    }
                    catch (MongoException ex)
                    {
                        MessageBox(ex.Message, System.Windows.MessageBoxImage.Warning);                        
                        UpdateDatagrid();
                    }
                }
            }
        }

        // удалить запись
        private void Button_deleteClick(object sender, RoutedEventArgs e)
        {
            // если ни одного объекта не выделено, выходим
            if (DataGrid1.SelectedItem == null) return;

            MessageBoxResult result = System.Windows.MessageBox.Show("Удалить запись ???", "Сообщение", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    // получаем выделенный объект
                    if (DataGrid1.SelectedItem is UserData ud)
                    {
                        var collection = ReadDatabase();
                        var filter = Builders<UserData>.Filter.Eq("_id", ud.Id);
                        collection.DeleteOne(filter);
                        UpdateDatagrid();
                    }

                    MessageBox("Запись удалена");
                    break;
                case MessageBoxResult.No:
                    break;
            }
        }

        // обновить запись
        private void Button_selectClick(object sender, RoutedEventArgs e)
        {
            UpdateDatagrid();
        }

        private readonly SolidColorBrush hb = new(Colors.MistyRose);
        private readonly SolidColorBrush nb = new(Colors.AliceBlue);
        private void DataGrid1_LoadingRow(object sender, DataGridRowEventArgs e)
        {            
            if ((e.Row.GetIndex() + 1) % 2 == 0)
                e.Row.Background = hb;
            else
                e.Row.Background = nb;

            // А можно в WPF установить - RowBackground - для нечетных строк и AlternatingRowBackground
        }

        // вывод диалогового окна
        public static void MessageBox(String infoMessage, MessageBoxImage mImage = System.Windows.MessageBoxImage.Information)
        {
            System.Windows.MessageBox.Show(infoMessage, "Сообщение", System.Windows.MessageBoxButton.OK, mImage);
        }
        
        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var row_list = (UserData)DataGrid1.SelectedItem;
                if (row_list != null)
                    DataGrig_Id = row_list.Id;
            }
            catch
            {
                DataGrig_Id = GetInternalId("");
            }
        }

        private void DataGrid_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            Button_updateClick(sender, e);
        }

        // применить фильтр
        private void Button_findClick(object sender, RoutedEventArgs e)
        {
            is_filter = true;
            UpdateDatagrid();
        }

        // отменить фильтр
        private void Button_find_cancelClick(object sender, RoutedEventArgs e)
        {
            is_filter = false;
            value1.Text = "";
            value2.Text = "";            
            UpdateDatagrid();
        }

        // изменение типа данных
        private void Value_type_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            if (is_initialize == true) return;

            ComboBox comboBox = (ComboBox)sender;
            ComboBoxItem selectedItem = (ComboBoxItem)comboBox.SelectedItem;
            String? value_type = selectedItem.Content.ToString();

            if (value_type == "id") { value2.IsEnabled = false; value2.Text = ""; }
            else if (value_type == "text") { value2.IsEnabled = false; value2.Text = ""; }
            else if (value_type == "int") value2.IsEnabled = true;
            else if (value_type == "double") value2.IsEnabled = true;
            else if (value_type == "bool") { value2.IsEnabled = false; value2.Text = ""; }
            else if (value_type == "date") value2.IsEnabled = true;
        }

        // изменение фокуса на value2
        private void Value2_GotKeyboardFocus(object sender, EventArgs e)
        {
            if (value1.Text != "") value2.Text = value1.Text;
        }
    }
}

