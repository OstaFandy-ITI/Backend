using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OstaFandy.DAL.Entities;
using OstaFandy.DAL.Repos.IRepos;

namespace OstaFandy.DAL.Repos
{
    internal class UserRepo : GeneralRepo<User>,IUserRepo
    {
        private readonly AppDbContext _db;
        public UserRepo(AppDbContext db) : base(db)
        {
            _db = db;
        }
        public bool CheckUniqueOfEmailPhone(string email, string phone)
        {
            return !_db.Users.Any(u => u.Email == email || u.Phone == phone);
        }
        public bool SoftDelete(int id)
        {
            var user = _db.Users.Find(id);
            if (user == null)
            {
                return false;
            }

            user.IsActive= false;
            _db.SaveChanges();
            return true;
        }
    }
}
