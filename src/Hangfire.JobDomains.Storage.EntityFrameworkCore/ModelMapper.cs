using Hangfire.JobDomains.Models;
using Hangfire.JobDomains.Storage.EntityFrameworkCore.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Storage.EntityFrameworkCore
{
    internal static class ModelMapper
    {

        public static Entities.Server Convert(this ServerDefine model)
        {
            return new Entities.Server
            {
                Name = model.Name,
                PlugPath = model.PlugPath,
                Description = model.Description,
                CreatedAt = DateTime.Now,
            };
        }

        public static Plugin GetDomain(this PluginDefine model)
        {
            return new Plugin
            {
                PathName= model.PathName,
                Title = model.Title,
                Description = model.Description,
                CreatedAt = DateTime.Now,
            };
        }

        public static Assembly GetAssembly(this AssemblyDefine model, int DomainID)
        {
            return new Assembly
            {
                DomainID = DomainID,
                FullName = model.FullName,
                FileName = model.FileName,
                ShortName = model.ShortName,
                Title = model.Title,
                Description = model.Description,
                CreatedAt = DateTime.Now,
            };
        }

        public static Job GetJob(this JobDefine model, int DomainID, int AssemblyID)
        {
            return new Job
            {
                DomainID = DomainID,
                AssemblyID = AssemblyID,
                Name = model.Name,
                FullName = model.FullName,
                Title = model.Title,
                Description = model.Description,
                CreatedAt = DateTime.Now,
            };
        }

        public static List<JobConstructorParameter> GetJobConstructorParameters(this ConstructorDefine one, int DomainID, int AssemblyID, int JobID)
        {
            var list = new List<JobConstructorParameter>();
            if (one.Paramers.Count == 0)
            {
                var item = new JobConstructorParameter
                {
                    DomainID = DomainID,
                    AssemblyID = AssemblyID,
                    JobID = JobID,
                    ConstructorGuid = Guid.NewGuid().ToString(),
                    Name = string.Empty,
                    Type = string.Empty,
                    CreatedAt = DateTime.Now,
                };
                list.Add(item);
                return list;
            }

            foreach (var par in one.Paramers)
            {
                var item = new JobConstructorParameter
                {
                    DomainID = DomainID,
                    AssemblyID = AssemblyID,
                    JobID = JobID,
                    ConstructorGuid = Guid.NewGuid().ToString(),
                    Name = par.Name,
                    Type = par.Type,
                    CreatedAt = DateTime.Now,
                };
                list.Add(item);
            }
            return list;
        }

        public static Entities.Queue Convert(this QueueDefine model)
        {
            return new Queue
            {
                Name = model.Name,
                Description = model.Description,
                CreatedAt = DateTime.Now,
            };
        }
    }
}
