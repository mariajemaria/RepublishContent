using System;
using System.Linq;
using EPiServer;
using EPiServer.Core;
using EPiServer.Data.Entity;
using EPiServer.DataAbstraction;
using EPiServer.DataAccess;

namespace MarijasPlayground.EPiServerUI.Admin.Tools
{
    public interface IRepublishContentHelper
    {
        void RepublishContentAndChildren(RepublishContentInput input);
    }

    public class RepublishContentHelper : IRepublishContentHelper
    {
        private readonly IContentRepository _contentRepo;
        private readonly IContentTypeRepository _contentTypeRepo;

        public RepublishContentHelper(IContentRepository contentRepo, IContentTypeRepository contentTypeRepo)
        {
            _contentRepo = contentRepo;
            _contentTypeRepo = contentTypeRepo;
        }

        public void RepublishContentAndChildren(RepublishContentInput input)
        {
            RepublishContentAndChildrenRecursively(input);
        }

        private void RepublishContentAndChildrenRecursively(RepublishContentInput input)
        {

            IContent content;
            _contentRepo.TryGet(input.FromReference, out content);
            var versionable = content as IVersionable;

            if (versionable != null &&
                versionable.StartPublish <= DateTime.Now &&
                !versionable.IsPendingPublish &&
                !input.ContentTypeIdsToSkip.Contains(content.ContentTypeID))
            {
                try
                {
                    var clone = ((IReadOnly)content).CreateWritableClone() as IContent;

                    // this is the place where you do "something" with each of the content items you are going through

                    SetPropertyDefaultValueIfRequired(clone);

                    _contentRepo.Save(clone, SaveAction.Publish);
                }
                catch (Exception ex)
                {
                    // add logging here
                }
            }

            // don't publish content from recycle bin
            if (input.FromReference != ContentReference.WasteBasket)
            {
                var children = _contentRepo.GetChildren<IContent>(input.FromReference).ToList();
                foreach (var child in children)
                {
                    var newInput = new RepublishContentInput
                    {
                        ContentTypeIdsToSkip = input.ContentTypeIdsToSkip,
                        FromReference = child.ContentLink,
                        SetDefaultValuesForEmptyRequiredProperties = input.SetDefaultValuesForEmptyRequiredProperties
                    };

                    RepublishContentAndChildrenRecursively(newInput);
                }
            }
        }

        private void SetPropertyDefaultValueIfRequired(IContent clone)
        {
            var contentType = _contentTypeRepo.Load(clone.ContentTypeID);

            foreach (var property in clone.Property)
            {
                var propertyDefinition = contentType.PropertyDefinitions.FirstOrDefault(p => p.ID == property.PropertyDefinitionID);
                
                if (propertyDefinition != null &&
                    propertyDefinition.DefaultValue != null &&
                    property.IsRequired &&
                    property.IsNull)
                {
                    clone.Property[property.Name].Value = propertyDefinition.DefaultValue;
                }
            }
        }
    }
}