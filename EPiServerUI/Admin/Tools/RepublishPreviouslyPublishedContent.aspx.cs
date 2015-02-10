using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;
using EPiServer;
using EPiServer.Core;
using EPiServer.Data.Entity;
using EPiServer.DataAccess;
using EPiServer.PlugIn;
using EPiServer.Security;
using EPiServer.Shell.WebForms;

namespace PROJECT.EPiServerUI.Admin.Tools
{
    [GuiPlugIn(
        DisplayName = "Re-publish content",
        Description = "Goes through the whole tree starting from the root and republishes all content (that was previously published)",
        Area = PlugInArea.AdminMenu,
        Url = "~/EPiServerUI/Admin/Tools/RepublishPreviouslyPublishedContent.aspx")]
    public partial class RepublishPreviouslyPublishedContent : ContentWebFormsBase
    {
        public override AccessLevel RequiredAccess()
        {
            return AccessLevel.Administer;
        }

        protected override bool SetMasterPageOnPreInit
        {
            get
            {
                return false;
            }
        }
        
        protected override void OnInit(EventArgs e)
        {
            EnableViewState = false;
            
            CheckAccess();
            skippedContentCheckBoxList.Items.Clear();

            var alphabeticalContentItems = ContentTypeRepository.List().OrderBy(t => t.LocalizedName).ToList();

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
            ContentRepository.TryGet(reference, out content);
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

                    ContentRepository.Save(clone, SaveAction.Publish);
                }
                catch (Exception ex)
                {
                    // add logging here
                }
            }

            // don't publish content from recycle bin
            if (reference != ContentReference.WasteBasket)
            {
                var children = ContentRepository.GetChildren<IContent>(reference).ToList();
                foreach (var child in children)
                {
                    RepublishContentAndChildren(child.ContentLink, skippedContentTypeIds);
                }
            }
        }
    }
}