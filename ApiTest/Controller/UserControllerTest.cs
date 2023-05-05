using Api.Interface;
using Api.Models;
using Api.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Api;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Api.Dto;
using Api.Models.Base;

namespace ApiTest.Controller
{
    public class UserControllerTest
    {
        private readonly Mock<IUserRepository> repositoryStubUser = new();
        private readonly Mock<IGroupRepository> repositoryStubGroup = new();
        private readonly Mock<IStateRepository> repositoryStubState = new();
        readonly Random rand = new();

        [Fact]
        public async void UserController_GetUsers_ReturnPaginatedList()
        {
            int itemsExpectedNumber = 10;
            var expectedItems = new List<User>();
            for (int i = 0; i < itemsExpectedNumber; i++)
            {
                expectedItems.Add(CreateRandomUser());
            }
            var pagination = new Mock<Pagination>();

            repositoryStubUser.Setup(r => r.GetUsersAsync(It.IsAny<Pagination>())).ReturnsAsync(expectedItems);

            var controller = new UserController(repositoryStubUser.Object, 
                repositoryStubGroup.Object, repositoryStubState.Object);

            var result = await controller.GetUsers(pagination.Object);

            result.Should().BeOfType<OkObjectResult>();

            var okResult = result as OkObjectResult;
            var verifyRes = (okResult!.Value as List<User>);
            verifyRes.Should().BeEquivalentTo(expectedItems);
        }

