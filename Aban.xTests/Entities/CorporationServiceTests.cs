using Aban.AppInfra;
using Aban.AppInfra.ErrorHandling;
using Aban.AppInfra.FileHelper;
using Aban.AppInfra.Transactions;
using Aban.Domain.Entities;
using Aban.Domain.Pagination;
using Aban.Domain.ResponcesSec;
using Aban.Services.ImplementEntties;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Aban.xTests.Entities
{
    public class CorporationServiceTests
    {
        private readonly DataContext _context;
        private readonly CorporationService _service;

        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<ITransactionManager> _transactionManagerMock;
        private readonly Mock<IFileStorage> _fileStorageMock;
        private readonly Mock<IStringLocalizer> _localizerMock;
        private readonly Mock<ILogger<HttpErrorHandler>> _loggerMock;
        private readonly Mock<IMemoryCache> _cacheMock;

        private readonly HttpErrorHandler _httpErrorHandler;
        private readonly ImgSetting _imgSettings;

        public CorporationServiceTests()
        {
            // InMemory DB
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new DataContext(options);

            // Mocks
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _transactionManagerMock = new Mock<ITransactionManager>();
            _fileStorageMock = new Mock<IFileStorage>();
            _localizerMock = new Mock<IStringLocalizer>();
            _loggerMock = new Mock<ILogger<HttpErrorHandler>>();
            _cacheMock = new Mock<IMemoryCache>();

            // HttpErrorHandler REAL
            _httpErrorHandler = new HttpErrorHandler(_localizerMock.Object, _loggerMock.Object);

            // ImgSetting real
            _imgSettings = new ImgSetting
            {
                ImgNoImage = "no-image.png",
                ImgCorporation = "corporations"
            };

            var imgOptions = Options.Create(_imgSettings);

            // HttpContext requerido por paginación
            _httpContextAccessorMock.Setup(x => x.HttpContext)
                .Returns(new DefaultHttpContext());

            // Servicio bajo prueba
            _service = new CorporationService(
                _context,
                _httpContextAccessorMock.Object,
                _transactionManagerMock.Object,
                _cacheMock.Object,
                _fileStorageMock.Object,
                imgOptions,
                _httpErrorHandler,
                _localizerMock.Object
            );
        }

        // -------------------------------------------------------------
        // COMBOASYNC
        // -------------------------------------------------------------
        [Fact]
        public async Task ComboAsync_ShouldReturnCorporationsWithDefaultItem()
        {
            _context.Corporations.Add(new Corporation
            {
                CorporationId = 1,
                Name = "Corp A",
                NroDocument = "123456789",
                Phone = "555-1111",
                Address = "Street 1",
                CountryId = 1,
                DateStart = DateTime.UtcNow,
                DateEnd = DateTime.UtcNow.AddYears(1),
                Active = true
            });

            _context.Corporations.Add(new Corporation
            {
                CorporationId = 2,
                Name = "Corp B",
                NroDocument = "987654321",
                Phone = "555-2222",
                Address = "Street 2",
                CountryId = 1,
                DateStart = DateTime.UtcNow,
                DateEnd = DateTime.UtcNow.AddYears(1),
                Active = true
            });

            await _context.SaveChangesAsync();

            var result = await _service.ComboAsync();

            Assert.True(result.WasSuccess);
            Assert.Equal(3, result.Result!.Count());
            Assert.Equal("[Select Corporation]", result.Result!.First().Name);

        }

        [Fact]
        public async Task ComboAsync_ShouldReturnOnlyDefaultItem_WhenNoCorporations()
        {
            var result = await _service.ComboAsync();

            Assert.True(result.WasSuccess);
            Assert.Single(result.Result!);
            Assert.Equal("[Select Corporation]", result.Result!.First().Name);
        }

        // -------------------------------------------------------------
        // GET ASYNC (PAGINATION)
        // -------------------------------------------------------------
        [Fact]
        public async Task GetAsync_Pagination_ShouldReturnPagedCorporations()
        {
            _context.Corporations.Add(new Corporation
            {
                CorporationId = 1,
                Name = "Alpha",
                NroDocument = "123456789",
                Phone = "555-1234",
                Address = "123 Main St",
                CountryId = 1,
                DateStart = DateTime.UtcNow,
                DateEnd = DateTime.UtcNow.AddYears(1),
                Active = true
            });

            _context.Corporations.Add(new Corporation
            {
                CorporationId = 2,
                Name = "Beta",
                NroDocument = "987654321",
                Phone = "555-5678",
                Address = "456 Market St",
                CountryId = 1,
                DateStart = DateTime.UtcNow,
                DateEnd = DateTime.UtcNow.AddYears(1),
                Active = true
            });

            await _context.SaveChangesAsync();

            var pagination = new PaginationDTO { Page = 1, RecordsNumber = 10 };

            _fileStorageMock.Setup(x => x.GetBlobSasUrlAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>()))
                .ReturnsAsync("https://fake-url.com/image.png");

            var result = await _service.GetAsync(pagination);

            Assert.True(result.WasSuccess);
            Assert.NotEmpty(result.Result!);

        }

        [Fact]
        public async Task GetAsync_Pagination_ShouldAssignDefaultImage_WhenImagenIsNull()
        {
            var corp = new Corporation
            {
                CorporationId = 1,
                Name = "Alpha",
                NroDocument = "123456789",
                Phone = "555-1234",
                Address = "123 Main St",
                CountryId = 1,
                DateStart = DateTime.UtcNow,
                DateEnd = DateTime.UtcNow.AddYears(1),
                Imagen = null,
                Active = true
            };

            _context.Corporations.Add(corp);   // ← ESTO FALTABA
            await _context.SaveChangesAsync();

            var pagination = new PaginationDTO { Page = 1, RecordsNumber = 10 };

            var result = await _service.GetAsync(pagination);

            Assert.True(result.WasSuccess);
            Assert.Equal("no-image.png", result.Result!.First().ImageFullPath);

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
        public async Task GetAsync_Id_ShouldReturnNotFound_WhenNotExists()
        {
            _localizerMock.Setup(x => x["Generic_IdNotFound"])
                .Returns(new LocalizedString("Generic_IdNotFound", "Not Found"));

            var result = await _service.GetAsync(999);

            Assert.False(result.WasSuccess);
            Assert.Equal("Not Found", result.Message);
        }

        [Fact]
        public async Task GetAsync_Id_ShouldReturnCorporationWithDefaultImage_WhenImagenIsNull()
        {
            var corp = new Corporation
            {
                CorporationId = 10,
                Name = "Corp X",
                NroDocument = "123456789",
                Phone = "555-1234",
                Address = "123 Main St",
                CountryId = 1,
                DateStart = DateTime.UtcNow,
                DateEnd = DateTime.UtcNow.AddYears(1),
                Imagen = null,
                Active = true
            };

            _context.Corporations.Add(corp);
            await _context.SaveChangesAsync();

            var result = await _service.GetAsync(10);

            Assert.True(result.WasSuccess);
            Assert.Equal("no-image.png", result.Result!.ImageFullPath);
        }

        [Fact]
        public async Task GetAsync_Id_ShouldReturnCorporationWithSasUrl_WhenImagenExists()
        {
            var corp = new Corporation
            {
                CorporationId = 10,
                Name = "Corp X",
                NroDocument = "123456789",
                Phone = "555-1234",
                Address = "123 Main St",
                CountryId = 1,
                DateStart = DateTime.UtcNow,
                DateEnd = DateTime.UtcNow.AddYears(1),
                Imagen = "img123.jpg",
                Active = true
            };

            _context.Corporations.Add(corp);
            await _context.SaveChangesAsync();

            _fileStorageMock.Setup(x => x.GetBlobSasUrlAsync("img123.jpg", "corporations", It.IsAny<TimeSpan>()))
                .ReturnsAsync("https://sas-url.com/img123.jpg");

            var result = await _service.GetAsync(10);

            Assert.True(result.WasSuccess);
            Assert.Equal("https://sas-url.com/img123.jpg", result.Result!.ImageFullPath);
        }

        // -------------------------------------------------------------
        // ADD ASYNC
        // -------------------------------------------------------------
        [Fact]
        public async Task AddAsync_ShouldReturnInvalidModel_WhenModelIsInvalid()
        {
            _localizerMock.Setup(x => x["Generic_InvalidModel"])
                .Returns(new LocalizedString("Generic_InvalidModel", "Invalid Model"));

            var result = await _service.AddAsync(null);

            Assert.False(result.WasSuccess);
            Assert.Equal("Invalid Model", result.Message);
        }

        [Fact]
        public async Task AddAsync_ShouldAddCorporation_WhenValid()
        {
            var corp = new Corporation
            {
                Name = "New Corp",
                NroDocument = "123456789",
                Phone = "555-1234",
                Address = "123 Main St",
                CountryId = 1,
                DateStart = DateTime.UtcNow,
                DateEnd = DateTime.UtcNow.AddYears(1),
                Active = true
            };

            _transactionManagerMock.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _transactionManagerMock.Setup(x => x.SaveChangesAsync())
                .Returns(async () => await _context.SaveChangesAsync());
            _transactionManagerMock.Setup(x => x.CommitTransactionAsync()).Returns(Task.CompletedTask);

            var result = await _service.AddAsync(corp);

            Assert.True(result.WasSuccess);
            Assert.Equal(1, _context.Corporations.Count());
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
        public async Task UpdateAsync_ShouldUpdateCorporation_WhenValid()
        {
            var corp = new Corporation
            {
                CorporationId = 1,
                Name = "Old",
                NroDocument = "123456789",
                Phone = "555-1234",
                Address = "123 Main St",
                CountryId = 1,
                DateStart = DateTime.UtcNow,
                DateEnd = DateTime.UtcNow.AddYears(1),
                Active = true
            };

            _context.Corporations.Add(corp);
            await _context.SaveChangesAsync();

            // Update
            corp.Name = "Updated";

            _transactionManagerMock.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _transactionManagerMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
            _transactionManagerMock.Setup(x => x.CommitTransactionAsync()).Returns(Task.CompletedTask);

            var result = await _service.UpdateAsync(corp);

            Assert.True(result.WasSuccess);
            Assert.Equal("Updated", result.Result!.Name);

        }

        // -------------------------------------------------------------
        // DELETE ASYNC
        // -------------------------------------------------------------
        [Fact]
        public async Task DeleteAsync_ShouldReturnNotFound_WhenNotExists()
        {
            _localizerMock.Setup(x => x["Generic_IdNotFound"])
                .Returns(new LocalizedString("Generic_IdNotFound", "Not Found"));

            var result = await _service.DeleteAsync(999);

            Assert.False(result.WasSuccess);
            Assert.Equal("Not Found", result.Message);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteCorporation_WhenExists()
        {
            var corp = new Corporation
            {
                CorporationId = 10,
                Name = "Corp X",
                NroDocument = "123456789",
                Phone = "555-1234",
                Address = "123 Main St",
                CountryId = 1,
                DateStart = DateTime.UtcNow,
                DateEnd = DateTime.UtcNow.AddYears(1),
                Active = true
            };

            _context.Corporations.Add(corp);
            await _context.SaveChangesAsync();

            _transactionManagerMock.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _transactionManagerMock.Setup(x => x.SaveChangesAsync())
                .Returns(async () => await _context.SaveChangesAsync());
            _transactionManagerMock.Setup(x => x.CommitTransactionAsync()).Returns(Task.CompletedTask);

            var result = await _service.DeleteAsync(10);

            Assert.True(result.WasSuccess);
            Assert.True(result.Result);
            Assert.Empty(_context.Corporations);

        }
    }
}
