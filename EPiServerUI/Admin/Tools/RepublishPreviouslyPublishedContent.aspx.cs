using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using EPiServer;
using EPiServer.Core;
using EPiServer.Data.Entity;
using EPiServer.DataAbstraction;
using EPiServer.DataAccess;
using EPiServer.PlugIn;
using EPiServer.ServiceLocation;

namespace PROJECT.EPiServerUI.Admin.Tools
{
    [GuiPlugIn(
        DisplayName = "Re-publish content",
        Description = "Goes through the whole tree starting from the root and republishes all content (that was previously published)",
        Area = PlugInArea.AdminMenu,
        Url = "~/EPiServerUI/Admin/Tools/RepublishPreviouslyPublishedContent.aspx")]
    public partial class RepublishPreviouslyPublishedContent : Page
    {
        private IContentRepository _contentRepository;
        private IContentTypeRepository _contentTypeRepository;
        
        protected override void OnInit(EventArgs e)
        {
            EnableViewState = false;

            _contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
            _contentTypeRepository = ServiceLocator.Current.GetInstance<IContentTypeRepository>();

            skippedContentCheckBoxList.Items.Clear();

            var alphabeticalContentItems = _contentTypeRepository.List().OrderBy(t => t.LocalizedName).ToList();

            foreach (var contentType in alphabeticalContentItems)
            {
                var checkboxItem = new ListItem(contentType.LocalizedName, contentType.ID.ToString(CultureInfo.InvariantCulture));
                skippedContentCheckBoxList.Items.Add(checkboxItem);
            }

            base.OnInit(e);
        }

        protected void doRepublishButton_OnClick(object sender, EventArgs e)
        {
            var skippedContentTypeIds = new List<int>();

            foreach (ListItem contentType in skippedContentCheckBoxList.Items)
            {
                if (contentType.Selected)
                {
                    skippedContentTypeIds.Add(int.Parse(contentType.Value));
                }
            }

            RepublishContentAndChildren(ContentReference.RootPage, skippedContentTypeIds);

            doneLabel.Visible = true;
        }

        private void RepublishContentAndChildren(ContentReference reference, List<int> skippedContentTypeIds)
        {
            IContent content;
            _contentRepository.TryGet(reference, out content);
            var versionable = content as IVersionable;

            if (versionable != null &&
                versionable.StartPublish <= DateTime.Now &&
                !versionable.IsPendingPublish &&
                !skippedContentTypeIds.Contains(content.ContentTypeID))
            {
                try
                {
                    var clone = ((IReadOnly)content).CreateWritableClone() as IContent;
								
                    // this is the place where you do "something" with each of the content items you are going through

                    _contentRepository.Save(clone, SaveAction.Publish);
                }
                catch (Exception ex)
                {
                    // add logging here
                }
            }

            // don't publish content from recycle bin
            if (reference != ContentReference.WasteBasket)
            {
                var children = _contentRepository.GetChildren<IContent>(reference).ToList();
                foreach (var child in children)
                {
                    RepublishContentAndChildren(child.ContentLink, skippedContentTypeIds);
                }
            }
        }
    }
}