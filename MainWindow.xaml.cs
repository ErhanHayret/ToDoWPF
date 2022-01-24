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
using System.Net.Http;
using System.Net.Http.Json;

namespace ToDoWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public class User
        {
            public int userID { get; set; }
            public string name { get; set; }
            public int status { get; set; }
            public string password { get; set; }
        }

        public class ToDo
        {
            public int toDoID { get; set; }
            public string toDoText { get; set; }
            public int toDoCompleted { get; set; }
            public int userID { get; set; }
        }

        List<ToDo> TodoList = new List<ToDo>();
        List<User> UserList = new List<User>();
        public int activeUser;

        public MainWindow()
        {
            InitializeComponent();
            TodoDG.ItemsSource = TodoList;
            AdminDG.ItemsSource = UserList;
        }

        public async void Login(object sender, RoutedEventArgs e)//Login Button Event
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://localhost:5001");
            try
            {
                var response = await client.GetFromJsonAsync<User[]>("api/User");
                for (int i = 0; i < response.Length; i++)
                {
                    if (response[i].name.ToString() == usr.Text && response[i].password.ToString() == psw.Text)
                    {
                        if (response[i].status == 0)
                        {
                            NormalUser(response[i].userID, response[i].name.ToString());
                        }
                        else if (response[i].status == 1)
                        {
                            AdminUser(response[i].userID, response[i].name.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errlbl.Text += ex.Message.ToString() + "**End Login**";
            }
        }

        public async void SingIn(object sender, RoutedEventArgs e)//SingIn Button Event
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://localhost:5001");
            var newUser = new User { userID = 0, name = usr.Text, status = 0, password = psw.Text };
            try
            {
                var response = await client.PostAsJsonAsync("api/User/addrec", newUser);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                errlbl.Text += ex.Message.ToString() + "**End SingIn**";
            }
        }

        public async void AddToDo(object sender, RoutedEventArgs e)//TextBox Text'i ToDos Tablosuna eklenir
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://localhost:5001");
            try
            {
                if (addTodo.Text.Length > 0)
                {
                    ToDo tmp = new ToDo() { toDoID = 0, toDoText = addTodo.Text, toDoCompleted = 0, userID = activeUser };
                    var response = await client.PostAsJsonAsync("api/Todo", tmp);
                    response.EnsureSuccessStatusCode();
                }
            }
            catch (Exception ex)
            {
                errlbl.Text += ex.Message.ToString() + "**End Add**";
            }
            GetToDos();
        }

        public async void Done(object sender, RoutedEventArgs e)//ToDo Data Grid'te seçili olan ToDo Completed durumu değiştirilir
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://localhost:5001");
            ToDo tmp = (ToDo)TodoDG.SelectedItem;
            try
            {
                var respone = await client.PutAsJsonAsync("/completed", tmp);
                respone.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                errlbl.Text += ex.Message.ToString() + "**End Done**";
            }
            GetToDos();
        }
        public async void Update(object sender, RoutedEventArgs e)//ToDo Data Grid'te seçili olan ToDo TextBox Text'i ile güncellenir
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://localhost:5001");
            try
            {
                ToDo tmp = (ToDo)TodoDG.SelectedItem;
                tmp.toDoText = addTodo.Text;
                tmp.userID = activeUser;
                var respone = await client.PutAsJsonAsync("api/ToDo/update", tmp);
                respone.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                errlbl.Text += ex.Message.ToString() + "**End Update**";
            }
            GetToDos();
        }

        public async void DeleteToDo(object sender, RoutedEventArgs e)//ToDo Data Grid'te seçili olan ToDo silinir
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://localhost:5001");
            try
            {
                ToDo tmp = (ToDo)TodoDG.SelectedItem;
                var response = await client.DeleteAsync("api/ToDo/" + tmp.toDoID);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                errlbl.Text += ex.Message.ToString() + "**End Del**";
            }
            GetToDos();
        }

        public void ShowTodos(object sender, RoutedEventArgs e)//Admin Data Grid'te seçili olan kullanıcın bütün ToDo'ları
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://localhost:5001");
            TodoList.Clear();
            try
            {
                User tmpU = (User)AdminDG.SelectedItem;
                activeUser = tmpU.userID;
                GetToDos();
            }
            catch (Exception ex)
            {
                errlbl.Text += ex.Message.ToString() + "**End ShowTodos**";
            }
        }

        public void AddandAppointment(object sender, RoutedEventArgs e)//TextBox Text'i Admin Data Grid'te seçili olan kullanıcıya atar
        {
            try
            {
                User tmp = (User)AdminDG.SelectedItem;
                activeUser = tmp.userID;
                AddToDo(sender, e);
            }
            catch (Exception ex)
            {
                errlbl.Text += ex.Message.ToString() + "**End AddandAppointment**";
            }
        }

        public void NormalUser(int id, string name)//Kullanıcı Status'ne bağlı arayüz düzeni
        {
            UserSP.Visibility = Visibility.Visible;
            LoginSP.Visibility = Visibility.Collapsed;
            activeUser = id;
            usrId.Content = "User ID:" + id.ToString();
            usrName.Content = "User Name:" + name;
            TodoDG.Visibility = Visibility.Visible;
            GetToDos();
        }

        public void AdminUser(int id, string name)//Kullanıcı Status'ne bağlı arayüz düzeni
        {
            UserSP.Visibility = Visibility.Visible;
            TodoDG.Visibility = Visibility.Visible;
            AdminDG.Visibility = Visibility.Visible;
            aaaBtn.Visibility = Visibility.Visible;
            adm.Visibility = Visibility.Visible;
            shoBtn.Visibility = Visibility.Visible;
            addBtn.Visibility = Visibility.Collapsed;
            LoginSP.Visibility = Visibility.Collapsed;
            uptBtn.Visibility = Visibility.Collapsed;
            activeUser = id;
            usrId.Content = "User ID:" + id.ToString();
            usrName.Content = "User Name:" + name;
            GetUsers();
        }

        public async void GetToDos()//Servisten ToDos tablosundan aktif kullanıcı adına bağlı bütün ToDo ları çeker
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://localhost:5001");
            TodoList.Clear();
            try
            {
                var response = await client.GetFromJsonAsync<ToDo[]>("api/ToDo/" + activeUser);
                for (int i = 0; i < response.Length; i++)
                {
                    ToDo tmp = new ToDo() { toDoID = response[i].toDoID, toDoText = response[i].toDoText.ToString(), toDoCompleted = response[i].toDoCompleted, userID = response[i].userID };
                    TodoList.Add(tmp);
                }
                TodoDG.Items.Refresh();
            }
            catch (Exception ex)
            {
                errlbl.Text += ex.Message.ToString() + "**End GetToDos**";
            }
        }

        public async void GetUsers()//Sevisten Users tablosunda bulunan bütün kullanıcıları çeker
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://localhost:5001");
            try
            {
                var response = await client.GetFromJsonAsync<User[]>("api/User");
                for (int i = 0; i < response.Length; i++)
                {
                    User tmp = new User() { userID = response[i].userID, name = response[i].name, status = response[i].status, password = "****" };
                    UserList.Add(tmp);
                }
                AdminDG.Items.Refresh();
            }
            catch (Exception ex)
            {
                errlbl.Text += ex.Message.ToString() + "**End GetUsers**";
            }
        }
    }
}
