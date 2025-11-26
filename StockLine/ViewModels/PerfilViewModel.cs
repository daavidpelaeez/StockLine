using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WpfApp1.Models;
using StockLine.Models;

namespace WpfApp1.ViewModels
{
    public class PerfilViewModel : INotifyPropertyChanged
    {
        private string _nombre;
        private string _apellidos;
        private string _email;
        private string _telefono;
        private string _rol;
        private string _passwordActual;
        private string _nuevaPassword;
        private string _confirmarPassword;
        private ObservableCollection<string> _historial;

        public string Nombre { get => _nombre; set { _nombre = value; OnPropertyChanged(); } }
        public string Apellidos { get => _apellidos; set { _apellidos = value; OnPropertyChanged(); } }
        public string Email { get => _email; set { _email = value; OnPropertyChanged(); } }
        public string Telefono { get => _telefono; set { _telefono = value; OnPropertyChanged(); } }
        public string Rol { get => _rol; set { _rol = value; OnPropertyChanged(); } }
        public string PasswordActual { get => _passwordActual; set { _passwordActual = value; OnPropertyChanged(); } }
        public string NuevaPassword { get => _nuevaPassword; set { _nuevaPassword = value; OnPropertyChanged(); } }
        public string ConfirmarPassword { get => _confirmarPassword; set { _confirmarPassword = value; OnPropertyChanged(); } }
        public ObservableCollection<string> Historial { get => _historial; set { _historial = value; OnPropertyChanged(); } }

        public ICommand GuardarCommand { get; }
        public ICommand CambiarPasswordCommand { get; }
        public ICommand CancelarCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public PerfilViewModel()
        {
            GuardarCommand = new RelayCommand(Guardar);
            CambiarPasswordCommand = new RelayCommand(CambiarPassword);
            CancelarCommand = new RelayCommand(Cancelar);
            Historial = new ObservableCollection<string>();
            // Cargar datos de usuario autenticado y su historial aquí
        }

        private void Guardar(object obj)
        {
            // Lógica para guardar cambios de perfil
        }

        private void CambiarPassword(object obj)
        {
            // Lógica para cambiar contraseña
        }

        private void Cancelar(object obj)
        {
            // Lógica para cancelar y cerrar ventana
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);
        public void Execute(object parameter) => _execute(parameter);
        public event EventHandler CanExecuteChanged { add { } remove { } }
    }
}
