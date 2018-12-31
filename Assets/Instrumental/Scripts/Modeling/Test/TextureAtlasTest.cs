using UnityEngine;
using System.Collections;

namespace Lucidigital.Modeling
{
    [RequireComponent(typeof(TextureAtlas))]
    public class TextureAtlasTest : MonoBehaviour
    {
        private TextureAtlas textureAtlas;
        [SerializeField] MeshRenderer worldPreview;
        [SerializeField] MeshRenderer stagingPreview;

        [Header("Debug Variables")]
        [SerializeField] Texture2D texture;
        [SerializeField] Texture2D[] multipleTextures;
        [SerializeField] int textureID;

        [Header("Debug Commands")]
        [SerializeField] bool AddTexture;
        [SerializeField] bool AddMultipleTextures;
        [SerializeField] bool UpdateTexture;
        [SerializeField] bool DeleteTexture;
        [SerializeField] bool ManualRepack;

        // Use this for initialization
        void Awake()
        {
            textureAtlas = GetComponent<TextureAtlas>();
        }

        void Start()
        {
            if(worldPreview != null)
            {
                worldPreview.material.SetTexture("_MainTex", textureAtlas.TextureSheet);
            }

            if(stagingPreview != null)
            {
                stagingPreview.material.SetTexture("_MainTex", textureAtlas.StagingTexture);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if(AddTexture)
            {
                textureAtlas.AddTexture(texture, true);
                AddTexture = false;
            }

            if(AddMultipleTextures)
            {
                for(int i=0; i < multipleTextures.Length; i++)
                {
                    textureAtlas.AddTexture(multipleTextures[i],
                        i == multipleTextures.Length - 1);
                }

                AddMultipleTextures = false;
            }                   

            if(UpdateTexture)
            {
                textureAtlas.UpdateTexture(textureID, texture);
                UpdateTexture = false;
            }

            if(DeleteTexture)
            {
                textureAtlas.DeleteTexture(textureID);
                DeleteTexture = false;
            }

            if(ManualRepack)
            {
                textureAtlas.ManualRepack();
                ManualRepack = false;
            }
        }
    }
}