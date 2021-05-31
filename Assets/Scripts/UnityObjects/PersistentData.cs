using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Assets.Scripts
{
    public class PersistentData : MonoBehaviour
    {
        #region IngamgeVariables
        private string _androidGameId;
        public string AndroidGameId
        {
            get
            {
                if(_androidGameId == null)
                {
                    LoadSettingsJson();
                }
                return _androidGameId;
            }
            set
            {
                _androidGameId = value;
            }
        }
        #endregion

        #region PlayerPrefs
        #endregion

        public static PersistentData Instance { get; set; }
        void Awake()
        {
            //If we don't currently have a game control...
            if (Instance == null)
            {
                //...set this one to be it...
                Instance = this;
            }
            //...otherwise...
            else if (Instance != this)
            {
                //...destroy this one because it is a duplicate.
                Destroy(gameObject);
            }
            DontDestroyOnLoad(this);

            // Load the soundval from last config file.
        }

        // Use this for initialization
        void Start()
        {
            LoadSettingsJson();
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void LoadSettingsJson()
        {
            using (StreamReader r = new StreamReader("secrets.json"))
            {
                string json = r.ReadToEnd();
                Secret secrets = JsonUtility.FromJson<Secret>(json);
                AndroidGameId = secrets.AndroidGameId;
            }
        }

        public class Secret
        {
            public string AndroidGameId;
        }
    }
}
