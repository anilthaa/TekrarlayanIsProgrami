using Dapper;
using System.Data.Common;
using System.Runtime.Intrinsics.Arm;

namespace TekrarlayanIsProgrami.Services
{
    public class DBClass
    {
        //public static string ConnectionString { get; set; } = string.Empty;
        private readonly DbProviderFactory dbProviderFactory;
        private readonly string _connectionString;
        public DBClass(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException("Bağlantı cümlesi null veya empty olamaz.", nameof(connectionString));
            }
            DbProviderFactories.RegisterFactory("Npgsql", Npgsql.NpgsqlFactory.Instance);
            dbProviderFactory = DbProviderFactories.GetFactory("Npgsql");
            _connectionString = connectionString;
            //if (!string.IsNullOrEmpty(DBConnectionString))
            //    ConnectionString = DBConnectionString;
        }

        private DbConnection CreDB()
        {
            //DbConnection Db;
            //Db = dbProviderFactory.CreateConnection();
            //Db.ConnectionString = ConnectionString;// DatabaseSettings.ConnectionString;
            //return Db;
            var db = dbProviderFactory.CreateConnection();
            db.ConnectionString = _connectionString;
            return db;
        }


        public int SQLExecute(string SQLSorgu, object? Params = null)
        {
            using var db = CreDB();
            if (db.State != System.Data.ConnectionState.Open)
            {
                db.Open();
            }
            try
            {
                int item=db.Execute(SQLSorgu, Params);
                Console.WriteLine($"✅ SQLExecute çalıştı. Etkilenen satır sayısı: {item}");
                return item;
            }
            catch (Exception ex)
            {

                Console.WriteLine($"❌ SQLExecute hata: {ex.Message}");
                return 0;

            }
            //DbConnection Db = CreDB();
            //if (Db.State != System.Data.ConnectionState.Open)
            //    Db.Open();
            //try
            //{
            //    Db.Execute(SQLSorgu, Params);
            //}
            //catch (Exception ex)
            //{
            //    var den = ex.Message;
            //}
            //finally
            //{
            //    if (Db.State != System.Data.ConnectionState.Closed)
            //        Db.Close();
            //    Db.Dispose();
            //}
        }

        public T SQLExecuteScalar<T>(string SQLSorgu, object? Params = null)
        {
            using var db = CreDB();
            if (db.State != System.Data.ConnectionState.Open)
                db.Open();

            try
            {
                Console.WriteLine($"📥 SQLExecuteScalar çalışıyor: {SQLSorgu}");
                var result = db.ExecuteScalar<object>(SQLSorgu, Params);

                if (result == null)
                {
                    Console.WriteLine("ExecuteScalar null döndü.");
                    return default!;
                }

                Console.WriteLine($"ExecuteScalar sonucu tipi: {result.GetType().Name}, değeri: {result}");

                
                if (typeof(T) == typeof(int))
                {
                    if (result is long l)
                        return (T)(object)(int)l;
                    if (result is int i)
                        return (T)(object)(int)i;
                    if (int.TryParse(result.ToString(), out var parsed))
                        return (T)(object)(parsed);
                    return default;
                   

                  
                }

                // Diğer tipler için normal dönüşüm
                return (T)Convert.ChangeType(result, typeof(T));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SQLExecuteScalar hata: {ex.Message}");
                return default!;
            }
            //DbConnection Db = CreDB();
            //if (Db.State != System.Data.ConnectionState.Open)
            //    Db.Open();
            //try
            //{
            //    return Db.ExecuteScalar<T>(SQLSorgu, Params);
            //}
            //catch (Exception ex)
            //{
            //    var den = ex.Message;
            //    return default;
            //}
            //finally
            //{
            //    if (Db.State != System.Data.ConnectionState.Closed)
            //        Db.Close();
            //    Db.Dispose();
            //}
        }

        public async Task<T> SQLQueryFirstOrDefaultAsync<T>(string SQLSorgu, object? Params = null)
        {
            //DbConnection Db = CreDB();
            using var Db = CreDB();
            if (Db.State != System.Data.ConnectionState.Open)
                Db.Open();
            try
            {
                return await Db.QueryFirstOrDefaultAsync<T>(SQLSorgu, Params);
            }
            catch (Exception ex)
            {
                var den = ex.Message;
                return default;
            }
            //finally
            //{
            //    if (Db.State != System.Data.ConnectionState.Closed)
            //        Db.Close();
            //    Db.Dispose();
            //}
        }

        public async Task<IEnumerable<T>> SQLQueryAsync<T>(string SQLSorgu, object? Params = null)
        {
            //DbConnection Db = CreDB();
            using var Db = CreDB();
            if (Db.State != System.Data.ConnectionState.Open)
                Db.Open();
            try
            {
                return await Db.QueryAsync<T>(SQLSorgu, Params);
            }
            catch (Exception ex)
            {
                var den = ex.Message;
                return default;
            }
            //finally
            //{
            //    if (Db.State != System.Data.ConnectionState.Closed)
            //        Db.Close();
            //    Db.Dispose();
            //}
        }
    }
}

