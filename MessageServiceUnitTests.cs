using messager;
using messager.models;
using messager.Services;
using messager.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Azure.Documents.SystemFunctions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace MessagerUnitTests
{
    public class MessageServiceUnitTests
    {
        private Mock<UserManager<UserModel>> userManagerMock;
        private Mock<SignInManager<UserModel>> signInManagerMock;
        private Mock<MessageService> messageServiceMock;
        private Mock<AppDbContext> ContextMock;
        private Mock<DbSet<MessageModel>> MockSet;
        private MessageService messageService;
        private AppDbContext Context;
        [SetUp]
        public void Setup()
        {
            var UserStoreMock = new Mock<IUserStore<UserModel>>();

            userManagerMock = new Mock<UserManager<UserModel>>(UserStoreMock.Object, null, null, null, null, null, null, null, null);

            var contextAccessor = new Mock<IHttpContextAccessor>();
            /*
                        var options = new DbContextOptionsBuilder<AppDbContext>()
                        .UseInMemoryDatabase(databaseName: "TestDatabase")
                        .Options;

                        MockSet = new Mock<DbSet<MessageModel>>();

                        ContextMock = new Mock<AppDbContext>(options);
                        ContextMock.Setup(x => x.MessageList).Returns(MockSet.Object);
                        messageServiceMock = new Mock<MessageService>(ContextMock.Object);
            
            */
            var userPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<UserModel>>();

            signInManagerMock = new Mock<SignInManager<UserModel>>(userManagerMock.Object, contextAccessor.Object, userPrincipalFactory.Object, null, null, null, null);

            var options = new DbContextOptionsBuilder<AppDbContext>()
           .UseInMemoryDatabase(databaseName: "TestDatabase")
           .Options;

            

            Context = new AppDbContext(options);

          

            messageService = new MessageService(Context);
        }
        [Test]
        public async Task AddMessage_AddingMessage()
        {
          

            var Content = "Content";
            var CreatorId = "1";
            var ReciverId = "2";

            
            await messageService.AddMessage(Content, ReciverId, CreatorId);



            Assert.True(Context.MessageList.ToList().Count == 1);
        }
        [Test]
        public async Task GetMessages_GettingMessages_TwoParameters()
        {

            var MessageList = new List<MessageModel>
            {
                new MessageModel{Content= "content",CreatorId = "1",ReciverId = "2" },
                new MessageModel{Content= "content",CreatorId = "2",ReciverId = "1" },
                new MessageModel{Content= "content",CreatorId = "1",ReciverId = "3" },
                new MessageModel{Content= "content",CreatorId = "3",ReciverId = "1" }
            };

            Context.MessageList.AddRange(MessageList);
            Context.SaveChanges();

            var result = await messageService.GetMessages("1","3");
                    
            Assert.True(result.Count == 2);          
        }
        [Test]
        public async Task GetMessages_GettingMessages_OneParameters()
        {
            var MessageList = new List<MessageModel>
            {
                new MessageModel{Content= "content",CreatorId = "1",ReciverId = "2" },
                new MessageModel{Content= "content",CreatorId = "2",ReciverId = "1" },
                new MessageModel{Content= "content",CreatorId = "1",ReciverId = "3" },
                new MessageModel{Content= "content",CreatorId = "3",ReciverId = "1" }
            };

            Context.MessageList.AddRange(MessageList);
            Context.SaveChanges();

            var result = await messageService.GetMessages("3");

            Assert.True(result.Count > 0);
        }

        }
    }
