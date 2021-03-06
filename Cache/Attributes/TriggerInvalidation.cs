using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using PostSharp.Aspects;

namespace CacheAspect.Attributes
{
    public static partial class Cache
    {
        [Serializable]
        public class TriggerInvalidation : OnMethodBoundaryAspect
        {
            private KeyBuilder _keyBuilder;
            public KeyBuilder KeyBuilder
            {
                get { return _keyBuilder ?? (_keyBuilder = new KeyBuilder()); }
            }

            private bool bySimilar = false;

            #region Constructors

            public TriggerInvalidation(String groupName, CacheSettings settings, String parameterProperty)
            {
                KeyBuilder.GroupName = groupName;
                KeyBuilder.Settings = settings;
                KeyBuilder.ParameterProperties = new string[] { parameterProperty };
            }

            public TriggerInvalidation(String groupName, CacheSettings settings, String[] parameterProperties, bool bySimilar)
            {
                KeyBuilder.GroupName = groupName;
                KeyBuilder.Settings = settings;
                KeyBuilder.ParameterProperties = parameterProperties;
                this.bySimilar = bySimilar;
            }

            public TriggerInvalidation(String groupName, CacheSettings settings)
                : this(groupName, settings, string.Empty)
            {
            }

            public TriggerInvalidation(String groupName)
                : this(groupName, CacheSettings.Default, string.Empty)
            {
            }

            public TriggerInvalidation()
                : this(string.Empty)
            {

            }
            #endregion

            //Method executed at build time.
            public override void CompileTimeInitialize(MethodBase method, AspectInfo aspectInfo)
            {
                KeyBuilder.MethodParameters = method.GetParameters();
                KeyBuilder.MethodName = string.Format("{0}.{1}", method.DeclaringType.FullName, method.Name);
            }

            public override void OnExit(MethodExecutionArgs args)
            {
                string key = KeyBuilder.BuildCacheKey(args.Instance, args.Arguments);

                if (bySimilar)
                {
                    CacheService.Cache.DeleteSimilar(key);
                }
                else if (CacheService.Cache.Contains(key))
                {
                    CacheService.Cache.Delete(key);
                }

                base.OnExit(args);
            }
        }
    }
}

