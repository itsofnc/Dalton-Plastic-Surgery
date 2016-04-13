<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="get_list.aspx.vb" Inherits="DPSResponsive.get_list" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">


<%--<table id="tblList" border="0" cellpadding="2" cellspacing="5" class="table">
    <asp:Repeater ID="rprList" runat="server">
        <ItemTemplate>
            <tr class="cls_dataRow" id="<%# Eval("recid")%>">
                <td class="cls_image_ref"><img src=<%# Eval("image_ref")%> class="shadow img-responsive"/></td>
                <td class="cls_bio" valign="top"><span id="Display_data"><strong><%# Eval("display_name")%></strong><br /><%# Eval("content")%></span></td>
            </tr>
        </ItemTemplate>
    </asp:Repeater>
</table>--%>

<h1 class="title_data"><asp:literal id="litHeader" runat="server"></asp:literal></h1>
<hr />                
<asp:literal id="lit2ColList" runat="server"></asp:literal>
            
