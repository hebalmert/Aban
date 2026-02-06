using Aban.AppInfra;
using Aban.AppInfra.EmailHelper;
using Aban.AppInfra.ErrorHandling;
using Aban.AppInfra.FileHelper;
using Aban.AppInfra.Mappings;
using Aban.AppInfra.Transactions;
using Aban.AppInfra.UserHelper;
using Aban.Domain.AbanResponse;
using Aban.Domain.Entities;
using Aban.Domain.Enum;
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
    public class ManagerServiceTests
    {
        private readonly DataContext _context;
        private readonly HttpErrorHandler _httpErrorHandler;

        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<ITransactionManager> _transactionManagerMock;
        private readonly Mock<IFileStorage> _fileStorageMock;
        private readonly Mock<IUserHelper> _userHelperMock;
        private readonly Mock<IEmailHelper> _emailHelperMock;
        private readonly Mock<IStringLocalizer> _localizerMock;
        private readonly Mock<IMapperService> _mapperServiceMock;
        private readonly Mock<IMemoryCache> _memoryCacheMock;
        private readonly Mock<ILogger<HttpErrorHandler>> _loggerMock;

        private readonly ManagerService _service;

        private readonly ImgSetting _imgSetting = new ImgSetting
        {
            ImgManager = "managers",
            ImgCorporation = "corp",
            ImgUsuario = "users",
            ImgProduct = "products",
            LogoSoftware = "logo",
            ImgNoImage = "noimg",
            ImgNoPicture = "nopicture",
            ImgBaseUrl = "baseurl"
        };

        public ManagerServiceTests()
        {
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new DataContext(options);

            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _transactionManagerMock = new Mock<ITransactionManager>();
            _fileStorageMock = new Mock<IFileStorage>();
            _userHelperMock = new Mock<IUserHelper>();
            _emailHelperMock = new Mock<IEmailHelper>();
            _localizerMock = new Mock<IStringLocalizer>();
            _mapperServiceMock = new Mock<IMapperService>();
            _memoryCacheMock = new Mock<IMemoryCache>();
            _loggerMock = new Mock<ILogger<HttpErrorHandler>>();

            _httpErrorHandler = new HttpErrorHandler(_localizerMock.Object, _loggerMock.Object);

            _httpContextAccessorMock.Setup(x => x.HttpContext)
                .Returns(new DefaultHttpContext());

            _transactionManagerMock.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _transactionManagerMock.Setup(x => x.CommitTransactionAsync()).Returns(Task.CompletedTask);
            _transactionManagerMock.Setup(x => x.RollbackTransactionAsync()).Returns(Task.CompletedTask);
            _transactionManagerMock.Setup(x => x.SaveChangesAsync())
                .Returns(async () => await _context.SaveChangesAsync());

            SetupLocalizerDefaults();

            _service = new ManagerService(
                _context,
                _httpContextAccessorMock.Object,
                _transactionManagerMock.Object,
                _memoryCacheMock.Object,
                _fileStorageMock.Object,
                _userHelperMock.Object,
                _emailHelperMock.Object,
                Options.Create(_imgSetting),
                _httpErrorHandler,
                _localizerMock.Object,
                _mapperServiceMock.Object
            );
        }

        private void SetupLocalizerDefaults()
        {
            _localizerMock.Setup(x => x["Generic_UserNameAlreadyUsed"])
                .Returns(new LocalizedString("Generic_UserNameAlreadyUsed", "Username already used"));

            _localizerMock.Setup(x => x["Generic_EmailAlreadyUsed"])
                .Returns(new LocalizedString("Generic_EmailAlreadyUsed", "Email already used"));

            _localizerMock.Setup(x => x["Generic_UserCreationFail"])
                .Returns(new LocalizedString("Generic_UserCreationFail", "User creation failed"));

            _localizerMock.Setup(x => x["Generic_IdNotFound"])
                .Returns(new LocalizedString("Generic_IdNotFound", "Id not found"));

            _localizerMock.Setup(x => x["Generic_RecordDeletedNoImage"])
                .Returns(new LocalizedString("Generic_RecordDeletedNoImage", "Record deleted but image missing"));
        }

        private Manager CreateManager(int id = 1)
        {
            return new Manager
            {
                ManagerId = id,
                FirstName = "John",
                LastName = "Doe",
                NroDocument = "123",
                PhoneNumber = "555",
                Address = "Street",
                Email = "john@doe.com",
                UserName = "johndoe",
                CorporationId = 1,
                Job = "Manager",
                UserType = UserType.Administrator,
                Active = true
            };
        }

        // -------------------------------------------------------------
        // ADD
        // -------------------------------------------------------------
        [Fact]
        public async Task AddAsync_ShouldReturnUserNameAlreadyUsed()
        {
            var model = CreateManager();

            _userHelperMock.Setup(x => x.GetUserByUserNameAsync(model.UserName))
                .ReturnsAsync(new User());

            var result = await _service.AddAsync(model, "http://front");

            Assert.True(result.WasSuccess);
            Assert.Equal("Username already used", result.Message);
        }

        [Fact]
        public async Task AddAsync_ShouldReturnEmailAlreadyUsed()
        {
            var model = CreateManager();

            _userHelperMock.Setup(x => x.GetUserByEmailAsync(model.Email))
                .ReturnsAsync(new User());

            var result = await _service.AddAsync(model, "http://front");

            Assert.True(result.WasSuccess);
            Assert.Equal("Email already used", result.Message);
        }

        [Fact]
        public async Task AddAsync_ShouldAddManager_WhenValid()
        {
            var model = CreateManager();

            _userHelperMock.Setup(x => x.GetUserByUserNameAsync(model.UserName))
                .ReturnsAsync((User)null);

            _userHelperMock.Setup(x => x.GetUserByEmailAsync(model.Email))
                .ReturnsAsync((User)null);

            _emailHelperMock.Setup(x => x.ConfirmarCuenta(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new Response { IsSuccess = true });

            _userHelperMock.Setup(x => x.AddUserUsuarioAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<UserType>()))
                .ReturnsAsync(new User { Id = "1", Email = model.Email, Pass = "123" });

            var result = await _service.AddAsync(model, "http://front");

            Assert.True(result.WasSuccess);
            Assert.Equal(1, _context.Managers.Count());
        }

        // -------------------------------------------------------------
        // UPDATE
        // -------------------------------------------------------------
        [Fact]
        public async Task UpdateAsync_ShouldReturnUserNameAlreadyUsed()
        {
            var model = CreateManager();

            _userHelperMock.Setup(x => x.GetUserByUserNameAsync(model.UserName))
                .ReturnsAsync(new User());

            var result = await _service.UpdateAsync(model, "http://front");

            Assert.True(result.WasSuccess);
            Assert.Equal("Username already used", result.Message);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnEmailAlreadyUsed()
        {
            var model = CreateManager();

            _userHelperMock.Setup(x => x.GetUserByEmailAsync(model.Email))
                .ReturnsAsync(new User());

            var result = await _service.UpdateAsync(model, "http://front");

            Assert.True(result.WasSuccess);
            Assert.Equal("Email already used", result.Message);
        }

        // -------------------------------------------------------------
        // DELETE
        // -------------------------------------------------------------
        [Fact]
        public async Task DeleteAsync_ShouldReturnNotFound_WhenMissing()
        {
            var result = await _service.DeleteAsync(999);

            Assert.False(result.WasSuccess);
            Assert.Equal("Id not found", result.Message);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteManager_WhenExists()
        {
            var model = CreateManager();
            _context.Managers.Add(model);
            await _context.SaveChangesAsync();

            _userHelperMock.Setup(x => x.GetUserByUserNameAsync(model.UserName))
                .ReturnsAsync(new User { Id = "1" });

            _userHelperMock
                .Setup(x => x.DeleteUser(It.IsAny<string>()))
                .Verifiable();

            _fileStorageMock.Setup(x => x.RemoveFileAsync(_imgSetting.ImgManager, It.IsAny<string>()))
                .ReturnsAsync(true);

            var result = await _service.DeleteAsync(model.ManagerId);

            Assert.True(result.WasSuccess);
            Assert.True(result.Result);
            Assert.Empty(_context.Managers);
        }
    }
}