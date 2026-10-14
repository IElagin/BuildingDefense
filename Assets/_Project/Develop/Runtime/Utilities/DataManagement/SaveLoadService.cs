using Assets._Project.Develop.Runtime.Utilities.DataManagement.DataRepository;
using Assets._Project.Develop.Runtime.Utilities.DataManagement.KeysStorage;
using Assets._Project.Develop.Runtime.Utilities.DataManagement.Serializers;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Assets._Project.Develop.Runtime.Utilities.DataManagement
{
    public class SaveLoadService : ISaveLoadService
    {
        private readonly IDataSerializer _serializer;
        private readonly IDataKeysStorage _keysStorage;
        private readonly IDataRepository _repository;

        public SaveLoadService(
            IDataSerializer serializer,
            IDataKeysStorage keysStorage,
            IDataRepository repository)
        {
            _serializer = serializer;
            _keysStorage = keysStorage;
            _repository = repository;
        }

        public async UniTask<bool> Exists<TData>(CancellationToken cancellationToken = default) where TData : ISaveData
        {
            cancellationToken.ThrowIfCancellationRequested();

            string key = _keysStorage.GetKeyFor<TData>();

            return await _repository.Exists(key, cancellationToken);
        }

        public async UniTask<TData> Load<TData>(CancellationToken cancellationToken = default) where TData : ISaveData
        {
            cancellationToken.ThrowIfCancellationRequested();

            string key = _keysStorage.GetKeyFor<TData>();

            string serializedData = await _repository.Read(key, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            TData data = _serializer.Deserialize<TData>(serializedData);

            return data;
        }

        public async UniTask Remove<TData>(CancellationToken cancellationToken = default) where TData : ISaveData
        {
            cancellationToken.ThrowIfCancellationRequested();

            string key = _keysStorage.GetKeyFor<TData>();

            await _repository.Remove(key, cancellationToken);
        }

        public async UniTask Save<TData>(TData data, CancellationToken cancellationToken = default) where TData : ISaveData
        {
            cancellationToken.ThrowIfCancellationRequested();

            string serializedData = _serializer.Serialize(data);
            string key = _keysStorage.GetKeyFor<TData>();
            await _repository.Write(key, serializedData, cancellationToken);
        }
    }
}
