using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using LiveCharts;
using LiveCharts.Wpf;
using WpfApp1.DTOs;
using WpfApp1.Services;
using WpfApp1;

namespace WpfApp1.ViewModels
{
    public class PerfilViewModel : INotifyPropertyChanged
    {
        private string _nombre;
        private string _apellidos;
        private string _email;
        private string _rol;
        private string _estado;
        private string _inicialNombre;
        private int _usuarioId;
        private string _mensaje;
        private bool _guardando;

        public string Nombre { get => _nombre; set { _nombre = value; OnPropertyChanged(); CheckChanges(); } }
        public string Apellidos { get => _apellidos; set { _apellidos = value; OnPropertyChanged(); CheckChanges(); } }
        public string Email { get => _email; set { _email = value; OnPropertyChanged(); CheckChanges(); } }
        public string Rol { get => _rol; set { _rol = value; OnPropertyChanged(); } }
        public string Estado { get => _estado; set { _estado = value; OnPropertyChanged(); } }
        public string InicialNombre { get => _inicialNombre; set { _inicialNombre = value; OnPropertyChanged(); } }
        public int UsuarioID { get => _usuarioId; set { _usuarioId = value; OnPropertyChanged(); } }
        public string Mensaje { get => _mensaje; set { _mensaje = value; OnPropertyChanged(); } }
        public bool Guardando { get => _guardando; set { _guardando = value; OnPropertyChanged(); } }

        public bool EditandoNombre { get => _editandoNombre; set { _editandoNombre = value; OnPropertyChanged(); } }
        public bool EditandoApellidos { get => _editandoApellidos; set { _editandoApellidos = value; OnPropertyChanged(); } }
        public bool EditandoEmail { get => _editandoEmail; set { _editandoEmail = value; OnPropertyChanged(); } }
        public string MensajePassword { get => _mensajePassword; set { _mensajePassword = value; OnPropertyChanged(); } }
        public ObservableCollection<string> Historial { get => _historial; set { _historial = value; OnPropertyChanged(); } }
        public SeriesCollection ActividadSemanaSeries { get; set; }
        public List<string> DiasSemana { get; set; }

        public ICommand GuardarCommand { get; }
        public ICommand CancelarCommand { get; }
        public ICommand EditarNombreCommand { get; }
        public ICommand EditarApellidosCommand { get; }
        public ICommand EditarEmailCommand { get; }
        public ICommand CambiarPasswordCommand { get; }

        private readonly IPersonaService _personaService;

        private string _passwordActual;
        private string _nuevaPassword;
        private string _confirmarPassword;
        private bool _editandoNombre;
        private bool _editandoApellidos;
        private bool _editandoEmail;
        private string _mensajePassword;
        private ObservableCollection<string> _historial = new ObservableCollection<string>();
        private bool _isNombreReadOnly = true;
        private bool _isApellidosReadOnly = true;
        private bool _isEmailReadOnly = true;
        private bool _puedeGuardar;
        private int _roleId;

        public string PasswordActual { get => _passwordActual; set { _passwordActual = value; OnPropertyChanged(); } }
        public string NuevaPassword { get => _nuevaPassword; set { _nuevaPassword = value; OnPropertyChanged(); } }
        public string ConfirmarPassword { get => _confirmarPassword; set { _confirmarPassword = value; OnPropertyChanged(); } }
        public bool IsNombreReadOnly { get => _isNombreReadOnly; set { _isNombreReadOnly = value; OnPropertyChanged(); } }
        public bool IsApellidosReadOnly { get => _isApellidosReadOnly; set { _isApellidosReadOnly = value; OnPropertyChanged(); } }
        public bool IsEmailReadOnly { get => _isEmailReadOnly; set { _isEmailReadOnly = value; OnPropertyChanged(); } }
        public bool PuedeGuardar
        {
            get => _puedeGuardar;
            set { _puedeGuardar = value; OnPropertyChanged(); ((RelayCommand)GuardarCommand).RaiseCanExecuteChanged(); }
        }
        public int RoleID { get => _roleId; set { _roleId = value; OnPropertyChanged(); } }

        public PerfilViewModel()
        {
            _personaService = new PersonaService();
            GuardarCommand = new RelayCommand(async o => await Guardar(), o => PuedeGuardar);
            CancelarCommand = new RelayCommand(o => (o as Window)?.Close());
            EditarNombreCommand = new RelayCommand(o => { IsNombreReadOnly = !IsNombreReadOnly; });
            EditarApellidosCommand = new RelayCommand(o => { IsApellidosReadOnly = !IsApellidosReadOnly; });
            EditarEmailCommand = new RelayCommand(o => { IsEmailReadOnly = !IsEmailReadOnly; });
            CambiarPasswordCommand = new RelayCommand(async o => await CambiarPassword());
            CargarDatosUsuario();
            CargarGraficoActividad();
        }

