using System.ComponentModel;
using System.Windows.Input;
using Common;

/// <summary>
/// This is kept in the CommonLibrary because it is generic with the exception that:
/// 1) it has to be copied into the ViewModels folder of the project where it will be used, and
/// 2) after copying the namespaced must be updated with the project_name.ViewModels
/// 3) the InitMessagePanel() must be called at the top of public MainVM()
/// </summary>
namespace WpfViewer.ViewModels
{
    public enum MessageAction
    {
        Acknowledge,
        DeleteAccount,
        DeleteTransaction,
        DeleteSubtransaction,
        MergeTransactions,
        ResolveTransfers
    }


    partial class MainVM : INotifyPropertyChanged
    {
        public void InitMessagePanel()
        {
            // Message Panel commands
            MessageResponseCommand = new Command(MessageResponseAction);
            MessageCancelCommand = new Command(MessageCancelAction);
        }

        #region Commands

        public ICommand MessageResponseCommand { get; set; }
        public ICommand MessageCancelCommand { get; set; }

        #endregion

        #region Actions

        private void MessageResponseAction(object obj) => MessagePanelResponse();

        private void MessageCancelAction(object obj) => HideMessagePanel();

        #endregion

        #region Properties

        private MessageAction CurrentMessageAction { get; set; }

        private string messagePanelVisibility = "Hidden";
        public string MessagePanelVisibility

        {
            get => messagePanelVisibility;
            set
            {
                messagePanelVisibility = value;
                NotifyPropertyChanged();
            }
        }

        private string messageContent;
        public string MessageContent
        {
            get => messageContent;
            set
            {
                messageContent = value;
                NotifyPropertyChanged();
            }
        }

        private string messageTitle;
        public string MessageTitle
        {
            get => messageTitle;
            set
            {
                messageTitle = value;
                NotifyPropertyChanged();
            }
        }

        private string messageNoVisibility = "Hidden";
        public string MessageNoVisibility
        {
            get => messageNoVisibility;
            set
            {
                messageNoVisibility = value;
                NotifyPropertyChanged();
            }
        }

        private string messageResponseContent;
        public string MessageResponseContent
        {
            get => messageResponseContent;
            set
            {
                messageResponseContent = value;
                NotifyPropertyChanged();
            }
        }

        private string messageResponseEnabled = "True";
        public string MessageResponseEnabled
        {
            get => messageResponseEnabled;
            set
            {
                messageResponseEnabled = value;
                NotifyPropertyChanged();
            }
        }

        private string messageConfirmVisibility = "Collapsed";
        public string MessageConfirmVisibilty
        {
            get => messageConfirmVisibility;
            set
            {
                messageConfirmVisibility = value;
                NotifyPropertyChanged();
                // Set the initial state of the Yes button
                MessageResponseEnabled = (!string.IsNullOrEmpty(MessageConfirmContent) && !MessageConfirmChecked) ? "False" : "True";
            }
        }

        private string messageConfirmContent;
        public string MessageConfirmContent
        {
            get => messageConfirmContent;
            set
            {
                messageConfirmContent = value;
                NotifyPropertyChanged();
                if (string.IsNullOrEmpty(messageConfirmContent))
                {
                    MessageConfirmVisibilty = "Collapsed";
                    MessageConfirmChecked = false;
                    CurrentMessageAction = 0;
                }
                else
                {
                    MessageConfirmVisibilty = "Visible";
                }
            }
        }

        private bool messageConfirmChecked = false;
        public bool MessageConfirmChecked
        {
            get => messageConfirmChecked;
            set
            {
                messageConfirmChecked = value;
                NotifyPropertyChanged();
                // Set the state of the Yes button according to whether the checkbox is checked
                if (MessageConfirmVisibilty == "Visible")
                {
                    MessageResponseEnabled = messageConfirmChecked ? "True" : "False";
                }
            }
        }

        #endregion

        #region Methods

        public void ShowMessagePanel(string title, string content, MessageAction msgAction = MessageAction.Acknowledge)
        {
            MessageTitle = title;
            MessageContent = content;
            MessagePanelVisibility = "Visible";
            MessageConfirmContent = "";

            // Button settings
            if (msgAction == MessageAction.Acknowledge)
            {
                MessageResponseContent = "OK";
                MessageNoVisibility = "Collapsed";
            }
            else
            {
                MessageResponseContent = "Yes";
                MessageNoVisibility = "Visible";
            }

            // Used by MessagePanelResponse to determine action
            CurrentMessageAction = msgAction;
        }

        private void MessagePanelResponse()
        {
            HideMessagePanel();
        }

        private void HideMessagePanel()
        {
            MessageConfirmContent = "";
            MessagePanelVisibility = "Hidden";
        }

        #endregion

    }
}
