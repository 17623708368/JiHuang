using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class ScienceMachineData 
{
  public List<int> scienceMachineList=new List<int>(10);

  public bool CheckScience(int configID)
  {
      return scienceMachineList.Contains(configID);
  }

  public void AddScienceMachine(int ID)
  {
      if (!scienceMachineList.Contains(ID))
      {
          scienceMachineList.Add(ID);
      }
  }

}
