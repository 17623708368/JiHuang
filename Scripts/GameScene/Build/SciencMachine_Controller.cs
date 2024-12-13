using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SciencMachine_Controller : BuildingBase
{
 public override void Init(MapChunkController mapChunk, ulong id, bool isFormBuilding)
 {
  base.Init(mapChunk, id, isFormBuilding);
  if (isFormBuilding)
  {
   ScienceMachineManager.Instance.AddScienceMachine(30);
  }
  ScienceMachineManager.Instance.AddScienceObjectDic(30,this);
 }

 public override void Onpreview()
 {
  
 }
}
