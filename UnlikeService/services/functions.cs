using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace UnlikeService.services
{
    public class Functions
    {
        public static async Task<(string message, int code, string error)> UnlikePublication(IMongoDatabase db, string publicationId, int userId)
        {
            var collection = db.GetCollection<BsonDocument>("Publications");

            if (!ObjectId.TryParse(publicationId, out ObjectId pubId))
            {
                return ("", 400, "Invalid publication ID");
            }

            var userIdStr = userId.ToString();

            var filter = Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Eq("_id", pubId),
                Builders<BsonDocument>.Filter.In("Likes", new[] { userIdStr })
            );

            var update = Builders<BsonDocument>.Update.Pull("Likes", userIdStr);

            var result = await collection.UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0)
            {
                return ("", 400, "You have not liked this publication");
            }

            return ("Unlike success", 200, null);
        }
    }
}
