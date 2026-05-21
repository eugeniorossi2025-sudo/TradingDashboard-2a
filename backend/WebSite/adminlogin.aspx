<%@ Page Language="VB" AutoEventWireup="false" CodeFile="adminlogin.aspx.vb" Inherits="_login" MaintainScrollPositionOnPostback="true" %>

<!DOCTYPE html>
<html lang="it">

<head runat="server" id="Head1">
    <meta http-equiv="Content-Type" content="text/html; charset=UTF-8">
    <meta http-equiv="x-ua-compatible" content="ie=edge">
    <meta name="robots" content="noindex, nofollow">
    <title>EuGenio&reg;</title>
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no"> 
    <meta name="keywords" content="">
    <script src="js/es6-promise.min.js"></script>
    <script src="js/es6-promise.auto.min.js"></script>
    <link rel="shortcut icon" href="favicon.ico" />
    <link href="https://fonts.googleapis.com/css2?family=Lato:ital,wght@0,100;0,300;0,400;0,700;0,900;1,100;1,300;1,400;1,700;1,900&display=swap" rel="stylesheet">

    <link rel="stylesheet" href="css/styles.css.css">
    <link href="/Admin/css/custom.css" rel="stylesheet">
    <link href="/Admin/css/custom_2024.css" rel="stylesheet">
    <script src="js/lazysizes_min.js" async=""></script>
    
</head>

<body class="theme-dark-blue" style="background: #fff;" data-aos-easing="ease" data-aos-duration="400" data-aos-delay="0">


    <div class="c-body-area w-100">
        <div class="vw-100">
            
            <div class="container-fluid content-module">
                <form class="admin-login" runat="server">
                            <br />
                            <img src="/img/logo.png"  alt="image"  >
                            <br />
                            <div class="title-login">Enter your credentials to log in.</div>
                            <asp:TextBox runat="server" ID="TxtUsername" CssClass="form-control"  placeholder="username"  Width="300"></asp:TextBox>
                            <br />
                            <asp:TextBox runat="server" ID="TxtPassword" TextMode="Password" placeholder="password" CssClass="form-control" Width="300"></asp:TextBox>
                            <br />
                            <asp:Button runat="server" ID="Btn_Login" Text="Log In" CssClass="btn btn-primary submit"  Width="300"/>
                            <span style="color:red"><br />
                            <asp:Literal runat="server" ID="Lit_Info"  Visible="false">Username or password incorrect</asp:Literal></span>
                </form>
            </div> 
        </div>
    </div>


</body>
</html>
