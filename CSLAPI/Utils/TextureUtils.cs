using System.IO;

using UnityEngine;

namespace CSLAPI.Utils
{
	public class TextureUtils
	{
		public static Texture2D LoadTexture(string texturePath)
		{
			var tex = new Texture2D(1, 1);
			// Loads the texture, sets the size, compression type, pixel format, etc.
			// Allow File.ReadAllBytes to throw FileNotFound
			tex.LoadImage(File.ReadAllBytes(texturePath));
			return tex;
		}

		public static void ReplaceNetTextures(string textureDir)
		{
			var collections = Object.FindObjectsOfType<NetCollection>();
			foreach (var nc in collections)
			{
				foreach (var prefab in nc.m_prefabs)
				{
					foreach (var node in prefab.m_nodes)
					{
						node.m_material.mainTexture = LoadTexture(Path.Combine(textureDir, prefab.name.Replace(" ", "_").ToLowerInvariant().Trim() + "_node.png"));
					}

					foreach (var segment in prefab.m_segments)
					{
						segment.m_material.mainTexture = LoadTexture(Path.Combine(textureDir, prefab.name.Replace(" ", "_").ToLowerInvariant().Trim() + "_segment.png"));
					}
				}
			}

			// Replace currently loaded ones
			
		}

		public static void DumpNetTextures(string textureDir)
		{
			var collections = Object.FindObjectsOfType<NetCollection>();
			foreach (var nc in collections)
			{
				foreach (var prefab in nc.m_prefabs)
				{
					foreach (var node in prefab.m_nodes)
					{
						File.WriteAllBytes(Path.Combine(textureDir, prefab.name.Replace(" ", "_").ToLowerInvariant().Trim() + "_node.png"), ((Texture2D)node.m_material.mainTexture).EncodeToPNG());
					}

					foreach (var segment in prefab.m_segments)
					{
						File.WriteAllBytes(Path.Combine(textureDir, prefab.name.Replace(" ", "_").ToLowerInvariant().Trim() + "_segment.png"), ((Texture2D)segment.m_material.mainTexture).EncodeToPNG());
					}
				}
			}
		}
	}
}