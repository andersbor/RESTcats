using Microsoft.EntityFrameworkCore;
using RESTcats.Models;
using System.Collections.ObjectModel;

namespace CatsTestProject1
{
    enum Version { Ordinary, Database, ThreadSafe }

    public class CatsRepsitoryTest
    {
        private readonly Version version = Version.Ordinary;
        private ICatsRepository repo;

        public CatsRepsitoryTest()
        {
            switch (version)
            {
                case Version.Ordinary:
                    repo = new CatsRepositoryList(includeData: false);
                    break;
                case Version.Database:

                    var optionsBuilder = new DbContextOptionsBuilder<CatsDbContext>();
                    // https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets
                    optionsBuilder.UseSqlServer(Secrets.ConnectionStringSimply);
                    // connection string structure
                    //   "Data Source=mssql7.unoeuro.com;Initial Catalog=FROM simply.com;Persist Security Info=True;User ID=FROM simply.com;Password=DB PASSWORD FROM simply.com;TrustServerCertificate=True"
                    CatsDbContext _dbContext = new(optionsBuilder.Options);
                    // clean database table: remove all rows
                    _dbContext.Database.ExecuteSqlRaw("TRUNCATE TABLE dbo.Cats");
                    repo = new CatsRepositoryDatabase(_dbContext);
                    break;
                case Version.ThreadSafe:
                    repo = new CatsRepositoryListThreadSafe(includeData: false);
                    break;
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

            IEnumerable<Cat> all = repo.GetAllCats();
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
        public void GetAllCats_ReturnsReadOnlyCollection()
        {
            //ICatsRepository repo = new CatsRepositoryList(includeData: true);

            IEnumerable<Cat> all = repo.GetAllCats();

            ReadOnlyCollection<Cat> readOnly = Assert.IsType<ReadOnlyCollection<Cat>>(all);
            IList<Cat> asList = (IList<Cat>)readOnly;

            // Attempting to modify the returned collection should throw NotSupportedException
            NotSupportedException ex = Assert.Throws<NotSupportedException>(() => asList.Add(new Cat { Name = "X", Weight = 1 }));
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

            int remaining = repo.GetAllCats().Count();
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