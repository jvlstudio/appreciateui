using System;
using AppreciateUI.Data;
using System.Collections.Generic;
using System.Threading;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using AppreciateUI.Cells;
using AppreciateUI.Models;

namespace AppreciateUI.Controllers
{
    public class IconBrowserController : PatternViewController
    {
        State _state = State.Waiting;
        List<Icon> _icons;

        /// <summary>
        /// A simple state machine enumeration
        /// </summary>
        enum State
        {
            Waiting, Loading, Loaded
        }

        public IconBrowserController()
        {
            this.WantsFullScreenLayout = true;
            this.Title = "Icons";
        }

        protected override BrowserViewController CreateBrowserViewController()
        {
            return new WebBrowserViewController(_loadedImages);
        }

        protected override int OnGetItemsInCollection ()
        {
            if (_icons == null)
                return 0;
            return _icons.Count;
        }

        protected override float HeightForView(int index)
        {
            return CollectionView.ColWidth;
        }

        protected override void OnAssignObject (Cell view, int index)
        {
            view.PrepareForUse(Cell.CellType.Icon);
            if (index < _icons.Count)
                view.FillViewWithObject(Utils.Util.DownloadIconUrl(_icons[index].Url, _icons[index].Ext));
        }

        List<Photo> _loadedImages = new List<Photo>();

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            LoadImages();
        }

        private void LoadImages()
        {
            if (_state != State.Waiting)
                return;

            _state = State.Loading;
            var hud = new RedPlum.MBProgressHUD(View.Frame)
            {
                Mode = RedPlum.MBProgressHUDMode.Indeterminate, 
                TitleText = "Loading...", 
                TitleFont = UIFont.BoldSystemFontOfSize(14f)
            };

            this.View.AddSubview(hud);
            hud.Show(false);

            //Do the loading
            ThreadPool.QueueUserWorkItem(delegate {
                try
                {
                    _icons = RequestFactory.GetIcons();
                    _loadedImages = new List<Photo>();
                    _icons.ForEach(x => {
                        var photo = new Photo(new NSUrl(x.FullUrl)) { Caption = x.App, Icon = true, CornerRadius = 512f * (10f / 57f) };
                        _loadedImages.Add(photo);
                    });

                    BeginInvokeOnMainThread(() => { 
                        _state = State.Loaded;
                        hud.Hide(true);
                        hud.RemoveFromSuperview();
                        CollectionView.ReloadData();
                    });
                }
                catch (Exception e)
                {
                    BeginInvokeOnMainThread(() =>  {
                        _state = State.Waiting;
                        hud.Hide(true);
                        hud.RemoveFromSuperview();
                        var alert = new UIAlertView {Message = e.Message, Title = "Error"};
                        alert.CancelButtonIndex = alert.AddButton("Ok");
                        alert.Show();
                    });
                }
            });
        }
    }
}

