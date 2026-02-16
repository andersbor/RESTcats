using Microsoft.EntityFrameworkCore;
using RESTcats.Models;
using System.Collections.ObjectModel;

namespace CatsTestProject1
{
    public class CatsRepsitoryTest
    {
        private bool useDatabase = true;
        private ICatsRepository repo;

        // some data for the test cases
        private Cat garfield = new() { Name = "Garfield", Weight = 100 };
        private Cat nedjem = new() { Name = "Nedjem", Weight = 20 };
        private Cat polleke = new() { Name = "Polleke", Weight = 12 };

        public CatsRepsitoryTest()
        {
            if (useDatabase)
            {
                var optionsBuilder = new DbContextOptionsBuilder<CatsDbContext>();
                // https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets
                optionsBuilder.UseSqlServer(Secrets.ConnectionStringSimply);
                // connection string structure
                //   "Data Source=mssql7.unoeuro.com;Initial Catalog=FROM simply.com;Persist Security Info=True;User ID=FROM simply.com;Password=DB PASSWORD FROM simply.com;TrustServerCertificate=True"
                CatsDbContext _dbContext = new(optionsBuilder.Options);
                // clean database table: remove all rows
                _dbContext.Database.ExecuteSqlRaw("TRUNCATE TABLE dbo.Cats");
                repo = new CatsRepositoryDatabase(_dbContext);
            }
            else
            {
                repo = new CatsRepositoryList(includeData: false);
            }
        }

        [Fact]
        public void AddCat_AssignsIdAndStores()
        {
            //ICatsRepository repo = new CatsRepositoryList(includeData: false);
            Cat newCat = new Cat { Name = "TestCat", Weight = 3 };

            Cat added = repo.AddCat(newCat);

            int firstId = added.Id;
            Assert.Equal(1, firstId);
            Assert.Equal("TestCat", added.Name);
            Assert.Equal(3, added.Weight);

            IEnumerable<Cat> all = repo.GetCats();
            int countAfterAdd = all.Count();
            Assert.Equal(1, countAfterAdd);

            // Add second cat and verify id increments
            Cat secondCat = new Cat { Name = "Second", Weight = 4 };
            Cat addedSecond = repo.AddCat(secondCat);
            Assert.Equal(2, addedSecond.Id);

            // Confirm original instance had its Id mutated by repository implementation
            Assert.Equal(1, newCat.Id);
            Assert.Equal(2, secondCat.Id);
        }

        [Fact]
        public void AddCat_Null_ThrowsArgumentNullException()
        {
            //ICatsRepository repo = new CatsRepositoryList(includeData: false);

            ArgumentNullException ex = Assert.Throws<ArgumentNullException>(() => repo.AddCat(null!));
            Assert.Equal("cat", ex.ParamName);
        }

        [Fact]
        public void GetCatById_ReturnsCorrectCatOrNull()
        {
            //ICatsRepository repo = new CatsRepositoryList(includeData: false);

            repo.AddCat(new Cat { Name = "Cat1", Weight = 2 });

            Cat? first = repo.GetCatById(1);
            Assert.NotNull(first);
            Assert.Equal(1, first!.Id);

            Cat? notFound = repo.GetCatById(999);
            Assert.Null(notFound);
        }

        [Fact]
  
