using System.Collections.Generic;
using EPiServer.Core;

namespace MarijasPlayground.EPiServerUI.Admin.Tools
{
    public class RepublishContentInput
    {
        public ContentReference FromReference { get; set; }
        public List<int> ContentTypeIdsToSkip { get; set; }
        public bool SetDefaultValuesForEmptyRequiredProperties { get; set; }
    }
}