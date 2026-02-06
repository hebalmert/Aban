using Aban.AppInfra;
using Aban.AppInfra.EmailHelper;
using Aban.AppInfra.FileHelper;
using Aban.AppInfra.Transactions;
using Aban.AppInfra.UserHelper;
using Aban.Domain.AbanResponse;
using Aban.Domain.Entities;
using Aban.Domain.Enum;
using Aban.Domain.Resources;
using Aban.Domain.ResponcesSec;
using Aban.Services.ImplementSecure;
using Aban.Services.InterfacesSecure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Aban.xTests.Secure
{
    public class AccountServiceTests
    {
        private readonly DataContext _context;
        private readonly AccountService _service;

        private readonly Mock<IUserHelper> _userHelperMock;
        private readonly Mock<IEmailHelper> _emailHelperMock;
        private readonly Mock<ITransactionManager> _transactionManagerMock;
        private readonly Mock<IStringLocalizer> _localizerMock;
        private readonly Mock<IFileStorage> _fileStorageMock;

        private readonly ImgSetting _imgSettings;
        private readonly JwtKeySetting _jwtSettings;

        public AccountServiceTests()
        {
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new DataContext(options);

            _userHelperMock = new Mock<IUserHelper>();
            _emailHelperMock = new Mock<IEmailHelper>();
            _transactionManagerMock = new Mock<ITransactionManager>();
            _localizerMock = new Mock<IStringLocalizer>();
            _fileStorageMock = new Mock<IFileStorage>();

            _imgSettings = new ImgSetting
            {
                ImgNoImage = "no-image.png",
                ImgBaseUrl = "https://base/",
                ImgManager = "manager",
                ImgUsuario = "usuario",
                ImgCorporation = "corp",
                LogoSoftware = "logo-soft"
            };

            _jwtSettings = new JwtKeySetting
            {
                jwtKey = "supersecretkey_supersecretkey_123456"
            };

            var imgOptions = Options.Create(_imgSettings);
            var jwtOptions = Options.Create(_jwtSettings);

            _service = new AccountService(
                _context,
                _userHelperMock.Object,
                _emailHelperMock.Object,
                imgOptions,
                _transactionManagerMock.Object,
                jwtOptions,
                _localizerMock.Object,
                _fileStorageMock.Object
            );

            SetupLocalizerDefaults();
        }

        private void SetupLocalizerDefaults()
        {
            _localizerMock.Setup(x => x["Generic_UserInactive"])
                .Returns(new LocalizedString("Generic_UserInactive", "User inactive"));
            _localizerMock.Setup(x => x["Generic_UserNoRoleAssigned"])
                .Returns(new LocalizedString("Generic_UserNoRoleAssigned", "User has no roles"));
            _localizerMock.Setup(x => x["Generic_CorporationInactive"])
                .Returns(new LocalizedString("Generic_CorporationInactive", "Corporation inactive"));
            _localizerMock.Setup(x => x["Generic_PlanExpired"])
                .Returns(new LocalizedString("Generic_PlanExpired", "Plan expired"));
            _localizerMock.Setup(x => x["Generic_UserBlocked"])
                .Returns(new LocalizedString("Generic_UserBlocked", "User blocked"));
            _localizerMock.Setup(x => x["Generic_AccessDenied"])
                .Returns(new LocalizedString("Generic_AccessDenied", "Access denied"));
            _localizerMock.Setup(x => x["Generic_InvalidCredentials"])
                .Returns(new LocalizedString("Generic_InvalidCredentials", "Invalid credentials"));
            _localizerMock.Setup(x => x[nameof(Resource.Generic_RegisterNotFound)])
                .Returns(new LocalizedString(nameof(Resource.Generic_RegisterNotFound), "Register not found"));
            _localizerMock.Setup(x => x[nameof(Resource.Generic_IdNotFound)])
                .Returns(new LocalizedString(nameof(Resource.Generic_IdNotFound), "Id not found"));
            _localizerMock.Setup(x => x[nameof(Resource.Generic_UserFail)])
                .Returns(new LocalizedString(nameof(Resource.Generic_UserFail), "User fail"));
            _localizerMock.Setup(x => x["Generic_UserFail"])
                .Returns(new LocalizedString("Generic_UserFail", "User fail"));
            _localizerMock.Setup(x => x["Generic_Success"])
                .Returns(new LocalizedString("Generic_Success", "Success"));
        }

        private User CreateUser(string? id = null, bool active = true, string userFrom = "UsuarioSoftware", int? corpId = 1)
        {
            return new User
            {
                Id = id ?? Guid.NewGuid().ToString(),
                UserName = "testuser",
                FirstName = "Test",
                LastName = "User",
                Active = active,
                UserFrom = userFrom,
                CorporationId = corpId,
                Email = "test@aban.com",
                PhotoUser = null
            };
        }

        private void SeedRole(string userId, UserType role)
        {
            _context.UserRoleDetails.Add(new UserRoleDetails
            {
                UserId = userId,
                UserType = role
            });
            _context.SaveChanges();
        }

        private void SeedCorporation(int id, bool active = true, bool expired = false)
        {
            _context.Corporations.Add(new Corporation
            {
                CorporationId = id,
                Name = "Corp X",
                NroDocument = "123456789",
                Phone = "555-1234",
                Address = "Street 1",
                CountryId = 1,
                DateStart = DateTime.UtcNow.AddDays(-10),
                DateEnd = expired ? DateTime.UtcNow.AddDays(-1) : DateTime.UtcNow.AddDays(10),
                Active = active
            });
            _context.SaveChanges();
        }

        // -------------------------------------------------------------
        // LOGIN
        // -------------------------------------------------------------
        [Fact]
        public async Task LoginAsync_ShouldReturnToken_WhenAdminUserValid()
        {
            var loginDto = new LoginDTO { UserName = "testuser", Password = "123456" };
            var user = CreateUser();

            _userHelperMock.Setup(x => x.LoginAsync(loginDto))
                .ReturnsAsync(SignInResult.Success);

            _userHelperMock.Setup(x => x.GetUserByUserNameAsync("testuser"))
                .ReturnsAsync(user);

            SeedRole(user.Id, UserType.Admin);

            var result = await _service.LoginAsync(loginDto);

            Assert.True(result.WasSuccess);
            Assert.NotNull(result.Result);
            Assert.False(string.IsNullOrWhiteSpace(result.Result!.Token));
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnUserInactive_WhenUserNotActive()
        {
            var loginDto = new LoginDTO { UserName = "testuser", Password = "123456" };
            var user = CreateUser(active: false);

            _userHelperMock.Setup(x => x.LoginAsync(loginDto))
                .ReturnsAsync(SignInResult.Success);

            _userHelperMock.Setup(x => x.GetUserByUserNameAsync("testuser"))
                .ReturnsAsync(user);

            var result = await _service.LoginAsync(loginDto);

            Assert.False(result.WasSuccess);
            Assert.Equal("User inactive", result.Message);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnNoRoleAssigned_WhenUserHasNoRoles()
        {
            var loginDto = new LoginDTO { UserName = "testuser", Password = "123456" };
            var user = CreateUser();

            _userHelperMock.Setup(x => x.LoginAsync(loginDto))
                .ReturnsAsync(SignInResult.Success);

            _userHelperMock.Setup(x => x.GetUserByUserNameAsync("testuser"))
                .ReturnsAsync(user);

            var result = await _service.LoginAsync(loginDto);

            Assert.False(result.WasSuccess);
            Assert.Equal("User has no roles", result.Message);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnCorporationInactive_WhenCorpInactive()
        {
            var loginDto = new LoginDTO { UserName = "testuser", Password = "123456" };
            var user = CreateUser(corpId: 1);

            _userHelperMock.Setup(x => x.LoginAsync(loginDto))
                .ReturnsAsync(SignInResult.Success);

            _userHelperMock.Setup(x => x.GetUserByUserNameAsync("testuser"))
                .ReturnsAsync(user);

            SeedRole(user.Id, UserType.Administrator);
            SeedCorporation(1, active: false);

            var result = await _service.LoginAsync(loginDto);

            Assert.False(result.WasSuccess);
            Assert.Equal("Corporation inactive", result.Message);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnPlanExpired_WhenCorpPlanExpired()
        {
            var loginDto = new LoginDTO { UserName = "testuser", Password = "123456" };
            var user = CreateUser(corpId: 1);

            _userHelperMock.Setup(x => x.LoginAsync(loginDto))
                .ReturnsAsync(SignInResult.Success);

            _userHelperMock.Setup(x => x.GetUserByUserNameAsync("testuser"))
                .ReturnsAsync(user);

            SeedRole(user.Id, UserType.Administrator);
            SeedCorporation(1, active: true, expired: true);

            var result = await _service.LoginAsync(loginDto);

            Assert.False(result.WasSuccess);
            Assert.Equal("Plan expired", result.Message);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnInvalidCredentials_WhenLoginFails()
        {
            var loginDto = new LoginDTO { UserName = "testuser", Password = "wrong" };

            _userHelperMock.Setup(x => x.LoginAsync(loginDto))
                .ReturnsAsync(SignInResult.Failed);

            var result = await _service.LoginAsync(loginDto);

            Assert.False(result.WasSuccess);
            Assert.Equal("Invalid credentials", result.Message);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnUserBlocked_WhenLockedOut()
        {
            var loginDto = new LoginDTO { UserName = "testuser", Password = "123456" };

            _userHelperMock.Setup(x => x.LoginAsync(loginDto))
                .ReturnsAsync(SignInResult.LockedOut);

            var result = await _service.LoginAsync(loginDto);

            Assert.False(result.WasSuccess);
            Assert.Equal("User blocked", result.Message);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnAccessDenied_WhenNotAllowed()
        {
            var loginDto = new LoginDTO { UserName = "testuser", Password = "123456" };

            _userHelperMock.Setup(x => x.LoginAsync(loginDto))
                .ReturnsAsync(SignInResult.NotAllowed);

            var result = await _service.LoginAsync(loginDto);

            Assert.False(result.WasSuccess);
            Assert.Equal("Access denied", result.Message);
        }

        // -------------------------------------------------------------
        // RECOVER PASSWORD
        // -------------------------------------------------------------
        [Fact]
        public async Task RecoverPasswordAsync_ShouldFail_WhenEmailNotFound()
        {
            var dto = new RecoveryPassDTO { Email = "no@user.com", UserName = "testuser" };

            _userHelperMock.Setup(x => x.GetUserByEmailAsync(dto.Email))
                .ReturnsAsync((User?)null);

            var result = await _service.RecoverPasswordAsync(dto, "https://front");

            Assert.False(result.WasSuccess);
            Assert.Equal("Register not found", result.Message);
        }

        [Fact]
        public async Task RecoverPasswordAsync_ShouldFail_WhenUserNameNotFound()
        {
            var dto = new RecoveryPassDTO { Email = "test@aban.com", UserName = "testuser" };
            var user = CreateUser();

            _userHelperMock.Setup(x => x.GetUserByEmailAsync(dto.Email))
                .ReturnsAsync(user);

            _userHelperMock.Setup(x => x.GetUserByUserNameAsync(dto.UserName))
                .ReturnsAsync((User?)null);

            var result = await _service.RecoverPasswordAsync(dto, "https://front");

            Assert.False(result.WasSuccess);
            Assert.Equal("Register not found", result.Message);
        }

        [Fact]
        public async Task RecoverPasswordAsync_ShouldFail_WhenIdsDoNotMatch()
        {
            var dto = new RecoveryPassDTO { Email = "test@aban.com", UserName = "testuser" };
            var userEmail = CreateUser(id: Guid.NewGuid().ToString());
            var userName = CreateUser(id: Guid.NewGuid().ToString());

            _userHelperMock.Setup(x => x.GetUserByEmailAsync(dto.Email))
                .ReturnsAsync(userEmail);

            _userHelperMock.Setup(x => x.GetUserByUserNameAsync(dto.UserName))
                .ReturnsAsync(userName);

            var result = await _service.RecoverPasswordAsync(dto, "https://front");

            Assert.False(result.WasSuccess);
            Assert.Equal("Id not found", result.Message);
        }

        [Fact]
        public async Task RecoverPasswordAsync_ShouldReturnSuccess_WhenEmailSent()
        {
            var dto = new RecoveryPassDTO { Email = "test@aban.com", UserName = "testuser" };
            var user = CreateUser();

            _userHelperMock.Setup(x => x.GetUserByEmailAsync(dto.Email))
                .ReturnsAsync(user);

            _userHelperMock.Setup(x => x.GetUserByUserNameAsync(dto.UserName))
                .ReturnsAsync(user);

            _userHelperMock.Setup(x => x.GeneratePasswordResetTokenAsync(user))
                .ReturnsAsync("token123");

            _emailHelperMock.Setup(x => x.ConfirmarCuenta(
                    user.Email!,
                    $"{user.FirstName} {user.LastName}",
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync(new Response { IsSuccess = true, Message = "OK" });

            var result = await _service.RecoverPasswordAsync(dto, "https://front");

            Assert.True(result.WasSuccess);
            Assert.Equal("OK", result.Message);
        }

        [Fact]
        public async Task RecoverPasswordAsync_ShouldReturnFail_WhenEmailSendFails()
        {
            var dto = new RecoveryPassDTO { Email = "test@aban.com", UserName = "testuser" };
            var user = CreateUser();

            _userHelperMock.Setup(x => x.GetUserByEmailAsync(dto.Email))
                .ReturnsAsync(user);

            _userHelperMock.Setup(x => x.GetUserByUserNameAsync(dto.UserName))
                .ReturnsAsync(user);

            _userHelperMock.Setup(x => x.GeneratePasswordResetTokenAsync(user))
                .ReturnsAsync("token123");

            _emailHelperMock.Setup(x => x.ConfirmarCuenta(
                    user.Email!,
                    $"{user.FirstName} {user.LastName}",
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync(new Response { IsSuccess = false, Message = "Error sending" });

            var result = await _service.RecoverPasswordAsync(dto, "https://front");

            Assert.False(result.WasSuccess);
            Assert.Equal("Error sending", result.Message);
        }

        // -------------------------------------------------------------
        // RESET PASSWORD
        // -------------------------------------------------------------
        [Fact]
        public async Task ResetPasswordAsync_ShouldReturnUserFail_WhenEmailNotFound()
        {
            var dto = new ResetPasswordDTO { Email = "no@user.com", UserName = "testuser", Token = "t", NewPassword = "123" };

            _userHelperMock.Setup(x => x.GetUserByEmailAsync(dto.Email))
                .ReturnsAsync((User?)null);

            var result = await _service.ResetPasswordAsync(dto);

            Assert.False(result.WasSuccess);
            Assert.Equal("User fail", result.Message);
        }

        [Fact]
        public async Task ResetPasswordAsync_ShouldReturnUserFail_WhenUserNameNotFound()
        {
            var dto = new ResetPasswordDTO { Email = "test@aban.com", UserName = "testuser", Token = "t", NewPassword = "123" };
            var userEmail = CreateUser();

            _userHelperMock.Setup(x => x.GetUserByEmailAsync(dto.Email))
                .ReturnsAsync(userEmail);

            _userHelperMock.Setup(x => x.GetUserByUserNameAsync(dto.UserName))
                .ReturnsAsync((User?)null);

            var result = await _service.ResetPasswordAsync(dto);

            Assert.False(result.WasSuccess);
            Assert.Equal("User fail", result.Message);
        }

        [Fact]
        public async Task ResetPasswordAsync_ShouldReturnUserFail_WhenIdsDoNotMatch()
        {
            var dto = new ResetPasswordDTO { Email = "test@aban.com", UserName = "testuser", Token = "t", NewPassword = "123" };
            var userEmail = CreateUser(id: Guid.NewGuid().ToString());
            var userName = CreateUser(id: Guid.NewGuid().ToString());

            _userHelperMock.Setup(x => x.GetUserByEmailAsync(dto.Email))
                .ReturnsAsync(userEmail);

            _userHelperMock.Setup(x => x.GetUserByUserNameAsync(dto.UserName))
                .ReturnsAsync(userName);

            var result = await _service.ResetPasswordAsync(dto);

            Assert.False(result.WasSuccess);
            Assert.Equal("User fail", result.Message);
        }

        [Fact]
        public async Task ResetPasswordAsync_ShouldReturnSuccess_WhenResetOk()
        {
            var dto = new ResetPasswordDTO { Email = "test@aban.com", UserName = "testuser", Token = "t", NewPassword = "123" };
            var user = CreateUser();

            _userHelperMock.Setup(x => x.GetUserByEmailAsync(dto.Email))
                .ReturnsAsync(user);

            _userHelperMock.Setup(x => x.GetUserByUserNameAsync(dto.UserName))
                .ReturnsAsync(user);

            _userHelperMock.Setup(x => x.ResetPasswordAsync(user, dto.Token, dto.NewPassword))
                .ReturnsAsync(IdentityResult.Success);

            var result = await _service.ResetPasswordAsync(dto);

            Assert.True(result.WasSuccess);
            Assert.Equal("Success", result.Message);
        }

        [Fact]
        public async Task ResetPasswordAsync_ShouldReturnError_WhenResetFails()
        {
            var dto = new ResetPasswordDTO { Email = "test@aban.com", UserName = "testuser", Token = "t", NewPassword = "123" };
            var user = CreateUser();

            _userHelperMock.Setup(x => x.GetUserByEmailAsync(dto.Email))
                .ReturnsAsync(user);

            _userHelperMock.Setup(x => x.GetUserByUserNameAsync(dto.UserName))
                .ReturnsAsync(user);

            var identityError = new IdentityError { Description = "Reset error" };

            _userHelperMock.Setup(x => x.ResetPasswordAsync(user, dto.Token, dto.NewPassword))
                .ReturnsAsync(IdentityResult.Failed(identityError));

            var result = await _service.ResetPasswordAsync(dto);

            Assert.False(result.WasSuccess);
            Assert.Equal("Reset error", result.Message);
        }

        // -------------------------------------------------------------
        // CHANGE PASSWORD
        // -------------------------------------------------------------
        [Fact]
        public async Task ChangePasswordAsync_ShouldReturnUserFail_WhenUserNotFound()
        {
            var dto = new ChangePasswordDTO { CurrentPassword = "old", NewPassword = "new" };

            _userHelperMock.Setup(x => x.GetUserByUserNameAsync("testuser"))
                .ReturnsAsync((User?)null);

            var result = await _service.ChangePasswordAsync(dto, "testuser");

            Assert.False(result.WasSuccess);
            Assert.Equal("User fail", result.Message);
        }

        [Fact]
        public async Task ChangePasswordAsync_ShouldReturnSuccess_WhenChangeOk()
        {
            var dto = new ChangePasswordDTO { CurrentPassword = "old", NewPassword = "new" };
            var user = CreateUser();

            _userHelperMock.Setup(x => x.GetUserByUserNameAsync("testuser"))
                .ReturnsAsync(user);

            _userHelperMock.Setup(x => x.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword))
                .ReturnsAsync(IdentityResult.Success);

            var result = await _service.ChangePasswordAsync(dto, "testuser");

            Assert.True(result.WasSuccess);
            Assert.Equal("Success", result.Message);
        }

        [Fact]
        public async Task ChangePasswordAsync_ShouldReturnError_WhenChangeFails()
        {
            var dto = new ChangePasswordDTO { CurrentPassword = "old", NewPassword = "new" };
            var user = CreateUser();

            _userHelperMock.Setup(x => x.GetUserByUserNameAsync("testuser"))
                .ReturnsAsync(user);

            var identityError = new IdentityError { Description = "Change error" };

            _userHelperMock.Setup(x => x.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword))
                .ReturnsAsync(IdentityResult.Failed(identityError));

            var result = await _service.ChangePasswordAsync(dto, "testuser");

            Assert.False(result.WasSuccess);
            Assert.Equal("Change error", result.Message);
        }

        // -------------------------------------------------------------
        // CONFIRM EMAIL
        // -------------------------------------------------------------
        [Fact]
        public async Task ConfirmEmailAsync_ShouldReturnUserFail_WhenUserNotFound()
        {
            var userId = Guid.NewGuid().ToString();

            _userHelperMock.Setup(x => x.GetUserByIdAsync(new Guid(userId)))
                .ReturnsAsync((User?)null);

            var result = await _service.ConfirmEmailAsync(userId, "token");

            Assert.False(result.WasSuccess);
            Assert.Equal("User fail", result.Message);
        }

        [Fact]
        public async Task ConfirmEmailAsync_ShouldReturnSuccess_WhenConfirmOk()
        {
            var id = Guid.NewGuid();
            var user = CreateUser(id.ToString());

            _userHelperMock.Setup(x => x.GetUserByIdAsync(id))
                .ReturnsAsync(user);

            _userHelperMock.Setup(x => x.ConfirmEmailAsync(user, "token"))
                .ReturnsAsync(IdentityResult.Success);

            var result = await _service.ConfirmEmailAsync(id.ToString(), "token");

            Assert.True(result.WasSuccess);
            Assert.Equal("Success", result.Message);
        }

        [Fact]
        public async Task ConfirmEmailAsync_ShouldReturnError_WhenConfirmFails()
        {
            var id = Guid.NewGuid();
            var user = CreateUser(id.ToString());

            _userHelperMock.Setup(x => x.GetUserByIdAsync(id))
                .ReturnsAsync(user);

            var identityError = new IdentityError { Description = "Confirm error" };

            _userHelperMock.Setup(x => x.ConfirmEmailAsync(user, "token"))
                .ReturnsAsync(IdentityResult.Failed(identityError));

            var result = await _service.ConfirmEmailAsync(id.ToString(), "token");

            Assert.False(result.WasSuccess);
            Assert.Equal("Confirm error", result.Message);
        }
    }
}