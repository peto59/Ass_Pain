using System;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using System.Collections.Generic;
using System.Linq;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using MWP.BackEnd;
using FragmentManager = AndroidX.Fragment.App.FragmentManager;
using FragmentTransaction = AndroidX.Fragment.App.FragmentTransaction;
using Orientation = Android.Widget.Orientation;
#if DEBUG
using MWP.Helpers;
#endif

namespace MWP
{
    public class UIRenderFunctions
    {
        private static List<string> _selectedPlaylists = new List<string>();

        public enum SongType
        {
            AlbumSong,
            AllSong,
            PlaylistSong
        }

        /// <summary>
        /// General object, that will take on different forms, based on where its currently being defined.
        /// E.g. if album fragment is currently active, than the object of that active album you are browsing will be
        /// used to define this global position parameter
        /// </summary>
        public static object FragmentPositionObject;
        
        
        private static void AreYouSure(
            object sender, EventArgs e, Song song, AlertDialog? di, 
            LinearLayout linFromDelete, LinearLayout linForDelete, 
            Context context, SongsFragment songsFragmentContext)
        {
            LayoutInflater? ifl = LayoutInflater.From(context);
            View? view = ifl?.Inflate(Resource.Layout.are_you_sure_popup, null);
            AlertDialog.Builder alert = new AlertDialog.Builder(context);
            alert.SetView(view);

            AlertDialog? dialog = alert.Create();

            TextView? txt = view?.FindViewById<TextView>(Resource.Id.are_you_sure_text);
            txt?.SetTextColor(Color.White);
            if (txt != null) txt.Text = "Deleting: " + song.Title;

            Button? yes = view?.FindViewById<Button>(Resource.Id.yes_daddy);
            if (BlendMode.Multiply != null)
                yes?.Background?.SetColorFilter(
                    new BlendModeColorFilter(Color.Rgb(255, 76, 41), BlendMode.Multiply)
                );
            yes?.SetTextColor(Color.Black);

            if (yes != null)
                yes.Click += delegate
                {
                    song.Delete();
                    dialog?.Hide();
                    di?.Hide();
                    songsFragmentContext.InvalidateCache();

                    linFromDelete.RemoveView(linForDelete);
                    Toast.MakeText(context, $"{song.Title} has been deleted", ToastLength.Short)?.Show();
                };

            Button? no = view?.FindViewById<Button>(Resource.Id.you_are_not_sure);
            if (BlendMode.Multiply != null)
                no?.Background?.SetColorFilter(
                    new BlendModeColorFilter(Color.Rgb(255, 76, 41), BlendMode.Multiply)
                );
            no?.SetTextColor(Color.Black);

            if (no != null)
                no.Click += (_, _) => { di?.Hide(); };

            dialog?.Show();
        }

        private static void ListPlaylistsPopup(Song song, Context context, float scale)
        {
            LayoutInflater? ifl = LayoutInflater.From(context);
            View? view = ifl?.Inflate(Resource.Layout.list_plas_popup, null);
            AlertDialog.Builder alert = new AlertDialog.Builder(context);
            alert.SetView(view);

            AlertDialog? dialog = alert.Create();

            LinearLayout? ln = view?.FindViewById<LinearLayout>(Resource.Id.playlists_list_la);

            List<string> playlists = FileManager.GetPlaylist();
            foreach (string p in playlists)
            {
                
                LinearLayout lnIn = new LinearLayout(context);
                lnIn.Orientation = Orientation.Vertical;
                lnIn.SetBackgroundResource(Resource.Drawable.rounded_light);

                LinearLayout.LayoutParams lnInParams = new LinearLayout.LayoutParams(
                    ViewGroup.LayoutParams.MatchParent,
                    (int)(50 * scale + 0.5f)
                );
                lnInParams.SetMargins(20, 20, 20, 20);
                lnIn.LayoutParameters = lnInParams;
                lnIn.SetGravity(GravityFlags.Center);

                lnIn.Click += (_, _) =>
                {
                    if (_selectedPlaylists.Contains(p))
                    {
                        _selectedPlaylists.Remove(p);
                        lnIn.SetBackgroundResource(Resource.Drawable.rounded_light);
                    }
                    else
                    {
                        _selectedPlaylists.Add(p);
                        lnIn.SetBackgroundResource(Resource.Drawable.rounded_dark);
                    }
                };
                
                TextView name = new TextView(context);
                name.TextAlignment = TextAlignment.Center;
                name.SetTextColor(Color.White);
                name.Text = p;
                lnIn.AddView(name);
                
                ln?.AddView(lnIn);
            }
            
            
            Button? submit = view?.FindViewById<Button>(Resource.Id.submit_plas);
            if (BlendMode.Multiply != null)
                submit?.Background?.SetColorFilter(
                    new BlendModeColorFilter(Color.Rgb(255, 76, 41), BlendMode.Multiply)
                );
            submit?.SetTextColor(Color.Black);

            if (submit != null)
                submit.Click += (_, _) =>
                {
                    foreach (string s in _selectedPlaylists)
                    {
                        List<Song> plaSongs = FileManager.GetPlaylist(s);
                        if (plaSongs.Any(a => a.Equals(song)))
                            Toast.MakeText(context, "already exists in : " + s, ToastLength.Short)?.Show();
                        else
                        {
                            FileManager.AddToPlaylist(s, song.Path);

                            Toast.MakeText(
                                    context, "added successfully",
                                    ToastLength.Short
                                )
                                ?.Show();
                        }
                    }


                    dialog?.Hide();
                    _selectedPlaylists.Clear();
                };

            Button? cancel = view?.FindViewById<Button>(Resource.Id.cancel_plas);
            if (BlendMode.Multiply != null)
                cancel?.Background?.SetColorFilter(
                    new BlendModeColorFilter(Color.Rgb(255, 76, 41), BlendMode.Multiply)
                );
            cancel?.SetTextColor(Color.Black);

            if (cancel != null)
                cancel.Click += (_, _) => { dialog?.Hide(); };


            dialog?.Show();
            
            
            
        }

