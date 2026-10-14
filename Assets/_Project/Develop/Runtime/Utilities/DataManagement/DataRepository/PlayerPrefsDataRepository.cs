using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Utilities.DataManagement.DataRepository
{
    public class PlayerPrefsDataRepository : IDataRepository
    {
        public UniTask<bool> Exists(string key, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            bool exists = PlayerPrefs.HasKey(key);

            return UniTask.FromResult(exists);
        }

        public UniTask<string> Read(string key, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string text = PlayerPrefs.GetString(key);

            return UniTask.FromResult(text);
        }

        public UniTask Remove(string key, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            PlayerPrefs.DeleteKey(key);

            return UniTask.CompletedTask;
        }

        public UniTask Write(string key, string serializedData, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            PlayerPrefs.SetString(key, serializedData);

            return UniTask.CompletedTask;
        }
    }
}
