using MongoDB.Driver;

namespace Demo.DB;

/// <summary>
/// DB管理器
/// </summary>
public interface IDBManager
{
    IMongoDatabase GetGameDB();
}

public class DBManger : IDBManager
{
    private readonly IMongoDatabase _gameDb;

    public DBManger(StartConfig startConfig)
    {
        string          connStr     = startConfig.GetProperty("game_db");
        MongoUrlBuilder urlBuilder  = new MongoUrlBuilder(connStr);
        MongoClient     mongoClient = new MongoClient(urlBuilder.ToMongoUrl());
        _gameDb = mongoClient.GetDatabase(urlBuilder.DatabaseName);
    }

    public IMongoDatabase GetGameDB() => _gameDb;
}