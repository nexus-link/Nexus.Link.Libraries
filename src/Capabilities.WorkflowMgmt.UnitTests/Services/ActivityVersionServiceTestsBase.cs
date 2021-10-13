using System;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services;
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
            var childId = await _service.CreateChildAsync(parentId, itemToCreate);
            var readItem = await _service.FindUniqueByWorkflowVersionActivityAsync(parentId, activityFormId);

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
            var parentId = Guid.NewGuid().ToString();
            var activityFormId = Guid.NewGuid().ToString();
            var itemToCreate = new ActivityVersionCreate
            {
                WorkflowVersionId = parentId,
                Position = 1,
                ActivityFormId = activityFormId,
                ParentActivityVersionId = Guid.NewGuid().ToString()
            };
            var childId = await _service.CreateChildAsync(parentId, itemToCreate);
            var itemToUpdate = await _service.FindUniqueByWorkflowVersionActivityAsync(parentId, activityFormId);

            // Act
            itemToUpdate.Position = 2;
            await _service.UpdateAsync(childId, itemToUpdate);
            var readItem = await _service.FindUniqueByWorkflowVersionActivityAsync(parentId, activityFormId);

            // Assert
            Assert.Equal(childId, readItem.Id);
            Assert.Equal(itemToUpdate.Position, readItem.Position);
        }

        [Fact]
        public async Task Update_Given_WrongEtag_Gives_Exception()
        {
            // Arrange
            var parentId = Guid.NewGuid().ToString();
            var activityFormId = Guid.NewGuid().ToString();
            var itemToCreate = new ActivityVersionCreate
            {
                WorkflowVersionId = parentId,
                Position = 1,
                ActivityFormId = activityFormId,
                ParentActivityVersionId = Guid.NewGuid().ToString()
            };
            var childId = await _service.CreateChildAsync(parentId, itemToCreate);
            var itemToUpdate = await _service.FindUniqueByWorkflowVersionActivityAsync(parentId, activityFormId);

            // Act & Assert
            itemToUpdate.Position = 2;
            itemToUpdate.Etag = Guid.NewGuid().ToString();
            await Assert.ThrowsAsync<FulcrumConflictException>(() =>
                _service.UpdateAsync(childId, itemToUpdate));
        }
    }
}