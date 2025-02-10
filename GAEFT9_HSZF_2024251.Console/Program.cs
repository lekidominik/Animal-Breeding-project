using GAEFT9_HSZF_2024251.Application;
using GAEFT9_HSZF_2024251.Model;
using GAEFT9_HSZF_2024251.Persistence.MsSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

class Program
{
    static void Main(string[] args)
    {
        // Dependency Injection 
        var serviceProvider = new ServiceCollection()
    .AddDbContext<BreedingDbContext>(options =>
     options.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=BreedingDb;Trusted_Connection=True;"))
    .AddScoped<IAnimalService, AnimalService>()
    .AddScoped<IPairingService, PairingService>() 
    .AddScoped<IReportService, ReportService>()  
    .BuildServiceProvider();

        var context = serviceProvider.GetRequiredService<BreedingDbContext>();
        context.Database.EnsureCreated();
        JsonDataSeeder.SeedFromJson(context, "data.json");

        var animalService = serviceProvider.GetService<IAnimalService>();
        if (animalService == null)
        {
            Console.WriteLine("AnimalService is not available.");
            return;
        }

        // Menü
        bool exit = false;
        while (!exit)
        {
            Console.WriteLine("\n=== Animal Management System ===");
            Console.WriteLine("1. Add a new animal");
            Console.WriteLine("2. List animals ");
            Console.WriteLine("3. Filter animals");
            Console.WriteLine("4. Update animal notes");
            Console.WriteLine("5. Delete an animal");
            Console.WriteLine("6. Pair animals");
            Console.WriteLine("7. Pregnancy warnings");
            Console.WriteLine("8. Generate animal report");
            Console.WriteLine("9. Generate species statistics");
            Console.WriteLine("0. Exit");
            Console.Write("Select an option: ");

            var choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    AddAnimal(animalService);
                    break;
                case "2":
                    ListAnimals(animalService);
                    break;
                case "3":
                    FilterAnimals(animalService);
                    break;
                case "4":
                    UpdateAnimal(animalService);
                    break;
                case "5":
                    DeleteAnimal(animalService);
                    break;
                case "6":
                    AddPairing(serviceProvider.GetRequiredService<IPairingService>());
                    break;
                case "7":
                    CheckPregnancyWarnings(serviceProvider.GetRequiredService<IPairingService>());
                    break;
                case "8":
                    GenerateAnimalReport(serviceProvider.GetRequiredService<IReportService>());
                    break;
                case "9":
                    GenerateSpeciesStatistics(serviceProvider.GetRequiredService<IReportService>());
                    break;
                case "0":
                    exit = true;
                    Console.WriteLine("Exiting the program...");
                    break;
                default:
                    Console.WriteLine("Invalid option. Please try again.");
                    break;
            }
        }
    }

    static void AddAnimal(IAnimalService animalService)
    {
        Console.Write("Enter name: ");
        string name = Console.ReadLine() ?? "";

        string gender;
        do
        {
            Console.Write("Enter gender (Male/Female): ");
            gender = Console.ReadLine() ?? "";
            if (gender != "Male" && gender != "Female")
            {
                Console.WriteLine("Invalid gender. Please enter 'Male' or 'Female'.");
            }
        } while (gender != "Male" && gender != "Female");

        Console.Write("Enter species: ");
        string species = Console.ReadLine() ?? "";

        int age;
        do
        {
            Console.Write("Enter age (positive integer): ");
            string ageInput = Console.ReadLine();
            if (!int.TryParse(ageInput, out age) || age < 0)
            {
                Console.WriteLine("Invalid age. Please enter a positive integer.");
            }
        } while (age < 0);

        Console.Write("Enter notes: ");
        string notes = Console.ReadLine() ?? "";

        int? motherId = null;
        Console.Write("Enter Mother ID (leave blank if none): ");
        string motherInput = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(motherInput) && int.TryParse(motherInput, out int parsedMotherId))
        {
            var mother = animalService.GetAnimalById(parsedMotherId);
            if (mother != null && mother.Gender == "Female" && mother.Species == species)
            {
                motherId = parsedMotherId;
            }
            else
            {
                Console.WriteLine($"Invalid Mother ID. It must be a valid Female animal of the same species ({species}).");
            }
        }

