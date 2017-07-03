using System.Windows;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void NumberTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            lbl1.Content = "ASDASDASD";
 //           this.chkbox1.SetValue(0, 1);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            lbl1.Content = "";
        }
    }
}
