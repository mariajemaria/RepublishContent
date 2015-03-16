using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.PlugIn;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Shell.WebForms;

namespace MarijasPlayground.EPiServerUI.Admin.Tools
{
    [GuiPlugIn(
        DisplayName = "Re-publish content",
        Description = "Goes through the whole tree starting from the root and republishes all content (that was previously published)",
        Area = PlugInArea.AdminMenu,
        Url = "~/EPiServerUI/Admin/Tools/RepublishPreviouslyPublishedContent.aspx")]
    public partial class RepublishPreviouslyPublishedContent : ContentWebFormsBase
    {
        private IContentTypeRepository _contentTypeRepo;

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

            _contentTypeRepo = ServiceLocator.Current.GetInstance<IContentTypeRepository>();

            CheckAccess();
            skippedContentCheckBoxList.Items.Clear();
            
            var alphabeticalContentItems = _contentTypeRepo.List().OrderBy(t => t.LocalizedName).ToList();

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

            var inputForRepublish = new RepublishContentInput
            {
                FromReference = ContentReference.RootPage,
                ContentTypeIdsToSkip = skippedContentTypeIds,
                SetDefaultValuesForEmptyRequiredProperties = setDefaultValuesForPropertiesCheckBox.Checked
            };

            var republishContentHelper = ServiceLocator.Current.GetInstance<IRepublishContentHelper>();
            republishContentHelper.RepublishContentAndChildren(inputForRepublish);

            doneLabel.Visible = true;
        }

    }
}