using System;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Linq;
using System.Web;



namespace MgDatabase
{
    public class Service
    {
        private MongoClient _mongoClient;
        private IMongoDatabase _mongoDatabase;
        private IMongoCollection<BsonDocument> collection;

        public Service()
        {
            _mongoClient = new MongoClient("mongodb+srv://testuser:1234@cluster0.4n5gfld.mongodb.net/?retryWrites=true&w=majority");
            // 取得 MongoDatabase 物件
            _mongoDatabase = _mongoClient.GetDatabase("student_db");
            collection = _mongoDatabase.GetCollection<BsonDocument>("student_records");


        }

        

        // Close
        public void OnClosed()
        {
            _mongoClient.Cluster.Dispose();
        }

        // 測試 insert
        public void Insert()
        {
            var document = new BsonDocument
              {
              { "name", "測試資料1" },
              { "roll_no", 2 },
              { "branch", "rew" },
              };
            collection.InsertOne(document);
        }

        // 測試 Search
        public void Search()
        {
            FilterDefinitionBuilder<BsonDocument> builderFilter = Builders<BsonDocument>.Filter;
            //约束条件
            FilterDefinition<BsonDocument> filter = builderFilter.Eq("name", "測試資料1");
            var result = collection.Find(filter).ToList();

            foreach (var item in result)
            {
                //取出整条值
                Console.WriteLine(item.AsBsonValue);
            }
        }

        // 測試 Update
        public void Update()
        {
            FilterDefinitionBuilder<BsonDocument> builderFilter = Builders<BsonDocument>.Filter;
            //约束条件
            FilterDefinition<BsonDocument> filter = builderFilter.Eq("name", "測試資料1");
            collection.UpdateMany(filter, Builders<BsonDocument>.Update.Set("roll_no", 6));
        }

        // 測試 Delete
        public void Delete()
        {
            FilterDefinitionBuilder<BsonDocument> builderFilter = Builders<BsonDocument>.Filter;
            //约束条件
            FilterDefinition<BsonDocument> filter = builderFilter.Eq("name", "測試資料1");
            var a = collection.DeleteMany(filter);
            
        }
        

    }
}