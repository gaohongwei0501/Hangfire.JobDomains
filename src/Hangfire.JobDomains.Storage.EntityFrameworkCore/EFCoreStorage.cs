using Hangfire.JobDomains.Models;
using Hangfire.JobDomains.Storage.EntityFrameworkCore.Entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Storage.EntityFrameworkCore
{

    public abstract class EFCoreStorage : IDomainStorage
    {

        public abstract EFCoreDBContext GetContext();

        public abstract bool AddService(string nameOrConnectionString);

        protected string GetConnectionString(string nameOrConnectionString)
        {
            if (IsConnectionString(nameOrConnectionString))
            {
                return nameOrConnectionString;
            }

            if (IsConnectionStringInConfiguration(nameOrConnectionString))
            {
                return ConfigurationManager.ConnectionStrings[nameOrConnectionString].ConnectionString;
            }

            throw new ArgumentException($"Could not find connection string with name '{nameOrConnectionString}' in application config file");
        }

        protected bool IsConnectionString(string nameOrConnectionString)
        {
            return nameOrConnectionString.Contains(";");
        }

        protected bool IsConnectionStringInConfiguration(string connectionStringName)
        {
            var connectionStringSetting = ConfigurationManager.ConnectionStrings[connectionStringName];

            return connectionStringSetting != null;
        }

        #region ServerDefine


        public async Task<bool> ClearServer(string serverName)
        {
            using (var context = GetContext())
            {
                using (var transaction = await context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var server = context.Servers.SingleOrDefault(s => s.Name == serverName);
                        if (server == null) {
                            await context.Servers.AddAsync(new Entities.Server {
                                Name = serverName,
                                CreatedAt = DateTime.Now,
                                Description = "未初始化配置的任务服务器",
                                PlugPath = "",
                            });
                        }

                        var plugs = context.ServerPlugs.Where(s => s.ServerName == serverName);
                        context.ServerPlugs.RemoveRange(plugs);

                        var queues = context.ServerQueues.Where(s => s.ServerName == serverName);
                        context.ServerPlugs.RemoveRange(plugs);

                        var result = await context.SaveChangesAsync();
                        transaction.Commit();
                        return result > 0;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }
        }


        public async Task<bool> AddOrUpdateServerAsync(ServerDefine model, List<string> pluginNames)
        {
            using (var context = GetContext())
            {
                using (var transaction = await context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var server = context.Servers.SingleOrDefault(s => s.Name == model.Name);
                        if (server != null) context.Servers.Remove(server);

                        var plugs = context.ServerPlugs.Where(s => s.ServerName == model.Name);
                        context.ServerPlugs.RemoveRange(plugs);

                        var queues = context.ServerQueues.Where(s => s.ServerName == model.Name);
                        context.ServerPlugs.RemoveRange(plugs);

                        await context.Servers.AddAsync(model.Convert());
                        var plugins = pluginNames.Select(s => new ServerPlugin(model.Name, s));
                        await context.ServerPlugs.AddRangeAsync(plugins);

                        var result = await context.SaveChangesAsync();
                        transaction.Commit();
                        return result > 0;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }
        }


        public List<ServerDefine> GetServers()
        {
            using (var context = GetContext())
            {
                var servers = context.Servers.Select(s => new ServerDefine(s.Name, s.PlugPath, s.Description));
                return servers.ToList();
            }
        }

        public ServerDefine GetServer(string server)
        {
            using (var context = GetContext())
            {
                var servers = context.Servers.Where(s => s.Name == server).Select(s => new ServerDefine(s.Name, s.PlugPath, s.Description));
                return servers.FirstOrDefault();
            }
        }

        public List<string> GetServersByDomain(string domain)
        {
            using (var context = GetContext())
            {
                var servers = context.ServerPlugs.Where(s => s.PlugName == domain).Select(s => s.ServerName);
                return servers.ToList();
            }
        }

        public List<string> GetServersByQueue(string queue)
        {
            using (var context = GetContext())
            {
                var servers = context.ServerQueues.Where(s => s.QueueName == queue).Select(s => s.ServerName);
                return servers.ToList();
            }
        }

        #endregion

        #region QueueDefine

        public List<QueueDefine> GetCustomerQueues()
        {
            using (var context = GetContext())
            {
                var servers = context.ServerQueues.Select(s=>s.QueueName).Distinct().Select(s => new QueueDefine { Name = s, Description = "自定义队列" });
                return servers.ToList();
            }
        }

        public List<QueueDefine> GetCustomerQueues(string server)
        {
            using (var context = GetContext())
            {
                var servers = context.ServerQueues.Where(s => s.ServerName == server).Select(s =>new QueueDefine {  Name=s.QueueName, Description="自定义队列" } );
                return servers.ToList();
            }
        }

        #endregion

        #region DomainDefine

        public async Task<bool> AddDomainAsync(DomainDefine define)
        {
            using (var context = GetContext())
            {
                using (var transaction = await context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var domain = define.GetDomain();
                        ClearDomain(context, domain.Title);
                        var domainResult = await context.Domains.AddAsync(domain);
                        await context.SaveChangesAsync();

                        var assemblies = define.InnerJobSets;
                        if (assemblies != null)
                        {
                            foreach (var assembly in assemblies)
                            {
                                var assemblyOne = assembly.GetAssembly(domainResult.Entity.ID);
                                var assemblyResult = await context.Assemblies.AddAsync(assemblyOne);
                                await context.SaveChangesAsync();

                                var jobs = assembly.InnerJobs;
                                if (jobs == null) continue;
                                foreach (var job in jobs)
                                {
                                    var jobOne = job.GetJob(domainResult.Entity.ID, assemblyResult.Entity.ID);
                                    var jobResult = await context.Jobs.AddAsync(jobOne);
                                    await context.SaveChangesAsync();

                                    var constructors = job.InnerConstructors;
                                    if (constructors == null) continue;
                                    foreach (var constructor in constructors)
                                    {
                                        var paramers = constructor.GetJobConstructorParameters(domainResult.Entity.ID, assemblyResult.Entity.ID, jobResult.Entity.ID);
                                        await context.JobConstructorParameters.AddRangeAsync(paramers);
                                    }
                                }
                            }
                        }

                        var result =await context.SaveChangesAsync();
                        transaction.Commit();
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }
        }

        void ClearDomain(EFCoreDBContext context, string domainName)
        {
            var domain = context.Domains.SingleOrDefault(s => s.Title == domainName);
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
            using (var context = GetContext())
            {
                var domains = context.Domains.ToList();
                return domains.Select(s => new DomainDefine(s.PathName,s.Title, s.Description)).ToList();
            }
        }

        public List<AssemblyDefine> GetAssemblies(DomainDefine domainDefine)
        {
            using (var context = GetContext())
            {
                var domain = context.Domains.FirstOrDefault(s => s.Title == domainDefine.Title);
                if (domain == null) return new List<AssemblyDefine>();
                var assemblies = context.Assemblies.Where(s => s.DomainID == domain.ID);
                return assemblies.Select(s => new AssemblyDefine(domainDefine, s.FileName, s.FullName, s.ShortName, s.Title, s.Description)).ToList();
            }
        }

        public List<JobDefine> GetJobs(AssemblyDefine assemblyDefine)
        {
            using (var context = GetContext())
            {
                var domainDefine = assemblyDefine.Parent;
                if (domainDefine == null) return new List<JobDefine>();

                var domain = context.Domains.FirstOrDefault(s => s.Title == domainDefine.Title);
                if (domain == null) return new List<JobDefine>();
                var assembly = context.Assemblies.Where(s => s.DomainID == domain.ID).FirstOrDefault(s => s.ShortName == assemblyDefine.ShortName);
                if (assembly == null) return new List<JobDefine>();
                var jobs = context.Jobs.Where(s => s.AssemblyID == assembly.ID);
                return jobs.Select(s => new JobDefine(assemblyDefine, s.FullName, s.Name, s.Title, s.Description)).ToList();
            }
        }

        public List<ConstructorDefine> GetConstructors(JobDefine jobDefine)
        {
            using (var context = GetContext())
            {
                var assemblyDefine = jobDefine.Parent;
                if (assemblyDefine == null) return new List<ConstructorDefine>();
                var domainDefine = assemblyDefine.Parent;
                if (domainDefine == null) return new List<ConstructorDefine>();

                var domain = context.Domains.FirstOrDefault(s => s.Title == assemblyDefine.Parent.Title);
                if (domain == null) return new List<ConstructorDefine>();
                var assembly = context.Assemblies.Where(s => s.DomainID == domain.ID).FirstOrDefault(s => s.ShortName == assemblyDefine.ShortName);
                if (assembly == null) return new List<ConstructorDefine>();
                var job = context.Jobs.Where(s => s.AssemblyID == assembly.ID).FirstOrDefault(s => s.Name == jobDefine.Name);
                if (job == null) return new List<ConstructorDefine>();
                var paramerGroups = context.JobConstructorParameters.Where(s => s.JobID == job.ID).GroupBy(s => s.ConstructorGuid);
                var constructors = new List<ConstructorDefine>();
                foreach (var group in paramerGroups)
                {
                    var ctor = new ConstructorDefine();
                    var count = group.Count();
                    foreach (var item in group)
                    {
                        if (count == 1 && item.Name == string.Empty && item.Type == string.Empty)
                        {
                            continue;
                        }
                        ctor.Paramers.Add((item.Name, item.Type));
                    }
                    constructors.Add(ctor);
                }
                return constructors;
            }
        }

        #endregion

        #region SysSetting

        public Dictionary<SysSettingKey, string> GetSysSetting()
        {
            using (var context = GetContext())
            {
                throw new NotImplementedException();
            }
        }

        public bool SetSysSetting(SysSettingKey key, string value)
        {
            using (var context = GetContext())
            {
                throw new NotImplementedException();

            }
        }

        public Dictionary<int, string> GetJobCornSetting()
        {
            var dic = new Dictionary<int, string>();
            dic.Add(1, "一分钟");
            dic.Add(5, "五分钟");
            dic.Add(10, "十分钟");
            return dic;
            //using (var context = GetContext())
            //{
            //    throw new NotImplementedException();
            //}
        }

        public bool AddJobCornSetting(int key, string value)
        {
            using (var context = GetContext())
            {
                throw new NotImplementedException();

            }
        }

        public bool DeleteJobCornSetting(int key)
        {
            using (var context = GetContext())
            {
                throw new NotImplementedException();

            }
        }

        #endregion

    }
}
