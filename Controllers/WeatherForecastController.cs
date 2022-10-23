using CustomerAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics.Metrics;
using System.Text;

namespace CustomerAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {

        private readonly IHttpClientFactory _httpClientFactory;
        const string ARENA_URL = "https://troll-game.org/warband/players/arena";


        public WeatherForecastController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("/[action]")]
        public IActionResult GetPlayerHistoryById(string id)
        {
            var players = new List<PlayerHistory>();
            using (var connection = new SqliteConnection("Data Source=tg.db"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                        select * from PlayerHistory where uuid = @id;              
                ";

                command.Parameters.AddWithValue("@id", id);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        players.Add(new PlayerHistory()
                        {
                            uuid = reader.GetString(0),
                            player = reader.GetString(1),
                            createdAt = reader.GetString(2),
                        });
                    }
                }
            }
            return Ok(players);

        }

            [HttpGet("/[action]")]
        public async Task<IActionResult> GetArenaPlayers()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, ARENA_URL);
            var client = _httpClientFactory.CreateClient();
            var response = await client.SendAsync(request);
            var resultSet = await ParseJsonResultToPlayerModel(response);

            using (var connection = new SqliteConnection("Data Source=tg.db"))
            {

                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText =
                    @"
                    INSERT INTO Player (uuid, player, isToxic, isAdmin, isMVP, createdData) values (@uuid, @player, @isToxic, @isAdmin, @isMVP, @createdDate);
                ";

                    //var parameter = command.CreateParameter();
                    //parameter.ParameterName = "$value";
                    //command.Parameters.Add(parameter);


                    foreach (var player in resultSet)
                    {
                    command.Parameters.Clear();
                    var checkIfExistsCommand = connection.CreateCommand();
                    checkIfExistsCommand.CommandText =
                        $@"
                            select * from [Player] where [uuid] = @playeruuid;
                        ";
                       var tt = checkIfExistsCommand.Parameters.AddWithValue("@playeruuid", player.uuid);
                    using (var reader = checkIfExistsCommand.ExecuteReader())
                    {
                        var rr = reader.GetValues;
                        var xxx = reader.HasRows;
        
                        if (reader.Read())
                        {
         
                            try
                            {
                                var addPlayerRecord = connection.CreateCommand();
                                addPlayerRecord.CommandText = (@"
                                        INSERT INTO PlayerHistory (uuid, player, createdDate) values (@uuid, @player, @createdDate);
                                     ");
                                addPlayerRecord.Parameters.AddWithValue("@uuid", player.uuid);
                                addPlayerRecord.Parameters.AddWithValue("@player", player.player);
                                addPlayerRecord.Parameters.AddWithValue("@createdDate", DateTime.UtcNow.ToString());
                                addPlayerRecord.ExecuteNonQuery();
                                continue;

                            }
                            catch (Exception ex)
                            {

                                Console.Write(ex.Message);
                                continue;
                            }
                        }
                    }

                    //if (res != null)
                    //{

                    //}
                    command.Parameters.AddWithValue("@uuid", player.uuid);
                        command.Parameters.AddWithValue("@player", player.player);
                        command.Parameters.AddWithValue("@isToxic", 0);
                        command.Parameters.AddWithValue("@isAdmin", 0);
                        command.Parameters.AddWithValue("@isMVP",0);
                        command.Parameters.AddWithValue("@createdDate", DateTime.UtcNow.ToString());


                        command.ExecuteNonQuery();

                    }

                }
                return Ok(resultSet);
            
        }

        [HttpGet("/[action]")]
        public async Task<IActionResult> Summary()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, ARENA_URL);
            var client = _httpClientFactory.CreateClient();
            var response = await client.SendAsync(request);
            var resultSet = await ParseJsonResultToPlayerModel(response);

            var playerSummaryList = new List<PlayerSummary>();
            using (var connection = new SqliteConnection("Data Source=tg.db"))
            {

                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                    select GROUP_CONCAT(PlayerHistory.player, ',') as history, * from player 
                    inner join PlayerHistory on PlayerHistory.uuid = player.uuid
                    where player.uuid = @uuid
                    group by  player.uuid;
                ";

                //var parameter = command.CreateParameter();
                //parameter.ParameterName = "$value";
                //command.Parameters.Add(parameter);


                foreach (var player in resultSet)
                {
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("uuid", player.uuid);
                    using (var reader = command.ExecuteReader())
                    {
                        var rr = reader.GetValues;
                        var xxx = reader.HasRows;

                        if (reader.Read())
                        {

                            try
                            {
                                playerSummaryList.Add(new PlayerSummary()
                                {
                                    history = reader.GetString(0),
                                    uuid = reader.GetString(1),
                                    player = reader.GetString(2),
                                    isToxic = reader.GetInt16(3) == 0 ? false : true,
                                    isMVP = reader.GetInt16(4) == 0 ? false : true,
                                    isAdmin = reader.GetInt16(5) == 0 ? false : true,
                                    createdData = reader.GetString(6).ToString()
                                });

                                continue;

                            }
                            catch (Exception ex)
                            {

                                Console.Write(ex.Message);
                                continue;
                            }
                        }
                    }


                    command.ExecuteNonQuery();

                }

            }
            return Ok(playerSummaryList);

        }

        [HttpGet("/[action]")]
        public async Task<IActionResult> SummaryByID(string id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, ARENA_URL);
            var client = _httpClientFactory.CreateClient();
            var response = await client.SendAsync(request);
            var resultSet = await ParseJsonResultToPlayerModel(response);

            var playerSummary = new PlayerSummary();
            using (var connection = new SqliteConnection("Data Source=tg.db"))
            {

                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                    select GROUP_CONCAT(PlayerHistory.player, ',') as history, * from player 
                    inner join PlayerHistory on PlayerHistory.uuid = player.uuid
                    where player.uuid = @uuid
                    group by  player.uuid;
                ";

                //var parameter = command.CreateParameter();
                //parameter.ParameterName = "$value";
                //command.Parameters.Add(parameter);

                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("uuid", id);
                    using (var reader = command.ExecuteReader())
                    {
                        var rr = reader.GetValues;
                        var xxx = reader.HasRows;

                        if (reader.Read())
                        {

                            try
                            {

                                playerSummary.history = reader.GetString(0);
                                playerSummary.uuid = reader.GetString(1);
                                playerSummary.player = reader.GetString(2);
                                playerSummary.isToxic = reader.GetInt16(3) == 0 ? false : true;
                                playerSummary.isMVP = reader.GetInt16(4) == 0 ? false : true;
                                playerSummary.isAdmin = reader.GetInt16(5) == 0 ? false : true;
                                playerSummary.createdData = reader.GetString(6).ToString();


                            }
                            catch (Exception ex)
                            {

                                Console.Write(ex.Message);
                            }
                        }
                    }


                    command.ExecuteNonQuery();

                

            }
            return Ok(playerSummary);

        }


        private static async Task<List<Player>> ParseJsonResultToPlayerModel(HttpResponseMessage response)
        {
            var playerList = new List<Player>();
            try
            {
                var result = await response.Content.ReadAsStringAsync();
                dynamic stuff = JsonConvert.DeserializeObject(result);
                var b = stuff as JObject;
                var c = b["active"];
                foreach (var item in c)
                {
                    if (!String.IsNullOrEmpty(item["uuid"].ToString()))
                    {
                        Console.WriteLine(item["player"]);
                        playerList.Add(new Player()
                        {
                            player = item["player"].ToString(),
                            uuid = item["uuid"].ToString(),
                            //isadmin = await CheckIfAdmin(item["uuid"].ToString()),
                            //ismvp = await CheckIfMVP(item["uuid"].ToString()),
                            //istoxic = await CheckIfToxic(item["uuid"].ToString())
                        });
                    }
                    if (!String.IsNullOrEmpty(item["uuid2"].ToString()))
                    {
                        Console.WriteLine(item["player2"]);

                        playerList.Add(new Player()
                        {
                            player = item["player2"].ToString(),
                            uuid = item["uuid2"].ToString(),
                            //isadmin = await CheckIfAdmin(item["uuid2"].ToString()),
                            //ismvp = await CheckIfMVP(item["uuid2"].ToString()),
                            //istoxic = await CheckIfToxic(item["uuid2"].ToString())


                        });
                    }
                }
                return playerList;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception when parsing player json");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.Data);
                Console.WriteLine(ex.StackTrace);
                return playerList;
            }
        }

        //[HttpPost("setmvp/{id}")]
        //public async Task<IActionResult> SetMVP(string id)
        //{
        //    var parameters = new { Identifier = id };
        //    var sql = "select * from get_players_by_name(@Identifier)";
        //    var setStatusSQL = @"update player set isMVP = true where player.id = @Identifier;";
        //    var removeStatusSQL = @"update player set isMVP = false where player.id = @Identifier;";

        //    try
        //    {


        //        return Ok("status changed");
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest("exception when changing status: " + ex.Message);
        //    }
        //}

        //[HttpPost("settoxic/{id}")]
        //public async Task<IActionResult> SetToxic(string id)
        //{
        //    var parameters = new { Identifier = id };
        //    var sql = "select istoxic from get_players_by_name(@Identifier)";
        //    var setStatusSQL = @"update player set isToxic = true where player.id = @Identifier;";
        //    var removeStatusSQL = @"update player set isToxic = false where player.id = @Identifier;";

        //    try
        //    {


        //        return Ok("status changed");
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest("exception when changing status: " + ex.Message);
        //    }
        //}



        //[HttpPost("setadmin/{id}")]
        //public async Task<IActionResult> SetAdmin(string id)
        //{
        //    var parameters = new { Identifier = id };
        //    var sql = "select * from get_players_by_name(@Identifier)";
        //    var setStatusSQL = @"update player set isAdmin = true where player.id = @Identifier;";
        //    var removeStatusSQL = @"update player set isAdmin = false where player.id = @Identifier;";

        //    try
        //    {

        //        return Ok("status changed");
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest("exception when changing status: " + ex.Message);
        //    }
        //}


        //[HttpGet("search/{searchText}")]
        //public async Task<List<PlayerHistory>> SearchPlayers(string searchText)
        //{
        //    var x = new List<PlayerHistory>();
        //    var parameters = new { Identifier = searchText };
        //    var sql = "select * from get_players_by_name(@Identifier)";

        //    return x;



        //}


        //[HttpGet("halloffame")]
        //public async Task CheckHallofFame()
        //{

        //    var sql = "select name as player, id as uuid, isAdmin, isMVP from player where isMVP = true";
        //    var request = new HttpRequestMessage(HttpMethod.Get, TG_ARENA_URI);
        //    var client = _clientFactory.CreateClient();
        //    var response = await client.SendAsync(request);
        //    var resultSet = await ParseJsonResultToPlayerModel(response);


        //}

        //public async Task<bool> CheckIfAdmin(string uuid)
        //{
        //    var sql = "select * from player where player.id = @Identifier and player.isAdmin = true";
        //    var parameters = new { Identifier = uuid };

        //    using (var connection = new Npgsql.NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection")))
        //    {
        //        connection.Open();
        //        var result = await connection.QueryAsync<Player>(sql, param: parameters);
        //        if (result.Any())
        //        {
        //            return true;
        //        }
        //        else return false;
        //    }
        //}

        //public async Task<bool> CheckIfMVP(string uuid)
        //{
        //    var sql = "select * from player where player.id = @Identifier and player.ismvp = true";
        //    var parameters = new { Identifier = uuid };

        //    using (var connection = new Npgsql.NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection")))
        //    {
        //        connection.Open();
        //        var result = await connection.QueryAsync<Player>(sql, param: parameters);
        //        if (result.Any())
        //        {
        //            return true;
        //        }
        //        else return false;
        //    }
        //}
        //public async Task<bool> CheckIfToxic(string uuid)
        //{
        //    var sql = "select * from player where player.id = @Identifier and player.istoxic = true";
        //    var parameters = new { Identifier = uuid };

        //    using (var connection = new Npgsql.NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection")))
        //    {
        //        connection.Open();
        //        var result = await connection.QueryAsync<Player>(sql, param: parameters);
        //        if (result.Any())
        //        {
        //            return true;
        //        }
        //        else return false;
        //    }
        //}
    }
}