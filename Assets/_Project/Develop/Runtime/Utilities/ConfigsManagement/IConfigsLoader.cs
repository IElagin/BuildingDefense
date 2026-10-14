using System;
using System.Threading;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Assets._Project.Develop.Runtime.Utilities.ConfigsManagement
{
    public interface IConfigsLoader
    {
        UniTask<Dictionary<Type, object>> LoadAsync(CancellationToken cancellationToken = default);
    }
}
