using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyMuse.Models
{
    public class Artist
    {
        public string ArtArtist;
        public List<Album> ArtAlbum = new List<Album>();
    }
}