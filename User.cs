using MailKit;
using MailKit.Net.Imap;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace Mail
{
    class User  : INotifyPropertyChanged
    {
        private string name, email, password;
        private List<string> email_friends;

        private List<MailMessageItem> mails_draft = new List<MailMessageItem>();

        public string Name
        {
            get => name;
            set => name = value;
        }
        public string Email
        {
            get => email;
            set => email = value;
        }

        public string Password
        {
            get => password;
            set => password = value;
        }




        public List<string> Email_friends
        {
            get { return email_friends; }
            set
            {
                email_friends = value;
                OnPropertyChanged("Email_friends");
            }
        }

        public List<MailMessageItem> Mails_draft
        {
            get => mails_draft;
            set => mails_draft = value;
        }

        /*  public List<MailMessageItem> Mails_draft
          {
              get { return mails_draft; }
              set
              {
                  mails_draft = value;
                  OnPropertyChanged("Email_friends");
              }
          }
  */






        public User(string name, string email, string password, List<string> email_friends, List<MailMessageItem> mails_draft)
        {
            this.name = name;
            this.email = email;
            this.password = password;
            this.email_friends = email_friends;
       
            this.mails_draft = mails_draft;
        }

        public static User user;

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        public static bool user_is_valid(string email, string password)
        {
            using (ImapClient client = new ImapClient())
            {
                try
                {
                    client.Connect("imap.gmail.com", 993, true);
                    client.Authenticate(email, password);
                    return true;
                }
                catch (Exception e2) { }
            }
            return false;
        }
    }
}
