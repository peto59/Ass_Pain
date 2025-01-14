using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android.Animation;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using Android.Widget;
using MWP.BackEnd;
using Google.Android.Material.FloatingActionButton;
using Xamarin.Essentials;
using Fragment = AndroidX.Fragment.App.Fragment;
using IOException = Java.IO.IOException;
#if DEBUG
using MWP.Helpers;
#endif

namespace MWP
{
    /// <inheritdoc />
    public class TagManagerFragment : Fragment
    {
        private const int ActionScrollViewHeight = 200;
        private float scale;
        private readonly Context context;
        private RelativeLayout? mainLayout;
        private Typeface? font;
        private AssetManager? assets;
        private MusicBaseClass? song;

        private ImageView? songCover;
        private Bitmap? newImage;
        
        private EditText? titleIn;
        private EditText? albumIn;
        private EditText? authorIn;
        private EditText? alauIn;
        
        
        private FloatingActionButton? saveChanges;
        private TextView? backButton;

        private TagManager tagManager;
        
        internal enum FieldTypes
        {
            Title,
            Alau,
            Author,
            Album
        }

        
        /// <inheritdoc cref="context"/>
        public TagManagerFragment(Context context, AssetManager? assets, MusicBaseClass? song)
        {
            this.song = song;
            this.newImage = null;
            this.context = context;
            this.assets = assets;
            font = Typeface.CreateFromAsset(assets, "sulphur.ttf");
            if (context.Resources is { DisplayMetrics: not null }) scale = context.Resources.DisplayMetrics.Density;

            if (song is not Song song1) return;
            
            tagManager = new TagManager(song1);
        }
        
        async Task<FileResult?> PickAndShow(PickOptions options, ImageView view)
        {
            try
            {
                
                var result = await FilePicker.PickAsync(options);
                tagManager = new TagManager((Song)song);
                if (result != null)
                {
                    string text = $"File Name: {result.FileName}";
                    if (result.FileName.EndsWith("jpg", StringComparison.OrdinalIgnoreCase) ||
                        result.FileName.EndsWith("png", StringComparison.OrdinalIgnoreCase))
                    {
                        var stream = await result.OpenReadAsync();
                        newImage = BitmapFactory.DecodeStream(stream);
                        HandleSaveButtonAppearImage(view, newImage);
                        view.SetImageBitmap(newImage);
                    }
                }
        
                return result;
            }
            catch (Exception ex)
            {
                // The user canceled or something went wrong
            }
    
            return null;
        }


    

