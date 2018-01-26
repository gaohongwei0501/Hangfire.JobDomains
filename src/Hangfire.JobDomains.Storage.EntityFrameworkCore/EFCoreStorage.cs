﻿using Common.Logging;
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
        static ILog loger = LogManager.GetLogger<EFCoreStorage>();

        public abstract EFCoreDBContext GetContext();

        public abstract bool TransactionEnable { get; }


        public async Task<T> TryTransaction<T>( Func<EFCoreDBContext, Task<T>> NomalBack, Func<T> ExceptionBack)
        {

            using (var context = GetContext())
            {
                if (TransactionEnable)
                {
                    using (var transaction = await context.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            var result = await NomalBack(context);
                            transaction.Commit();
                            return result;
                        }
                        catch (Exception ex)
                        {
                            loger.Error(ex);
                            if (ExceptionBack == null) ExceptionBack = () => default(T);
                            return ExceptionBack();
                        }
                    }
                }
                else
                {
                    try
                    {
                        var result = await NomalBack(context);
                        return result;
                    }
                    catch (Exception ex)
                    {
                        loger.Error(ex);
                        if (ExceptionBack == null) ExceptionBack = () => default(T);
                        return ExceptionBack();
                    }
                }

            }
        }





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

        public Task<bool> ClearServer(string serverName)
        {
            return TryTransaction<bool>(async (context) =>
            {
                var server = context.Servers.SingleOrDefault(s => s.Name == serverName);
                if (server == null)
                {
                    await context.Servers.AddAsync(new Entities.Server
                    {
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
                return result > 0;

            }, () => false);
        }


        public Task<bool> AddOrUpdateServerAsync(ServerDefine model, List<string> pluginNames)
        {
            return TryTransaction<bool>(async (context) =>
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
                return result > 0;

            }, () => false);
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

        public List<string> GetServersByPlugin(string domain)
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

        #region  PluginDefine

        void ClearPlugin(EFCoreDBContext context, string domainName)
        {
            var domain = context.Plugins.SingleOrDefault(s => s.Title == domainName);
            if (domain == null) return;
            var assemblies = context.Assemblies.Where(s => s.PluginID == domain.ID);
            var jobs = context.Jobs.Where(s => s.DomainID == domain.ID);
            var constructorParameters = context.JobConstructorParameters.Where(s => s.DomainID == domain.ID);

            context.JobConstructorParameters.RemoveRange(constructorParameters);
            context.Jobs.RemoveRange(jobs);
            context.Assemblies.RemoveRange(assemblies);
            context.Plugins.Remove(domain);
        }

        public Task<bool> AddPluginAsync(PluginDefine define)
        {
            return TryTransaction<bool>(async (context) =>
            {
                var domain = define.GetDomain();
                ClearPlugin(context, domain.Title);
                var domainResult = await context.Plugins.AddAsync(domain);
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

                var result = await context.SaveChangesAsync();
                return true;
            }, () => false);
        }
 
        public List<PluginDefine> GetAllPlugins()
        {
            using (var context = GetContext())
            {
                var Plugins = context.Plugins.ToList();
                return Plugins.Select(s => new PluginDefine(s.PathName,s.Title, s.Description)).ToList();
            }
        }

        public List<PluginDefine> GetPlugins(string server)
        {
            using (var context = GetContext())
            {
                var result = from o in context.Plugins
                             join m in context.ServerPlugs
                             on o.PathName equals m.PlugName
                             where m.ServerName == server
                             select o;
        
                return result.Select(s => new PluginDefine(s.PathName, s.Title, s.Description)).ToList();
            }
        }

        public List<AssemblyDefine> GetAssemblies(PluginDefine pluginDefine)
        {
            using (var context = GetContext())
            {
                var plugin = context.Plugins.FirstOrDefault(s => s.Title == pluginDefine.Title);
                if (plugin == null) return new List<AssemblyDefine>();
                var assemblies = context.Assemblies.Where(s => s.PluginID == plugin.ID);
                return assemblies.Select(s => new AssemblyDefine(pluginDefine, s.FileName, s.FullName, s.ShortName, s.Title, s.Description)).ToList();
            }
        }

        public List<JobDefine> GetJobs(AssemblyDefine assemblyDefine)
        {
            using (var context = GetContext())
            {
                var domainDefine = assemblyDefine.Parent;
                if (domainDefine == null) return new List<JobDefine>();

                var domain = context.Plugins.FirstOrDefault(s => s.Title == domainDefine.Title);
                if (domain == null) return new List<JobDefine>();
                var assembly = context.Assemblies.Where(s => s.PluginID == domain.ID).FirstOrDefault(s => s.ShortName == assemblyDefine.ShortName);
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

                var domain = context.Plugins.FirstOrDefault(s => s.Title == assemblyDefine.Parent.Title);
                if (domain == null) return new List<ConstructorDefine>();
                var assembly = context.Assemblies.Where(s => s.PluginID == domain.ID).FirstOrDefault(s => s.ShortName == assemblyDefine.ShortName);
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


    }
}
