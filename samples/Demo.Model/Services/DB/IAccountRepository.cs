using MongoDB.Bson;
using MongoDB.Driver;

namespace Demo.DB;

public interface IAccountRepository
{
    /// <summary>
    /// 登录查询
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <param name="loginTime"></param>
    /// <param name="loginIp"></param>
    /// <returns></returns>
    ValueTask<AccountModel> Login(string username, string password, DateTime loginTime, string loginIp);

    ValueTask Insert(AccountModel model);
}

public class AccountRepository : IAccountRepository
{
    private readonly IMongoCollection<AccountModel> _c;
    private readonly IMongoCollection<BsonDocument> _ids;

    private readonly FindOneAndUpdateOptions<BsonDocument> _idsOptions = new()
    {
        IsUpsert       = true,
        ReturnDocument = ReturnDocument.After
    };

    public AccountRepository(IDBManager databaseMgr)
    {
        var db = databaseMgr.GetGameDB();
        _c   = db.GetCollection<AccountModel>("Account");
        _ids = db.GetCollection<BsonDocument>("Ids");
    }

    public async ValueTask<AccountModel> Login(string username, string password, DateTime loginTime, string loginIp)
    {
        var update = Builders<AccountModel>.Update.Combine(
            Builders<AccountModel>.Update.Set(f => f.LastLoginTime, loginTime),
            Builders<AccountModel>.Update.Set(f => f.LastLoginIp, loginIp));

        AccountModel accountModel =
            await _c.FindOneAndUpdateAsync<AccountModel>(f => f.Username == username && f.Password == password, update);
        return accountModel;
    }

    public async ValueTask Insert(AccountModel model)
    {
        // 分配一个id
        BsonDocument doc = await _ids.FindOneAndUpdateAsync(Builders<BsonDocument>.Filter.Eq("_id", 0),
            Builders<BsonDocument>.Update.Inc("v", 1), _idsOptions);
        int id = doc["v"].AsInt32;
        model.Id         = id;
        model.RegisterAt = DateTime.Now;
        await _c.InsertOneAsync(model);
    }
}