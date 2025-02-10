using GAEFT9_HSZF_2024251.Model;
using GAEFT9_HSZF_2024251.Persistence.MsSql;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAEFT9_HSZF_2024251.Application
{
    public class ReportService : IReportService
    {
        private readonly BreedingDbContext _context;

        public ReportService(BreedingDbContext context)
        {
            _context = context;
        }

        public void SaveAnimalReport(int animalId)
        {
            // Az állat betöltése az adatbázisból, szülők adataival együtt
            var animal = _context.Animals
                .Include(a => a.Mother)
                .Include(a => a.Father)
                .FirstOrDefault(a => a.Id == animalId);

            if (animal == null)
            {
                Console.WriteLine("Animal not found.");
                return;
            }

            // A jelentés tartalmának előkészítése
            var reportContent = new StringBuilder();
            reportContent.AppendLine("Animal Report:");
            reportContent.AppendLine($"ID: {animal.Id}");
            reportContent.AppendLine($"Name: {animal.Name}");
            reportContent.AppendLine($"Gender: {animal.Gender}");
            reportContent.AppendLine($"Species: {animal.Species}");
            reportContent.AppendLine($"Age: {animal.Age}");
            reportContent.AppendLine($"Notes: {animal.Notes}");
            reportContent.AppendLine();
            reportContent.AppendLine("Ancestors:");
            AppendAncestors(reportContent, animal, 0);

            // Jelentés mentése TXT fájlba
            var reportPath = Path.Combine("Reports", DateTime.Now.Year.ToString());
            Directory.CreateDirectory(reportPath);

            var fileName = $"Animal_Report_{animal.Name}_{DateTime.Now:yyyy-MM-dd}.txt";
            var filePath = Path.Combine(reportPath, fileName);

            File.WriteAllText(filePath, reportContent.ToString());
            Console.WriteLine($"Animal report saved to {filePath}");
        }

        //  Felmenők kiírása rekurzívan
        private void AppendAncestors(StringBuilder reportContent, Animal animal, int generation)
        {
            if (animal == null) return;

            var indent = new string(' ', generation * 4); 

            if (animal.Mother != null)
            {
                reportContent.AppendLine($"{indent}Mother: {animal.Mother.Name} (ID: {animal.Mother.Id}, Species: {animal.Mother.Species})");
                AppendAncestors(reportContent, _context.Animals.Include(a => a.Mother).Include(a => a.Father).FirstOrDefault(a => a.Id == animal.MotherId), generation + 1);
            }
            else
            {
                reportContent.AppendLine($"{indent}Mother: Unknown");
            }

            if (animal.Father != null)
            {
                reportContent.AppendLine($"{indent}Father: {animal.Father.Name} (ID: {animal.Father.Id}, Species: {animal.Father.Species})");
                AppendAncestors(reportContent, _context.Animals.Include(a => a.Mother).Include(a => a.Father).FirstOrDefault(a => a.Id == animal.FatherId), generation + 1);
            }
            else
            {
                reportContent.AppendLine($"{indent}Father: Unknown");
            }
        }



        public void SaveSpeciesStatistics()
        {
            var currentYear = DateTime.Now.Year;

            // Csoportosítás születési év és faj szerint
            var stats = _context.Animals
                .Where(a => a.Age >= 0) // Csak érvényes korú állatok
                .GroupBy(a => new { BirthYear = currentYear - a.Age, a.Species })
                .Select(g => new
                {
                    g.Key.BirthYear,
                    g.Key.Species,
                    Count = g.Count()
                })
                .OrderBy(g => g.BirthYear)
                .ThenBy(g => g.Species)
                .ToList();

            var reportPath = Path.Combine("Reports", DateTime.Now.Year.ToString());
            Directory.CreateDirectory(reportPath);

            var fileName = $"Species_Statistics_{DateTime.Now:yyyy-MM-dd}.txt";
            var filePath = Path.Combine(reportPath, fileName);

            var reportContent = new StringBuilder();
            reportContent.AppendLine("Species Birth Statistics:");
            foreach (var stat in stats)
            {
                reportContent.AppendLine($"Year: {stat.BirthYear}, Species: {stat.Species}, Count: {stat.Count}");
            }

            File.WriteAllText(filePath, reportContent.ToString());
            Console.WriteLine($"Species statistics saved to {filePath}");
        }
    }
}
