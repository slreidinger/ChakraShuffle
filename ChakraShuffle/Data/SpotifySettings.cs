using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChakraShuffle.Data {
    public class SpotifySettings {
        public string ClientID { get; set; }
        public string ClientSecret { get; set; }
        public string API_URI { get; set; }
        public string AuthURI { get; set; }
        public string TokenURI { get; set; }
    }
}
