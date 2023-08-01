using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets
{

    public static class Options
    {

        public static int ChunkLoaderRadius { get; set; }
		public static bool Lod { get; set; }
		public static bool Invert { get; set;}


        static Options()
        {
            ChunkLoaderRadius = 9;
			Lod = true;
            Invert = PlayerPrefs.GetInt("Invertir") == 0 ? true : false;
            PlayerPrefs.SetInt("Invertir", Invert ? 0 : 1);
            PlayerPrefs.Save();
        }

    }
}
