using System;
using MemorizeIt.MemoryStorage;
using MonoTouch.UIKit;
using MemorizeIt.MemorySourceSupplier.CredentialsStorage;

namespace MemorizeIt.IOs.Screens
{
	public class PrivateGoogleUpdateController:GoogleUpdateController
	{
		private UIBarButtonItem btnLogin;

		public PrivateGoogleUpdateController (IMemoryStorage store):base(store)
		{

		}
		protected override void Initialize ()
		{
			base.Initialize ();
			btnLogin =
				new UIBarButtonItem ("",UIBarButtonItemStyle.Plain, (s,e) => Login ());
			this.NavigationItem.SetRightBarButtonItem(btnLogin,false);
			this.NavigationItem.Title="My Google Drive Memories";
		}

		protected override void PopulateSources ()
		{
			base.PopulateSources ();
			btnLogin.Title = supplier.CredentialsStorage.IsLoggedIn?"Log out":"Log in";
		}

		protected void Login(){

			if (supplier.CredentialsStorage.IsLoggedIn)
			{
				supplier.CredentialsStorage.LogOut();
				PopulateSources ();
				return;
			}


			var dialod = new UIAlertView ("Enter credentials", "", null, "Cancel", null);

			dialod.AlertViewStyle = UIAlertViewStyle.LoginAndPasswordInput;
			dialod.GetTextField (0).KeyboardType = UIKeyboardType.EmailAddress;

			dialod.AddButton ("Log in");

			dialod.Show ();

			dialod.Clicked += (sender, e) =>
			{
				if (e.ButtonIndex == 0)
					return;
				try {
					supplier.CredentialsStorage.LogIn(dialod.GetTextField(0).Text, dialod.GetTextField(1).Text);
					PopulateSources ();
				} catch (CredentialsException ex) {
					this.InvokeOnMainThread(() =>
					                        new UIAlertView("Error", ex.Message, null, "OK",
					                null).Show());
				}
			};

		}

		protected override string GetSectionTitle ()
		{
			if (supplier.CredentialsStorage.IsLoggedIn)
				return string.Format ("Memory Sources for {0}", supplier.CredentialsStorage.GetCurrentUser ().Login);
			return "Memory Sources";
		}
		protected override string GetEmptyListReasonTitle ()
		{
			if (supplier.CredentialsStorage.IsLoggedIn)
				return "Memory sources are absent";
			return "Sources are  not avalible, please log in";
		}
	}
}
