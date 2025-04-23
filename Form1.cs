using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Text;
using Newtonsoft.Json;
using System.Runtime.InteropServices;

namespace WLFAPP
{
    public partial class Form1 : Form
    {
        // Colores temáticos cannabis
        private readonly Color colorVerde = Color.FromArgb(58, 131, 65);
        private readonly Color colorVerdeClaro = Color.FromArgb(152, 203, 103);
        private readonly Color colorAmarillo = Color.FromArgb(241, 196, 15);
        private readonly Color colorCafe = Color.FromArgb(120, 76, 40);
        private readonly Color colorFondo = Color.FromArgb(240, 240, 230);

        // Listas de productos y orden actual
        private List<Producto> productos = new List<Producto>();
        private List<ItemOrden> ordenActual = new List<ItemOrden>();
        private int numeroOrden = 1;

        // Controles principales
        private Panel panelProductos;
        private Panel panelOrden;
        private Panel panelCategorias;
        private Label lblTotal;
        private Button btnFinalizarOrden;
        private Button btnCancelarOrden;
        private Button btnEliminarItem;
        private ListBox lstOrdenActual;
        private DataGridView dgvOrdenActual;

        public Form1()
        {
            ConfigurarFormulario();
            CargarDatos();
            ConfigurarInterfaz();
        }

        private void ConfigurarFormulario()
        {
            this.Text = "Punto de Venta - Festival WLF";
            this.Size = new Size(1024, 768);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = colorFondo;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
        }

        private void ConfigurarInterfaz()
        {
            this.Controls.Clear();

            // Panel de categorías con scroll
            panelCategorias = new Panel
            {
                Dock = DockStyle.Left,
                Width = 150,
                BackColor = colorVerde,
                AutoScroll = true // Habilitar scroll automático
            };
            this.Controls.Add(panelCategorias);

            // Botones de categorías
            var categorias = productos.Select(p => p.Categoria).Distinct().ToList();
            int buttonY = 10;
            foreach (var categoria in categorias)
            {
                Button btnCategoria = new Button
                {
                    Text = categoria,
                    Width = 130,
                    Height = 50,
                    Location = new Point(10, buttonY),
                    BackColor = colorVerdeClaro,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Arial", 12, FontStyle.Bold)
                };
                btnCategoria.FlatAppearance.BorderSize = 0;

                // Efectos visuales
                btnCategoria.MouseEnter += (s, e) => { btnCategoria.BackColor = colorAmarillo; btnCategoria.ForeColor = Color.Black; };
                btnCategoria.MouseLeave += (s, e) => { btnCategoria.BackColor = colorVerdeClaro; btnCategoria.ForeColor = Color.White; };

                btnCategoria.Click += (s, e) => MostrarProductosCategoria(categoria);
                panelCategorias.Controls.Add(btnCategoria);
                buttonY += 60;
            }

            // Panel de productos con scrolling
            panelProductos = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = colorFondo,
                Padding = new Padding(10),
                AutoScroll = true // Habilitar scroll automático
            };
            this.Controls.Add(panelProductos);

            // Panel de orden
            // Panel de orden con un diseño más detallado
            panelOrden = new Panel
            {
                Dock = DockStyle.Right,
                Width = 300,
                BackColor = Color.WhiteSmoke,
                Padding = new Padding(5)
            };
            this.Controls.Add(panelOrden);

            // Título de la orden con mejor formato
            Label lblTituloOrden = new Label
            {
                Text = "Orden Actual",
                Dock = DockStyle.Top,
                Height = 40,
                Font = new Font("Arial", 16, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = colorVerde
            };
            panelOrden.Controls.Add(lblTituloOrden);


            // Contenedor para la lista de orden
            Panel panelListaOrden = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle,
                    Margin = new Padding(5),
                    Padding = new Padding(2)
                };
                panelOrden.Controls.Add(panelListaOrden);

            
                // Usar DataGridView en lugar de ListBox
                dgvOrdenActual = new DataGridView
                    {
                        Dock = DockStyle.Fill,
                        BackgroundColor = Color.White,
                        BorderStyle = BorderStyle.None,
                        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                        RowHeadersVisible = false,
                        AllowUserToAddRows = false,
                        AllowUserToDeleteRows = false,
                        AllowUserToResizeRows = false,
                        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                        MultiSelect = false,
                        ReadOnly = true,
                        Font = new Font("Arial", 11),
                        ColumnHeadersHeight = 30
                    };

