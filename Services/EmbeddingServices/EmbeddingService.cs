using BlingFire;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Vocon.Services.EmbeddingServices
{
    public class EmbeddingService
    {
        

        private InferenceSession? _session;
        private ulong _handle;
        private bool _isInitialize = false;
        private readonly ModelAssetProvider _assetProvider = new();

        public async Task InitializeAsync(){
            if (_isInitialize == true) return;
            var (onnxPath, tokenizerPath) = await _assetProvider.EnsureModelFilesAsync();
            _session = new InferenceSession(onnxPath);
            _handle = BlingFireUtils.LoadModel(tokenizerPath);
            if(_handle==0){
                throw new Exception("NullReferenceException");
            }

            Debug.WriteLine($"CacheDirectory: {FileSystem.CacheDirectory}");
            _isInitialize = true;
        }
        private int[] Tokenize(string text){
            byte[] inputbytes = Encoding.UTF8.GetBytes(text);
            int[] _rawIds = new int[128];

            int producedCount = BlingFireUtils.TextToIds(_handle,inputbytes,inputbytes.Length,_rawIds,_rawIds.Length,0);

            if(producedCount<0){
                throw new Exception("InvalidOperationException");
            }

            int[] finalIds = new int[producedCount+2];
            finalIds[0] = 0;
            Array.Copy(_rawIds, 0, finalIds, 1, producedCount);
            finalIds[producedCount+1] = 2;

            return finalIds;
        }


        public float[] GetEmbeddings(string text){
            if(_isInitialize==false){
                throw new Exception("InvalidOperationException");
            }

            int[] tokenIds=Tokenize(text);

            DenseTensor<long> inputIdsTensor = new DenseTensor<long>(new[] { 1, tokenIds.Length });
            DenseTensor<long> attentionMaskTensor = new DenseTensor<long>(new[] { 1, tokenIds.Length });
            var tokenTypeIdsTensor = new DenseTensor<long>(new[] { 1, tokenIds.Length });

            for (int i = 0; i < tokenIds.Length; i++)
            {
                inputIdsTensor[0, i] = tokenIds[i];
                attentionMaskTensor[0, i] = 1;
            }

            List<NamedOnnxValue> namedOnnxValues = new List<NamedOnnxValue>();

            namedOnnxValues.Add(NamedOnnxValue.CreateFromTensor("input_ids", inputIdsTensor));
            namedOnnxValues.Add(NamedOnnxValue.CreateFromTensor("attention_mask", attentionMaskTensor));
            namedOnnxValues.Add(NamedOnnxValue.CreateFromTensor("token_type_ids", tokenTypeIdsTensor));
            using var results = _session.Run(namedOnnxValues);
            var lastHiddenState = results.First(v => v.Name == "last_hidden_state").AsTensor<float>();

            float[] embedding = MeanPooling(lastHiddenState,attentionMaskTensor);
            NormalizeInPlace(embedding);
            return embedding;
        }

        private static float[] MeanPooling(Tensor<float> lastHiddenState, DenseTensor<long> attentionMask){
            int seqLen = lastHiddenState.Dimensions[1];
            int hiddenSize = lastHiddenState.Dimensions[2];
            float[] sumVector = new float[hiddenSize];
            long validTokenCount = 0;
            for(int tokenIndex = 0; tokenIndex < seqLen; tokenIndex++){
                if(attentionMask[0, tokenIndex] == 0){
                    continue;
                }
                validTokenCount++;
                for (int dim = 0; dim < hiddenSize; dim++)
                {
                    
                    sumVector[dim] += lastHiddenState[0, tokenIndex, dim];
                }
            }
            if (validTokenCount == 0) return sumVector;

            for (int dim = 0; dim < hiddenSize; dim++)
            {
                sumVector[dim] /= validTokenCount;
            }

            return sumVector;
        }


        private static void NormalizeInPlace(float[] vector){
            double sum=0;
            for(int i = 0; i < vector.Length; i++){
                sum += vector[i] * vector[i];
            }
            double norm = Math.Sqrt(sum);
            if (norm < 1e-12) return;

            for(int i = 0;i < vector.Length;i++){
                vector[i] = (float)(vector[i] / norm);
            }
        }
    }
}
