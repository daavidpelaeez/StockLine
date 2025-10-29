using System;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1.Views
{
    /// <summary>
    /// Interaction logic for FiltroStockWindow.xaml
    /// </summary>
    public partial class FiltroStockWindow : Window
    {
        public int Maximo { get; private set; } = 2000;

        public FiltroStockWindow(int valorActual)
        {
            InitializeComponent();

            sliderMaximo.Value = valorActual;
            txtValor.Text = valorActual.ToString();

            sliderMaximo.ValueChanged += SliderMaximo_ValueChanged;

            // Opcional: botones de valores rápidos
            btnRapido500.Click += (s, e) => SetSliderValue(500);
            btnRapido1000.Click += (s, e) => SetSliderValue(1000);
            btnRapido2000.Click += (s, e) => SetSliderValue(2000);
            btnRapido5000.Click += (s, e) => SetSliderValue(5000);
        }

        private void SliderMaximo_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            txtValor.Text = ((int)sliderMaximo.Value).ToString();
        }

        private void SetSliderValue(int valor)
        {
            sliderMaximo.Value = valor;
        }

        private void Aceptar_Click(object sender, RoutedEventArgs e)
        {
            Maximo = (int)sliderMaximo.Value;
            this.DialogResult = true;
            this.Close();
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void QuickValue_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && int.TryParse(btn.Content.ToString(), out int valor))
            {
                sliderMaximo.Value = valor;  // Actualiza el slider
                txtValor.Text = valor.ToString(); // Actualiza el texto junto al slider
            }
        }
    }
}
