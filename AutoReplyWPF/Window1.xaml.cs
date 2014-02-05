using System;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Lync.Model;
using Microsoft.Lync.Model.Conversation;
using System.Windows.Threading;

namespace AutoReplyWPF
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        private LyncClient _LyncClient = LyncClient.GetClient();
        private Conversation _Conversation;
        System.Collections.Generic.List<Conversation> currentConversationList = new List<Conversation>();
        public Window1()
        {
            InitializeComponent();
            foreach (Conversation con in _LyncClient.ConversationManager.Conversations)
            {
                //con.End();
            }
        }

        private void autoReply_Click(object sender, RoutedEventArgs e)
        {
            _LyncClient = LyncClient.GetClient();
            if (_LyncClient.State == ClientState.SignedIn)
            {
                string buttonContent=autoReply.Content.ToString();
                if (buttonContent.Equals("Start Auto Reply"))
                {
                    StartAutoReply();
                    autoReply.Content = "Stop Auto Reply";
                }
                else
                {
                    StopAutoReply();
                    autoReply.Content = "Start Auto Reply";
                }
            }
        }

        private void StopAutoReply()
        {
            _LyncClient.ConversationManager.ConversationAdded -= new EventHandler<ConversationManagerEventArgs>(ConversationManager_ConversationAdded);
        }

        private void StartAutoReply()
        {
            currentConversationList = _LyncClient.ConversationManager.Conversations.ToList();
            _LyncClient.ConversationManager.ConversationAdded += new EventHandler<ConversationManagerEventArgs>(ConversationManager_ConversationAdded);
        }

        private void ConversationManager_ConversationAdded(object sender, ConversationManagerEventArgs e)
        {
            e.Conversation.ParticipantAdded += new EventHandler<ParticipantCollectionChangedEventArgs>(Conversation_ParticipantAdded);
            _Conversation = e.Conversation;
        }

        void Conversation_ParticipantAdded(object sender, ParticipantCollectionChangedEventArgs e)
        {
            if (e.Participant.IsSelf == false)
            {
                if (((Conversation)sender).Modalities.ContainsKey(ModalityTypes.InstantMessage))
                {
                    ((InstantMessageModality)e.Participant.Modalities[ModalityTypes.InstantMessage]).InstantMessageReceived += new EventHandler<MessageSentEventArgs>(Page_InstantMessageReceived);
                }
            }
        }

        void Page_InstantMessageReceived(object sender, MessageSentEventArgs e)
        {
            if (currentConversationList.Contains(_Conversation))
            {

            }
            else
            {
                SendMessage();
            }
        }

        private void SendMessage()
        {
            try
            {
                if (((InstantMessageModality)_Conversation.Modalities[ModalityTypes.InstantMessage]).CanInvoke(ModalityAction.SendInstantMessage))
                {
                    //string url = HtmlPage.Document.DocumentUri.ToString();
                    //string txtMessage = message.Text;
                    string txtMessage="Busy";
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal,
                     (Action)(() =>
                            {
                                txtMessage = message.Text;
                            }));

                    ((InstantMessageModality)_Conversation.Modalities[ModalityTypes.InstantMessage]).BeginSendMessage(
                        txtMessage
                        , SendMessageCallBack
                        , null);
                }
                currentConversationList.Add(_Conversation);

            }
            catch (LyncClientException e)
            {
                MessageBox.Show("Lync Client Exception" + e.Message);
            }
        }

        private void SendMessageCallBack(IAsyncResult ar)
        {
            if (ar.IsCompleted == true)
            {
                try
                {
                    ((InstantMessageModality)_Conversation.Modalities[ModalityTypes.InstantMessage]).EndSendMessage(ar);
                }
                catch (LyncClientException e)
                {
                    MessageBox.Show("Lync Client Exception" + e.Message);
                }
            }
        }
    }
}
