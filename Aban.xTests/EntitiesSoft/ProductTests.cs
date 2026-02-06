using Aban.AppInfra;
using Aban.AppInfra.ErrorHandling;
using Aban.AppInfra.Mappings;
using Aban.AppInfra.Transactions;
using Aban.AppInfra.UserHelper;
using Aban.Domain.DTOs;
using Aban.Domain.Entities;
using Aban.Domain.EntitiesSoft;
using Aban.Domain.Pagination;
using Aban.Services.ImplementEntitiesSoft;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;

namespace Aban.xTests.Entities
{
    public class ProductServiceTests
    {
        private readonly DataContext _context;
        private readonly HttpErrorHandler _httpErrorHandler;   // REAL
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<ITransactionManager> _transactionManagerMock;
        private readonly Mock<IUserHelper> _userHelperMock;
        private readonly Mock<IMapperService> _mapperMock;
        private readonly Mock<IStringLocalizer> _localizerMock;
        private readonly Mock<ILogger<HttpErrorHandler>> _loggerMock;

        private readonly ProductService _service;

        public ProductServiceTests()
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
            _userHelperMock = new Mock<IUserHelper>();
            _mapperMock = new Mock<IMapperService>();

            // HttpErrorHandler REAL
            _httpErrorHandler = new HttpErrorHandler(_localizerMock.Object, _loggerMock.Object);

            // HttpContext requerido por paginación
            _httpContextAccessorMock.Setup(x => x.HttpContext)
                .Returns(new DefaultHttpContext());

            // Servicio bajo prueba
            _service = new ProductService(
                _context,
                _httpContextAccessorMock.Object,
                _transactionManagerMock.Object,
                _userHelperMock.Object,
                _mapperMock.Object,
                _httpErrorHandler,
                _localizerMock.Object
            );
        }

        // -------------------------------------------------------------
        // GET ASYNC (ID)
        // -------------------------------------------------------------
        [Fact]
        public async Task GetAsync_Id_ShouldReturnInvalidId_WhenIdIsEmpty()
        {
            _localizerMock.Setup(x => x["Generic_InvalidId"])
                .Returns(new LocalizedString("Generic_InvalidId", "Invalid Id"));

            var result = await _service.GetAsync(Guid.Empty);

            Assert.False(result.WasSuccess);
            Assert.Equal("Invalid Id", result.Message);
        }

        [Fact]
        public async Task GetAsync_Id_ShouldReturnNotFound_WhenProductDoesNotExist()
        {
            _localizerMock.Setup(x => x["Generic_IdNotFound"])
                .Returns(new LocalizedString("Generic_IdNotFound", "Not Found"));

            var result = await _service.GetAsync(Guid.NewGuid());

            Assert.False(result.WasSuccess);
            Assert.Equal("Not Found", result.Message);
        }

        [Fact]
        public async Task GetAsync_Id_ShouldReturnProduct_WhenExists()
        {
            var product = new Product
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Laptop",
                Price = 100,
                Quantity = 5,
                Active = true,
                CorporationId = 1
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            var result = await _service.GetAsync(product.ProductId);

            Assert.True(result.WasSuccess);
            Assert.Equal("Laptop", result.Result!.ProductName);
        }

        // -------------------------------------------------------------
        // ADD ASYNC
        // -------------------------------------------------------------
        [Fact]
        public async Task AddAsync_ShouldReturnInvalidModel_WhenModelIsNull()
        {
            _localizerMock.Setup(x => x["Generic_InvalidModel"])
                .Returns(new LocalizedString("Generic_InvalidModel", "Invalid Model"));

            var result = await _service.AddAsync(null, "admin");

            Assert.False(result.WasSuccess);
            Assert.Equal("Invalid Model", result.Message);
        }

