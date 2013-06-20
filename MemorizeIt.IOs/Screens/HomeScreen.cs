using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using MemorizeIt.MemoryStorage;
using MemorizeIt.MemorySourceSupplier;
using GoogleMemorySupplier;
using FileMemoryStorage;
using MemorizeIt.MemoryTrainers;
using MemorizeIt.IOs.ApplicationLayer;
using System.Threading.Tasks;
using MemorizeIt.Model;


namespace MemorizeIt.IOs.Screens {

    /// <summary>
    /// A UITableViewController that uses MonoTouch.Dialog - displays the list of Tasks
    /// </summary>
    public class HomeScreen : DialogViewController
    {
        // 


        // MonoTouch.Dialog individual TaskDetails view (uses /AL/TaskDialog.cs wrapper class)
        private BindingContext context;
        private DialogViewController detailsScreen;
        private LoadingOverlay loadingOverlay;
		private UIBarButtonItem btnTrain;
        private readonly IMemoryStorage store;
        private readonly SimpleMemoryTrainer trainer;

        public HomeScreen()
            : base(UITableViewStyle.Plain, null)
        {
            Initialize();
            this.store = new FileSystemMemoryStorage();
            this.trainer = new SimpleMemoryTrainer(this.store);
        }

        protected void Initialize()
		{
			NavigationItem.SetRightBarButtonItem (
				new UIBarButtonItem ("Upadate",

			                                 UIBarButtonItemStyle.Plain,
			                                 (sender, e) => Upload ()), false);
			this.btnTrain =
				new UIBarButtonItem ("Train",

			                      UIBarButtonItemStyle.Plain,
			                      (sender, e) => Train ());
			NavigationItem.SetLeftBarButtonItem (btnTrain, false);
		}

        protected void Upload()
        {

            var dialod = new UIAlertView("Enter credentials", "Document path", null, "Cancel", null);

            dialod.AlertViewStyle = UIAlertViewStyle.LoginAndPasswordInput;
            dialod.AddButton("Upload");

            dialod.Show();

            dialod.Clicked += (sender, e) =>
                {
                    if (e.ButtonIndex == 0)
                        return;
                    loadingOverlay = new LoadingOverlay(UIScreen.MainScreen.Bounds);
                    View.Add(loadingOverlay);
                    var supplierParams = new string[]
                        {"MemorizeIt", dialod.GetTextField(0).Text, dialod.GetTextField(1).Text};

                    Task.Factory.StartNew(() =>
                        {
                            try
                            {
                                var supplier = CreateSourceSupplier(supplierParams);
                                var data = supplier.Download();
                                this.store.Store(data);
                                this.InvokeOnMainThread(PopulateTable);
                            }
                            catch (Exception downloadException)
                            {
                                this.InvokeOnMainThread(() =>
                                                        new UIAlertView("Error", downloadException.Message, null, "OK",
                                                                        null).Show());
                            }
                            this.InvokeOnMainThread(() =>
                                                    loadingOverlay.Hide());
                        });
                };


        }

        protected void Train()
        {
            trainer.PickQuestion();
            ShowQuestion(trainer.CurrentQuestion.Question);

        }

        protected IMemorySourceSupplier CreateSourceSupplier(params string[] supplierParameters)
        {
            return new GoogleMemorySourceSupplier(supplierParameters[0], supplierParameters[1], supplierParameters[2]);
            /*return new SimpleMemorySourceSupplier (new MemoryItem[]{
                new MemoryItem("q1","a1"),
                new MemoryItem("q2","a2"),
                new MemoryItem("q3","a3")
            });*/
        }

        protected void ShowQuestion(string s)
        {

            var dialod = new UIAlertView("I have a question for you", s, null, "Skip", null);

            dialod.AlertViewStyle = UIAlertViewStyle.PlainTextInput;
            dialod.AddButton("Answer");

            dialod.Show();

            dialod.Clicked += (sender, e) =>
                {
                    if (e.ButtonIndex == 0)
                    {
                        trainer.Clear();
                        return;
                    }
                    var answer = dialod.GetTextField(0).Text;
                    var result = trainer.Validate(answer);
                    PopulateTable();

                    if (result)
                    {
                        new UIAlertView("Well Done!", string.Format("'{0}' is correct answer", answer), null, "OK", null)
                            .Show();

                    }
                    else
                    {
                        new UIAlertView("Sorry",
                                        string.Format(
                                            "'{0}' is correct answer on question '{2}'. Your answer was '{1}'", answer,
                                            trainer.CurrentQuestion.Answer, s), null, "OK", null).Show();
                    }
                    trainer.Clear();
                   
                };

        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // reload/refresh
            PopulateTable();
        }

        protected void PopulateTable()
        {
            var memories = store.Items;
            if (memories != null)
            {
                var items = new Section();
                items.Add(from t in memories
                          select
                              (Element)
                              new StringElement(string.Format("{1}({0})", t.SuccessCount, t.Values[0]), t.Values[1]));

                Root = new RootElement("Memories")
                    {
                        items
                    };
            }

			if (!trainer.IsQuestionsAvalible ()) {
				new UIAlertView ("Well Done!", "You are done with all your questions", null, "OK", null).Show ();

				btnTrain.Enabled = false;
			} else {
				btnTrain.Enabled = true;
			}
		}

        public override Source CreateSizingSource(bool unevenRows)
        {
            return new EditingSource(this);
        }

    }
}