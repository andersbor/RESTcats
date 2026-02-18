namespace RESTcats.Models
{
    public class CatsRepositoryListThreadSafe: ICatsRepository
    {
        private readonly List<Cat> cats = new();
        private int nextId = 0;
        private readonly object sync = new();

        public CatsRepositoryListThreadSafe(bool includeData = false)
        {
            if (includeData)
            {
                AddCat(new Cat { Name = "Whiskers", Weight = 4 });
                AddCat(new Cat { Name = "Mittens", Weight = 5 });
                AddCat(new Cat { Name = "Shadow", Weight = 6 });
            }
        }

        // Keep the original name for callers that expect it
        public IEnumerable<Cat> GetAllCats()
        {
            lock (sync)
            {
                // return a snapshot (read-only) to avoid exposing the internal list structure
                List<Cat> snapshot = cats.ToList();
                return snapshot.AsReadOnly();
            }
        }

        // Implementation required by ICatsRepository: supports the same filters/order semantics
        public IEnumerable<Cat> GetCats(string? filterNameContains = null,
            int? filterWeightAtLeast = null,
            string? orderBy = null)
        {
            lock (sync)
            {
                IEnumerable<Cat> query = cats;

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
                        break;
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
                        break;
                }

                // materialize a snapshot to avoid deferred execution outside the lock
                List<Cat> result = query.ToList();
                return result.AsReadOnly();
            }
        }

        public Cat? GetCatById(int id)
        {
            lock (sync)
            {
                return cats.FirstOrDefault(c => c.Id == id);
            }
        }

        public Cat AddCat(Cat cat)
        {
            if (cat is null)
            {
                throw new ArgumentNullException(nameof(cat));
            }

            // generate id in a thread-safe manner
            int assignedId = Interlocked.Increment(ref nextId);
            cat.Id = assignedId;

            lock (sync)
            {
                cats.Add(cat);
            }

            return cat;
        }

        public Cat? RemoveCat(int id)
        {
            lock (sync)
            {
                Cat? cat = cats.FirstOrDefault(c => c.Id == id);
                if (cat != null)
                {
                    cats.Remove(cat);
                    return cat;
                }
                return null;
            }
        }

        public Cat? UpdateCat(int id, Cat updatedCat)
        {
            if (updatedCat is null)
            {
                throw new ArgumentNullException(nameof(updatedCat));
            }

            lock (sync)
            {
                Cat? existingCat = cats.FirstOrDefault(c => c.Id == id);
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
}