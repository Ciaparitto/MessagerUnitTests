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
        private Mock<UserManager<UserModel>> _userManagerMock;
        private Mock<SignInManager<UserModel>> _signInManager;
        private Mock<UserService> _userServiceMock;
        private Mock<MessageService> messageServiceMock;
        private AppDbContext Context;
        private UserManager<UserModel> _userManager;
        private UserService _userService;

        [SetUp]
        public void Setup()
        {
            var UserStoreMock = new Mock<IUserStore<UserModel>>();
            _userManagerMock = new Mock<UserManager<UserModel>>(UserStoreMock.Object, null, null, null, null, null, null, null, null);

            var contextAccessor = new Mock<IHttpContextAccessor>();


            var userStoreMock = new Mock<IUserStore<UserModel>>();
            var queryableStoreMock = userStoreMock.As<IQueryableUserStore<UserModel>>();
            _userManager = new UserManager<UserModel>(userStoreMock.Object, null, null, null, null, null, null, null, null);

            var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

            Context = new AppDbContext(options);

            messageServiceMock = new Mock<MessageService>(Context);

            var userPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<UserModel>>();

            _signInManager = new Mock<SignInManager<UserModel>>(_userManagerMock.Object, contextAccessor.Object, userPrincipalFactory.Object, null, null, null, null);

            _userServiceMock = new Mock<UserService>(messageServiceMock.Object , _userManagerMock.Object, _signInManager.Object, contextAccessor.Object);

            _userService = new UserService(messageServiceMock.Object, _userManager, _signInManager.Object, contextAccessor.Object);
        }

        [Test]
        public async Task Register_CreateAndSignInUser()
        {
            var UserName = "User";
            var Password = "UserPassword";
            var Email = "User@Gmail.com";


            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<UserModel>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            _signInManager.Setup(x => x.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), false, false)).ReturnsAsync(SignInResult.Success);

            await _userServiceMock.Object.Register(UserName, Password, Email);


            _userManagerMock.Verify(x => x.CreateAsync(It.Is<UserModel>(x => x.UserName == UserName && x.Email == Email), Password), Times.Once);
            _signInManager.Verify(x => x.PasswordSignInAsync(UserName, Password, false, false), Times.Once);


        }
        [Test]
        public async Task Login_SignInUser()
        {
            var UserName = "User";
            var Password = "UserPassword";

            _signInManager.Setup(x => x.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), false, false)).ReturnsAsync(SignInResult.Success);

            await _userServiceMock.Object.Login(UserName, Password);

            _signInManager.Verify(x => x.PasswordSignInAsync(UserName, Password, false, false), Times.Once);

        }
        [Test]
        public async Task GetUsers_GettingUsers()
        {
            var MessageList = new List<MessageModel>
            {
                new MessageModel{Content= "content",CreatorId = "1",ReciverId = "2" },
                new MessageModel{Content= "content",CreatorId = "2",ReciverId = "1" },
                new MessageModel{Content= "content",CreatorId = "1",ReciverId = "3" },
                new MessageModel{Content= "content",CreatorId = "3",ReciverId = "1" },
                new MessageModel{Content= "content",CreatorId = "1",ReciverId = "3" }
            };

            Context.MessageList.AddRange(MessageList);
            Context.SaveChanges();

            var result = await _userService.GetUsers("1");
            Assert.True(result.Count == 2);
        }

    }

}