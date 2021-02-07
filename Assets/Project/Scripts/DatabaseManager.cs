using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager Instance { get; set; }

    [Header("The URL used too connect to the Database."), SerializeField] private string _connectionURL;
    [Header("The ID of the Database you want too connect with."), SerializeField] private string _databaseID;

    private MongoClient _client;
    private IMongoDatabase _database;
    private IMongoCollection<BsonDocument> _collection;

    private void Awake()
    {
        DontDestroyOnLoad(this);

        if (Instance != null)
            Destroy(gameObject);

        Instance = this;
    }

    public void InitializeCollection(string p_collection) 
    {
        _client = new MongoClient(_connectionURL);
        _database = _client.GetDatabase(_databaseID);
        _collection = _database.GetCollection<BsonDocument>(p_collection);
    }

    public async void PushToDatabase(string p_key, string p_value) 
    {
        BsonDocument t_doc = new BsonDocument { { p_key, p_value } };

        Task<string> t_task = PullValueFromDatabase(p_key);
        string t_result = await t_task;

        if (t_result != "") return;

        await _collection.InsertOneAsync(t_doc);
    }

    public async Task<Dictionary<string, string>> PullFromDatabase()
    {
        Task<IAsyncCursor<BsonDocument>> t_task = _collection.FindAsync(new BsonDocument());
        IAsyncCursor<BsonDocument> t_await = await t_task;

        Dictionary<string, string> t_output = new Dictionary<string, string>();

        foreach (var t_data in t_await.ToList())
        {
            t_output.Add(GetKey(t_data.ToString()), GetValue(t_data.ToString()));
        }

        return t_output;
    }

    public async Task<string> PullValueFromDatabase(string p_key)
    {
        Task<IAsyncCursor<BsonDocument>> t_task = _collection.FindAsync(new BsonDocument());
        IAsyncCursor<BsonDocument> t_await = await t_task;

        string t_output = "";

        foreach (var t_data in t_await.ToList()) 
        {
            if (GetKey(t_data.ToString()) == p_key)
            {
                t_output = GetValue(t_data.ToString());
            }
        }

        return t_output;
    }

    public async void ChangeValueOnDatabase(string p_key, string p_value)
    {
        Task<string> t_task = PullValueFromDatabase(p_key);
        string t_result = await t_task;

        BsonDocument t_original = new BsonDocument(p_key, t_result);
        BsonDocument t_replacement = new BsonDocument(p_key, p_value);

        await _collection.FindOneAndReplaceAsync(t_original, t_replacement);
    }

    private string GetKey(string p_rawJson)
    {
        var t_jsonWOID = p_rawJson.Substring(p_rawJson.IndexOf("),") + 4);
        var t_key = t_jsonWOID.Substring(0, t_jsonWOID.IndexOf(":") - 2);

        return t_key;
    }
    private string GetValue(string p_rawJson)
    {
        var t_jsonWOID = p_rawJson.Substring(p_rawJson.IndexOf("),") + 4);
        var t_value = t_jsonWOID.Substring(t_jsonWOID.IndexOf(":") + 2, t_jsonWOID.IndexOf("}") - t_jsonWOID.IndexOf(":") - 3);

        return t_value.Replace('"'.ToString(), "");
    }
}