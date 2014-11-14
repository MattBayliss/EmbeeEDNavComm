using EmbeeEDModel.Entities;
using EmbeeEDNavServer.Edsc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace EmbeeEDNavServer
{
    /// <summary>
    /// THANKS HEAPS to http://www.edstarcoordinator.com/
    /// </summary>
    public class StarLoader
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private string _cachedStarsPath;
        private string _backupPath;
        private string _newStarsPath;
        private DateTime _lastFetched;      

        public StarLoader()
        {
            var appfolder = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EmbeeED");
            if (!Directory.Exists(appfolder))
            {
                Directory.CreateDirectory(appfolder);
            }
            _cachedStarsPath = Path.Combine(appfolder, "CachedStars.json");
            _backupPath = Path.Combine(appfolder, "CachedStars.backup.json");
            _newStarsPath = Path.Combine(appfolder, "LatestStars.json");
        }

        public async Task<IEnumerable<StarSystem>> GetStarsAsync()
        {
            var stars = new Dictionary<string, StarSystem>();

            _lastFetched = new DateTime(2000,1,1);

            try
            {
                if (File.Exists(_cachedStarsPath))
                {
                    stars = await ReadCoordinatesFileAsync(_cachedStarsPath);
                }
                else
                {
                    stars = await ReadCoordinatesFileAsync(@"Resources\stars.json");
                }
                // check for new stars every few days
                if (File.Exists(_newStarsPath))
                {
                    File.Delete(_newStarsPath);
                }
                var newstars = await ReadCoordinatesFromEDStarCoordinatorAsync(_lastFetched);

                foreach (var star in newstars.Keys)
                {
                    if (stars.ContainsKey(star))
                    {
                        stars[star] = newstars[star];
                    }
                    else
                    {
                        stars.Add(star, newstars[star]);
                    }
                }

                //write stars to file
                if (File.Exists(_cachedStarsPath))
                {
                    File.Delete(_cachedStarsPath);
                }

                using (var sw = new StreamWriter(_cachedStarsPath))
                {
                    await sw.WriteLineAsync(_lastFetched.ToString("u"));
                    foreach (var ss in stars.Values)
                    {
                        await sw.WriteLineAsync(JsonConvert.SerializeObject(ss));
                    }
                    sw.Close();
                }
            }
            catch (AggregateException agex)
            {
                // problem reading from edstarcoordinator, it might be down
                Logger.Debug("Error trying to download latest star data", agex);
            }
            return stars.Values;
        }

        private async Task<Dictionary<string,StarSystem>> ReadCoordinatesFromEDStarCoordinatorAsync(DateTime lastFetched)
        {
            var stars = new Dictionary<string, StarSystem>();

            var edscRequest = new DataWrapper()
            {
                Data = new GetSystemsRequest()
                {
                    OutputMode = 2, //verbose
                    Filter = new RequestFilter()
                    {
                        KnownStatus = 1, //coords are known
                        ConfidenceRating = 3,
                        LastUpdate = lastFetched
                    }
                }
            };

            var jsonrequest = JsonConvert.SerializeObject(edscRequest);

            using (var client = new HttpClient())
            {
                try
                {
                    var uri = new Uri("http://www.edstarcoordinator.com/");
                    client.BaseAddress = uri;
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var jsonpost = new StringContent(jsonrequest, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync("/api.asmx/GetSystems", jsonpost);

                    string contentString = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        Edsc.Response edscresponse = null;
                        Logger.Trace("Star systems received");
                        try
                        {
                            edscresponse = JsonConvert.DeserializeObject<Response>(contentString);
                        }
                        catch (JsonSerializationException jsex)
                        {
                            // failed
                            Logger.Debug("Deserialization failed", jsex);
                        }

                        if ((edscresponse == null) || (edscresponse.Data.Status.StatusInputs[0].Status.StatusNum != 0))
                        {
                            return stars;
                        }
                        Logger.Debug("Downloading {0} new systems", edscresponse.Data.Systems.Length);

                        foreach (var s in edscresponse.Data.Systems)
                        {
                            stars.Add(s.Name.ToLower(), new StarSystem(s.Name, s.Coordinates[0], s.Coordinates[1], s.Coordinates[2]));
                        }
                        _lastFetched = edscresponse.Data.DateUpdated;
                    }
                }
                catch (AggregateException aex)
                {
                    Logger.Error("Failed to fetch lastest star systems from http://www.edstarcoordinator.com/", aex);
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to fetch lastest star systems from http://www.edstarcoordinator.com/", ex);
                }

            }

            return stars;
        }

        private async Task<Dictionary<string, StarSystem>> ReadCoordinatesFileAsync(string filePath)
        {
            _lastFetched = new DateTime(2000,1,1);

            var stars = new Dictionary<string, StarSystem>();

            if(string.IsNullOrEmpty(filePath)) {
                return null;
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("The coordinate file could not be found: " + filePath);
            }

            try
            {
                using (var coordReader = new StreamReader(filePath))
                {
                    try
                    {
                        var fetchedOnString = await coordReader.ReadLineAsync();

                        DateTime.TryParse(fetchedOnString, out _lastFetched);

                        while (!coordReader.EndOfStream)
                        {
                            var starline = await coordReader.ReadLineAsync();

                            try
                            {
                                var star = JsonConvert.DeserializeObject<StarSystem>(starline);
                                stars.Add(star.Name.ToLower(), star);
                            }
                            catch (JsonSerializationException jsonex)
                            {
                                Logger.Debug("Invalid line in {0}: {1}", filePath, starline);
                                Logger.Debug(string.Empty, jsonex);
                            }
                        }
                    }
                    catch (IOException iioex)
                    {
                        Logger.Error("Error reading stars", iioex);
                    }
                    finally
                    {
                        coordReader.Close();
                    }
                }
            }
            catch (IOException ioex)
            {
                Logger.Error("Error reading stars", ioex);
            }

            return stars;
        }
    }
}
