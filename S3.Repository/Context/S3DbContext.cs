using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S3.Repository
{
    public class S3DbContext : DbContext
    {
        public S3DbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<S3FileModel> s3FileModels { get; set; }

    }
}
