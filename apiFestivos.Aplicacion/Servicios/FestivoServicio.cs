using apiFestivos.Core.Interfaces.Repositorios;
using apiFestivos.Core.Interfaces.Servicios;
using apiFestivos.Dominio.DTOs;
using apiFestivos.Dominio.Entidades;
using Moq;
using Xunit;

namespace apiFestivos.Aplicacion.Servicios
{
    public class FestivoServicio : IFestivoServicio
    {
        private readonly IFestivoRepositorio repositorio;

        public FestivoServicio(IFestivoRepositorio repositorio)
        {
            this.repositorio = repositorio;
        }

        public async Task<Festivo> Obtener(int Id)
        {
            return await repositorio.Obtener(Id);
        }

        public async Task<IEnumerable<Festivo>> ObtenerTodos()
        {
            return await repositorio.ObtenerTodos();
        }
        public async Task<IEnumerable<Festivo>> Buscar(string Dato)
        {
            return await repositorio.Buscar(Dato);
        }

        public async Task<Festivo> Agregar(Festivo Festivo)
        {
            return await repositorio.Agregar(Festivo);
        }

        public async Task<Festivo> Modificar(Festivo Festivo)
        {
            return await repositorio.Modificar(Festivo);
        }

        public async Task<bool> Eliminar(int Id)
        {
            return await repositorio.Eliminar(Id);
        }

        //********** Consultas //**********

        private DateTime ObtenerInicioSemanaSanta(int año)
        {
            int a = año % 19;
            int b = año % 4;
            int c = año % 7;
            int d = (19 * a + 24) % 30;

            int dias = d + (2 * b + 4 * c + 6 * d + 5) % 7;

            int dia = 15 + dias;
            int mes = 3;
            if (dia > 31)
            {
                dia = dia - 31;
                mes = 4;
            }
            return new DateTime(año, mes, dia);
        }

        private DateTime AgregarDias(DateTime fecha, int dias)
        {
            return fecha.AddDays(dias);
        }

        private DateTime SiguienteLunes(DateTime fecha)
        {
            DayOfWeek diaSemana = fecha.DayOfWeek;
            int diasLunes = ((int)DayOfWeek.Monday - (int)diaSemana + 7) % 7;
            return AgregarDias(fecha, diasLunes);
        }

        private FechaFestivo ObtenerFestivo(int año, Festivo festivo)
        {
            FechaFestivo fechaFestivo = null;
            switch (festivo.IdTipo)
            {
                case 1:
                    fechaFestivo = new FechaFestivo
                    {
                        Fecha = new DateTime(año, festivo.Mes, festivo.Dia),
                        Nombre = festivo.Nombre
                    };
                    break;
                case 2:
                    fechaFestivo = new FechaFestivo
                    {
                        Fecha = SiguienteLunes(new DateTime(año, festivo.Mes, festivo.Dia)),
                        Nombre = festivo.Nombre
                    };
                    break;
                case 3:
                    fechaFestivo = new FechaFestivo
                    {
                        Fecha = AgregarDias(ObtenerInicioSemanaSanta(año), festivo.DiasPascua),
                        Nombre = festivo.Nombre
                    };
                    break;
                case 4:
                    fechaFestivo = new FechaFestivo
                    {
                        Fecha = SiguienteLunes(AgregarDias(ObtenerInicioSemanaSanta(año), festivo.DiasPascua)),
                        Nombre = festivo.Nombre
                    };
                    break;
            }
            return fechaFestivo;
        }

        public async Task<IEnumerable<FechaFestivo>> ObtenerAño(int Año)
        {
            var festivos = await repositorio.ObtenerTodos();

            List<FechaFestivo> fechaFestivos = new List<FechaFestivo>();
            foreach (var festivo in festivos)
            {
                fechaFestivos.Add(ObtenerFestivo(Año, festivo));
            }
            return fechaFestivos;
        }

        public async Task<bool> EsFestivo(DateTime Fecha)
        {
            // Obtener los festivos para el año específico
            var festivos = await ObtenerAño(Fecha.Year);

            // Verificar si la fecha existe en la lista de festivos
            return festivos.Any(f => f.Fecha.Date == Fecha.Date);
        }


        public async Task EsFestivo_FechaEsFestivo_RetornaTrue()
        {
            // Arrange
            var mockRepositorio = new Mock<IFestivoRepositorio>();
            var festivosMock = new List<Festivo>
        {
            new Festivo { IdTipo = 1, Mes = 12, Dia = 25, Nombre = "Navidad" } // Navidad
        };
            mockRepositorio.Setup(r => r.ObtenerTodos()).ReturnsAsync(festivosMock);

            var servicio = new FestivoServicio(mockRepositorio.Object);
            var fechaFestiva = new DateTime(2024, 12, 25); // Navidad 2024

            // Act
            var esFestivo = await servicio.EsFestivo(fechaFestiva);

            // Assert
            Assert.True(esFestivo); // La fecha es festiva
        }


        public async Task EsFestivo_FechaNoEsFestivo_RetornaFalse()
        {
            // Arrange
            var mockRepositorio = new Mock<IFestivoRepositorio>();
            var festivosMock = new List<Festivo>
        {
            new Festivo { IdTipo = 1, Mes = 12, Dia = 25, Nombre = "Navidad" } // Navidad
        };
            mockRepositorio.Setup(r => r.ObtenerTodos()).ReturnsAsync(festivosMock);

            var servicio = new FestivoServicio(mockRepositorio.Object);
            var fechaNoFestiva = new DateTime(2024, 12, 26); // Día no festivo

            // Act
            var esFestivo = await servicio.EsFestivo(fechaNoFestiva);

            // Assert
            Assert.False(esFestivo); // La fecha no es festiva
        }
    }

}

