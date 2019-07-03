using System.Collections.Generic;

/* OAuth Token = BQA2JZJPBd3vAYqAplQZmXdxxR1NC*/

namespace SpotifyTest1
{
    public partial class MainWindow
    {
        public class AlbumModel
        {
            public string AlbumName { get; set; }
            public List<TrackModel> Tracks { get; set; }
        }
    }

}