        /// <inheritdoc />
        public override View? OnCreateView(LayoutInflater inflater, ViewGroup? container, Bundle? savedInstanceState)
        {
            View? view = inflater.Inflate(Resource.Layout.tag_manager_fragment, container, false);

            mainLayout = view?.FindViewById<RelativeLayout>(Resource.Id.tag_manager_main);

            if (Activity != null) ((MainActivity)Activity).Title = "Tag Manager";

            /*
             * changing fonts
             */
            SetGenericFont<TextView>(view, Resource.Id.tagmngr_album_label);
            // SetGenericFont<TextView>(view, Resource.Id.tagmngr_aual_label);
            SetGenericFont<TextView>(view, Resource.Id.tagmngr_author_label);
            SetGenericFont<TextView>(view, Resource.Id.tagmngr_title_label);
            
            // SetGenericFont<TextView>(view, Resource.Id.tagmngr_alau_field);
            SetGenericFont<TextView>(view, Resource.Id.tagmngr_album_field);
            SetGenericFont<TextView>(view, Resource.Id.tagmngr_author_field);
            SetGenericFont<TextView>(view, Resource.Id.tagmngr_title_field);

            SetGenericFont<TextView>(view, Resource.Id.tagmngr_back_button);
            
            /*
             * Back Button handle
             */
            backButton = view?.FindViewById<TextView>(Resource.Id.tagmngr_back_button);
            if (backButton != null)
                backButton.Click += (sender, args) =>
                {
                    if (tagManager.Changed)
                    {
                        AreYouSureDiscard();
                    }
                    else
                    {
                        ParentFragmentManager.PopBackStack();
                    }
                };

            /*
             * load image
             */ 
            songCover = view?.FindViewById<ImageView>(Resource.Id.song_cover_tag_manager);
            songCover?.SetImageBitmap(song?.Image);
            try
            {
                Stream? stream = assets?.Open("music_placeholder.png");  
                Bitmap? imgBitmap = BitmapFactory.DecodeStream(stream);  
            }
            catch (IOException)
            {
                return view;
            }

            var customFileType =
                new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.Android, new[] { "image/jpeg", "image/png", } },
                });
            var options = new PickOptions
            {
                PickerTitle = "Please select a image",
                FileTypes = customFileType,
            };
            
            if (songCover != null)
                songCover.Click += async delegate
                {
                    var res = await PickAndShow(options, songCover);
#if DEBUG
                    MyConsole.WriteLine($"Loading image result: {res?.FileName}");
#endif
                };


            /*
             * Save Button
             */
            saveChanges = mainLayout?.FindViewById<FloatingActionButton>(Resource.Id.tag_manager_savebtn);
            if (BlendMode.Multiply != null)
                saveChanges?.Background?.SetColorFilter(
                    new BlendModeColorFilter(Color.Rgb(255, 76, 41), BlendMode.Multiply)
                );
            if (saveChanges != null)
            {
                saveChanges.Click += delegate { AreYouSureSave(); };
                saveChanges.Visibility = ViewStates.Gone;
                

            }
            

            // if (createPlaylist != null) createPlaylist.Click += CreatePlaylistPopup;
            
            
            
            /*
             * Inputs, handle on change, appearance of save button
             */
            titleIn = view?.FindViewById<EditText>(Resource.Id.tagmngr_title_field);
            albumIn = view?.FindViewById<EditText>(Resource.Id.tagmngr_album_field);
            authorIn = view?.FindViewById<EditText>(Resource.Id.tagmngr_author_field);
            // alauIn = view?.FindViewById<EditText>(Resource.Id.tagmngr_alau_field);
            
            if (view != null)
            {
                HandleSaveButtonAppear(titleIn, tagManager.OriginalTitle, FieldTypes.Title);
                //HandleSaveButtonAppear(alauIn, initialAlauIn, FieldTypes.Alau);
                HandleSaveButtonAppear(authorIn, tagManager.OriginalArtist, FieldTypes.Author);
                HandleSaveButtonAppear(albumIn, tagManager.OriginalAlbum, FieldTypes.Album);
            }

            if (titleIn != null) titleIn.Text = tagManager.OriginalTitle;
            if (albumIn != null) albumIn.Text = tagManager.OriginalAlbum;
            if (authorIn != null) authorIn.Text = tagManager.OriginalArtist;
            //if (alauIn != null) alauIn.Text = initialAlauIn;

            return view;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            tagManager.Dispose();
        }

        public override void OnDestroyView()
        {
            base.OnDestroyView();
            tagManager.Dispose();
        }

        public override void OnStop()
        {
            base.OnStop();
            tagManager.Dispose();
        }


        private void HandleSaveButtonAppear(EditText? input, string initial, FieldTypes type)
        {
            if (input != null)
                input.TextChanged += (_, _) =>
                {
                    if (initial?.Equals(input.Text) == false && saveChanges != null)
                        saveChanges.Visibility = ViewStates.Visible;
                    if (initial?.Equals(input.Text) == true && saveChanges != null)
                        saveChanges.Visibility = ViewStates.Gone;
                    
                    switch (type)
                    {
                        case FieldTypes.Title:
                            tagManager.Title = titleIn?.Text ?? tagManager.Title;
                            break;
                        case FieldTypes.Alau:
                            break;
                        case FieldTypes.Author:
                            tagManager.Artist = authorIn?.Text ?? tagManager.Artist;
                            break;
                        case FieldTypes.Album:
                            if(albumIn?.Text == "No Album")
                                tagManager.NoAlbum();
                            else
                                tagManager.Album = albumIn?.Text ?? tagManager.Album;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(type), type, null);
                    }
                };
        }

        private void HandleSaveButtonAppearImage(ImageView view, Bitmap? newIm)
        {
            if (((BitmapDrawable)view.Drawable)?.Bitmap != newIm && saveChanges != null)
                saveChanges.Visibility = ViewStates.Visible;
            if (((BitmapDrawable)view.Drawable)?.Bitmap == newIm && saveChanges != null)
                saveChanges.Visibility = ViewStates.Gone;
            
            if (newIm != null)
                tagManager.Image = newIm;
        }
        
        private void SetGenericFont<T>(View? view, int id)
        {
            if (view?.FindViewById(id) is TextView label) label.Typeface = font;
            if (view?.FindViewById(id) is EditText input) input.Typeface = font;
        }

        private void SaveChanges()
        {
#if DEBUG
            MyConsole.WriteLine("Saving changes");
#endif
            tagManager.Save();
            tagManager.Dispose();
        }

        private void AreYouSureSave()
        {
            LayoutInflater? ifl = LayoutInflater.From(context);
            View? popupView = ifl?.Inflate(Resource.Layout.share_are_you_sure, null);
            AlertDialog.Builder alert = new AlertDialog.Builder(context);
            alert.SetView(popupView);

            TextView? title = popupView?.FindViewById<TextView>(Resource.Id.share_are_you_sure_title);
            TextView? yes = popupView?.FindViewById<TextView>(Resource.Id.share_are_you_sure_yes);
            TextView? no = popupView?.FindViewById<TextView>(Resource.Id.share_are_you_sure_no);

            if (title != null) title.Typeface = font;
            if (yes != null) yes.Typeface = font;
            if (no != null) no.Typeface = font;

            AlertDialog? dialog = alert.Create();
            dialog?.Window?.SetBackgroundDrawable(new ColorDrawable(Color.Transparent));

            if (title != null) title.Text = "Do you want to write changes?";
            if (yes != null)
            {
                yes.Click += (_, _) =>
                {   
                    SaveChanges();
                    dialog?.Cancel();
                    ParentFragmentManager.PopBackStack();
                };
            }
            
            if (no != null) no.Click += (_, _) => dialog?.Cancel();
            dialog?.Show();
        }
        
        private void AreYouSureDiscard()
        {
            LayoutInflater? ifl = LayoutInflater.From(context);
            View? popupView = ifl?.Inflate(Resource.Layout.share_are_you_sure, null);
            AlertDialog.Builder alert = new AlertDialog.Builder(context);
            alert.SetView(popupView);

            TextView? title = popupView?.FindViewById<TextView>(Resource.Id.share_are_you_sure_title);
            TextView? yes = popupView?.FindViewById<TextView>(Resource.Id.share_are_you_sure_yes);
            TextView? no = popupView?.FindViewById<TextView>(Resource.Id.share_are_you_sure_no);

            if (title != null) title.Typeface = font;
            if (yes != null) yes.Typeface = font;
            if (no != null) no.Typeface = font;

            AlertDialog? dialog = alert.Create();
            dialog?.Window?.SetBackgroundDrawable(new ColorDrawable(Color.Transparent));

            if (title != null) title.Text = "You have changes pending. Discard?";
            if (yes != null)
            {
                yes.Click += (_, _) =>
                {   
                    dialog?.Cancel();
                    ParentFragmentManager.PopBackStack();
                };
            }
            
            if (no != null) no.Click += (_, _) => dialog?.Cancel();
            dialog?.Show();
        }


        ~TagManagerFragment()
        {
            tagManager.Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            tagManager.Dispose();
        }
    }

   
        
}