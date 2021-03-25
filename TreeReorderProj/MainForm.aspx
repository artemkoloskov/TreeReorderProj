<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MainForm.aspx.cs" Inherits="TreeReorderProj.MainForm" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Tree Reorder</title>

    <script src="Script/jquery-1.11.0.min.js" type="text/javascript"></script>
    <script src="//code.jquery.com/ui/1.11.2/jquery-ui.js" type="text/javascript"></script>
    <%--Скрипт обработки Drag-and-drop--%>
    <script type="text/javascript">
        $(function () {
            <%--Переменная c адресом C# метода для вызова обновления БД--%>
            var pageUrl = '<%= ResolveUrl("~/MainForm.aspx/UpdateDB") %>';

            <%--Обработка ноды, которую тянут--%>
            $(".treeNode").draggable({ helper: 'clone', opacity: 0.3, cursor: "crosshair" });
            <%--Обработка ноды в которую перетягивают ноду--%>
            $("#<%= MyTreeView.ClientID%> a.treeNode").droppable({
                accept: " .treeNode",
                <%--метод определяющий ID претягиваемой ноды и ноды в которую перетянули и запускающий через ajax обновление БД--%>
                drop: function (event, ui) {

                    var droppedOnNodeID = "";
                    var draggedNodeID = "";
                    <%--Вычленение ID ноды на которую перетянули--%>
                    var hrefParts = $($(this).context.href.split("\\"));
                    if (hrefParts.length > 1) {
                        droppedOnNodeID = hrefParts[hrefParts.length - 1];
                        droppedOnNodeID = droppedOnNodeID.substring(0, droppedOnNodeID.length - 2);
                    } else {
                        droppedOnNodeID = hrefParts[0];
                        droppedOnNodeID = droppedOnNodeID.substring(droppedOnNodeID.indexOf("'s") + 2, droppedOnNodeID.length - 2)
                    }
                    <%--Вычленение ID перетянутой ноды--%>
                    hrefParts = $(ui.draggable.context.href.split("\\"));
                    if (hrefParts.length > 1) {
                        draggedNodeID = hrefParts[hrefParts.length - 1];
                        draggedNodeID = draggedNodeID.substring(0, draggedNodeID.length - 2);
                    } else {
                        draggedNodeID = hrefParts[0];
                        draggedNodeID = draggedNodeID.substring(draggedNodeID.indexOf("'s") + 2, draggedNodeID.length - 2)
                    }
                    <%--параметр для C# метода UpdateDB--%>
                    var parameter = { "draggedNodeID": draggedNodeID, "droppedOnNodeID": droppedOnNodeID };

                    $.ajax({
                        <%--Инициализация ajax вызова--%>
                        type: 'POST',
                        url: pageUrl,
                        data: JSON.stringify(parameter),
                        contentType: 'application/json; charset=utf-8',
                        dataType: 'json',
                        success: function (data) {
                            <%--при успешном обновлении - перезагрузка PostBack'ом--%>
                            <%= PostBackString%>
                        },
                        error: function (data, success, error) {
                            <%--При ошибке - оповещение об ошибке--%>
                            alert("Error : " + error);
                        }
                    });
                }
            });
        });
    </script>
</head>
<body style="background-color: #E1E1E1">
    <form id="form1" runat="server">
        <h1 style="font-family: 'Segoe UI'; background-color: #6699FF;">DRAG-AND-DROP ДЕРЕВО</h1>
        <div style="background-color: #E1E1E1">
                <div>
                    <asp:Button ID="expandBtn" runat="server" Text="Раскрыть все" OnClick="ExpandBtn_Click" Font-Names="Segoe UI" Height="25px" Width="100px" />
                    <asp:Button ID="collapseBtn" runat="server" Text="Свернуть все" OnClick="CollapseBtn_Click" Font-Names="Segoe UI" Height="25px" Width="100px" />
                </div>
                <br />
                <div style="width: 300px; height:auto; border: 1px solid black; background-color: #FFFFFF;">
                    <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true" />
                    <asp:TreeView ID="MyTreeView" runat="server" OnTreeNodeCollapsed="MyTreeView_TreeNodeStateChanged" OnTreeNodeExpanded="MyTreeView_TreeNodeStateChanged">
                        <NodeStyle CssClass="treeNode" Font-Names="Segoe UI" ForeColor ="black"/>
                    </asp:TreeView>
                </div>
        </div>
    </form>
</body>
</html>
