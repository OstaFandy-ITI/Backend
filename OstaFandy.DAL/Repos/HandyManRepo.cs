using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OstaFandy.DAL.Entities;
using OstaFandy.DAL.Repos.IRepos;

namespace OstaFandy.DAL.Repos
{
    class HandyManRepo : GeneralRepo<Handyman>, IHandyManRepo
    {
        private readonly AppDbContext _db;
        public HandyManRepo(AppDbContext db) : base(db)
        {
            _db = db;
        }
        public bool checkUniqueNationalId(string nationalid)
        {
            return _db.Handymen.FirstOrDefault(a => a.NationalId == nationalid) == null;
        }
    }
}