                // Configurar las columnas del DataGridView
                dgvOrdenActual.Columns.Add("Cantidad", "Cant.");
                dgvOrdenActual.Columns.Add("Nombre", "Producto");
                dgvOrdenActual.Columns.Add("Precio", "Precio");
                dgvOrdenActual.Columns.Add("Subtotal", "Subtotal");

                // Ajustar el ancho de las columnas
                dgvOrdenActual.Columns["Cantidad"].Width = 40;
                dgvOrdenActual.Columns["Nombre"].Width = 130;
                dgvOrdenActual.Columns["Precio"].Width = 60;
                dgvOrdenActual.Columns["Subtotal"].Width = 70;

                // Configurar eventos
                dgvOrdenActual.SelectionChanged += (s, e) =>
                {
                    btnEliminarItem.Enabled = dgvOrdenActual.SelectedRows.Count > 0;
                };

                panelListaOrden.Controls.Add(dgvOrdenActual);

            // Panel de total y botones con mejor disposición
            Panel panelTotal = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 180,
                BackColor = Color.WhiteSmoke,
                Padding = new Padding(5)
            };

            // Label para el total con mejor formato
            lblTotal = new Label
            {
                Text = "Total: $0.00",
                Dock = DockStyle.Top,
                Height = 40,
                Font = new Font("Arial", 16, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = colorVerde
            };
            panelTotal.Controls.Add(lblTotal);

            // Espacio entre el total y los botones
            Panel panelEspacio = new Panel
            {
                Dock = DockStyle.Top,
                Height = 10
            };
            panelTotal.Controls.Add(panelEspacio);

            // Botón eliminar con mejor estilo
            btnEliminarItem = new Button
            {
                Text = "Eliminar Item",
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                Font = new Font("Arial", 12, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            btnEliminarItem.FlatAppearance.BorderSize = 0;
            btnEliminarItem.Click += EliminarItemSeleccionado;
            panelTotal.Controls.Add(btnEliminarItem);

            // Espacio entre botones
            Panel panelEspacio2 = new Panel
            {
                Dock = DockStyle.Top,
                Height = 5
            };
            panelTotal.Controls.Add(panelEspacio2);

            // Botón finalizar orden con mejor estilo
            btnFinalizarOrden = new Button
            {
                Text = "Finalizar Orden",
                Dock = DockStyle.Top,
                Height = 45,
                BackColor = colorVerde,
                ForeColor = Color.White,
                Font = new Font("Arial", 14, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnFinalizarOrden.FlatAppearance.BorderSize = 0;
            btnFinalizarOrden.Click += FinalizarOrden;
            panelTotal.Controls.Add(btnFinalizarOrden);

            // Espacio entre botones
            Panel panelEspacio3 = new Panel
            {
                Dock = DockStyle.Top,
                Height = 5
            };
            panelTotal.Controls.Add(panelEspacio3);

            // Botón cancelar orden
            btnCancelarOrden = new Button
            {
                Text = "Cancelar Orden",
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                Font = new Font("Arial", 12, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnCancelarOrden.FlatAppearance.BorderSize = 0;
            btnCancelarOrden.Click += CancelarOrden;
            panelTotal.Controls.Add(btnCancelarOrden);

            panelOrden.Controls.Add(panelTotal);

            if (categorias.Count > 0)
            {
                MostrarProductosCategoria(categorias[0]);
            }

            // Evento de redimensionamiento para la ventana
            this.SizeChanged += (s, e) =>
            {
                // Solo redimensionar si ya hay una categoría seleccionada
                if (panelProductos.Tag != null && panelProductos.Tag is string categoria)
                {
                    MostrarProductosCategoria(categoria);
                }
            };
        }

        private void EliminarItemSeleccionado(object sender, EventArgs e)
        {
            if (dgvOrdenActual.SelectedRows.Count == 0) return;

            DialogResult respuesta = MessageBox.Show(
                "¿Eliminar este item de la orden?",
                "Confirmar",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (respuesta == DialogResult.Yes)
            {
                // Obtener el índice de la fila seleccionada
                int selectedRowIndex = dgvOrdenActual.SelectedRows[0].Index;

                // Eliminar el item de la orden
                ordenActual.RemoveAt(selectedRowIndex);

                // Actualizar la vista
                ActualizarVistaOrden();

                MessageBox.Show("Item eliminado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void MostrarProductosCategoria(string categoria)
        {
            panelProductos.Controls.Clear();

            var productosFiltrados = productos.Where(p => p.Categoria == categoria).ToList();
            int btnWidth = 140;
            int btnHeight = 120;
            int margin = 10;
            int columns = Math.Max(1, (panelProductos.Width - margin) / (btnWidth + margin));

            for (int i = 0; i < productosFiltrados.Count; i++)
            {
                var producto = productosFiltrados[i];
                int row = i / columns;
                int col = i % columns;

                Button btnProducto = new Button
                {
                    Text = $"{producto.Nombre}\n${producto.Precio:F2}",
                    Width = btnWidth,
                    Height = btnHeight,
                    Location = new Point(margin + col * (btnWidth + margin), margin + row * (btnHeight + margin)),
                    BackColor = colorVerdeClaro,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Arial", 11, FontStyle.Bold),
                    Tag = producto
                };
                btnProducto.FlatAppearance.BorderSize = 0;
                btnProducto.Click += (s, e) => AgregarProductoOrden((Producto)((Button)s).Tag);
                panelProductos.Controls.Add(btnProducto);
            }
        }

        private void AgregarProductoOrden(Producto producto)
        {
            var itemExistente = ordenActual.FirstOrDefault(i => i.Producto.Id == producto.Id);

            if (itemExistente != null)
            {
                itemExistente.Cantidad++;
            }
            else
            {
                ordenActual.Add(new ItemOrden
                {
                    Producto = producto,
                    Cantidad = 1,
                    Subtotal = producto.Precio
                });
            }

            ActualizarVistaOrden(); // ¡Llamada crítica aquí!
        }

        private void ActualizarVistaOrden()
        {
            dgvOrdenActual.Rows.Clear(); // Limpiar el grid

            foreach (var item in ordenActual)
            {
                item.Subtotal = item.Producto.Precio * item.Cantidad;

                // Agregar una fila al DataGridView
                int index = dgvOrdenActual.Rows.Add(
                    item.Cantidad.ToString(),
                    item.Producto.Nombre,
                    $"${item.Producto.Precio:F2}",
                    $"${item.Subtotal:F2}"
                );

                // Almacenar una referencia al item para poder identificarlo luego
                dgvOrdenActual.Rows[index].Tag = item;
            }

            decimal total = ordenActual.Sum(i => i.Subtotal);
            lblTotal.Text = $"Total: ${total:F2}";

            // Habilitar/deshabilitar botones según corresponda
            btnFinalizarOrden.Enabled = ordenActual.Count > 0;
            btnCancelarOrden.Enabled = ordenActual.Count > 0;
            btnEliminarItem.Enabled = dgvOrdenActual.SelectedRows.Count > 0;
        }

        private void FinalizarOrden(object sender, EventArgs e)
        {
            if (ordenActual.Count == 0)
            {
                MessageBox.Show("No hay productos en la orden actual.", "Orden vacía", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Crear una orden nueva
            Orden nuevaOrden = new Orden
            {
                Id = numeroOrden++,
                Fecha = DateTime.Now,
                Items = new List<ItemOrden>(ordenActual),
                Total = ordenActual.Sum(i => i.Subtotal)
            };

            // Guardar la orden en JSON
            GuardarOrden(nuevaOrden);

            MessageBox.Show($"Orden #{nuevaOrden.Id} finalizada con éxito.\nTotal: ${nuevaOrden.Total:F2}",
                "Orden Finalizada", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Limpiar la orden actual
            ordenActual.Clear();
            ActualizarVistaOrden();
        }

        private void CancelarOrden(object sender, EventArgs e)
        {
            if (ordenActual.Count == 0)
            {
                MessageBox.Show("No hay productos en la orden actual.", "Orden vacía", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var result = MessageBox.Show("¿Estás seguro de cancelar la orden actual?",
                "Cancelar Orden", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                ordenActual.Clear();
                ActualizarVistaOrden();
                MessageBox.Show("Orden cancelada con éxito.", "Orden Cancelada", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void CargarDatos()
        {
            // Cargar productos de muestra para el festival
            productos = new List<Producto>
            {
                // Bebidas
                new Producto { Id = 1, Nombre = "Agua", Categoria = "Bebidas", Precio = 1.50m },
                new Producto { Id = 2, Nombre = "Gaseosa", Categoria = "Bebidas", Precio = 2.00m },
                new Producto { Id = 3, Nombre = "Refresco", Categoria = "Bebidas", Precio = 2.00m },
                new Producto { Id = 4, Nombre = "Jugo", Categoria = "Bebidas", Precio = 2.50m },
                new Producto { Id = 5, Nombre = "Bebida Energética", Categoria = "Bebidas", Precio = 3.50m },
                
                // Cervezas
                new Producto { Id = 6, Nombre = "Cerveza Nacional", Categoria = "Cervezas", Precio = 3.00m },
                new Producto { Id = 7, Nombre = "Cerveza Importada", Categoria = "Cervezas", Precio = 5.00m },
                new Producto { Id = 8, Nombre = "Cerveza Artesanal", Categoria = "Cervezas", Precio = 6.00m },
                new Producto { Id = 9, Nombre = "Cerveza de Cáñamo", Categoria = "Cervezas", Precio = 7.50m },
                
                // Comida
                new Producto { Id = 10, Nombre = "Hamburguesa", Categoria = "Comida", Precio = 8.00m },
                new Producto { Id = 11, Nombre = "Pizza", Categoria = "Comida", Precio = 7.00m },
                new Producto { Id = 12, Nombre = "Hot Dog", Categoria = "Comida", Precio = 5.00m },
                new Producto { Id = 13, Nombre = "Nachos", Categoria = "Comida", Precio = 6.00m },
                new Producto { Id = 14, Nombre = "Brownies", Categoria = "Comida", Precio = 4.00m },
                
                // Souvenirs
                new Producto { Id = 15, Nombre = "Camiseta", Categoria = "Souvenirs", Precio = 15.00m },
                new Producto { Id = 16, Nombre = "Gorra", Categoria = "Souvenirs", Precio = 12.00m },
                new Producto { Id = 17, Nombre = "Pulsera", Categoria = "Souvenirs", Precio = 5.00m },
                new Producto { Id = 18, Nombre = "Llavero", Categoria = "Souvenirs", Precio = 3.00m }
            };

            // Verificar si existe el directorio de datos
            string directorioData = Path.Combine(Application.StartupPath, "Data");
            if (!Directory.Exists(directorioData))
            {
                Directory.CreateDirectory(directorioData);
            }

            // Recuperar el número de orden más reciente
            string rutaOrdenes = Path.Combine(directorioData, "ordenes.json");
            if (File.Exists(rutaOrdenes))
            {
                try
                {
                    string json = File.ReadAllText(rutaOrdenes);
                    var ordenes = JsonConvert.DeserializeObject<List<Orden>>(json);
                    if (ordenes != null && ordenes.Count > 0)
                    {
                        numeroOrden = ordenes.Max(o => o.Id) + 1;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al cargar datos: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void GuardarOrden(Orden orden)
        {
            string directorioData = Path.Combine(Application.StartupPath, "Data");
            string rutaOrdenes = Path.Combine(directorioData, "ordenes.json");

            try
            {
                // Asegurar que el directorio existe
                Directory.CreateDirectory(directorioData); // ¡Este es el cambio clave!

                List<Orden> ordenes = new List<Orden>();

                // Cargar órdenes existentes si el archivo existe
                if (File.Exists(rutaOrdenes))
                {
                    string json = File.ReadAllText(rutaOrdenes);
                    ordenes = JsonConvert.DeserializeObject<List<Orden>>(json) ?? new List<Orden>();
                }

                // Agregar la nueva orden
                ordenes.Add(orden);

                // Guardar todas las órdenes
                string jsonActualizado = JsonConvert.SerializeObject(ordenes, Formatting.Indented);
                File.WriteAllText(rutaOrdenes, jsonActualizado);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    // Clases de modelo
    public class Producto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Categoria { get; set; }
        public decimal Precio { get; set; }
    }

    public class ItemOrden
    {
        public Producto Producto { get; set; }
        public int Cantidad { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class Orden
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public List<ItemOrden> Items { get; set; }
        public decimal Total { get; set; }
    }
}