﻿using Android.App;
using Android.Content;
using Android.Media.Session;
using Android.OS;
using Android.Runtime;
using Android.Systems;
using Android.Views;
using Android.Widget;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using AndroidApp = Android.App.Application;
#if DEBUG
using MWP.Helpers;
#endif

namespace MWP
{
    internal class MediaSessionCallback : Android.Support.V4.Media.Session.MediaSessionCompat.Callback
    {
        public override void OnPlay()
        {
#if DEBUG
            MyConsole.WriteLine("OnPlay");
#endif
            MainActivity.ServiceConnection.Binder?.Service.Play();
            //OnPlayImpl();
            base.OnPlay();
        }

        public override void OnSkipToQueueItem(long id)
        {
#if DEBUG
            MyConsole.WriteLine("OnSkipToQueueItem");
#endif
            if (id != MainActivity.ServiceConnection.Binder?.Service.QueueObject.Index)
            {
                if (MainActivity.ServiceConnection.Binder?.Service.QueueObject.SetIndex(id) ?? false)
                {
                    MainActivity.ServiceConnection.Binder?.Service.Play(true);
                }
            }
            base.OnSkipToQueueItem(id);
        }

        public override void OnSeekTo(long pos)
        {
#if DEBUG
            MyConsole.WriteLine("OnSeekTo");
            MyConsole.WriteLine($"POSTION: {pos}");
#endif

            MainActivity.ServiceConnection.Binder?.Service.SeekTo((int)pos);
            //OnSeekToImpl(pos);
            base.OnSeekTo(pos);
        }

        public override void OnPlayFromMediaId(string? mediaId, Bundle? extras)
        {
            if (mediaId == null)
            {
                base.OnPlayFromMediaId(mediaId, extras);
                return;
            }
#if DEBUG
            MyConsole.WriteLine($"OnPlayFromMediaId mediaId {mediaId}");
#endif
            MediaType mediaType = (MediaType)(mediaId[0] - '0');
#if DEBUG
            MyConsole.WriteLine($"MediaType {mediaType}");

#endif
            mediaId = mediaId[1..];
#if DEBUG
            MyConsole.WriteLine($"mediaId {mediaId}");
#endif

            switch (mediaType)
            {
                case MediaType.Song:
                    if (mediaId == MyMediaBrowserService.MySongsPlayAll)
                    {
                        MainActivity.ServiceConnection.Binder?.Service.GenerateQueue(MainActivity.stateHandler.Songs);
                    }
                    else if (mediaId == MyMediaBrowserService.MySongsShuffle)
                    {
                        MainActivity.ServiceConnection.Binder?.Service.GenerateQueue(MainActivity.stateHandler.Songs, null, false);
                        MainActivity.ServiceConnection.Binder?.Service.Shuffle(true);
                        MainActivity.ServiceConnection.Binder?.Service.Play();
                    }
                    else
                    {
                        MainActivity.ServiceConnection.Binder?.Service.GenerateQueue(Song.FromId(mediaId));
                    }
                    break;
                case MediaType.ThisPlayAll:
                    MediaType mediaTypePlayAll = (MediaType)(mediaId[0] - '0');
                    mediaId = mediaId[1..];
                    if (mediaTypePlayAll == MediaType.Album)
                    {
                        MainActivity.ServiceConnection.Binder?.Service.GenerateQueue(Album.FromId(mediaId));
                    }else if (mediaTypePlayAll == MediaType.Artist)
                    {
                        MainActivity.ServiceConnection.Binder?.Service.GenerateQueue(Artist.FromId(mediaId));
                    }
                    break;
                case MediaType.ThisShufflePlay:
                    MediaType mediaTypeShuffle = (MediaType)(mediaId[0] - '0');
                    mediaId = mediaId[1..];
                    if (mediaTypeShuffle == MediaType.Album)
                    {
                        MainActivity.ServiceConnection.Binder?.Service.GenerateQueue(Album.FromId(mediaId), null, false);
                        MainActivity.ServiceConnection.Binder?.Service.Shuffle(true);
                        MainActivity.ServiceConnection.Binder?.Service.Play();
                    }else if (mediaTypeShuffle == MediaType.Artist)
                    {
                        MainActivity.ServiceConnection.Binder?.Service.GenerateQueue(Artist.FromId(mediaId), null, false);
                        MainActivity.ServiceConnection.Binder?.Service.Shuffle(true);
                        MainActivity.ServiceConnection.Binder?.Service.Play();
                    }
                    break;
                case MediaType.Album:
                case MediaType.Artist:
                default:
                    throw new ArgumentOutOfRangeException();
            }
            base.OnPlayFromMediaId(mediaId, extras);
        }

        public override void OnPause()
        {
#if DEBUG
            MyConsole.WriteLine("OnPause");
#endif
            MainActivity.ServiceConnection.Binder?.Service.Pause();
            //OnPauseImpl();
            base.OnPause();
        }

        public override void OnStop()
        {
#if DEBUG
            MyConsole.WriteLine("OnStop");
#endif
            MainActivity.ServiceConnection.Binder?.Service.Stop();
            //OnStopImpl();
            base.OnStop();
        }

        public override void OnSkipToNext()
        {
#if DEBUG
            MyConsole.WriteLine("OnSkipToNext");
#endif
            MainActivity.ServiceConnection.Binder?.Service.NextSong();
            //OnSkipToNextImpl();
            base.OnSkipToNext();
        }

        public override void OnSkipToPrevious()
        {
#if DEBUG
            MyConsole.WriteLine("OnSkipToPrevious");
#endif
            MainActivity.ServiceConnection.Binder?.Service.PreviousSong();
            //OnSkipToPreviousImpl();
            base.OnSkipToPrevious();
        }

        public override void OnCustomAction(string? action, Bundle? extras)
        {
#if DEBUG
            MyConsole.WriteLine("OnCustomAction");
#endif
            try
            {
                switch (action)
                {
                    case "loop":
                        MainActivity.ServiceConnection.Binder?.Service.ToggleLoop((int)MainActivity.ServiceConnection.Binder.Service.QueueObject.LoopState + 1);
                        break;
                    case "shuffle":
                        MainActivity.ServiceConnection.Binder?.Service.Shuffle(!MainActivity.ServiceConnection.Binder.Service.QueueObject.IsShuffled);
                        break;
                    default:
                        throw new ArgumentException("Must use loop or shuffle as action argument");
                }
            }
            catch (Exception e)
            {
#if DEBUG
                MyConsole.WriteLine(e);
#endif
                //throw;
            }
            //OnCustomActionImpl(action, extras);
            base.OnCustomAction(action, extras);
        }

        public override void OnPlayFromSearch(string? query, Bundle? extras)
        {
#if DEBUG
            MyConsole.WriteLine("OnPlayFromSearch");
#endif

            //OnPlayFromSearchImpl(query, extras);
            base.OnPlayFromSearch(query, extras);
        }
        
    }
}