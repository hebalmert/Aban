using Aban.AppInfra;
using Aban.AppInfra.ErrorHandling;
using Aban.AppInfra.Transactions;
using Aban.Domain.Entities;
using Aban.Services.ImplementEntties;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;

namespace Aban.xTests.Entities
{
    public class CityServiceTests
    {
        private readonly DataContext _context;
        private readonly HttpErrorHandler _httpErrorHandler;   // ← REAL
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<ITransactionManager> _transactionManagerMock;
        private readonly Mock<IStringLocalizer> _localizerMock;
        private readonly Mock<ILogger<HttpErrorHandler>> _loggerMock;

        private readonly CityService _service;

        public CityServiceTests()
        {
            // InMemory DB
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new DataContext(options);

            // Mocks
            _localizerMock = new Mock<IStringLocalizer>();
            _loggerMock = new Mock<ILogger<HttpErrorHandler>>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _transactionManagerMock = new Mock<ITransactionManager>();

            // HttpErrorHandler REAL con dependencias mockeadas
            _httpErrorHandler = new HttpErrorHandler(_localizerMock.Object, _loggerMock.Object);

            // HttpContext requerido por ApplyFullPaginationAsync
            _httpContextAccessorMock.Setup(x => x.HttpContext)
                .Returns(new DefaultHttpContext());

            // Servicio bajo prueba
            _service = new CityService(
                _context,
                _httpErrorHandler,
                _httpContextAccessorMock.Object,
                _transactionManagerMock.Object,
                _localizerMock.Object
            );
        }



        [Fact]
        public async Task ComboAsync_ShouldReturnCities_WhenStateIdMatches()
        {
            _context.Cities.Add(new City { CityId = 1, Name = "Miami", StateId = 10 });
            _context.Cities.Add(new City { CityId = 2, Name = "Orlando", StateId = 10 });
            await _context.SaveChangesAsync();

            var result = await _service.ComboAsync(10);

            Assert.True(result.WasSuccess);
            Assert.Equal(2, result.Result!.Count());
        }

        [Fact]
        public async Task ComboAsync_ShouldReturnEmptyList_WhenNoCitiesFound()
        {
            var result = await _service.ComboAsync(999);

            Assert.True(result.WasSuccess);
            Assert.Empty(result.Result!);
        }

        // -------------------------------------------------------------
        // GET ASYNC (ID)
        // -------------------------------------------------------------
        [Fact]
        public async Task GetAsync_Id_ShouldReturnInvalidId_WhenIdIsZero()
        {
            _localizerMock.Setup(x => x["Generic_InvalidId"])
                .Returns(new LocalizedString("Generic_InvalidId", "Invalid Id"));

            var result = await _service.GetAsync(0);

            Assert.False(result.WasSuccess);
            Assert.Equal("Invalid Id", result.Message);
        }

        [Fact]
        public async Task GetAsync_Id_ShouldReturnNotFound_WhenCityDoesNotExist()
        {
            _localizerMock.Setup(x => x["Generic_IdNotFound"])
                .Returns(new LocalizedString("Generic_IdNotFound", "Not Found"));

            var result = await _service.GetAsync(999);

            Assert.False(result.WasSuccess);
            Assert.Equal("Not Found", result.Message);
        }

        [Fact]
        public async Task GetAsync_Id_ShouldReturnCity_WhenExists()
        {
            var city = new City { CityId = 5, Name = "Tampa", StateId = 1 };
            _context.Cities.Add(city);
            await _context.SaveChangesAsync();

            var result = await _service.GetAsync(5);

            Assert.True(result.WasSuccess);
            Assert.Equal("Tampa", result.Result!.Name);
        }

        // -------------------------------------------------------------
        // ADD ASYNC
        // -------------------------------------------------------------
        [Fact]
        public async Task AddAsync_ShouldReturnInvalidModel_WhenModelIsNull()
        {
            _localizerMock.Setup(x => x["Generic_InvalidModel"])
                .Returns(new LocalizedString("Generic_InvalidModel", "Invalid Model"));

            var result = await _service.AddAsync(null);

            Assert.False(result.WasSuccess);
            Assert.Equal("Invalid Model", result.Message);
        }

        [Fact]
        public async Task AddAsync_ShouldAddCity_WhenModelIsValid()
        {
            var city = new City { Name = "Dallas", StateId = 20 };

            _transactionManagerMock.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _transactionManagerMock.Setup(x => x.SaveChangesAsync()).Returns(async () => await _context.SaveChangesAsync());
            _transactionManagerMock.Setup(x => x.CommitTransactionAsync()).Returns(Task.CompletedTask);

            var result = await _service.AddAsync(city);

            Assert.True(result.WasSuccess);
            Assert.Equal("Dallas", result.Result!.Name);
            Assert.Equal(1, _context.Cities.Count());
        }

        // -------------------------------------------------------------
        // UPDATE ASYNC
        // -------------------------------------------------------------
        [Fact]
        public async Task UpdateAsync_ShouldReturnInvalidId_WhenModelIsNull()
        {
            _localizerMock.Setup(x => x["Generic_InvalidId"])
                .Returns(new LocalizedString("Generic_InvalidId", "Invalid Id"));

            var result = await _service.UpdateAsync(null);

            Assert.False(result.WasSuccess);
            Assert.Equal("Invalid Id", result.Message);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateCity_WhenValid()
        {
            var city = new City { CityId = 1, Name = "OldName", StateId = 1 };
            _context.Cities.Add(city);
            await _context.SaveChangesAsync();

            city.Name = "NewName";

            _transactionManagerMock.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _transactionManagerMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
            _transactionManagerMock.Setup(x => x.CommitTransactionAsync()).Returns(Task.CompletedTask);

            var result = await _service.UpdateAsync(city);

            Assert.True(result.WasSuccess);
            Assert.Equal("NewName", result.Result!.Name);
        }

        // -------------------------------------------------------------
        // DELETE ASYNC
        // -------------------------------------------------------------
        [Fact]
        public async Task DeleteAsync_ShouldReturnInvalidId_WhenIdIsZero()
        {
            _localizerMock.Setup(x => x["Generic_InvalidId"])
                .Returns(new LocalizedString("Generic_InvalidId", "Invalid Id"));

            var result = await _service.DeleteAsync(0);

            Assert.False(result.WasSuccess);
            Assert.Equal("Invalid Id", result.Message);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnNotFound_WhenCityDoesNotExist()
        {
            _localizerMock.Setup(x => x["Generic_IdNotFound"])
                .Returns(new LocalizedString("Generic_IdNotFound", "Not Found"));

            var result = await _service.DeleteAsync(999);

            Assert.False(result.WasSuccess);
            Assert.Equal("Not Found", result.Message);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteCity_WhenExists()
        {
            var city = new City { CityId = 10, Name = "Phoenix", StateId = 1 };
            _context.Cities.Add(city);
            await _context.SaveChangesAsync();

            _transactionManagerMock.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _transactionManagerMock.Setup(x => x.SaveChangesAsync()).Returns(async () => await _context.SaveChangesAsync());
            _transactionManagerMock.Setup(x => x.CommitTransactionAsync()).Returns(Task.CompletedTask);

            var result = await _service.DeleteAsync(10);

            Assert.True(result.WasSuccess);
            Assert.True(result.Result);
            Assert.Empty(_context.Cities);
        }

    }
}
