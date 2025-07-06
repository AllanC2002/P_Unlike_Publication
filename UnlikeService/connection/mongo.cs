using MongoDB.Driver;
using System;

namespace UnlikeService.connection
{
    public class Mongo
    {
        public static IMongoDatabase MongoConnection()
        {
            var host = Environment.GetEnvironmentVariable("MONGO_HOSTIP");
            var port = Environment.GetEnvironmentVariable("MONGO_PORT");
            var user = Environment.GetEnvironmentVariable("MONGO_USER");
            var pass = Environment.GetEnvironmentVariable("MONGO_PASSWORD");
            var dbName = Environment.GetEnvironmentVariable("MONGO_DB");

            var uri = $"mongodb://{user}:{pass}@{host}:{port}/?authSource=admin";
            var client = new MongoClient(uri);
            return client.GetDatabase(dbName);
        }
    }
}
