using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using MemorizeIt.DictionaryStorage;


namespace MemorizeIt.IOs.Screens
{
	public class SourceTypeController : DialogViewController
	{
		private readonly DialogViewController privateController;
		private readonly DialogViewController publicController;



		public SourceTypeController (IDictionaryStorage store) : base (UITableViewStyle.Grouped, null,true)
		{
			privateController = new PrivateGoogleUpdateController (store);
			publicController = new PublicUpdateController (store);
		}
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			Root = new RootElement ("Sources") {
				new Section (""){ 
					new RootElement("Public",(r)=>{
						publicController.ViewWillAppear(true);
						return publicController;}),
					new RootElement("Private",(r)=>{
						privateController.ViewWillAppear(true);
						return privateController;})}

			};
		}	
	}
}
