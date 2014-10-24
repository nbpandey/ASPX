<%@ Page Language="C#" AutoEventWireup="true" EnableViewState="false" %>

<%@ Import Namespace="System.Data.SqlClient" %>

<script runat="server">
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            tbConn.Text = @"Data source=ServerName\Instance;Initial Catalog=DBName;Trusted_Connection=true;";
        }
    }

    protected void btnExecute_Click(object sender, EventArgs e)
    {
        if (tbPassword.Text == "*******")
        {
             this.BindGrid();
        }
        else
        {
            lblMessage.Text = "<font color=red>Invalid credentials.</font>";
        }
    }

    private void BindGrid()
    {
        string strConnString = tbConn.Text;
        using (SqlConnection con = new SqlConnection(strConnString))
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandText = tbSQL.Text;
                cmd.Connection = con;
                con.Open();
                if ( (tbSQL.Text.StartsWith("INSERT", System.StringComparison.CurrentCultureIgnoreCase)) ||
                     (tbSQL.Text.StartsWith("UPDATE", System.StringComparison.CurrentCultureIgnoreCase)) ||
                     (tbSQL.Text.StartsWith("DELETE", System.StringComparison.CurrentCultureIgnoreCase)) )
                {
                    int rowsAffected = cmd.ExecuteNonQuery();
                    lblMessage.Text = " " + rowsAffected.ToString() + " rows affected.";
                }
                else // (tbSQL.Text.IndexOf("SELECT", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    GridView1.DataSource = cmd.ExecuteReader();
                    GridView1.DataBind();
                    GridView1.Visible = true;
                    lblMessage.Text = "Command executed sucessfully";
                }
                con.Close();
            }
        }
    }
</script>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Executing T-SQL</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <p>
            <b>Connection String:</b> (See connection strings options at <a href="http://www.connectionstrings.com/sql-server/"
                target="_blank">http://www.connectionstrings.com/sql-server/</a>)
            <br />
            <asp:TextBox ID="tbConn" runat="server" Width="600px"></asp:TextBox>
            <br />
            <!-- 
            If a local .MDF file is attached to SQL Express database, you can use following string
            Data source=.\SQLEXPRESS;Initial Catalog=D:\MY DOCUMENTS\VISUAL STUDIO 2010\PROJECTS\WEBAPPLICATION2\WEBAPPLICATION2\APP_DATA\ASPNETDB.MDF; User Id=; Password=; 
            -->
            <b>Password:</b><br />
            <asp:TextBox ID="tbPassword" runat="server"></asp:TextBox><br />
            <b>SQL:</b><br />
            <asp:TextBox ID="tbSQL" runat="server" Width="600px" Height="100px" TextMode="MultiLine"></asp:TextBox><br />
            <asp:Button ID="btnExecute" runat="server" Text="Execute" OnClick="btnExecute_Click" />
        </p>
    </div>
    <asp:GridView ID="GridView1" runat="server">
    </asp:GridView>
    <asp:Label ID="lblMessage" runat="server"></asp:Label>
    </form>
</body>
</html>
