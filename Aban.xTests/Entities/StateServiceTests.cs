using Aban.AppInfra;
using Aban.AppInfra.ErrorHandling;
using Aban.AppInfra.Transactions;
using Aban.Domain.Entities;
using Aban.Domain.Pagination;
using Aban.Domain.ResponcesSec;
using Aban.Services.ImplementEntties;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;

namespace Aban.xTests.Entities
{
    public class StateServiceTests
    {
        private readonly DataContext _context;
        private readonly HttpErrorHandler _httpErrorHandler;   // REAL
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<ITransactionManager> _transactionManagerMock;
        private readonly Mock<IStringLocalizer> _localizerMock;
        private readonly Mock<ILogger<HttpErrorHandler>> _loggerMock;

        private readonly StateService _service;

        public StateServiceTests()
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

            _service = new StateService(
                _context,
                _httpErrorHandler,
                _httpContextAccessorMock.Object,
                _transactionManagerMock.Object,
                _localizerMock.Object
            );
        }

        private void SetupLocalizerDefaults()
        {
            _localizerMock.Setup(x => x["Generic_InvalidId"])
                .Returns(new LocalizedString("Generic_InvalidId", "Invalid Id"));

            _localizerMock.Setup(x => x["Generic_IdNotFound"])
                .Returns(new LocalizedString("Generic_IdNotFound", "Id not found"));

            _localizerMock.Setup(x => x["Generic_InvalidModel"])
                .Returns(new LocalizedString("Generic_InvalidModel", "Invalid model"));

            _localizerMock.Setup(x => x["Generic_Success"])
                .Returns(new LocalizedString("Generic_Success", "Success"));
        }

        private State CreateState(int id, int countryId, string name)
        {
            return new State
            {
                StateId = id,
                CountryId = countryId,
                Name = name
            };
        }

        private Corporation CreateCorporation(int id, int countryId)
        {
            return new Corporation
            {
                CorporationId = id,
                Name = "Corp X",
                NroDocument = "123",
                Phone = "555",
                Address = "Street",
                CountryId = countryId,
                DateStart = DateTime.UtcNow.AddDays(-10),
                DateEnd = DateTime.UtcNow.AddDays(10),
                Active = true
            };
        }

        // -------------------------------------------------------------
        // COMBO
        // -------------------------------------------------------------
        [Fact]
        public async Task ComboAsync_ShouldReturnStates_WhenCorporationCountryMatches()
        {
            _context.Corporations.Add(CreateCorporation(1, 100));
            _context.States.Add(CreateState(1, 100, "Florida"));
            _context.States.Add(CreateState(2, 100, "Georgia"));
            await _context.SaveChangesAsync();

            var claims = new ClaimsDTOs { CorporationId = 1 };

            var result = await _service.ComboAsync(claims);

            Assert.True(result.WasSuccess);
            Assert.Equal(2, result.Result!.Count());
        }

        [Fact]
        public async Task ComboAsync_ShouldReturnEmpty_WhenNoStatesFound()
        {
            _context.Corporations.Add(CreateCorporation(1, 999));
            await _context.SaveChangesAsync();

            var claims = new ClaimsDTOs { CorporationId = 1 };

            var result = await _service.ComboAsync(claims);

            Assert.True(result.WasSuccess);
            Assert.Empty(result.Result!);
        }

        // -------------------------------------------------------------
        // GET PAGINATED
        // -------------------------------------------------------------
        [Fact]
        public async Task GetAsync_Pagination_ShouldReturnStatesForCountry()
        {
            _context.States.Add(CreateState(1, 50, "Texas"));
            _context.States.Add(CreateState(2, 50, "Tennessee"));
            _context.States.Add(CreateState(3, 99, "Nevada"));
            await _context.SaveChangesAsync();

            var pagination = new PaginationDTO
            {
                Id = 50,
                Page = 1,
                RecordsNumber = 10
            };

            var result = await _service.GetAsync(pagination);

            Assert.True(result.WasSuccess);
            Assert.Equal(2, result.Result!.Count());
        }

        // -------------------------------------------------------------
        // GET BY ID
        // -------------------------------------------------------------
        [Fact]
        public async Task GetAsync_Id_ShouldReturnInvalidId_WhenZero()
        {
            var result = await _service.GetAsync(0);

            Assert.False(result.WasSuccess);
            Assert.Equal("Invalid Id", result.Message);
        }

        [Fact]
        public async Task GetAsync_Id_ShouldReturnNotFound_WhenMissing()
        {
            var result = await _service.GetAsync(999);

            Assert.False(result.WasSuccess);
            Assert.Equal("Id not found", result.Message);
        }

        [Fact]
        public async Task GetAsync_Id_ShouldReturnState_WhenExists()
        {
            _context.States.Add(CreateState(1, 10, "Ohio"));
            await _context.SaveChangesAsync();

            var result = await _service.GetAsync(1);

            Assert.True(result.WasSuccess);
            Assert.Equal("Ohio", result.Result!.Name);
        }

        // -------------------------------------------------------------
        // ADD
        // -------------------------------------------------------------
        [Fact]
        public async Task AddAsync_ShouldReturnInvalidModel_WhenModelInvalid()
        {
            var invalidState = new State { Name = "", CountryId = 0 };

            var result = await _service.AddAsync(invalidState);

            Assert.False(result.WasSuccess);
            Assert.Equal("Invalid model", result.Message);
        }

        [Fact]
        public async Task AddAsync_ShouldAddState_WhenValid()
        {
            var state = CreateState(0, 10, "Colorado");

            var result = await _service.AddAsync(state);

            Assert.True(result.WasSuccess);
            Assert.Equal("Success", result.Message);
            Assert.Equal(1, _context.States.Count());
        }

        // -------------------------------------------------------------
        // UPDATE
        // -------------------------------------------------------------
        [Fact]
        public async Task UpdateAsync_ShouldReturnInvalidId_WhenZero()
        {
            var result = await _service.UpdateAsync(new State { StateId = 0 });

            Assert.False(result.WasSuccess);
            Assert.Equal("Invalid Id", result.Message);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateState_WhenValid()
        {
            var state = CreateState(1, 10, "OldName");
            _context.States.Add(state);
            await _context.SaveChangesAsync();

            state.Name = "NewName";

            var result = await _service.UpdateAsync(state);

            Assert.True(result.WasSuccess);
            Assert.Equal("NewName", result.Result!.Name);
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
        public async Task DeleteAsync_ShouldDeleteState_WhenExists()
        {
            _context.States.Add(CreateState(1, 10, "Arizona"));
            await _context.SaveChangesAsync();

            var result = await _service.DeleteAsync(1);

            Assert.True(result.WasSuccess);
            Assert.True(result.Result);
            Assert.Empty(_context.States);
        }
    }
}