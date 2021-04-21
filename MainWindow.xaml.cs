using MailKit.Net.Pop3;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
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

using MailKit;
using MailKit.Net.Imap;
using MimeKit;
using Microsoft.Win32;
using System.Text.Json;
using System.Collections.ObjectModel;
using System.Threading;

namespace Mail
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        ObservableCollection<Attachment> listAttachments = new ObservableCollection<Attachment>();
        // List<MimeMessage> listMails = new List<MimeMessage>();
        ObservableCollection<MailMessageItem> listMails = new ObservableCollection<MailMessageItem>();

        ObservableCollection<MailMessageItem> listDropMails = new ObservableCollection<MailMessageItem>();



        private void Button_Click_SV(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void Grid_Mouse(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            string user_json = JsonSerializer.Serialize<User>(User.user);
            Database.writeInFile(Database.FILE_USER_INFO, user_json, FileMode.Create);
            Application.Current.Shutdown();
        }

        public void mode_new_message()
        {
            WebBro.Visibility = Visibility.Hidden;
            RichTextBox_BodyMessage.Visibility = Visibility.Visible;
            RichTextBox_BodyMessage.IsEnabled = true;
            BtnSendMessage.IsEnabled = true;
            BtnSaveMessage.IsEnabled = true;
            TextBox_To.IsEnabled = true;
            TextBox_Subject.IsEnabled = true;
            if(User.user == null)
                TextBox_From.IsEnabled = true;

            BtnDeleteAttachments.IsEnabled = true;
            BtnOpenFileAttachments.IsEnabled = true;
            BtnDownloadAttachments.IsEnabled = false;
            TextBox_dropFile.IsEnabled = true;

        }

        public void mode_view_message()
        {
            RichTextBox_BodyMessage.IsEnabled = false;
            BtnSendMessage.IsEnabled = false;
            BtnSaveMessage.IsEnabled = false;
            TextBox_To.IsEnabled = false;
            TextBox_Subject.IsEnabled = false;
            TextBox_From.IsEnabled = false ;
            BtnDeleteAttachments.IsEnabled = false;
            BtnOpenFileAttachments.IsEnabled = false;
            BtnDownloadAttachments.IsEnabled = true;
            TextBox_dropFile.IsEnabled = false;
        }




        public MainWindow()
        {
           

            InitializeComponent();

            mode_view_message();

            ComboboxFontFamily.ItemsSource = Fonts.SystemFontFamilies.OrderBy(f => f.Source);
            ComboboxTextSize.ItemsSource = new List<double>() { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };

            ListBoxMailMessageDraftItem.ItemsSource = listDropMails;
            ListBoxAttachments.ItemsSource = listAttachments;


          
            ComboboxFontFamily.Text = "Arial";
            ComboboxTextSize.Text = "14";
         

            update_user_data();
        }



        private void SelectionKeyUp(object sender, MouseButtonEventArgs e)
        {
            
            ListView listView = (ListView) sender;
            if (listView.SelectedValue != null)
            {
                RichTextBox_BodyMessage.Document.Blocks.Clear();
                //ListBoxAttachments.Items.Clear();
                listAttachments.Clear();

                try
                {
                    mode_view_message();
                    MailMessageItem mmi = (MailMessageItem) listView.SelectedValue;
                    MimeMessage mimeMessage = mmi.MimeMessage;

                    TextBox_From.Text = mimeMessage.From.ToString();
                    TextBox_Subject.Text = mimeMessage.Subject;
                    TextBox_To.Text = mimeMessage.To.ToString();


                    foreach (MimeEntity attachment in mimeMessage.Attachments)
                    {
                        string fileName = attachment.ContentDisposition?.FileName ?? attachment.ContentType.Name;
                        ListBoxAttachments.Items.Add(fileName);
                    }


                    try
                    {
                        Database.writeInFile(Database.HTML_BUFFER, mimeMessage.HtmlBody, FileMode.Create);
                        WebBro.Visibility = Visibility.Visible;
                        RichTextBox_BodyMessage.Visibility = Visibility.Hidden;
                        string path = Directory.GetCurrentDirectory();
                        WebBro.Source = new Uri("file:///" + path + "/" + Database.HTML_BUFFER);
                    }
                    catch (ArgumentNullException re)
                    {
                        WebBro.Visibility = Visibility.Hidden;
                        RichTextBox_BodyMessage.Visibility = Visibility.Visible;
                        string b = mimeMessage.GetTextBody(MimeKit.Text.TextFormat.Text);
                        RichTextBox_BodyMessage.AppendText(b);
                    }
                    catch (Exception re2)
                    {
                        MessageBox.Show("Po ... file poshol");
                    }


                }
                catch (Exception e2)
                {
                    mode_new_message();
                    MailMessageItem mmi = (MailMessageItem)listView.SelectedValue;
                    TextBox_From.Text = mmi.From;
                    TextBox_Subject.Text = mmi.Subject;
                    TextBox_To.Text = mmi.To;



                    foreach (Attachment attachment in mmi.Attachments)
                    {
                       
                        listAttachments.Add(attachment);
                    }

                    WebBro.Visibility = Visibility.Hidden;
                    RichTextBox_BodyMessage.Visibility = Visibility.Visible;
                    string b = mmi.Body;
                    RichTextBox_BodyMessage.AppendText(b);

                }
            }
        }

   



        public void update_user_data()
        {


            try
            {
                string user_json = Database.getData(Database.FILE_USER_INFO);
                User.user = JsonSerializer.Deserialize<User>(user_json);
            }
            catch (Exception e) { }

            if (User.user != null)
            {
                testUpdateMailBox();

                foreach (MailMessageItem item in User.user.Mails_draft)
                    listDropMails.Add(item);

                Button_Account.Content = "Exit";
                Label_UserEmail.Content = User.user.Email;
                Label_UserName.Content = User.user.Name;

                ComboBox_recent_friends.ItemsSource = User.user.Email_friends;

            }
            else Button_Account.Content = "Log in";
        }

        private void Button_Click_New_Message(object sender, RoutedEventArgs e)
        {
            mode_new_message();
 
            RichTextBox_BodyMessage.Document.Blocks.Clear();
            //ListBoxAttachments.Items.Clear();
            listAttachments.Clear();
            TextBox_To.Text = "";
            TextBox_Subject.Text = "";
            if (User.user != null)
                TextBox_From.Text = User.user.Email;
            else TextBox_From.Text = "";
        }

        private void Button_Click_Send_Message(object sender, RoutedEventArgs e)
        {
            if(User.user != null){
                if (TextBox_To.Text.Length > 0)
                {
                    if (isValid(TextBox_From.Text))
                    {

                        // отправитель - устанавливаем адрес и отображаемое в письме имя
                        MailAddress from = new MailAddress(User.user.Email, User.user.Name);

                        /*   if (User.user != null)
                               from = new MailAddress(User.user.Email, User.user.Name);
                           else
                               from = new MailAddress(TextBox_From.Text);
       */


                        MailMessage m = new MailMessage();

                        string emails = TextBox_To.Text.Replace(" ", "");

                        string[] emails_to = emails.Split(new char[] { ',' });

                        foreach (string str in emails_to)
                        {
                            m.To.Add(new MailAddress(str));
                            if (!User.user.Email_friends.Contains(str))
                                User.user.Email_friends.Add(str);
                        }
                



                        m.From = from;



                        // тема письма
                        m.Subject = TextBox_Subject.Text;
                        // текст письма
                        m.Body = StringFromRichTextBox(RichTextBox_BodyMessage);

                        //вложения
                        for (int i = 0; i < listAttachments.Count; i++)
                            m.Attachments.Add(listAttachments[i]);


                        // адрес smtp-сервера и порт, с которого будем отправлять письмо
                        SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);

                        // логин и пароль


                        listAttachments.Clear();
                        //ListBoxAttachments.Items.Clear();
                        RichTextBox_BodyMessage.Document.Blocks.Clear();
                        TextBox_To.Text = "";
                        TextBox_Subject.Text = "";
                        smtp.Credentials = new NetworkCredential(User.user.Email, User.user.Password);
                        smtp.EnableSsl = true;
                        smtp.Send(m);


                        MimeMessage emailMessage = new MimeMessage();

                        emailMessage.From.Add(new MailboxAddress(User.user.Name, User.user.Email));
                        emailMessage.To.Add(new MailboxAddress("", TextBox_To.Text));
                        emailMessage.Subject = TextBox_Subject.Text;
                        emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                        {
                            Text = StringFromRichTextBox(RichTextBox_BodyMessage)
                        };
                        testUpdateMailBox();
                    }
                    else
                        MessageBox.Show("Не корректный адрес отправителя");

                }
                else
                    MessageBox.Show("Пустой адресс получателя");
            }
            else
            {
                Button_Click_Account(sender, e);
            }
             


        }


        private void Button_Click_Account(object sender, RoutedEventArgs e)
        {
            if (Button_Account.Content.Equals("Log in"))
            {
                WindowAuthorization windowAu = new WindowAuthorization();
                windowAu.ShowDialog();
                update_user_data();
                
            }
            else
            {
                //ComboBox_recent_friends.ItemsSource = null;
                Database.DeleteFile(Database.FILE_USER_INFO);
                User.user = null;
                Label_UserName.Content = "";
                Label_UserEmail.Content = "";
                Button_Account.Content = "Log in";
                ListBoxMailMessageItem.ItemsSource = new List<string>();
                WebBro.Visibility = Visibility.Hidden;
                RichTextBox_BodyMessage.Visibility = Visibility.Visible;
                RichTextBox_BodyMessage.Document.Blocks.Clear();
                // ListBoxMailMessageDraftItem.Items.Clear();
                listDropMails.Clear();
              
            }
        }

        string StringFromRichTextBox(RichTextBox rtb)
        {
            TextRange textRange = new TextRange(
                // TextPointer to the start of content in the RichTextBox.
                rtb.Document.ContentStart,
                // TextPointer to the end of content in the RichTextBox.
                rtb.Document.ContentEnd
            );

            // The Text property on a TextRange object returns a string
            // representing the plain text content of the TextRange.
            return textRange.Text;
        }


        public static bool isValid(string email)
        {
            string pattern = "[.\\-_a-z0-9]+@([a-z0-9][\\-a-z0-9]+\\.)+[a-z]{2,6}";
            Match isMatch = Regex.Match(email.ToLower(), pattern, RegexOptions.IgnoreCase);
            return isMatch.Success;
        }
        private void FontFamilySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboboxFontFamily.SelectedItem != null)
                RichTextBox_BodyMessage.Selection.ApplyPropertyValue(Inline.FontFamilyProperty, ComboboxFontFamily.SelectedItem);
        }


        private void FontSizeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TextSelection selectedText = RichTextBox_BodyMessage.Selection;
            object pr_font = selectedText.GetPropertyValue(Inline.FontSizeProperty);
            if (selectedText.Start.GetOffsetToPosition(selectedText.End) == 0) return;
            if (float.TryParse(ComboboxTextSize.SelectedItem.ToString(), out _) &&
                pr_font.ToString() != ComboboxTextSize.SelectedItem.ToString())
                selectedText.ApplyPropertyValue(Inline.FontSizeProperty, ComboboxTextSize.SelectedItem.ToString());
        }


        private void Enter_friend(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBox_recent_friends.SelectedItem != null)
            {
                if(TextBox_To.Text.Length > 0)
                    TextBox_To.Text += ", " + ComboBox_recent_friends.SelectedItem;
                else
                    TextBox_To.Text +=  ComboBox_recent_friends.SelectedItem;
            }
              
           

        }


        private void Button_Click_DeleteAttachments(object sender, RoutedEventArgs e)
        {
            if(ListBoxAttachments.SelectedItem != null)
            {
           
                listAttachments.RemoveAt(ListBoxAttachments.SelectedIndex);
                //ListBoxAttachments.Items.Remove(ListBoxAttachments.SelectedItem);
            }
          
        }

        private void Button_Click_DownloadAttachments(object sender, RoutedEventArgs e)
        {
            if(ListBoxAttachments.SelectedItem != null)
            {
                SaveFileDialog dialog = new SaveFileDialog();
                dialog.Filter = "Excel|*.xlsx|Word|*.docx|JPEG|*.jpg|PNG|*.png|mp4|*.mp4|mp3|*.mp3|Rich Text|*.rtf|Text|*.txt|All Files|*";
                string fileName = ListBoxAttachments.SelectedItem.ToString();

                MailMessageItem mmi = (MailMessageItem)ListBoxMailMessageItem.SelectedValue;
                MimeMessage mimeMessage = mmi.MimeMessage;


                foreach (MimeEntity attachment in mimeMessage.Attachments)
                {
                    string fn = attachment.ContentDisposition?.FileName ?? attachment.ContentType.Name;
                    if (fn.Equals(fileName))
                    {

                        if (dialog.ShowDialog() == true)
                        {
                            fileName = dialog.FileName;
                            using (var stream = File.Create(fileName))
                            {
                                if (attachment is MessagePart)
                                {
                                    var rfc822 = (MessagePart)attachment;

                                    rfc822.Message.WriteTo(stream);
                                }
                                else
                                {
                                    var part = (MimePart)attachment;

                                    part.Content.DecodeTo(stream);
                                }
                            }
                        }
                    }
                }
            }
        }


        private void Open(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Excel|*.xlsx|Word|*.docx|JPEG|*.jpg|PNG|*.png|mp4|*.mp4|mp3|*.mp3|Rich Text|*.rtf|Text|*.txt|All Files|*";
            if (dialog.ShowDialog() == true)
            {
                string FileName = dialog.FileName;

                listAttachments.Add(new Attachment(FileName));
                

               // ListBoxAttachments.Items.Add(dialog.SafeFileName);
            }
        }



        private void PreviewDragEnter(object sender, DragEventArgs e)
        {
            bool isCorrect = true;

            if (e.Data.GetDataPresent(DataFormats.FileDrop, true) == true)
            {
                string[] filenames = (string[])e.Data.GetData(DataFormats.FileDrop, true);
                foreach (string filename in filenames)
                {
                    if (File.Exists(filename) == false)
                    {
                        isCorrect = false;
                        break;
                    }
                    FileInfo info = new FileInfo(filename);

                }
            }
            if (isCorrect == true)
                e.Effects = DragDropEffects.All;
            else
                e.Effects = DragDropEffects.None;
            e.Handled = true;
        }




        private void PreviewDrop(object sender, DragEventArgs e)
        {
            string[] filenames = (string[])e.Data.GetData(DataFormats.FileDrop, true);
            foreach (string filename in filenames)
            {
                FileInfo info = new FileInfo(filename);
                listAttachments.Add(new Attachment(info.FullName));
                //ListBoxAttachments.Items.Add(info.Name);
            }
            e.Handled = true;
        }
        //для рич текст бокс
        private void text_PreviewDragEnter(object sender, DragEventArgs e)
        {
            bool isCorrect = true;

            if (e.Data.GetDataPresent(DataFormats.FileDrop, true) == true)
            {
                string[] filenames = (string[])e.Data.GetData(DataFormats.FileDrop, true);
                foreach (string filename in filenames)
                {
                    if (File.Exists(filename) == false)
                    {
                        isCorrect = false;
                        break;
                    }
                    FileInfo info = new FileInfo(filename);
                    if (info.Extension != ".txt")
                    {
                        isCorrect = false;
                        break;
                    }
                }
            }
            if (isCorrect == true)
                e.Effects = DragDropEffects.All;
            else
                e.Effects = DragDropEffects.None;
            e.Handled = true;
        }
        private void text_PreviewDrop(object sender, DragEventArgs e)
        {
            string[] filenames = (string[])e.Data.GetData(DataFormats.FileDrop, true);
            foreach (string filename in filenames)
                RichTextBox_BodyMessage.AppendText(File.ReadAllText(filename));
            e.Handled = true;
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (User.user != null)
            {

                using (ImapClient client = new ImapClient())
                {
                    try
                    {
                        client.Connect("imap.gmail.com", 993, true);
                        client.Authenticate(User.user.Email, User.user.Password);

                        IMailFolder inbox = client.Inbox;

                        inbox.Open(FolderAccess.ReadOnly);
                        MimeMessage message = inbox.GetMessage(inbox.Count - 1);

                        List<Attachment> list = new List<Attachment>();
                        foreach (MimeEntity attachment in message.Attachments)
                        {
                            string fn = attachment.ContentDisposition?.FileName ?? attachment.ContentType.Name;
                            list.Add(new Attachment(fn));
                        }



                        listMails.Add(new MailMessageItem(message.Subject, message.From.ToString(), message.To.ToString(), message.TextBody, list, message));

                    }
                    catch (Exception e2) { }
                }
            }
            else MessageBox.Show("User is null");
        }



        public async void testUpdateMailBox()
        {
            await Task.Run(() =>
            {
                if (User.user != null)
                {
                   
                    using (ImapClient client = new ImapClient())
                    {
                        try
                        {
                            client.Connect("imap.gmail.com", 993, true);
                            client.Authenticate(User.user.Email, User.user.Password);

                            IMailFolder inbox = client.Inbox;

                            inbox.Open(FolderAccess.ReadOnly);

                            listMails.Clear();
                            for (int i = inbox.Count-1; i >= 0 && i >= inbox.Count - 10; i--)
                            {
                                MimeMessage message = inbox.GetMessage(i);
                                List<Attachment> list = new List<Attachment>();
                                foreach (MimeEntity attachment in message.Attachments)
                                {
                                    string fn = attachment.ContentDisposition?.FileName ?? attachment.ContentType.Name;
                                    list.Add(new Attachment(fn));
                                }
                                listMails.Add(new MailMessageItem(message.Subject, message.From.ToString(), message.To.ToString(), message.TextBody, list, message));
                            }
                        }
                        catch (Exception e2) { }
                    }
                }
                else MessageBox.Show("User is null");
            });

            ListBoxMailMessageItem.ItemsSource = listMails;
        }

        private void BtnShowInboxList_Click(object sender, RoutedEventArgs e)
        {
            ListBoxMailMessageDraftItem.Visibility = Visibility.Hidden;
            ListBoxMailMessageItem.Visibility = Visibility.Visible;
            BtnDelete.IsEnabled = false;
        }

        private void BtnShowDraftList_Click(object sender, RoutedEventArgs e)
        {
            ListBoxMailMessageDraftItem.Visibility = Visibility.Visible;
            ListBoxMailMessageItem.Visibility = Visibility.Hidden;
            BtnDelete.IsEnabled = true;
        }

        private void Button_Click_Save_Message(object sender, RoutedEventArgs e)
        {
            if (User.user != null)
            {
                /*      List<Attachment> att = new List<Attachment>();
                      foreach (Attachment a in listAttachments)
                          att.Add(a);*/


                string[] emails_to = TextBox_To.Text.Split(new char[] { ',' });

                foreach (string str in emails_to)
                {
                    if (!User.user.Email_friends.Contains(str))
                        User.user.Email_friends.Add(str);
                }
              


                MailMessageItem item = new MailMessageItem(TextBox_Subject.Text, TextBox_From.Text,
                TextBox_To.Text, StringFromRichTextBox(RichTextBox_BodyMessage), new List<Attachment>(), null);
                listDropMails.Add(item);
                User.user.Mails_draft.Add(item);
            }
            else
            {
                Button_Click_Account(sender, e);
            }
        }

        private void BtnDeleteDraft(object sender, RoutedEventArgs e)
        {
  
            if (ListBoxMailMessageDraftItem.SelectedValue != null)
                listDropMails.RemoveAt(ListBoxMailMessageDraftItem.SelectedIndex);
            

        }

    }
}
