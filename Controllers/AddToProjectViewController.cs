using System;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using MobilePatterns.Models;
using MonoTouch.Foundation;
using System.IO;

namespace MobilePatterns.Controllers
{
    public class AddToProjectViewController : DialogViewController
    {
        private UIImage _img;
        private static string SavePath = Environment.GetFolderPath (Environment.SpecialFolder.Personal);

        public AddToProjectViewController(UIImage img)
            : base (UITableViewStyle.Plain, null, true)
        {
            _img = img;
            Title = "Add To Project";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            //Add a new project
            NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Add, (s, e) => {
                PresentModalViewController(new UINavigationController(new NewProjectViewController(() => {
                    DismissModalViewControllerAnimated(true);
                    LoadTable();
                })), true);
            });

            LoadTable();
        }

        private void LoadTable()
        {
            var section = new Section();
            var projects = Data.Database.Main.Table<Project>();
            foreach (var p in projects)
            {
                var project = p;
                var element = new StyledStringElement(project.Name, () => Save(project))
                                  { Accessory = UITableViewCellAccessory.DisclosureIndicator };
                section.Add(element);
            }

            var root = new RootElement(Title) { section };
            Root = root;
        }

        private void Save(Project project)
        {
            //Save the image to the project
            var path = Path.Combine(SavePath, Guid.NewGuid().ToString() + ".png");
            NSError error;
            _img.AsPNG().Save(path, true, out error);
            if (error != null && error.Code != 0)
            {
                //Unable to save image
                var alert = new UIAlertView() { Title = "Error", Message = "Unable to save image. Error code: " + error.Code };
                alert.CancelButtonIndex = alert.AddButton("Ok");
                alert.Show();
                return;
            }


            var pi = new ProjectImage() { ProjectId = project.Id, Path = path };
            Data.Database.Main.Insert(pi);

            //Return to the previous controller
            NavigationController.PopViewControllerAnimated(true);
        }
    }
}

