using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace MyMuse.Models
{
    [CollectionDataContract(Name = "ArrayOfAlbumView", Namespace = "OneMusic")]
    public class AlbumsView : List<Models.AlbumView> { }

    [Table("AlbumsView")]
    [DataContract(Namespace = "OneMusic")]
    public class AlbumView : Album
    {
        //[Key]
        //[DisplayName("Guid")]
        //public Guid AlbumGuid { get; set; }
     
        //public string VFileURI;

        //public string VTitle;

        [DataMember]
        public List<string> FileURI = new List<string>();
 
        [DataMember]
        public List<string> Title = new List<string>();
    }
    
    [Table("Covers")]
    public class Covers 
    {
        [Key]
        public Guid CoverGuid { get; set; }
    }
}