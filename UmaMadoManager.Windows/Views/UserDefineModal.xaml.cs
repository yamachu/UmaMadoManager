using System;
using System.Windows;

namespace UmaMadoManager.Windows.Views
{
    /// <summary>
    /// UserDefineModal.xaml の相互作用ロジック
    /// </summary>
    public partial class UserDefineModal : Window
    {
        public Action OnClickUsingPrevious { get;  set; }
        public Action OnClickUsingCurrent { get; set; }


        public UserDefineModal()
        {
            InitializeComponent();
        }

        public UserDefineModal(bool enablePrevious)
        {
            InitializeComponent();
            this.previousButton.IsEnabled = enablePrevious;
        }

        private void _onClickUsingPrevious(object sender, RoutedEventArgs e)
        {
            this.OnClickUsingPrevious?.Invoke();
        }

        private void _onClickUsingCurrent(object sender, RoutedEventArgs e)
        {
            this.OnClickUsingCurrent?.Invoke();
        }
    }
}
