﻿using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Icu.Number;
using Android.OS;
using Android.Support.V4.Media.Session;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Core.App;
using System;
using System.IO;
using System.Runtime.Remoting.Contexts;
using Xamarin.Essentials;
using static Android.Renderscripts.ScriptGroup;
using AndroidApp = Android.App.Application;

namespace Ass_Pain
{
    public class Local_notification_service
    {
        private const string CHANNEL_ID = "local_notification_channel";
        private const string CHANNEL_NAME = "Notifications";
        private const string CHANNEL_DESCRIPTION = "description";

        private int notification_id = -1;
        private const string TITLE_KEY = "title";
        private const string MESSAGE_KEY = "message";

        private bool is_channel_init = false;

        private MediaSessionCompat media_session;

        private void create_notification_channel()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                return;
            }

            var channel = new NotificationChannel(CHANNEL_ID, CHANNEL_NAME, NotificationImportance.Default)
            {
                Description = CHANNEL_DESCRIPTION
            };

            NotificationManager manager = (NotificationManager)AndroidApp.Context.GetSystemService(AndroidApp.NotificationService);
            manager.CreateNotificationChannel(channel);
        }


        public void push_notification()
        {


            if (!is_channel_init)
            {
                create_notification_channel();
            }

            Intent intent = new Intent(AndroidApp.Context, typeof(MainActivity));
            intent.PutExtra(TITLE_KEY, "title");
            intent.PutExtra(MESSAGE_KEY, "message");
            intent.AddFlags(ActivityFlags.ClearTop);

            notification_id++;

            PendingIntent pending = PendingIntent.GetActivity(AndroidApp.Context, notification_id, intent, PendingIntentFlags.OneShot);
            NotificationCompat.Builder notification_builder = new NotificationCompat.Builder(AndroidApp.Context, CHANNEL_ID)
                .SetSmallIcon(
                    Resource.Drawable.ic_menu_camera
                )
                .SetContentTitle("title")
                .SetContentText("message")
                .SetAutoCancel(true)
                .SetContentIntent(pending)
                .SetDefaults((int)NotificationDefaults.Sound | (int)NotificationDefaults.Vibrate);


            NotificationManagerCompat manager = NotificationManagerCompat.From(AndroidApp.Context);
            manager.Notify(notification_id, notification_builder.Build());

        }

        private Bitmap get_current_song_image()
        {
            Bitmap image = null;
            TagLib.File tagFile;

            try
            {
                Console.WriteLine($"now playing: {MainActivity.player.NowPlaying()}");
                tagFile = TagLib.File.Create(
                    MainActivity.player.NowPlaying()
                );
                MemoryStream ms = new MemoryStream(tagFile.Tag.Pictures[0].Data.Data);
                image = BitmapFactory.DecodeStream(ms);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine($"Doesnt contain image: {MainActivity.player.NowPlaying()}");
            }

            if (image == null)
            {
                image = BitmapFactory.DecodeStream(AndroidApp.Context.Assets.Open("music_placeholder.png"));
            }

            return image;
        }

        public void song_control_notification()
        {
            media_session = new MediaSessionCompat(AndroidApp.Context, "tag");

            if (!is_channel_init)
            {
                create_notification_channel();
            }

            RemoteViews view = new RemoteViews(AppInfo.PackageName, Resource.Layout.player_notification);

            /*
            NotificationCompat.Builder notification_builder = new NotificationCompat.Builder(AndroidApp.Context, CHANNEL_ID)
              .SetSmallIcon(
                  Resource.Drawable.ic_menu_camera
              )
              .SetCustomContentView(view)
              .SetOngoing(true)
              .SetDefaults((int)NotificationDefaults.Sound | (int)NotificationDefaults.Vibrate);
            */


            Bitmap current_song_image = get_current_song_image();
          
            NotificationCompat.Builder notification_builder = new NotificationCompat.Builder(AndroidApp.Context, CHANNEL_ID)
              .SetSmallIcon(
                  Resource.Drawable.ic_menu_camera
              )
              .SetContentTitle(FileManager.GetSongTitle(MainActivity.player.NowPlaying()))
              .SetContentText(FileManager.GetSongArtist(MainActivity.player.NowPlaying())[0])
              .SetLargeIcon(
                    current_song_image
               )
              .AddAction(Resource.Drawable.previous, "Previous", null)
              .AddAction(Resource.Drawable.play, "play", null)
              .AddAction(Resource.Drawable.next, "next", null)
              .SetStyle(new AndroidX.Media.App.NotificationCompat.MediaStyle().SetMediaSession(media_session.SessionToken))
              .SetDefaults((int)NotificationDefaults.Sound | (int)NotificationDefaults.Vibrate);

            

            NotificationManagerCompat manager = NotificationManagerCompat.From(AndroidApp.Context);
            manager.Notify(notification_id, notification_builder.Build());


        }

    }
}