using System.Threading;
using Cysharp.Threading.Tasks;
using System.IO;

namespace Assets._Project.Develop.Runtime.Utilities.DataManagement.DataRepository
{
    public class LocalFileDataRepository : IDataRepository
    {
        private readonly string _folderPath;
        private readonly string _saveFileExtension;

        public LocalFileDataRepository(string folderPath, string saveFileExtension)
        {
            _folderPath = folderPath;
            _saveFileExtension = saveFileExtension;
        }

        public UniTask<bool> Exists(string key, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            bool exists = File.Exists(FullPathFor(key));

            return UniTask.FromResult(exists);
        }

        public UniTask<string> Read(string key, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string text = File.ReadAllText(FullPathFor(key));

            return UniTask.FromResult(text);
        }

        public UniTask Remove(string key, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            File.Delete(FullPathFor(key));

            return UniTask.CompletedTask;
        }

        public UniTask Write(string key, string serializedData, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            File.WriteAllText(FullPathFor(key), serializedData);

            return UniTask.CompletedTask;
        }

        private string FullPathFor(string key)
            => Path.Combine(_folderPath, key) + "." + _saveFileExtension;
    }
}
