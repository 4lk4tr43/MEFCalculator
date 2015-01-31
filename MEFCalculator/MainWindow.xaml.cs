using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MEFCalculator
{
    public partial class MainWindow
    {
        [ImportMany(typeof(IOperation))]
        private IEnumerable<IOperation> _operations;

        private float _leftValue;
        private bool _operationPending;

        public MainWindow()
        {
            InitializeComponent();

            Plugins.Compose(this);

            foreach (IOperation operation in _operations)
            {
                var button = new Button() {Content = operation.Display, Height = 30, Width = 30, Margin = new Thickness(4)};
                var op = operation;
                button.Click += (s, e) =>
                {
                    if (_operationPending)
                    {
                        float rightValue = 0;

                        try
                        {
                            rightValue = float.Parse(Display.Text);
                        }
                        catch (Exception)
                        {
                            rightValue = _leftValue;
                        }
                        finally
                        {
                            Operation.Background = new SolidColorBrush(Colors.White);
                            Operation.Text = "";
                            Display.Text = op.Calculate(_leftValue, rightValue).ToString(CultureInfo.InvariantCulture);
                        }
                    }
                    else
                    {
                        try
                        {
                            Operation.Background = new SolidColorBrush(Colors.LightYellow);
                            Operation.Text = op.Display;
                            _leftValue = float.Parse(Display.Text);
                        }
                        catch (Exception)
                        {
                            _leftValue = 0;
                        }
                        finally
                        {
                            Display.Text = "";
                        }
                    }

                    _operationPending = !_operationPending;
                };

                Operations.Children.Add(button);
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            Display.Focus();
        }
    }

    public static class Plugins
    {
        public static void Compose(object application)
        {
            var catalog = new AssemblyCatalog(Assembly.GetExecutingAssembly());
            var container = new CompositionContainer(catalog);
            container.ComposeParts(application);
        }
    }

    [InheritedExport]
    public interface IOperation
    {
        string Display { get; }
        float Calculate(float a, float b);
    }

    public class Addition : IOperation
    {
        public string Display => "+";

        public float Calculate(float a, float b)
        {
            return a+b;
        }
    }

    public class Substraction : IOperation
    {
        public string Display => "-";

        public float Calculate(float a, float b)
        {
            return a - b;
        }
    }
}