        int? fatherId = null;
        Console.Write("Enter Father ID (leave blank if none): ");
        string fatherInput = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(fatherInput) && int.TryParse(fatherInput, out int parsedFatherId))
        {
            var father = animalService.GetAnimalById(parsedFatherId);
            if (father != null && father.Gender == "Male" && father.Species == species)
            {
                fatherId = parsedFatherId;
            }
            else
            {
                Console.WriteLine($"Invalid Father ID. It must be a valid Male animal of the same species ({species}).");
            }
        }

        var newAnimal = new Animal
        {
            Name = name,
            Gender = gender,
            Species = species,
            Age = age,
            Notes = notes,
            MotherId = motherId,
            FatherId = fatherId
        };

        animalService.AddAnimal(newAnimal);
        Console.WriteLine($"Animal '{name}' added successfully!");
    }
    static void ListAnimals(IAnimalService animalService)
    {
        int pageNumber = 1;
        const int pageSize = 10;

        string? genderFilter = null;
        string? speciesFilter = null;
        string? orderBy = null;

        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== Animal List ===");
            Console.WriteLine($"Page {pageNumber}");
            Console.WriteLine("Filters: ");
            Console.WriteLine($"  Gender: {genderFilter ?? "None"}");
            Console.WriteLine($"  Species: {speciesFilter ?? "None"}");
            Console.WriteLine($"  Order by: {orderBy ?? "None"}");

            var animals = animalService.GetAnimalsPaged(pageNumber, pageSize, genderFilter, speciesFilter, orderBy);

            if (!animals.Any())
            {
                Console.WriteLine("No animals to display.");
            }
            else
            {
                foreach (var animal in animals)
                {
                    Console.WriteLine($"- {animal.Name} ({animal.Species}, {animal.Gender}, Age: {animal.Age})");
                }
            }

            Console.WriteLine("\nOptions:");
            Console.WriteLine("[N] Next page");
            Console.WriteLine("[P] Previous page");
            Console.WriteLine("[F] Set filters");
            Console.WriteLine("[O] Set order");
            Console.WriteLine("[Q] Quit");
            Console.Write("Select an option: ");

            var choice = Console.ReadLine()?.ToUpper();

            switch (choice)
            {
                case "N":
                    pageNumber++;
                    break;
                case "P":
                    if (pageNumber > 1) pageNumber--;
                    break;
                case "F":
                    Console.Write("Enter gender filter (Male/Female or leave blank): ");
                    genderFilter = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(genderFilter)) genderFilter = null;

                    Console.Write("Enter species filter (or leave blank): ");
                    speciesFilter = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(speciesFilter)) speciesFilter = null;

                    pageNumber = 1; 
                    break;
                case "O":
                    Console.Write("Enter order by field (Name, Age, Species or leave blank): ");
                    orderBy = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(orderBy)) orderBy = null;

                    pageNumber = 1; 
                    break;
                case "Q":
                    return;
                default:
                    Console.WriteLine("Invalid option. Please try again.");
                    break;
            }
        }
    }

    

    static void FilterAnimals(IAnimalService animalService)
    {
        Console.Write("Enter gender to filter (or leave blank): ");
        string gender = Console.ReadLine() ?? "";
        Console.Write("Enter species to filter (or leave blank): ");
        string species = Console.ReadLine() ?? "";

        var animals = animalService.FilterAnimals(gender, species);
        Console.WriteLine("\nFiltered animals:");
        foreach (var animal in animals)
        {
            Console.WriteLine($"- {animal.Name} ({animal.Species}, {animal.Gender}, Age: {animal.Age})");
        }
    }

    static void UpdateAnimal(IAnimalService animalService)
    {
        Console.Write("Enter the ID of the animal to update: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("Invalid ID. Please enter a numeric value.");
            return;
        }

        var existingAnimal = animalService.GetAnimalById(id);
        if (existingAnimal == null)
        {
            Console.WriteLine("Animal not found.");
            return;
        }

        Console.WriteLine("Leave a field blank to keep the current value.");

        Console.Write($"Enter new name (current: {existingAnimal.Name}): ");
        string name = Console.ReadLine();
        if (!string.IsNullOrEmpty(name))
        {
            existingAnimal.Name = name;
        }

        string gender;
        do
        {
            Console.Write($"Enter new gender (current: {existingAnimal.Gender}, Male/Female): ");
            gender = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(gender))
            {
                gender = existingAnimal.Gender;
            }
            else if (gender != "Male" && gender != "Female")
            {
                Console.WriteLine("Invalid gender. Please enter 'Male' or 'Female'.");
                gender = null;
            }
        } while (gender == null);
        existingAnimal.Gender = gender;

        Console.Write($"Enter new species (current: {existingAnimal.Species}): ");
        string species = Console.ReadLine();
        if (!string.IsNullOrEmpty(species))
        {
            existingAnimal.Species = species;
        }

        Console.Write($"Enter new age (current: {existingAnimal.Age}, positive integer): ");
        string ageInput = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(ageInput) && int.TryParse(ageInput, out int age) && age >= 0)
        {
            existingAnimal.Age = age;
        }

        Console.Write($"Enter new notes (current: {existingAnimal.Notes}): ");
        string notes = Console.ReadLine();
        if (!string.IsNullOrEmpty(notes))
        {
            existingAnimal.Notes = notes;
        }

        Console.Write("Enter new Mother ID (current: {0}, leave blank if none): ", existingAnimal.MotherId);
        string motherInput = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(motherInput) && int.TryParse(motherInput, out int parsedMotherId))
        {
            var mother = animalService.GetAnimalById(parsedMotherId);
            if (mother != null && mother.Gender == "Female" && mother.Species == existingAnimal.Species)
            {
                existingAnimal.MotherId = parsedMotherId;
            }
            else
            {
                Console.WriteLine($"Invalid Mother ID. It must be a valid Female animal of the same species ({existingAnimal.Species}).");
            }
        }

        Console.Write("Enter new Father ID (current: {0}, leave blank if none): ", existingAnimal.FatherId);
        string fatherInput = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(fatherInput) && int.TryParse(fatherInput, out int parsedFatherId))
        {
            var father = animalService.GetAnimalById(parsedFatherId);
            if (father != null && father.Gender == "Male" && father.Species == existingAnimal.Species)
            {
                existingAnimal.FatherId = parsedFatherId;
            }
            else
            {
                Console.WriteLine($"Invalid Father ID. It must be a valid Male animal of the same species ({existingAnimal.Species}).");
            }
        }

        animalService.UpdateAnimal(id, existingAnimal);
        Console.WriteLine("Animal updated successfully!");
    }

    static void DeleteAnimal(IAnimalService animalService)
    {
        Console.Write("Enter the ID of the animal to delete: ");
        if (int.TryParse(Console.ReadLine(), out int animalId))
        {
            animalService.DeleteAnimal(animalId);
        }
        else
        {
            Console.WriteLine("Invalid ID. Please enter a numeric value.");
        }
    }
    static void AddPairing(IPairingService pairingService)
    {
        Console.Write("Enter the ID of the mother: ");
        int motherId = int.TryParse(Console.ReadLine(), out var parsedMotherId) ? parsedMotherId : 0;

        Console.Write("Enter the ID of the father: ");
        int fatherId = int.TryParse(Console.ReadLine(), out var parsedFatherId) ? parsedFatherId : 0;

        Console.Write("Enter the pairing date (yyyy-mm-dd): ");
        if (!DateTime.TryParse(Console.ReadLine(), out var pairingDate) || pairingDate > DateTime.Now)
        {
            Console.WriteLine("Invalid date. Please provide a valid date in the format yyyy-mm-dd.");
            return;
        }

        pairingService.AddPairing(motherId, fatherId, pairingDate);
    }
    static void CheckPregnancyWarnings(IPairingService pairingService)
    {
        Console.WriteLine("Checking for pregnancy warnings...");
        var warnings = pairingService.CheckPregnancyWarnings();

        if (!warnings.Any())
        {
            Console.WriteLine("No pregnancy warnings at this time.");
        }
    }
    static void GenerateAnimalReport(IReportService reportService)
    {
        Console.Write("Enter the ID of the animal for the report: ");
        if (int.TryParse(Console.ReadLine(), out var animalId))
        {
            reportService.SaveAnimalReport(animalId);
        }
        else
        {
            Console.WriteLine("Invalid ID. Please enter a numeric value.");
        }
    }
    static void GenerateSpeciesStatistics(IReportService reportService)
    {
        Console.WriteLine("Generating species statistics...");
        reportService.SaveSpeciesStatistics();
    }
}