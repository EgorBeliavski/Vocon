
namespace Vocon.Services
{
    public class ModelAssetProvider
    {
        private const string _onnxmodel = "model_quantized.onnx";
        private const string _tokenizer = "xlm_roberta_base.bin";

        private async Task<string> EnsureFileExistsInCacheAsync(string assetFileName){
            string targetPath = Path.Combine(FileSystem.CacheDirectory, assetFileName);
            if (System.IO.File.Exists(targetPath)) return targetPath;

            using Stream sourceStream = await FileSystem.OpenAppPackageFileAsync(assetFileName);
            using Stream destinationStream = System.IO.File.Create(targetPath);

            await sourceStream.CopyToAsync(destinationStream);

            return targetPath;
        }


        public async Task<(string OnnxModelPath, string TokenizerPath)> EnsureModelFilesAsync(){
            string TokenizerPath =  await EnsureFileExistsInCacheAsync(_tokenizer);
            string OnnxModelPath =await EnsureFileExistsInCacheAsync(_onnxmodel);

            return (OnnxModelPath, TokenizerPath);
        }
    }
}
