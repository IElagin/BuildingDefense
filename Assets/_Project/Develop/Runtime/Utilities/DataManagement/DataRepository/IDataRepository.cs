using System.Threading;
using Cysharp.Threading.Tasks;

namespace Assets._Project.Develop.Runtime.Utilities.DataManagement.DataRepository
{
    public interface IDataRepository
    {
        UniTask<string> Read(string key, CancellationToken cancellationToken = default);
        UniTask Write(string key, string serializedData, CancellationToken cancellationToken = default);
        UniTask Remove(string key, CancellationToken cancellationToken = default);
        UniTask<bool> Exists(string key, CancellationToken cancellationToken = default);
    }
}
