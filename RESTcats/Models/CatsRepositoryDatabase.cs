namespace RESTcats.Models
{
    public class CatsRepositoryDatabase : ICatsRepository
    {
        private readonly CatsDbContext _context;
        public CatsRepositoryDatabase(CatsDbContext context)
        {
            _context = context;
        }

        public Cat AddCat(Cat cat)
        {
            if (cat is null)
            {
                throw new ArgumentNullException(nameof(cat));
            }
            _context.Cats.Add(cat);
            _context.SaveChanges();
            return cat;
        }

        public IEnumerable<Cat> GetCats(
            string? filterNameContains = null,
            int? filterWeightAtLeast = null,
            string? orderBy = null
            )
        {
            IQueryable<Cat> query = _context.Cats;
            if (filterNameContains != null)
            {
                query = query.Where(cat => cat.Name != null && cat.Name.Contains(filterNameContains));
            }
            if (filterWeightAtLeast != null)
            {
                query = query.Where(cat => cat.Weight >= filterWeightAtLeast);
            }
            switch (orderBy)
            {
                case null:
                    break; // do nothing
                case "name":
                case "name_asc":
                    query = query.OrderBy(cat => cat.Name);
                    break;
                case "name_desc":
                    query = query.OrderByDescending(cat => cat.Name);
                    break;
                case "weight":
                case "weight_asc":
                    query = query.OrderBy(cat => cat.Weight);
                    break;
                case "weight_desc":
                    query = query.OrderByDescending(cat => cat.Weight);
                    break;
                default:
                    break; // do nothing
                    //throw new ArgumentException("Unknown sort order: " + orderBy);
            }
            return query;
        }

        public Cat? GetCatById(int id)
        {
            return _context.Cats.Find(id);
        }

        public Cat? RemoveCat(int id)
        {
            var cat = GetCatById(id);
            if (cat != null)
            {
                _context.Cats.Remove(cat);
                _context.SaveChanges();
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
                _context.SaveChanges();
                return existingCat;
            }
            return null;
        }
    }
}
