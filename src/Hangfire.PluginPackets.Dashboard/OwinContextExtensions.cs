using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.PluginPackets.Dashboard
{
    internal static class OwinContextExtensions
    {

        public static OwinContext GetOwinContext(this IDictionary<string, object> env)
        {
            return new OwinContext(env);
        }

        private const string FormCollectionKey = "Microsoft.Owin.Form#collection";

        /// <summary>
        /// Hack to prevent "Unable to cast object of type 'Microsoft.Owin.FormCollection' 
        /// to type 'Microsoft.Owin.IFormCollection'" exception, when internalized version
        /// does not match the current project's one.
        /// </summary>
        public static async Task<IFormCollection> ReadFormSafeAsync(this IOwinContext context)
        {
            // We are using internalized version of Microsoft.Owin library.
            // When project that uses Hangfire has another version of this
            // library, we could end with an InvalidCastException. So we
            // can't simply use the `ReadFormAsync` method.

            // As a hack, we're simply removing the corresponding form
            // from an environment, trying to rewind the body stream
            // to the beginning and re-read the form.

            object previousForm = null;

            if (context.Environment.ContainsKey(FormCollectionKey))
            {
                previousForm = context.Environment[FormCollectionKey];
                context.Environment.Remove(FormCollectionKey);
            }

            try
            {
                if (context.Request.Body.CanSeek)
                {
                    context.Request.Body.Seek(0L, SeekOrigin.Begin);
                }

                return await context.Request.ReadFormAsync();
            }
            finally
            {
                if (previousForm != null)
                {
                    context.Environment[FormCollectionKey] = previousForm;
                }
            }
        }
    }
}
