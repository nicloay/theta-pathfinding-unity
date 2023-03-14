using UnityEngine;

namespace Modules.MapGenerator
{
    // we don't want to use BitArray, as we will access array often. so sacrifice space and single bool take 1 byte (not bit)
    public class RawMapData
    {
        public readonly int Width;
        public readonly int Height;
        public readonly bool[,] Map;
        
        public RawMapData(int width, int height)
        {
            Width = width;
            Height = height;
            Map = GenerateMap();
        }


        private bool[,] GenerateMap()
        {
            var noise = new FastNoiseLite(UnityEngine.Random.Range(int.MinValue, int.MaxValue));
            noise.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
            noise.SetFrequency(-0.01f);
            noise.SetFractalType(FastNoiseLite.FractalType.PingPong);
            noise.SetCellularDistanceFunction(FastNoiseLite.CellularDistanceFunction.Hybrid);
            noise.SetCellularReturnType(FastNoiseLite.CellularReturnType.Distance2Mul);
            noise.SetCellularJitter(0.6f);

            var noiseMap = new bool[Width, Height];
             
            
            // need to collect and count all districts. 
            for (var y = 0; y < Height; y++)
            for (var x = 0; x < Width; x++)
            {
                var value = noise.GetNoise(x, y) >= 0;
                noiseMap[x, y] = value;
            }

            return noiseMap;
        }

        private static readonly Color32 COLOR_PASS = Color.black;
        private static readonly Color32 COLOR_BLOCK = Color.white;
        
        public Texture2D GetTexture()
        {
            var result = new Texture2D(Width, Height, TextureFormat.R8, false);
            var pixels = new Color32[Width * Height];
            var i = 0;
            for (var y = 0; y < Height; y++)
            for (var x = 0; x < Width; x++)
            {
                pixels[i++] = Map[x, y] ? COLOR_BLOCK : COLOR_PASS;
            }
            
            result.SetPixels32(pixels);
            result.Apply(false, true);
            return result;
        }
    }
}