        public void GetCats_OrderByNameAscendingAndDescending()
        {
            repo.AddCat(garfield);
            repo.AddCat(nedjem);
            repo.AddCat(polleke);

            IEnumerable<Cat> catsByNameAsc = repo.GetCats(orderBy: "name");
            IEnumerable<string> ascNames = catsByNameAsc.Select(c => c.Name!);
            List<string> expectedAsc = new List<string> { "Garfield", "Nedjem", "Polleke" };
            Assert.Equal(expectedAsc, ascNames);

            IEnumerable<Cat> catsByNameDesc = repo.GetCats(orderBy: "name_desc");
            IEnumerable<string> descNames = catsByNameDesc.Select(c => c.Name!);
            List<string> expectedDesc = new List<string> { "Polleke", "Nedjem", "Garfield" };
            Assert.Equal(expectedDesc, descNames);

            IEnumerable<Cat> catsByWeightAsc = repo.GetCats(orderBy: "weight");
            IEnumerable<int> ascWeights = catsByWeightAsc.Select(c => c.Weight);
            List<int> expectedWeightAsc = new List<int> { 12, 20, 100 };
            Assert.Equal(expectedWeightAsc, ascWeights);

            IEnumerable<Cat> catsByWeightDesc = repo.GetCats(orderBy: "weight_desc");
            IEnumerable<int> descWeights = catsByWeightDesc.Select(c => c.Weight);
            List<int> expectedWeightsDesc = new List<int> { 100, 20, 12 };
            Assert.Equal(expectedWeightsDesc, descWeights);
        }

        [Fact]
        public void GetCats_FilterNameContains_ReturnsOnlyMatching()
        {
            repo.AddCat(garfield);
            repo.AddCat(nedjem);
            repo.AddCat(polleke);

            IEnumerable<Cat> query = repo.GetCats(filterNameContains: "field");

            Assert.Single(query);
            List<Cat> list = query.ToList();
            Cat first = list[0];
            Assert.Equal("Garfield", first.Name);
            Assert.Equal(100, first.Weight);

            IEnumerable<Cat> namesContaingE = repo.GetCats(filterNameContains: "e");
            Assert.Equal(3, namesContaingE.Count());

            IEnumerable<Cat> highWeight = repo.GetCats(filterWeightAtLeast: 80);
            Assert.Single(highWeight);

            IEnumerable<Cat> mediumWeight = repo.GetCats(filterWeightAtLeast: 15);
            Assert.Equal(2, mediumWeight.Count());
        }

        [Fact]
        public void RemoveCat_RemovesAndReturns()
        {
            //ICatsRepository repo = new CatsRepositoryList(includeData: false);
            Cat cat = new Cat { Name = "ToRemove", Weight = 2 };
            Cat added = repo.AddCat(cat);

            Cat? removed = repo.RemoveCat(added.Id);
            Assert.NotNull(removed);
            Assert.Equal(added.Id, removed!.Id);

            Cat? postLookup = repo.GetCatById(added.Id);
            Assert.Null(postLookup);

            int remaining = repo.GetCats().Count();
            Assert.Equal(0, remaining);
        }

        [Fact]
        public void RemoveCat_NonExistent_ReturnsNull()
        {
            //ICatsRepository repo = new CatsRepositoryList(includeData: false);

            Cat? removed = repo.RemoveCat(12345);
            Assert.Null(removed);
        }

        [Fact]
        public void UpdateCat_UpdatesExisting_ReturnsUpdated()
        {
            //ICatsRepository repo = new CatsRepositoryList(includeData: false);
            Cat original = new Cat { Name = "Old", Weight = 2 };
            Cat added = repo.AddCat(original);

            Cat updatedPayload = new Cat { Name = "NewName", Weight = 5 };
            Cat? updated = repo.UpdateCat(added.Id, updatedPayload);

            Assert.NotNull(updated);
            Assert.Equal(added.Id, updated!.Id);
            Assert.Equal("NewName", updated.Name);
            Assert.Equal(5, updated.Weight);

            // Confirm repository stored the updated values
            Cat? stored = repo.GetCatById(added.Id);
            Assert.NotNull(stored);
            Assert.Equal("NewName", stored!.Name);
            Assert.Equal(5, stored.Weight);
        }

        [Fact]
        public void UpdateCat_NonExistent_ReturnsNull()
        {
            //ICatsRepository repo = new CatsRepositoryList(includeData: false);
            Cat payload = new Cat { Name = "Whatever", Weight = 1 };

            Cat? updated = repo.UpdateCat(42, payload);
            Assert.Null(updated);
        }
    }
}