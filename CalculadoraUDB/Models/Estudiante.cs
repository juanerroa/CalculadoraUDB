using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace CalculadoraUDB.Models
{
    public class Estudiante
    {
        public double Cum { get; set; }
        public int UvGanadas { get; set; }
        public double Promedio { get; set; }
        public double Avance { get; set; }

        public int MaxUV { get; set; }
        public double UM { get; set; }
        public EstudiantePerfil Perfil { get; set; }
        public List<MateriaPensum> Pensum { get; set; }
        public Expediente Expediente { get; set; }

        public async Task ActualizarResultados()
        {
            double UMs = 0; //Suma de unidades unidades de merito
            int UVs = 0; //Suma de unidades valorativas
            double sumNotas = 0; //Sumatoria de todas las notas no retiradas (Solo para calcular Promedio)
            
            var manteriasNoRetiradas = await Expediente.ConsolidadoMaterias.Where(m => !m.Resultado.Equals("Retirada") && !m.Resultado.Equals("Ret. Total")).ToDynamicListAsync();
            
            //listado que contendra las materias validas en el expediente para calcular el CUM
            List<MateriaExpediente> materiasValidas = new List<MateriaExpediente>(); 

            foreach (var materia in manteriasNoRetiradas)
            {
                //Para el promedio se toma todas las materias en el expediente que no sean retiradas sin importar que sean reprobadas
                sumNotas += materia.Nota;


                /*Para calcular el CUM 
                 * En primer lugar estar retiradas eso ya lo cumple la lista manteriasNoRetiradas que estoy recorriendo
                 * Si las materia aparece mas de una vez en el expediente quiere decir que fue reprobada al menos una vez,
                   por lo que es necesario solo tomar en cuenta la ultima vez que aparece en el expediente. 
                 */
                var materiaInExp = Expediente.ConsolidadoMaterias.ToList().Where(m => m.Codigo.Equals(materia.Codigo));
                if (materiaInExp.Count() == 1)
                {
                    materiasValidas.Add(materiaInExp.SingleOrDefault());
                }
                else if (!materiasValidas.Where(m => m.Codigo.Equals(materia.Codigo)).Any())
                {
                    materiasValidas.Add(materiaInExp.Last());
                }

            }

            foreach (var materia in materiasValidas)
            {
                if (Pensum.Where(m => m.Codigo.Equals(materia.Codigo)).Any()) //Si la materia existe en el pensum, se tomara en cuenta para el calculo
                {
                    var materiaPensum = Pensum.Where(m => m.Codigo.Equals(materia.Codigo)).SingleOrDefault();
                    UMs += (materia.Nota * materiaPensum.UV);
                    UVs += materiaPensum.UV;
                }
            }
            this.Cum = UMs / UVs;
            this.UvGanadas = UVs;
            this.Promedio = sumNotas / manteriasNoRetiradas.Count();
            this.Avance = (((double)UvGanadas / (double)MaxUV) * 100);
            this.UM = UMs; //Para calculo de CUM Deseado    
        }
        public double GetNotaForCumDeseado(double cumDeseado)
        {
            double Nota = 0;
            cumDeseado -= 0.05;
            try
            {
                int UV_faltantes = MaxUV - UvGanadas;

                Nota = ((double)cumDeseado - ((double)UM / (double)MaxUV)) * (((double)MaxUV) / ((double)UV_faltantes));

                Debug.WriteLine("Nota: " + Nota);
            }
            catch (Exception) { }
            return Nota;
        }
        public IEnumerable<MateriaExpediente> GetTopAprobadas()
        {
            var list = Expediente.ConsolidadoMaterias.Where(m => m.Resultado.Equals("Aprobada") || m.Resultado.Equals("Equivalencia")).OrderByDescending(m => m.Nota).ToList();
            if (list.Count() < 5)
                list = list.Take(list.Count()).ToList();
            else
                list = list.Take(5).ToList();

            return list;
        }
        public IEnumerable<MateriaExpediente> GetTopReprobadas()
        {
            var list = Expediente.ConsolidadoMaterias.Where(m => m.Resultado.Equals("Reprobada")).OrderBy(m => m.Nota).ToList();
            if (list.Count() < 5)
                list = list.Take(list.Count()).ToList();
            else
                list = list.Take(5).ToList();

            return list;
        }
        public IEnumerable<MateriaExpediente> GetTopRecursadas()
        {
            var list = Expediente.ConsolidadoMaterias.Where(m=>m.Matricula > 1).ToList();
            list = list.OrderByDescending(m => m.Matricula).ToList();

            if (list.Count() < 5)
                list = list.Take(list.Count()).ToList();
            else
                list = list.Take(5).ToList();

            return list;
        }

    }
}