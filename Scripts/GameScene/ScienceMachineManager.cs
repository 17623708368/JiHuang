    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JKFrame;
    using UnityEngine;

    public class ScienceMachineManager:SingletonMono<ScienceMachineManager>
    {
        private ScienceMachineData scienceMachineData;
        private Dictionary<int, List<BuildingBase>> scienceObjectContorllerDic   = new Dictionary<int, List<BuildingBase>>();
        public void Init()
        {
            scienceMachineData = ArchiveManager.Instance.ScienceMachineData;
            EventManager.AddEventListener(EventName.SaveGame,OnSaveGame);
        }

        public bool CheckScience(int ID)
        {
            return scienceMachineData.CheckScience(ID);
        }

        public void AddScienceObjectDic(int ID, SciencMachine_Controller sciencMachineController)
        {
            if (!scienceObjectContorllerDic.ContainsKey(ID))
            {
                scienceObjectContorllerDic.Add(ID,new List<BuildingBase>(5)
                {
                    sciencMachineController
                });
            }
            else
            {
                scienceObjectContorllerDic[ID].Add(sciencMachineController);
            }
        }

        public List<BuildingBase> GetScienceObjectList(int ID)
        {
            if (scienceObjectContorllerDic.ContainsKey(ID))
            {
                return scienceObjectContorllerDic[ID];
            }

            return null;
        }
        public void AddScienceMachine(int ID)
        {
            scienceMachineData.AddScienceMachine(ID);
        }
        private void OnSaveGame()
        {
             ArchiveManager.Instance.SaveScienceMachineData();
        }
    }
