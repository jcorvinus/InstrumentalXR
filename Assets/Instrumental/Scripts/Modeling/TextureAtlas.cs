/*-----------------------------------------------------------
 * Lucidigital Texture Atlas
 * By JCorvinus
 * Currently unlicenced for any use.
 * ----------------------------------------------------------*/
using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Lucidigital.Modeling
{
    /// <summary>
    /// This texture atlas lets you bake multiple textures
    /// into a single large sheet to save on memory
    /// and enforce POT constraints.
    /// </summary>
    public class TextureAtlas : MonoBehaviour
    {
        public delegate void AtlasChangedHandler(TextureAtlas sender);
        public event AtlasChangedHandler AtlasChanged;

        #region Definitions
        /// <summary>
        /// Describes the size of the texture sheet.
        /// </summary>
        public enum Size { x512, x1024, x2048 }
        #endregion

        [SerializeField] Size sheetSize = Size.x2048;
        [SerializeField] RenderTextureFormat pixelFormat = RenderTextureFormat.ARGB32;
        [SerializeField] int emptyArea = 0;
        List<Texture2D> textures;
        List<Rect> regions;
        List<int> indexes;

        RenderTexture textureSheet;
        Texture2D stagingTexture;

        public RenderTexture TextureSheet
        {
            get { return textureSheet; }
        }

        public Texture2D StagingTexture
        {
            get { return stagingTexture; }
        }

        void Awake()
        {
            int rtSize = 128;

            switch (sheetSize)
            {
                case Size.x512:
                    rtSize = 512;
                    break;

                case Size.x1024:
                    rtSize = 1024;
                    break;

                case Size.x2048:
                    rtSize = 2048;
                    break;

                default:
                    break;
            }

            textureSheet = new RenderTexture(rtSize, rtSize, 24, pixelFormat);
            stagingTexture = new Texture2D(rtSize, rtSize, GetFormatFromRTFormat(pixelFormat), false);
            indexes = new List<int>();
            for(int x=0; x < rtSize; x++)
            {
                for (int y = 0; y < rtSize; y++)
                {
                    stagingTexture.SetPixel(x, y, Color.black);
                }
            }
            stagingTexture.Apply();

            Graphics.Blit(stagingTexture, textureSheet);

            emptyArea = rtSize * rtSize;

            textures = new List<Texture2D>();
            regions = new List<Rect>();
        }

        private void RepackTextures()
        {
            Rect[] rectangles = stagingTexture.PackTextures(textures.ToArray(), 0, 2048, false);
            regions.Clear();
            regions.AddRange(rectangles);

            stagingTexture.Apply();
            Graphics.Blit(stagingTexture, textureSheet);

            if (AtlasChanged != null) AtlasChanged(this);
        }

        private TextureFormat GetFormatFromRTFormat(RenderTextureFormat format)
        {
            switch (format)
            {
                case RenderTextureFormat.ARGB32:
                    return TextureFormat.ARGB32;

                case RenderTextureFormat.Depth:
                    throw new System.NotSupportedException();

                case RenderTextureFormat.ARGBHalf:
                    throw new System.NotSupportedException();

                case RenderTextureFormat.Shadowmap:
                    throw new System.NotSupportedException();

                case RenderTextureFormat.RGB565:
                    return TextureFormat.RGB565;

                case RenderTextureFormat.ARGB4444:
                    return TextureFormat.ARGB4444;

                case RenderTextureFormat.ARGB1555:
                    throw new System.NotSupportedException();

                case RenderTextureFormat.Default:
                    throw new System.NotSupportedException();

                case RenderTextureFormat.ARGB2101010:
                    throw new System.NotSupportedException();

                case RenderTextureFormat.DefaultHDR:
                    throw new System.NotSupportedException();

                case RenderTextureFormat.ARGBFloat:
                    return TextureFormat.RGBAFloat;

                case RenderTextureFormat.RGFloat:
                    throw new System.NotSupportedException();

                case RenderTextureFormat.RGHalf:
                    return TextureFormat.RGHalf;

                case RenderTextureFormat.RFloat:
                    return TextureFormat.RFloat;

                case RenderTextureFormat.RHalf:
                    return TextureFormat.RHalf;

                case RenderTextureFormat.R8:
                    throw new System.NotImplementedException();

                case RenderTextureFormat.ARGBInt:
                    return TextureFormat.ARGB32;

                case RenderTextureFormat.RGInt:
                    throw new System.NotImplementedException();

                case RenderTextureFormat.RInt:
                    throw new System.NotImplementedException();

                default:
                    return TextureFormat.ARGB32;
            }
        }

        /// <summary>
        /// Adds a new texture to the atlas.
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="finalTexture">If you're doing a batch update, mark this true to trigger the atlas rebuild.</param>
        /// <returns>A Texture ID used for indexing the new texture. -1 for failure (no room on sheet)</returns>
        public int AddTexture(Texture2D texture, bool finalTexture)
        {
            int textureArea = texture.width * texture.height;

            if (emptyArea < textureArea)
            {
                Debug.Log("Not enough space on sheet!");
                return -1;
            }

            int newID = 0;

            while (true)
            {
                bool found = false;
                for (int i = 0; i < indexes.Count; i++)
                {
                    if (indexes[i] == newID) found = true;
                }

                if (found) newID++;
                else break;
            }

            textures.Add(texture);
            indexes.Add(newID);

            // we'll need to repack
            if (finalTexture) RepackTextures();
            emptyArea -= texture.width * texture.height;

            return newID;
        }

        public bool SheetCanAccomodate(int width, int height)
        {
            return emptyArea >= width * height;
        }

        public bool UpdateTexture(int textureID, Texture2D texture)
        {
            // we'll need to see if the new texture size and old texture size
            if ((texture.width == regions[textureID].width) &&
                    (texture.height == regions[textureID].height))
            {
                // if the sizes are the same, graphics.drawtexture to the specified region
                RenderBuffer activeColorBuffer = Graphics.activeColorBuffer;
                RenderBuffer activeDepthBuffer = Graphics.activeDepthBuffer;
                Graphics.SetRenderTarget(textureSheet);
                Graphics.DrawTexture(regions[textureID], texture);
                Graphics.SetRenderTarget(activeColorBuffer, activeColorBuffer);
            }
            else
            {
                // if the sizes are different, repack and copy
                textures[textureID] = texture;
                RepackTextures();
            }

            return true;
        }

        public void DeleteTexture(int textureID)
        {
            int index = -1;

            for (int i = 0; i < indexes.Count; i++)
            {
                if (indexes[i] == textureID)
                {
                    index = i;
                }
            }

            if(index >= 0)
            {
                indexes.RemoveAt(index);
                textures.RemoveAt(index);
                RepackTextures();
            }
            else
            {
                Debug.LogError("Delete Texture failed - texture index not found.");
                return;
            }
        }

        public void ManualRepack()
        {
            RepackTextures();
        }
    }
}