        [Fact]
        public async Task UserController_GetUser_ReturnNotFound()
        {
            repositoryStubUser.Setup(r => r.GetUserAsync(It.IsAny<int>()))!.ReturnsAsync((User)null);

            var controller = new UserController(repositoryStubUser.Object,
                repositoryStubGroup.Object, repositoryStubState.Object);

            var result = await controller.GetUser(rand.Next(100));

            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task UserController_GetUser_ReturnsExpectedItem()
        {
            var expectedItem = CreateRandomUser();

            repositoryStubUser.Setup(r => r.GetUserAsync(It.IsAny<int>())).ReturnsAsync(expectedItem);
            repositoryStubUser.Setup(r => r.UserExistsAsync(It.IsAny<int>())).ReturnsAsync(true);

            var controller = new UserController(repositoryStubUser.Object,
                repositoryStubGroup.Object, repositoryStubState.Object);

            var result = await controller.GetUser(rand.Next(100));

            result.Should().BeOfType<OkObjectResult>();

            var okResult = result as OkObjectResult;
            var verifyRes = (okResult!.Value as User);
            verifyRes.Should().BeEquivalentTo(expectedItem);
        }

        [Fact]
        public async Task UserController_CreateUser_ReturnsBadRequest()
        {
            var controller = new UserController(repositoryStubUser.Object,
                repositoryStubGroup.Object, repositoryStubState.Object);

            var result = await controller.CreateUser((UserCreate)null);

            result.Should().BeOfType<BadRequestResult>();
        }

        [Fact]
        public async Task UserController_CreateUser_ReturnsGroupNotFound()
        {
            var randomState = CreateRandomState();
            var randomUser = CreateRandomUser();
            UserCreate userCreate = new()
            {
                Login = randomUser.Login,
                Password = randomUser.Password,
                User_group_id = randomUser.Id,
            };

            repositoryStubGroup.Setup(r => r.GetGroupAsync(It.IsAny<int>()))!.ReturnsAsync((User_group)null);
            repositoryStubState.Setup(r => r.GetStateAsync(It.IsAny<string>())).ReturnsAsync(randomState);

            var controller = new UserController(repositoryStubUser.Object, 
                repositoryStubGroup.Object, repositoryStubState.Object);

            var result = await controller.CreateUser(userCreate);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task UserController_CreateUser_ReturnsStateNotFound()
        {
            var randomGroup = CreateRandomGroup();
            var randomUser = CreateRandomUserCreate();

            repositoryStubGroup.Setup(r => r.GetGroupAsync(It.IsAny<int>())).ReturnsAsync(randomGroup);
            repositoryStubState.Setup(r => r.GetStateAsync(It.IsAny<string>()))!.ReturnsAsync((User_state)null);

            var controller = new UserController(repositoryStubUser.Object, 
                repositoryStubGroup.Object, repositoryStubState.Object);

            var result = await controller.CreateUser(randomUser);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task UserController_CreateUser_ReturnsStateBadRequest_AdminInDatabase()
        {
            var randomGroup = CreateRandomGroup();
            randomGroup.Code = "admin";
            var randomState = CreateRandomState();
            var randomUser = CreateRandomUserCreate();

            repositoryStubGroup.Setup(r => r.GetGroupAsync(It.IsAny<int>())).ReturnsAsync(randomGroup);
            repositoryStubState.Setup(r => r.GetStateAsync(It.IsAny<string>())).ReturnsAsync(randomState);
            repositoryStubUser.Setup(r => r.UsersAlredyHaveAdminAsync()).ReturnsAsync(true);

            var controller = new UserController(repositoryStubUser.Object, 
                repositoryStubGroup.Object, repositoryStubState.Object);

            var result = await controller.CreateUser(randomUser);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task UserController_CreateUser_ReturnsOK()
        {
            var randomGroup = CreateRandomGroup();
            var randomState = CreateRandomState();
            var randomUser = CreateRandomUserCreate();

            repositoryStubGroup.Setup(r => r.GetGroupAsync(It.IsAny<int>())).ReturnsAsync(randomGroup);
            repositoryStubState.Setup(r => r.GetStateAsync(It.IsAny<string>())).ReturnsAsync(randomState);
            repositoryStubUser.Setup(r => r.UsersAlredyHaveAdminAsync()).ReturnsAsync(false);

            var controller = new UserController(repositoryStubUser.Object, 
                repositoryStubGroup.Object, repositoryStubState.Object);

            var result = await controller.CreateUser(randomUser);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UserController_DeleteUser_ReturnUserNotFound()
        {
            var randomState = CreateRandomState();

            repositoryStubUser.Setup(r => r.GetUserAsync(It.IsAny<int>()))!.ReturnsAsync((User)null);
            repositoryStubState.Setup(r => r.GetStateAsync(It.IsAny<string>())).ReturnsAsync(randomState);

            var controller = new UserController(repositoryStubUser.Object, 
                repositoryStubGroup.Object, repositoryStubState.Object);

            var result = await controller.DeleteUser(It.IsAny<int>());

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task UserController_DeleteUser_ReturnsStateNotFound()
        {
            var randomUser = CreateRandomUser();

            repositoryStubUser.Setup(r => r.GetUserAsync(It.IsAny<int>())).ReturnsAsync(randomUser);
            repositoryStubState.Setup(r => r.GetStateAsync(It.IsAny<string>()))!.ReturnsAsync((User_state)null);

            var controller = new UserController(repositoryStubUser.Object, 
                repositoryStubGroup.Object, repositoryStubState.Object);

            var result = await controller.DeleteUser(It.IsAny<int>());

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task UserController_DeleteUser_ReturnsOK()
        {
            var randomState = CreateRandomState();
            var randomUser = CreateRandomUser();

            repositoryStubUser.Setup(r => r.GetUserAsync(It.IsAny<int>())).ReturnsAsync(randomUser);
            repositoryStubState.Setup(r => r.GetStateAsync(It.IsAny<string>()))!.ReturnsAsync(randomState);

            var controller = new UserController(repositoryStubUser.Object, 
                repositoryStubGroup.Object, repositoryStubState.Object);

            var result = await controller.DeleteUser(It.IsAny<int>());

            result.Should().BeOfType<OkObjectResult>();
        }

        private User CreateRandomUser()
        {
            return new()
            {
                Id = rand.Next(100),
                Login = "Login",
                Password = "password",
                User_group_id = CreateRandomGroup(),
                User_state_id = CreateRandomState(),
            };
        }

        private User_group CreateRandomGroup()
        {
            return new()
            {
                Id = rand.Next(100),
                Code = RandomString(5),
                Description = RandomString(20),
            };
        }

        private User_state CreateRandomState()
        {
            return new()
            {
                Id = rand.Next(100),
                Code = RandomString(5),
                Description = RandomString(20),
            };
        }

        private UserCreate CreateRandomUserCreate()
        {
            return new()
            {
                Login = "Login",
                Password = "password",
                User_group_id = CreateRandomGroup().Id,
            };
        }

        private string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[rand.Next(s.Length)]).ToArray());
        }
    }
}
