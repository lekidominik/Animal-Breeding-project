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
    public class PairingService : IPairingService
    {
        private readonly BreedingDbContext _context;

        public PairingService(BreedingDbContext context)
        {
            _context = context;
        }
        private readonly Dictionary<string, int> PregnancyDurations = new()
        {
            { "Dog", 70 },
            { "Cat", 60 },
            { "Horse", 365 }
        };


        public void AddPairing(int motherId, int fatherId, DateTime? pairingDate, string? notes = null)
        {
            // Ellenőrizzük, hogy a dátum érvényes-e
            if (pairingDate == null || pairingDate > DateTime.Now)
            {
                Console.WriteLine("Invalid pairing date. Please provide a valid date that is not in the future.");
                return;
            }

            var mother = _context.Animals.Find(motherId);
            var father = _context.Animals.Find(fatherId);

            if (mother == null || father == null)
            {
                Console.WriteLine("One or both animals not found.");
                return;
            }

            if (mother.Gender != "Female")
            {
                Console.WriteLine($"Invalid pairing: Mother must be female. {mother.Name} is {mother.Gender}.");
                return;
            }

            if (father.Gender != "Male")
            {
                Console.WriteLine($"Invalid pairing: Father must be male. {father.Name} is {father.Gender}.");
                return;
            }

            if (mother.Species != father.Species)
            {
                Console.WriteLine($"Cannot pair animals of different species: {mother.Species} and {father.Species}.");
                return;
            }

            var pairing = new Pairing
            {
                MotherId = motherId,
                FatherId = fatherId,
                PairingDate = pairingDate.Value,
                Notes = notes
            };

            _context.Pairings.Add(pairing);
            _context.SaveChanges();

            Console.WriteLine($"Pairing added: {mother.Name} and {father.Name} on {pairingDate:yyyy-MM-dd}");
        }

        public IEnumerable<Pairing> CheckPregnancyWarnings()
        {
            var warnings = new List<Pairing>();

            foreach (var pairing in _context.Pairings.Include(p => p.Mother).Include(p => p.Father))
            {
                if (PregnancyDurations.TryGetValue(pairing.Mother.Species, out int pregnancyDuration))
                {
                    var warningDate = pairing.PairingDate.AddDays(pregnancyDuration);
                    if (DateTime.Now >= warningDate)
                    {
                        warnings.Add(pairing);
                        Console.WriteLine($"Pregnancy warning: {pairing.Mother.Name} and {pairing.Father.Name} (paired on {pairing.PairingDate:yyyy-MM-dd})");
                    }
                }
                else
                {
                    Console.WriteLine($"Unknown species: {pairing.Mother.Species}. Cannot determine pregnancy duration.");
                }
            }

            return warnings;
        }

        public bool IsPairingAllowed(int motherId, int fatherId)
        {
            var mother = _context.Animals.Find(motherId);
            var father = _context.Animals.Find(fatherId);

            if (mother == null || father == null) return false;

            return mother.Species == father.Species;
        }
    


        public IEnumerable<Pairing> GetPairings()
        {
            return _context.Pairings
                .Include(p => p.Mother)
                .Include(p => p.Father)
                .AsNoTracking()
                .ToList();
        }

        public IEnumerable<Pairing> CheckPregnancyWarnings(int pregnancyDurationDays)
        {
            var warningDate = DateTime.Now.AddDays(-pregnancyDurationDays);
            return _context.Pairings
                .Include(p => p.Mother)
                .Include(p => p.Father)
                .Where(p => p.PairingDate <= warningDate)
                .AsNoTracking()
                .ToList();
        }
    }
}

