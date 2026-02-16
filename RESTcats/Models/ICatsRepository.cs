
namespace RESTcats.Models
{
    public interface ICatsRepository
    {
        Cat AddCat(Cat cat);
        IEnumerable<Cat> GetCats(string? filterNameContains = null,
            int? filterWeightAtLeast = null,
            string? orderBy = null);
        Cat? GetCatById(int id);
        Cat? RemoveCat(int id);
        Cat? UpdateCat(int id, Cat updatedCat);
    }
}