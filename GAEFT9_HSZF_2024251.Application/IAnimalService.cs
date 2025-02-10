using GAEFT9_HSZF_2024251.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAEFT9_HSZF_2024251.Application
{
    public interface IAnimalService
    {
        void AddAnimal(Animal animal);
        IEnumerable<Animal> GetAnimals();
        Animal? GetAnimalById(int id);
        void UpdateAnimal(int id, Animal updatedAnimal);
        void DeleteAnimal(int id);


       
        IEnumerable<Animal> GetAnimalsPaged(int pageNumber, int pageSize, string? gender = null, string? species = null, string? orderBy = null);

       
        IEnumerable<Animal> FilterAnimals(string? gender = null, string? species = null);
    }
}
