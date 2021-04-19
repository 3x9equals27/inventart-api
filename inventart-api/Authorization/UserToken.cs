using System;
using System.Linq;
using System.Security.Claims;

namespace Inventart.Authorization
{
    public class UserToken
    {
        public Guid guid { get; set; }
        //public string role { get; set; }

        public UserToken(Guid guid/*,string role*/)
        {
            this.guid = guid;
            //this.role = role;
        }
        public UserToken(Claim[] claims)
        {
            guid = Guid.Parse(claims.First(x => x.Type == "guid").Value);
            //role = claims.First(x => x.Type == "role").Value;
        }

        public Claim[] GetClaims()
        {
            return new[] {
                new Claim("guid", guid.ToString()), 
                //new Claim("role", role) 
            };
        }
    }
}
