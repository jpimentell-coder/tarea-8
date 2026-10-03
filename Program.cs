using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace RegistroGastos
{
    public class Gasto
    {
        public int Id { get; set; }
        public string Descripcion { get; set; }
        public decimal Monto { get; set; }
        public string Categoria { get; set; }

        public override string ToString()
        {
            return $"{Id} {Descripcion} Q {Monto.ToString("F2", CultureInfo.InvariantCulture)} {Categoria}";
        }
    }

    internal class Program
    {
        static List<Gasto> gastos = new List<Gasto>();
        static int siguienteId = 1;
        const string Archivo = "gastos.csv";

        static void Main(string[] args)
        {
            CargarGastos();

            int opcion;
            do
            {
                opcion = LeerOpcionMenu();

                switch (opcion)
                {
                    case 1:
                        AgregarGasto();
                        break;
                    case 2:
                        ListarGastos();
                        break;
                    case 3:
                        BuscarPorCategoria();
                        break;
                    case 4:
                        GuardarGastos();
                        Console.WriteLine("Saliendo...");
                        break;
                    default:
                        Console.WriteLine("Opción no válida.");
                        break;
                }
                Console.WriteLine();
            } while (opcion != 4);
        }

        static int LeerOpcionMenu()
        {
            Console.WriteLine("=== REGISTRO DE GASTOS ===");
            Console.WriteLine("1. Agregar gasto");
            Console.WriteLine("2. Listar gastos");
            Console.WriteLine("3. Buscar por categoría");
            Console.WriteLine("4. Salir");
            Console.Write("Elige una opción: ");

            if (int.TryParse(Console.ReadLine(), out int opcion))
                return opcion;
            return 0; 
        }

        static void AgregarGasto()
        {
            Console.Write("Descripción: ");
            string descripcion = (Console.ReadLine() ?? "").Trim();

            Console.Write("Monto: ");
            string montoTexto = (Console.ReadLine() ?? "").Trim();

            Console.Write("Categoría: ");
            string categoria = (Console.ReadLine() ?? "").Trim();

            // Validación: no se cae con "abc"
            if (string.IsNullOrWhiteSpace(descripcion) ||
                string.IsNullOrWhiteSpace(categoria) ||
                !decimal.TryParse(montoTexto, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal monto) ||
                monto <= 0)
            {
                Console.WriteLine("Datos inválidos.");
                return;
            }

            gastos.Add(new Gasto
            {
                Id = siguienteId++,
                Descripcion = descripcion,
                Monto = monto,
                Categoria = categoria
            });
            Console.WriteLine("Gasto agregado.");
        }

        static void ListarGastos()
        {
            if (gastos.Count == 0)
            {
                Console.WriteLine("No hay gastos registrados.");
                return;
            }

            foreach (Gasto g in gastos)
                Console.WriteLine(g);

            decimal total = gastos.Sum(g => g.Monto);
            Console.WriteLine($"TOTAL GASTADO: Q {total.ToString("F2", CultureInfo.InvariantCulture)}");
        }

        static void BuscarPorCategoria()
        {
            Console.Write("Categoría a buscar: ");
            string buscada = (Console.ReadLine() ?? "").Trim();

            var resultados = gastos
                .Where(g => g.Categoria.Equals(buscada, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (resultados.Count == 0)
            {
                Console.WriteLine("No se encontraron gastos en esa categoría.");
                return;
            }

            foreach (Gasto g in resultados)
                Console.WriteLine("- " + g);
        }

        
        static void GuardarGastos()
        {
            try
            {
                var lineas = gastos.Select(g =>
                    string.Join(";",
                        g.Id,
                        Limpiar(g.Descripcion),
                        g.Monto.ToString(CultureInfo.InvariantCulture),
                        Limpiar(g.Categoria)));

                File.WriteAllLines(Archivo, lineas);
                Console.WriteLine($"Gastos guardados en {Archivo} ({gastos.Count} registros).");
            }
            catch (IOException ex)
            {
                Console.WriteLine("No se pudo guardar el archivo: " + ex.Message);
            }
        }

        static void CargarGastos()
        {
            if (!File.Exists(Archivo))
                return;

            try
            {
                foreach (string linea in File.ReadAllLines(Archivo))
                {
                    string[] partes = linea.Split(';');
                    if (partes.Length != 4) continue;

                    if (int.TryParse(partes[0], out int id) &&
                        decimal.TryParse(partes[2], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal monto))
                    {
                        gastos.Add(new Gasto
                        {
                            Id = id,
                            Descripcion = partes[1],
                            Monto = monto,
                            Categoria = partes[3]
                        });
                    }
                }

                
                siguienteId = gastos.Count > 0 ? gastos.Max(g => g.Id) + 1 : 1;
                Console.WriteLine($"Cargados {gastos.Count} gastos desde {Archivo}.");
                Console.WriteLine();
            }
            catch (IOException ex)
            {
                Console.WriteLine("No se pudo leer el archivo: " + ex.Message);
            }
        }

        static string Limpiar(string texto)
        {
            return texto.Replace(";", " ");
        }
    }
}
