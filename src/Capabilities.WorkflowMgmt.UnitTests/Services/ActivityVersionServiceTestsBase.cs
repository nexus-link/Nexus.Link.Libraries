using System;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.Configuration;
using Nexus.Link.Libraries.Core.Error.Logic;
using Xunit;

namespace Nexus.Link.Capabilities.WorkflowMgmt.UnitTests.Services
{
    public abstract class ActivityVersionServiceTestsBase
    {
        private readonly IActivityVersionService _service;

        protected ActivityVersionServiceTestsBase(IActivityVersionService service)
        {
            _service = service;
        }

        [Fact]
        public async Task CreateAndReadAsync()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();
            var parentId = Guid.NewGuid().ToString();
            var activityFormId = Guid.NewGuid().ToString();
            var itemToCreate = new ActivityVersionCreate
            {
                WorkflowVersionId = parentId,
                Position = 1,
                ActivityFormId = activityFormId,
                ParentActivityVersionId = Guid.NewGuid().ToString()
            };

            // Act
            var activityVersion = await _service.CreateWithSpecifiedIdAndReturnAsync(id, itemToCreate);
            var childId = activityVersion.Id;
            var readItem = await _service.ReadAsync(id);

            // Assert
            Assert.NotNull(readItem);
            Assert.Equal(childId, readItem.Id);
            Assert.Equal(parentId, readItem.WorkflowVersionId);
            Assert.Equal(itemToCreate.Position, readItem.Position);
            Assert.Equal(itemToCreate.ActivityFormId, readItem.ActivityFormId);
            Assert.Equal(itemToCreate.ParentActivityVersionId, readItem.ParentActivityVersionId);
        }

        [Fact]
        public async Task UpdateAsync()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();
            var parentId = Guid.NewGuid().ToString();
            var activityFormId = Guid.NewGuid().ToString();
            var itemToCreate = new ActivityVersionCreate
            {
                WorkflowVersionId = parentId,
                Position = 1,
                ActivityFormId = activityFormId,
                ParentActivityVersionId = Guid.NewGuid().ToString(),
                FailUrgency = ActivityFailUrgencyEnum.Stopping
            };
            var activityVersion = await _service.CreateWithSpecifiedIdAndReturnAsync(id, itemToCreate);
            var childId = activityVersion.Id;
            var itemToUpdate = await _service.ReadAsync(id);

            // Act
            itemToUpdate.Position = 2;
            itemToUpdate.FailUrgency = ActivityFailUrgencyEnum.Ignore;
            await _service.UpdateAndReturnAsync(childId, itemToUpdate);
            var readItem = await _service.ReadAsync(id);

            // Assert
            Assert.Equal(childId, readItem.Id);
            Assert.Equal(itemToUpdate.Position, readItem.Position);
            Assert.Equal(itemToUpdate.FailUrgency, readItem.FailUrgency);
        }

        [Fact]
        public async Task Update_Given_WrongEtag_Gives_Exception()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();
            var parentId = Guid.NewGuid().ToString();
            var activityFormId = Guid.NewGuid().ToString();
            var itemToCreate = new ActivityVersionCreate
            {
                WorkflowVersionId = parentId,
                Position = 1,
                ActivityFormId = activityFormId,
                FailUrgency = ActivityFailUrgencyEnum.Stopping,
                ParentActivityVersionId = Guid.NewGuid().ToString()
            };
            var activityVersion = await _service.CreateWithSpecifiedIdAndReturnAsync(id, itemToCreate);
            var childId = activityVersion.Id;
            var itemToUpdate = await _service.ReadAsync(id);

            // Act & Assert
            itemToUpdate.Position = 2;
            itemToUpdate.Etag = Guid.NewGuid().ToString();
            await Assert.ThrowsAsync<FulcrumConflictException>(() =>
                _service.UpdateAndReturnAsync(childId, itemToUpdate));
        }
    }
}
