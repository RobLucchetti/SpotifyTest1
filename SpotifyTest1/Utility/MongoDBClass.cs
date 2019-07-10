using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Bson;

/* OAuth Token = BQA2JZJPBd3vAYqAplQZmXdxxR1NC*/

namespace SpotifyTest1
{
    public class MongoDBClass
    {
        public IMongoDatabase Db { get; set; }
        public MongoClient client { get; set; }



        public MongoDBClass(string database)
        {
            client = new MongoClient("mongodb://localhost:27017,localhost:27027,localhost:27037/admin?replicaSet=Lyricsfy");
            Db = client.GetDatabase(database);
        }

        public void InsertRecord<T>(string Table, T Record)
        {
            var collection = Db.GetCollection<T>(Table);
            collection.InsertOne(Record);
        }
        public List<T> LoadRecord<T>(string Table)
        {
            var collection = Db.GetCollection<T>(Table);

            return collection.Find(new BsonDocument()).ToList();
        }
        public T LoadRecordById<T>(string Table, Guid Id)
        {
            var collection = Db.GetCollection<T>(Table);
            var filter = Builders<T>.Filter.Eq("Id", Id);
           
            return collection.Find(filter).First();
        }
        public T LoadRecordByName<T>(string Table,string Field, string Value)
        {
            var collection = Db.GetCollection<T>(Table);
            var filter = Builders<T>.Filter.Eq(Field, Value);
            try
            {
                var result = collection.Find(filter).FirstOrDefault();
                return result;
            }
            catch(System.InvalidOperationException e)
            {
                Console.WriteLine(e.Message);
                return default;
            }
        }
    }
}
