﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OstaFandy.DAL.Entities;
using OstaFandy.DAL.Repos.IRepos;

namespace OstaFandy.DAL.Repos
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _db;

        public UnitOfWork(AppDbContext db)
        {
            _db = db;
        }
        //add repos here
        private IUserRepo _userRepo;


        // This propertyes initializes the repos if it hasn't been initialized yet
        public IUserRepo UserRepo => _userRepo??=new UserRepo(_db);


        public int Save()
        {
            return _db.SaveChanges();
        }

        public IDbContextTransaction BeginTransaction()
        {
            return _db.Database.BeginTransaction();
        }


    }
}
