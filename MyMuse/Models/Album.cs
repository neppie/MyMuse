using System.ComponentModel;
using System.Runtime.Serialization;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace MyMuse.Models
{
    [Table("Albums")]
    //[Bind(Exclude = "AlbumId")]
    [DataContract(Namespace = "OneMusic")]
    public class Album
    {
        [DataMember]
        [DisplayName("Album Art URL")]
        public string AlbumArtUrl { get; set; }

        [Key]
        [DataMember]
        public int AlbumId { get; set; }

        public Guid AlbumGuid { get; set; }

        [DataMember]
        [DisplayName("Artist")]
        public string Artist { get; set; }

        [DataMember]
        [DisplayName("Album")]
        public string AlbumName { get; set; }

        public string FileURI { get; set; }

        [DataMember]
        [DisplayName("Genre")]
        public string GenreId { get; set; }

        public string Title { get; set; }

        [DataMember]
        [DisplayName("#")]
        public int Track { get; set; }

        [DataMember]
        [DisplayName("Year")]
        public int Year { get; set; }
    }

    public class MyMuseContext : DbContext
    {
        public MyMuseContext()           
           //     : base("DefaultConnection")
           : base("MyMuseDb")
        {
        }
        public DbSet<Album> Albums { get; set; }
        public DbSet<AlbumView> AlbumsView { get; set; }
        public DbSet<Covers> Covers { get; set; }
    }
}
