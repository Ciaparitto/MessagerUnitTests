using messager;
using messager.models;
using messager.Services;
using messager.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Threading.Tasks;

namespace MessagerUnitTests
{

    public class UserSerivceTests
    {
        private Mock<UserManager<UserModel>> _userManager;
        private Mock<SignInManager<UserModel>> _signInManager;
        private Mock<UserService> _userService;
        private Mock<MessageService> messageServiceMock;
        private AppDbContext ContextMock;
        [SetUp]
        public void Setup()
        {
            var UserStoreMock = new Mock<IUserStore<UserModel>>();
            _userManager = new Mock<UserManager<UserModel>>(UserStoreMock.Object, null, null, null, null, null, null, null, null);

            var contextAccessor = new Mock<IHttpContextAccessor>();

            var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
            ContextMock = new AppDbContext(options);

            messageServiceMock = new Mock<MessageService>(ContextMock);

            var userPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<UserModel>>();

            _signInManager = new Mock<SignInManager<UserModel>>(_userManager.Object, contextAccessor.Object, userPrincipalFactory.Object, null, null, null, null);

            _userService = new Mock<UserService>(messageServiceMock.Object ,_userManager.Object, _signInManager.Object, contextAccessor.Object);
        }

        [Test]
        public async Task Register_CreateAndSignInUser()
        {
            var UserName = "User";
            var Password = "UserPassword";
            var Email = "User@Gmail.com";


            _userManager.Setup(x => x.CreateAsync(It.IsAny<UserModel>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            _signInManager.Setup(x => x.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), false, false)).ReturnsAsync(SignInResult.Success);

            await _userService.Object.Register(UserName, Password, Email);
           

            _userManager.Verify(x => x.CreateAsync(It.Is<UserModel>(x => x.UserName == UserName && x.Email == Email), Password), Times.Once);
            _signInManager.Verify(x => x.PasswordSignInAsync(UserName, Password, false, false), Times.Once);


        }
        [Test]
        public async Task Login_SignInUser()
        {
            var UserName = "User";
            var Password = "UserPassword";

            _signInManager.Setup(x => x.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), false, false)).ReturnsAsync(SignInResult.Success);

            await _userService.Object.Login(UserName, Password);

            _signInManager.Verify(x => x.PasswordSignInAsync(UserName, Password, false, false), Times.Once);

        }

    }

}