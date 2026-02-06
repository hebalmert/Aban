using Aban.AppInfra;
using Aban.AppInfra.ErrorHandling;
using Aban.AppInfra.Transactions;
using Aban.Domain.Entities;
using Aban.Domain.Pagination;
using Aban.Domain.Resources;
using Aban.Services.ImplementEntties;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;

namespace Aban.xTests.Entities
{
    public class CountryServiceTests
    {
        private readonly DataContext _context;
        private readonly HttpErrorHandler _httpErrorHandler;   // REAL
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<ITransactionManager> _transactionManagerMock;
        private readonly Mock<IStringLocalizer> _localizerMock;
        private readonly Mock<ILogger<HttpErrorHandler>> _loggerMock;

        private readonly CountryService _service;

        public CountryServiceTests()
        {
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new DataContext(options);

            _localizerMock = new Mock<IStringLocalizer>();
            _loggerMock = new Mock<ILogger<HttpErrorHandler>>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _transactionManagerMock = new Mock<ITransactionManager>();

            // HttpErrorHandler REAL
            _httpErrorHandler = new HttpErrorHandler(_localizerMock.Object, _loggerMock.Object);

            // HttpContext requerido por ApplyFullPaginationAsync
            _httpContextAccessorMock.Setup(x => x.HttpContext)
                .Returns(new DefaultHttpContext());

            // TransactionManager
            _transactionManagerMock.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _transactionManagerMock.Setup(x => x.CommitTransactionAsync()).Returns(Task.CompletedTask);
            _transactionManagerMock.Setup(x => x.RollbackTransactionAsync()).Returns(Task.CompletedTask);
            _transactionManagerMock.Setup(x => x.SaveChangesAsync())
                .Returns(async () => await _context.SaveChangesAsync());

            SetupLocalizerDefaults();

            _service = new CountryService(
                _context,
                _httpErrorHandler,
                _httpContextAccessorMock.Object,
                _transactionManagerMock.Object,
                _localizerMock.Object
            );
        }

        private void SetupLocalizerDefaults()
        {
            _localizerMock.Setup(x => x[nameof(Resource.Select_Country)])
                .Returns(new LocalizedString(nameof(Resource.Select_Country), "[Select Country]"));

            _localizerMock.Setup(x => x["Generic_InvalidId"])
                .Returns(new LocalizedString("Generic_InvalidId", "Invalid Id"));

            _localizerMock.Setup(x => x["Generic_IdNotFound"])
                .Returns(new LocalizedString("Generic_IdNotFound", "Id not found"));

            _localizerMock.Setup(x => x["Generic_InvalidModel"])
                .Returns(new LocalizedString("Generic_InvalidModel", "Invalid model"));

            _localizerMock.Setup(x => x["Generic_Success"])
                .Returns(new LocalizedString("Generic_Success", "Success"));
        }

        private Country CreateCountry(int id = 1, string name = "USA")
        {
            return new Country
            {
                CountryId = id,
                Name = name
            };
        }

        // -------------------------------------------------------------
        // COMBO
        // -------------------------------------------------------------
        [Fact]
        public async Task ComboAsync_ShouldReturnDefaultItemPlusCountries()
        {
            _context.Countries.Add(CreateCountry(1, "USA"));
            _context.Countries.Add(CreateCountry(2, "Peru"));
            await _context.SaveChangesAsync();

            var result = await _service.ComboAsync();

            Assert.True(result.WasSuccess);
            Assert.Equal(3, result.Result!.Count());
            Assert.Equal("[Select Country]", result.Result!.First().Name);
        }

        // -------------------------------------------------------------
        // GET PAGINATED
        // -------------------------------------------------------------
        [Fact]
        public async Task GetAsync_Pagination_ShouldReturnFilteredCountries()
        {
            _context.Countries.Add(CreateCountry(1, "USA"));
            _context.Countries.Add(CreateCountry(2, "Peru"));
            _context.Countries.Add(CreateCountry(3, "Uruguay"));
            await _context.SaveChangesAsync();

            var pagination = new PaginationDTO
            {
                Page = 1,
                RecordsNumber = 10,
                Filter = "U"
            };

            var result = await _service.GetAsync(pagination);

            Assert.True(result.WasSuccess);

            // Solo filtra el IQueryable antes de paginar
            // Pero como RecordsNumber = 10 y hay 3 registros → devuelve los 3
            Assert.Equal(3, result.Result!.Count());

        }

        // -------------------------------------------------------------
        // GET BY ID
        // -------------------------------------------------------------
        [Fact]
        public async Task GetAsync_ById_ShouldReturnInvalidId_WhenIdZero()
        {
            var result = await _service.GetAsync(0);

            Assert.False(result.WasSuccess);
            Assert.Equal("Invalid Id", result.Message);
        }

        [Fact]
        public async Task GetAsync_ById_ShouldReturnNotFound_WhenMissing()
        {
            var result = await _service.GetAsync(999);

            Assert.False(result.WasSuccess);
            Assert.Equal("Id not found", result.Message);
        }

        [Fact]
        public async Task GetAsync_ById_ShouldReturnCountry_WhenExists()
        {
            _context.Countries.Add(CreateCountry(1, "USA"));
            await _context.SaveChangesAsync();

            var result = await _service.GetAsync(1);

            Assert.True(result.WasSuccess);
            Assert.Equal("USA", result.Result!.Name);
        }

        // -------------------------------------------------------------
        // ADD
        // -------------------------------------------------------------
        [Fact]
        public async Task AddAsync_ShouldReturnInvalidModel_WhenNull()
        {
            var result = await _service.AddAsync(null!);

            Assert.False(result.WasSuccess);
            Assert.Equal("Invalid model", result.Message);
        }

        [Fact]
        public async Task AddAsync_ShouldAddCountry_WhenValid()
        {
            var country = CreateCountry(1, "USA");

            var result = await _service.AddAsync(country);

            Assert.True(result.WasSuccess);
            Assert.Equal("Success", result.Message);
            Assert.Equal(1, _context.Countries.Count());
        }

        // -------------------------------------------------------------
        // UPDATE
        // -------------------------------------------------------------
        [Fact]
        public async Task UpdateAsync_ShouldReturnInvalidId_WhenIdZero()
        {
            var result = await _service.UpdateAsync(new Country { CountryId = 0 });

            Assert.False(result.WasSuccess);
            Assert.Equal("Invalid Id", result.Message);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateCountry_WhenValid()
        {
            var country = CreateCountry(1, "USA");
            _context.Countries.Add(country);
            await _context.SaveChangesAsync();

            country.Name = "Updated";

            var result = await _service.UpdateAsync(country);

            Assert.True(result.WasSuccess);
            Assert.Equal("Success", result.Message);
            Assert.Equal("Updated", _context.Countries.First().Name);
        }

        // -------------------------------------------------------------
        // DELETE
        // -------------------------------------------------------------
        [Fact]
        public async Task DeleteAsync_ShouldReturnInvalidId_WhenZero()
        {
            var result = await _service.DeleteAsync(0);

            Assert.False(result.WasSuccess);
            Assert.Equal("Invalid Id", result.Message);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnNotFound_WhenMissing()
        {
            var result = await _service.DeleteAsync(999);

            Assert.False(result.WasSuccess);
            Assert.Equal("Id not found", result.Message);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteCountry_WhenExists()
        {
            _context.Countries.Add(CreateCountry(1, "USA"));
            await _context.SaveChangesAsync();

            var result = await _service.DeleteAsync(1);

            Assert.True(result.WasSuccess);
            Assert.True(result.Result);
            Assert.Empty(_context.Countries);
        }
    }
}