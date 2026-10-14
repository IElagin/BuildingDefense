using System.Threading;
using Cysharp.Threading.Tasks;

namespace Assets._Project.Develop.Runtime.Utilities.DataManagement
{
    public interface ISaveLoadService
    {
        UniTask<TData> Load<TData>(CancellationToken cancellationToken = default) where TData : ISaveData;
        UniTask Save<TData>(TData data, CancellationToken cancellationToken = default) where TData : ISaveData;
        UniTask Remove<TData>(CancellationToken cancellationToken = default) where TData : ISaveData;
        UniTask<bool> Exists<TData>(CancellationToken cancellationToken = default) where TData : ISaveData;
    }
}