        private void CargarGraficoActividad()
        {
            DiasSemana = new List<string> { "Lun", "Mar", "Mié", "Jue", "Vie", "Sáb", "Dom" };
            ActividadSemanaSeries = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Acciones",
                    Values = new ChartValues<int> { 2, 3, 1, 4, 2, 1, 0 },
                    Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(33, 150, 243)),
                    StrokeThickness = 0
                }
            };
            OnPropertyChanged(nameof(ActividadSemanaSeries));
            OnPropertyChanged(nameof(DiasSemana));
        }

        private string _originalNombre, _originalApellidos, _originalEmail;
        private async void CargarDatosUsuario()
        {
            try
            {
                Guardando = true;
                var usuarioId = Session.UsuarioID;
                var usuarios = await _personaService.GetAllAsync();
                var usuario = usuarios?.Find(u => u.UsuarioID == usuarioId);
                if (usuario != null)
                {
                    UsuarioID = usuario.UsuarioID;
                    Nombre = usuario.Nombre;
                    Apellidos = usuario.Apellidos;
                    Email = usuario.Email;
                    Rol = ObtenerRol(usuario.RoleID);
                    Estado = usuario.Activo ? "Activo" : "Inactivo";
                    InicialNombre = string.IsNullOrEmpty(usuario.Nombre) ? "?" : usuario.Nombre.Substring(0, 1).ToUpper();
                    _originalNombre = usuario.Nombre;
                    _originalApellidos = usuario.Apellidos;
                    _originalEmail = usuario.Email;
                }
            }
            catch (Exception ex)
            {
                Mensaje = "Error al cargar datos: " + ex.Message;
            }
            finally
            {
                Guardando = false;
                PuedeGuardar = false;
            }
        }

        private void CheckChanges()
        {
            PuedeGuardar =
                (!IsNombreReadOnly && Nombre != _originalNombre) ||
                (!IsApellidosReadOnly && Apellidos != _originalApellidos) ||
                (!IsEmailReadOnly && Email != _originalEmail);
        }

        private async Task Guardar()
        {
            Guardando = true;
            Mensaje = string.Empty;
            try
            {
               
                var usuarios = await _personaService.GetAllAsync();
                bool rolValido = false;
                if (usuarios != null)
                {
                    rolValido = usuarios.Any(u => u.RoleID == this.RoleID);
                }
                if (!rolValido)
                {
                    Mensaje = "Error: El rol asignado al usuario no existe en la base de datos. Contacta con el administrador.";
                    Guardando = false;
                    return;
                }

                
                if (usuarios.Any(u => u.Email.Equals(Email, StringComparison.OrdinalIgnoreCase) && u.UsuarioID != UsuarioID))
                {
                    Mensaje = "Error: El email ya está en uso por otro usuario.";
                    Guardando = false;
                    return;
                }

                var usuario = new UsuarioDTO
                {
                    UsuarioID = UsuarioID,
                    Nombre = Nombre,
                    Apellidos = Apellidos,
                    Email = Email,
                    RoleID = this.RoleID
                };
                var ok = await _personaService.UpdateAsync(usuario);
                if (ok)
                {
                    Mensaje = "Datos guardados correctamente.";
                    _originalNombre = Nombre;
                    _originalApellidos = Apellidos;
                    _originalEmail = Email;
                    IsNombreReadOnly = true;
                    IsApellidosReadOnly = true;
                    IsEmailReadOnly = true;
                    PuedeGuardar = false;
                }
                else
                {
                    Mensaje = "No se pudo guardar. Intenta de nuevo.";
                }
            }
            catch (Exception ex)
            {
                Mensaje = "Error al guardar: " + ex.Message;
            }
            finally
            {
                Guardando = false;
            }
        }

        private async Task CambiarPassword()
        {
            MensajePassword = string.Empty;
            if (string.IsNullOrWhiteSpace(PasswordActual) || string.IsNullOrWhiteSpace(NuevaPassword) || string.IsNullOrWhiteSpace(ConfirmarPassword))
            {
                MensajePassword = "Completa todos los campos de contraseña.";
                return;
            }
            if (NuevaPassword != ConfirmarPassword)
            {
                MensajePassword = "La nueva contraseña y la confirmación no coinciden.";
                return;
            }
            try
            {
                
                var usuario = await _personaService.LoginAsync(Email, PasswordActual);
                if (usuario == null)
                {
                    MensajePassword = "Contraseña actual incorrecta.";
                    return;
                }
                
                using (var client = new System.Net.Http.HttpClient())
                {
                    client.BaseAddress = new Uri("http://localhost:5200/");
                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(NuevaPassword);
                    var content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json");
                    var request = new System.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("PATCH"), $"api/Usuarios/{UsuarioID}/password")
                    {
                        Content = content
                    };
                    var res = await client.SendAsync(request);
                    if (!res.IsSuccessStatusCode)
                    {
                        MensajePassword = "Error cambiando contraseña: " + await res.Content.ReadAsStringAsync();
                        return;
                    }
                }
                MensajePassword = "Contraseña cambiada correctamente.";
                PasswordActual = NuevaPassword = ConfirmarPassword = string.Empty;
            }
            catch (Exception ex)
            {
                MensajePassword = "Error: " + ex.Message;
            }
        }

        private string ObtenerRol(int roleId)
        {
            switch (roleId)
            {
                case 1: return "Usuario";
                case 2: return "Comercial";
                case 3: return "Admin";
                default: return "Desconocido";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;
        public event EventHandler CanExecuteChanged;
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }
        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);
        public void Execute(object parameter) => _execute(parameter);
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
