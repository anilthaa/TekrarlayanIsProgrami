using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using VeriKatmani.Models;

namespace VeriKatmani.Repository
{
    public class UserRepository
    {
        private readonly DbContext _context;
        
        public UserRepository(DbContext context)
        {
            _context = context;
        }
        public async Task <List<User>> GetAllUsers()
        {
            using var connection=_context.CreateConnection();
            var sql = "Select*From Kullanici";   //Kullanici tablosunu seç.
            return (await connection.QueryAsync<User>(sql)).ToList();
        }
    }
}
