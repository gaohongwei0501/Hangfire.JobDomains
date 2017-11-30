using Hangfire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BehaviorAnalysis.Jobs
{

    public enum JobScheduleStatus
    {
        /// <summary>
        /// 未注册
        /// </summary>
        UnRegistered,
         
        /// <summary>
        /// 等待中，延迟job 注册后进入此状态，其他job 不会进入此状态
        /// </summary>
        Waiting,

        /// <summary>
        /// 立即启动，循环job,注册后进入此状态，延迟job 启动进入此状态
        /// </summary>
        Running,

        /// <summary>
        /// 立即启动，延迟任务执行完进入此状态，循环job不会进入此状态
        /// </summary>
        Completed,
    }

    /// <summary>
    /// 任务
    /// </summary>
    public interface IJob
    {
        string Name { get; }

        string Description { get; }

        Task aa();
    }

   
    /// <summary>
    /// 任务激活器
    /// </summary>
    public interface IActivater
    {
        Task Invoke();
    }


    public class Activater: IActivater
    {
        public IJob Job { get; private set; }

        public ScheduleSetting Setting { get; private set; }

        public Activater(IJob job, ScheduleSetting setting)
        {
            Job = job;
            Setting = setting;
        }

        public async Task Invoke()
        {
            if (Setting.IsCycle)
            {
                RecurringJob.AddOrUpdate(()=>Job.aa(), Cron.Daily);
            }
            else if (Setting.ScheduleInterval.TotalSeconds == 0)
            {
                BackgroundJob.Enqueue(() => Job.aa());
            }
            else {
                BackgroundJob.Schedule(() => Job.aa(), new TimeSpan());
            }
        }
    }
    
    [DefaultSchedule(300,true)]
    public class DataAnalysis: IJob
    {
        public  string Name { get; } = "数据分析";

        public  string Description { get; } = "分析数据";

        public  async Task aa()
        {


        }
    }

    public struct ScheduleSetting
    {

        public static ScheduleSetting NoSetting = new ScheduleSetting();

        public Guid ScheduleID { get; set; }

        public bool IsCycle { get; set; }

        public TimeSpan ScheduleInterval { get; private set; }

        public ScheduleSetting(TimeSpan span, bool isCycle = false)
        {
            ScheduleInterval = span;
            IsCycle = isCycle;
            ScheduleID = Guid.NewGuid();
        }

        public ScheduleSetting(int seconds = 0, bool isCycle = false) : this(new TimeSpan(0, 0, seconds), isCycle)
        {

        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            var other = (ScheduleSetting)obj;
            if (other == null) return false;
            return this == other;
        }

        public override int GetHashCode()
        {
            return ScheduleID.GetHashCode();
        }

        public static bool operator ==(ScheduleSetting the, ScheduleSetting other)
        {
            return the.ScheduleID == other.ScheduleID;
        }

        public static bool operator !=(ScheduleSetting the, ScheduleSetting other)
        {
            return the.ScheduleID != other.ScheduleID;
        }

    }

    public class DefaultScheduleAttribute : Attribute
    {
        public ScheduleSetting Setting { get; private set; }

        public DefaultScheduleAttribute(int seconds = 0, bool isCycle = false)
        {
            Setting = new ScheduleSetting(seconds, isCycle);
        }
    }

    /// <summary>
    /// 任务定义集
    /// </summary>
    public sealed class JobDefine
    {
        private JobDefine() { }

        public readonly static JobDefine Sets = new JobDefine();
        static List<JobDescriptor> Jobs { get; set; } = new List<JobDescriptor>();

        public void Add<T>() where T : class, IJob, new()
        {
            var Descriptor = new JobDescriptor<T>();
            Jobs.Add(Descriptor);
        }

        public void Add(Type job)
        {

        }

        public void Add(IEnumerable<Type> jobs)
        {

        }

        public T Get<T>() where T : class, IJob, new()
        {
            var Descriptor = new JobDescriptor<T>();
            return Descriptor.Invoker() as T;
        }

        public JobDescriptor GetDescriptor<T>()
        {
            return Jobs.FirstOrDefault(s=>s.JobType==typeof(T));
        }

        public List<JobDescriptor> GetDescriptors()
        {
            return Jobs;
        }


    }

    public class SchedulingInvoker
    {
        public static async Task ConfigurationAsync()
        {
            Assembly ab = typeof(SchedulingInvoker).Assembly;
            var types = ab.GetExportedTypes();
            var jobs = types.Where(s => s is IJob);
            JobDefine.Sets.Add(jobs);
            await DefaultScheduleRegisterAsync();
        }

        public static async Task DefaultScheduleRegisterAsync()
        {
            var Descriptors = JobDefine.Sets.GetDescriptors();
            foreach (var desc in Descriptors)
            {
                var setting = desc.GetDefaultSetting();
                if (setting != ScheduleSetting.NoSetting)
                {
                    var job = desc.Invoker();
                    var activater = new Activater(job, setting);
                    await activater.Invoke();
                }
            }
        }



    }


}
