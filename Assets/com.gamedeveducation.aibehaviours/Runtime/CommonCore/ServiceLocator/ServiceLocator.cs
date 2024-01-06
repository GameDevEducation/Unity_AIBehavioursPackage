using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore
{
    public enum EServiceSearchMode
    {
        GlobalOnly,
        LocalOnly,
        GlobalFirst,
        LocalFirst
    }

    public class ServiceLocator : StandaloneSingleton<ServiceLocator>
    {
        class ServiceAddress
        {
            public System.Type ServiceType;
            public System.Object Context;
        }

        class ServiceQuery
        {
            public EServiceSearchMode SearchMode;
            public ServiceAddress Address;
            public System.Action<ILocatableService> CallbackFn;
        }

        Dictionary<ServiceAddress, ILocatableService> ServiceRegistry;
        List<ServiceQuery> PendingQueries;

        ServiceAddress FindOrCreateAddress<T>(T InService, System.Object InContext) where T : class, ILocatableService
        {
            foreach (var Address in ServiceRegistry.Keys)
            {
                if ((Address.Context == InContext) && (Address.ServiceType == typeof(T)))
                    return Address;
            }

            return new ServiceAddress() { ServiceType = typeof(T), Context = InContext };
        }

        public static void RegisterService<T>(T InService, System.Object InContext = null) where T : class, ILocatableService
        {
            if (Instance != null)
                Instance.RegisterServiceInternal(InService, InContext);
        }

        void RegisterServiceInternal<T>(T InService, System.Object InContext = null) where T : class, ILocatableService
        {
            var Address = FindOrCreateAddress<T>(InService, InContext);

            ServiceRegistry[Address] = InService;

            AttemptToFlushPendingQueries();
        }

        public static void AsyncLocateService<T>(System.Action<ILocatableService> InCallbackFn, 
                                                 System.Object InContext = null, 
                                                 EServiceSearchMode InSearchMode = EServiceSearchMode.GlobalFirst) where T : class, ILocatableService
        {
            if (Instance != null)
                Instance.AsyncLocateServiceInternal<T>(InCallbackFn, InContext, InSearchMode);
        }

        void AsyncLocateServiceInternal<T>(System.Action<ILocatableService> InCallbackFn,
                                           System.Object InContext,
                                           EServiceSearchMode InSearchMode) where T : class, ILocatableService
        {
            // see if we already have the service
            var Result = AttemptToFindService(typeof(T), InContext, InSearchMode);
            if (Result != null)
            {
                InCallbackFn(Result);
                return;
            }

            PendingQueries.Add(new ServiceQuery() { Address = new ServiceAddress() { ServiceType = typeof(T), Context = InContext },
                                                    CallbackFn = InCallbackFn, SearchMode = InSearchMode} );
        }

        void AttemptToFlushPendingQueries()
        {
            for (int Index = PendingQueries.Count - 1; Index >= 0; Index--) 
            { 
                var Query = PendingQueries[Index];

                // attempt to find the service and run the callback if found
                var Result = AttemptToFindService(Query.Address.ServiceType, Query.Address.Context, Query.SearchMode);
                if (Result != null) 
                { 
                    Query.CallbackFn(Result);
                    PendingQueries.RemoveAt(Index);
                }
            }
        }

        ILocatableService AttemptToFindService(System.Type InType, System.Object InContext, EServiceSearchMode InSearchMode)
        {
            System.Func<System.Type, System.Object, ILocatableService> PerformSearchFn = (System.Type InType, System.Object InContext) =>
            {
                foreach(var RegistryEntry in ServiceRegistry)
                {
                    var Address = RegistryEntry.Key;

                    if ((Address.Context == InContext) && (Address.ServiceType == InType))
                        return RegistryEntry.Value;
                }

                return null;
            };

            // are we doing any form of global search?
            if ((InSearchMode == EServiceSearchMode.GlobalOnly) || (InSearchMode == EServiceSearchMode.GlobalFirst))
            {
                // try the global search
                var Result = PerformSearchFn(InType, null);
                if (Result != null)
                    return Result;

                // global only or global first but no context to perform a local search
                if ((InSearchMode == EServiceSearchMode.GlobalOnly) || (InContext == null))
                    return null;

                // try a local search
                return PerformSearchFn(InType, InContext);
            }
            else
            {
                // can only do a local search if we have a context
                if (InContext != null)
                {
                    var Result = PerformSearchFn(InType, InContext);
                    if (Result != null) 
                        return Result;

                    // if we are looking only locally then exit
                    if (InSearchMode == EServiceSearchMode.LocalOnly)
                        return null;

                    // try a global search
                    return PerformSearchFn(InType, null);
                }
            }

            return null;
        }

        protected override void OnInitialise()
        {
            base.OnInitialise();

            ServiceRegistry = new();
            PendingQueries = new();
        }

#if UNITY_EDITOR
        protected override void OnPlayInEditorStopped()
        {
            base.OnPlayInEditorStopped();

            ServiceRegistry = null;
            PendingQueries = null;
        }
#endif // UNITY_EDITOR
    }

    public static class ServiceLocatorBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        public static void Initialise() 
        { 
            if (ServiceLocator.Instance != null)
            {
                ServiceLocator.Instance.OnBootstrapped();
            }
        }
    }
}
