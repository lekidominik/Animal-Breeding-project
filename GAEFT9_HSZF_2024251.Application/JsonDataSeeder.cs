using GAEFT9_HSZF_2024251.Model;
using GAEFT9_HSZF_2024251.Persistence.MsSql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GAEFT9_HSZF_2024251.Application
{
    public static class JsonDataSeeder
    {
        public static void SeedFromJson(BreedingDbContext context, string jsonFilePath)
        {
            try
            {
                var jsonData = File.ReadAllText(jsonFilePath);
                if (string.IsNullOrWhiteSpace(jsonData))
                {
                    Console.WriteLine("JSON file is empty or invalid.");
                    return;
                }

               
                var jsonRoot = JsonSerializer.Deserialize<JsonRoot>(jsonData);
                if (jsonRoot == null)
                {
                    Console.WriteLine("Invalid JSON structure.");
                    return;
                }

               
                if (jsonRoot.Animals != null && jsonRoot.Animals.Any())
                {
                    if (!context.Animals.Any())
                    {
                        context.Animals.AddRange(jsonRoot.Animals);
                        context.SaveChanges();
                        Console.WriteLine("Animals seeded successfully.");
                    }
                    else
                    {
                        Console.WriteLine("Animals table already has data.");
                    }
                }
                else
                {
                    Console.WriteLine("No valid Animals data found in JSON.");
                }

               
                if (jsonRoot.Pairings != null && jsonRoot.Pairings.Any())
                {
                    if (!context.Pairings.Any())
                    {
                        context.Pairings.AddRange(jsonRoot.Pairings);
                        context.SaveChanges();
                        Console.WriteLine("Pairings seeded successfully.");
                    }
                    else
                    {
                        Console.WriteLine("Pairings table already has data.");
                    }
                }
                else
                {
                    Console.WriteLine("No valid Pairings data found in JSON.");
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error deserializing JSON: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
        }

        
        private class JsonRoot
        {
            public List<Animal>? Animals { get; set; }
            public List<Pairing>? Pairings { get; set; }
        }
    }

}