        private static void ShowPopupSongEdit(
            MusicBaseClass path, LinearLayout linFromDelete, LinearLayout linForDelete, Context context, float scale, 
            AssetManager? assets, FragmentManager manager, SongsFragment? songsFragmentContext = null
        )
        {
            LayoutInflater? ifl = LayoutInflater.From(context);
            View? view = ifl?.Inflate(path is Song ? Resource.Layout.edit_song_popup : Resource.Layout.edit_album_popup, null);

            AlertDialog.Builder alert = new AlertDialog.Builder(context);
            alert.SetView(view);
            
            Typeface? font = Typeface.CreateFromAsset(assets, "sulphur.ttf");
            if (context.Resources is { DisplayMetrics: not null }) scale = context.Resources.DisplayMetrics.Density;

            AlertDialog? dialog = alert.Create();
            
            if (path is Song song)
            {
                TextView? addToPla = view?.FindViewById<TextView>(Resource.Id.add_to_pla);
                if (addToPla != null)
                {
                    addToPla.SetTextColor(Color.White);
                    addToPla.Typeface = font;
                    if (BlendMode.Multiply != null)
                        addToPla?.Background?.SetColorFilter(
                            new BlendModeColorFilter(Color.Rgb(255, 76, 41), BlendMode.Multiply)
                        );
                    if (addToPla != null)
                        addToPla.Click += (_, _) =>
                        {
                            dialog?.Hide();
                            ListPlaylistsPopup(song, context, scale);
                        };
                }

                
                
                
                TextView? addToQu = view?.FindViewById<TextView>(Resource.Id.add_to_qu);
                TextView? delete = view?.FindViewById<TextView>(Resource.Id.delete);
                TextView? edit = view?.FindViewById<TextView>(Resource.Id.editSong_tags);
                if (addToQu != null && delete != null && edit != null)
                {
                    addToQu.Typeface = font;
                    delete.Typeface = font;
                    edit.Typeface = font;
                    
                    if (BlendMode.Multiply != null)
                    {
                        addToQu?.Background?.SetColorFilter(
                            new BlendModeColorFilter(Color.Rgb(255, 76, 41), BlendMode.Multiply)
                        );
                        delete?.Background?.SetColorFilter(
                            new BlendModeColorFilter(Color.Rgb(255, 76, 41), BlendMode.Multiply)
                        );
                        edit?.Background?.SetColorFilter(
                            new BlendModeColorFilter(Color.Rgb(255, 76, 41), BlendMode.Multiply)
                        );
                    }

                    if (addToQu != null)
                        addToQu.Click += (_, _) =>
                        {
                            MainActivity.ServiceConnection?.Binder?.Service?.AddToQueue(song);
                            dialog?.Hide();
                        };

                    if (delete != null)
                        delete.Click += (o, args) =>
                        {
                            if (songsFragmentContext != null)
                                AreYouSure(o, args, song, dialog, linFromDelete, linForDelete, context,
                                    songsFragmentContext);
                        };

                    if (edit != null)
                        edit.Click += (_, _) =>
                        {
                            dialog?.Hide();

                            
                            TagManagerFragment tagFrag = new TagManagerFragment(context, assets, path);
                            FragmentTransaction fragmentTransaction = manager.BeginTransaction();
                            fragmentTransaction.Replace(Resource.Id.MainFragmentLayoutDynamic, tagFrag);
                            fragmentTransaction.AddToBackStack(null);
                            fragmentTransaction.Commit();
                        };
                }
            }
            
            dialog?.Window?.SetBackgroundDrawable(new ColorDrawable(Color.Transparent));
            dialog?.Show();
        }


