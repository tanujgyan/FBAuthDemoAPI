using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FBAuthDemoAPI.Models
{
    public class Children
    {
        [JsonProperty(PropertyName = "firstName")]
        public string FirstName { get; set; }
        [JsonProperty(PropertyName = "gender")]
        public string gender { get; set; }
        [JsonProperty(PropertyName = "grade")]
        public int grade { get; set; }
        [JsonProperty(PropertyName = "pets")]
        public List<Pets> pets { get; set; }
    }
}
