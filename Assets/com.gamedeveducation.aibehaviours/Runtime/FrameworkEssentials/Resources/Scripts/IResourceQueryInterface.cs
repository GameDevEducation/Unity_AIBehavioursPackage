using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CommonCore
{
    public interface IResourceQueryInterface
    {
        void RequestResourceFocus(GameObject InQuerier, System.Action<CommonCore.Resources.EType> InCallbackFn);
        void RequestResourceSource(GameObject InQuerier, CommonCore.Resources.EType InType, System.Action<GameObject> InCallbackFn);
        void RequestResourceStorage(GameObject InQuerier, CommonCore.Resources.EType InType, System.Action<GameObject> InCallbackFn);
    }
}
