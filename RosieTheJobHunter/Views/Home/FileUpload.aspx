<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="FileUpload.aspx.cs" Inherits="RosieTheJobHunter.FileUpload" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server" enctype="multipart/form-data">
    <input type="file" id="myFile" name="myFile" />
    <asp:Button runat="server" ID="btnUpload" OnClick="btnUploadClick" Text="Upload" />
</form>
</body>
</html>