        [Fact]
        public async Task AddAsync_ShouldReturnAuthError_WhenUserNotFound()
        {
            _localizerMock.Setup(x => x["Generic_AuthIdFail"])
                .Returns(new LocalizedString("Generic_AuthIdFail", "Auth Error"));

            _userHelperMock.Setup(x => x.GetUserByUserNameAsync("ghost"))
                .ReturnsAsync((User)null);

            var dto = new ProductDTO
            {
                ProductName = "Mouse",
                Price = 10,
                Quantity = 1,
                Active = true
            };

            var result = await _service.AddAsync(dto, "ghost");

            Assert.False(result.WasSuccess);
            Assert.Equal("Auth Error", result.Message);
        }

        [Fact]
        public async Task AddAsync_ShouldAddProduct_WhenValid()
        {
            var dto = new ProductDTO
            {
                ProductName = "Keyboard",
                Price = 20,
                Quantity = 2,
                Active = true
            };

            _userHelperMock.Setup(x => x.GetUserByUserNameAsync("admin"))
                .ReturnsAsync(new User { CorporationId = 1 });

            _mapperMock.Setup(x => x.Map<ProductDTO, Product>(dto))
                .Returns(new Product
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Keyboard",
                    Price = 20,
                    Quantity = 2,
                    Active = true,
                    CorporationId = 1
                });

            _transactionManagerMock.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _transactionManagerMock.Setup(x => x.SaveChangesAsync()).Returns(async () => await _context.SaveChangesAsync());
            _transactionManagerMock.Setup(x => x.CommitTransactionAsync()).Returns(Task.CompletedTask);

            var result = await _service.AddAsync(dto, "admin");

            Assert.True(result.WasSuccess);
            Assert.Equal("Keyboard", result.Result!.ProductName);
            Assert.Equal(1, _context.Products.Count());
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
        public async Task UpdateAsync_ShouldUpdateProduct_WhenValid()
        {
            var product = new Product
            {
                ProductId = Guid.NewGuid(),
                ProductName = "OldName",
                Price = 10,
                Quantity = 1,
                Active = true,
                CorporationId = 1
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            product.ProductName = "NewName";

            _transactionManagerMock.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _transactionManagerMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
            _transactionManagerMock.Setup(x => x.CommitTransactionAsync()).Returns(Task.CompletedTask);

            var result = await _service.UpdateAsync(product);

            Assert.True(result.WasSuccess);
            Assert.Equal("NewName", result.Result!.ProductName);
        }

        // -------------------------------------------------------------
        // DELETE ASYNC
        // -------------------------------------------------------------
        [Fact]
        public async Task DeleteAsync_ShouldReturnInvalidId_WhenIdIsEmpty()
        {
            _localizerMock.Setup(x => x["Generic_InvalidId"])
                .Returns(new LocalizedString("Generic_InvalidId", "Invalid Id"));

            var result = await _service.DeleteAsync(Guid.Empty);

            Assert.False(result.WasSuccess);
            Assert.Equal("Invalid Id", result.Message);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnNotFound_WhenProductDoesNotExist()
        {
            _localizerMock.Setup(x => x["Generic_IdNotFound"])
                .Returns(new LocalizedString("Generic_IdNotFound", "Not Found"));

            var result = await _service.DeleteAsync(Guid.NewGuid());

            Assert.False(result.WasSuccess);
            Assert.Equal("Not Found", result.Message);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteProduct_WhenExists()
        {
            var product = new Product
            {
                ProductId = Guid.NewGuid(),
                ProductName = "ToDelete",
                Price = 10,
                Quantity = 1,
                Active = true,
                CorporationId = 1
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            _transactionManagerMock.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _transactionManagerMock.Setup(x => x.SaveChangesAsync()).Returns(async () => await _context.SaveChangesAsync());
            _transactionManagerMock.Setup(x => x.CommitTransactionAsync()).Returns(Task.CompletedTask);

            var result = await _service.DeleteAsync(product.ProductId);

            Assert.True(result.WasSuccess);
            Assert.True(result.Result);
            Assert.Empty(_context.Products);
        }
    }
}