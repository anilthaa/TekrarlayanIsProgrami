using Dapper;
using System.Data.Common;

namespace TekrarlayanIsProgrami.Services
{
    public class DBClass
    {
        public static string ConnectionString { get; set; } = string.Empty;
        DbProviderFactory dbProviderFactory;
        private readonly string _connectionString;
        public DBClass(string DBConnectionString = "")
        {
            DbProviderFactories.RegisterFactory("Npgsql", Npgsql.NpgsqlFactory.Instance);
            dbProviderFactory = DbProviderFactories.GetFactory("Npgsql");
            if (!string.IsNullOrEmpty(DBConnectionString))
                ConnectionString = DBConnectionString;
        }

        DbConnection CreDB()
        {
            DbConnection Db;
            Db = dbProviderFactory.CreateConnection();
            Db.ConnectionString = ConnectionString;// DatabaseSettings.ConnectionString;
            return Db;
        }


        public void SQLExecute(string SQLSorgu, object? Params = null)
        {
            DbConnection Db = CreDB();
            if (Db.State != System.Data.ConnectionState.Open)
                Db.Open();
            try
            {
                Db.Execute(SQLSorgu, Params);
            }
            catch (Exception ex)
            {
                var den = ex.Message;
            }
            finally
            {
                if (Db.State != System.Data.ConnectionState.Closed)
                    Db.Close();
                Db.Dispose();
            }
        }

        public T SQLExecuteScalar<T>(string SQLSorgu, object? Params = null)
        {
            DbConnection Db = CreDB();
            if (Db.State != System.Data.ConnectionState.Open)
                Db.Open();
            try
            {
                return Db.ExecuteScalar<T>(SQLSorgu, Params);
            }
            catch (Exception ex)
            {
                var den = ex.Message;
                return default;
            }
            finally
            {
                if (Db.State != System.Data.ConnectionState.Closed)
                    Db.Close();
                Db.Dispose();
            }
        }

        public async Task<T> SQLQueryFirstOrDefaultAsync<T>(string SQLSorgu, object? Params = null)
        {
            DbConnection Db = CreDB();
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
            finally
            {
                if (Db.State != System.Data.ConnectionState.Closed)
                    Db.Close();
                Db.Dispose();
            }
        }

        public async Task<IEnumerable<T>> SQLQueryAsync<T>(string SQLSorgu, object? Params = null)
        {
            DbConnection Db = CreDB();
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
            finally
            {
                if (Db.State != System.Data.ConnectionState.Closed)
                    Db.Close();
                Db.Dispose();
            }
        }
    }
}

