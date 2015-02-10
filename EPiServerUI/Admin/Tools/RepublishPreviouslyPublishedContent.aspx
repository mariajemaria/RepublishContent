<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RepublishPreviouslyPublishedContent.aspx.cs" Inherits="PROJECT.Web.EPiServerUI.Admin.Tools.RepublishPreviouslyPublishedContent" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Republish previously published content</title>

</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:Label runat="server" ID="doneLabel" Text="Done!" Visible="false" />
            <p>
                <strong>Note:</strong> This job publishes goes through the whole tree and republishes all pages (that were previously published).
                <br/>
            </p>
            
            <p>Skip the following content types:</p>
            
            <input type="checkbox" class="js-toggle-all-page-types" value="Toggle all/none" /> <strong>Toggle all/none</strong>
            <asp:CheckBoxList runat="server" ID="skippedContentCheckBoxList" CssClass="js-skipped-page-types" />
            <br />
            <asp:Button runat="server" ID="doRepublishButton" OnClick="doRepublishButton_OnClick" Text="Re-publish all" />

        </div>
    </form>
    <script src="http://code.jquery.com/jquery-latest.min.js" type="text/javascript"></script>
    <script type="text/javascript">
        $(document).ready(function () {
            $(".js-toggle-all-page-types").change(function () {
                var allChecked = $(this).is(':checked');
                var allCheckboxes = $(".js-skipped-page-types input:checkbox");
                $(allCheckboxes).prop("checked", allChecked);
            });
        });
    </script>
</body>
</html>
