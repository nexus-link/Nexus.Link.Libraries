using System;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Libraries.Core.Error.Logic;
using Xunit;

namespace Nexus.Link.Capabilities.WorkflowMgmt.UnitTests.Services
{
    public abstract class ActivityParameterServiceTestsBase<TContractException>
        where TContractException : Exception
    {
        private readonly IActivityParameterService _workflowParameterService;

        protected ActivityParameterServiceTestsBase(IActivityParameterService workflowParameterService)
        {
            _workflowParameterService = workflowParameterService;
        }

        [Fact]
        public async Task CreateAndReadAsync()
        {
            // Arrange
            var masterId = Guid.NewGuid().ToString();
            var name = "Name1";
            var itemToCreate = new ActivityParameterCreate
            {
                ActivityFormId = masterId,
                Name = name
            };

            // Act
            await _workflowParameterService.CreateWithSpecifiedIdAsync(masterId, name, itemToCreate);
            var readItem = await _workflowParameterService.ReadAsync(masterId, name);

            // Assert
            Assert.NotNull(readItem);
            Assert.Equal(masterId, readItem.ActivityFormId);
            Assert.Equal(name, readItem.Name);
        }

        [Fact]
        public async Task Create_Given_SameParameter_Gives_Exception()
        {
            // Arrange
            var masterId = Guid.NewGuid().ToString();
            var name = "Name1";
            var itemToCreate = new ActivityParameterCreate
            {
                ActivityFormId = masterId,
                Name = name
            };
            await _workflowParameterService.CreateWithSpecifiedIdAsync(masterId, name, itemToCreate);

            // Act && Assert
            await Assert.ThrowsAsync<FulcrumConflictException>(() =>
                _workflowParameterService.CreateWithSpecifiedIdAsync(masterId, name, itemToCreate));
        }
    }
}
