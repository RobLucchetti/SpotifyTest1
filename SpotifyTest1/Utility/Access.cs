using System.Linq;
using System.Threading.Tasks;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;
using MongoDB.Driver;
using System.Diagnostics;
using System.Windows;

/* OAuth Token = BQA2JZJPBd3vAYqAplQZmXdxxR1NC*/

namespace SpotifyTest1
{
    public class Access
    {
        public ImplictGrantAuth auth { get; set; }
        public AuthorizationCodeAuth auth2 { get; set; }
        public SpotifyWebAPI api { get; set; }
        public PlaybackContext context { get; set; }
        public FullTrack Track { get; set; }
        public SimpleArtist artist { get; set; }
        public FullAlbum album { get; set; }
        public Image image { get; set; }
        //public ArtistModel Artistmodel { get; set; }
        public string FullName { get; set; }
        public string AlbumTracks { get; set; }
        public bool wait { get; set; }

        public Access()
        {
            
            this.auth = new ImplictGrantAuth("2e44a66b23614ac9b2aaa31b525dd1b2", "http://localhost:1410", "http://localhost:1410", Scope.UserReadPrivate | Scope.UserReadCurrentlyPlaying);
            this.artist = new SimpleArtist();
            this.auth2 = new AuthorizationCodeAuth("2e44a66b23614ac9b2aaa31b525dd1b2", "89de205ded774a41b137857806c1e479", "http://localhost:1410", "http://localhost:1410", Scope.UserReadPrivate | Scope.UserReadCurrentlyPlaying);
        }
        public Task Init()
        {
            this.context = this.api.GetPlayback();
            return Task.CompletedTask;
        }
        public bool Update()
        {
            if (this.context.Item.Uri != this.api.GetPlayingTrack().Item.Uri)
            {
                this.context = this.api.GetPlayback();
                return true;
            }
            return false;
        }
        public Task Start()
        {
            this.auth.Start(); // Starts an internal HTTP Server
            this.auth.OpenBrowser();
            return Task.CompletedTask;
        }
        public Task Start2()
        {
            this.auth2.Start();
            this.auth2.OpenBrowser();
            return Task.CompletedTask;
        }
        public void Assign()
        {
            // Artistmodel = new ArtistModel();
            
            this.artist = this.context.Item.Artists.First();
            this.album = this.api.GetAlbum(this.context.Item.Album.Id);
            this.Track = this.context.Item;
            this.image = this.context.Item.Album.Images.First();
            this.AlbumTracks = "";
            this.FullName = "Artista : " + this.artist.Name + "\n Album : " + this.album.Name + "\n Traccia : " + this.Track.Name;
            //Artistmodel.ArtistName = this.artist.Name;
            foreach (SimpleTrack tmp in this.album.Tracks.Items)
            {
                this.AlbumTracks += "- " + tmp.Name + "\n";
            }
        }


       /* public async Task<Task> OpenSpotify()
        {
            Process[] lst = Process.GetProcesses();
            
            foreach (Process p in lst)
            {
                if (p.ProcessName.ToLower() == "spotify.exe")
                    MessageBox.Show("Attivo");
                else
                {
                    Process.Start("Spotify.exe");
                    break;
                }
            }
            return Task.CompletedTask;
        }*/
    }

}
