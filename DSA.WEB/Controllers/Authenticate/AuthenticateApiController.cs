using DSA.WEB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace DSA.WEB.Controllers.Authenticate
{
    public class AuthenticateApiController : ApiController
    {
        [Route("api/authenticate/login")]
        [HttpPost]
        public IHttpActionResult LoginJwt(LoginRequest request)
        {
            if (request.Username == "djuro" && request.Password == "zguro")
            {
                var loginJwt = new LoginResponse
                {
                    Jwt = "123478c23n91t964x43"
                };

                return Ok(loginJwt);
            }

            return Unauthorized();
        }
    }
}
