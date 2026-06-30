using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace Vocon.Services
{
    public class ModelAssetProvider
    {
        private const string _onnxmodel = "model_quantized.onnx";
        private const string _tokenizer = "xlm_roberta_base.bin";

        private async Task<string> EnsureFileExistsInCacheAsync(string assetFileName){
            string targetPath = Path.Combine(FileSystem.CacheDirectory, assetFileName);
            if (File.Exists(targetPath)) return targetPath;

            using Stream sourceStream = await FileSystem.OpenAppPackageFileAsync(assetFileName);
            using Stream destinationStream = File.Create(targetPath);

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
