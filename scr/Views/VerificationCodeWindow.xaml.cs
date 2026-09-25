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
using System.Windows.Shapes;

namespace DominoTrainGame.Views
{
    /// <summary>
    /// Lógica de interacción para VerificationCode.xaml
    /// </summary>
    public partial class VerificationCodeWindow : Window
    {
        public VerificationCodeWindow()
        {
            InitializeComponent();
        }

        private TextBox[] CodeBoxes => new[] { _codeBox1, _codeBox2, _codeBox3, _codeBox4, _codeBox5 };

        private void CodeBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var box = (TextBox)sender;
            int index = Array.IndexOf(CodeBoxes, box);

            if (box.Text.Length == 1 && index < CodeBoxes.Length - 1)
            {
                CodeBoxes[index + 1].Focus();
            }
        }

        private void CodeBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var box = (TextBox)sender;
            int index = Array.IndexOf(CodeBoxes, box);

            if (e.Key == Key.Back && box.Text.Length == 0 && index > 0)
            {
                CodeBoxes[index - 1].Focus();
                CodeBoxes[index - 1].Clear();
                e.Handled = true;
            }
        }

        private void Verify_Click(object sender, RoutedEventArgs e)
        {
            string code = string.Concat(CodeBoxes.Select(b => b.Text));
        }

        private void ResendCode_Click(object sender, RoutedEventArgs e)
        {
            // TODO: solicitar al servidor un nuevo código.
        }

        private void GoBack(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void OpenSettings(object sender, RoutedEventArgs e)
        {
            // TODO: copiar la misma lógica que usa ChangePasswordWindow.xaml.cs
        }

        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
