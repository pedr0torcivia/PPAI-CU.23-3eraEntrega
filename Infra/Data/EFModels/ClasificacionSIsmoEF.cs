// Infra/Data/EFModels/ClasificacionSismoEF.cs
using System;

namespace PPAI_2.Infra.Data.EFModels
{
    public class ClasificacionSismoEF
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; }
        public double KmProfundidadDesde { get; set; }
        public double KmProfundidadHasta { get; set; }
    }
}