        /// <summary>
        /// Horizontal order of elements within single element in list of songs or albums (only songs, bcs only albums have vertical alignment of cover and title)
        /// </summary>
        /// <param name="musics"></param>
        /// <param name="scale"></param>
        /// <param name="ww"></param>
        /// <param name="hh"></param>
        /// <param name="btnMargins"></param>
        /// <param name="nameMargins"></param>
        /// <param name="cardMargins"></param>
        /// <param name="nameSize"></param>
        /// <param name="index"></param>
        /// <param name="context"></param>
        /// <param name="songButtons"></param>
        /// <param name="songType"></param>
        /// <param name="assets"></param>
        /// <param name="linForDelete"></param>
        /// <returns></returns>
        public static LinearLayout PopulateHorizontal(
            MusicBaseClass musics, float scale, int ww, int hh, int[] btnMargins, int[] nameMargins, int[] cardMargins, int nameSize,
            Context context, Dictionary<LinearLayout, Guid> songButtons, SongType songType, AssetManager? assets, FragmentManager manager,
            LinearLayout? linForDelete = null, SongsFragment? songsfragmentContext = null 
        )
        {
            //リネアルレーアート作る
            LinearLayout lnIn = new LinearLayout(context);
            lnIn.Orientation = Orientation.Horizontal;
            lnIn.SetBackgroundResource(Resource.Drawable.rounded_primaryColor);

            LinearLayout.LayoutParams lnInParams = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.WrapContent
            );
            lnInParams.SetMargins(cardMargins[0], cardMargins[1], cardMargins[2], cardMargins[3]);
            lnIn.LayoutParameters = lnInParams;
            
          
            lnIn.Click += (sender, _) =>
            {
                LinearLayout pressedButton = (LinearLayout)sender;
                foreach (KeyValuePair<LinearLayout, Guid> pr in songButtons.Where(pr => pr.Key == pressedButton))
                {
#if DEBUG
                    MyConsole.WriteLine("UI Render functions, line 282, testing pr value : " + pr.Value);
#endif
                    switch (songType)
                    {
                        case SongType.AllSong:
                            MainActivity.ServiceConnection?.Binder?.Service?.GenerateQueue(StateHandler.Songs, pr.Value);
                            break;
                        case SongType.AlbumSong:
                            MainActivity.ServiceConnection?.Binder?.Service?.GenerateQueue((Album)FragmentPositionObject, pr.Value);
                            break;
                        case SongType.PlaylistSong:
                            MainActivity.ServiceConnection?.Binder?.Service?.GenerateQueue((List<Song>)FragmentPositionObject, pr.Value);
                            break;
                    }
                }
            };
            lnIn.LongClick += (_, _) =>
            {
                if (linForDelete != null)
                    ShowPopupSongEdit(musics, linForDelete, lnIn, context, scale, assets, manager,
                        songsfragmentContext);
            };

            songButtons.Add(lnIn, musics.Id);
            
            lnIn.SetHorizontalGravity(GravityFlags.Center);
            return lnIn;
        }


        /// <summary>
        /// Vertical order of elements within single element in list of songs or albums (only albums, bcs only albums have vertical alignment of cover and title) 
        /// </summary>
        /// <param name="musics"></param>
        /// <param name="scale"></param>
        /// <param name="cardMargins"></param>
        /// <param name="nameSize"></param>
        /// <param name="index"></param>
        /// <param name="context"></param>
        /// <param name="albumButtons"></param>
        /// <param name="manager"></param>
        /// <param name="assets"></param>
        /// <param name="linForDelete"></param>
        /// <param name="albumFragment"></param>
        /// <param name="authorFragment"></param>
        /// <returns></returns>
        public static LinearLayout PopulateVertical(
            MusicBaseClass musics, float scale, int[] cardMargins, int nameSize, int index,
            Context context, Dictionary<LinearLayout, object> albumButtons, 
            FragmentManager manager, AssetManager? assets,
            AlbumFragment? albumFragment = null, AuthorFragment? authorFragment = null,
            LinearLayout? linForDelete = null, SongsFragment? songsfragmentContext = null 
        )
        {
            //リネアルレーアート作る
            LinearLayout lnIn = new LinearLayout(context);
            lnIn.Orientation = Orientation.Vertical;
            lnIn.SetBackgroundResource(Resource.Drawable.rounded_primaryColor);

            LinearLayout.LayoutParams lnInParams = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.WrapContent
            );
            lnInParams.SetMargins(cardMargins[0], cardMargins[1], cardMargins[2], cardMargins[3]);
            lnIn.LayoutParameters = lnInParams;


