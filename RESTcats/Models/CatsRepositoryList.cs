namespace RESTcats.Models
{
    public class CatsRepositoryList : ICatsRepository
    {
        private readonly List<Cat> cats = new();
        private int nextId = 1;

        public CatsRepositoryList(bool includeData = false)
        {
            if (includeData)
            {
                AddCat(new Cat { Name = "Whiskers", Weight = 4 });
                AddCat(new Cat { Name = "Mittens", Weight = 5 });
                AddCat(new Cat { Name = "Shadow", Weight = 6 });
            }
        }

        public IEnumerable<Cat> GetCats(string? filterNameContains = null,
            int? filterWeightAtLeast = null,
            string? orderBy = null
            )
        {
            // TODO use filter + order
            return cats.AsReadOnly();
        }
        public Cat? GetCatById(int id)
        {
            return cats.FirstOrDefault(c => c.Id == id);
        }
        public Cat AddCat(Cat cat)
        {
            if (cat is null)
            {
                throw new ArgumentNullException(nameof(cat));
            }
            cat.Id = nextId++;
            cats.Add(cat);
            return cat;
        }

        public Cat? RemoveCat(int id)
        {
            var cat = GetCatById(id);
            if (cat != null)
            {
                cats.Remove(cat);
                return cat;
            }
            return null;
        }

        public Cat? UpdateCat(int id, Cat updatedCat)
        {
            var existingCat = GetCatById(id);
            if (existingCat != null)
            {
                existingCat.Name = updatedCat.Name;
                existingCat.Weight = updatedCat.Weight;
                return existingCat;
            }
            return null;
        }
    }
}