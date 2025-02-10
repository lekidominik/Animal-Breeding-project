using GAEFT9_HSZF_2024251.Application;
using GAEFT9_HSZF_2024251.Model;
using GAEFT9_HSZF_2024251.Persistence.MsSql;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GAEFT9_HSZF_2024251.Test
{
    public class ApplicationTests
    {
        private BreedingDbContext _context;
        private PairingService _pairingService;
        private AnimalService _animalService;

        [SetUp]
        public void Setup()
        {
            // InMemory Database 
            var options = new DbContextOptionsBuilder<BreedingDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new BreedingDbContext(options);

            // Tesztadatok
            _context.Animals.AddRange(new List<Animal>
            {
                new Animal { Id = 1, Name = "Bella", Gender = "Female", Species = "Dog" },
                new Animal { Id = 2, Name = "Max", Gender = "Male", Species = "Dog" },
                new Animal { Id = 3, Name = "Luna", Gender = "Female", Species = "Cat" },
                new Animal { Id = 4, Name = "Charlie", Gender = "Male", Species = "Cat" },
                new Animal { Id = 5, Name = "Daisy", Gender = "Female", Species = "Horse" },
                new Animal { Id = 6, Name = "Thunder", Gender = "Male", Species = "Horse" }
            });
            _context.SaveChanges();

            _pairingService = new PairingService(_context);
            _animalService = new AnimalService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted(); // Tesztadatbázist töröljük a tesztek között
            _context.Dispose();
        }

        [Test]
        public void AddAnimal_ShouldAddValidAnimal()
        {
            var animal = new Animal { Name = "Buddy", Gender = "Male", Species = "Dog", Age = 2 };
            _animalService.AddAnimal(animal);
            var animals = _animalService.GetAnimalsPaged(1, 10).ToList();
            ClassicAssert.AreEqual(7, animals.Count);
            ClassicAssert.AreEqual("Buddy", animals.Last().Name);
        }

        [Test]
        public void AddPairing_ShouldAddValidPairing()
        {
            _pairingService.AddPairing(1, 2, DateTime.Now.AddDays(-10));
            ClassicAssert.AreEqual(1, _context.Pairings.Count());
        }

        [Test]
        public void AddPairing_ShouldNotAllowDifferentSpecies()
        {
            _pairingService.AddPairing(1, 4, DateTime.Now.AddDays(-10));
            ClassicAssert.AreEqual(0, _context.Pairings.Count());
        }

        [Test]
        public void AddPairing_ShouldNotAllowFutureDate()
        {
            _pairingService.AddPairing(1, 2, DateTime.Now.AddDays(10));
            ClassicAssert.AreEqual(0, _context.Pairings.Count());
        }

        [Test]
        public void AddPairing_ShouldRespectGenderRoles()
        {
            _pairingService.AddPairing(2, 1, DateTime.Now.AddDays(-10));
            ClassicAssert.AreEqual(0, _context.Pairings.Count());
        }

        [Test]
        public void GetAnimalsPaged_ShouldReturnCorrectPage()
        {
            var pagedAnimals = _animalService.GetAnimalsPaged(1, 2).ToList();
            ClassicAssert.AreEqual(2, pagedAnimals.Count);
            ClassicAssert.AreEqual("Bella", pagedAnimals.First().Name);
        }

        [Test]
        public void GetAnimalsPaged_ShouldReturnEmptyForInvalidPage()
        {
            var pagedAnimals = _animalService.GetAnimalsPaged(10, 2).ToList();
            ClassicAssert.AreEqual(0, pagedAnimals.Count());
        }

        [Test]
        public void UpdateAnimal_ShouldUpdateExistingAnimal()
        {
            var animal = _context.Animals.First(a => a.Id == 1);
            animal.Age = 4;
            _animalService.UpdateAnimal(1, animal);
            ClassicAssert.AreEqual(4, _context.Animals.First(a => a.Id == 1).Age);
        }

        [Test]
        public void AddAnimal_ShouldNotAllowInvalidGender()
        {
            
            var animal = new Animal { Name = "Buddy", Gender = "Unknown", Species = "Dog", Age = 2 };

           
            var ex = Assert.Throws<ArgumentException>(() => _animalService.AddAnimal(animal));
            Assert.That(ex.Message, Is.EqualTo("Invalid gender. Only 'Male' or 'Female' are allowed."));
        }

        [Test]
        public void AddPairing_ShouldNotAllowInvalidMotherId()
        {
            _pairingService.AddPairing(999, 2, DateTime.Now.AddDays(-10)); 
            ClassicAssert.AreEqual(0, _context.Pairings.Count());
        }

        [Test]
        public void AddPairing_ShouldNotAllowInvalidFatherId()
        {
            _pairingService.AddPairing(1, 999, DateTime.Now.AddDays(-10));
            ClassicAssert.AreEqual(0, _context.Pairings.Count());
        }

        [Test]
        public void GetAnimals_ShouldFilterBySpecies()
        {
            var filteredAnimals = _animalService.GetAnimalsPaged(1, 10)
                                                .Where(a => a.Species == "Dog").ToList();
            ClassicAssert.AreEqual(2, filteredAnimals.Count);
            ClassicAssert.IsTrue(filteredAnimals.All(a => a.Species == "Dog"));
        }
    }
}
