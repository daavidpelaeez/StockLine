using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WpfApp1.DTOs;
using WpfApp1.Services;
using WpfApp1.ViewModels;

namespace WpfApp1.Views
{
    public partial class CrearEditarUsuarioWindow : Window
    {
        private readonly IPersonaService _personaService;
        private UsuarioViewModel _usuarioEdicion;
        private bool _esEdicion;

        public event Action UsuarioGuardado;

        public CrearEditarUsuarioWindow()
        {
            InitializeComponent();
            _personaService = new PersonaService();
            _esEdicion = false;

            cbRol.SelectedIndex = 0;
        }

        public CrearEditarUsuarioWindow(UsuarioViewModel usuario) : this()
        {
            _usuarioEdicion = usuario;
            _esEdicion = true;

            txtTitulo.Text = "Editar Usuario";
            this.Title = "Editar Usuario";

            CargarDatosUsuario();
        }

        private void CargarDatosUsuario()
        {
            if (_usuarioEdicion == null) return;

            txtNombre.Text = _usuarioEdicion.Nombre;
            txtApellidos.Text = _usuarioEdicion.Apellidos;
            txtEmail.Text = _usuarioEdicion.Email;

            switch (_usuarioEdicion.RoleID)
            {
                case 1:
                    cbRol.SelectedIndex = 0;
                    break;
                case 2:
                    cbRol.SelectedIndex = 1;
                    break;
                case 3:
                    cbRol.SelectedIndex = 2;
                    break;
            }
        }

        private async void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarFormulario())
                return;

            try
            {
                btnGuardar.IsEnabled = false;
                btnCancelar.IsEnabled = false;

                var selectedItem = cbRol.SelectedItem as ComboBoxItem;
                int roleId = int.Parse(selectedItem.Tag.ToString());

                var usuario = new UsuarioDTO
                {
                    Nombre = txtNombre.Text.Trim(),
                    Apellidos = txtApellidos.Text.Trim(),
                    Email = txtEmail.Text.Trim(),
                    RoleID = roleId
                };

                bool resultado;

                if (_esEdicion)
                {
                    usuario.UsuarioID = _usuarioEdicion.UsuarioID;
                    resultado = await _personaService.UpdateAsync(usuario);
                }
                else
                {
                    resultado = await _personaService.CreateAsync(usuario);
                }

                if (resultado)
                {
                    MessageBox.Show(
                        _esEdicion ? "Usuario actualizado correctamente" : "Usuario creado correctamente",
                        "Exito",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    if (UsuarioGuardado != null)
                        UsuarioGuardado();

                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("No se pudo guardar el usuario", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    btnGuardar.IsEnabled = true;
                    btnCancelar.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar el usuario: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                btnGuardar.IsEnabled = true;
                btnCancelar.IsEnabled = true;
            }
        }

        private bool ValidarFormulario()
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El nombre es obligatorio", "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtNombre.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtApellidos.Text))
            {
                MessageBox.Show("Los apellidos son obligatorios", "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtApellidos.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("El email es obligatorio", "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtEmail.Focus();
                return false;
            }

            if (!txtEmail.Text.Contains("@") || !txtEmail.Text.Contains("."))
            {
                MessageBox.Show("El email no tiene un formato valido", "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtEmail.Focus();
                return false;
            }

            if (cbRol.SelectedIndex == -1)
            {
                MessageBox.Show("Debes seleccionar un rol", "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                cbRol.Focus();
                return false;
            }

            return true;
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
