using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace BehaviorAnalysis.Jobs
{

    public class JobMethodDispatcher
    {
        public JobMethodDispatcher(MethodInfo methodInfo)
        {
            _executor = GetExecutor(methodInfo);
            MethodInfo = methodInfo;
        }

        private JobExecutor _executor;

        private delegate object JobExecutor(IJob job, object[] parameters);

        private delegate void VoidActionExecutor(IJob job, object[] parameters);

        public MethodInfo MethodInfo { get; private set; }

        public object Execute(IJob job, object[] parameters)
        {
            return _executor(job, parameters);
        }

        private static JobExecutor GetExecutor(MethodInfo methodInfo)
        {
            // Parameters to executor
            ParameterExpression jobParameter = Expression.Parameter(typeof(IJob), "job");
            ParameterExpression parametersParameter = Expression.Parameter(typeof(object[]), "parameters");

            // Build parameter list
            List<Expression> parameters = new List<Expression>();
            ParameterInfo[] paramInfos = methodInfo.GetParameters();
            for (int i = 0; i < paramInfos.Length; i++)
            {
                ParameterInfo paramInfo = paramInfos[i];
                BinaryExpression valueObj = Expression.ArrayIndex(parametersParameter, Expression.Constant(i));
                UnaryExpression valueCast = Expression.Convert(valueObj, paramInfo.ParameterType);

                // valueCast is "(Ti) parameters[i]"
                parameters.Add(valueCast);
            }

            // Call method
            UnaryExpression instanceCast = (!methodInfo.IsStatic) ? Expression.Convert(jobParameter, methodInfo.ReflectedType) : null;
            MethodCallExpression methodCall = methodCall = Expression.Call(instanceCast, methodInfo, parameters);

            // methodCall is "((TJob) job) method((T0) parameters[0], (T1) parameters[1], ...)"
            // Create function
            if (methodCall.Type == typeof(void))
            {
                Expression<VoidActionExecutor> lambda = Expression.Lambda<VoidActionExecutor>(methodCall, jobParameter, parametersParameter);
                VoidActionExecutor voidExecutor = lambda.Compile();
                return WrapVoidAction(voidExecutor);
            }
            else
            {
                // must coerce methodCall to match ActionExecutor signature
                UnaryExpression castMethodCall = Expression.Convert(methodCall, typeof(object));
                Expression<JobExecutor> lambda = Expression.Lambda<JobExecutor>(castMethodCall, jobParameter, parametersParameter);
                return lambda.Compile();
            }
        }

        private static JobExecutor WrapVoidAction(VoidActionExecutor executor)
        {
            return delegate (IJob job, object[] parameters)
            {
                executor(job, parameters);
                return null;
            };
        }
    }

    internal class JobMethodDispatcherCache : ReaderWriterCache<MethodInfo, JobMethodDispatcher>
    {
        public JobMethodDispatcherCache()
        {
        }

        public JobMethodDispatcher GetDispatcher(MethodInfo methodInfo)
        {
            // Frequently called, so ensure delegate remains static
            return FetchOrCreateItem(methodInfo, (MethodInfo methodInfoInner) => new JobMethodDispatcher(methodInfoInner), methodInfo);
        }
    }

    public abstract class JobDescriptor
    {

       // internal static readonly JobMethodDispatcherCache _staticDispatcherCache = new JobMethodDispatcherCache();

        public Type JobType { get; protected set; }

        public abstract IJob Invoker();

        public abstract ScheduleSetting GetDefaultSetting();

    }

    public class JobDescriptor<T> : JobDescriptor where T : class, IJob, new()
    {

        public JobDescriptor()
        {
            JobType = typeof(T);
        }

        public override ScheduleSetting GetDefaultSetting()
        {
            var attr = JobType.GetCustomAttribute<DefaultScheduleAttribute>();
            if (attr == null)
            {
                return ScheduleSetting.NoSetting;
            }
            else
            {
                return attr.Setting;
            }
        }

        public override IJob Invoker()
        {
            return new T();
        }
    }

}
