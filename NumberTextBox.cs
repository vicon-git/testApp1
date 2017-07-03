using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace WpfApplication1
{
    /// <summary>
    /// A WPF custom <see cref="System.Windows.Controls.TextBox">TextBox</see> for editing numbers.
    /// </summary>
    public class NumberTextBox : TextBox
    {
        public NumberTextBox()
            : base()
        {
            PreviewTextInput += OnPreviewTextInput;
        }

        private static Regex regexDisallowedInteger = new Regex(@"[^0-9-]+");  // matches disallowed text
        private static Regex regexDisallowedFloat = new Regex(@"[^0-9-+.,e]+");  // matches disallowed text

        private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (Text.Length > 0 && OpenMathMenu(e.Text))
                e.Handled = true;
            else
                e.Handled = regexDisallowedFloat.IsMatch(e.Text);  // or regexDisallowedInteger
        }

        #region Math

        private enum EOperation { Add, Subtract, Multiply, Divide }
        private EOperation operation = EOperation.Add;
        private ContextMenu menuMath = null;
        private MenuItem miOperand = null;
        private MenuItem miResult = null;

        private bool OpenMathMenu(string text)
        {
            if (menuMath != null)
                return false;

            if (text == "+" || text == "-" || text == "*" || text == "/")
            {
                if (text == "-" && CaretIndex == 0)  // negative sign in front of number
                    return false;

                miOperand = new MenuItem();
                miOperand.Header = "";
                miOperand.FontSize = 18;
                miOperand.FontWeight = FontWeights.Medium;
                miOperand.IsEnabled = false;

                if (text == "+")
                {
                    operation = EOperation.Add;
                    miOperand.Icon = new Image { Source = new BitmapImage(new Uri("pack://application:,,,/WpfApplication1;component/Images/operator_add.png", UriKind.Absolute)) };
                }
                else if (text == "-")
                {
                    operation = EOperation.Subtract;
                    miOperand.Icon = new Image { Source = new BitmapImage(new Uri("pack://application:,,,/WpfApplication1;component/Images/operator_sub.png", UriKind.Absolute)) };
                }
                else if (text == "*")
                {
                    operation = EOperation.Multiply;
                    miOperand.Icon = new Image { Source = new BitmapImage(new Uri("pack://application:,,,/WpfApplication1;component/Images/operator_mult.png", UriKind.Absolute)) };
                }
                else if (text == "/")
                {
                    operation = EOperation.Divide;
                    miOperand.Icon = new Image { Source = new BitmapImage(new Uri("pack://application:,,,/WpfApplication1;component/Images/operator_div.png", UriKind.Absolute)) };
                }

                miResult = new MenuItem();
                miResult.Header = "";
                miResult.Icon = new Image { Source = new BitmapImage(new Uri("pack://application:,,,/WpfApplication1;component/Images/operator_equals.png", UriKind.Absolute)) };
                miResult.FontSize = 18;
                miResult.FontWeight = FontWeights.Medium;
                miResult.IsEnabled = false;
                miResult.Click += OnResultClick;

                menuMath = new ContextMenu();
                menuMath.Items.Add(miOperand);
                menuMath.Items.Add(new Separator());
                menuMath.Items.Add(miResult);
                menuMath.PlacementTarget = this;
                menuMath.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                menuMath.PreviewKeyDown += OnMathPreviewKeyDown;
                menuMath.Closed += OnMathClosed;
                menuMath.IsOpen = true;
                return true;
            }

            return false;
        }

        private void OnMathPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                return;

            string operand = miOperand.Header.ToString();
            //System.Diagnostics.Debug.WriteLine(string.Format("OnMathPreviewKeyDown {0}", e.Key.ToString()));
            switch (e.Key)
            {
                case Key.Cancel:
                case Key.Clear:
                case Key.Escape:
                case Key.OemClear:
                    miResult.Header = "";
                    OnResultClick(this, null);
                    break;

                case Key.Back:
                case Key.Delete:
                    if (operand.Length > 0)
                        UpdateResult(operand.Substring(0, operand.Length - 1));
                    else
                        OnResultClick(this, null);
                    break;

                case Key.LineFeed:
                case Key.Enter:
                    OnResultClick(this, null);
                    break;

                case Key.OemPlus:  // '='
                    OnResultClick(this, null);
                    break;

                case Key.Subtract:
                case Key.OemMinus:
                    if (operand.Length == 0)
                        miOperand.Header = "-";
                    break;

                case Key.D0:
                case Key.D1:
                case Key.D2:
                case Key.D3:
                case Key.D4:
                case Key.D5:
                case Key.D6:
                case Key.D7:
                case Key.D8:
                case Key.D9:
                    char key1 = (char)((e.Key - Key.D0) + '0');
                    if (char.IsDigit(key1))
                        UpdateResult(operand + key1);
                    break;

                case Key.NumPad0:
                case Key.NumPad1:
                case Key.NumPad2:
                case Key.NumPad3:
                case Key.NumPad4:
                case Key.NumPad5:
                case Key.NumPad6:
                case Key.NumPad7:
                case Key.NumPad8:
                case Key.NumPad9:
                    char key2 = (char)((e.Key - Key.NumPad0) + '0');
                    if (char.IsDigit(key2))
                        UpdateResult(operand + key2);
                    break;

                case Key.OemComma:
                    if (!operand.Contains(","))
                        miOperand.Header = operand + ",";
                    break;

                case Key.Decimal:
                case Key.OemPeriod:
                    if (!operand.Contains("."))
                        miOperand.Header = operand + ".";
                    break;

                //case Key.Multiply:
                //case Key.Add:
                //case Key.Divide:
            }
        }

        private void UpdateResult(string operand)
        {
            miOperand.Header = operand;
            double dop1, dop2, result;
            if (double.TryParse(Text, out dop1) && double.TryParse(operand, out dop2))
            {
                switch (operation)
                {
                    case EOperation.Add:
                        result = dop1 + dop2;
                        miResult.Header = result.ToString();
                        miResult.IsEnabled = true;
                        break;

                    case EOperation.Subtract:
                        result = dop1 - dop2;
                        miResult.Header = result.ToString();
                        miResult.IsEnabled = true;
                        break;

                    case EOperation.Multiply:
                        result = dop1 * dop2;
                        miResult.Header = result.ToString();
                        miResult.IsEnabled = true;
                        break;

                    case EOperation.Divide:
                        if (dop2 == 0.0)
                        {
                            miResult.Header = "";
                            miResult.IsEnabled = false;
                        }
                        else
                        {
                            result = dop1 / dop2;
                            if (double.IsInfinity(result) || double.IsNaN(result))
                            {
                                miResult.Header = "";
                                miResult.IsEnabled = false;
                            }
                            else
                            {
                                miResult.Header = result.ToString();
                                miResult.IsEnabled = true;
                            }
                        }
                        break;
                }
            }
            else
            {
                miResult.Header = "";
                miResult.IsEnabled = false;
            }
        }

        private void OnResultClick(object sender, RoutedEventArgs e)
        {
            string result = miResult.Header.ToString();
            if (result.Length > 0)
            {
                Text = result;
                CaretIndex = Text.Length;
            }

            menuMath.IsOpen = false;
        }

        private void OnMathClosed(object sender, RoutedEventArgs e)
        {
            menuMath.PreviewKeyDown -= OnMathPreviewKeyDown;
            menuMath.Closed -= OnMathClosed;
            miResult.Click -= OnResultClick;

            menuMath = null;
            miOperand = null;
            miResult = null;
        }

        #endregion Math
    }
}
