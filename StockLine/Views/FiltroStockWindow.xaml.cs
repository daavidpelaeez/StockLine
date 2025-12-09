using System;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1.Views
{
  
    public partial class FiltroStockWindow : Window
    {
        public int Maximo { get; private set; } = 2000;

        public FiltroStockWindow(int valorActual)
        {
            InitializeComponent();

            sliderMaximo.Value = valorActual;
            txtValor.Text = valorActual.ToString();

            sliderMaximo.ValueChanged += SliderMaximo_ValueChanged;

           
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
            if (sender is Button btn && btn.Name != null)
            {
                int valor = 2000; 

                if (btn.Name == "btnRapido500") valor = 500;
                else if (btn.Name == "btnRapido1000") valor = 1000;
                else if (btn.Name == "btnRapido2000") valor = 2000;
                else if (btn.Name == "btnRapido5000") valor = 5000;

                sliderMaximo.Value = valor;  
                txtValor.Text = valor.ToString(); 
            }
        }
    }
}
