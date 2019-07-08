using MongoDB.Driver;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

/* OAuth Token = BQA2JZJPBd3vAYqAplQZmXdxxR1NC*/

namespace SpotifyTest1
{

    public partial class MainWindow : Window {


        public string lyrics { get; set; }
        public int albumpos { get; set; }
        public int trackpos { get; set; }



        private Access access;

        private MongoDBClass mongo;

        public MainWindow()
        {
            InitializeComponent();
            //Access è la classe che gestisce il collegamento a spotify
            lyrics = null;
            access = new Access();
            try
            { 
                mongo = new MongoDBClass("Lyricsfy");
                //istanzio la classe che gestisce le api id spotify
                access.auth.AuthReceived += async (senders, payload) =>
                {
                    access.auth.Stop(); // `sender` is also the auth instance
                    access.api = new SpotifyWebAPI() { TokenType = payload.TokenType, AccessToken = payload.AccessToken };

                    //Scarico le informazioni da spotify
                    await access.Init();
                    await InitInterface();

                    //Una volta scaricate Inizializzo gli elementi grafici
                    lyrics = TrovaArtista();

                    //lyrics = TrovaArtista(mongo, access
                    //Aggiorna l'interfaccia e fa richieste a spotify
                    UpdateEvent();
                };
                access.Start();
            }
            catch (MongoExecutionTimeoutException e)
            {
                MessageBox.Show("Connection Expired", "Warning", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void InserisciArtista()
        {
            try
            {
                mongo.InsertRecord("Artists", new ArtistModel()
                {
                    ArtistName = access.artist.Name,
                    Albums = new List<AlbumModel>()
                    {
                        new AlbumModel()
                        {
                            AlbumName = access.album.Name,
                            Tracks = new List<TrackModel>()
                            {
                                new TrackModel()
                                {
                                    TrackName = access.Track.Name
                                }
                            }
                        }
                    }
                });
                MessageBox.Show("Artista Inserito");
                Achtung();
            }
            catch (MongoException e)
            {
                _ = MessageBox.Show("Errore : " + e.Message);
            }
        }
        private string TrovaArtista()
        {
            IMongoCollection<ArtistModel> collection = mongo.Db.GetCollection<ArtistModel>("Artists"); 
            ArtistModel artistcollection = null;

            try
            {
                artistcollection = mongo.LoadRecordByName<ArtistModel>("Artists", "ArtistName", access.artist.Name);
                
                if (artistcollection != null)
                {
                    string lyrics = TrovaAlbum(access, artistcollection, collection);
                    if (lyrics != null)
                    {
                        return lyrics;
                    }
                }
                else
                {
                    InserisciArtista();
                    Achtung();
                }
            }
            catch (InvalidOperationException e)
            {
                //InserisciArtista();
                MessageBox.Show("Error : " + e.Message);
            }
            return null;
        }
        private string TrovaAlbum(Access access, ArtistModel artistcollection, IMongoCollection<ArtistModel> collection)
        {
            FilterDefinition<ArtistModel> ArtistFilter = Builders<ArtistModel>.Filter.Eq("ArtistName", access.artist.Name);
            

            var UpdateAlbum = Builders<ArtistModel>.Update.Push("Albums",
                new AlbumModel
                {
                    AlbumName = access.album.Name,
                    Tracks = new List<TrackModel>
                    {
                        new TrackModel
                        {
                            TrackName = access.Track.Name,
                            Lyrics = null
                        }
                    }
                });
            int AlbumPos = -1;
            int i = 0;

            while (i < artistcollection.Albums.Count)
            {
                if (artistcollection.Albums[i].AlbumName == access.album.Name)
                {
                    MessageBox.Show("Album Trovato : " + artistcollection.Albums[i].AlbumName);
                    AlbumPos = i;
                    albumpos = i;
                    break;
                }
                i++;
            }
            if (AlbumPos == -1)
            {
                UpdateResult updateResult = collection.UpdateOne(ArtistFilter, UpdateAlbum, new UpdateOptions { IsUpsert = true });
                Achtung();
                MessageBox.Show("Album Inserito");
            }
            else
            {
                string lyrics = TrovaTraccia(access, artistcollection, collection, ArtistFilter, AlbumPos);
                if (lyrics != null)
                {
                    return lyrics;
                }
            }
            return null;
        }
        private string TrovaTraccia(Access access, ArtistModel artistcollection, IMongoCollection<ArtistModel> collection, FilterDefinition<ArtistModel> ArtistFilter, int albumPos)
        {
            int TrackPos = -1;
            int i = 0;

            while (i < artistcollection.Albums[albumPos].Tracks.Count)
            {
                if (artistcollection.Albums[albumPos].Tracks[i].TrackName == access.Track.Name)
                {
                    //MessageBox.Show("Traccia Trovata : " + artistcollection.Albums[albumPos].Tracks[i].TrackName);
                    TrackPos = i;
                    trackpos = i;
                    break;
                }
                i++;
            }
            if (TrackPos == -1)
            {
                artistcollection.Albums[albumPos].Tracks.Add(new TrackModel { TrackName = access.Track.Name, Lyrics = null });

                ReplaceOneResult ReplaceResult = collection.ReplaceOne(ArtistFilter, artistcollection, new UpdateOptions { IsUpsert = true });
                //Achtung();
                //MessageBox.Show("Traccia Aggiunta");

            }
            else
            {
                if (artistcollection.Albums[albumPos].Tracks[TrackPos].Lyrics == null)
                {
                    Achtung();
                }
                else
                {
                    return artistcollection.Albums[albumPos].Tracks[TrackPos].Lyrics;
                }
            }
            return null;
        }


        private void UpdateEvent()
        {
            
            while (true)
            {
                _ = Task.Factory.StartNew(async () =>
                  {
                      //Se e' cambiato artista, album o canzone
                      if (access.Update())
                      {
                          //TrovaArtista(mongo, access);
                          //ristampo l'interfaccia con tutti i dati
                          await InitInterface();

                      }
                  });
                Thread.Sleep(1000);
            }
        }
        private Task InitInterface()
        {
            access.Assign();
            Dispatcher.Invoke(() =>
            {
                CurrentlyPlaying.Text = access.FullName;
            });

            Circle.Dispatcher.Invoke(() =>
            {
                Circle.Fill = new ImageBrush(new BitmapImage(new Uri(access.image.Url)));
            });

            Playlist.Dispatcher.Invoke(() =>
            {
                Playlist.Text = " ";
                Playlist.Text = access.AlbumTracks;
            });
            Text.Dispatcher.Invoke(() =>
            {
                this.lyrics = TrovaArtista();
                if (lyrics == null)
                    Text.Text = " ";
                else
                    Text.Text = lyrics;
            });
            return Task.CompletedTask;
        }

        private void OpenInsertMode()
        {
            Dispatcher.Invoke(() =>
            {
                MainW.Width = 950;
                em_Panel.IsEnabled = true;
                Lyrics.IsEnabled = true;
            });
        }
        private void onClickClose(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                MainW.Width = 600;
                em_Panel.IsEnabled = false;
                Lyrics.IsEnabled = false;
            });
        }
        private void onClickOpenInsertMode(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            OpenInsertMode();
        }
        private void onClickInsertLyrics(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() => 
            {
                this.lyrics = Lyrics.Text;
                Text.Text = this.lyrics;
                Lyrics.Text = "";
            });

            ArtistModel artistcollection = mongo.LoadRecordByName<ArtistModel>("Artists", "ArtistName", access.artist.Name);
            FilterDefinition<ArtistModel> ArtistFilter = Builders<ArtistModel>.Filter.Eq("ArtistName", access.artist.Name);
            IMongoCollection<ArtistModel> collection = mongo.Db.GetCollection<ArtistModel>("Artist");

            if (artistcollection.Albums[albumpos].Tracks[trackpos].Lyrics == null)
            {
                artistcollection.Albums[albumpos].Tracks[trackpos].Lyrics = this.lyrics;
                ReplaceOneResult ReplaceResult = collection.ReplaceOne(ArtistFilter, artistcollection, new UpdateOptions { IsUpsert = true });
            }

        }
        private void Achtung()
        {
            var messageResult = MessageBox.Show("Testo Non presente\n\n Vuoi inserire il testo?", "Achtung", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (messageResult == MessageBoxResult.Yes)
            {
                //MessageBox.Show("TO DO!");   
                //_ = Dispatcher.BeginInvoke(new Action(() => OpenInsertPage()));
                OpenInsertMode();
            }
        }
    }
}
