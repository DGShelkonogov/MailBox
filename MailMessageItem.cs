using MimeKit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Mail
{
   
    class MailMessageItem : INotifyPropertyChanged
    {
        MimeMessage mimeMessage;
        string subject, from, to, body;
        List<Attachment> attachments = new List<Attachment>();

        public string Subject
        {
            get { return subject; }
            set
            {
                subject = value;
                OnPropertyChanged("Subject");
            }
        }


        public string From
        {
            get { return from; }
            set
            {
                from = value;
                OnPropertyChanged("From");
            }
        }

        public string To
        {
            get { return to; }
            set
            {
                to = value;
                OnPropertyChanged("To");
            }
        }

        public string Body
        {
            get { return body; }
            set
            {
                body = value;
                OnPropertyChanged("Body");
            }
        }

        public List<Attachment> Attachments
        {
            get { return attachments; }
            set
            {
                attachments = value;
                OnPropertyChanged("Attachments");
            }
        }






        public MimeMessage MimeMessage
        {
            get { return mimeMessage; }
            set
            {
                mimeMessage = value;
                OnPropertyChanged("MimeMessage");
            }
        }


     
        public MailMessageItem(string subject, string from, string to, string body, List<Attachment> attachments, MimeMessage mimeMessage)
        {
            this.subject = subject;
            this.from = from;
            this.to = to;
            this.body = body;
            this.attachments = attachments;
            this.mimeMessage = mimeMessage;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }

}
