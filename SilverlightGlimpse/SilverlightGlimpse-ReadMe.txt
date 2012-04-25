o install SilverlightGlimpse in your application you will need to:

1) Add two references:
  a)  SilverlightGlimpse.dll
  b)  System.Windows.Controls.dll (part of the Silverlight Toolkit SDK http://silverlight.codeplex.com/)

2) Include the following code to your App.xaml.cs file:

private void Application_Startup(object sender, StartupEventArgs e)
{
	try
	{
		this.RootVisual = new Views.UserView();
		SilverlightGlimpse.Services.Glimpse.Service.Load(this);
	}
	catch (Exception ex)
	{
		SilverlightGlimpse.Services.Glimpse.Service.DisplayLoadFailure(this, ex);
	}
}