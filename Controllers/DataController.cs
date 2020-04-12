using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace gameTrackerApp.Controllers
{
    public class DataController : ApiController
    {
        private string filePath = @"~/Games/";
        private string fileName= "data.json";
        private string password = Properties.Settings.Default.Password;

        // GET api/data
        public string Get()
        {
            var fullPath = System.Web.Hosting.HostingEnvironment.MapPath(filePath);
            string fileData = System.IO.File.ReadAllText(fullPath + fileName);

            return fileData;
        }

        // POST api/data
        //public HttpResponseMessage Post([FromBody]string value)
        public HttpResponseMessage Post(postData value)
        {
            bool updated = false;

            if (value == null) {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            if (value.password != password)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid Password");
            }

            try
            {
                var fullPath = System.Web.Hosting.HostingEnvironment.MapPath(filePath);
                string fileDataString = System.IO.File.ReadAllText(fullPath + fileName);
                dynamic fileData = JsonConvert.DeserializeObject(fileDataString);
                JArray games = fileData.Games;

                // override last saved game if submitted twice
                dynamic lastGame = games.First();
                if (lastGame.GameDate == value.gameData.GameDate)
                {
                    games.Remove(lastGame);
                    updated = true;
                }

                // backup
                string newFileName = fileName + ".backup_" + DateTime.Now.ToString("dd-MM-yyyy_hhmm");
                System.IO.File.WriteAllText(fullPath + newFileName, JsonConvert.SerializeObject(fileData));

                // save new game
                games.AddFirst(value.gameData);
                System.IO.File.WriteAllText(fullPath + fileName, JsonConvert.SerializeObject(fileData));
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e.ToString());
            }

            string text;

            if(updated)
            {
                text = "Game updated successfully";
            }
            else
            {
                text = "Game saved successfully";
            }

            return Request.CreateResponse(HttpStatusCode.OK, text);
        }
    }

    public class postData
    {
        public dynamic gameData { get; set; }
        public string password { get; set; }
    }
}
