using Hangfire.JobDomains.Models;
using Hangfire.JobDomains.Storage.Sqlite.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Storage.Sqlite
{
    internal static class EntityMapper
    {
        public static Entities.Server Convert(this ServerDefine model)
        {
            return new Entities.Server
            {
                 Name =model.Name,
                 PlugPath=model.PlugPath,
                 Description=model.Description,
                 CreatedAt=DateTime.Now,
            };
        }

        public static Domain GetDomain(this DomainDefine model)
        {
            return new Domain
            {
                BasePath = model.BasePath,
                Name = model.Name,
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


    }


    public class SQLiteStorage : IDomainStorage
    {
 
        public string ServerName  {
            get {
                return Environment.MachineName.ToLower();
            }
        }

        public bool SetConnectString(string connectString)
        {
            SQLiteDBContext.ConnectionString = connectString;
            return SQLiteDBContext.CanService();
        }

        public async Task<bool> AddOrUpdateServerAsync(ServerDefine model)
        {
            if (model.Name != ServerName) throw (new Exception("服务器数据本身修改"));
            using (var context = new SQLiteDBContext())
            {
                var server = context.Servers.SingleOrDefault(s => s.Name == model.Name);
                if (server != null) context.Servers.Remove(server);
                await context.AddAsync(model.Convert());
                var result = await context.SaveChangesAsync();
                return result > 0;
            }
        }

        public async Task<bool> UpdateServerDomainMapAsync(string server, List<string> domains)
        {
            if (server != ServerName) throw (new Exception("服务器数据本身修改"));
            using (var context = new SQLiteDBContext())
            {
                var mappers = context.ServerPlugMaps.Where(s=>s.ServerName== server);
                context.ServerPlugMaps.RemoveRange(mappers);
                var newMappers = domains.Select(s => new ServerPlugMap(server, s));
                await context.ServerPlugMaps.AddRangeAsync(newMappers);
                var result = await context.SaveChangesAsync();
                return result > 0;
            }
        }

        public List<ServerDefine> GetServers()
        {
            using (var context = new SQLiteDBContext())
            {
                var servers = context.Servers.Select(s => new ServerDefine(s.Name,s.PlugPath,s.Description));
                return servers.ToList();
            }
        }

        public ServerDefine GetServer(string server)
        {
            using (var context = new SQLiteDBContext())
            {
                var servers = context.Servers.Where(s => s.Name == server).Select(s => new ServerDefine(s.Name, s.PlugPath, s.Description));
                return servers.FirstOrDefault();
            }
        }

        public List<string> GetServersByDomain(string domain)
        {
            using (var context = new SQLiteDBContext())
            {
                var servers = context.ServerPlugMaps.Where(s => s.PlugName == domain).Select(s => s.ServerName);
                return servers.ToList();
            }
        }

        public bool IsDomainsEmpty()
        {
            using (var context = new SQLiteDBContext())
            {
                return context.ServerPlugMaps.Where(s => s.ServerName == ServerName).Count() == 0;
            }
        }

        public async Task<bool> AddDomainAsync(DomainDefine define)
        {
            using (var context = new SQLiteDBContext())
            {
                var domain = define.GetDomain();
                ClearDomainAsync(context, domain.Name);
                var domainResult = await context.Domains.AddAsync(domain);
                foreach (var assemblyOne in define.GetJobSets())
                {
                    var assembly = assemblyOne.GetAssembly(domainResult.Entity.ID);
                    var assemblyResult = await context.Assemblies.AddAsync(assembly);
                    foreach (var jobOne in assemblyOne.Jobs)
                    {
                        var job = jobOne.GetJob(domainResult.Entity.ID, assemblyResult.Entity.ID);
                        var jobResult = await context.Jobs.AddAsync(job);
                        foreach (var constructorOne in jobOne.Constructors)
                        {
                            var paramers = constructorOne.GetJobConstructorParameters(domainResult.Entity.ID, assemblyResult.Entity.ID, jobResult.Entity.ID);
                            await context.JobConstructorParameters.AddRangeAsync(paramers);
                        }
                    }
                }
                var result = await context.SaveChangesAsync();
                return result > 0;
            }
        }

        void ClearDomainAsync(SQLiteDBContext context,string domainName)
        {
            var domain = context.Domains.SingleOrDefault(s => s.Name == domainName);
            if (domain == null) return;
            var assemblies = context.Assemblies.Where(s => s.DomainID == domain.ID);
            var jobs = context.Jobs.Where(s => s.DomainID == domain.ID);
            var constructorParameters = context.JobConstructorParameters.Where(s => s.DomainID == domain.ID);

            context.JobConstructorParameters.RemoveRange(constructorParameters);
            context.Jobs.RemoveRange(jobs);
            context.Assemblies.RemoveRange(assemblies);
            context.Domains.Remove(domain);
        }

        public List<DomainDefine> GetAllDomains()
        {
            throw new NotImplementedException();
        }

        public Dictionary<SysSettingKey, string> GetSysSetting()
        {
            throw new NotImplementedException();
        }

        public bool SetSysSetting(SysSettingKey key, string value)
        {
            throw new NotImplementedException();
        }

        public Dictionary<int, string> GetJobCornSetting()
        {
            throw new NotImplementedException();
        }

        public bool AddJobCornSetting(int key, string value)
        {
            throw new NotImplementedException();
        }

        public bool DeleteJobCornSetting(int key)
        {
            throw new NotImplementedException();
        }
    }
}
