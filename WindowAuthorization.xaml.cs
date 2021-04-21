using System.Windows;
using System.Windows.Input;
using System.Text.Json;
using System.IO;
using System.Collections.Generic;
using MailKit.Net.Imap;
using MailKit;
using System;
using MimeKit;
using System.Net.Mail;
using System.Collections.ObjectModel;

namespace Mail
{
    public partial class WindowAuthorization : Window
    {
        public WindowAuthorization()
        {
            InitializeComponent();
        }

        private void Grid_Mouse(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if(TXName.Text.Length > 0 && TXEmail.Text.Length > 0 && TXPassword.Password.Length > 0)
            {
                if (MainWindow.isValid(TXEmail.Text))
                {
                    List<string> l1 = new List<string>();
          
                    List<MailMessageItem> l2 = new List<MailMessageItem>();
                    User user = new User(TXName.Text, TXEmail.Text, TXPassword.Password, l1, l2);
                    if (User.user_is_valid(TXEmail.Text, TXPassword.Password))
                    {
                        string user_json = JsonSerializer.Serialize<User>(user);
                        Database.writeInFile(Database.FILE_USER_INFO, user_json, FileMode.Create);
                        User.user = user;
                        this.Close();
                    }
                    else MessageBox.Show("Не верный Mail или пароль");
                }
                else
                    MessageBox.Show("Не корректный Mail");
            }
            else
                MessageBox.Show("Не все поля заполнены");
            
           
        }
    }
}
