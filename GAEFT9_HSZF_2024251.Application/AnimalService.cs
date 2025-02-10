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
    public class AnimalService : IAnimalService
    {
        private readonly BreedingDbContext _context;

        public AnimalService(BreedingDbContext context)
        {
            _context = context;
        }

        public void AddAnimal(Animal animal)
        {
            // Gender mező csak "Male" vagy "Female" lehet
            if (animal.Gender != "Male" && animal.Gender != "Female")
            {
                throw new ArgumentException("Invalid gender. Only 'Male' or 'Female' are allowed.");
            }

            _context.Animals.Add(animal);
            _context.SaveChanges();
        }

        public IEnumerable<Animal> GetAnimals()
        {
            return _context.Animals.AsNoTracking().ToList();
        }

        public Animal? GetAnimalById(int id)
        {
            return _context.Animals.AsNoTracking().FirstOrDefault(a => a.Id == id);
        }

        public void UpdateAnimal(int animalId, Animal updatedAnimal)
        {
            
            var animal = _context.Animals.FirstOrDefault(a => a.Id == animalId);

            if (animal == null)
            {
                throw new ArgumentException($"Animal with ID {animalId} not found.");
            }

            
            animal.Name = updatedAnimal.Name ?? animal.Name;
            animal.Gender = updatedAnimal.Gender ?? animal.Gender;
            animal.Species = updatedAnimal.Species ?? animal.Species;
            animal.Age = updatedAnimal.Age > 0 ? updatedAnimal.Age : animal.Age;
            animal.Notes = updatedAnimal.Notes ?? animal.Notes;

           
            if (updatedAnimal.MotherId.HasValue)
            {
                var mother = _context.Animals.FirstOrDefault(a => a.Id == updatedAnimal.MotherId.Value);
                if (mother == null || mother.Gender != "Female" || mother.Species != animal.Species)
                {
                    throw new ArgumentException("Invalid MotherId: Mother must exist, be Female, and match the species.");
                }
                animal.MotherId = updatedAnimal.MotherId;
            }

           
            if (updatedAnimal.FatherId.HasValue)
            {
                var father = _context.Animals.FirstOrDefault(a => a.Id == updatedAnimal.FatherId.Value);
                if (father == null || father.Gender != "Male" || father.Species != animal.Species)
                {
                    throw new ArgumentException("Invalid FatherId: Father must exist, be Male, and match the species.");
                }
                animal.FatherId = updatedAnimal.FatherId;
            }

            
            _context.SaveChanges();
        }

        public void DeleteAnimal(int animalId)
        {
            try
            {
              
                var animal = _context.Animals.FirstOrDefault(a => a.Id == animalId);

                if (animal == null)
                {
                    Console.WriteLine("Animal not found.");
                    return;
                }

                
                _context.Animals.Remove(animal);
                _context.SaveChanges();

                Console.WriteLine($"Animal with ID {animalId} has been successfully deleted.");
            }
            catch (DbUpdateException ex)
            {
                //  Az állat kapcsolatban szerepel egy Pairingben
                Console.WriteLine($"Cannot delete animal with ID {animalId} because it is referenced in a breeding pair.");
            }
            catch (Exception ex)
            {
                
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            }
        }

        // Lapozás
        public IEnumerable<Animal> GetAnimalsPaged(int pageNumber, int pageSize, string? gender = null, string? species = null, string? orderBy = null)
        {
            var query = _context.Animals.AsQueryable();

            // Szűrés
            if (!string.IsNullOrEmpty(gender))
            {
                query = query.Where(a => a.Gender == gender);
            }

            if (!string.IsNullOrEmpty(species))
            {
                query = query.Where(a => a.Species == species);
            }

            // Rendezés
            if (!string.IsNullOrEmpty(orderBy))
            {
                query = orderBy.ToLower() switch
                {
                    "age" => query.OrderBy(a => a.Age),
                    "name" => query.OrderBy(a => a.Name),
                    "species" => query.OrderBy(a => a.Species),
                    _ => query
                };
            }

            // Lapozás
            return query
                .AsNoTracking()
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        // Szűrés
        public IEnumerable<Animal> FilterAnimals(string? gender = null, string? species = null)
        {
            var query = _context.Animals.AsQueryable();

            if (!string.IsNullOrEmpty(gender))
            {
                query = query.Where(a => a.Gender == gender);
            }

            if (!string.IsNullOrEmpty(species))
            {
                query = query.Where(a => a.Species == species);
            }

            return query.AsNoTracking().ToList();
        }
    }
}
