﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;

namespace OstaFandy.DAL.Repos.IRepos
{
    public interface IUnitOfWork
    {
        public IUserRepo UserRepo { get; }



        public int Save();
        public IDbContextTransaction BeginTransaction();
    }
}