            // ボッタン作って
            if (musics is Album album)
            {
                lnIn.Click += (sender, _) =>
                {
                    LinearLayout pressedButton = (LinearLayout)sender;
                    foreach(KeyValuePair<LinearLayout, object> pr in albumButtons)
                    {
                        if (pr.Key == pressedButton && pr.Value is Album album1)
                        {
                            // ((AllSongsFragment)activity).ReplaceFragments(AllSongsFragment.FragmentType.AlbumFrag, album1.Title);
                            // AllSongsFragment.GetInstance().ReplaceFragments(AllSongsFragment.FragmentType.AlbumFrag, album1.Title);
                            FragmentTransaction fragmentTransaction = manager.BeginTransaction();
                            Bundle bundle = new Bundle();
                            bundle.PutString("title", album1.Title);

                            if (albumFragment != null)
                            {
                                albumFragment.Arguments = bundle;
                                fragmentTransaction.Replace(Resource.Id.MainFragmentLayoutDynamic, albumFragment);
                            }

                            fragmentTransaction.AddToBackStack(null);
                            fragmentTransaction.Commit();
                        
                            break;
                        }
                    }
                    
                };

                
                lnIn.LongClick += (_, _) =>
                {
                    if (linForDelete != null)
                        ShowPopupSongEdit(album, linForDelete, lnIn, context, scale, assets, manager,
                            songsfragmentContext);
                };
                

                albumButtons.Add(lnIn, album);
            }
            else if (musics is Artist artist)
            {
                lnIn.Click += (sender, _) =>
                {
                    LinearLayout pressedButton = (LinearLayout)sender;

                    foreach (KeyValuePair<LinearLayout, object> pr in albumButtons)
                    {
                        if (pr.Key == pressedButton && pr.Value is Artist artist1)
                        {
                            // ((AllSongsFragment)activity).ReplaceFragments(AllSongsFragment.FragmentType.AuthorFrag, artist1.Title);
                            // AllSongsFragment.GetInstance().ReplaceFragments(AllSongsFragment.FragmentType.AuthorFrag, artist1.Title);
                            FragmentTransaction fragmentTransaction = manager.BeginTransaction();
                            Bundle bundle = new Bundle();
                            bundle.PutString("title", artist1.Title);


                            if (authorFragment != null)
                            {
                                authorFragment.Arguments = bundle;
                                fragmentTransaction.Replace(Resource.Id.MainFragmentLayoutDynamic, authorFragment);
                            }

                            fragmentTransaction.AddToBackStack(null);
                            fragmentTransaction.Commit();
                            
                            break;
                        }
                    }
                };

                
                lnIn.LongClick += (_, _) =>
                {
                    if (linForDelete != null)
                        ShowPopupSongEdit(artist, linForDelete, lnIn, context, scale, assets, manager,
                            songsfragmentContext);
                };
                

                albumButtons.Add(lnIn, artist);
            }

            lnIn.SetHorizontalGravity(GravityFlags.Center);
            return lnIn;
        }
        
        
        
        
        
        
        
        /// <summary>
        /// Function used for applying an image to a view based on the source, which can be either album cover or song cover. 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="obj"></param>
        /// <param name="ww"></param>
        /// <param name="hh"></param>
        /// <param name="btnMargins"></param>
        /// <param name="nameSize"></param>
        /// <param name="nameMargins"></param>
        /// <param name="scale"></param>
        /// <param name="context"></param>
        public static void SetTilesImage(LinearLayout parent, MusicBaseClass obj, int ww, int hh, int[] btnMargins, int nameSize, int[] nameMargins, float scale, Context context)
        {
            ImageView mori = new ImageView(context);
            LinearLayout.LayoutParams ll = new LinearLayout.LayoutParams(
                (int)(ww * scale + 0.5f), (int)(hh * scale + 0.5f)
            );
            ll.SetMargins(btnMargins[0], btnMargins[1], btnMargins[2], btnMargins[3]);
            mori.LayoutParameters = ll;

            if (obj is not (Album or Artist or Song))
            {
                return;
            }

            mori.SetImageBitmap(
                obj.Image
            );
            
            
            

            parent.AddView(mori);

            //アルブムの名前
            TextView name = new TextView(context);
            name.Text = obj.Title;
            name.TextSize = nameSize;
            name.SetTextColor(Color.White);
            name.TextAlignment = TextAlignment.Center;
            name.SetForegroundGravity(GravityFlags.Center);

            LinearLayout.LayoutParams lnNameParams = new LinearLayout.LayoutParams(
                (int)(130 * scale + 0.5f),
                ViewGroup.LayoutParams.WrapContent
            );
            lnNameParams.SetMargins(nameMargins[0], nameMargins[1], nameMargins[2], nameMargins[3]);

            name.LayoutParameters = lnNameParams;

            parent.SetGravity(GravityFlags.Center);
            parent.SetHorizontalGravity(GravityFlags.Center);
            parent.AddView(name);
            

        }
    }
}